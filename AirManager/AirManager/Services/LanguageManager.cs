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
        private string _currentLanguage;
        private string _languagesPath;

        public static event EventHandler LanguageChanged;

        private LanguageManager()
        {
            _translations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _languagesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Languages");

            // ✅ LEGGI LINGUA SALVATA DAL REGISTRY
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
        /// ✅ LEGGE LA LINGUA SALVATA DAL REGISTRY
        /// </summary>
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
                // Silenzioso
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
                        string italianPath = Path.Combine(_languagesPath, "English.ini");
                        if (File.Exists(italianPath))
                        {
                            filePath = italianPath;
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
                // Silenzioso
            }
        }

        public static string GetString(string key, string defaultValue = null)
        {
            if (Instance._translations.TryGetValue(key, out string value))
            {
                return value;
            }

            return defaultValue ?? key;
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