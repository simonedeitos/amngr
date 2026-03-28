using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using AirManager.Services;
using AirManager.Themes;

namespace AirManager.Controls
{
    public partial class StationSelectorControl : UserControl
    {
        private Label lblTitle;
        private Label lblSubtitle;
        private FlowLayoutPanel flowPanel;
        private Button btnManageStations;

        public event EventHandler<StationConfig> StationSelected;

        public StationSelectorControl()
        {
            InitializeComponent();
            InitializeCustomUI();
            LoadStations();
        }

        private void InitializeCustomUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = AppTheme.BgLight;
            this.Padding = new Padding(0);

            // ✅ HEADER
            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 150,
                BackColor = AppTheme.BgDark
            };

            lblTitle = new Label
            {
                Text = "📻 SELEZIONA EMITTENTE",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true
            };
            headerPanel.Controls.Add(lblTitle);

            lblSubtitle = new Label
            {
                Text = "Scegli quale emittente aprire",
                Font = new Font("Segoe UI", 12, FontStyle.Regular),
                ForeColor = Color.FromArgb(180, 180, 180),
                AutoSize = true
            };
            headerPanel.Controls.Add(lblSubtitle);

            btnManageStations = new Button
            {
                Text = "⚙️ Gestisci Emittenti",
                Size = new Size(180, 45),
                BackColor = Color.FromArgb(138, 43, 226),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnManageStations.FlatAppearance.BorderSize = 0;
            btnManageStations.Click += BtnManageStations_Click;
            headerPanel.Controls.Add(btnManageStations);

            headerPanel.Resize += (s, e) => RepositionHeaderControls(headerPanel);
            RepositionHeaderControls(headerPanel);

            this.Controls.Add(headerPanel);

            // ✅ FLOW PANEL PER CARDS
            flowPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = AppTheme.BgLight,
                AutoScroll = true,
                Padding = new Padding(40),
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight
            };
            this.Controls.Add(flowPanel);
        }

        private void RepositionHeaderControls(Panel headerPanel)
        {
            int centerX = headerPanel.Width / 2;

            lblTitle.Location = new Point(centerX - lblTitle.Width / 2, 30);
            lblSubtitle.Location = new Point(centerX - lblSubtitle.Width / 2, 65);
            btnManageStations.Location = new Point(centerX - btnManageStations.Width / 2, 100);
        }

        public void LoadStations()
        {
            flowPanel.Controls.Clear();

            var stations = StationRegistry.LoadAllStations();

            if (stations.Count == 0)
            {
                ShowEmptyState();
                return;
            }

            // ✅ ORDINA PER ULTIMO ACCESSO
            stations = stations.OrderByDescending(s => s.LastAccessed).ToList();

            foreach (var station in stations)
            {
                var card = CreateStationCard(station);
                flowPanel.Controls.Add(card);
            }
        }

        private void ShowEmptyState()
        {
            Panel emptyPanel = new Panel
            {
                Size = new Size(500, 250),
                BackColor = Color.FromArgb(40, 40, 40),
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblEmpty = new Label
            {
                Text = "📻\n\nNessuna emittente configurata\n\nClicca '⚙️ Gestisci Emittenti' per configurare\nla tua prima emittente",
                Font = new Font("Segoe UI", 13, FontStyle.Regular),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
            emptyPanel.Controls.Add(lblEmpty);

            // ✅ CENTRA IL PANEL
            emptyPanel.Location = new Point(
                (flowPanel.Width - emptyPanel.Width) / 2,
                (flowPanel.Height - emptyPanel.Height) / 2
            );

            flowPanel.Controls.Add(emptyPanel);
        }

        private Panel CreateStationCard(StationConfig station)
        {
            // ✅ CARD PIÙ GRANDE PER SELEZIONE
            Panel card = new Panel
            {
                Size = new Size(380, 280),
                BackColor = Color.FromArgb(40, 40, 40),
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(15),
                Cursor = Cursors.Hand,
                Tag = station
            };

            // ✅ ACCENT BAR (più spessa)
            Panel accentBar = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(380, 8),
                BackColor = Color.FromArgb(0, 150, 136)
            };
            card.Controls.Add(accentBar);

            // ✅ LOGO (più grande)
            PictureBox picLogo = new PictureBox
            {
                Location = new Point(130, 25),
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
                // ✅ PLACEHOLDER
                picLogo.BackColor = Color.FromArgb(60, 60, 60);
                Label lblNoLogo = new Label
                {
                    Text = "📻",
                    Font = new Font("Segoe UI", 36),
                    ForeColor = Color.Gray,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                picLogo.Controls.Add(lblNoLogo);
            }
            card.Controls.Add(picLogo);

            // ✅ NOME EMITTENTE (centrato)
            Label lblName = new Label
            {
                Text = station.Name,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 115),
                Size = new Size(340, 35),
                TextAlign = ContentAlignment.MiddleCenter,
                AutoEllipsis = true
            };
            card.Controls.Add(lblName);

            // ✅ DATABASE PATH
            Label lblPath = new Label
            {
                Text = $"📂 {TruncatePath(station.DatabasePath, 40)}",
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = Color.FromArgb(180, 180, 180),
                Location = new Point(20, 155),
                Size = new Size(340, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                AutoEllipsis = true
            };
            card.Controls.Add(lblPath);

            // ✅ ULTIMO ACCESSO
            Label lblLastAccess = new Label
            {
                Text = $"🕐 Ultimo accesso: {FormatLastAccess(station.LastAccessed)}",
                Font = new Font("Segoe UI", 8, FontStyle.Regular),
                ForeColor = Color.Gray,
                Location = new Point(20, 180),
                Size = new Size(340, 18),
                TextAlign = ContentAlignment.MiddleCenter
            };
            card.Controls.Add(lblLastAccess);

            // ✅ STATUS INDICATOR (più grande)
            Panel statusIndicator = new Panel
            {
                Location = new Point(10, 15),
                Size = new Size(20, 20),
                BackColor = StationRegistry.ValidateDatabasePath(station.DatabasePath)
                    ? Color.FromArgb(0, 255, 0)
                    : Color.FromArgb(255, 0, 0)
            };

            ToolTip tooltip = new ToolTip();
            tooltip.SetToolTip(statusIndicator,
                StationRegistry.ValidateDatabasePath(station.DatabasePath)
                    ? "✅ Database accessibile"
                    : "❌ Database non accessibile");

            card.Controls.Add(statusIndicator);

            // ✅ BOTTONE SELEZIONA (grande e centrato)
            Button btnSelect = new Button
            {
                Text = "✓ APRI QUESTA EMITTENTE",
                Location = new Point(60, 220),
                Size = new Size(260, 45),
                BackColor = Color.FromArgb(0, 150, 136),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSelect.FlatAppearance.BorderSize = 0;
            btnSelect.Click += (s, e) =>
            {
                SelectStation(station);
            };
            card.Controls.Add(btnSelect);

            // ✅ HOVER EFFECTS
            card.MouseEnter += (s, e) =>
            {
                card.BackColor = Color.FromArgb(50, 50, 50);
                accentBar.BackColor = Color.FromArgb(0, 180, 166);
            };

            card.MouseLeave += (s, e) =>
            {
                card.BackColor = Color.FromArgb(40, 40, 40);
                accentBar.BackColor = Color.FromArgb(0, 150, 136);
            };

            // ✅ CLICK SU CARD → SELEZIONA
            card.Click += (s, e) => SelectStation(station);
            lblName.Click += (s, e) => SelectStation(station);
            lblPath.Click += (s, e) => SelectStation(station);
            picLogo.Click += (s, e) => SelectStation(station);

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
                return "adesso";
            if (diff.TotalMinutes < 60)
                return $"{(int)diff.TotalMinutes} minuti fa";
            if (diff.TotalHours < 24)
                return $"{(int)diff.TotalHours} ore fa";
            if (diff.TotalDays < 7)
                return $"{(int)diff.TotalDays} giorni fa";

            return lastAccess.ToString("dd/MM/yyyy HH:mm");
        }

        private void SelectStation(StationConfig station)
        {
            try
            {
                // ✅ VERIFICA ACCESSIBILITÀ
                if (!StationRegistry.ValidateDatabasePath(station.DatabasePath))
                {
                    var result = MessageBox.Show(
                        $"⚠️ Il database di '{station.Name}' non è accessibile:\n\n{station.DatabasePath}\n\nVuoi comunque aprire questa emittente?",
                        "Database Non Accessibile",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (result == DialogResult.No)
                        return;
                }

                // ✅ NOTIFICA SELEZIONE
                StationSelected?.Invoke(this, station);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"❌ Errore selezione emittente:\n\n{ex.Message}",
                    "Errore",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void BtnManageStations_Click(object sender, EventArgs e)
        {
            // ✅ APRI FORM GESTIONE EMITTENTI
            using (var managerForm = new Forms.StationManagementDialog())
            {
                if (managerForm.ShowDialog() == DialogResult.OK)
                {
                    LoadStations();
                }
            }
        }
    }
}