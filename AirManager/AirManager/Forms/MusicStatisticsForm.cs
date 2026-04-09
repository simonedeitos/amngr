using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using AirManager.Services;
using AirManager.Services.Database;
using AirManager.Themes;

namespace AirManager.Forms
{
    public partial class MusicStatisticsForm : Form
    {
        private DateTimePicker dtpFrom = null!;
        private DateTimePicker dtpTo = null!;
        private Button btnUpdate = null!;
        private TabControl tabControl = null!;

        private ChartPanel chartTopTracks = null!;
        private ChartPanel chartTopArtists = null!;
        private ChartPanel chartDailyTrend = null!;
        private ChartPanel chartHourlyDist = null!;
        private ChartPanel chartWeekdayDist = null!;
        private ChartPanel chartAvgDuration = null!;
        private Panel pnlRotation = null!;
        private Panel pnlSummary = null!;
        private Panel pnlSingleTrack = null!;
        private Panel pnlSingleArtist = null!;

        private ComboBox cmbSingleTrack = null!;
        private ComboBox cmbSingleArtist = null!;

        private List<ReportEntry> _data = new List<ReportEntry>();

        public MusicStatisticsForm()
        {
            InitializeComponent();
            InitializeCustomUI();
            ApplyLanguage();
            LanguageManager.LanguageChanged += OnLanguageChanged;
            this.FormClosed += (s, e) => LanguageManager.LanguageChanged -= OnLanguageChanged;
        }

        private void OnLanguageChanged(object? sender, EventArgs e) => ApplyLanguage();

        private void ApplyLanguage()
        {
            this.Text = "📊 " + LanguageManager.GetString("MusicStatistics.Title", "Music Statistics");
            btnUpdate.Text = "🔄 " + LanguageManager.GetString("MusicStatistics.Update", "Update");

            if (tabControl.TabPages.Count >= 10)
            {
                tabControl.TabPages[0].Text = LanguageManager.GetString("MusicStatistics.Tab.TopTracks", "Top Tracks");
                tabControl.TabPages[1].Text = LanguageManager.GetString("MusicStatistics.Tab.TopArtists", "Top Artists");
                tabControl.TabPages[2].Text = LanguageManager.GetString("MusicStatistics.Tab.DailyTrend", "Daily Trend");
                tabControl.TabPages[3].Text = LanguageManager.GetString("MusicStatistics.Tab.HourlyDist", "Hourly Distribution");
                tabControl.TabPages[4].Text = LanguageManager.GetString("MusicStatistics.Tab.WeekdayDist", "Weekday Distribution");
                tabControl.TabPages[5].Text = LanguageManager.GetString("MusicStatistics.Tab.AvgDuration", "Avg Duration/Hour");
                tabControl.TabPages[6].Text = LanguageManager.GetString("MusicStatistics.Tab.Rotation", "Rotation Index");
                tabControl.TabPages[7].Text = LanguageManager.GetString("MusicStatistics.Tab.Summary", "Summary");
                tabControl.TabPages[8].Text = LanguageManager.GetString("MusicStatistics.Tab.SingleTrack", "Single Track");
                tabControl.TabPages[9].Text = LanguageManager.GetString("MusicStatistics.Tab.SingleArtist", "Single Artist");
            }
        }

        private void InitializeCustomUI()
        {
            this.Text = "📊 Music Statistics";
            this.Size = new Size(1200, 800);
            this.MinimumSize = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = AppTheme.BgDark;

            // ── Header / filter bar ──────────────────────────────────────────
            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = AppTheme.BgPanel,
                Padding = new Padding(10, 8, 10, 8)
            };

            int x = 15;
            var lblFrom = new Label
            {
                Text = "From:",
                ForeColor = AppTheme.TextSecondary,
                Font = new Font("Segoe UI", 9),
                Location = new Point(x, 14),
                AutoSize = true
            };
            pnlHeader.Controls.Add(lblFrom);
            x += 45;

            dtpFrom = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Font = new Font("Segoe UI", 9),
                Location = new Point(x, 12),
                Size = new Size(110, 25),
                Value = DateTime.Today.AddMonths(-1)
            };
            pnlHeader.Controls.Add(dtpFrom);
            x += 120;

            var lblTo = new Label
            {
                Text = "To:",
                ForeColor = AppTheme.TextSecondary,
                Font = new Font("Segoe UI", 9),
                Location = new Point(x, 14),
                AutoSize = true
            };
            pnlHeader.Controls.Add(lblTo);
            x += 30;

            dtpTo = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Font = new Font("Segoe UI", 9),
                Location = new Point(x, 12),
                Size = new Size(110, 25),
                Value = DateTime.Today
            };
            pnlHeader.Controls.Add(dtpTo);
            x += 120;

            btnUpdate = new Button
            {
                Text = "🔄 Update",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = new Point(x, 10),
                Size = new Size(110, 30),
                BackColor = AppTheme.ButtonPrimary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnUpdate.FlatAppearance.BorderSize = 0;
            btnUpdate.Click += BtnUpdate_Click;
            pnlHeader.Controls.Add(btnUpdate);

            // ── TabControl ───────────────────────────────────────────────────
            tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9),
                BackColor = AppTheme.BgDark,
                Padding = new Point(8, 4)
            };

            chartTopTracks   = new ChartPanel();
            chartTopArtists  = new ChartPanel();
            chartDailyTrend  = new ChartPanel();
            chartHourlyDist  = new ChartPanel();
            chartWeekdayDist = new ChartPanel();
            chartAvgDuration = new ChartPanel();

            pnlRotation = CreateDarkPanel();
            pnlSummary  = CreateDarkPanel();
            pnlSingleTrack  = CreateDarkPanel();
            pnlSingleArtist = CreateDarkPanel();

            // Build Single Track tab container (ComboBox on top + detail panel below)
            var tabSingleTrack = new TabPage("🎵 Single Track") { BackColor = AppTheme.BgDark };
            cmbSingleTrack = new ComboBox
            {
                Dock = DockStyle.Top,
                Height = 28,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9),
                BackColor = AppTheme.BgInput,
                ForeColor = AppTheme.TextPrimary
            };
            cmbSingleTrack.SelectedIndexChanged += (s, e) => BuildSingleTrackPanel();
            tabSingleTrack.Controls.Add(pnlSingleTrack);
            tabSingleTrack.Controls.Add(cmbSingleTrack);

            // Build Single Artist tab container (ComboBox on top + detail panel below)
            var tabSingleArtist = new TabPage("🎤 Single Artist") { BackColor = AppTheme.BgDark };
            cmbSingleArtist = new ComboBox
            {
                Dock = DockStyle.Top,
                Height = 28,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9),
                BackColor = AppTheme.BgInput,
                ForeColor = AppTheme.TextPrimary
            };
            cmbSingleArtist.SelectedIndexChanged += (s, e) => BuildSingleArtistPanel();
            tabSingleArtist.Controls.Add(pnlSingleArtist);
            tabSingleArtist.Controls.Add(cmbSingleArtist);

            tabControl.TabPages.Add(CreateTab("📊 Top Tracks",           chartTopTracks));
            tabControl.TabPages.Add(CreateTab("🎤 Top Artists",          chartTopArtists));
            tabControl.TabPages.Add(CreateTab("📈 Daily Trend",          chartDailyTrend));
            tabControl.TabPages.Add(CreateTab("🕐 Hourly Distribution",  chartHourlyDist));
            tabControl.TabPages.Add(CreateTab("📅 Weekday Distribution", chartWeekdayDist));
            tabControl.TabPages.Add(CreateTab("⏱️ Avg Duration/Hour",    chartAvgDuration));
            tabControl.TabPages.Add(CreateTab("🔄 Rotation Index",       pnlRotation));
            tabControl.TabPages.Add(CreateTab("📋 Summary",              pnlSummary));
            tabControl.TabPages.Add(tabSingleTrack);
            tabControl.TabPages.Add(tabSingleArtist);

            // correct dock order: header (Top) added after tabControl (Fill)
            this.Controls.Add(tabControl);
            this.Controls.Add(pnlHeader);
        }

        // ── Helpers ─────────────────────────────────────────────────────────

        private static Panel CreateDarkPanel() =>
            new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.BgDark, AutoScroll = true };

        private static TabPage CreateTab(string title, Control control)
        {
            var page = new TabPage(title) { BackColor = AppTheme.BgDark };
            page.Controls.Add(control);
            return page;
        }

        // ── Data loading & chart building ────────────────────────────────────

        private void BtnUpdate_Click(object? sender, EventArgs e)
        {
            LoadAndRefresh();
        }

        private void LoadAndRefresh()
        {
            try
            {
                var raw = ReportManager.LoadReport(dtpFrom.Value.Date, dtpTo.Value.Date);
                _data = raw.Where(r => string.Equals(r.Type, "Music", StringComparison.OrdinalIgnoreCase)).ToList();
                RefreshAllCharts();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(LanguageManager.GetString("MusicStatistics.Error.Load", "Error loading data: {0}"), ex.Message),
                    LanguageManager.GetString("MusicStatistics.Title", "Music Statistics"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshAllCharts()
        {
            BuildTopTracksChart();
            BuildTopArtistsChart();
            BuildDailyTrendChart();
            BuildHourlyDistChart();
            BuildWeekdayDistChart();
            BuildAvgDurationChart();
            BuildRotationPanel();
            BuildSummaryPanel();
            PopulateSingleTrackCombo();
            PopulateSingleArtistCombo();
        }

        // 1. Top N Tracks ────────────────────────────────────────────────────
        private void BuildTopTracksChart()
        {
            if (_data.Count == 0) { chartTopTracks.ClearData(); return; }

            var top = _data
                .GroupBy(r => $"{r.Artist} – {r.Title}")
                .OrderByDescending(g => g.Count())
                .Take(20)
                .Reverse()
                .ToList();

            double[] values = top.Select(g => (double)g.Count()).ToArray();
            string[] labels = top.Select(g => g.Key.Length > 40 ? g.Key.Substring(0, 40) + "…" : g.Key).ToArray();

            chartTopTracks.SetBarData(
                values, labels, horizontal: true,
                title: LanguageManager.GetString("MusicStatistics.Chart.TopTracks", "Top 20 Most Played Tracks"),
                xLabel: LanguageManager.GetString("MusicStatistics.Chart.Plays", "Plays"),
                yLabel: "");
        }

        // 2. Top N Artists ───────────────────────────────────────────────────
        private void BuildTopArtistsChart()
        {
            if (_data.Count == 0) { chartTopArtists.ClearData(); return; }

            var top = _data
                .Where(r => !string.IsNullOrWhiteSpace(r.Artist))
                .GroupBy(r => r.Artist)
                .OrderByDescending(g => g.Count())
                .Take(20)
                .Reverse()
                .ToList();

            double[] values = top.Select(g => (double)g.Count()).ToArray();
            string[] labels = top.Select(g => g.Key.Length > 30 ? g.Key.Substring(0, 30) + "…" : g.Key).ToArray();

            chartTopArtists.SetBarData(
                values, labels, horizontal: true,
                title: LanguageManager.GetString("MusicStatistics.Chart.TopArtists", "Top 20 Most Played Artists"),
                xLabel: LanguageManager.GetString("MusicStatistics.Chart.Plays", "Plays"),
                yLabel: "");
        }

        // 3. Daily Trend ─────────────────────────────────────────────────────
        private void BuildDailyTrendChart()
        {
            if (_data.Count == 0) { chartDailyTrend.ClearData(); return; }

            var byDay = _data
                .GroupBy(r => r.Date.Date)
                .OrderBy(g => g.Key)
                .ToList();

            double[] xs = byDay.Select(g => g.Key.ToOADate()).ToArray();
            double[] ys = byDay.Select(g => (double)g.Count()).ToArray();
            string[] xLabels = byDay.Select(g => g.Key.ToString("dd/MM")).ToArray();

            chartDailyTrend.SetScatterData(
                xs, ys, xLabels,
                title: LanguageManager.GetString("MusicStatistics.Chart.DailyTrend", "Daily Plays Trend"),
                xLabel: "",
                yLabel: LanguageManager.GetString("MusicStatistics.Chart.Plays", "Plays"));
        }

        // 4. Hourly Distribution ─────────────────────────────────────────────
        private void BuildHourlyDistChart()
        {
            if (_data.Count == 0) { chartHourlyDist.ClearData(); return; }

            var byHour = new double[24];
            foreach (var r in _data)
            {
                if (TimeSpan.TryParse(r.StartTime, out var ts))
                    byHour[ts.Hours]++;
            }

            string[] hourLabels = Enumerable.Range(0, 24).Select(i => $"{i:D2}:00").ToArray();

            chartHourlyDist.SetBarData(
                byHour, hourLabels, horizontal: false,
                title: LanguageManager.GetString("MusicStatistics.Chart.HourlyDist", "Plays by Hour of Day"),
                xLabel: LanguageManager.GetString("MusicStatistics.Chart.Hour", "Hour"),
                yLabel: LanguageManager.GetString("MusicStatistics.Chart.Plays", "Plays"));
        }

        // 5. Weekday Distribution ────────────────────────────────────────────
        private void BuildWeekdayDistChart()
        {
            if (_data.Count == 0) { chartWeekdayDist.ClearData(); return; }

            var byDay = new double[7];
            foreach (var r in _data)
            {
                int idx = ((int)r.Date.DayOfWeek + 6) % 7; // Mon=0..Sun=6
                byDay[idx]++;
            }

            string[] dayLabels = {
                LanguageManager.GetString("MusicStatistics.Day.Mon", "Mon"),
                LanguageManager.GetString("MusicStatistics.Day.Tue", "Tue"),
                LanguageManager.GetString("MusicStatistics.Day.Wed", "Wed"),
                LanguageManager.GetString("MusicStatistics.Day.Thu", "Thu"),
                LanguageManager.GetString("MusicStatistics.Day.Fri", "Fri"),
                LanguageManager.GetString("MusicStatistics.Day.Sat", "Sat"),
                LanguageManager.GetString("MusicStatistics.Day.Sun", "Sun")
            };

            chartWeekdayDist.SetBarData(
                byDay, dayLabels, horizontal: false,
                title: LanguageManager.GetString("MusicStatistics.Chart.WeekdayDist", "Plays by Day of Week"),
                xLabel: "",
                yLabel: LanguageManager.GetString("MusicStatistics.Chart.Plays", "Plays"));
        }

        // 6. Avg Duration per Hour ───────────────────────────────────────────
        private void BuildAvgDurationChart()
        {
            if (_data.Count == 0) { chartAvgDuration.ClearData(); return; }

            var totalSec = new double[24];
            var counts   = new int[24];
            foreach (var r in _data)
            {
                if (TimeSpan.TryParse(r.StartTime, out var ts) &&
                    TimeSpan.TryParse(r.PlayDuration, out var dur))
                {
                    totalSec[ts.Hours] += dur.TotalSeconds;
                    counts[ts.Hours]++;
                }
            }

            double[] ys = Enumerable.Range(0, 24)
                .Select(i => counts[i] > 0 ? totalSec[i] / counts[i] : 0)
                .ToArray();

            string[] hourLabels = Enumerable.Range(0, 24).Select(i => $"{i:D2}:00").ToArray();

            chartAvgDuration.SetBarData(
                ys, hourLabels, horizontal: false,
                title: LanguageManager.GetString("MusicStatistics.Chart.AvgDuration", "Average Play Duration by Hour (seconds)"),
                xLabel: LanguageManager.GetString("MusicStatistics.Chart.Hour", "Hour"),
                yLabel: LanguageManager.GetString("MusicStatistics.Chart.AvgSec", "Avg seconds"));
        }

        // 7. Rotation Index ──────────────────────────────────────────────────
        private void BuildRotationPanel()
        {
            pnlRotation.Controls.Clear();
            int total   = _data.Count;
            int unique  = _data.Select(r => $"{r.Artist}|{r.Title}").Distinct().Count();
            int repeats = total - unique;
            double rotIdx = total > 0 ? Math.Round((double)unique / total * 100, 1) : 0;

            var lv = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                BackColor = AppTheme.BgPanel,
                ForeColor = AppTheme.TextPrimary,
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.None
            };
            lv.Columns.Add(LanguageManager.GetString("MusicStatistics.Rotation.Metric", "Metric"), 400);
            lv.Columns.Add(LanguageManager.GetString("MusicStatistics.Rotation.Value", "Value"), 200);

            lv.Items.Add(new ListViewItem(new[]
            {
                LanguageManager.GetString("MusicStatistics.Rotation.TotalPlays", "Total plays"),
                total.ToString()
            }));
            lv.Items.Add(new ListViewItem(new[]
            {
                LanguageManager.GetString("MusicStatistics.Rotation.UniqueTracks", "Unique tracks"),
                unique.ToString()
            }));
            lv.Items.Add(new ListViewItem(new[]
            {
                LanguageManager.GetString("MusicStatistics.Rotation.Repeats", "Repeated plays"),
                repeats.ToString()
            }));
            lv.Items.Add(new ListViewItem(new[]
            {
                LanguageManager.GetString("MusicStatistics.Rotation.RotationIndex", "Rotation index (% unique)"),
                $"{rotIdx}%"
            }));

            var topRepeated = _data
                .GroupBy(r => $"{r.Artist} – {r.Title}")
                .Where(g => g.Count() > 1)
                .OrderByDescending(g => g.Count())
                .Take(10);

            foreach (var g in topRepeated)
            {
                lv.Items.Add(new ListViewItem(new[]
                {
                    $"  ↺ {g.Key}",
                    $"{g.Count()}x"
                }));
            }

            pnlRotation.Controls.Add(lv);
        }

        // 8. Summary table ───────────────────────────────────────────────────
        private void BuildSummaryPanel()
        {
            pnlSummary.Controls.Clear();
            if (_data.Count == 0)
            {
                pnlSummary.Controls.Add(new Label
                {
                    Text = LanguageManager.GetString("MusicStatistics.NoData", "No data available for the selected period."),
                    ForeColor = AppTheme.TextSecondary,
                    Font = new Font("Segoe UI", 11),
                    AutoSize = true,
                    Location = new Point(20, 20)
                });
                return;
            }

            int total  = _data.Count;
            int unique = _data.Select(r => $"{r.Artist}|{r.Title}").Distinct().Count();
            int uniqueArtists = _data.Where(r => !string.IsNullOrWhiteSpace(r.Artist))
                                     .Select(r => r.Artist).Distinct().Count();

            TimeSpan totalDur = TimeSpan.Zero, maxDur = TimeSpan.Zero, minDur = TimeSpan.MaxValue;
            int durCount = 0;
            foreach (var r in _data)
            {
                if (TimeSpan.TryParse(r.PlayDuration, out var d))
                {
                    totalDur += d;
                    if (d > maxDur) maxDur = d;
                    if (d < minDur) minDur = d;
                    durCount++;
                }
            }
            TimeSpan avgDur = durCount > 0 ? TimeSpan.FromSeconds(totalDur.TotalSeconds / durCount) : TimeSpan.Zero;
            if (minDur == TimeSpan.MaxValue) minDur = TimeSpan.Zero;

            var lv = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                BackColor = AppTheme.BgPanel,
                ForeColor = AppTheme.TextPrimary,
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.None
            };
            lv.Columns.Add(LanguageManager.GetString("MusicStatistics.Summary.Metric", "Metric"), 400);
            lv.Columns.Add(LanguageManager.GetString("MusicStatistics.Summary.Value", "Value"), 300);

            void AddRow(string metricKey, string metricFallback, string value)
                => lv.Items.Add(new ListViewItem(new[] { LanguageManager.GetString(metricKey, metricFallback), value }));

            AddRow("MusicStatistics.Summary.TotalTracks",   "Total tracks played",    total.ToString());
            AddRow("MusicStatistics.Summary.UniqueTracks",  "Unique tracks",          unique.ToString());
            AddRow("MusicStatistics.Summary.UniqueArtists", "Unique artists",         uniqueArtists.ToString());
            AddRow("MusicStatistics.Summary.TotalDuration", "Total duration",         FormatTs(totalDur));
            AddRow("MusicStatistics.Summary.AvgDuration",   "Average duration",       FormatTs(avgDur));
            AddRow("MusicStatistics.Summary.MaxDuration",   "Longest track",          FormatTs(maxDur));
            AddRow("MusicStatistics.Summary.MinDuration",   "Shortest track",         FormatTs(minDur));

            var topArtist = _data
                .Where(r => !string.IsNullOrWhiteSpace(r.Artist))
                .GroupBy(r => r.Artist)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault();
            AddRow("MusicStatistics.Summary.TopArtist", "Most played artist",
                topArtist != null ? $"{topArtist.Key} ({topArtist.Count()}x)" : "—");

            var topTrack = _data
                .GroupBy(r => $"{r.Artist} – {r.Title}")
                .OrderByDescending(g => g.Count())
                .FirstOrDefault();
            AddRow("MusicStatistics.Summary.TopTrack", "Most played track",
                topTrack != null ? $"{topTrack.Key} ({topTrack.Count()}x)" : "—");

            pnlSummary.Controls.Add(lv);
        }

        // ── Utilities ────────────────────────────────────────────────────────

        private static string FormatTs(TimeSpan ts)
            => $"{(int)ts.TotalHours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";

        // ── Single Track / Single Artist helpers ──────────────────────────────

        private void PopulateSingleTrackCombo()
        {
            cmbSingleTrack.Items.Clear();
            var tracks = _data
                .Select(r => $"{r.Artist} – {r.Title}")
                .Distinct()
                .OrderBy(s => s)
                .ToList();
            foreach (var t in tracks)
                cmbSingleTrack.Items.Add(t);
            if (cmbSingleTrack.Items.Count > 0)
                cmbSingleTrack.SelectedIndex = 0;
            else
                BuildSingleTrackPanel();
        }

        private void PopulateSingleArtistCombo()
        {
            cmbSingleArtist.Items.Clear();
            var artists = _data
                .Where(r => !string.IsNullOrWhiteSpace(r.Artist))
                .Select(r => r.Artist)
                .Distinct()
                .OrderBy(a => a)
                .ToList();
            foreach (var a in artists)
                cmbSingleArtist.Items.Add(a);
            if (cmbSingleArtist.Items.Count > 0)
                cmbSingleArtist.SelectedIndex = 0;
            else
                BuildSingleArtistPanel();
        }

        // 9. Single Track ────────────────────────────────────────────────────
        private void BuildSingleTrackPanel()
        {
            pnlSingleTrack.Controls.Clear();
            if (_data.Count == 0 || cmbSingleTrack.SelectedItem == null)
            {
                AddNoDataLabel(pnlSingleTrack);
                return;
            }

            string? selected = cmbSingleTrack.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selected)) { AddNoDataLabel(pnlSingleTrack); return; }
            var trackData = _data
                .Where(r => $"{r.Artist} – {r.Title}" == selected)
                .ToList();

            if (trackData.Count == 0) { AddNoDataLabel(pnlSingleTrack); return; }

            int total = trackData.Count;
            DateTime firstPlay = trackData.Min(r => r.Date);
            DateTime lastPlay  = trackData.Max(r => r.Date);
            int daySpan = Math.Max(1, (lastPlay - firstPlay).Days + 1);
            double avgPerDay = Math.Round((double)total / daySpan, 2);

            TimeSpan totalDur = TimeSpan.Zero;
            int durCount = 0;
            foreach (var r in trackData)
            {
                if (TimeSpan.TryParse(r.PlayDuration, out var d))
                { totalDur += d; durCount++; }
            }
            TimeSpan avgDur = durCount > 0 ? TimeSpan.FromSeconds(totalDur.TotalSeconds / durCount) : TimeSpan.Zero;

            var byHour   = new double[24];
            var byWeekday = new double[7];
            foreach (var r in trackData)
            {
                if (TimeSpan.TryParse(r.StartTime, out var ts) && ts.Hours < 24) byHour[ts.Hours]++;
                int idx = ((int)r.Date.DayOfWeek + 6) % 7;
                byWeekday[idx]++;
            }

            var lv = BuildStatsListView();
            lv.Columns.Add(LanguageManager.GetString("MusicStatistics.Summary.Metric", "Metric"), 350);
            lv.Columns.Add(LanguageManager.GetString("MusicStatistics.Summary.Value", "Value"), 280);

            void AddRow(string metric, string value)
                => lv.Items.Add(new ListViewItem(new[] { metric, value }));

            AddRow(LanguageManager.GetString("MusicStatistics.SingleTrack.TotalPlays",   "Total plays"),          total.ToString());
            AddRow(LanguageManager.GetString("MusicStatistics.SingleTrack.FirstPlay",    "First play"),           firstPlay.ToString("dd/MM/yyyy"));
            AddRow(LanguageManager.GetString("MusicStatistics.SingleTrack.LastPlay",     "Last play"),            lastPlay.ToString("dd/MM/yyyy"));
            AddRow(LanguageManager.GetString("MusicStatistics.SingleTrack.AvgPerDay",    "Avg plays/day"),        avgPerDay.ToString());
            AddRow(LanguageManager.GetString("MusicStatistics.SingleTrack.TotalDuration","Total play duration"),  FormatTs(totalDur));
            AddRow(LanguageManager.GetString("MusicStatistics.SingleTrack.AvgDuration",  "Avg play duration"),    FormatTs(avgDur));

            lv.Items.Add(new ListViewItem(""));
            lv.Items.Add(new ListViewItem(new[]
            {
                LanguageManager.GetString("MusicStatistics.SingleTrack.HourlyDist", "Hourly distribution"), ""
            }));
            string[] hourLabels = Enumerable.Range(0, 24).Select(i => $"{i:D2}:00").ToArray();
            for (int i = 0; i < 24; i++)
            {
                if (byHour[i] > 0)
                    lv.Items.Add(new ListViewItem(new[] { $"  {hourLabels[i]}", ((int)byHour[i]).ToString() }));
            }

            lv.Items.Add(new ListViewItem(""));
            lv.Items.Add(new ListViewItem(new[]
            {
                LanguageManager.GetString("MusicStatistics.SingleTrack.WeekdayDist", "Day-of-week distribution"), ""
            }));
            string[] dayNames = {
                LanguageManager.GetString("MusicStatistics.Day.Mon", "Mon"),
                LanguageManager.GetString("MusicStatistics.Day.Tue", "Tue"),
                LanguageManager.GetString("MusicStatistics.Day.Wed", "Wed"),
                LanguageManager.GetString("MusicStatistics.Day.Thu", "Thu"),
                LanguageManager.GetString("MusicStatistics.Day.Fri", "Fri"),
                LanguageManager.GetString("MusicStatistics.Day.Sat", "Sat"),
                LanguageManager.GetString("MusicStatistics.Day.Sun", "Sun")
            };
            for (int i = 0; i < 7; i++)
            {
                if (byWeekday[i] > 0)
                    lv.Items.Add(new ListViewItem(new[] { $"  {dayNames[i]}", ((int)byWeekday[i]).ToString() }));
            }

            pnlSingleTrack.Controls.Add(lv);
        }

        // 10. Single Artist ──────────────────────────────────────────────────
        private void BuildSingleArtistPanel()
        {
            pnlSingleArtist.Controls.Clear();
            if (_data.Count == 0 || cmbSingleArtist.SelectedItem == null)
            {
                AddNoDataLabel(pnlSingleArtist);
                return;
            }

            string? selected = cmbSingleArtist.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selected)) { AddNoDataLabel(pnlSingleArtist); return; }
            var artistData = _data
                .Where(r => string.Equals(r.Artist, selected, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (artistData.Count == 0) { AddNoDataLabel(pnlSingleArtist); return; }

            int total = artistData.Count;
            int uniqueTracks = artistData.Select(r => r.Title).Distinct().Count();
            DateTime firstPlay = artistData.Min(r => r.Date);
            DateTime lastPlay  = artistData.Max(r => r.Date);
            int daySpan = Math.Max(1, (lastPlay - firstPlay).Days + 1);
            double avgPerDay = Math.Round((double)total / daySpan, 2);

            TimeSpan totalDur = TimeSpan.Zero;
            int durCount = 0;
            foreach (var r in artistData)
            {
                if (TimeSpan.TryParse(r.PlayDuration, out var d))
                { totalDur += d; durCount++; }
            }

            var topTrack = artistData
                .GroupBy(r => r.Title)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault();

            var byHour    = new double[24];
            var byWeekday = new double[7];
            foreach (var r in artistData)
            {
                if (TimeSpan.TryParse(r.StartTime, out var ts) && ts.Hours < 24) byHour[ts.Hours]++;
                int idx = ((int)r.Date.DayOfWeek + 6) % 7;
                byWeekday[idx]++;
            }

            var lv = BuildStatsListView();
            lv.Columns.Add(LanguageManager.GetString("MusicStatistics.Summary.Metric", "Metric"), 350);
            lv.Columns.Add(LanguageManager.GetString("MusicStatistics.Summary.Value", "Value"), 280);

            void AddRow(string metric, string value)
                => lv.Items.Add(new ListViewItem(new[] { metric, value }));

            AddRow(LanguageManager.GetString("MusicStatistics.SingleArtist.TotalPlays",    "Total plays"),         total.ToString());
            AddRow(LanguageManager.GetString("MusicStatistics.SingleArtist.UniqueTracks",  "Unique tracks"),       uniqueTracks.ToString());
            AddRow(LanguageManager.GetString("MusicStatistics.SingleArtist.FirstPlay",     "First play"),          firstPlay.ToString("dd/MM/yyyy"));
            AddRow(LanguageManager.GetString("MusicStatistics.SingleArtist.LastPlay",      "Last play"),           lastPlay.ToString("dd/MM/yyyy"));
            AddRow(LanguageManager.GetString("MusicStatistics.SingleArtist.AvgPerDay",     "Avg plays/day"),       avgPerDay.ToString());
            AddRow(LanguageManager.GetString("MusicStatistics.SingleArtist.TotalDuration", "Total play duration"), FormatTs(totalDur));
            AddRow(LanguageManager.GetString("MusicStatistics.SingleArtist.TopTrack",      "Most played track"),
                topTrack != null ? $"{topTrack.Key} ({topTrack.Count()}x)" : "—");

            lv.Items.Add(new ListViewItem(""));
            lv.Items.Add(new ListViewItem(new[]
            {
                LanguageManager.GetString("MusicStatistics.SingleArtist.TrackList", "Tracks played"), ""
            }));
            var trackList = artistData
                .GroupBy(r => r.Title)
                .OrderByDescending(g => g.Count())
                .ToList();
            foreach (var g in trackList)
                lv.Items.Add(new ListViewItem(new[] { $"  {g.Key}", $"{g.Count()}x" }));

            lv.Items.Add(new ListViewItem(""));
            lv.Items.Add(new ListViewItem(new[]
            {
                LanguageManager.GetString("MusicStatistics.SingleArtist.HourlyDist", "Hourly distribution"), ""
            }));
            string[] hourLabels = Enumerable.Range(0, 24).Select(i => $"{i:D2}:00").ToArray();
            for (int i = 0; i < 24; i++)
            {
                if (byHour[i] > 0)
                    lv.Items.Add(new ListViewItem(new[] { $"  {hourLabels[i]}", ((int)byHour[i]).ToString() }));
            }

            lv.Items.Add(new ListViewItem(""));
            lv.Items.Add(new ListViewItem(new[]
            {
                LanguageManager.GetString("MusicStatistics.SingleArtist.WeekdayDist", "Day-of-week distribution"), ""
            }));
            string[] dayNames = {
                LanguageManager.GetString("MusicStatistics.Day.Mon", "Mon"),
                LanguageManager.GetString("MusicStatistics.Day.Tue", "Tue"),
                LanguageManager.GetString("MusicStatistics.Day.Wed", "Wed"),
                LanguageManager.GetString("MusicStatistics.Day.Thu", "Thu"),
                LanguageManager.GetString("MusicStatistics.Day.Fri", "Fri"),
                LanguageManager.GetString("MusicStatistics.Day.Sat", "Sat"),
                LanguageManager.GetString("MusicStatistics.Day.Sun", "Sun")
            };
            for (int i = 0; i < 7; i++)
            {
                if (byWeekday[i] > 0)
                    lv.Items.Add(new ListViewItem(new[] { $"  {dayNames[i]}", ((int)byWeekday[i]).ToString() }));
            }

            pnlSingleArtist.Controls.Add(lv);
        }

        private static ListView BuildStatsListView() => new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            GridLines = true,
            BackColor = AppTheme.BgPanel,
            ForeColor = AppTheme.TextPrimary,
            Font = new Font("Segoe UI", 10),
            BorderStyle = BorderStyle.None
        };

        private void AddNoDataLabel(Panel target)
        {
            target.Controls.Add(new Label
            {
                Text = LanguageManager.GetString("MusicStatistics.NoData", "No data available for the selected period."),
                ForeColor = AppTheme.TextSecondary,
                Font = new Font("Segoe UI", 11),
                AutoSize = true,
                Location = new Point(20, 20)
            });
        }

        // ── Inner GDI+ Chart Panel ────────────────────────────────────────────

        private sealed class ChartPanel : Panel
        {
            private enum ChartKind { None, VerticalBars, HorizontalBars, LinePlot }

            private ChartKind _kind   = ChartKind.None;
            private double[]  _values = Array.Empty<double>();
            private double[]  _xs     = Array.Empty<double>();
            private string[]  _labels = Array.Empty<string>();
            private string    _title  = "";
            private string    _xLabel = "";
            private string    _yLabel = "";

            private static readonly Color BgColor = Color.FromArgb(30, 30, 30);
            private static readonly Color DataBg  = Color.FromArgb(40, 40, 40);
            private static readonly Color GridClr = Color.FromArgb(60, 60, 60);
            private static readonly Color BarClr  = Color.FromArgb(0, 150, 136);
            private static readonly Color TextClr = Color.FromArgb(200, 200, 200);
            private static readonly Color AxisClr = Color.FromArgb(140, 140, 140);

            public ChartPanel()
            {
                DoubleBuffered = true;
                BackColor = BgColor;
                Dock = DockStyle.Fill;
            }

            public void SetBarData(double[] values, string[] labels, bool horizontal,
                                   string title, string xLabel, string yLabel)
            {
                _kind   = horizontal ? ChartKind.HorizontalBars : ChartKind.VerticalBars;
                _values = values;
                _xs     = Enumerable.Range(0, values.Length).Select(i => (double)i).ToArray();
                _labels = labels;
                _title  = title;
                _xLabel = xLabel;
                _yLabel = yLabel;
                Invalidate();
            }

            public void SetScatterData(double[] xs, double[] ys, string[] xLabels,
                                       string title, string xLabel, string yLabel)
            {
                _kind   = ChartKind.LinePlot;
                _values = ys;
                _xs     = xs;
                _labels = xLabels;
                _title  = title;
                _xLabel = xLabel;
                _yLabel = yLabel;
                Invalidate();
            }

            public void ClearData()
            {
                _kind = ChartKind.None;
                Invalidate();
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(BgColor);

                int w = ClientSize.Width;
                int h = ClientSize.Height;
                if (w < 50 || h < 50) return;

                // Draw title
                using (var tf = new Font("Segoe UI", 11, FontStyle.Bold))
                {
                    var sz = g.MeasureString(_title, tf);
                    g.DrawString(_title, tf, new SolidBrush(Color.White), (w - sz.Width) / 2f, 6f);
                }

                if (_kind == ChartKind.None || _values.Length == 0)
                {
                    DrawNoData(g, w, h);
                    return;
                }

                switch (_kind)
                {
                    case ChartKind.VerticalBars:   DrawVerticalBars(g, w, h);   break;
                    case ChartKind.HorizontalBars: DrawHorizontalBars(g, w, h); break;
                    case ChartKind.LinePlot:       DrawLinePlot(g, w, h);       break;
                }
            }

            private void DrawNoData(Graphics g, int w, int h)
            {
                const string msg = "No data";
                using var f = new Font("Segoe UI", 11);
                var sz = g.MeasureString(msg, f);
                g.DrawString(msg, f, new SolidBrush(TextClr), (w - sz.Width) / 2f, (h - sz.Height) / 2f);
            }

            // ── Vertical Bars ────────────────────────────────────────────────
            private void DrawVerticalBars(Graphics g, int w, int h)
            {
                const int mTop = 40, mBottom = 70, mLeft = 65, mRight = 20;
                int pw = w - mLeft - mRight;
                int ph = h - mTop - mBottom;
                if (pw < 10 || ph < 10) return;

                g.FillRectangle(new SolidBrush(DataBg), mLeft, mTop, pw, ph);

                double maxV = _values.Length > 0 ? _values.Max() : 1;
                if (maxV <= 0) maxV = 1;

                using var gridPen = new Pen(GridClr);
                using var lf = new Font("Segoe UI", 7);
                for (int i = 0; i <= 5; i++)
                {
                    int y = mTop + ph - ph * i / 5;
                    g.DrawLine(gridPen, mLeft, y, mLeft + pw, y);
                    string lbl = SmartFormat(maxV * i / 5);
                    var ls = g.MeasureString(lbl, lf);
                    g.DrawString(lbl, lf, new SolidBrush(TextClr), mLeft - ls.Width - 3, y - ls.Height / 2f);
                }

                int n = _values.Length;
                if (n == 0) return;
                float barW = (float)pw / (n * 1.4f + 0.4f);
                float gap  = barW * 0.4f;

                using var barBrush = new SolidBrush(BarClr);
                for (int i = 0; i < n; i++)
                {
                    float bx = mLeft + i * (barW + gap) + gap;
                    float bh = (float)(ph * _values[i] / maxV);
                    float by = mTop + ph - bh;
                    g.FillRectangle(barBrush, bx, by, barW, bh);

                    if (i < _labels.Length)
                    {
                        string lbl = _labels[i].Length > 8 ? _labels[i].Substring(0, 7) + "…" : _labels[i];
                        var state = g.Save();
                        g.TranslateTransform(bx + barW / 2f, mTop + ph + 4f);
                        g.RotateTransform(-40);
                        g.DrawString(lbl, lf, new SolidBrush(TextClr), 0, 0);
                        g.Restore(state);
                    }
                }

                DrawAxes(g, mLeft, mTop, pw, ph, _xLabel, _yLabel, w, h);
            }

            // ── Horizontal Bars ──────────────────────────────────────────────
            private void DrawHorizontalBars(Graphics g, int w, int h)
            {
                const int mTop = 40, mBottom = 30, mRight = 55;
                int mLeft = Math.Min(220, w / 3);
                int pw = w - mLeft - mRight;
                int ph = h - mTop - mBottom;
                if (pw < 10 || ph < 10) return;

                g.FillRectangle(new SolidBrush(DataBg), mLeft, mTop, pw, ph);

                double maxV = _values.Length > 0 ? _values.Max() : 1;
                if (maxV <= 0) maxV = 1;

                using var gridPen = new Pen(GridClr);
                using var lf = new Font("Segoe UI", 7);
                for (int i = 0; i <= 5; i++)
                {
                    int x = mLeft + pw * i / 5;
                    g.DrawLine(gridPen, x, mTop, x, mTop + ph);
                    string lbl = ((int)(maxV * i / 5)).ToString();
                    var ls = g.MeasureString(lbl, lf);
                    g.DrawString(lbl, lf, new SolidBrush(TextClr), x - ls.Width / 2f, mTop + ph + 2f);
                }

                int n = _values.Length;
                if (n == 0) return;
                float barH  = (float)ph / (n + 1);
                float barPad = barH * 0.15f;

                using var barBrush = new SolidBrush(BarClr);
                using var catFont  = new Font("Segoe UI", 8);
                using var valFont  = new Font("Segoe UI", 7);

                for (int i = 0; i < n; i++)
                {
                    float by = mTop + i * barH + barPad;
                    float bh = barH - barPad * 2f;
                    float bw = (float)(pw * _values[i] / maxV);
                    g.FillRectangle(barBrush, mLeft, by, bw, bh);

                    if (i < _labels.Length)
                    {
                        string lbl = _labels[i];
                        var ls = g.MeasureString(lbl, catFont);
                        while (ls.Width > mLeft - 8 && lbl.Length > 4)
                        {
                            lbl = lbl.Substring(0, lbl.Length - 4) + "…";
                            ls  = g.MeasureString(lbl, catFont);
                        }
                        g.DrawString(lbl, catFont, new SolidBrush(TextClr),
                            mLeft - ls.Width - 4f, by + (bh - ls.Height) / 2f);
                    }

                    string vs = ((int)_values[i]).ToString();
                    var vs2 = g.MeasureString(vs, valFont);
                    g.DrawString(vs, valFont, new SolidBrush(TextClr),
                        mLeft + bw + 3f, by + (bh - vs2.Height) / 2f);
                }

                using var axisPen = new Pen(AxisClr);
                g.DrawLine(axisPen, mLeft, mTop, mLeft, mTop + ph);
                g.DrawLine(axisPen, mLeft, mTop + ph, mLeft + pw, mTop + ph);
            }

            // ── Line Plot ────────────────────────────────────────────────────
            private void DrawLinePlot(Graphics g, int w, int h)
            {
                const int mTop = 40, mBottom = 65, mLeft = 60, mRight = 20;
                int pw = w - mLeft - mRight;
                int ph = h - mTop - mBottom;
                if (pw < 10 || ph < 10) return;

                g.FillRectangle(new SolidBrush(DataBg), mLeft, mTop, pw, ph);

                double minX = _xs.Length > 0 ? _xs.Min() : 0;
                double maxX = _xs.Length > 0 ? _xs.Max() : 1;
                double maxY = _values.Length > 0 ? _values.Max() : 1;
                if (maxX == minX) maxX = minX + 1;
                if (maxY <= 0) maxY = 1;

                using var gridPen = new Pen(GridClr);
                using var lf = new Font("Segoe UI", 7);
                for (int i = 0; i <= 5; i++)
                {
                    int y = mTop + ph - ph * i / 5;
                    g.DrawLine(gridPen, mLeft, y, mLeft + pw, y);
                    string lbl = SmartFormat(maxY * i / 5);
                    var ls = g.MeasureString(lbl, lf);
                    g.DrawString(lbl, lf, new SolidBrush(TextClr), mLeft - ls.Width - 3, y - ls.Height / 2f);
                }

                if (_xs.Length >= 2)
                {
                    var pts = new PointF[_xs.Length];
                    for (int i = 0; i < _xs.Length; i++)
                    {
                        float px = mLeft + (float)((_xs[i] - minX) / (maxX - minX) * pw);
                        float py = mTop  + ph - (float)(_values[i] / maxY * ph);
                        pts[i] = new PointF(px, py);
                    }
                    using var lp = new Pen(BarClr, 2);
                    g.DrawLines(lp, pts);
                    using var dotBrush = new SolidBrush(BarClr);
                    foreach (var pt in pts)
                        g.FillEllipse(dotBrush, pt.X - 3, pt.Y - 3, 6, 6);
                }

                int step = Math.Max(1, _labels.Length / 12);
                for (int i = 0; i < _labels.Length; i += step)
                {
                    if (i >= _xs.Length) break;
                    float px = mLeft + (float)((_xs[i] - minX) / (maxX - minX) * pw);
                    var state = g.Save();
                    g.TranslateTransform(px, mTop + ph + 4f);
                    g.RotateTransform(-45);
                    g.DrawString(_labels[i], lf, new SolidBrush(TextClr), 0, 0);
                    g.Restore(state);
                }

                DrawAxes(g, mLeft, mTop, pw, ph, _xLabel, _yLabel, w, h);
            }

            // ── Shared helpers ───────────────────────────────────────────────
            private static void DrawAxes(Graphics g, int mLeft, int mTop, int pw, int ph,
                                         string xLabel, string yLabel, int w, int h)
            {
                using var ap = new Pen(AxisClr);
                g.DrawLine(ap, mLeft, mTop, mLeft, mTop + ph);
                g.DrawLine(ap, mLeft, mTop + ph, mLeft + pw, mTop + ph);

                using var af = new Font("Segoe UI", 9);
                if (!string.IsNullOrEmpty(xLabel))
                {
                    var sz = g.MeasureString(xLabel, af);
                    g.DrawString(xLabel, af, new SolidBrush(TextClr),
                        mLeft + (pw - sz.Width) / 2f, h - af.Height - 2f);
                }
                if (!string.IsNullOrEmpty(yLabel))
                {
                    var state = g.Save();
                    g.TranslateTransform(12f, mTop + ph / 2f);
                    g.RotateTransform(-90);
                    var sz = g.MeasureString(yLabel, af);
                    g.DrawString(yLabel, af, new SolidBrush(TextClr), -sz.Width / 2f, -af.Height / 2f);
                    g.Restore(state);
                }
            }

            private static string SmartFormat(double v)
            {
                if (v >= 3600) return $"{v / 3600:F1}h";
                if (v >= 60)   return $"{v / 60:F0}m";
                return $"{(int)v}";
            }
        }
    }
}
