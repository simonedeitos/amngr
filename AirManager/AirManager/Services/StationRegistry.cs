using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Win32;

namespace AirManager.Services
{
    /// <summary>
    /// Gestisce il salvataggio/caricamento delle emittenti nel Registry
    /// Ogni emittente è salvata in una sottochiave di HKEY_CURRENT_USER\SOFTWARE\AirManager\Stations\{GUID}
    /// </summary>
    public static class StationRegistry
    {
        private const string REGISTRY_BASE = @"SOFTWARE\AirManager";
        private const string REGISTRY_STATIONS = @"SOFTWARE\AirManager\Stations";
        private const string REGISTRY_ACTIVE_STATION = "ActiveStationId";

        private static string _currentDatabasePath = @"C:\AirDirector\Database"; // ✅ DEFAULT FALLBACK

        /// <summary>
        /// Salva una nuova emittente o aggiorna esistente
        /// </summary>
        public static bool SaveStation(StationConfig station)
        {
            try
            {
                string stationKeyPath = $"{REGISTRY_STATIONS}\\{station.Id}";

                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(stationKeyPath))
                {
                    if (key == null)
                    {
                        Console.WriteLine($"[StationRegistry] ❌ Impossibile creare chiave: {stationKeyPath}");
                        return false;
                    }

                    key.SetValue("Id", station.Id);
                    key.SetValue("Name", station.Name ?? "");
                    key.SetValue("LogoPath", station.LogoPath ?? "");
                    key.SetValue("DatabasePath", station.DatabasePath ?? "");
                    key.SetValue("StationType", station.StationType.ToString());
                    key.SetValue("CreatedDate", station.CreatedDate.ToString("o")); // ISO 8601
                    key.SetValue("LastAccessed", station.LastAccessed.ToString("o"));

                    Console.WriteLine($"[StationRegistry] ✅ Salvata emittente: {station.Name} (ID: {station.Id})");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[StationRegistry] ❌ Errore salvataggio: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Carica tutte le emittenti configurate
        /// </summary>
        public static List<StationConfig> LoadAllStations()
        {
            var stations = new List<StationConfig>();

            try
            {
                using (RegistryKey baseKey = Registry.CurrentUser.OpenSubKey(REGISTRY_STATIONS))
                {
                    if (baseKey == null)
                    {
                        Console.WriteLine("[StationRegistry] ⚠️ Nessuna emittente configurata");
                        return stations;
                    }

                    string[] stationIds = baseKey.GetSubKeyNames();

                    foreach (string stationId in stationIds)
                    {
                        using (RegistryKey stationKey = baseKey.OpenSubKey(stationId))
                        {
                            if (stationKey != null)
                            {
                                var station = new StationConfig
                                {
                                    Id = stationKey.GetValue("Id", stationId).ToString(),
                                    Name = stationKey.GetValue("Name", "Emittente Senza Nome").ToString(),
                                    LogoPath = stationKey.GetValue("LogoPath", "").ToString(),
                                    DatabasePath = stationKey.GetValue("DatabasePath", "").ToString()
                                };

                                // ✅ PARSE STATION TYPE
                                if (Enum.TryParse<StationType>(stationKey.GetValue("StationType", "Radio")?.ToString(), out StationType stationType))
                                    station.StationType = stationType;

                                // ✅ PARSE DATE
                                if (DateTime.TryParse(stationKey.GetValue("CreatedDate", "")?.ToString(), out DateTime created))
                                    station.CreatedDate = created;

                                if (DateTime.TryParse(stationKey.GetValue("LastAccessed", "")?.ToString(), out DateTime accessed))
                                    station.LastAccessed = accessed;

                                stations.Add(station);
                            }
                        }
                    }
                }

                Console.WriteLine($"[StationRegistry] ✅ Caricate {stations.Count} emittenti");
                return stations.OrderBy(s => s.Name).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[StationRegistry] ❌ Errore caricamento:  {ex.Message}");
                return stations;
            }
        }

        /// <summary>
        /// Carica una singola emittente per ID
        /// </summary>
        public static StationConfig LoadStation(string stationId)
        {
            try
            {
                string stationKeyPath = $"{REGISTRY_STATIONS}\\{stationId}";

                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(stationKeyPath))
                {
                    if (key == null)
                        return null;

                    var station = new StationConfig
                    {
                        Id = key.GetValue("Id", stationId).ToString(),
                        Name = key.GetValue("Name", "").ToString(),
                        LogoPath = key.GetValue("LogoPath", "").ToString(),
                        DatabasePath = key.GetValue("DatabasePath", "").ToString()
                    };

                    // ✅ PARSE STATION TYPE
                    if (Enum.TryParse<StationType>(key.GetValue("StationType", "Radio")?.ToString(), out StationType stationType))
                        station.StationType = stationType;

                    if (DateTime.TryParse(key.GetValue("CreatedDate", "")?.ToString(), out DateTime created))
                        station.CreatedDate = created;

                    if (DateTime.TryParse(key.GetValue("LastAccessed", "")?.ToString(), out DateTime accessed))
                        station.LastAccessed = accessed;

                    return station;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[StationRegistry] ❌ Errore caricamento ID {stationId}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Elimina un'emittente
        /// </summary>
        public static bool DeleteStation(string stationId)
        {
            try
            {
                string stationKeyPath = $"{REGISTRY_STATIONS}\\{stationId}";

                using (RegistryKey baseKey = Registry.CurrentUser.OpenSubKey(REGISTRY_STATIONS, true))
                {
                    if (baseKey != null)
                    {
                        baseKey.DeleteSubKeyTree(stationId, false);
                        Console.WriteLine($"[StationRegistry] 🗑️ Eliminata emittente ID: {stationId}");
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[StationRegistry] ❌ Errore eliminazione:  {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Aggiorna LastAccessed di un'emittente
        /// </summary>
        public static void UpdateLastAccessed(string stationId)
        {
            try
            {
                string stationKeyPath = $"{REGISTRY_STATIONS}\\{stationId}";

                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(stationKeyPath, true))
                {
                    if (key != null)
                    {
                        key.SetValue("LastAccessed", DateTime.Now.ToString("o"));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[StationRegistry] ❌ Errore aggiornamento LastAccessed: {ex.Message}");
            }
        }

        /// <summary>
        /// Imposta l'emittente attiva corrente (salva ID nel registry)
        /// </summary>
        public static void SetActiveStation(StationConfig station)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(REGISTRY_BASE))
                {
                    if (key != null)
                    {
                        key.SetValue(REGISTRY_ACTIVE_STATION, station.Id);
                        _currentDatabasePath = station.DatabasePath;

                        UpdateLastAccessed(station.Id);

                        Console.WriteLine($"[StationRegistry] ✅ Emittente attiva: {station.Name}");
                        Console.WriteLine($"[StationRegistry]    DatabasePath: {station.DatabasePath}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[StationRegistry] ❌ Errore SetActiveStation: {ex.Message}");
            }
        }

        /// <summary>
        /// Ottiene l'ID dell'emittente attiva
        /// </summary>
        public static string GetActiveStationId()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(REGISTRY_BASE))
                {
                    return key?.GetValue(REGISTRY_ACTIVE_STATION, "")?.ToString() ?? "";
                }
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// Ottiene l'emittente attiva
        /// </summary>
        public static StationConfig GetActiveStation()
        {
            string activeId = GetActiveStationId();
            if (string.IsNullOrEmpty(activeId))
                return null;

            return LoadStation(activeId);
        }

        /// <summary>
        /// Ottiene il Database Path dell'emittente corrente
        /// ✅ USATO DA DbcManager
        /// </summary>
        public static string GetCurrentDatabasePath()
        {
            return _currentDatabasePath;
        }

        /// <summary>
        /// Esporta emittenti in JSON
        /// </summary>
        public static string ExportStations(List<string> stationIds)
        {
            try
            {
                var stationsToExport = new List<StationConfig>();

                foreach (string id in stationIds)
                {
                    var station = LoadStation(id);
                    if (station != null)
                    {
                        stationsToExport.Add(station);
                    }
                }

                string json = Newtonsoft.Json.JsonConvert.SerializeObject(stationsToExport, Newtonsoft.Json.Formatting.Indented);
                Console.WriteLine($"[StationRegistry] ✅ Esportate {stationsToExport.Count} emittenti");
                return json;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[StationRegistry] ❌ Errore export: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Importa emittenti da JSON
        /// </summary>
        public static int ImportStations(string json, bool overwriteExisting)
        {
            try
            {
                var importedStations = Newtonsoft.Json.JsonConvert.DeserializeObject<List<StationConfig>>(json);
                if (importedStations == null || importedStations.Count == 0)
                    return 0;

                int imported = 0;
                var existingStations = LoadAllStations();

                foreach (var station in importedStations)
                {
                    // ✅ VERIFICA SE ESISTE GIÀ
                    var existing = existingStations.FirstOrDefault(s => s.Id == station.Id);

                    if (existing != null && !overwriteExisting)
                    {
                        Console.WriteLine($"[StationRegistry] ⚠️ Emittente '{station.Name}' già esistente, saltata");
                        continue;
                    }

                    // ✅ GENERA NUOVO ID SE NECESSARIO
                    if (existing != null && overwriteExisting)
                    {
                        station.Id = Guid.NewGuid().ToString();
                    }

                    if (SaveStation(station))
                    {
                        imported++;
                    }
                }

                Console.WriteLine($"[StationRegistry] ✅ Importate {imported} emittenti");
                return imported;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[StationRegistry] ❌ Errore import: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Verifica se il database path è accessibile
        /// </summary>
        public static bool ValidateDatabasePath(string path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path))
                    return false;

                // ✅ VERIFICA SE È UN PERCORSO DI RETE
                if (path.StartsWith("\\\\"))
                {
                    return Directory.Exists(path);
                }

                // ✅ VERIFICA SE È UN PERCORSO LOCALE
                return Directory.Exists(path);
            }
            catch
            {
                return false;
            }
        }
    }
}