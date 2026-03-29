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
    public class TrackHistoryForm : Form
    {
        private DataGridView _dgv;
        private string _artist;
        private string _title;

        public TrackHistoryForm(string artist, string title)
        {
            _artist = artist ?? "";
            _title = title ?? "";

            this.Text = $"📋 {LanguageManager.GetString("TrackHistory.Title", "Storico Passaggi")} - {_artist} - {_title}";
            this.Size = new Size(750, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimumSize = new Size(500, 300);
            this.BackColor = AppTheme.BgDark;
            this.ForeColor = Color.White;

            InitializeUI();
            LoadHistory();

            LanguageManager.LanguageChanged += OnLanguageChanged;
        }

        private void OnLanguageChanged(object? sender, EventArgs e)
        {
            ApplyLanguage();
        }

        private void ApplyLanguage()
        {
            this.Text = $"📋 {LanguageManager.GetString("TrackHistory.Title", "Storico Passaggi")} - {_artist} - {_title}";

            if (_dgv.Columns.Count >= 4)
            {
                _dgv.Columns["colDate"].HeaderText = LanguageManager.GetString("TrackHistory.Column.Date", "Data");
                _dgv.Columns["colTime"].HeaderText = LanguageManager.GetString("TrackHistory.Column.Time", "Ora");
                _dgv.Columns["colDuration"].HeaderText = LanguageManager.GetString("TrackHistory.Column.Duration", "Durata");
                _dgv.Columns["colType"].HeaderText = LanguageManager.GetString("TrackHistory.Column.Type", "Tipo");
            }
        }

        private void InitializeUI()
        {
            // Header panel
            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 55,
                BackColor = AppTheme.BgPanel
            };

            Label lblHeader = new Label
            {
                Text = $"📋 {_artist} - {_title}",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(15, 8),
                AutoSize = true
            };
            headerPanel.Controls.Add(lblHeader);

            Label lblSubtitle = new Label
            {
                Text = LanguageManager.GetString("TrackHistory.Subtitle", "Storico passaggi in onda"),
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                ForeColor = AppTheme.TextSecondary,
                Location = new Point(15, 32),
                AutoSize = true
            };
            headerPanel.Controls.Add(lblSubtitle);

            // Button panel
            Panel buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 45,
                BackColor = AppTheme.BgPanel
            };

            Button btnExport = new Button
            {
                Text = "💾 " + LanguageManager.GetString("TrackHistory.Export", "Esporta CSV"),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Size = new Size(130, 30),
                Location = new Point(15, 8),
                BackColor = AppTheme.ButtonPrimary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnExport.FlatAppearance.BorderSize = 0;
            btnExport.Click += BtnExport_Click;
            buttonPanel.Controls.Add(btnExport);

            Button btnClose = new Button
            {
                Text = LanguageManager.GetString("TrackHistory.Close", "Chiudi"),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Size = new Size(90, 30),
                Location = new Point(155, 8),
                BackColor = AppTheme.ButtonSecondary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => this.Close();
            buttonPanel.Controls.Add(btnClose);

            // DataGridView
            _dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = AppTheme.BgDark,
                GridColor = AppTheme.GridBorder,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                Font = new Font("Segoe UI", 9)
            };

            _dgv.EnableHeadersVisualStyles = false;
            _dgv.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = AppTheme.GridHeader,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleLeft
            };

            _dgv.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = AppTheme.GridRowEven,
                ForeColor = Color.White,
                SelectionBackColor = AppTheme.GridSelection,
                SelectionForeColor = Color.White
            };

            _dgv.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = AppTheme.GridRowOdd,
                ForeColor = Color.White,
                SelectionBackColor = AppTheme.GridSelection,
                SelectionForeColor = Color.White
            };

            // Columns
            _dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colDate",
                HeaderText = LanguageManager.GetString("TrackHistory.Column.Date", "Data"),
                FillWeight = 25
            });
            _dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colTime",
                HeaderText = LanguageManager.GetString("TrackHistory.Column.Time", "Ora"),
                FillWeight = 20
            });
            _dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colDuration",
                HeaderText = LanguageManager.GetString("TrackHistory.Column.Duration", "Durata"),
                FillWeight = 20
            });
            _dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colType",
                HeaderText = LanguageManager.GetString("TrackHistory.Column.Type", "Tipo"),
                FillWeight = 15
            });

            // Add controls in correct order (bottom-up for docking)
            this.Controls.Add(_dgv);
            this.Controls.Add(buttonPanel);
            this.Controls.Add(headerPanel);
        }

        private void LoadHistory()
        {
            try
            {
                _dgv.Rows.Clear();

                // Load all reports and filter by artist+title
                var allReports = ReportManager.LoadReport(DateTime.MinValue, DateTime.MaxValue);

                var trackHistory = allReports
                    .Where(r => string.Equals(r.Artist, _artist, StringComparison.OrdinalIgnoreCase)
                             && string.Equals(r.Title, _title, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(r => r.Date)
                    .ThenByDescending(r => r.StartTime)
                    .ToList();

                foreach (var entry in trackHistory)
                {
                    _dgv.Rows.Add(
                        entry.Date.ToString("dd/MM/yyyy"),
                        entry.StartTime,
                        entry.PlayDuration,
                        entry.Type
                    );
                }

                Console.WriteLine($"[TrackHistoryForm] ✅ Trovati {trackHistory.Count} passaggi per {_artist} - {_title}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TrackHistoryForm] ❌ Errore caricamento: {ex.Message}");
                MessageBox.Show(
                    LanguageManager.GetString("TrackHistory.Error.Load", "Errore durante il caricamento dello storico."),
                    LanguageManager.GetString("Common.Error", "Errore"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            try
            {
                if (_dgv.Rows.Count == 0)
                {
                    MessageBox.Show(
                        LanguageManager.GetString("TrackHistory.Error.NoData", "Nessun dato da esportare."),
                        LanguageManager.GetString("Common.Warning", "Attenzione"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                using var sfd = new SaveFileDialog
                {
                    Filter = "CSV Files (*.csv)|*.csv",
                    FileName = SanitizeFileName($"TrackHistory_{_artist}_{_title}_{DateTime.Now:yyyyMMdd}.csv"),
                    Title = LanguageManager.GetString("TrackHistory.Export.Title", "Esporta Storico")
                };

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine($"{LanguageManager.GetString("TrackHistory.Column.Date", "Data")};" +
                                  $"{LanguageManager.GetString("TrackHistory.Column.Time", "Ora")};" +
                                  $"{LanguageManager.GetString("TrackHistory.Column.Duration", "Durata")};" +
                                  $"{LanguageManager.GetString("TrackHistory.Column.Type", "Tipo")}");

                    foreach (DataGridViewRow row in _dgv.Rows)
                    {
                        sb.AppendLine($"{row.Cells[0].Value};{row.Cells[1].Value};{row.Cells[2].Value};{row.Cells[3].Value}");
                    }

                    File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);

                    MessageBox.Show(
                        LanguageManager.GetString("TrackHistory.Export.Success", "Esportazione completata!"),
                        LanguageManager.GetString("Common.Success", "Successo"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TrackHistoryForm] ❌ Errore export: {ex.Message}");
                MessageBox.Show(
                    LanguageManager.GetString("TrackHistory.Error.Export", "Errore durante l'esportazione."),
                    LanguageManager.GetString("Common.Error", "Errore"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private static string SanitizeFileName(string name)
        {
            char[] invalid = Path.GetInvalidFileNameChars();
            return string.Join("_", name.Split(invalid, StringSplitOptions.RemoveEmptyEntries));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                LanguageManager.LanguageChanged -= OnLanguageChanged;
            }
            base.Dispose(disposing);
        }
    }
}
