using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using NAudio.Wave;
using AirManager.Controls;
using AirManager.Services;
using AirManager.Services.Database;
using AirManager.Services.Licensing;
using AirManager.Themes;
using AirManager.Forms;

namespace AirManager
{
    public partial class MainForm : Form
    {
        private Panel spacerPanel;
        private Panel mainPanel;
        private Panel contentPanel;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel lblStatus;
        private ToolStripStatusLabel lblStation;
        private MenuStrip menuStrip;

        // ✅ RIFERIMENTI AI MENU (per aggiornamento dinamico)
        private ToolStripMenuItem menuFile;
        private ToolStripMenuItem menuStations;
        private ToolStripMenuItem menuArchives;
        private ToolStripMenuItem menuReport;
        private ToolStripMenuItem menuHelp;

        // ✅ RIFERIMENTI ALLE VOCI DI MENU FILE
        private ToolStripMenuItem menuItemReload;
        private ToolStripMenuItem menuItemExportConfig;
        private ToolStripMenuItem menuItemImportConfig;
        private ToolStripMenuItem menuItemSettings;
        private ToolStripMenuItem menuItemExit;

        // ✅ RIFERIMENTI ALLE VOCI DI MENU STATIONS
        private ToolStripMenuItem menuItemManageStations;
        private ToolStripMenuItem menuItemNewStation;

        // ✅ RIFERIMENTI ALLE VOCI DI MENU ARCHIVES
        private ToolStripMenuItem menuItemArchiveMusic;
        private ToolStripMenuItem menuItemArchiveClips;

        // ✅ RIFERIMENTI ALLE VOCI DI MENU REPORT
        private ToolStripMenuItem menuItemViewReport;
        private ToolStripMenuItem menuItemExportReport;
        private ToolStripMenuItem menuItemBroadcastHistory;

        // ✅ RIFERIMENTI ALLE VOCI DI MENU HELP
        private ToolStripMenuItem menuItemAbout;
        private ToolStripMenuItem menuItemManual;
        private ToolStripMenuItem menuItemLicense;

        private StationManagerControl _stationManager;
        private ArchiveControl _archiveMusicControl;
        private ArchiveControl _archiveClipsControl;
        private ReportAdvancedControl _reportControl;

        private StationConfig _currentStation;

        private const int MENUSTRIP_HEIGHT = 28;
        private const int SPACER_HEIGHT = 5;
        private const string REGISTRY_PATH = @"SOFTWARE\AirManager\Settings";

        public MainForm()
        {
            InitializeComponent();
            InitializeCustomUI();
            ApplyLanguage(); // ✅ APPLICA LINGUA INIZIALE
            CheckStartupConfiguration();

            // ✅ SOTTOSCRIVI EVENTO CAMBIO LINGUA
            LanguageManager.LanguageChanged += (s, e) => ApplyLanguage();
        }

        private void InitializeCustomUI()
        {
            this.Text = "AirManager - Multi-Station Database Manager";
            this.Size = new Size(1400, 900);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = AppTheme.BgDark;
            this.Icon = SystemIcons.Application;

            menuStrip = new MenuStrip
            {
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                Padding = new Padding(5, 2, 0, 2),
                Dock = DockStyle.Top
            };

            // ✅ MENU FILE
            menuFile = new ToolStripMenuItem("📁 File");

            menuItemReload = new ToolStripMenuItem("🔄 Ricarica Database", null, MenuReload_Click);
            menuFile.DropDownItems.Add(menuItemReload);
            menuFile.DropDownItems.Add(new ToolStripSeparator());

            menuItemExportConfig = new ToolStripMenuItem("📤 Esporta Configurazioni...", null, MenuExportConfig_Click);
            menuFile.DropDownItems.Add(menuItemExportConfig);

            menuItemImportConfig = new ToolStripMenuItem("📥 Importa Configurazioni...", null, MenuImportConfig_Click);
            menuFile.DropDownItems.Add(menuItemImportConfig);
            menuFile.DropDownItems.Add(new ToolStripSeparator());

            menuItemSettings = new ToolStripMenuItem("⚙️ Impostazioni", null, MenuSettings_Click);
            menuFile.DropDownItems.Add(menuItemSettings);
            menuFile.DropDownItems.Add(new ToolStripSeparator());

            menuItemExit = new ToolStripMenuItem("❌ Esci", null, MenuExit_Click);
            menuFile.DropDownItems.Add(menuItemExit);

            menuStrip.Items.Add(menuFile);

            // ✅ MENU STATIONS
            menuStations = new ToolStripMenuItem("📻 Stations");

            menuItemManageStations = new ToolStripMenuItem("🏠 Gestione Emittenti", null, MenuManageStations_Click);
            menuStations.DropDownItems.Add(menuItemManageStations);
            menuStations.DropDownItems.Add(new ToolStripSeparator());

            menuItemNewStation = new ToolStripMenuItem("➕ Nuova Emittente", null, MenuNewStation_Click);
            menuStations.DropDownItems.Add(menuItemNewStation);
            menuStations.DropDownItems.Add(new ToolStripSeparator());

            LoadStationsMenu();
            menuStrip.Items.Add(menuStations);

            // ✅ MENU ARCHIVI
            menuArchives = new ToolStripMenuItem("🗂️ Archivi");

            menuItemArchiveMusic = new ToolStripMenuItem("🎵 Archivio Musica", null, MenuArchiveMusic_Click);
            menuArchives.DropDownItems.Add(menuItemArchiveMusic);

            menuItemArchiveClips = new ToolStripMenuItem("⚡ Archivio Clips", null, MenuArchiveClips_Click);
            menuArchives.DropDownItems.Add(menuItemArchiveClips);

            menuStrip.Items.Add(menuArchives);

            // ✅ MENU REPORT
            menuReport = new ToolStripMenuItem("📊 Report");

            menuItemViewReport = new ToolStripMenuItem("📈 Visualizza Report", null, MenuViewReport_Click);
            menuReport.DropDownItems.Add(menuItemViewReport);
            menuReport.DropDownItems.Add(new ToolStripSeparator());

            menuItemBroadcastHistory = new ToolStripMenuItem("📜 Broadcast History", null, MenuBroadcastHistory_Click);
            menuReport.DropDownItems.Add(menuItemBroadcastHistory);
            menuReport.DropDownItems.Add(new ToolStripSeparator());

            menuItemExportReport = new ToolStripMenuItem("💾 Esporta CSV Avanzato...", null, MenuExportReport_Click);
            menuReport.DropDownItems.Add(menuItemExportReport);

            menuStrip.Items.Add(menuReport);

            // ✅ MENU AIUTO
            menuHelp = new ToolStripMenuItem("❓ Aiuto");

            menuItemLicense = new ToolStripMenuItem("🔑 License", null, MenuLicense_Click);
            menuHelp.DropDownItems.Add(menuItemLicense);
            menuHelp.DropDownItems.Add(new ToolStripSeparator());

            menuItemAbout = new ToolStripMenuItem("ℹ️ Informazioni", null, MenuAbout_Click);
            menuHelp.DropDownItems.Add(menuItemAbout);

            menuItemManual = new ToolStripMenuItem("📖 Manuale Utente", null, MenuManual_Click);
            menuHelp.DropDownItems.Add(menuItemManual);

            menuStrip.Items.Add(menuHelp);

            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);

            ApplyMenuTheme(menuStrip);

            spacerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = SPACER_HEIGHT,
                BackColor = AppTheme.BgDark
            };
            this.Controls.Add(spacerPanel);

            statusStrip = new StatusStrip
            {
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9),
                Dock = DockStyle.Bottom
            };

            lblStatus = new ToolStripStatusLabel
            {
                Text = "Pronto",
                ForeColor = Color.White,
                Spring = true,
                TextAlign = ContentAlignment.MiddleLeft
            };
            statusStrip.Items.Add(lblStatus);

            lblStation = new ToolStripStatusLabel
            {
                Text = "Nessuna emittente selezionata",
                ForeColor = Color.FromArgb(255, 140, 0),
                BorderSides = ToolStripStatusLabelBorderSides.Left,
                BorderStyle = Border3DStyle.Etched
            };
            statusStrip.Items.Add(lblStation);

            this.Controls.Add(statusStrip);

            mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppTheme.BgLight,
                Padding = new Padding(0)
            };
            this.Controls.Add(mainPanel);

            contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppTheme.BgLight,
                Padding = new Padding(10)
            };
            mainPanel.Controls.Add(contentPanel);

            this.Controls.SetChildIndex(statusStrip, 0);
            this.Controls.SetChildIndex(mainPanel, 1);
            this.Controls.SetChildIndex(spacerPanel, 2);
            this.Controls.SetChildIndex(menuStrip, 3);
        }

        /// <summary>
        /// ✅ APPLICA LE TRADUZIONI DA LanguageManager
        /// </summary>
        private void ApplyLanguage()
        {
            // ✅ MENU PRINCIPALI
            menuFile.Text = "📁 " + LanguageManager.GetString("mainform.menu.File");
            menuStations.Text = "📻 " + LanguageManager.GetString("mainform.menu.Stations");
            menuArchives.Text = "🗂️ " + LanguageManager.GetString("mainform.menu.Archives");
            menuReport.Text = "📊 " + LanguageManager.GetString("MainForm.Menu.Report");
            menuHelp.Text = "❓ " + LanguageManager.GetString("MainForm.Menu.Help");

            // ✅ MENU FILE
            menuItemReload.Text = "🔄 " + LanguageManager.GetString("MainForm.Menu.File.Reload");
            menuItemExportConfig.Text = "📤 " + LanguageManager.GetString("MainForm.Menu.File.ExportConfig");
            menuItemImportConfig.Text = "📥 " + LanguageManager.GetString("MainForm.Menu.File.ImportConfig");
            menuItemSettings.Text = "⚙️ " + LanguageManager.GetString("MainForm.Menu.File.Settings");
            menuItemExit.Text = "❌ " + LanguageManager.GetString("MainForm.Menu.File.Exit");

            // ✅ MENU STATIONS
            menuItemManageStations.Text = "📻" + LanguageManager.GetString("MainForm.Menu.Stations.Manage");
            menuItemNewStation.Text = "➕ " + LanguageManager.GetString("MainForm.Menu.Stations.New");

            // ✅ MENU ARCHIVES
            menuItemArchiveMusic.Text = "🎵 " + LanguageManager.GetString("MainForm.Menu.Archives.Music");
            menuItemArchiveClips.Text = "⚡ " + LanguageManager.GetString("MainForm.Menu.Archives.Clips");

            // ✅ MENU REPORT
            menuItemViewReport.Text = "📈 " + LanguageManager.GetString("MainForm.Menu.Report.View");
            menuItemExportReport.Text = "💾 " + LanguageManager.GetString("MainForm.Menu.Report.Export");
            menuItemBroadcastHistory.Text = "📜 " + LanguageManager.GetString("MainForm.Menu.Report.BroadcastHistory");

            // ✅ MENU HELP
            menuItemAbout.Text = "ℹ️ " + LanguageManager.GetString("MainForm.Menu.Help.About");
            menuItemManual.Text = "📖 " + LanguageManager.GetString("MainForm.Menu.Help.Manual");
            menuItemLicense.Text = "🔑 " + LanguageManager.GetString("MainForm.Menu.Help.License");

            // ✅ STATUS BAR
            if (lblStatus.Text == "Pronto" || lblStatus.Text.Contains("Ready"))
            {
                lblStatus.Text = LanguageManager.GetString("MainForm.Status.Ready");
            }

            if (_currentStation == null)
            {
                lblStation.Text = LanguageManager.GetString("MainForm.Status.NoStation");
            }

            Console.WriteLine($"[MainForm] ✅ Lingua applicata: {LanguageManager.CurrentLanguage}");
        }

        private void ApplyMenuTheme(MenuStrip menu)
        {
            menu.Renderer = new ToolStripProfessionalRenderer(new MenuColorTable());

            foreach (ToolStripMenuItem item in menu.Items)
            {
                item.ForeColor = Color.White;
                ApplyMenuItemTheme(item);
            }
        }

        private void ApplyMenuItemTheme(ToolStripMenuItem item)
        {
            foreach (ToolStripItem subItem in item.DropDownItems)
            {
                subItem.ForeColor = Color.White;
                subItem.BackColor = Color.FromArgb(45, 45, 48);

                if (subItem is ToolStripMenuItem menuItem)
                {
                    ApplyMenuItemTheme(menuItem);
                }
            }
        }

        private void CheckStartupConfiguration()
        {
            var stations = StationRegistry.LoadAllStations();

            if (stations.Count == 0)
            {
                MessageBox.Show(
                    LanguageManager.GetString("MainForm.Startup.WelcomeMessage"),
                    LanguageManager.GetString("MainForm.Startup.WelcomeTitle"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                ShowNewStationDialog();
            }

            string activeStationId = StationRegistry.GetActiveStationId();
            if (!string.IsNullOrEmpty(activeStationId))
            {
                var activeStation = StationRegistry.LoadStation(activeStationId);
                if (activeStation != null)
                {
                    _currentStation = activeStation;
                    LoadStation(_currentStation);
                }
            }

            ShowStationManager();
        }

        private void LoadStationsMenu()
        {
            for (int i = menuStations.DropDownItems.Count - 1; i >= 0; i--)
            {
                if (menuStations.DropDownItems[i].Tag?.ToString() == "DYNAMIC_STATION")
                {
                    menuStations.DropDownItems.RemoveAt(i);
                }
            }

            var stations = StationRegistry.LoadAllStations();
            string activeStationId = StationRegistry.GetActiveStationId();

            if (stations.Count > 0)
            {
                foreach (var station in stations)
                {
                    bool isActive = station.Id == activeStationId;
                    string displayText = isActive ? $"📻 {station.Name} ✓" : $"📻 {station.Name}";

                    var stationItem = new ToolStripMenuItem
                    {
                        Text = displayText,
                        Tag = "DYNAMIC_STATION"
                    };

                    if (isActive)
                    {
                        stationItem.ForeColor = Color.FromArgb(0, 255, 0);
                        stationItem.Font = new Font(stationItem.Font, FontStyle.Bold);
                    }

                    stationItem.Click += (s, e) => LoadStationFromMenu(station);
                    menuStations.DropDownItems.Add(stationItem);
                }
            }
            else
            {
                var emptyItem = new ToolStripMenuItem
                {
                    Text = LanguageManager.GetString("MainForm.Menu.Stations.NoStations"),
                    Tag = "DYNAMIC_STATION",
                    Enabled = false,
                    ForeColor = Color.Gray
                };
                menuStations.DropDownItems.Add(emptyItem);
            }

            Console.WriteLine($"[MainForm] ✅ Menu Stations aggiornato con {stations.Count} emittenti");
        }

        private void LoadStationFromMenu(StationConfig station)
        {
            _currentStation = station;
            LoadStation(station);
            LoadStationsMenu();

            if (_stationManager != null && contentPanel.Controls.Contains(_stationManager))
            {
                _stationManager.RefreshStations();
            }
        }

        private void LoadStation(StationConfig station)
        {
            try
            {
                StationRegistry.SetActiveStation(station);
                DbcManager.Initialize();

                lblStation.Text = $"📻 {station.Name}";
                lblStation.ForeColor = Color.FromArgb(0, 255, 0);
                lblStatus.Text = string.Format(
                    LanguageManager.GetString("MainForm.Status.StationLoaded"),
                    station.Name);

                Console.WriteLine($"[AirManager] ✅ Caricata emittente: {station.Name}");
                Console.WriteLine($"[AirManager]    Database: {station.DatabasePath}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(LanguageManager.GetString("MainForm.Error.LoadStation"), ex.Message),
                    LanguageManager.GetString("Common.Error"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void ShowStationManager()
        {
            contentPanel.Controls.Clear();

            if (_stationManager == null)
            {
                _stationManager = new StationManagerControl();
                _stationManager.StationChanged += (s, station) =>
                {
                    _currentStation = station;
                    LoadStation(station);
                    LoadStationsMenu();
                    _stationManager.RefreshStations();
                };
            }

            _stationManager.Dock = DockStyle.Fill;
            contentPanel.Controls.Add(_stationManager);
            _stationManager.RefreshStations();

            lblStatus.Text = LanguageManager.GetString("MainForm.Status.ManagingStations");
        }

        private void MenuReload_Click(object sender, EventArgs e)
        {
            if (_currentStation != null)
            {
                LoadStation(_currentStation);

                if (_archiveMusicControl != null && contentPanel.Controls.Contains(_archiveMusicControl))
                {
                    _archiveMusicControl.RefreshArchive();
                }
                else if (_archiveClipsControl != null && contentPanel.Controls.Contains(_archiveClipsControl))
                {
                    _archiveClipsControl.RefreshArchive();
                }
                else if (_reportControl != null && contentPanel.Controls.Contains(_reportControl))
                {
                    _reportControl.LoadDefaultReport();
                }

                MessageBox.Show(
                    LanguageManager.GetString("MainForm.Message.DatabaseReloaded"),
                    LanguageManager.GetString("Common.Success"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(
                    LanguageManager.GetString("MainForm.Message.NoStationSelected"),
                    LanguageManager.GetString("Common.Warning"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private void MenuExportConfig_Click(object sender, EventArgs e)
        {
            using (var exportForm = new StationExportImportForm(false))
            {
                exportForm.ShowDialog();
            }
        }

        private void MenuImportConfig_Click(object sender, EventArgs e)
        {
            using (var importForm = new StationExportImportForm(true))
            {
                if (importForm.ShowDialog() == DialogResult.OK)
                {
                    LoadStationsMenu();

                    if (_stationManager != null && contentPanel.Controls.Contains(_stationManager))
                    {
                        _stationManager.RefreshStations();
                    }

                    MessageBox.Show(
                        LanguageManager.GetString("MainForm.Message.ImportCompleted"),
                        LanguageManager.GetString("MainForm.Title.ImportCompleted"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
        }

        private void MenuSettings_Click(object sender, EventArgs e)
        {
            using (var settingsForm = new SettingsForm())
            {
                settingsForm.ShowDialog();
            }
        }

        private void MenuExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void MenuManageStations_Click(object sender, EventArgs e)
        {
            ShowStationManager();
        }

        private void MenuNewStation_Click(object sender, EventArgs e)
        {
            ShowNewStationDialog();
        }

        private void ShowNewStationDialog()
        {
            using (var form = new StationConfigForm(null))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadStationsMenu();

                    if (_stationManager != null && contentPanel.Controls.Contains(_stationManager))
                    {
                        _stationManager.RefreshStations();
                    }

                    MessageBox.Show(
                        LanguageManager.GetString("MainForm.Message.StationCreated"),
                        LanguageManager.GetString("MainForm.Title.NewStation"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    Console.WriteLine("[MainForm] ✅ Menu Stations aggiornato dopo creazione nuova emittente");
                }
            }
        }

        private void MenuArchiveMusic_Click(object sender, EventArgs e)
        {
            if (_currentStation == null)
            {
                MessageBox.Show(
                    LanguageManager.GetString("MainForm.Message.SelectStationFirst"),
                    LanguageManager.GetString("Common.Warning"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            contentPanel.Controls.Clear();

            if (_archiveMusicControl == null)
            {
                _archiveMusicControl = new ArchiveControl("Music");
                _archiveMusicControl.StatusChanged += (s, msg) => lblStatus.Text = msg;
            }

            _archiveMusicControl.Dock = DockStyle.Fill;
            contentPanel.Controls.Add(_archiveMusicControl);
            _archiveMusicControl.RefreshArchive();

            lblStatus.Text = $"{LanguageManager.GetString("MainForm.Status.ArchiveMusic")} - {_currentStation.Name}";
        }

        private void MenuArchiveClips_Click(object sender, EventArgs e)
        {
            if (_currentStation == null)
            {
                MessageBox.Show(
                    LanguageManager.GetString("MainForm.Message.SelectStationFirst"),
                    LanguageManager.GetString("Common.Warning"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            contentPanel.Controls.Clear();

            if (_archiveClipsControl == null)
            {
                _archiveClipsControl = new ArchiveControl("Clips");
                _archiveClipsControl.StatusChanged += (s, msg) => lblStatus.Text = msg;
            }

            _archiveClipsControl.Dock = DockStyle.Fill;
            contentPanel.Controls.Add(_archiveClipsControl);
            _archiveClipsControl.RefreshArchive();

            lblStatus.Text = $"{LanguageManager.GetString("MainForm.Status.ArchiveClips")} - {_currentStation.Name}";
        }

        private void MenuViewReport_Click(object sender, EventArgs e)
        {
            if (_currentStation == null)
            {
                MessageBox.Show(
                    LanguageManager.GetString("MainForm.Message.SelectStationFirst"),
                    LanguageManager.GetString("Common.Warning"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            contentPanel.Controls.Clear();

            if (_reportControl == null)
            {
                _reportControl = new ReportAdvancedControl();
            }

            _reportControl.Dock = DockStyle.Fill;
            contentPanel.Controls.Add(_reportControl);
            _reportControl.LoadDefaultReport();

            lblStatus.Text = $"{LanguageManager.GetString("MainForm.Status.AdvancedReport")} - {_currentStation.Name}";
        }

        private void MenuExportReport_Click(object sender, EventArgs e)
        {
            if (_currentStation == null)
            {
                MessageBox.Show(
                    LanguageManager.GetString("MainForm.Message.SelectStationFirst"),
                    LanguageManager.GetString("Common.Warning"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (_reportControl == null || !contentPanel.Controls.Contains(_reportControl))
            {
                MenuViewReport_Click(sender, e);
            }

            _reportControl?.ShowExportDialog();
        }

        private void MenuAbout_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                LanguageManager.GetString("MainForm.About.Message"),
                LanguageManager.GetString("MainForm.About.Title"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void MenuManual_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                LanguageManager.GetString("MainForm.Manual.Content"),
                LanguageManager.GetString("MainForm.Manual.Title"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void MenuBroadcastHistory_Click(object sender, EventArgs e)
        {
            using (var form = new BroadcastHistoryForm())
            {
                form.ShowDialog();
            }
        }

        private void MenuLicense_Click(object sender, EventArgs e)
        {
            if (LicenseManager.IsLicenseValid())
            {
                using (var infoForm = new LicenseInfoForm())
                {
                    if (infoForm.ShowDialog(this) == DialogResult.OK && infoForm.LicenseRemoved)
                    {
                        MessageBox.Show(
                            LanguageManager.GetString("MainForm.LicenseRemoved", "Licenza rimossa con successo. L'applicazione verrà chiusa."),
                            LanguageManager.GetString("Common.Success", "Successo"),
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                        Application.Exit();
                    }
                }
            }
            else
            {
                using (var form = new LicenseForm())
                {
                    form.ShowDialog();
                }
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            var result = MessageBox.Show(
                LanguageManager.GetString("MainForm.Exit.ConfirmMessage"),
                LanguageManager.GetString("MainForm.Exit.ConfirmTitle"),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.No)
            {
                e.Cancel = true;
            }
            else
            {
                try
                {
                    _archiveMusicControl?.Dispose();
                    _archiveClipsControl?.Dispose();
                    _reportControl?.Dispose();
                    _stationManager?.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[MainForm] Errore cleanup: {ex.Message}");
                }
            }

            base.OnFormClosing(e);
        }
    }

    public class MenuColorTable : ProfessionalColorTable
    {
        public override Color MenuItemSelected => Color.FromArgb(62, 62, 66);
        public override Color MenuItemSelectedGradientBegin => Color.FromArgb(62, 62, 66);
        public override Color MenuItemSelectedGradientEnd => Color.FromArgb(62, 62, 66);
        public override Color MenuItemBorder => Color.FromArgb(62, 62, 66);
        public override Color MenuBorder => Color.FromArgb(62, 62, 66);
        public override Color MenuItemPressedGradientBegin => Color.FromArgb(0, 122, 204);
        public override Color MenuItemPressedGradientEnd => Color.FromArgb(0, 122, 204);
        public override Color ImageMarginGradientBegin => Color.FromArgb(45, 45, 48);
        public override Color ImageMarginGradientMiddle => Color.FromArgb(45, 45, 48);
        public override Color ImageMarginGradientEnd => Color.FromArgb(45, 45, 48);
        public override Color ToolStripDropDownBackground => Color.FromArgb(45, 45, 48);
    }
}
