using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using AirManager.Services;
using AirManager.Services.Database;
using AirManager.Themes;
using ScottPlot;
using ScottPlot.WinForms;

namespace AirManager.Forms
{
    public partial class MusicStatisticsForm : Form
    {
        private DateTimePicker dtpFrom;
        private DateTimePicker dtpTo;
        private Button btnUpdate;
        private TabControl tabControl;

        private FormsPlot chartTopTracks;
        private FormsPlot chartTopArtists;
        private FormsPlot chartDailyTrend;
        private FormsPlot chartHourlyDist;
        private FormsPlot chartWeekdayDist;
        private FormsPlot chartAvgDuration;
        private Panel pnlRotation;
        private Panel pnlSummary;

        private List<ReportEntry> _data = new List<ReportEntry>();

        public MusicStatisticsForm()
        {
            InitializeComponent();
            InitializeCustomUI();
            ApplyLanguage();
            LanguageManager.LanguageChanged += OnLanguageChanged;
        }

        private void OnLanguageChanged(object sender, EventArgs e) => ApplyLanguage();

        private void ApplyLanguage()
        {
            this.Text = "📊 " + LanguageManager.GetString("MusicStatistics.Title", "Music Statistics");
            btnUpdate.Text = "🔄 " + LanguageManager.GetString("MusicStatistics.Update", "Update");

            if (tabControl.TabPages.Count >= 8)
            {
                tabControl.TabPages[0].Text = LanguageManager.GetString("MusicStatistics.Tab.TopTracks", "Top Tracks");
                tabControl.TabPages[1].Text = LanguageManager.GetString("MusicStatistics.Tab.TopArtists", "Top Artists");
                tabControl.TabPages[2].Text = LanguageManager.GetString("MusicStatistics.Tab.DailyTrend", "Daily Trend");
                tabControl.TabPages[3].Text = LanguageManager.GetString("MusicStatistics.Tab.HourlyDist", "Hourly Distribution");
                tabControl.TabPages[4].Text = LanguageManager.GetString("MusicStatistics.Tab.WeekdayDist", "Weekday Distribution");
                tabControl.TabPages[5].Text = LanguageManager.GetString("MusicStatistics.Tab.AvgDuration", "Avg Duration/Hour");
                tabControl.TabPages[6].Text = LanguageManager.GetString("MusicStatistics.Tab.Rotation", "Rotation Index");
                tabControl.TabPages[7].Text = LanguageManager.GetString("MusicStatistics.Tab.Summary", "Summary");
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
                Padding = new Padding(8, 4, 8, 4)
            };

            chartTopTracks  = CreateFormsPlot();
            chartTopArtists = CreateFormsPlot();
            chartDailyTrend = CreateFormsPlot();
            chartHourlyDist = CreateFormsPlot();
            chartWeekdayDist = CreateFormsPlot();
            chartAvgDuration = CreateFormsPlot();

            pnlRotation = CreateDarkPanel();
            pnlSummary  = CreateDarkPanel();

            tabControl.TabPages.Add(CreateChartTab("📊 Top Tracks",           chartTopTracks));
            tabControl.TabPages.Add(CreateChartTab("🎤 Top Artists",          chartTopArtists));
            tabControl.TabPages.Add(CreateChartTab("📈 Daily Trend",          chartDailyTrend));
            tabControl.TabPages.Add(CreateChartTab("🕐 Hourly Distribution",  chartHourlyDist));
            tabControl.TabPages.Add(CreateChartTab("📅 Weekday Distribution", chartWeekdayDist));
            tabControl.TabPages.Add(CreateChartTab("⏱️ Avg Duration/Hour",    chartAvgDuration));
            tabControl.TabPages.Add(CreatePanelTab("🔄 Rotation Index",       pnlRotation));
            tabControl.TabPages.Add(CreatePanelTab("📋 Summary",              pnlSummary));

            // correct dock order: header (Top) added after tabControl (Fill)
            this.Controls.Add(tabControl);
            this.Controls.Add(pnlHeader);
        }

        // ── Helpers ─────────────────────────────────────────────────────────

        private static FormsPlot CreateFormsPlot()
        {
            return new FormsPlot { Dock = DockStyle.Fill };
        }

        private static Panel CreateDarkPanel()
        {
            return new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppTheme.BgDark,
                AutoScroll = true
            };
        }

        private static TabPage CreateChartTab(string title, FormsPlot chart)
        {
            var page = new TabPage(title) { BackColor = AppTheme.BgDark };
            page.Controls.Add(chart);
            return page;
        }

        private static TabPage CreatePanelTab(string title, Panel panel)
        {
            var page = new TabPage(title) { BackColor = AppTheme.BgDark };
            page.Controls.Add(panel);
            return page;
        }

        // ── Data loading & chart building ────────────────────────────────────

        private void BtnUpdate_Click(object sender, EventArgs e)
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
        }

        // 1. Top N Tracks ────────────────────────────────────────────────────
        private void BuildTopTracksChart()
        {
            chartTopTracks.Plot.Clear();
            if (_data.Count == 0) { chartTopTracks.Refresh(); return; }

            var top = _data
                .GroupBy(r => $"{r.Artist} – {r.Title}")
                .OrderByDescending(g => g.Count())
                .Take(20)
                .Reverse()
                .ToList();

            double[] values = top.Select(g => (double)g.Count()).ToArray();
            string[] labels = top.Select(g => g.Key.Length > 40 ? g.Key.Substring(0, 40) + "…" : g.Key).ToArray();

            var bar = chartTopTracks.Plot.Add.Bars(values);
            bar.Horizontal = true;
            chartTopTracks.Plot.Axes.Left.SetTicks(
                Enumerable.Range(0, labels.Length).Select(i => (double)i).ToArray(),
                labels);

            ApplyPlotTheme(chartTopTracks.Plot,
                LanguageManager.GetString("MusicStatistics.Chart.TopTracks", "Top 20 Most Played Tracks"),
                LanguageManager.GetString("MusicStatistics.Chart.Plays", "Plays"),
                "");
            chartTopTracks.Refresh();
        }

        // 2. Top N Artists ───────────────────────────────────────────────────
        private void BuildTopArtistsChart()
        {
            chartTopArtists.Plot.Clear();
            if (_data.Count == 0) { chartTopArtists.Refresh(); return; }

            var top = _data
                .Where(r => !string.IsNullOrWhiteSpace(r.Artist))
                .GroupBy(r => r.Artist)
                .OrderByDescending(g => g.Count())
                .Take(20)
                .Reverse()
                .ToList();

            double[] values = top.Select(g => (double)g.Count()).ToArray();
            string[] labels = top.Select(g => g.Key.Length > 30 ? g.Key.Substring(0, 30) + "…" : g.Key).ToArray();

            var bar = chartTopArtists.Plot.Add.Bars(values);
            bar.Horizontal = true;
            chartTopArtists.Plot.Axes.Left.SetTicks(
                Enumerable.Range(0, labels.Length).Select(i => (double)i).ToArray(),
                labels);

            ApplyPlotTheme(chartTopArtists.Plot,
                LanguageManager.GetString("MusicStatistics.Chart.TopArtists", "Top 20 Most Played Artists"),
                LanguageManager.GetString("MusicStatistics.Chart.Plays", "Plays"),
                "");
            chartTopArtists.Refresh();
        }

        // 3. Daily Trend ─────────────────────────────────────────────────────
        private void BuildDailyTrendChart()
        {
            chartDailyTrend.Plot.Clear();
            if (_data.Count == 0) { chartDailyTrend.Refresh(); return; }

            var byDay = _data
                .GroupBy(r => r.Date.Date)
                .OrderBy(g => g.Key)
                .ToList();

            double[] xs = byDay.Select(g => g.Key.ToOADate()).ToArray();
            double[] ys = byDay.Select(g => (double)g.Count()).ToArray();

            var scatter = chartDailyTrend.Plot.Add.Scatter(xs, ys);
            scatter.LineWidth = 2;
            chartDailyTrend.Plot.Axes.DateTimeTicksBottom();

            ApplyPlotTheme(chartDailyTrend.Plot,
                LanguageManager.GetString("MusicStatistics.Chart.DailyTrend", "Daily Plays Trend"),
                "",
                LanguageManager.GetString("MusicStatistics.Chart.Plays", "Plays"));
            chartDailyTrend.Refresh();
        }

        // 4. Hourly Distribution ─────────────────────────────────────────────
        private void BuildHourlyDistChart()
        {
            chartHourlyDist.Plot.Clear();
            if (_data.Count == 0) { chartHourlyDist.Refresh(); return; }

            var byHour = new double[24];
            foreach (var r in _data)
            {
                if (TimeSpan.TryParse(r.StartTime, out var ts))
                    byHour[ts.Hours]++;
            }

            double[] xs = Enumerable.Range(0, 24).Select(i => (double)i).ToArray();
            chartHourlyDist.Plot.Add.Bars(xs, byHour);

            string[] hourLabels = Enumerable.Range(0, 24).Select(i => $"{i:D2}:00").ToArray();
            chartHourlyDist.Plot.Axes.Bottom.SetTicks(xs, hourLabels);

            ApplyPlotTheme(chartHourlyDist.Plot,
                LanguageManager.GetString("MusicStatistics.Chart.HourlyDist", "Plays by Hour of Day"),
                LanguageManager.GetString("MusicStatistics.Chart.Hour", "Hour"),
                LanguageManager.GetString("MusicStatistics.Chart.Plays", "Plays"));
            chartHourlyDist.Refresh();
        }

        // 5. Weekday Distribution ────────────────────────────────────────────
        private void BuildWeekdayDistChart()
        {
            chartWeekdayDist.Plot.Clear();
            if (_data.Count == 0) { chartWeekdayDist.Refresh(); return; }

            var byDay = new double[7];
            foreach (var r in _data)
            {
                int idx = ((int)r.Date.DayOfWeek + 6) % 7; // Mon=0..Sun=6
                byDay[idx]++;
            }

            double[] xs = Enumerable.Range(0, 7).Select(i => (double)i).ToArray();
            chartWeekdayDist.Plot.Add.Bars(xs, byDay);

            string[] dayLabels = {
                LanguageManager.GetString("MusicStatistics.Day.Mon", "Mon"),
                LanguageManager.GetString("MusicStatistics.Day.Tue", "Tue"),
                LanguageManager.GetString("MusicStatistics.Day.Wed", "Wed"),
                LanguageManager.GetString("MusicStatistics.Day.Thu", "Thu"),
                LanguageManager.GetString("MusicStatistics.Day.Fri", "Fri"),
                LanguageManager.GetString("MusicStatistics.Day.Sat", "Sat"),
                LanguageManager.GetString("MusicStatistics.Day.Sun", "Sun")
            };
            chartWeekdayDist.Plot.Axes.Bottom.SetTicks(xs, dayLabels);

            ApplyPlotTheme(chartWeekdayDist.Plot,
                LanguageManager.GetString("MusicStatistics.Chart.WeekdayDist", "Plays by Day of Week"),
                "",
                LanguageManager.GetString("MusicStatistics.Chart.Plays", "Plays"));
            chartWeekdayDist.Refresh();
        }

        // 6. Avg Duration per Hour ───────────────────────────────────────────
        private void BuildAvgDurationChart()
        {
            chartAvgDuration.Plot.Clear();
            if (_data.Count == 0) { chartAvgDuration.Refresh(); return; }

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

            double[] xs = Enumerable.Range(0, 24).Select(i => (double)i).ToArray();
            double[] ys = Enumerable.Range(0, 24)
                .Select(i => counts[i] > 0 ? totalSec[i] / counts[i] : 0)
                .ToArray();

            chartAvgDuration.Plot.Add.Bars(xs, ys);
            string[] hourLabels = Enumerable.Range(0, 24).Select(i => $"{i:D2}:00").ToArray();
            chartAvgDuration.Plot.Axes.Bottom.SetTicks(xs, hourLabels);

            ApplyPlotTheme(chartAvgDuration.Plot,
                LanguageManager.GetString("MusicStatistics.Chart.AvgDuration", "Average Play Duration by Hour (seconds)"),
                LanguageManager.GetString("MusicStatistics.Chart.Hour", "Hour"),
                LanguageManager.GetString("MusicStatistics.Chart.AvgSec", "Avg seconds"));
            chartAvgDuration.Refresh();
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

        private static void ApplyPlotTheme(Plot plot, string title, string xLabel, string yLabel)
        {
            plot.Title(title);
            if (!string.IsNullOrEmpty(xLabel)) plot.XLabel(xLabel);
            if (!string.IsNullOrEmpty(yLabel)) plot.YLabel(yLabel);
            plot.Style.Background(
                figure: System.Drawing.Color.FromArgb(30, 30, 30),
                data:   System.Drawing.Color.FromArgb(40, 40, 40));
            plot.Style.ColorAxes(System.Drawing.Color.FromArgb(180, 180, 180));
            plot.Style.ColorGrids(System.Drawing.Color.FromArgb(60, 60, 60));
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
