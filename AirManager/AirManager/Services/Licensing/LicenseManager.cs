using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AirManager.Models;

namespace AirManager.Services.Licensing
{
    /// <summary>
    /// Manages license validation, activation, and persistence for AirManager
    /// </summary>
    public static class LicenseManager
    {
        // ── License file path ──────────────────────────────────────────────
        private static readonly string AppDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "AirManager"
        );

        private static readonly string LicenseFilePath = Path.Combine(AppDataPath, "AirManager.lic");

        // ── API ───────────────────────────────────────────────────────────────
        private const string API_BASE = "https://store.airdirector.app/api/";

        private static readonly string API_KEY = LoadApiKey();

        private static readonly HttpClient _http = CreateHttpClient();

        private const string REGISTRY_KEY = @"SOFTWARE\AirManager";
        private const string REGISTRY_VALUE_NAME = "ApiKey";

        private const string DEFAULT_API_KEY = "73a434a1107442481e13ed52ceba1a574648adb12fd5bc0e0c967f25f6743731";

        /// <summary>
        /// Loads the API key from the Windows Registry (HKCU\SOFTWARE\AirManager\ApiKey).
        /// If the value does not exist, creates it with the default key.
        /// </summary>
        private static string LoadApiKey()
        {
            try
            {
                using (RegistryKey? key = Registry.CurrentUser.CreateSubKey(REGISTRY_KEY))
                {
                    object? val = key?.GetValue(REGISTRY_VALUE_NAME);
                    if (val != null)
                    {
                        string apiKey = val.ToString() ?? string.Empty;
                        if (!string.IsNullOrWhiteSpace(apiKey))
                            return apiKey;
                    }

                    // Value not found → create the field with the default API key
                    key?.SetValue(REGISTRY_VALUE_NAME, DEFAULT_API_KEY, RegistryValueKind.String);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading ApiKey from Registry: {ex.Message}");
            }

            return DEFAULT_API_KEY;
        }

        private static HttpClient CreateHttpClient()
        {
            var client = new HttpClient();
            if (!string.IsNullOrEmpty(API_KEY))
                client.DefaultRequestHeaders.Add("X-API-Key", API_KEY);
            client.Timeout = TimeSpan.FromSeconds(15);
            return client;
        }

        // ── License file encryption (AES-256-CBC) ─────────────────────────────
        private const string EncPassphrase = "AirManager.Lic.2024#Secure";
        private const int PBKDF2_ITERATIONS = 100_000;
        private static readonly byte[] EncSalt =
        {
            0x4A, 0x69, 0x2E, 0xAC, 0x7B, 0xF3, 0x1D, 0x88,
            0x5C, 0x40, 0xE2, 0x9A, 0x3F, 0xC1, 0x55, 0x7E
        };

        private static (byte[] key, byte[] iv) DeriveKeyAndIV()
        {
            using var kdf = new Rfc2898DeriveBytes(
                EncPassphrase, EncSalt, PBKDF2_ITERATIONS, HashAlgorithmName.SHA256);
            return (kdf.GetBytes(32), kdf.GetBytes(16));
        }

        private static string EncryptLicense(string plainText)
        {
            var (key, iv) = DeriveKeyAndIV();
            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;

            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs, Encoding.UTF8))
            {
                sw.Write(plainText);
                sw.Flush();
            }

            return Convert.ToBase64String(ms.ToArray());
        }

        private static string DecryptLicense(string cipherText)
        {
            var (key, iv) = DeriveKeyAndIV();
            byte[] cipherBytes = Convert.FromBase64String(cipherText);

            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;

            using var ms = new MemoryStream(cipherBytes);
            using var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using var sr = new StreamReader(cs, Encoding.UTF8);
            return sr.ReadToEnd();
        }

        // ── Cache ─────────────────────────────────────────────────────────────
        private static LicenseInfo? _cachedLicense = null;

        // ── License reading ───────────────────────────────────────────────────

        /// <summary>
        /// Gets the current license (from cache or file).
        /// Returns null if no valid license is found.
        /// </summary>
        public static LicenseInfo? GetCurrentLicense()
        {
            if (_cachedLicense != null)
                return _cachedLicense;

            if (File.Exists(LicenseFilePath))
            {
                try
                {
                    string fileContent = File.ReadAllText(LicenseFilePath);

                    string json;
                    bool wasEncrypted = true;
                    try
                    {
                        json = DecryptLicense(fileContent);
                    }
                    catch (FormatException)
                    {
                        json = fileContent;
                        wasEncrypted = false;
                    }
                    catch (CryptographicException)
                    {
                        json = fileContent;
                        wasEncrypted = false;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[LicenseManager] Unexpected decryption error: {ex.Message}");
                        json = fileContent;
                        wasEncrypted = false;
                    }

                    var loaded = JsonConvert.DeserializeObject<LicenseInfo>(json);
                    if (loaded != null && loaded.IsValid())
                    {
                        // If the file was unencrypted, rewrite it encrypted
                        if (!wasEncrypted)
                            SaveLicenseToFile(loaded, out _);

                        _cachedLicense = loaded;
                        return loaded;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[LicenseManager] Error loading license: {ex.Message}");
                }
            }

            // No valid license found
            return null;
        }

        /// <summary>
        /// Checks if the license is activated and valid
        /// </summary>
        public static bool IsLicenseValid()
        {
            var license = GetCurrentLicense();
            return license != null && license.IsActivated;
        }

        // ── Activation ───────────────────────────────────────────────────────

        /// <summary>
        /// Verifies and activates the license through the server API.
        /// Returns true if activation was successful.
        /// </summary>
        public static bool ActivateLicense(string serialKey, string ownerName, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(serialKey))
            {
                errorMessage = "Please enter the serial code";
                return false;
            }

            serialKey = serialKey.ToUpper().Trim();

            if (!LicenseInfo.IsValidSerialFormat(serialKey))
            {
                errorMessage = $"Invalid serial format.\nCorrect format: {LicenseInfo.SERIAL_PREFIX}XXXX-XXXX-XXXX";
                return false;
            }

            string hwId = HardwareIdentifier.GetMachineID();

            try
            {
                // 1. Check license on server
                var checkResult = Task.Run(() => CheckLicenseOnServerAsync(serialKey))
                                      .GetAwaiter().GetResult();

                if (!checkResult.exists)
                {
                    errorMessage = "Invalid serial";
                    return false;
                }

                if (!checkResult.orderConfirmed)
                {
                    errorMessage = "Order awaiting confirmation";
                    return false;
                }

                if (checkResult.expired)
                {
                    errorMessage = "License expired";
                    return false;
                }

                if (checkResult.isActive && checkResult.hardwareId != hwId)
                {
                    errorMessage = "License already active on another device.\nPlease deactivate it first from your account.";
                    return false;
                }

                // 2. If not yet active on this PC → activate
                if (!checkResult.isActive)
                {
                    var activateResult = Task.Run(() => ActivateLicenseOnServerAsync(serialKey, hwId))
                                             .GetAwaiter().GetResult();
                    if (!activateResult)
                    {
                        errorMessage = "Error during server activation";
                        return false;
                    }
                }

                // 3. Create and save the local license
                var license = new LicenseInfo
                {
                    SerialKey   = serialKey,
                    OwnerName   = string.IsNullOrWhiteSpace(ownerName) ? serialKey : ownerName.Trim(),
                    ActivatedOn = DateTime.Now,
                    MachineID   = hwId,
                    ProductName = "AirManager",
                    Version     = "1.0.0",
                    IsActivated = true
                };

                if (!SaveLicenseToFile(license, out string saveError))
                {
                    errorMessage = saveError;
                    return false;
                }

                _cachedLicense = license;
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = $"Error connecting to the license server:\n{ex.Message}";
                return false;
            }
        }

        // ── Deactivation ────────────────────────────────────────────────────

        /// <summary>
        /// Removes the local license and deactivates it on the server.
        /// </summary>
        public static bool RemoveLicense(out string errorMessage)
        {
            errorMessage = string.Empty;

            var current = GetCurrentLicense();

            if (current == null)
            {
                errorMessage = "No active license to remove";
                return false;
            }

            try
            {
                bool serverOk = Task.Run(() => DeactivateLicenseOnServerAsync(current.SerialKey))
                                    .GetAwaiter().GetResult();

                if (!serverOk)
                {
                    errorMessage = "Error during server deactivation";
                    return false;
                }

                if (File.Exists(LicenseFilePath))
                    File.Delete(LicenseFilePath);

                _cachedLicense = null;
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = $"Error removing license: {ex.Message}";
                return false;
            }
        }

        // ── Periodic check ───────────────────────────────────────────────────

        /// <summary>
        /// Periodic license check on the server (call at startup or every 24 hours).
        /// If the license is deactivated, it removes it.
        /// </summary>
        public static bool PeriodicCheck(out string statusMessage)
        {
            statusMessage = string.Empty;

            var current = GetCurrentLicense();
            if (current == null)
                return false;

            try
            {
                var result = Task.Run(() => CheckLicenseOnServerAsync(current.SerialKey))
                                  .GetAwaiter().GetResult();

                if (!result.exists || !result.isActive)
                {
                    if (File.Exists(LicenseFilePath))
                        File.Delete(LicenseFilePath);
                    _cachedLicense = null;
                    statusMessage = "License deactivated. The software will close.";
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                // No connection → continue working locally
                Console.WriteLine($"[LicenseManager] PeriodicCheck: no connection — {ex.Message}");
                return true;
            }
        }

        // ── API calls ──────────────────────────────────────────────────────

        private static async Task<(bool exists, bool orderConfirmed, bool expired, bool isActive, string hardwareId)>
            CheckLicenseOnServerAsync(string serial)
        {
            var response = await _http.GetAsync($"{API_BASE}license_check.php?serial={Uri.EscapeDataString(serial)}");
            string json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Server error ({(int)response.StatusCode}): {json}");

            var obj = JObject.Parse(json);

            bool exists         = obj["exists"]?.Value<bool>() ?? false;
            bool orderOk        = obj["order_confirmed"]?.Value<bool>() ?? false;
            bool expired        = obj["expired"]?.Value<bool>() ?? false;
            bool isActive       = (obj["is_active"]?.Value<int>() ?? 0) == 1;
            string hwId         = obj["hardware_id"]?.Value<string>() ?? string.Empty;

            return (exists, orderOk, expired, isActive, hwId);
        }

        private static async Task<bool> ActivateLicenseOnServerAsync(string serial, string hardwareId)
        {
            var body = JsonConvert.SerializeObject(new { serial, hardware_id = hardwareId });
            var content = new StringContent(body, Encoding.UTF8, "application/json");

            var response = await _http.PostAsync($"{API_BASE}license_activate.php", content);

            if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                return true;

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Activation error ({(int)response.StatusCode}): {await response.Content.ReadAsStringAsync()}");

            string json = await response.Content.ReadAsStringAsync();
            var obj = JObject.Parse(json);

            bool success       = obj["success"]?.Value<bool>() ?? false;
            bool alreadyActive = obj["already_active"]?.Value<bool>() ?? false;

            return success || alreadyActive;
        }

        private static async Task<bool> DeactivateLicenseOnServerAsync(string serial)
        {
            var body = JsonConvert.SerializeObject(new { serial });
            var content = new StringContent(body, Encoding.UTF8, "application/json");

            var response = await _http.PostAsync($"{API_BASE}license_deactivate.php", content);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Deactivation error ({(int)response.StatusCode}): {await response.Content.ReadAsStringAsync()}");

            string json = await response.Content.ReadAsStringAsync();
            var obj = JObject.Parse(json);

            bool success = obj["success"]?.Value<bool>() ?? false;
            return success;
        }

        // ── Utilities ──────────────────────────────────────────────────────

        private static bool SaveLicenseToFile(LicenseInfo license, out string errorMessage)
        {
            errorMessage = string.Empty;
            try
            {
                if (!Directory.Exists(AppDataPath))
                    Directory.CreateDirectory(AppDataPath);

                string json = JsonConvert.SerializeObject(license, Formatting.Indented);
                File.WriteAllText(LicenseFilePath, EncryptLicense(json));
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = $"Error saving license: {ex.Message}";
                return false;
            }
        }

        /// <summary>License file path</summary>
        public static string GetLicenseFilePath() => LicenseFilePath;

        /// <summary>Force reload the license from file</summary>
        public static void ReloadLicense()
        {
            _cachedLicense = null;
            GetCurrentLicense();
        }
    }
}
