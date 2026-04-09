using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AirManager.Services;
using AirManager.Services.Database;
using AirManager.Themes;

namespace AirManager.Forms
{
    public partial class ReportExportDialog : Form
    {
        private List<ReportEntry> _data;

        private Label lblTitle;
        private GroupBox grpColumns;
        private GroupBox grpOptions;
        private GroupBox grpExportMode;
        private Label lblDelimiter;

        private CheckBox chkDate;
        private CheckBox chkStartTime;
        private CheckBox chkEndTime;
        private CheckBox chkArtist;
        private CheckBox chkTitle;
        private CheckBox chkType;
        private CheckBox chkPlayDuration;
        private CheckBox chkFileDuration;

        private RadioButton rbStandardReport;
        private RadioButton rbAdvSpots;
        private RadioButton rbAdvHourly;

        private ComboBox cmbDelimiter;
        private CheckBox chkIncludeHeader;
        private Button btnSelectAll;
        private Button btnDeselectAll;
        private Button btnExport;
        private Button btnCancel;

        // ADV filter: titles starting with these prefixes are excluded
        private static readonly string[] _advExcludedPrefixes =
            { "OPENER", "INFRASPOT", "CLOSER" };

        public ReportExportDialog(List<ReportEntry> data)
        {
            InitializeComponent();
            _data = data;
            InitializeCustomUI();
            ApplyLanguage(); // ✅ APPLICA LINGUA INIZIALE

            // ✅ SOTTOSCRIVI EVENTO CAMBIO LINGUA
            LanguageManager.LanguageChanged += (s, e) => ApplyLanguage();
        }

        /// <summary>
        /// ✅ APPLICA LE TRADUZIONI DA LanguageManager
        /// </summary>
        private void ApplyLanguage()
        {
            this.Text = "💾 " + LanguageManager.GetString("ReportExport.Title");

            lblTitle.Text = string.Format(
                LanguageManager.GetString("ReportExport.HeaderTitle"),
                _data.Count);

            // ✅ GRUPPI
            grpColumns.Text = LanguageManager.GetString("ReportExport.Group.Columns");
            grpOptions.Text = LanguageManager.GetString("ReportExport.Group.Options");
            grpExportMode.Text = LanguageManager.GetString("ReportExport.Group.ExportMode");

            // ✅ MODALITÀ EXPORT
            rbStandardReport.Text = LanguageManager.GetString("ReportExport.ExportMode.Standard");
            rbAdvSpots.Text = LanguageManager.GetString("ReportExport.ExportMode.AdvSpots");
            rbAdvHourly.Text = LanguageManager.GetString("ReportExport.ExportMode.AdvHourly");

            // ✅ CHECKBOXES COLONNE
            chkDate.Text = "📅 " + LanguageManager.GetString("ReportExport.Column.Date");
            chkStartTime.Text = "🕐 " + LanguageManager.GetString("ReportExport.Column.StartTime");
            chkEndTime.Text = "🕐 " + LanguageManager.GetString("ReportExport.Column.EndTime");
            chkArtist.Text = "🎤 " + LanguageManager.GetString("ReportExport.Column.Artist");
            chkTitle.Text = "🎶 " + LanguageManager.GetString("ReportExport.Column.Title");
            chkType.Text = "🎵 " + LanguageManager.GetString("ReportExport.Column.Type");
            chkPlayDuration.Text = "⏱️ " + LanguageManager.GetString("ReportExport.Column.PlayDuration");
            chkFileDuration.Text = "📏 " + LanguageManager.GetString("ReportExport.Column.FileDuration");

            // ✅ BOTTONI
            btnSelectAll.Text = "✓ " + LanguageManager.GetString("ReportExport.SelectAll");
            btnDeselectAll.Text = "✖ " + LanguageManager.GetString("ReportExport.DeselectAll");

            // ✅ OPZIONI
            lblDelimiter.Text = LanguageManager.GetString("ReportExport.Delimiter");
            chkIncludeHeader.Text = LanguageManager.GetString("ReportExport.IncludeHeader");

            // ✅ DROPDOWN SEPARATORI (mantieni selezione, default ; )
            int currentIndex = cmbDelimiter.SelectedIndex;
            cmbDelimiter.Items.Clear();
            cmbDelimiter.Items.Add(LanguageManager.GetString("ReportExport.Delimiter.Comma"));
            cmbDelimiter.Items.Add(LanguageManager.GetString("ReportExport.Delimiter.Semicolon"));
            cmbDelimiter.Items.Add(LanguageManager.GetString("ReportExport.Delimiter.Tab"));
            cmbDelimiter.SelectedIndex = currentIndex >= 0 ? currentIndex : 1; // default: semicolon

            // ✅ BOTTONI AZIONE
            btnExport.Text = "💾 " + LanguageManager.GetString("ReportExport.Export");
            btnCancel.Text = "✖ " + LanguageManager.GetString("Common.Cancel");

            Console.WriteLine($"[ReportExport] ✅ Lingua applicata: {LanguageManager.CurrentLanguage}");
        }

        private void InitializeCustomUI()
        {
            this.Text = "💾 Esporta Report CSV";
            this.Size = new Size(600, 620);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = AppTheme.BgLight;

            // ✅ HEADER
            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = AppTheme.BgDark
            };

            lblTitle = new Label
            {
                Text = $"💾 ESPORTA REPORT CSV ({_data.Count} elementi)",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 18),
                AutoSize = true
            };
            headerPanel.Controls.Add(lblTitle);

            this.Controls.Add(headerPanel);

            // ✅ COLONNE DA ESPORTARE
            grpColumns = new GroupBox
            {
                Text = "Colonne da Esportare",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 80),
                Size = new Size(540, 220)
            };

            chkDate = CreateCheckBox("📅 Data", 20, 30, true);
            chkStartTime = CreateCheckBox("🕐 Ora Inizio", 20, 60, true);
            chkEndTime = CreateCheckBox("🕐 Ora Fine", 20, 90, true);
            chkArtist = CreateCheckBox("🎤 Artista", 20, 120, true);
            chkTitle = CreateCheckBox("🎶 Titolo", 20, 150, true);
            chkType = CreateCheckBox("🎵 Tipo", 20, 180, true);

            chkPlayDuration = CreateCheckBox("⏱️ Durata Riproduzione", 300, 30, true);
            chkFileDuration = CreateCheckBox("📏 Durata File", 300, 60, true);

            grpColumns.Controls.Add(chkDate);
            grpColumns.Controls.Add(chkStartTime);
            grpColumns.Controls.Add(chkEndTime);
            grpColumns.Controls.Add(chkArtist);
            grpColumns.Controls.Add(chkTitle);
            grpColumns.Controls.Add(chkType);
            grpColumns.Controls.Add(chkPlayDuration);
            grpColumns.Controls.Add(chkFileDuration);

            btnSelectAll = new Button
            {
                Text = "✓ Seleziona Tutti",
                Location = new Point(300, 120),
                Size = new Size(130, 30),
                BackColor = Color.FromArgb(0, 150, 136),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSelectAll.FlatAppearance.BorderSize = 0;
            btnSelectAll.Click += (s, e) => SetAllCheckboxes(true);
            grpColumns.Controls.Add(btnSelectAll);

            btnDeselectAll = new Button
            {
                Text = "✖ Deseleziona Tutti",
                Location = new Point(300, 160),
                Size = new Size(130, 30),
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnDeselectAll.FlatAppearance.BorderSize = 0;
            btnDeselectAll.Click += (s, e) => SetAllCheckboxes(false);
            grpColumns.Controls.Add(btnDeselectAll);

            this.Controls.Add(grpColumns);

            // ✅ MODALITÀ EXPORT ADV
            grpExportMode = new GroupBox
            {
                Text = "Modalità Export",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 310),
                Size = new Size(540, 95)
            };

            rbStandardReport = new RadioButton
            {
                Text = "📋 Report Standard",
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = Color.White,
                Location = new Point(20, 22),
                Size = new Size(240, 22),
                Checked = true
            };
            rbStandardReport.CheckedChanged += (s, e) => UpdateControlsForMode();
            grpExportMode.Controls.Add(rbStandardReport);

            rbAdvSpots = new RadioButton
            {
                Text = "📢 Durate Singoli Spot ADV",
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = Color.White,
                Location = new Point(20, 46),
                Size = new Size(240, 22)
            };
            rbAdvSpots.CheckedChanged += (s, e) => UpdateControlsForMode();
            grpExportMode.Controls.Add(rbAdvSpots);

            rbAdvHourly = new RadioButton
            {
                Text = "🕐 Per Fasce Orarie ADV",
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = Color.White,
                Location = new Point(20, 68),
                Size = new Size(240, 22)
            };
            rbAdvHourly.CheckedChanged += (s, e) => UpdateControlsForMode();
            grpExportMode.Controls.Add(rbAdvHourly);

            this.Controls.Add(grpExportMode);

            // ✅ OPZIONI ESPORTAZIONE
            grpOptions = new GroupBox
            {
                Text = "Opzioni Esportazione",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 415),
                Size = new Size(540, 110)
            };

            lblDelimiter = new Label
            {
                Text = "Separatore:",
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = Color.White,
                Location = new Point(20, 35),
                Size = new Size(80, 25)
            };
            grpOptions.Controls.Add(lblDelimiter);

            cmbDelimiter = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9),
                Location = new Point(110, 33),
                Size = new Size(150, 25),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White
            };
            cmbDelimiter.Items.Add("Virgola (,)");
            cmbDelimiter.Items.Add("Punto e virgola (;)");
            cmbDelimiter.Items.Add("Tabulazione (Tab)");
            cmbDelimiter.SelectedIndex = 1; // default: semicolon
            grpOptions.Controls.Add(cmbDelimiter);

            chkIncludeHeader = new CheckBox
            {
                Text = "Includi intestazione (prima riga)",
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = Color.White,
                Location = new Point(20, 70),
                Size = new Size(300, 25),
                Checked = true
            };
            grpOptions.Controls.Add(chkIncludeHeader);

            this.Controls.Add(grpOptions);

            // ✅ BOTTONI
            btnCancel = new Button
            {
                Text = "✖ Annulla",
                Location = new Point(330, 540),
                Size = new Size(110, 40),
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.Cancel
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            this.Controls.Add(btnCancel);

            btnExport = new Button
            {
                Text = "💾 Esporta",
                Location = new Point(450, 540),
                Size = new Size(110, 40),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnExport.FlatAppearance.BorderSize = 0;
            btnExport.Click += BtnExport_Click;
            this.Controls.Add(btnExport);

            this.CancelButton = btnCancel;
        }

        /// <summary>
        /// Updates the enabled state of column checkboxes based on the selected export mode.
        /// </summary>
        private void UpdateControlsForMode()
        {
            bool columnsEnabled = !rbAdvHourly.Checked;
            grpColumns.Enabled = columnsEnabled;
        }

        private CheckBox CreateCheckBox(string text, int x, int y, bool isChecked)
        {
            return new CheckBox
            {
                Text = text,
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = Color.White,
                Location = new Point(x, y),
                Size = new Size(250, 25),
                Checked = isChecked
            };
        }

        private void SetAllCheckboxes(bool isChecked)
        {
            chkDate.Checked = isChecked;
            chkStartTime.Checked = isChecked;
            chkEndTime.Checked = isChecked;
            chkArtist.Checked = isChecked;
            chkTitle.Checked = isChecked;
            chkType.Checked = isChecked;
            chkPlayDuration.Checked = isChecked;
            chkFileDuration.Checked = isChecked;
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            // For standard and ADV spots modes, validate that at least one column is selected
            if (!rbAdvHourly.Checked)
            {
                if (!chkDate.Checked && !chkStartTime.Checked && !chkEndTime.Checked &&
                    !chkArtist.Checked && !chkTitle.Checked && !chkType.Checked &&
                    !chkPlayDuration.Checked && !chkFileDuration.Checked)
                {
                    MessageBox.Show(
                        LanguageManager.GetString("ReportExport.Validation.SelectColumn"),
                        LanguageManager.GetString("ReportExport.Title.NoColumn"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "File CSV (*.csv)|*.csv";

                if (rbAdvSpots.Checked)
                    sfd.FileName = $"ADV_Spots_{DateTime.Now:yyyy-MM-dd_HH-mm}.csv";
                else if (rbAdvHourly.Checked)
                    sfd.FileName = $"ADV_Hourly_{DateTime.Now:yyyy-MM-dd_HH-mm}.csv";
                else
                    sfd.FileName = $"Report_{DateTime.Now:yyyy-MM-dd_HH-mm}.csv";

                sfd.Title = LanguageManager.GetString("ReportExport.Dialog.SaveTitle");

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        if (rbAdvSpots.Checked)
                            ExportAdvSpotsToCSV(sfd.FileName);
                        else if (rbAdvHourly.Checked)
                            ExportAdvHourlyToCSV(sfd.FileName);
                        else
                            ExportToCSV(sfd.FileName);

                        MessageBox.Show(
                            string.Format(LanguageManager.GetString("ReportExport.Message.ExportSuccess"), _data.Count, sfd.FileName),
                            LanguageManager.GetString("ReportExport.Title.ExportComplete"),
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);

                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            string.Format(LanguageManager.GetString("ReportExport.Error.ExportFailed"), ex.Message),
                            LanguageManager.GetString("Common.Error"),
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ExportToCSV(string filePath)
        {
            string delimiter = GetDelimiter();

            using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                // ✅ HEADER (TRADOTTO)
                if (chkIncludeHeader.Checked)
                {
                    List<string> headers = new List<string>();

                    if (chkDate.Checked) headers.Add(LanguageManager.GetString("ReportExport.CSV.Date"));
                    if (chkStartTime.Checked) headers.Add(LanguageManager.GetString("ReportExport.CSV.StartTime"));
                    if (chkEndTime.Checked) headers.Add(LanguageManager.GetString("ReportExport.CSV.EndTime"));
                    if (chkArtist.Checked) headers.Add(LanguageManager.GetString("ReportExport.CSV.Artist"));
                    if (chkTitle.Checked) headers.Add(LanguageManager.GetString("ReportExport.CSV.Title"));
                    if (chkType.Checked) headers.Add(LanguageManager.GetString("ReportExport.CSV.Type"));
                    if (chkPlayDuration.Checked) headers.Add(LanguageManager.GetString("ReportExport.CSV.PlayDuration"));
                    if (chkFileDuration.Checked) headers.Add(LanguageManager.GetString("ReportExport.CSV.FileDuration"));

                    writer.WriteLine(string.Join(delimiter, headers));
                }

                // ✅ DATI
                foreach (var entry in _data)
                {
                    List<string> values = new List<string>();

                    if (chkDate.Checked) values.Add(EscapeCsv(entry.Date.ToString("dd/MM/yyyy"), delimiter));
                    if (chkStartTime.Checked) values.Add(EscapeCsv(entry.StartTime, delimiter));
                    if (chkEndTime.Checked) values.Add(EscapeCsv(entry.EndTime, delimiter));
                    if (chkArtist.Checked) values.Add(EscapeCsv(entry.Artist, delimiter));
                    if (chkTitle.Checked) values.Add(EscapeCsv(entry.Title, delimiter));
                    if (chkType.Checked) values.Add(EscapeCsv(entry.Type, delimiter));
                    if (chkPlayDuration.Checked) values.Add(EscapeCsv(entry.PlayDuration, delimiter));
                    if (chkFileDuration.Checked) values.Add(EscapeCsv(entry.FileDuration, delimiter));

                    writer.WriteLine(string.Join(delimiter, values));
                }
            }
        }

        /// <summary>
        /// Returns true if the entry is an ADV spot that should be included
        /// (Type == "ADV", title does not start with OPENER, INFRASPOT, or CLOSER).
        /// </summary>
        private static bool IsAdvSpotEntry(ReportEntry entry)
        {
            if (!string.Equals(entry.Type, "ADV", StringComparison.OrdinalIgnoreCase))
                return false;

            string title = entry.Title ?? "";
            foreach (string prefix in _advExcludedPrefixes)
            {
                if (title.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Parses a duration string "HH:MM:SS" (with possible surrounding spaces) to TimeSpan.
        /// Returns TimeSpan.Zero on failure.
        /// </summary>
        private static TimeSpan ParseDuration(string duration)
        {
            if (string.IsNullOrWhiteSpace(duration))
                return TimeSpan.Zero;

            var parts = duration.Split(':');
            if (parts.Length == 3 &&
                int.TryParse(parts[0].Trim(), out int h) &&
                int.TryParse(parts[1].Trim(), out int m) &&
                int.TryParse(parts[2].Trim(), out int s))
            {
                return new TimeSpan(h, m, s);
            }

            return TimeSpan.Zero;
        }

        /// <summary>
        /// Extracts the hour (0-23) from a StartTime string "HH:mm:ss" (may have spaces).
        /// </summary>
        private static int GetHourFromStartTime(string startTime)
        {
            if (string.IsNullOrWhiteSpace(startTime))
                return 0;

            var parts = startTime.Split(':');
            if (parts.Length >= 1 && int.TryParse(parts[0].Trim(), out int hour))
                return Math.Max(0, Math.Min(23, hour));

            return 0;
        }

        /// <summary>
        /// Exports ADV spots (excluding OPENER/INFRASPOT/CLOSER) with user-selected columns.
        /// </summary>
        private void ExportAdvSpotsToCSV(string filePath)
        {
            string delimiter = GetDelimiter();
            var advEntries = _data
                .Where(IsAdvSpotEntry)
                .OrderBy(e => e.Date)
                .ThenBy(e => e.StartTime)
                .ToList();

            if (advEntries.Count == 0)
            {
                MessageBox.Show(
                    LanguageManager.GetString("ReportExport.ADV.NoData"),
                    LanguageManager.GetString("ReportExport.ADV.NoDataTitle"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                if (chkIncludeHeader.Checked)
                {
                    List<string> headers = new List<string>();

                    if (chkDate.Checked) headers.Add(LanguageManager.GetString("ReportExport.CSV.Date"));
                    if (chkStartTime.Checked) headers.Add(LanguageManager.GetString("ReportExport.CSV.StartTime"));
                    if (chkEndTime.Checked) headers.Add(LanguageManager.GetString("ReportExport.CSV.EndTime"));
                    if (chkArtist.Checked) headers.Add(LanguageManager.GetString("ReportExport.CSV.Artist"));
                    if (chkTitle.Checked) headers.Add(LanguageManager.GetString("ReportExport.CSV.Title"));
                    if (chkType.Checked) headers.Add(LanguageManager.GetString("ReportExport.CSV.Type"));
                    if (chkPlayDuration.Checked) headers.Add(LanguageManager.GetString("ReportExport.CSV.PlayDuration"));
                    if (chkFileDuration.Checked) headers.Add(LanguageManager.GetString("ReportExport.CSV.FileDuration"));

                    writer.WriteLine(string.Join(delimiter, headers));
                }

                foreach (var entry in advEntries)
                {
                    List<string> values = new List<string>();

                    if (chkDate.Checked) values.Add(EscapeCsv(entry.Date.ToString("dd/MM/yyyy"), delimiter));
                    if (chkStartTime.Checked) values.Add(EscapeCsv(entry.StartTime, delimiter));
                    if (chkEndTime.Checked) values.Add(EscapeCsv(entry.EndTime, delimiter));
                    if (chkArtist.Checked) values.Add(EscapeCsv(entry.Artist, delimiter));
                    if (chkTitle.Checked) values.Add(EscapeCsv(entry.Title, delimiter));
                    if (chkType.Checked) values.Add(EscapeCsv(entry.Type, delimiter));
                    if (chkPlayDuration.Checked) values.Add(EscapeCsv(entry.PlayDuration, delimiter));
                    if (chkFileDuration.Checked) values.Add(EscapeCsv(entry.FileDuration, delimiter));

                    writer.WriteLine(string.Join(delimiter, values));
                }
            }
        }

        /// <summary>
        /// Exports ADV hourly slot totals in vertical CSV format:
        /// Data;Fascia Oraria;Durata Totale
        /// </summary>
        private void ExportAdvHourlyToCSV(string filePath)
        {
            string delimiter = GetDelimiter();
            var advEntries = _data
                .Where(IsAdvSpotEntry)
                .ToList();

            if (advEntries.Count == 0)
            {
                MessageBox.Show(
                    LanguageManager.GetString("ReportExport.ADV.NoData"),
                    LanguageManager.GetString("ReportExport.ADV.NoDataTitle"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            // Group entries by date and hour
            var grouped = advEntries
                .GroupBy(e => e.Date.Date)
                .OrderBy(g => g.Key)
                .ToList();

            using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                // Fixed header
                if (chkIncludeHeader.Checked)
                {
                    writer.WriteLine(string.Join(delimiter,
                        LanguageManager.GetString("ReportExport.CSV.Date"),
                        LanguageManager.GetString("ReportExport.CSV.HourSlot"),
                        LanguageManager.GetString("ReportExport.CSV.TotalDuration")));
                }

                foreach (var dayGroup in grouped)
                {
                    string dateStr = dayGroup.Key.ToString("dd/MM/yyyy");

                    // Build hourly totals (0-23)
                    var hourlyTotals = new TimeSpan[24];
                    foreach (var entry in dayGroup)
                    {
                        int hour = GetHourFromStartTime(entry.StartTime);
                        hourlyTotals[hour] += ParseDuration(entry.PlayDuration);
                    }

                    for (int h = 0; h < 24; h++)
                    {
                        TimeSpan total = hourlyTotals[h];
                        string totalStr = string.Format("{0:D2}:{1:D2}:{2:D2}",
                            (int)total.TotalHours, total.Minutes, total.Seconds);

                        writer.WriteLine(string.Join(delimiter,
                            EscapeCsv(dateStr, delimiter),
                            EscapeCsv(h.ToString("D2"), delimiter),
                            EscapeCsv(totalStr, delimiter)));
                    }
                }
            }
        }

        private string GetDelimiter()
        {
            switch (cmbDelimiter.SelectedIndex)
            {
                case 1: return ";";
                case 2: return "\t";
                default: return ",";
            }
        }

        private string EscapeCsv(string value, string delimiter)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            if (value.Contains(delimiter) || value.Contains("\"") || value.Contains("\n"))
            {
                return $"\"{value.Replace("\"", "\"\"")}\"";
            }

            return value;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // ✅ DISOTTOSCRIVI EVENTO
                LanguageManager.LanguageChanged -= (s, e) => ApplyLanguage();
            }

            base.Dispose(disposing);
        }
    }
}