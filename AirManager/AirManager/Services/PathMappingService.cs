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
        private static readonly object Sync = new object();
        private static List<PathMappingEntry> _cachedMappings;

        public static List<PathMappingEntry> LoadMappings()
        {
            lock (Sync)
            {
                if (_cachedMappings != null)
                    return _cachedMappings.Select(m => new PathMappingEntry { SourcePath = m.SourcePath, TargetPath = m.TargetPath }).ToList();
            }

            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(RegistryPath))
                {
                    string json = key?.GetValue(RegistryValue, "[]")?.ToString() ?? "[]";
                    var mappings = JsonConvert.DeserializeObject<List<PathMappingEntry>>(json) ?? new List<PathMappingEntry>();
                    lock (Sync)
                    {
                        _cachedMappings = mappings;
                    }
                    return mappings.Select(m => new PathMappingEntry { SourcePath = m.SourcePath, TargetPath = m.TargetPath }).ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PathMappingService] ❌ Errore caricamento mappature: {ex.Message}");
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

            lock (Sync)
            {
                _cachedMappings = (mappings ?? new List<PathMappingEntry>())
                    .Select(m => new PathMappingEntry { SourcePath = m.SourcePath, TargetPath = m.TargetPath })
                    .ToList();
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
