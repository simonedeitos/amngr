using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using AirManager.Services;
using AirManager.Themes;

namespace AirManager.Forms
{
    public partial class StationConfigForm : Form
    {
        private StationConfig _station;
        private bool _isEditMode;

        private Panel headerPanel;
        private Label lblTitle;

        private Label lblName;
        private TextBox txtName;

        private Label lblLogo;
        private TextBox txtLogoPath;
        private Button btnBrowseLogo;
        private PictureBox picLogoPreview;

        private Label lblDatabase;
        private TextBox txtDatabasePath;
        private Button btnBrowseDatabase;
        private Label lblDatabaseStatus;

        private Button btnSave;
        private Button btnCancel;
        private Button btnTestConnection;

        public StationConfigForm(StationConfig station)
        {
            InitializeComponent();

            _station = station;
            _isEditMode = station != null;

            InitializeCustomUI();
            ApplyLanguage(); // ✅ APPLICA LINGUA INIZIALE
            LoadData();

            // ✅ SOTTOSCRIVI EVENTO CAMBIO LINGUA
            LanguageManager.LanguageChanged += (s, e) => ApplyLanguage();
        }

        /// <summary>
        /// ✅ APPLICA LE TRADUZIONI DA LanguageManager
        /// </summary>
        private void ApplyLanguage()
        {
            // ✅ TITOLO FORM E HEADER
            if (_isEditMode)
            {
                this.Text = "✏️ " + LanguageManager.GetString("StationConfig.Title.Edit");
                lblTitle.Text = "✏️ " + LanguageManager.GetString("StationConfig.HeaderTitle.Edit");
            }
            else
            {
                this.Text = "➕ " + LanguageManager.GetString("StationConfig.Title.New");
                lblTitle.Text = "➕ " + LanguageManager.GetString("StationConfig.HeaderTitle.New");
            }

            // ✅ LABELS
            lblName.Text = LanguageManager.GetString("StationConfig.Label.Name");
            lblLogo.Text = LanguageManager.GetString("StationConfig.Label.Logo");
            lblDatabase.Text = LanguageManager.GetString("StationConfig.Label.Database");

            // ✅ BOTTONI
            btnBrowseLogo.Text = "📁 " + LanguageManager.GetString("StationConfig.Browse");
            btnBrowseDatabase.Text = "📁 " + LanguageManager.GetString("StationConfig.Browse");
            btnTestConnection.Text = "🔍 " + LanguageManager.GetString("StationConfig.TestConnection");
            btnSave.Text = "💾 " + LanguageManager.GetString("Common.Save");
            btnCancel.Text = "✖ " + LanguageManager.GetString("Common.Cancel");

            // ✅ RICONVALIDA DATABASE STATUS (per tradurre)
            if (!string.IsNullOrEmpty(txtDatabasePath.Text))
            {
                ValidateDatabasePath(txtDatabasePath.Text);
            }

            Console.WriteLine($"[StationConfig] ✅ Lingua applicata: {LanguageManager.CurrentLanguage}");
        }

        private void InitializeCustomUI()
        {
            this.Text = _isEditMode ? "✏️ Modifica Emittente" : "➕ Nuova Emittente";
            this.Size = new Size(700, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = AppTheme.BgLight;

            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = AppTheme.BgDark
            };

            lblTitle = new Label
            {
                Text = _isEditMode ? "✏️ MODIFICA EMITTENTE" : "➕ NUOVA EMITTENTE",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 18),
                AutoSize = true
            };
            headerPanel.Controls.Add(lblTitle);

            this.Controls.Add(headerPanel);

            lblName = new Label
            {
                Text = "Nome Emittente:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(30, 90),
                Size = new Size(150, 25)
            };
            this.Controls.Add(lblName);

            txtName = new TextBox
            {
                Font = new Font("Segoe UI", 11),
                Location = new Point(30, 120),
                Size = new Size(620, 30),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(txtName);

            lblLogo = new Label
            {
                Text = "Logo (opzionale - consigliato 120x70 px):",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(30, 170),
                Size = new Size(350, 25)
            };
            this.Controls.Add(lblLogo);

            txtLogoPath = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(30, 200),
                Size = new Size(480, 30),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                ReadOnly = true
            };
            txtLogoPath.TextChanged += TxtLogoPath_TextChanged;
            this.Controls.Add(txtLogoPath);

            btnBrowseLogo = new Button
            {
                Text = "📁 Sfoglia",
                Location = new Point(520, 198),
                Size = new Size(130, 34),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnBrowseLogo.FlatAppearance.BorderSize = 0;
            btnBrowseLogo.Click += BtnBrowseLogo_Click;
            this.Controls.Add(btnBrowseLogo);

            picLogoPreview = new PictureBox
            {
                Location = new Point(30, 245),
                Size = new Size(120, 70),
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(60, 60, 60)
            };
            this.Controls.Add(picLogoPreview);

            lblDatabase = new Label
            {
                Text = "Percorso Database (locale o rete):",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(30, 335),
                Size = new Size(350, 25)
            };
            this.Controls.Add(lblDatabase);

            txtDatabasePath = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(30, 365),
                Size = new Size(480, 30),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            txtDatabasePath.TextChanged += TxtDatabasePath_TextChanged;
            this.Controls.Add(txtDatabasePath);

            btnBrowseDatabase = new Button
            {
                Text = "📁 Sfoglia",
                Location = new Point(520, 363),
                Size = new Size(130, 34),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnBrowseDatabase.FlatAppearance.BorderSize = 0;
            btnBrowseDatabase.Click += BtnBrowseDatabase_Click;
            this.Controls.Add(btnBrowseDatabase);

            lblDatabaseStatus = new Label
            {
                Text = "⚪ Percorso non verificato",
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = Color.Gray,
                Location = new Point(30, 405),
                Size = new Size(620, 20),
                AutoSize = false
            };
            this.Controls.Add(lblDatabaseStatus);

            btnTestConnection = new Button
            {
                Text = "🔍 Verifica Accessibilità",
                Location = new Point(30, 435),
                Size = new Size(180, 35),
                BackColor = Color.FromArgb(138, 43, 226),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnTestConnection.FlatAppearance.BorderSize = 0;
            btnTestConnection.Click += BtnTestConnection_Click;
            this.Controls.Add(btnTestConnection);

            btnCancel = new Button
            {
                Text = "✖ Annulla",
                Location = new Point(430, 505),
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

            btnSave = new Button
            {
                Text = "💾 Salva",
                Location = new Point(550, 505),
                Size = new Size(110, 40),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            this.CancelButton = btnCancel;
        }

        private void LoadData()
        {
            if (_isEditMode && _station != null)
            {
                txtName.Text = _station.Name;
                txtLogoPath.Text = _station.LogoPath;
                txtDatabasePath.Text = _station.DatabasePath;

                LoadLogoPreview(_station.LogoPath);
                ValidateDatabasePath(_station.DatabasePath);
            }
            else
            {
                txtDatabasePath.Text = @"C:\AirDirector\Database";
                ValidateDatabasePath(txtDatabasePath.Text);
            }
        }

        private void BtnBrowseLogo_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = LanguageManager.GetString("StationConfig.Dialog.LogoFilter");
                ofd.Title = LanguageManager.GetString("StationConfig.Dialog.LogoTitle");

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtLogoPath.Text = ofd.FileName;
                    LoadLogoPreview(ofd.FileName);
                }
            }
        }

        private void BtnBrowseDatabase_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.Description = LanguageManager.GetString("StationConfig.Dialog.DatabaseDescription");
                fbd.ShowNewFolderButton = true;

                if (!string.IsNullOrEmpty(txtDatabasePath.Text))
                {
                    fbd.SelectedPath = txtDatabasePath.Text;
                }

                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    txtDatabasePath.Text = fbd.SelectedPath;
                    ValidateDatabasePath(fbd.SelectedPath);
                }
            }
        }

        private void TxtLogoPath_TextChanged(object sender, EventArgs e)
        {
            LoadLogoPreview(txtLogoPath.Text);
        }

        private void TxtDatabasePath_TextChanged(object sender, EventArgs e)
        {
            ValidateDatabasePath(txtDatabasePath.Text);
        }

        private void LoadLogoPreview(string path)
        {
            try
            {
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    if (picLogoPreview.Image != null)
                    {
                        picLogoPreview.Image.Dispose();
                    }

                    picLogoPreview.Image = Image.FromFile(path);
                }
                else
                {
                    if (picLogoPreview.Image != null)
                    {
                        picLogoPreview.Image.Dispose();
                        picLogoPreview.Image = null;
                    }
                }
            }
            catch
            {
                if (picLogoPreview.Image != null)
                {
                    picLogoPreview.Image.Dispose();
                    picLogoPreview.Image = null;
                }
            }
        }

        private void ValidateDatabasePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                lblDatabaseStatus.Text = "⚪ " + LanguageManager.GetString("StationConfig.Status.EnterPath");
                lblDatabaseStatus.ForeColor = Color.Gray;
                return;
            }

            bool isAccessible = StationRegistry.ValidateDatabasePath(path);

            if (isAccessible)
            {
                lblDatabaseStatus.Text = "✅ " + LanguageManager.GetString("StationConfig.Status.Accessible");
                lblDatabaseStatus.ForeColor = Color.FromArgb(0, 255, 0);
            }
            else
            {
                lblDatabaseStatus.Text = "⚠️ " + LanguageManager.GetString("StationConfig.Status.NotAccessible");
                lblDatabaseStatus.ForeColor = Color.FromArgb(255, 140, 0);
            }
        }

        private void BtnTestConnection_Click(object sender, EventArgs e)
        {
            string path = txtDatabasePath.Text.Trim();

            if (string.IsNullOrWhiteSpace(path))
            {
                MessageBox.Show(
                    LanguageManager.GetString("StationConfig.Validation.EnterPath"),
                    LanguageManager.GetString("StationConfig.Title.PathMissing"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            this.Cursor = Cursors.WaitCursor;

            try
            {
                bool isAccessible = StationRegistry.ValidateDatabasePath(path);

                if (isAccessible)
                {
                    string[] requiredFiles = { "Music.dbc", "Clips.dbc", "Config.dbc" };
                    int foundFiles = 0;

                    foreach (string file in requiredFiles)
                    {
                        if (File.Exists(Path.Combine(path, file)))
                        {
                            foundFiles++;
                        }
                    }

                    string statusMessage = foundFiles == 0
                        ? LanguageManager.GetString("StationConfig.Test.NoFilesFound")
                        : LanguageManager.GetString("StationConfig.Test.ExistingDatabase");

                    MessageBox.Show(
                        string.Format(LanguageManager.GetString("StationConfig.Test.AccessibleMessage"),
                            path, foundFiles, requiredFiles.Length, statusMessage),
                        LanguageManager.GetString("StationConfig.Test.Title"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    ValidateDatabasePath(path);
                }
                else
                {
                    var result = MessageBox.Show(
                        string.Format(LanguageManager.GetString("StationConfig.Test.NotAccessibleMessage"), path),
                        LanguageManager.GetString("StationConfig.Test.NotAccessibleTitle"),
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes)
                    {
                        try
                        {
                            Directory.CreateDirectory(path);
                            MessageBox.Show(
                                string.Format(LanguageManager.GetString("StationConfig.Message.FolderCreated"), path),
                                LanguageManager.GetString("StationConfig.Title.FolderCreated"),
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);

                            ValidateDatabasePath(path);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(
                                string.Format(LanguageManager.GetString("StationConfig.Error.CreateFolder"), ex.Message),
                                LanguageManager.GetString("Common.Error"),
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                    }
                }
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show(
                    LanguageManager.GetString("StationConfig.Validation.EnterName"),
                    LanguageManager.GetString("StationConfig.Title.NameMissing"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtDatabasePath.Text))
            {
                MessageBox.Show(
                    LanguageManager.GetString("StationConfig.Validation.EnterDatabase"),
                    LanguageManager.GetString("StationConfig.Title.DatabaseMissing"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                txtDatabasePath.Focus();
                return;
            }

            if (_station == null)
            {
                _station = new StationConfig();
            }

            _station.Name = txtName.Text.Trim();
            _station.LogoPath = txtLogoPath.Text.Trim();
            _station.DatabasePath = txtDatabasePath.Text.Trim();

            if (!Directory.Exists(_station.DatabasePath))
            {
                try
                {
                    Directory.CreateDirectory(_station.DatabasePath);
                    Console.WriteLine($"[StationConfigForm] ✅ Directory creata: {_station.DatabasePath}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        string.Format(LanguageManager.GetString("StationConfig.Error.CreateDatabaseFolder"), ex.Message),
                        LanguageManager.GetString("Common.Error"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }
            }

            if (StationRegistry.SaveStation(_station))
            {
                MessageBox.Show(
                    string.Format(LanguageManager.GetString("StationConfig.Message.SaveSuccess"), _station.Name),
                    LanguageManager.GetString("StationConfig.Title.SaveComplete"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show(
                    LanguageManager.GetString("StationConfig.Error.SaveFailed"),
                    LanguageManager.GetString("Common.Error"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (picLogoPreview.Image != null)
            {
                picLogoPreview.Image.Dispose();
            }

            // ✅ DISOTTOSCRIVI EVENTO
            LanguageManager.LanguageChanged -= (s, e2) => ApplyLanguage();

            base.OnFormClosing(e);
        }
    }
}