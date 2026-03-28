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
    public partial class BroadcastHistoryForm : Form
    {
        private List<ReportEntry> _allData = new List<ReportEntry>();
        private List<ReportEntry> _filteredData = new List<ReportEntry>();

        // Header
        private Label lblTitle;

        // Filter controls
        private Panel pnlFilters;
        private Label lblDateFrom;
        private Label lblDateTo;
        private Label lblSearch;
        private Label lblType;
        private DateTimePicker dtpFrom;
        private DateTimePicker dtpTo;
        private TextBox txtSearch;
        private ComboBox cmbType;
        private Button btnRefresh;
        private Button btnExport;

        // DataGridView
        private DataGridView dgvHistory;

        // Statistics panel
        private Panel pnlStats;
        private Label lblStatTotal;
        private Label lblStatDuration;
        private Label lblStatTopArtist;
        private Label lblStatTopTrack;

        public BroadcastHistoryForm()
        {
            InitializeComponent();
            InitializeCustomUI();
            LoadData();
            ApplyLanguage();

            LanguageManager.LanguageChanged += OnLanguageChanged;
        }

        private void OnLanguageChanged(object sender, EventArgs e)
        {
            ApplyLanguage();
        }

        private void ApplyLanguage()
        {
            this.Text = "📻 " + LanguageManager.GetString("BroadcastHistory.Title", "Broadcast History");

            lblTitle.Text = "📻 " + LanguageManager.GetString("BroadcastHistory.Title", "Broadcast History");

            // Filter labels
            lblDateFrom.Text = LanguageManager.GetString("BroadcastHistory.DateFrom", "From:");
            lblDateTo.Text = LanguageManager.GetString("BroadcastHistory.DateTo", "To:");
            lblSearch.Text = LanguageManager.GetString("BroadcastHistory.Search", "Search:");
            lblType.Text = LanguageManager.GetString("BroadcastHistory.Type", "Type:");

            // Type ComboBox
            int currentTypeIndex = cmbType.SelectedIndex;
            cmbType.Items.Clear();
            cmbType.Items.Add(LanguageManager.GetString("BroadcastHistory.Type.All", "All"));
            cmbType.Items.Add(LanguageManager.GetString("BroadcastHistory.Type.Music", "Music"));
            cmbType.Items.Add(LanguageManager.GetString("BroadcastHistory.Type.Clips", "Clips"));
            cmbType.SelectedIndex = currentTypeIndex >= 0 ? currentTypeIndex : 0;

            // Buttons
            btnRefresh.Text = "🔄 " + LanguageManager.GetString("BroadcastHistory.Refresh", "Refresh");
            btnExport.Text = "💾 " + LanguageManager.GetString("BroadcastHistory.Export", "Export CSV");

            // Grid columns
            if (dgvHistory.Columns.Count >= 6)
            {
                dgvHistory.Columns["colDate"].HeaderText = LanguageManager.GetString("BroadcastHistory.Column.Date", "Date");
                dgvHistory.Columns["colTime"].HeaderText = LanguageManager.GetString("BroadcastHistory.Column.Time", "Time");
                dgvHistory.Columns["colType"].HeaderText = LanguageManager.GetString("BroadcastHistory.Column.Type", "Type");
                dgvHistory.Columns["colArtist"].HeaderText = LanguageManager.GetString("BroadcastHistory.Column.Artist", "Artist");
                dgvHistory.Columns["colTitle"].HeaderText = LanguageManager.GetString("BroadcastHistory.Column.Title", "Title");
                dgvHistory.Columns["colDuration"].HeaderText = LanguageManager.GetString("BroadcastHistory.Column.Duration", "Duration");
            }

            UpdateStatisticsLabels();
        }

        private void InitializeCustomUI()
        {
            this.Text = "📻 Broadcast History";
            this.Size = new Size(1100, 700);
            this.MinimumSize = new Size(900, 550);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = AppTheme.BgLight;

            // ✅ HEADER PANEL
            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = AppTheme.BgDark
            };

            lblTitle = new Label
            {
                Text = "📻 BROADCAST HISTORY",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 15),
                AutoSize = true
            };
            headerPanel.Controls.Add(lblTitle);
            this.Controls.Add(headerPanel);

            // ✅ FILTER PANEL
            pnlFilters = new Panel
            {
                Dock = DockStyle.Top,
                Height = 55,
                BackColor = AppTheme.BgPanel,
                Padding = new Padding(10, 8, 10, 8)
            };

            int filterX = 15;
            int filterY = 14;

            lblDateFrom = new Label
            {
                Text = "From:",
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = AppTheme.TextSecondary,
                Location = new Point(filterX, filterY + 2),
                AutoSize = true
            };
            pnlFilters.Controls.Add(lblDateFrom);
            filterX += 45;

            dtpFrom = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Font = new Font("Segoe UI", 9),
                Location = new Point(filterX, filterY),
                Size = new Size(110, 25),
                Value = DateTime.Today
            };
            dtpFrom.ValueChanged += (s, e) => ApplyFilters();
            pnlFilters.Controls.Add(dtpFrom);
            filterX += 120;

            lblDateTo = new Label
            {
                Text = "To:",
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = AppTheme.TextSecondary,
                Location = new Point(filterX, filterY + 2),
                AutoSize = true
            };
            pnlFilters.Controls.Add(lblDateTo);
            filterX += 30;

            dtpTo = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Font = new Font("Segoe UI", 9),
                Location = new Point(filterX, filterY),
                Size = new Size(110, 25),
                Value = DateTime.Today
            };
            dtpTo.ValueChanged += (s, e) => ApplyFilters();
            pnlFilters.Controls.Add(dtpTo);
            filterX += 130;

            lblType = new Label
            {
                Text = "Type:",
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = AppTheme.TextSecondary,
                Location = new Point(filterX, filterY + 2),
                AutoSize = true
            };
            pnlFilters.Controls.Add(lblType);
            filterX += 45;

            cmbType = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9),
                Location = new Point(filterX, filterY),
                Size = new Size(100, 25),
                BackColor = AppTheme.BgInput,
                ForeColor = AppTheme.TextPrimary
            };
            cmbType.Items.AddRange(new object[] { "All", "Music", "Clips" });
            cmbType.SelectedIndex = 0;
            cmbType.SelectedIndexChanged += (s, e) => ApplyFilters();
            pnlFilters.Controls.Add(cmbType);
            filterX += 115;

            lblSearch = new Label
            {
                Text = "Search:",
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = AppTheme.TextSecondary,
                Location = new Point(filterX, filterY + 2),
                AutoSize = true
            };
            pnlFilters.Controls.Add(lblSearch);
            filterX += 55;

            txtSearch = new TextBox
            {
                Font = new Font("Segoe UI", 9),
                Location = new Point(filterX, filterY),
                Size = new Size(160, 25),
                BackColor = AppTheme.BgInput,
                ForeColor = AppTheme.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle
            };
            txtSearch.TextChanged += (s, e) => ApplyFilters();
            pnlFilters.Controls.Add(txtSearch);
            filterX += 175;

            btnRefresh = new Button
            {
                Text = "🔄 Refresh",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = new Point(filterX, filterY - 2),
                Size = new Size(100, 30),
                BackColor = AppTheme.ButtonPrimary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += BtnRefresh_Click;
            pnlFilters.Controls.Add(btnRefresh);
            filterX += 110;

            btnExport = new Button
            {
                Text = "💾 Export CSV",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = new Point(filterX, filterY - 2),
                Size = new Size(110, 30),
                BackColor = AppTheme.ButtonSuccess,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnExport.FlatAppearance.BorderSize = 0;
            btnExport.Click += BtnExport_Click;
            pnlFilters.Controls.Add(btnExport);

            this.Controls.Add(pnlFilters);

            // ✅ STATISTICS PANEL (bottom)
            pnlStats = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 70,
                BackColor = AppTheme.BgDark,
                Padding = new Padding(15, 10, 15, 10)
            };

            lblStatTotal = new Label
            {
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = AppTheme.TextAccent,
                Location = new Point(15, 10),
                AutoSize = true,
                Text = ""
            };
            pnlStats.Controls.Add(lblStatTotal);

            lblStatDuration = new Label
            {
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = AppTheme.TextAccent,
                Location = new Point(15, 35),
                AutoSize = true,
                Text = ""
            };
            pnlStats.Controls.Add(lblStatDuration);

            lblStatTopArtist = new Label
            {
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = AppTheme.TextSecondary,
                Location = new Point(350, 10),
                AutoSize = true,
                Text = ""
            };
            pnlStats.Controls.Add(lblStatTopArtist);

            lblStatTopTrack = new Label
            {
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = AppTheme.TextSecondary,
                Location = new Point(350, 35),
                AutoSize = true,
                Text = ""
            };
            pnlStats.Controls.Add(lblStatTopTrack);

            this.Controls.Add(pnlStats);

            // ✅ DATAGRIDVIEW (fills remaining space)
            dgvHistory = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                BackgroundColor = AppTheme.BgDark,
                GridColor = AppTheme.GridBorder,
                Font = new Font("Segoe UI", 9)
            };

            // Column header style
            dgvHistory.EnableHeadersVisualStyles = false;
            dgvHistory.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = AppTheme.GridHeader,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                Padding = new Padding(5, 0, 0, 0),
                SelectionBackColor = AppTheme.GridHeader,
                SelectionForeColor = Color.White
            };
            dgvHistory.ColumnHeadersHeight = 35;
            dgvHistory.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            // Default cell style
            dgvHistory.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = AppTheme.GridRowEven,
                ForeColor = Color.White,
                SelectionBackColor = AppTheme.GridSelection,
                SelectionForeColor = Color.White,
                Padding = new Padding(5, 0, 0, 0)
            };

            dgvHistory.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = AppTheme.GridRowOdd,
                ForeColor = Color.White,
                SelectionBackColor = AppTheme.GridSelection,
                SelectionForeColor = Color.White,
                Padding = new Padding(5, 0, 0, 0)
            };

            dgvHistory.RowTemplate.Height = 28;

            // Define columns
            dgvHistory.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colDate",
                HeaderText = "Date",
                FillWeight = 12
            });
            dgvHistory.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colTime",
                HeaderText = "Time",
                FillWeight = 12
            });
            dgvHistory.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colType",
                HeaderText = "Type",
                FillWeight = 10
            });
            dgvHistory.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colArtist",
                HeaderText = "Artist",
                FillWeight = 25
            });
            dgvHistory.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colTitle",
                HeaderText = "Title",
                FillWeight = 30
            });
            dgvHistory.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colDuration",
                HeaderText = "Duration",
                FillWeight = 11
            });

            dgvHistory.CellFormatting += DgvHistory_CellFormatting;

            this.Controls.Add(dgvHistory);
        }

        private void LoadData()
        {
            try
            {
                _allData = ReportManager.LoadReport(dtpFrom.Value.Date, dtpTo.Value.Date);
                ApplyFilters();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BroadcastHistory] ❌ Error loading data: {ex.Message}");
                _allData = new List<ReportEntry>();
                ApplyFilters();
            }
        }

        private void ApplyFilters()
        {
            string searchText = txtSearch.Text.Trim().ToLowerInvariant();
            int typeIndex = cmbType.SelectedIndex;

            _filteredData = _allData.Where(entry =>
            {
                // Type filter
                if (typeIndex == 1 && !string.Equals(entry.Type, "Music", StringComparison.OrdinalIgnoreCase))
                    return false;
                if (typeIndex == 2 && !string.Equals(entry.Type, "Clip", StringComparison.OrdinalIgnoreCase))
                    return false;

                // Search filter (artist or title)
                if (!string.IsNullOrEmpty(searchText))
                {
                    bool matchArtist = (entry.Artist ?? "").ToLowerInvariant().Contains(searchText);
                    bool matchTitle = (entry.Title ?? "").ToLowerInvariant().Contains(searchText);
                    if (!matchArtist && !matchTitle)
                        return false;
                }

                return true;
            }).ToList();

            PopulateGrid();
            UpdateStatistics();
        }

        private void PopulateGrid()
        {
            dgvHistory.Rows.Clear();

            foreach (var entry in _filteredData)
            {
                dgvHistory.Rows.Add(
                    entry.Date.ToString("dd/MM/yyyy"),
                    entry.StartTime,
                    entry.Type,
                    entry.Artist,
                    entry.Title,
                    entry.PlayDuration
                );
            }
        }

        private void UpdateStatistics()
        {
            UpdateStatisticsLabels();
        }

        private void UpdateStatisticsLabels()
        {
            // Total items
            string totalLabel = LanguageManager.GetString("BroadcastHistory.Stats.Total", "Total items:");
            lblStatTotal.Text = $"📊 {totalLabel} {_filteredData.Count}";

            // Total duration
            TimeSpan totalDuration = TimeSpan.Zero;
            foreach (var entry in _filteredData)
            {
                if (TimeSpan.TryParse(entry.PlayDuration, out TimeSpan dur))
                    totalDuration += dur;
            }
            string durationLabel = LanguageManager.GetString("BroadcastHistory.Stats.Duration", "Total duration:");
            lblStatDuration.Text = $"⏱️ {durationLabel} {(int)totalDuration.TotalHours:D2}:{totalDuration.Minutes:D2}:{totalDuration.Seconds:D2}";

            // Most played artist
            string topArtistLabel = LanguageManager.GetString("BroadcastHistory.Stats.TopArtist", "Most played artist:");
            var topArtist = _filteredData
                .Where(r => !string.IsNullOrWhiteSpace(r.Artist))
                .GroupBy(r => r.Artist)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault();
            lblStatTopArtist.Text = topArtist != null
                ? $"🎤 {topArtistLabel} {topArtist.Key} ({topArtist.Count()}x)"
                : $"🎤 {topArtistLabel} —";

            // Most played track
            string topTrackLabel = LanguageManager.GetString("BroadcastHistory.Stats.TopTrack", "Most played track:");
            var topTrack = _filteredData
                .Where(r => !string.IsNullOrWhiteSpace(r.Title))
                .GroupBy(r => $"{r.Artist} - {r.Title}")
                .OrderByDescending(g => g.Count())
                .FirstOrDefault();
            lblStatTopTrack.Text = topTrack != null
                ? $"🎶 {topTrackLabel} {topTrack.Key} ({topTrack.Count()}x)"
                : $"🎶 {topTrackLabel} —";
        }

        private void DgvHistory_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex == dgvHistory.Columns["colType"].Index && e.Value != null)
            {
                string type = e.Value.ToString();
                e.CellStyle.ForeColor = AppTheme.GetArchiveColor(type);
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadData();
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            if (_filteredData.Count == 0)
            {
                MessageBox.Show(
                    LanguageManager.GetString("BroadcastHistory.Export.NoData", "No data to export."),
                    LanguageManager.GetString("BroadcastHistory.Title", "Broadcast History"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            using (var dialog = new ReportExportDialog(_filteredData))
            {
                dialog.ShowDialog(this);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                LanguageManager.LanguageChanged -= OnLanguageChanged;
                components?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
