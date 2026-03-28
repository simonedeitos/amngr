using System;
using System.Management;
using System.Security.Cryptography;
using System.Text;

namespace AirManager.Services.Licensing
{
    /// <summary>
    /// Generates a unique hardware identifier for license binding
    /// </summary>
    public static class HardwareIdentifier
    {
        private static string? _cachedHardwareId;

        /// <summary>
        /// Gets a unique hardware ID based on CPU, motherboard, and disk identifiers
        /// </summary>
        public static string GetHardwareId()
        {
            if (!string.IsNullOrEmpty(_cachedHardwareId))
                return _cachedHardwareId;

            try
            {
                string cpuId = GetWmiProperty("Win32_Processor", "ProcessorId");
                string boardId = GetWmiProperty("Win32_BaseBoard", "SerialNumber");
                string diskId = GetWmiProperty("Win32_DiskDrive", "SerialNumber");
                string biosId = GetWmiProperty("Win32_BIOS", "SerialNumber");

                string combined = $"{cpuId}|{boardId}|{diskId}|{biosId}";

                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
                    _cachedHardwareId = BitConverter.ToString(hashBytes).Replace("-", "").Substring(0, 32);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[HardwareIdentifier] Error generating hardware ID: {ex.Message}");
                // Fallback: use machine name + environment
                string fallback = $"{Environment.MachineName}|{Environment.UserName}|{Environment.OSVersion}";
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(fallback));
                    _cachedHardwareId = BitConverter.ToString(hashBytes).Replace("-", "").Substring(0, 32);
                }
            }

            return _cachedHardwareId!;
        }

        /// <summary>
        /// Gets a WMI property value
        /// </summary>
        private static string GetWmiProperty(string wmiClass, string propertyName)
        {
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher($"SELECT {propertyName} FROM {wmiClass}"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        object? value = obj[propertyName];
                        if (value != null)
                        {
                            string result = value.ToString()?.Trim() ?? string.Empty;
                            if (!string.IsNullOrEmpty(result) &&
                                !result.Equals("To Be Filled By O.E.M.", StringComparison.OrdinalIgnoreCase) &&
                                !result.Equals("Default string", StringComparison.OrdinalIgnoreCase))
                            {
                                return result;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[HardwareIdentifier] WMI query failed for {wmiClass}.{propertyName}: {ex.Message}");
            }

            return string.Empty;
        }

        /// <summary>
        /// Resets the cached hardware ID (for testing purposes)
        /// </summary>
        public static void ResetCache()
        {
            _cachedHardwareId = null;
        }
    }
}
