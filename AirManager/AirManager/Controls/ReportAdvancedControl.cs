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

namespace AirManager.Controls
{
    public partial class ReportAdvancedControl : UserControl
    {
        private DataGridView dgvReport;
        private Panel headerPanel;
        private Label lblHeader;
        private Label lblCount;

        private Label lblFrom;
        private Label lblTo;
        private DateTimePicker dtpFromDate;
        private DateTimePicker dtpToDate;
        private Button btnLoad;
        private Button btnToday;
        private Button btnYesterday;
        private Button btnLast7Days;
        private Button btnLast30Days;
        private Button btnExport;

        private List<ReportEntry> _currentData;

        public ReportAdvancedControl()
        {
            InitializeComponent();
            InitializeCustomUI();
            ApplyLanguage(); // ✅ APPLICA LINGUA INIZIALE
            _currentData = new List<ReportEntry>();

            // ✅ SOTTOSCRIVI EVENTO CAMBIO LINGUA
            LanguageManager.LanguageChanged += (s, e) => ApplyLanguage();
        }

        /// <summary>
        /// ✅ APPLICA LE TRADUZIONI DA LanguageManager
        /// </summary>
        private void ApplyLanguage()
        {
            // ✅ HEADER
            lblHeader.Text = "📊 " + LanguageManager.GetString("Report.HeaderTitle");

            if (_currentData == null || _currentData.Count == 0)
            {
                lblCount.Text = LanguageManager.GetString("Report.NoElements");
            }

            // ✅ FILTRI DATA
            lblFrom.Text = LanguageManager.GetString("Report.From");
            lblTo.Text = LanguageManager.GetString("Report.To");

            // ✅ BOTTONI
            btnLoad.Text = "📊 " + LanguageManager.GetString("Report.Load");
            btnToday.Text = LanguageManager.GetString("Report.Today");
            btnYesterday.Text = LanguageManager.GetString("Report.Yesterday");
            btnLast7Days.Text = LanguageManager.GetString("Report.Last7Days");
            btnLast30Days.Text = LanguageManager.GetString("Report.Last30Days");
            btnExport.Text = "💾 " + LanguageManager.GetString("Report.ExportCSV");

            // ✅ COLONNE DATAGRIDVIEW
            dgvReport.Columns["Date"].HeaderText = "📅 " + LanguageManager.GetString("Report.Column.Date");
            dgvReport.Columns["StartTime"].HeaderText = "🕐 " + LanguageManager.GetString("Report.Column.StartTime");
            dgvReport.Columns["EndTime"].HeaderText = "🕐 " + LanguageManager.GetString("Report.Column.EndTime");
            dgvReport.Columns["Artist"].HeaderText = "🎤 " + LanguageManager.GetString("Report.Column.Artist");
            dgvReport.Columns["Title"].HeaderText = "🎶 " + LanguageManager.GetString("Report.Column.Title");
            dgvReport.Columns["Type"].HeaderText = "🎵 " + LanguageManager.GetString("Report.Column.Type");
            dgvReport.Columns["PlayDuration"].HeaderText = "⏱️ " + LanguageManager.GetString("Report.Column.PlayDuration");

            Console.WriteLine($"[ReportAdvanced] ✅ Lingua applicata: {LanguageManager.CurrentLanguage}");
        }

        private void InitializeCustomUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = AppTheme.BgLight;
            this.Padding = new Padding(0);

            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 120,
                BackColor = AppTheme.BgDark,
                Padding = new Padding(15, 10, 15, 10)
            };

            lblHeader = new Label
            {
                Text = "📊 REPORT AVANZATO",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(15, 12),
                AutoSize = true
            };
            headerPanel.Controls.Add(lblHeader);

            lblCount = new Label
            {
                Text = "0 elementi",
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = AppTheme.LEDGreen,
                Location = new Point(15, 40),
                AutoSize = true
            };
            headerPanel.Controls.Add(lblCount);

            // ✅ FILTRI DATA - RIGA 1
            lblFrom = new Label
            {
                Text = "Da:",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(15, 75),
                Size = new Size(30, 25),
                TextAlign = ContentAlignment.MiddleLeft
            };
            headerPanel.Controls.Add(lblFrom);

            dtpFromDate = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Location = new Point(50, 73),
                Size = new Size(120, 25),
                Font = new Font("Segoe UI", 9),
                Value = DateTime.Now.AddDays(-7)
            };
            headerPanel.Controls.Add(dtpFromDate);

            lblTo = new Label
            {
                Text = "A:",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(180, 75),
                Size = new Size(25, 25),
                TextAlign = ContentAlignment.MiddleLeft
            };
            headerPanel.Controls.Add(lblTo);

            dtpToDate = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Location = new Point(210, 73),
                Size = new Size(120, 25),
                Font = new Font("Segoe UI", 9),
                Value = DateTime.Now
            };
            headerPanel.Controls.Add(dtpToDate);

            btnLoad = new Button
            {
                Text = "📊 Carica",
                Location = new Point(340, 71),
                Size = new Size(90, 29),
                BackColor = Color.FromArgb(0, 150, 136),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnLoad.FlatAppearance.BorderSize = 0;
            btnLoad.Click += BtnLoad_Click;
            headerPanel.Controls.Add(btnLoad);

            btnToday = new Button
            {
                Text = "Oggi",
                Location = new Point(440, 71),
                Size = new Size(70, 29),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnToday.FlatAppearance.BorderSize = 0;
            btnToday.Click += (s, e) => LoadQuickFilter(0, 0);
            headerPanel.Controls.Add(btnToday);

            btnYesterday = new Button
            {
                Text = "Ieri",
                Location = new Point(520, 71),
                Size = new Size(70, 29),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnYesterday.FlatAppearance.BorderSize = 0;
            btnYesterday.Click += (s, e) => LoadQuickFilter(-1, -1);
            headerPanel.Controls.Add(btnYesterday);

            btnLast7Days = new Button
            {
                Text = "7 giorni",
                Location = new Point(600, 71),
                Size = new Size(80, 29),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnLast7Days.FlatAppearance.BorderSize = 0;
            btnLast7Days.Click += (s, e) => LoadQuickFilter(-7, 0);
            headerPanel.Controls.Add(btnLast7Days);

            btnLast30Days = new Button
            {
                Text = "30 giorni",
                Location = new Point(690, 71),
                Size = new Size(85, 29),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnLast30Days.FlatAppearance.BorderSize = 0;
            btnLast30Days.Click += (s, e) => LoadQuickFilter(-30, 0);
            headerPanel.Controls.Add(btnLast30Days);

            btnExport = new Button
            {
                Text = "💾 Esporta CSV",
                Size = new Size(150, 45),
                BackColor = Color.FromArgb(138, 43, 226),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnExport.FlatAppearance.BorderSize = 0;
            btnExport.Click += BtnExport_Click;
            headerPanel.Controls.Add(btnExport);

            headerPanel.Resize += (s, e) => RepositionHeaderControls();
            RepositionHeaderControls();

            this.Controls.Add(headerPanel);

            dgvReport = new DataGridView
            {
                Location = new Point(0, 120),
                Size = new Size(this.Width, this.Height - 120),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BackgroundColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White,
                GridColor = Color.FromArgb(60, 60, 60),
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = true,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersHeight = 40,
                RowTemplate = { Height = 35 },
                Font = new Font("Segoe UI", 10),
                AllowUserToResizeRows = false,
                ScrollBars = ScrollBars.Both
            };

            dgvReport.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(50, 50, 50);
            dgvReport.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvReport.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvReport.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvReport.ColumnHeadersDefaultCellStyle.Padding = new Padding(5);

            dgvReport.DefaultCellStyle.BackColor = Color.FromArgb(40, 40, 40);
            dgvReport.DefaultCellStyle.ForeColor = Color.White;
            dgvReport.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 215);
            dgvReport.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvReport.DefaultCellStyle.Padding = new Padding(5);

            dgvReport.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(35, 35, 35);
            dgvReport.EnableHeadersVisualStyles = false;

            dgvReport.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Date",
                HeaderText = "📅 Data",
                FillWeight = 12,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            dgvReport.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "StartTime",
                HeaderText = "🕐 Inizio",
                FillWeight = 10,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            dgvReport.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "EndTime",
                HeaderText = "🕐 Fine",
                FillWeight = 10,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            dgvReport.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Artist",
                HeaderText = "🎤 Artista",
                FillWeight = 25,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft }
            });

            dgvReport.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Title",
                HeaderText = "🎶 Titolo",
                FillWeight = 26,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft }
            });

            dgvReport.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Type",
                HeaderText = "🎵 Tipo",
                FillWeight = 10,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            dgvReport.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "PlayDuration",
                HeaderText = "⏱️ Durata Play",
                FillWeight = 12,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            this.Controls.Add(dgvReport);

            this.Resize += (s, e) =>
            {
                dgvReport.Size = new Size(this.Width, this.Height - 120);
            };
        }

        private void RepositionHeaderControls()
        {
            const int MARGIN = 15;
            int panelWidth = headerPanel.Width;

            int exportX = panelWidth - btnExport.Width - MARGIN;
            btnExport.Location = new Point(exportX, 12);
        }

        private void LoadQuickFilter(int daysFromOffset, int daysToOffset)
        {
            dtpFromDate.Value = DateTime.Now.AddDays(daysFromOffset);
            dtpToDate.Value = DateTime.Now.AddDays(daysToOffset);
            LoadReport();
        }

        public void LoadDefaultReport()
        {
            dtpFromDate.Value = DateTime.Now.AddDays(-1);
            dtpToDate.Value = DateTime.Now;
            LoadReport();
        }

        private void BtnLoad_Click(object sender, EventArgs e)
        {
            LoadReport();
        }

        private void LoadReport()
        {
            try
            {
                dgvReport.Rows.Clear();

                DateTime from = dtpFromDate.Value.Date;
                DateTime to = dtpToDate.Value.Date.AddDays(1).AddSeconds(-1);

                if (from > to)
                {
                    MessageBox.Show(
                        LanguageManager.GetString("Report.Error.InvalidDateRange"),
                        LanguageManager.GetString("Report.Title.DateError"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                this.Cursor = Cursors.WaitCursor;

                var reports = ReportManager.LoadReport(from, to);

                var sortedReports = reports
                    .OrderByDescending(r => r.Date)
                    .ThenByDescending(r => r.StartTime)
                    .ToList();

                _currentData = sortedReports;

                foreach (var report in sortedReports)
                {
                    // ✅ TRADUCI TIPO
                    string typeDisplay = report.Type == "Music"
                        ? "🎵 " + LanguageManager.GetString("Report.Type.Music")
                        : "⚡ " + LanguageManager.GetString("Report.Type.Clip");

                    dgvReport.Rows.Add(
                        report.Date.ToString("dd/MM/yyyy"),
                        report.StartTime,
                        report.EndTime,
                        report.Artist,
                        report.Title,
                        typeDisplay,
                        report.PlayDuration
                    );
                }

                lblCount.Text = string.Format(
                    LanguageManager.GetString("Report.ElementsInPeriod"),
                    sortedReports.Count);
                lblCount.ForeColor = sortedReports.Count > 0 ? AppTheme.LEDGreen : Color.Orange;

                Console.WriteLine($"[ReportAdvanced] Caricati {sortedReports.Count} report da {from: dd/MM/yyyy} a {to:dd/MM/yyyy}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(LanguageManager.GetString("Report.Error.LoadFailed"), ex.Message),
                    LanguageManager.GetString("Common.Error"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                Console.WriteLine($"[ReportAdvanced] Errore:  {ex.Message}");
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            ShowExportDialog();
        }

        public void ShowExportDialog()
        {
            if (_currentData == null || _currentData.Count == 0)
            {
                MessageBox.Show(
                    LanguageManager.GetString("Report.Error.NoDataToExport"),
                    LanguageManager.GetString("Report.Title.NoData"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            using (var exportForm = new Forms.ReportExportDialog(_currentData))
            {
                exportForm.ShowDialog();
            }
        }

        private string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
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