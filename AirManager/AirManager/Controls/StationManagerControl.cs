using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using AirManager.Services;
using AirManager.Forms;
using AirManager.Themes;

namespace AirManager.Controls
{
    public partial class StationManagerControl : UserControl
    {
        private Panel headerPanel;
        private Label lblTitle;
        private Button btnNewStation;
        private Button btnRefresh;
        private Panel spacerPanel;
        private FlowLayoutPanel flowPanel;

        public event EventHandler<StationConfig> StationChanged;

        private const int HEADER_HEIGHT = 80;
        private const int SPACER_HEIGHT = 15;

        public StationManagerControl()
        {
            InitializeComponent();
            InitializeCustomUI();
            ApplyLanguage(); // ✅ APPLICA LINGUA INIZIALE
            RefreshStations();

            // ✅ SOTTOSCRIVI EVENTO CAMBIO LINGUA
            LanguageManager.LanguageChanged += (s, e) => ApplyLanguage();
        }

        private void InitializeCustomUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = AppTheme.BgLight;
            this.Padding = new Padding(0);

            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = HEADER_HEIGHT,
                BackColor = AppTheme.BgDark,
                Padding = new Padding(15, 10, 15, 10)
            };

            lblTitle = new Label
            {
                Text = "📻 GESTIONE EMITTENTI",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(15, 12),
                AutoSize = true
            };
            headerPanel.Controls.Add(lblTitle);

            btnNewStation = new Button
            {
                Text = "➕ Nuova Emittente",
                Size = new Size(160, 40),
                BackColor = Color.FromArgb(0, 150, 136),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnNewStation.FlatAppearance.BorderSize = 0;
            btnNewStation.Click += BtnNewStation_Click;
            headerPanel.Controls.Add(btnNewStation);

            btnRefresh = new Button
            {
                Text = "🔄",
                Size = new Size(40, 40),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 14),
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (s, e) => RefreshStations();
            headerPanel.Controls.Add(btnRefresh);

            headerPanel.Resize += (s, e) => RepositionHeaderButtons();
            RepositionHeaderButtons();

            this.Controls.Add(headerPanel);

            spacerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = SPACER_HEIGHT,
                BackColor = AppTheme.BgLight
            };
            this.Controls.Add(spacerPanel);

            flowPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = AppTheme.BgLight,
                AutoScroll = true,
                Padding = new Padding(20),
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight
            };
            this.Controls.Add(flowPanel);

            this.Controls.SetChildIndex(flowPanel, 0);
            this.Controls.SetChildIndex(spacerPanel, 1);
            this.Controls.SetChildIndex(headerPanel, 2);
        }

        /// <summary>
        /// ✅ APPLICA LE TRADUZIONI DA LanguageManager
        /// </summary>
        private void ApplyLanguage()
        {
            lblTitle.Text = "📻 " + LanguageManager.GetString("StationManager.HeaderTitle");
            btnNewStation.Text = "➕ " + LanguageManager.GetString("StationManager.NewStation");

            // ✅ RICARICA LE CARD PER AGGIORNARE I TESTI
            RefreshStations();

            Console.WriteLine($"[StationManager] ✅ Lingua applicata: {LanguageManager.CurrentLanguage}");
        }

        private void RepositionHeaderButtons()
        {
            const int MARGIN = 15;
            int panelWidth = headerPanel.Width;

            int x = panelWidth - btnRefresh.Width - MARGIN;
            btnRefresh.Location = new Point(x, 20);

            x -= (btnNewStation.Width + 10);
            btnNewStation.Location = new Point(x, 20);
        }

        public void RefreshStations()
        {
            flowPanel.Controls.Clear();

            var stations = StationRegistry.LoadAllStations();

            if (stations.Count == 0)
            {
                ShowEmptyState();
                return;
            }

            string activeStationId = StationRegistry.GetActiveStationId();

            foreach (var station in stations)
            {
                bool isActive = station.Id == activeStationId;
                var card = CreateStationCard(station, isActive);
                flowPanel.Controls.Add(card);
            }
        }

        private void ShowEmptyState()
        {
            Panel emptyPanel = new Panel
            {
                Size = new Size(400, 200),
                BackColor = Color.FromArgb(40, 40, 40),
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblEmpty = new Label
            {
                Text = LanguageManager.GetString("StationManager.EmptyState"),
                Font = new Font("Segoe UI", 12, FontStyle.Regular),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
            emptyPanel.Controls.Add(lblEmpty);

            flowPanel.Controls.Add(emptyPanel);
        }

        private Panel CreateStationCard(StationConfig station, bool isActive)
        {
            Panel card = new Panel
            {
                Size = new Size(320, 240),
                BackColor = Color.FromArgb(40, 40, 40),
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(10),
                Cursor = Cursors.Hand,
                Tag = station
            };

            Panel accentBar = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(320, 5),
                BackColor = isActive ? Color.FromArgb(0, 255, 0) : Color.FromArgb(0, 150, 136)
            };
            card.Controls.Add(accentBar);

            PictureBox picLogo = new PictureBox
            {
                Location = new Point(20, 20),
                Size = new Size(120, 70),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent
            };

            if (!string.IsNullOrEmpty(station.LogoPath) && File.Exists(station.LogoPath))
            {
                try
                {
                    picLogo.Image = Image.FromFile(station.LogoPath);
                }
                catch
                {
                    picLogo.Image = null;
                }
            }
            else
            {
                picLogo.BackColor = Color.FromArgb(60, 60, 60);
                Label lblNoLogo = new Label
                {
                    Text = "📻",
                    Font = new Font("Segoe UI", 32),
                    ForeColor = Color.Gray,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                picLogo.Controls.Add(lblNoLogo);
            }
            card.Controls.Add(picLogo);

            Label lblName = new Label
            {
                Text = station.Name + (isActive ? " ✓" : ""),
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = isActive ? Color.FromArgb(0, 255, 0) : Color.White,
                Location = new Point(20, 105),
                Size = new Size(280, 30),
                AutoEllipsis = true
            };
            card.Controls.Add(lblName);

            Label lblPath = new Label
            {
                Text = $"📂 {TruncatePath(station.DatabasePath, 35)}",
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = Color.FromArgb(180, 180, 180),
                Location = new Point(20, 140),
                Size = new Size(280, 20),
                AutoEllipsis = true
            };
            card.Controls.Add(lblPath);

            Label lblLastAccess = new Label
            {
                Text = $"🕐 {FormatLastAccess(station.LastAccessed)}",
                Font = new Font("Segoe UI", 8, FontStyle.Regular),
                ForeColor = Color.Gray,
                Location = new Point(20, 165),
                Size = new Size(280, 18)
            };
            card.Controls.Add(lblLastAccess);

            // ✅ BOTTONI CON TRADUZIONI
            Button btnSelect = new Button
            {
                Text = isActive
                    ? "✓ " + LanguageManager.GetString("StationManager.Card.Active")
                    : "✓ " + LanguageManager.GetString("StationManager.Card.Select"),
                Location = new Point(20, 195),
                Size = new Size(100, 30),
                BackColor = isActive ? Color.FromArgb(0, 255, 0) : Color.FromArgb(0, 150, 136),
                ForeColor = isActive ? Color.Black : Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Enabled = !isActive
            };
            btnSelect.FlatAppearance.BorderSize = 0;
            btnSelect.Click += (s, e) =>
            {
                e = new MouseEventArgs(MouseButtons.None, 0, 0, 0, 0);
                SelectStation(station);
            };
            card.Controls.Add(btnSelect);

            Button btnEdit = new Button
            {
                Text = "✏️",
                Location = new Point(130, 195),
                Size = new Size(40, 30),
                BackColor = Color.FromArgb(255, 140, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12),
                Cursor = Cursors.Hand
            };
            btnEdit.FlatAppearance.BorderSize = 0;
            btnEdit.Click += (s, e) =>
            {
                e = new MouseEventArgs(MouseButtons.None, 0, 0, 0, 0);
                EditStation(station);
            };
            card.Controls.Add(btnEdit);

            Button btnDelete = new Button
            {
                Text = "🗑️",
                Location = new Point(180, 195),
                Size = new Size(40, 30),
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12),
                Cursor = Cursors.Hand
            };
            btnDelete.FlatAppearance.BorderSize = 0;
            btnDelete.Click += (s, e) =>
            {
                e = new MouseEventArgs(MouseButtons.None, 0, 0, 0, 0);
                DeleteStation(station);
            };
            card.Controls.Add(btnDelete);

            Panel statusIndicator = new Panel
            {
                Location = new Point(290, 15),
                Size = new Size(15, 15),
                BackColor = isActive
                    ? Color.FromArgb(0, 255, 0)
                    : (StationRegistry.ValidateDatabasePath(station.DatabasePath)
                        ? Color.FromArgb(255, 140, 0)
                        : Color.FromArgb(255, 0, 0))
            };

            ToolTip tooltip = new ToolTip();
            string tooltipText = isActive
                ? LanguageManager.GetString("StationManager.Tooltip.Active")
                : (StationRegistry.ValidateDatabasePath(station.DatabasePath)
                    ? LanguageManager.GetString("StationManager.Tooltip.Accessible")
                    : LanguageManager.GetString("StationManager.Tooltip.NotAccessible"));
            tooltip.SetToolTip(statusIndicator, tooltipText);

            card.Controls.Add(statusIndicator);

            if (!isActive)
            {
                card.Click += (s, e) => SelectStation(station);
                lblName.Click += (s, e) => SelectStation(station);
                lblPath.Click += (s, e) => SelectStation(station);
            }

            return card;
        }

        private string TruncatePath(string path, int maxLength)
        {
            if (string.IsNullOrEmpty(path) || path.Length <= maxLength)
                return path;

            return "..." + path.Substring(path.Length - maxLength + 3);
        }

        private string FormatLastAccess(DateTime lastAccess)
        {
            TimeSpan diff = DateTime.Now - lastAccess;

            if (diff.TotalMinutes < 1)
                return LanguageManager.GetString("StationManager.Time.Now");
            if (diff.TotalMinutes < 60)
                return string.Format(LanguageManager.GetString("StationManager.Time.MinutesAgo"), (int)diff.TotalMinutes);
            if (diff.TotalHours < 24)
                return string.Format(LanguageManager.GetString("StationManager.Time.HoursAgo"), (int)diff.TotalHours);
            if (diff.TotalDays < 7)
                return string.Format(LanguageManager.GetString("StationManager.Time.DaysAgo"), (int)diff.TotalDays);

            return lastAccess.ToString("dd/MM/yyyy");
        }

        private void SelectStation(StationConfig station)
        {
            try
            {
                if (!StationRegistry.ValidateDatabasePath(station.DatabasePath))
                {
                    var result = MessageBox.Show(
                        string.Format(LanguageManager.GetString("StationManager.Message.DatabaseNotAccessible"), station.Name, station.DatabasePath),
                        LanguageManager.GetString("StationManager.Title.DatabaseNotAccessible"),
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (result == DialogResult.No)
                        return;
                }

                StationChanged?.Invoke(this, station);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(LanguageManager.GetString("StationManager.Error.SelectStation"), ex.Message),
                    LanguageManager.GetString("Common.Error"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void EditStation(StationConfig station)
        {
            using (var form = new StationConfigForm(station))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    RefreshStations();
                }
            }
        }

        private void DeleteStation(StationConfig station)
        {
            var result = MessageBox.Show(
                string.Format(LanguageManager.GetString("StationManager.Message.ConfirmDelete"), station.Name),
                LanguageManager.GetString("StationManager.Title.ConfirmDelete"),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                if (StationRegistry.DeleteStation(station.Id))
                {
                    MessageBox.Show(
                        string.Format(LanguageManager.GetString("StationManager.Message.DeleteSuccess"), station.Name),
                        LanguageManager.GetString("StationManager.Title.DeleteSuccess"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    RefreshStations();
                }
                else
                {
                    MessageBox.Show(
                        LanguageManager.GetString("StationManager.Error.DeleteFailed"),
                        LanguageManager.GetString("Common.Error"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        private void BtnNewStation_Click(object sender, EventArgs e)
        {
            using (var form = new StationConfigForm(null))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    RefreshStations();
                }
            }
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