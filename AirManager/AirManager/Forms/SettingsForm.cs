using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Win32;
using NAudio.Wave;
using AirManager.Services;
using AirManager.Themes;

namespace AirManager.Forms
{
    public partial class SettingsForm : Form
    {
        private Panel headerPanel;
        private Label lblTitle;

        private GroupBox grpAudioOutput;
        private Label lblAudioOutput;
        private ComboBox cmbAudioOutput;

        private GroupBox grpLanguage;
        private Label lblLanguage;
        private ComboBox cmbLanguage;
        private GroupBox grpPaths;
        private DataGridView dgvPaths;
        private Button btnAddPath;
        private Button btnEditPath;
        private Button btnRemovePath;

        private Button btnSave;
        private Button btnCancel;

        private const string REGISTRY_PATH = @"SOFTWARE\AirManager\Settings";

        private List<WaveOutCapabilities> _audioDevices;
        private List<string> _languageFiles;
        private List<PathMappingEntry> _pathMappings;

        public SettingsForm()
        {
            InitializeComponent();
            InitializeCustomUI();
            LoadSettings();
            ApplyLanguage(); // ✅ APPLICA LINGUA INIZIALE

            // ✅ SOTTOSCRIVI EVENTO CAMBIO LINGUA
            LanguageManager.LanguageChanged += (s, e) => ApplyLanguage();
        }

        private void InitializeCustomUI()
        {
            this.Text = "⚙️ Impostazioni";
            this.Size = new Size(600, 650);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = AppTheme.BgLight;

            // ✅ HEADER
            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = AppTheme.BgDark
            };

            lblTitle = new Label
            {
                Text = "⚙️ IMPOSTAZIONI",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 20),
                AutoSize = true
            };
            headerPanel.Controls.Add(lblTitle);

            this.Controls.Add(headerPanel);

            // ✅ AUDIO OUTPUT
            grpAudioOutput = new GroupBox
            {
                Text = "🔊 Audio Output",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 90),
                Size = new Size(540, 100)
            };

            lblAudioOutput = new Label
            {
                Text = "Periferica audio per preascolto:",
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = Color.White,
                Location = new Point(15, 30),
                AutoSize = true
            };
            grpAudioOutput.Controls.Add(lblAudioOutput);

            cmbAudioOutput = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9),
                Location = new Point(15, 55),
                Size = new Size(500, 25),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White
            };
            grpAudioOutput.Controls.Add(cmbAudioOutput);

            this.Controls.Add(grpAudioOutput);

            // ✅ LINGUE
            grpLanguage = new GroupBox
            {
                Text = "🌐 Lingua",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 210),
                Size = new Size(540, 100)
            };

            lblLanguage = new Label
            {
                Text = "Lingua interfaccia:",
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = Color.White,
                Location = new Point(15, 30),
                AutoSize = true
            };
            grpLanguage.Controls.Add(lblLanguage);

            cmbLanguage = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9),
                Location = new Point(15, 55),
                Size = new Size(500, 25),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White
            };
            grpLanguage.Controls.Add(cmbLanguage);

            this.Controls.Add(grpLanguage);

            // ✅ PERCORSI (MAPPATURE)
            grpPaths = new GroupBox
            {
                Text = "📂 Percorsi",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 320),
                Size = new Size(540, 220)
            };

            dgvPaths = new DataGridView
            {
                Location = new Point(15, 30),
                Size = new Size(400, 175),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                AutoGenerateColumns = false,
                BackgroundColor = Color.FromArgb(50, 50, 50),
                BorderStyle = BorderStyle.FixedSingle
            };
            dgvPaths.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = LanguageManager.GetString("SettingsForm.Paths.Source", "Origine"),
                DataPropertyName = "SourcePath",
                Width = 190
            });
            dgvPaths.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = LanguageManager.GetString("SettingsForm.Paths.Target", "Destinazione"),
                DataPropertyName = "TargetPath",
                Width = 190
            });
            grpPaths.Controls.Add(dgvPaths);

            btnAddPath = new Button
            {
                Text = "➕",
                Location = new Point(430, 40),
                Size = new Size(90, 32),
                FlatStyle = FlatStyle.Flat
            };
            btnAddPath.Click += BtnAddPath_Click;
            grpPaths.Controls.Add(btnAddPath);

            btnEditPath = new Button
            {
                Text = "✏️",
                Location = new Point(430, 80),
                Size = new Size(90, 32),
                FlatStyle = FlatStyle.Flat
            };
            btnEditPath.Click += BtnEditPath_Click;
            grpPaths.Controls.Add(btnEditPath);

            btnRemovePath = new Button
            {
                Text = "🗑️",
                Location = new Point(430, 120),
                Size = new Size(90, 32),
                FlatStyle = FlatStyle.Flat
            };
            btnRemovePath.Click += BtnRemovePath_Click;
            grpPaths.Controls.Add(btnRemovePath);

            this.Controls.Add(grpPaths);

            // ✅ BOTTONI
            btnCancel = new Button
            {
                Text = "✖ Annulla",
                Location = new Point(360, 560),
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
                Location = new Point(480, 560),
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

        /// <summary>
        /// ✅ APPLICA LE TRADUZIONI DA LanguageManager
        /// </summary>
        private void ApplyLanguage()
        {
            this.Text = "⚙️ " + LanguageManager.GetString("SettingsForm.Title");
            lblTitle.Text = "⚙️ " + LanguageManager.GetString("SettingsForm.HeaderTitle");

            grpAudioOutput.Text = "🔊 " + LanguageManager.GetString("SettingsForm.AudioOutput.GroupTitle");
            lblAudioOutput.Text = LanguageManager.GetString("SettingsForm.AudioOutput.Label");

            grpLanguage.Text = "🌐 " + LanguageManager.GetString("SettingsForm.Language.GroupTitle");
            lblLanguage.Text = LanguageManager.GetString("SettingsForm.Language.Label");
            grpPaths.Text = "📂 " + LanguageManager.GetString("SettingsForm.Paths.GroupTitle", "Percorsi");
            btnAddPath.Text = "➕ " + LanguageManager.GetString("SettingsForm.Paths.Add", "Aggiungi");
            btnEditPath.Text = "✏️ " + LanguageManager.GetString("SettingsForm.Paths.Edit", "Modifica");
            btnRemovePath.Text = "🗑️ " + LanguageManager.GetString("SettingsForm.Paths.Remove", "Rimuovi");
            if (dgvPaths.Columns.Count >= 2)
            {
                dgvPaths.Columns[0].HeaderText = LanguageManager.GetString("SettingsForm.Paths.Source", "Origine");
                dgvPaths.Columns[1].HeaderText = LanguageManager.GetString("SettingsForm.Paths.Target", "Destinazione");
            }

            btnSave.Text = "💾 " + LanguageManager.GetString("Common.Save");
            btnCancel.Text = "✖ " + LanguageManager.GetString("Common.Cancel");

            Console.WriteLine($"[SettingsForm] ✅ Lingua applicata:  {LanguageManager.CurrentLanguage}");
        }

        private void LoadSettings()
        {
            LoadAudioDevices();
            LoadLanguageFiles();
            LoadSavedSettings();
            LoadPathMappings();
        }

        private void LoadPathMappings()
        {
            _pathMappings = PathMappingService.LoadMappings();
            RefreshPathMappingsGrid();
        }

        private void RefreshPathMappingsGrid()
        {
            if (dgvPaths == null) return;
            dgvPaths.DataSource = null;
            dgvPaths.DataSource = _pathMappings
                .Select(p => new PathMappingEntry { SourcePath = p.SourcePath, TargetPath = p.TargetPath })
                .ToList();
        }

        private void LoadAudioDevices()
        {
            try
            {
                _audioDevices = new List<WaveOutCapabilities>();
                cmbAudioOutput.Items.Clear();

                int deviceCount = WaveOut.DeviceCount;

                // ✅ AGGIUNGI DEVICE DI DEFAULT (tradotto)
                cmbAudioOutput.Items.Add("🔊 " + LanguageManager.GetString("SettingsForm.AudioOutput.Default", "Default (System)"));
                _audioDevices.Add(new WaveOutCapabilities());

                var deviceList = new List<(int index, string name)>();

                for (int i = 0; i < deviceCount; i++)
                {
                    var capabilities = WaveOut.GetCapabilities(i);
                    _audioDevices.Add(capabilities);
                    deviceList.Add((i, capabilities.ProductName));
                }

                deviceList = deviceList.OrderBy(d => d.name).ToList();

                foreach (var device in deviceList)
                {
                    cmbAudioOutput.Items.Add($"🎧 {device.name}");
                }

                cmbAudioOutput.SelectedIndex = 0;

                Console.WriteLine($"[Settings] ✅ Caricate {deviceCount} periferiche audio");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Settings] ❌ Errore caricamento audio devices: {ex.Message}");
                cmbAudioOutput.Items.Add("⚠️ " + LanguageManager.GetString("SettingsForm.AudioOutput.NoDevices", "No devices available"));
                cmbAudioOutput.SelectedIndex = 0;
                cmbAudioOutput.Enabled = false;
            }
        }

        private void LoadLanguageFiles()
        {
            try
            {
                _languageFiles = new List<string>();
                cmbLanguage.Items.Clear();

                string exePath = AppDomain.CurrentDomain.BaseDirectory;
                string languagesPath = Path.Combine(exePath, "Languages");

                if (!Directory.Exists(languagesPath))
                {
                    Directory.CreateDirectory(languagesPath);
                    Console.WriteLine($"[Settings] ✅ Cartella Languages creata: {languagesPath}");
                    CreateDefaultLanguageFile(languagesPath);
                }

                var iniFiles = Directory.GetFiles(languagesPath, "*.ini");

                if (iniFiles.Length == 0)
                {
                    CreateDefaultLanguageFile(languagesPath);
                    iniFiles = Directory.GetFiles(languagesPath, "*.ini");
                }

                var sortedFiles = iniFiles
                    .Select(f => Path.GetFileNameWithoutExtension(f))
                    .OrderBy(name => name)
                    .ToList();

                foreach (var fileName in sortedFiles)
                {
                    _languageFiles.Add(fileName);
                    cmbLanguage.Items.Add($"🌐 {fileName}");
                }

                cmbLanguage.SelectedIndex = 0;

                Console.WriteLine($"[Settings] ✅ Caricate {_languageFiles.Count} lingue");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Settings] ❌ Errore caricamento lingue: {ex.Message}");
                cmbLanguage.Items.Add("⚠️ " + LanguageManager.GetString("SettingsForm.Language.NoLanguages", "No languages available"));
                cmbLanguage.SelectedIndex = 0;
                cmbLanguage.Enabled = false;
            }
        }

        private void CreateDefaultLanguageFile(string languagesPath)
        {
            try
            {
                string italianFile = Path.Combine(languagesPath, "Italiano.ini");

                string defaultContent = @"; ============================================
; AIRMANAGER - LANGUAGE FILE (ITALIANO)
; ============================================

[General]
AppName=AirManager
Version=1.0.0

[Common]
Save=Salva
Cancel=Annulla
Close=Chiudi
Delete=Elimina
Edit=Modifica
New=Nuovo
Yes=Sì
No=No
OK=OK
Error=Errore
Warning=Attenzione
Success=Successo
Confirm=Conferma
";

                File.WriteAllText(italianFile, defaultContent);
                Console.WriteLine($"[Settings] ✅ File lingua default creato: {italianFile}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Settings] ❌ Errore creazione file lingua:  {ex.Message}");
            }
        }

        private void LoadSavedSettings()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(REGISTRY_PATH))
                {
                    if (key != null)
                    {
                        // ✅ AUDIO OUTPUT
                        string savedAudioDevice = key.GetValue("AudioOutput", "Default") as string;

                        if (savedAudioDevice == "Default")
                        {
                            cmbAudioOutput.SelectedIndex = 0;
                        }
                        else
                        {
                            for (int i = 0; i < cmbAudioOutput.Items.Count; i++)
                            {
                                string itemText = cmbAudioOutput.Items[i].ToString();
                                if (itemText.Contains(savedAudioDevice))
                                {
                                    cmbAudioOutput.SelectedIndex = i;
                                    break;
                                }
                            }
                        }

                        // ✅ LINGUA
                        string savedLanguage = key.GetValue("Language", "Italiano") as string;

                        for (int i = 0; i < _languageFiles.Count; i++)
                        {
                            if (_languageFiles[i] == savedLanguage)
                            {
                                cmbLanguage.SelectedIndex = i;
                                break;
                            }
                        }

                        Console.WriteLine($"[Settings] ✅ Impostazioni caricate dal Registry");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Settings] ❌ Errore caricamento impostazioni: {ex.Message}");
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(REGISTRY_PATH))
                {
                    if (key != null)
                    {
                        // ✅ SALVA AUDIO OUTPUT
                        string selectedAudio = cmbAudioOutput.SelectedIndex == 0
                            ? "Default"
                            : cmbAudioOutput.SelectedItem.ToString().Replace("🎧 ", "");

                        key.SetValue("AudioOutput", selectedAudio);

                        // ✅ SALVA LINGUA
                        string selectedLanguage = _languageFiles[cmbLanguage.SelectedIndex];
                        key.SetValue("Language", selectedLanguage);

                        PathMappingService.SaveMappings(_pathMappings ?? new List<PathMappingEntry>());

                        // ✅ APPLICA NUOVA LINGUA IMMEDIATAMENTE
                        LanguageManager.SetLanguage(selectedLanguage);

                        Console.WriteLine($"[Settings] ✅ Impostazioni salvate:");
                        Console.WriteLine($"  Audio Output: {selectedAudio}");
                        Console.WriteLine($"  Language: {selectedLanguage}");
                    }
                }

                MessageBox.Show(
                    LanguageManager.GetString("SettingsForm.Message.SaveSuccess"),
                    LanguageManager.GetString("SettingsForm.Title.SaveSuccess"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(LanguageManager.GetString("SettingsForm.Error.SaveFailed"), ex.Message),
                    LanguageManager.GetString("Common.Error"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void BtnAddPath_Click(object sender, EventArgs e)
        {
            var entry = ShowPathMappingDialog(new PathMappingEntry());
            if (entry == null) return;
            _pathMappings.Add(entry);
            RefreshPathMappingsGrid();
        }

        private void BtnEditPath_Click(object sender, EventArgs e)
        {
            if (dgvPaths.SelectedRows.Count != 1) return;
            int index = dgvPaths.SelectedRows[0].Index;
            if (index < 0 || index >= _pathMappings.Count) return;
            var edited = ShowPathMappingDialog(new PathMappingEntry
            {
                SourcePath = _pathMappings[index].SourcePath,
                TargetPath = _pathMappings[index].TargetPath
            });
            if (edited == null) return;
            _pathMappings[index] = edited;
            RefreshPathMappingsGrid();
        }

        private void BtnRemovePath_Click(object sender, EventArgs e)
        {
            if (dgvPaths.SelectedRows.Count != 1) return;
            int index = dgvPaths.SelectedRows[0].Index;
            if (index < 0 || index >= _pathMappings.Count) return;
            _pathMappings.RemoveAt(index);
            RefreshPathMappingsGrid();
        }

        private PathMappingEntry ShowPathMappingDialog(PathMappingEntry entry)
        {
            using (var form = new Form())
            {
                form.Text = LanguageManager.GetString("SettingsForm.Paths.DialogTitle", "Mappatura Percorso");
                form.StartPosition = FormStartPosition.CenterParent;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.MaximizeBox = false;
                form.MinimizeBox = false;
                form.ClientSize = new Size(560, 170);

                var lblSource = new Label { Left = 12, Top = 20, Width = 110, Text = LanguageManager.GetString("SettingsForm.Paths.Source", "Origine") };
                var txtSource = new TextBox { Left = 130, Top = 18, Width = 340, Text = entry.SourcePath ?? "" };
                var btnBrowseSource = new Button { Left = 480, Top = 18, Width = 65, Text = "..." };
                btnBrowseSource.Click += (s, e) => BrowseFolderInto(txtSource);

                var lblTarget = new Label { Left = 12, Top = 60, Width = 110, Text = LanguageManager.GetString("SettingsForm.Paths.Target", "Destinazione") };
                var txtTarget = new TextBox { Left = 130, Top = 58, Width = 340, Text = entry.TargetPath ?? "" };
                var btnBrowseTarget = new Button { Left = 480, Top = 58, Width = 65, Text = "..." };
                btnBrowseTarget.Click += (s, e) => BrowseFolderInto(txtTarget);

                var btnOk = new Button { Left = 390, Top = 110, Width = 75, Text = LanguageManager.GetString("Common.Save", "Salva"), DialogResult = DialogResult.OK };
                var btnCancelDialog = new Button { Left = 470, Top = 110, Width = 75, Text = LanguageManager.GetString("Common.Cancel", "Annulla"), DialogResult = DialogResult.Cancel };
                btnOk.Click += (s, e) =>
                {
                    if (string.IsNullOrWhiteSpace(txtSource.Text) || string.IsNullOrWhiteSpace(txtTarget.Text))
                    {
                        MessageBox.Show(
                            LanguageManager.GetString("SettingsForm.Paths.Validation", "Origine e destinazione sono obbligatorie."),
                            LanguageManager.GetString("Common.Warning", "Attenzione"),
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        form.DialogResult = DialogResult.None;
                        return;
                    }
                };

                form.Controls.AddRange(new Control[] { lblSource, txtSource, btnBrowseSource, lblTarget, txtTarget, btnBrowseTarget, btnOk, btnCancelDialog });
                form.AcceptButton = btnOk;
                form.CancelButton = btnCancelDialog;

                if (form.ShowDialog(this) != DialogResult.OK)
                    return null;

                return new PathMappingEntry
                {
                    SourcePath = txtSource.Text.Trim(),
                    TargetPath = txtTarget.Text.Trim()
                };
            }
        }

        private void BrowseFolderInto(TextBox textBox)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.SelectedPath = textBox.Text;
                if (fbd.ShowDialog(this) == DialogResult.OK)
                    textBox.Text = fbd.SelectedPath;
            }
        }

        /// <summary>
        /// ✅ METODO STATICO PER LEGGERE AUDIO OUTPUT SALVATO
        /// </summary>
        public static string GetSavedAudioOutput()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(REGISTRY_PATH))
                {
                    if (key != null)
                    {
                        return key.GetValue("AudioOutput", "Default") as string ?? "Default";
                    }
                }
            }
            catch
            {
                // Ignora errori
            }

            return "Default";
        }

        /// <summary>
        /// ✅ METODO STATICO PER OTTENERE DEVICE NUMBER DA USARE
        /// </summary>
        public static int GetAudioDeviceNumber()
        {
            try
            {
                string savedDevice = GetSavedAudioOutput();

                if (savedDevice == "Default")
                    return -1;

                int deviceCount = WaveOut.DeviceCount;
                for (int i = 0; i < deviceCount; i++)
                {
                    var capabilities = WaveOut.GetCapabilities(i);
                    if (capabilities.ProductName == savedDevice)
                    {
                        return i;
                    }
                }
            }
            catch
            {
                // Ignora errori
            }

            return -1;
        }

        /// <summary>
        /// ✅ METODO STATICO PER LEGGERE LINGUA SALVATA
        /// </summary>
        public static string GetSavedLanguage()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(REGISTRY_PATH))
                {
                    if (key != null)
                    {
                        return key.GetValue("Language", "Italiano") as string ?? "Italiano";
                    }
                }
            }
            catch
            {
                // Ignora errori
            }

            return "Italiano";
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
