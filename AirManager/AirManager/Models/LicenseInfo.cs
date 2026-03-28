using System;
using AirManager.Services.Licensing;

namespace AirManager.Models
{
    public class LicenseInfo
    {
        public const string SERIAL_PREFIX = "AMG-";
        public const string ProductName = "AirManager";

        // License properties
        public string SerialCode { get; set; } = string.Empty;
        public string LicenseKey { get; set; } = string.Empty;
        public string HardwareId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string ProductVersion { get; set; } = string.Empty;
        public DateTime ActivationDate { get; set; } = DateTime.MinValue;
        public DateTime ExpirationDate { get; set; } = DateTime.MinValue;
        public string LicenseType { get; set; } = "Standard";
        public bool IsActivated { get; set; } = false;
        public int MaxActivations { get; set; } = 1;
        public int CurrentActivations { get; set; } = 0;
        public string Status { get; set; } = "Inactive";

        /// <summary>
        /// Checks if the serial code has a valid format (AMG-XXXX-XXXX-XXXX)
        /// </summary>
        public static bool IsValidSerialFormat(string serial)
        {
            if (string.IsNullOrWhiteSpace(serial))
                return false;

            serial = serial.Trim().ToUpper();

            if (!serial.StartsWith(SERIAL_PREFIX))
                return false;

            // Format: AMG-XXXX-XXXX-XXXX (19 characters)
            if (serial.Length != 19)
                return false;

            string[] parts = serial.Split('-');
            if (parts.Length != 4)
                return false;

            // First part must be "AMG"
            if (parts[0] != "AMG")
                return false;

            // Each segment after prefix must be 4 alphanumeric characters
            for (int i = 1; i < parts.Length; i++)
            {
                if (parts[i].Length != 4)
                    return false;

                foreach (char c in parts[i])
                {
                    if (!char.IsLetterOrDigit(c))
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Checks if the license is valid and not expired
        /// </summary>
        public bool IsValid()
        {
            if (!IsActivated)
                return false;

            if (string.IsNullOrEmpty(SerialCode) || string.IsNullOrEmpty(LicenseKey))
                return false;

            // Check hardware ID matches current machine
            string currentHwId = HardwareIdentifier.GetHardwareId();
            if (!string.Equals(HardwareId, currentHwId, StringComparison.OrdinalIgnoreCase))
                return false;

            // Check expiration (DateTime.MinValue = never expires)
            if (ExpirationDate != DateTime.MinValue && DateTime.Now > ExpirationDate)
                return false;

            return true;
        }

        /// <summary>
        /// Returns license status description
        /// </summary>
        public string GetStatusDescription()
        {
            if (!IsActivated)
                return "Not Activated";

            if (ExpirationDate != DateTime.MinValue && DateTime.Now > ExpirationDate)
                return "Expired";

            string currentHwId = HardwareIdentifier.GetHardwareId();
            if (!string.Equals(HardwareId, currentHwId, StringComparison.OrdinalIgnoreCase))
                return "Hardware Mismatch";

            return "Active";
        }

        /// <summary>
        /// Returns remaining days until expiration, or -1 if perpetual
        /// </summary>
        public int GetRemainingDays()
        {
            if (ExpirationDate == DateTime.MinValue)
                return -1; // Perpetual license

            int days = (ExpirationDate - DateTime.Now).Days;
            return days < 0 ? 0 : days;
        }
    }
}
