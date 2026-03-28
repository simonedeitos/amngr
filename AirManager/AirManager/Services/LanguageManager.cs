using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Win32;

namespace AirManager.Services
{
    public class LanguageManager
    {
        private static LanguageManager _instance;
        private static readonly object _lock = new object();

        private Dictionary<string, string> _translations;
        private readonly Dictionary<string, string> _missingKeys;
        private string _currentLanguage;
        private string _languagesPath;

        public static event EventHandler LanguageChanged;

        private LanguageManager()
        {
            _translations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _missingKeys = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _languagesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Languages");

            string savedLanguage = GetSavedLanguage();
            _currentLanguage = !string.IsNullOrEmpty(savedLanguage) ? savedLanguage : "Italiano";

            LoadLanguage(_currentLanguage);
        }

        public static LanguageManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new LanguageManager();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Initializes the LanguageManager singleton instance.
        /// Call this at application startup to ensure the language is loaded early.
        /// </summary>
        public static void Initialize()
        {
            _ = Instance;
        }

        private string GetSavedLanguage()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\AirManager\Settings"))
                {
                    if (key != null)
                    {
                        string language = key.GetValue("Language") as string;
                        if (!string.IsNullOrEmpty(language))
                        {
                            string filePath = Path.Combine(_languagesPath, $"{language}.ini");
                            if (File.Exists(filePath))
                            {
                                return language;
                            }
                        }
                    }
                }
            }
            catch
            {
                // Silent
            }

            return null;
        }

        public void LoadLanguage(string languageName)
        {
            try
            {
                string filePath = Path.Combine(_languagesPath, $"{languageName}.ini");

                if (!File.Exists(filePath))
                {
                    if (languageName != "English")
                    {
                        string englishPath = Path.Combine(_languagesPath, "English.ini");
                        if (File.Exists(englishPath))
                        {
                            filePath = englishPath;
                            languageName = "English";
                        }
                        else
                        {
                            return;
                        }
                    }
                    else
                    {
                        return;
                    }
                }

                _translations.Clear();
                _missingKeys.Clear();
                _currentLanguage = languageName;

                string currentSection = "";

                foreach (string line in File.ReadAllLines(filePath))
                {
                    string trimmedLine = line.Trim();

                    if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith(";") || trimmedLine.StartsWith("#"))
                        continue;

                    if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                    {
                        currentSection = trimmedLine.Substring(1, trimmedLine.Length - 2).Trim();
                        continue;
                    }

                    int equalsIndex = trimmedLine.IndexOf('=');
                    if (equalsIndex > 0)
                    {
                        string key = trimmedLine.Substring(0, equalsIndex).Trim();
                        string value = trimmedLine.Substring(equalsIndex + 1).Trim();

                        string fullKey = string.IsNullOrEmpty(currentSection) ? key : $"{currentSection}.{key}";

                        _translations[fullKey] = value;
                    }
                }
            }
            catch
            {
                // Silent
            }
        }

        public static string GetString(string key, string defaultValue = null)
        {
            if (Instance._translations.TryGetValue(key, out string value))
            {
                return value.Replace("\\n", Environment.NewLine);
            }

            // Track missing keys for diagnostics
            if (!Instance._missingKeys.ContainsKey(key))
            {
                Instance._missingKeys[key] = defaultValue ?? key;
            }

            return defaultValue ?? key;
        }

        /// <summary>
        /// Saves all missing translation keys to the current language file.
        /// Keys are appended under a [MissingKeys] section at the end of the file.
        /// </summary>
        public static void SaveMissingKeysToFile()
        {
            try
            {
                if (Instance._missingKeys.Count == 0)
                    return;

                string filePath = Path.Combine(Instance._languagesPath, $"{Instance._currentLanguage}.ini");

                if (!File.Exists(filePath))
                    return;

                var lines = new List<string>
                {
                    "",
                    "; ============================================",
                    "; MISSING KEYS (auto-generated)",
                    "; ============================================"
                };

                // Group missing keys by section prefix
                var grouped = Instance._missingKeys
                    .OrderBy(k => k.Key)
                    .GroupBy(k =>
                    {
                        int dotIndex = k.Key.LastIndexOf('.');
                        return dotIndex > 0 ? k.Key.Substring(0, dotIndex) : "General";
                    });

                foreach (var group in grouped)
                {
                    lines.Add("");
                    lines.Add($"[{group.Key}]");
                    foreach (var kvp in group)
                    {
                        string shortKey = kvp.Key;
                        int dotIndex = kvp.Key.LastIndexOf('.');
                        if (dotIndex > 0)
                            shortKey = kvp.Key.Substring(dotIndex + 1);

                        lines.Add($"{shortKey}={kvp.Value}");
                    }
                }

                File.AppendAllLines(filePath, lines);
            }
            catch
            {
                // Silent
            }
        }

        /// <summary>
        /// Returns the dictionary of missing translation keys found at runtime.
        /// </summary>
        public static IReadOnlyDictionary<string, string> GetMissingKeys()
        {
            return Instance._missingKeys;
        }

        public static void SetLanguage(string languageName)
        {
            Instance.LoadLanguage(languageName);
            LanguageChanged?.Invoke(null, EventArgs.Empty);
        }

        public static string CurrentLanguage => Instance._currentLanguage;

        public static List<string> GetAvailableLanguages()
        {
            try
            {
                string languagesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Languages");

                if (!Directory.Exists(languagesPath))
                    return new List<string> { "English" };

                return Directory.GetFiles(languagesPath, "*.ini")
                    .Select(f => Path.GetFileNameWithoutExtension(f))
                    .OrderBy(name => name)
                    .ToList();
            }
            catch
            {
                return new List<string> { "English" };
            }
        }
    }
}