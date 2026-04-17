using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace AirManager.Services
{
    public class PathMappingEntry
    {
        public string SourcePath { get; set; } = string.Empty;
        public string TargetPath { get; set; } = string.Empty;
    }

    public static class PathMappingService
    {
        private const string RegistryPath = @"SOFTWARE\AirManager\Settings";
        private const string RegistryValue = "PathMappingsJson";

        public static List<PathMappingEntry> LoadMappings()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(RegistryPath))
                {
                    string json = key?.GetValue(RegistryValue, "[]")?.ToString() ?? "[]";
                    return JsonConvert.DeserializeObject<List<PathMappingEntry>>(json) ?? new List<PathMappingEntry>();
                }
            }
            catch
            {
                return new List<PathMappingEntry>();
            }
        }

        public static void SaveMappings(List<PathMappingEntry> mappings)
        {
            using (var key = Registry.CurrentUser.CreateSubKey(RegistryPath))
            {
                string json = JsonConvert.SerializeObject(mappings ?? new List<PathMappingEntry>());
                key?.SetValue(RegistryValue, json);
            }
        }

        public static string ResolvePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return path;

            var mappings = LoadMappings()
                .Where(m => !string.IsNullOrWhiteSpace(m.SourcePath) && !string.IsNullOrWhiteSpace(m.TargetPath))
                .OrderByDescending(m => m.SourcePath.Length)
                .ToList();

            foreach (var map in mappings)
            {
                if (path.StartsWith(map.SourcePath, StringComparison.OrdinalIgnoreCase))
                {
                    return map.TargetPath + path.Substring(map.SourcePath.Length);
                }
            }

            return path;
        }
    }
}
