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
    public class LicenseManager
    {
        // API Configuration
        private const string API_BASE = "https://store.airdirector.app/api/";
        private const string DEFAULT_API_KEY = "73a434a1107442481e13ed52ceba1a574648adb12fd5bc0e0c967f25f6743731";

        // Storage paths
        private static readonly string AppDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AirManager");
        private static readonly string LicenseFilePath = Path.Combine(AppDataPath, "AirManager.lic");

        // Registry key
        private const string REGISTRY_KEY = @"SOFTWARE\AirManager";

        // Encryption
        private const string EncPassphrase = "AirManager.Lic.2024#Secure";

        // Singleton
        private static LicenseManager? _instance;
        private static readonly object _lock = new object();

        // Current license
        public LicenseInfo? CurrentLicense { get; private set; }

        private LicenseManager() { }

        /// <summary>
        /// Gets the singleton instance of LicenseManager
        /// </summary>
        public static LicenseManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance ??= new LicenseManager();
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Checks if a valid license exists
        /// </summary>
        public bool IsLicensed()
        {
            try
            {
                if (CurrentLicense == null)
                    LoadLicense();

                return CurrentLicense != null && CurrentLicense.IsValid();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LicenseManager] Error checking license: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Loads the license from file
        /// </summary>
        public void LoadLicense()
        {
            try
            {
                if (File.Exists(LicenseFilePath))
                {
                    string encryptedData = File.ReadAllText(LicenseFilePath);
                    string jsonData = Decrypt(encryptedData);
                    CurrentLicense = JsonConvert.DeserializeObject<LicenseInfo>(jsonData);
                    Console.WriteLine("[LicenseManager] License loaded successfully");
                }
                else
                {
                    // Try loading from registry as fallback
                    LoadLicenseFromRegistry();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LicenseManager] Error loading license: {ex.Message}");
                CurrentLicense = null;
            }
        }

        /// <summary>
        /// Saves the license to file
        /// </summary>
        private void SaveLicense()
        {
            try
            {
                if (CurrentLicense == null)
                    return;

                // Ensure directory exists
                if (!Directory.Exists(AppDataPath))
                    Directory.CreateDirectory(AppDataPath);

                string jsonData = JsonConvert.SerializeObject(CurrentLicense, Formatting.Indented);
                string encryptedData = Encrypt(jsonData);
                File.WriteAllText(LicenseFilePath, encryptedData);

                // Also save to registry as backup
                SaveLicenseToRegistry();

                Console.WriteLine("[LicenseManager] License saved successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LicenseManager] Error saving license: {ex.Message}");
            }
        }

        /// <summary>
        /// Saves license data to Windows Registry
        /// </summary>
        private void SaveLicenseToRegistry()
        {
            try
            {
                using (RegistryKey? key = Registry.CurrentUser.CreateSubKey(REGISTRY_KEY))
                {
                    if (key != null && CurrentLicense != null)
                    {
                        string jsonData = JsonConvert.SerializeObject(CurrentLicense);
                        string encryptedData = Encrypt(jsonData);
                        key.SetValue("LicenseData", encryptedData);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LicenseManager] Error saving to registry: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads license data from Windows Registry
        /// </summary>
        private void LoadLicenseFromRegistry()
        {
            try
            {
                using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(REGISTRY_KEY))
                {
                    if (key != null)
                    {
                        string? encryptedData = key.GetValue("LicenseData") as string;
                        if (!string.IsNullOrEmpty(encryptedData))
                        {
                            string jsonData = Decrypt(encryptedData);
                            CurrentLicense = JsonConvert.DeserializeObject<LicenseInfo>(jsonData);
                            Console.WriteLine("[LicenseManager] License loaded from registry");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LicenseManager] Error loading from registry: {ex.Message}");
            }
        }

        /// <summary>
        /// Activates a license with the given serial code via API
        /// </summary>
        public async Task<(bool Success, string Message)> ActivateLicenseAsync(string serialCode)
        {
            try
            {
                serialCode = serialCode.Trim().ToUpper();

                if (!LicenseInfo.IsValidSerialFormat(serialCode))
                    return (false, "Invalid serial code format. Expected: AMG-XXXX-XXXX-XXXX");

                string hardwareId = HardwareIdentifier.GetHardwareId();

                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(30);

                    var requestData = new
                    {
                        serial_code = serialCode,
                        hardware_id = hardwareId,
                        product_name = LicenseInfo.ProductName,
                        api_key = GetApiKey()
                    };

                    string jsonContent = JsonConvert.SerializeObject(requestData);
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync($"{API_BASE}license/activate", content);
                    string responseBody = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        JObject result = JObject.Parse(responseBody);

                        bool success = result["success"]?.Value<bool>() ?? false;
                        string message = result["message"]?.Value<string>() ?? "Unknown response";

                        if (success)
                        {
                            JObject? licenseData = result["license"] as JObject;

                            CurrentLicense = new LicenseInfo
                            {
                                SerialCode = serialCode,
                                LicenseKey = licenseData?["license_key"]?.Value<string>() ?? string.Empty,
                                HardwareId = hardwareId,
                                CustomerName = licenseData?["customer_name"]?.Value<string>() ?? string.Empty,
                                CustomerEmail = licenseData?["customer_email"]?.Value<string>() ?? string.Empty,
                                ProductVersion = licenseData?["product_version"]?.Value<string>() ?? string.Empty,
                                ActivationDate = DateTime.UtcNow,
                                ExpirationDate = ParseExpirationDate(licenseData?["expiration_date"]?.Value<string>()),
                                LicenseType = licenseData?["license_type"]?.Value<string>() ?? "Standard",
                                IsActivated = true,
                                MaxActivations = licenseData?["max_activations"]?.Value<int>() ?? 1,
                                CurrentActivations = licenseData?["current_activations"]?.Value<int>() ?? 1,
                                Status = "Active"
                            };

                            SaveLicense();

                            return (true, message);
                        }
                        else
                        {
                            return (false, message);
                        }
                    }
                    else
                    {
                        // Try to parse error message from response
                        try
                        {
                            JObject errorResult = JObject.Parse(responseBody);
                            string errorMessage = errorResult["message"]?.Value<string>() ?? $"Server error: {response.StatusCode}";
                            return (false, errorMessage);
                        }
                        catch
                        {
                            return (false, $"Server error: {response.StatusCode}");
                        }
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"[LicenseManager] Network error: {ex.Message}");
                return (false, "Network error. Please check your internet connection and try again.");
            }
            catch (TaskCanceledException)
            {
                return (false, "Connection timeout. Please check your internet connection and try again.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LicenseManager] Activation error: {ex.Message}");
                return (false, $"Activation error: {ex.Message}");
            }
        }

        /// <summary>
        /// Deactivates the current license
        /// </summary>
        public async Task<(bool Success, string Message)> DeactivateLicenseAsync()
        {
            try
            {
                if (CurrentLicense == null || !CurrentLicense.IsActivated)
                    return (false, "No active license found");

                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(30);

                    var requestData = new
                    {
                        serial_code = CurrentLicense.SerialCode,
                        hardware_id = CurrentLicense.HardwareId,
                        product_name = LicenseInfo.ProductName,
                        api_key = GetApiKey()
                    };

                    string jsonContent = JsonConvert.SerializeObject(requestData);
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync($"{API_BASE}license/deactivate", content);
                    string responseBody = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        JObject result = JObject.Parse(responseBody);
                        bool success = result["success"]?.Value<bool>() ?? false;
                        string message = result["message"]?.Value<string>() ?? "License deactivated";

                        if (success)
                        {
                            // Clear local license
                            CurrentLicense = null;
                            DeleteLicenseFiles();
                            return (true, message);
                        }

                        return (false, message);
                    }

                    return (false, $"Server error: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LicenseManager] Deactivation error: {ex.Message}");
                return (false, $"Deactivation error: {ex.Message}");
            }
        }

        /// <summary>
        /// Validates the current license against the API
        /// </summary>
        public async Task<(bool Valid, string Message)> ValidateLicenseAsync()
        {
            try
            {
                if (CurrentLicense == null)
                    return (false, "No license found");

                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(15);

                    var requestData = new
                    {
                        serial_code = CurrentLicense.SerialCode,
                        hardware_id = HardwareIdentifier.GetHardwareId(),
                        product_name = LicenseInfo.ProductName,
                        api_key = GetApiKey()
                    };

                    string jsonContent = JsonConvert.SerializeObject(requestData);
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync($"{API_BASE}license/validate", content);
                    string responseBody = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        JObject result = JObject.Parse(responseBody);
                        bool valid = result["valid"]?.Value<bool>() ?? false;
                        string message = result["message"]?.Value<string>() ?? "Validation complete";
                        return (valid, message);
                    }

                    // If server is unreachable, allow offline validation
                    return (CurrentLicense.IsValid(), "Offline validation (server unavailable)");
                }
            }
            catch (Exception)
            {
                // Allow offline validation when server is unavailable
                if (CurrentLicense != null)
                    return (CurrentLicense.IsValid(), "Offline validation (server unavailable)");

                return (false, "No license found");
            }
        }

        /// <summary>
        /// Deletes all license data
        /// </summary>
        private void DeleteLicenseFiles()
        {
            try
            {
                if (File.Exists(LicenseFilePath))
                    File.Delete(LicenseFilePath);

                using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(REGISTRY_KEY, true))
                {
                    key?.DeleteValue("LicenseData", false);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LicenseManager] Error deleting license files: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the API key from registry or uses default
        /// </summary>
        private string GetApiKey()
        {
            try
            {
                using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(REGISTRY_KEY))
                {
                    string? apiKey = key?.GetValue("ApiKey") as string;
                    if (!string.IsNullOrEmpty(apiKey))
                        return apiKey;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LicenseManager] Error reading API key from registry: {ex.Message}");
            }

            return DEFAULT_API_KEY;
        }

        /// <summary>
        /// Parses an expiration date string, returns DateTime.MinValue for perpetual licenses
        /// </summary>
        private DateTime ParseExpirationDate(string? dateStr)
        {
            if (string.IsNullOrEmpty(dateStr) || dateStr.Equals("never", StringComparison.OrdinalIgnoreCase))
                return DateTime.MinValue;

            if (DateTime.TryParse(dateStr, out DateTime result))
                return result;

            return DateTime.MinValue;
        }

        // ==================== ENCRYPTION ====================

        /// <summary>
        /// Encrypts a string using AES encryption
        /// </summary>
        private string Encrypt(string plainText)
        {
            byte[] key = DeriveKey(EncPassphrase);
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.GenerateIV();
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (ICryptoTransform encryptor = aes.CreateEncryptor())
                {
                    byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                    byte[] encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

                    // Prepend IV to encrypted data
                    byte[] result = new byte[aes.IV.Length + encryptedBytes.Length];
                    Array.Copy(aes.IV, 0, result, 0, aes.IV.Length);
                    Array.Copy(encryptedBytes, 0, result, aes.IV.Length, encryptedBytes.Length);

                    return Convert.ToBase64String(result);
                }
            }
        }

        /// <summary>
        /// Decrypts an AES encrypted string
        /// </summary>
        private string Decrypt(string encryptedText)
        {
            byte[] key = DeriveKey(EncPassphrase);
            byte[] fullData = Convert.FromBase64String(encryptedText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                // Extract IV from the beginning of the data
                byte[] iv = new byte[aes.BlockSize / 8];
                byte[] encryptedBytes = new byte[fullData.Length - iv.Length];

                Array.Copy(fullData, 0, iv, 0, iv.Length);
                Array.Copy(fullData, iv.Length, encryptedBytes, 0, encryptedBytes.Length);

                aes.IV = iv;

                using (ICryptoTransform decryptor = aes.CreateDecryptor())
                {
                    byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                    return Encoding.UTF8.GetString(decryptedBytes);
                }
            }
        }

        /// <summary>
        /// Derives a 256-bit key from a passphrase using SHA256
        /// </summary>
        private byte[] DeriveKey(string passphrase)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(Encoding.UTF8.GetBytes(passphrase));
            }
        }
    }
}
