using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AirManager.Services.Database
{
    /// <summary>
    /// Gestisce la scrittura e lettura dei report (Report.dbc)
    /// Utilizza il database path dell'emittente corrente
    /// </summary>
    public static class ReportManager
    {
        private static readonly object _lock = new object();

        /// <summary>
        /// Scrive una riga nel Report.dbc dell'emittente corrente
        /// </summary>
        /// <param name="type">Music o Clip</param>
        /// <param name="artist">Artista (per musica) o titolo (per clip)</param>
        /// <param name="title">Titolo brano</param>
        /// <param name="startTime">Data/ora inizio riproduzione</param>
        /// <param name="endTime">Data/ora fine riproduzione</param>
        /// <param name="fileDuration">Durata originale del file</param>
        public static void LogTrack(string type, string artist, string title, DateTime startTime, DateTime endTime, TimeSpan fileDuration)
        {
            lock (_lock)
            {
                try
                {
                    string dbPath = DbcManager.GetDatabasePath();
                    string reportPath = Path.Combine(dbPath, "Report.dbc");

                    // ✅ CREA IL FILE SE NON ESISTE (con header)
                    if (!File.Exists(reportPath))
                    {
                        string header = "Date;StartTime;EndTime;Type;Artist;Title;PlayDuration;FileDuration";
                        File.WriteAllText(reportPath, header + Environment.NewLine, Encoding.UTF8);
                        Console.WriteLine($"[ReportManager] ✅ Report.dbc creato: {reportPath}");
                    }

                    // ✅ CALCOLA PlayDuration (differenza tra start e end)
                    TimeSpan playDuration = endTime - startTime;

                    // ✅ PREPARA LA RIGA
                    string date = startTime.ToString("yyyy-MM-dd");
                    string start = startTime.ToString("HH:mm: ss");
                    string end = endTime.ToString("HH: mm:ss");
                    string playDur = FormatDuration(playDuration);
                    string fileDur = FormatDuration(fileDuration);

                    // ✅ ESCAPE CARATTERI SPECIALI (punto e virgola, virgolette)
                    artist = EscapeCsvField(artist ?? "");
                    title = EscapeCsvField(title ?? "");
                    type = EscapeCsvField(type ?? "Music");

                    string line = $"{date};{start};{end};{type};{artist};{title};{playDur};{fileDur}";

                    // ✅ SCRIVI IN APPEND
                    File.AppendAllText(reportPath, line + Environment.NewLine, Encoding.UTF8);

                    Console.WriteLine($"[ReportManager] ✅ Log:  {date} {start}-{end} | {artist} - {title} | Play:{playDur} File:{fileDur}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ReportManager] ❌ Errore scrittura: {ex.Message}");
                    Console.WriteLine($"[ReportManager]    StackTrace: {ex.StackTrace}");
                }
            }
        }

        /// <summary>
        /// Carica report filtrato per intervallo date
        /// </summary>
        /// <param name="from">Data inizio (inclusa)</param>
        /// <param name="to">Data fine (inclusa)</param>
        /// <returns>Lista di ReportEntry</returns>
        public static List<ReportEntry> LoadReport(DateTime from, DateTime to)
        {
            lock (_lock)
            {
                try
                {
                    string dbPath = DbcManager.GetDatabasePath();
                    string reportPath = Path.Combine(dbPath, "Report.dbc");

                    if (!File.Exists(reportPath))
                    {
                        Console.WriteLine($"[ReportManager] ⚠️ Report.dbc non trovato: {reportPath}");
                        return new List<ReportEntry>();
                    }

                    var lines = File.ReadAllLines(reportPath, Encoding.UTF8);
                    var entries = new List<ReportEntry>();

                    // ✅ SKIP HEADER (riga 0)
                    for (int i = 1; i < lines.Length; i++)
                    {
                        string line = lines[i].Trim();

                        if (string.IsNullOrWhiteSpace(line))
                            continue;

                        var parts = SplitCsvLine(line);

                        if (parts.Length < 8)
                        {
                            Console.WriteLine($"[ReportManager] ⚠️ Riga malformata (riga {i + 1}): {line}");
                            continue;
                        }

                        try
                        {
                            // ✅ PARSE DATA
                            if (!DateTime.TryParse(parts[0], out DateTime date))
                            {
                                Console.WriteLine($"[ReportManager] ⚠️ Data non valida (riga {i + 1}): {parts[0]}");
                                continue;
                            }

                            // ✅ FILTRA PER INTERVALLO DATE
                            if (date.Date >= from.Date && date.Date <= to.Date)
                            {
                                var entry = new ReportEntry
                                {
                                    Date = date,
                                    StartTime = parts[1],
                                    EndTime = parts[2],
                                    Type = UnescapeCsvField(parts[3]),
                                    Artist = UnescapeCsvField(parts[4]),
                                    Title = UnescapeCsvField(parts[5]),
                                    PlayDuration = parts[6],
                                    FileDuration = parts[7]
                                };

                                entries.Add(entry);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[ReportManager] ⚠️ Errore parsing riga {i + 1}: {ex.Message}");
                        }
                    }

                    Console.WriteLine($"[ReportManager] ✅ Caricati {entries.Count} report da {from: dd/MM/yyyy} a {to:dd/MM/yyyy}");
                    return entries;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ReportManager] ❌ Errore lettura report: {ex.Message}");
                    return new List<ReportEntry>();
                }
            }
        }

        /// <summary>
        /// Carica tutti i report (senza filtro date)
        /// ⚠️ ATTENZIONE: Può essere lento con molti dati
        /// </summary>
        public static List<ReportEntry> LoadAllReports()
        {
            return LoadReport(DateTime.MinValue, DateTime.MaxValue);
        }

        /// <summary>
        /// Carica report di un singolo giorno
        /// </summary>
        public static List<ReportEntry> LoadReportByDay(DateTime day)
        {
            return LoadReport(day.Date, day.Date);
        }

        /// <summary>
        /// Carica report degli ultimi N giorni
        /// </summary>
        public static List<ReportEntry> LoadReportLastDays(int days)
        {
            DateTime to = DateTime.Now;
            DateTime from = to.AddDays(-days);
            return LoadReport(from, to);
        }

        /// <summary>
        /// Ottiene statistiche report per un periodo
        /// </summary>
        public static ReportStatistics GetStatistics(DateTime from, DateTime to)
        {
            var reports = LoadReport(from, to);

            var stats = new ReportStatistics
            {
                TotalTracks = reports.Count,
                TotalMusic = reports.Count(r => r.Type == "Music"),
                TotalClips = reports.Count(r => r.Type == "Clip"),
                PeriodStart = from,
                PeriodEnd = to
            };

            // ✅ ARTISTI PIÙ RIPRODOTTI
            stats.TopArtists = reports
                .Where(r => r.Type == "Music" && !string.IsNullOrWhiteSpace(r.Artist))
                .GroupBy(r => r.Artist)
                .OrderByDescending(g => g.Count())
                .Take(10)
                .Select(g => new KeyValuePair<string, int>(g.Key, g.Count()))
                .ToList();

            // ✅ BRANI PIÙ RIPRODOTTI
            stats.TopTracks = reports
                .Where(r => r.Type == "Music")
                .GroupBy(r => $"{r.Artist} - {r.Title}")
                .OrderByDescending(g => g.Count())
                .Take(10)
                .Select(g => new KeyValuePair<string, int>(g.Key, g.Count()))
                .ToList();

            // ✅ CLIPS PIÙ RIPRODOTTE
            stats.TopClips = reports
                .Where(r => r.Type == "Clip")
                .GroupBy(r => r.Title)
                .OrderByDescending(g => g.Count())
                .Take(10)
                .Select(g => new KeyValuePair<string, int>(g.Key, g.Count()))
                .ToList();

            return stats;
        }

        /// <summary>
        /// Pulisce report più vecchi di N giorni
        /// </summary>
        public static int CleanOldReports(int daysToKeep)
        {
            lock (_lock)
            {
                try
                {
                    string dbPath = DbcManager.GetDatabasePath();
                    string reportPath = Path.Combine(dbPath, "Report.dbc");

                    if (!File.Exists(reportPath))
                        return 0;

                    var lines = File.ReadAllLines(reportPath, Encoding.UTF8);
                    var newLines = new List<string>();

                    // ✅ MANTIENI HEADER
                    if (lines.Length > 0)
                        newLines.Add(lines[0]);

                    DateTime cutoffDate = DateTime.Now.AddDays(-daysToKeep).Date;
                    int removedCount = 0;

                    for (int i = 1; i < lines.Length; i++)
                    {
                        var parts = SplitCsvLine(lines[i]);

                        if (parts.Length > 0 && DateTime.TryParse(parts[0], out DateTime date))
                        {
                            if (date.Date >= cutoffDate)
                            {
                                newLines.Add(lines[i]);
                            }
                            else
                            {
                                removedCount++;
                            }
                        }
                    }

                    // ✅ BACKUP PRIMA DI SOVRASCRIVERE
                    string backupPath = reportPath + ".bak";
                    File.Copy(reportPath, backupPath, true);

                    // ✅ SCRIVI NUOVO FILE
                    File.WriteAllLines(reportPath, newLines, Encoding.UTF8);

                    Console.WriteLine($"[ReportManager] 🗑️ Rimossi {removedCount} report più vecchi di {daysToKeep} giorni");
                    return removedCount;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ReportManager] ❌ Errore pulizia: {ex.Message}");
                    return 0;
                }
            }
        }

        /// <summary>
        /// Esporta report in CSV (usa ReportExportDialog per opzioni avanzate)
        /// </summary>
        public static bool ExportToCsv(List<ReportEntry> reports, string filePath, string delimiter = ",", bool includeHeader = true)
        {
            try
            {
                using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
                {
                    if (includeHeader)
                    {
                        writer.WriteLine($"Data{delimiter}OraInizio{delimiter}OraFine{delimiter}Tipo{delimiter}Artista{delimiter}Titolo{delimiter}DurataPlay{delimiter}DurataFile");
                    }

                    foreach (var report in reports)
                    {
                        string line = string.Join(delimiter,
                            EscapeCsvField(report.Date.ToString("dd/MM/yyyy"), delimiter),
                            EscapeCsvField(report.StartTime, delimiter),
                            EscapeCsvField(report.EndTime, delimiter),
                            EscapeCsvField(report.Type, delimiter),
                            EscapeCsvField(report.Artist, delimiter),
                            EscapeCsvField(report.Title, delimiter),
                            EscapeCsvField(report.PlayDuration, delimiter),
                            EscapeCsvField(report.FileDuration, delimiter)
                        );
                        writer.WriteLine(line);
                    }
                }

                Console.WriteLine($"[ReportManager] ✅ Esportati {reports.Count} report in {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ReportManager] ❌ Errore export:  {ex.Message}");
                return false;
            }
        }

        // ✅ UTILITY METHODS

        private static string FormatDuration(TimeSpan duration)
        {
            return $"{(int)duration.TotalHours: D2}:{duration.Minutes:D2}:{duration.Seconds:D2}";
        }

        private static string EscapeCsvField(string field, string delimiter = ";")
        {
            if (string.IsNullOrEmpty(field))
                return "";

            if (field.Contains(delimiter) || field.Contains("\"") || field.Contains("\n"))
            {
                return $"\"{field.Replace("\"", "\"\"")}\"";
            }

            return field;
        }

        private static string UnescapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return "";

            if (field.StartsWith("\"") && field.EndsWith("\"") && field.Length >= 2)
            {
                return field.Substring(1, field.Length - 2).Replace("\"\"", "\"");
            }

            return field;
        }

        private static string[] SplitCsvLine(string line)
        {
            var result = new List<string>();
            var currentField = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        currentField.Append('"');
                        i++; // Skip next quote
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ';' && !inQuotes)
                {
                    result.Add(currentField.ToString());
                    currentField.Clear();
                }
                else
                {
                    currentField.Append(c);
                }
            }

            result.Add(currentField.ToString());
            return result.ToArray();
        }
    }

    /// <summary>
    /// Entry singola del report
    /// </summary>
    public class ReportEntry
    {
        public DateTime Date { get; set; }
        public string StartTime { get; set; } = "";
        public string EndTime { get; set; } = "";
        public string Type { get; set; } = ""; // "Music" o "Clip"
        public string Artist { get; set; } = "";
        public string Title { get; set; } = "";
        public string PlayDuration { get; set; } = ""; // Durata effettiva riproduzione (HH:MM:SS)
        public string FileDuration { get; set; } = ""; // Durata originale file (HH:MM: SS)

        public override string ToString()
        {
            return $"{Date: dd/MM/yyyy} {StartTime} - {Artist} - {Title}";
        }
    }

    /// <summary>
    /// Statistiche report
    /// </summary>
    public class ReportStatistics
    {
        public int TotalTracks { get; set; }
        public int TotalMusic { get; set; }
        public int TotalClips { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }

        public List<KeyValuePair<string, int>> TopArtists { get; set; } = new List<KeyValuePair<string, int>>();
        public List<KeyValuePair<string, int>> TopTracks { get; set; } = new List<KeyValuePair<string, int>>();
        public List<KeyValuePair<string, int>> TopClips { get; set; } = new List<KeyValuePair<string, int>>();

        public override string ToString()
        {
            return $"Report {PeriodStart:dd/MM/yyyy} - {PeriodEnd:dd/MM/yyyy}:  {TotalTracks} tracks ({TotalMusic} music, {TotalClips} clips)";
        }
    }
}