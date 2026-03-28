using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using AirManager.Services;
using AirManager.Themes;

namespace AirManager.Forms
{
    public partial class StationExportImportForm : Form
    {
        private bool _isImportMode;
        private List<StationConfig> _stations;
        private List<StationConfig> _importedStations;

        private Panel headerPanel;
        private Label lblTitle;
        private Label lblSubtitle;

        private Label lblSelect;
        private Label lblFile;
        private CheckedListBox lstStations;
        private Button btnSelectAll;
        private Button btnDeselectAll;

        private CheckBox chkOverwriteExisting;
        private Label lblInfo;

        private Button btnBrowse;
        private TextBox txtFilePath;
        private Button btnExecute;
        private Button btnCancel;

        public StationExportImportForm(bool isImportMode)
        {
            InitializeComponent();
            _isImportMode = isImportMode;
            _stations = new List<StationConfig>();
            _importedStations = new List<StationConfig>();
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
            if (_isImportMode)
            {
                this.Text = "📥 " + LanguageManager.GetString("StationImportExport.Title.Import");
                lblTitle.Text = "📥 " + LanguageManager.GetString("StationImportExport.HeaderTitle.Import");

                // Se non ha ancora caricato file
                if (_importedStations == null || _importedStations.Count == 0)
                {
                    lblSubtitle.Text = LanguageManager.GetString("StationImportExport.Subtitle.Import");
                }
            }
            else
            {
                this.Text = "📤 " + LanguageManager.GetString("StationImportExport.Title.Export");
                lblTitle.Text = "📤 " + LanguageManager.GetString("StationImportExport.HeaderTitle.Export");
                lblSubtitle.Text = LanguageManager.GetString("StationImportExport.Subtitle.Export");
            }

            // ✅ LABELS
            if (lblSelect != null)
            {
                lblSelect.Text = _isImportMode
                    ? LanguageManager.GetString("StationImportExport.Label.StationsToImport")
                    : LanguageManager.GetString("StationImportExport.Label.StationsToExport");
            }

            if (lblFile != null)
            {
                lblFile.Text = LanguageManager.GetString("StationImportExport.Label.JSONFile");
            }

            // ✅ BOTTONI
            btnSelectAll.Text = "✓ " + LanguageManager.GetString("StationImportExport.SelectAll");
            btnDeselectAll.Text = "✖ " + LanguageManager.GetString("StationImportExport.DeselectAll");

            if (btnBrowse != null)
            {
                btnBrowse.Text = "📁 " + LanguageManager.GetString("StationImportExport.Browse");
            }

            btnExecute.Text = _isImportMode
                ? "📥 " + LanguageManager.GetString("StationImportExport.Import")
                : "📤 " + LanguageManager.GetString("StationImportExport.Export");

            btnCancel.Text = "✖ " + LanguageManager.GetString("Common.Cancel");

            // ✅ INFO LABEL
            if (lblInfo != null)
            {
                lblInfo.Text = "💡 " + LanguageManager.GetString("StationImportExport.Info.JSONFormat");
            }

            // ✅ CHECKBOX
            if (chkOverwriteExisting != null)
            {
                chkOverwriteExisting.Text = LanguageManager.GetString("StationImportExport.Option.OverwriteExisting");
            }

            Console.WriteLine($"[StationImportExport] ✅ Lingua applicata: {LanguageManager.CurrentLanguage}");
        }

        private void InitializeCustomUI()
        {
            this.Text = _isImportMode ? "📥 Importa Configurazioni Emittenti" : "📤 Esporta Configurazioni Emittenti";
            this.Size = new Size(700, 650);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = AppTheme.BgLight;

            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100,
                BackColor = AppTheme.BgDark
            };

            lblTitle = new Label
            {
                Text = _isImportMode ? "📥 IMPORTA EMITTENTI" : "📤 ESPORTA EMITTENTI",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 15),
                AutoSize = true
            };
            headerPanel.Controls.Add(lblTitle);

            lblSubtitle = new Label
            {
                Text = _isImportMode
                    ? "Seleziona un file JSON per importare le configurazioni"
                    : "Seleziona le emittenti da esportare",
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.FromArgb(180, 180, 180),
                Location = new Point(20, 50),
                AutoSize = true
            };
            headerPanel.Controls.Add(lblSubtitle);

            this.Controls.Add(headerPanel);

            if (_isImportMode)
            {
                CreateImportUI();
            }
            else
            {
                CreateExportUI();
            }

            btnCancel = new Button
            {
                Text = "✖ Annulla",
                Location = new Point(460, 560),
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

            btnExecute = new Button
            {
                Text = _isImportMode ? "📥 Importa" : "📤 Esporta",
                Location = new Point(580, 560),
                Size = new Size(110, 40),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnExecute.FlatAppearance.BorderSize = 0;
            btnExecute.Click += BtnExecute_Click;
            this.Controls.Add(btnExecute);

            this.CancelButton = btnCancel;
        }

        private void CreateExportUI()
        {
            lblSelect = new Label
            {
                Text = "Seleziona emittenti da esportare:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 120),
                AutoSize = true
            };
            this.Controls.Add(lblSelect);

            lstStations = new CheckedListBox
            {
                Location = new Point(20, 150),
                Size = new Size(650, 300),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10),
                CheckOnClick = true
            };
            this.Controls.Add(lstStations);

            btnSelectAll = new Button
            {
                Text = "✓ Seleziona Tutti",
                Location = new Point(20, 460),
                Size = new Size(140, 35),
                BackColor = Color.FromArgb(0, 150, 136),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSelectAll.FlatAppearance.BorderSize = 0;
            btnSelectAll.Click += (s, e) => SetAllChecked(true);
            this.Controls.Add(btnSelectAll);

            btnDeselectAll = new Button
            {
                Text = "✖ Deseleziona Tutti",
                Location = new Point(170, 460),
                Size = new Size(150, 35),
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnDeselectAll.FlatAppearance.BorderSize = 0;
            btnDeselectAll.Click += (s, e) => SetAllChecked(false);
            this.Controls.Add(btnDeselectAll);

            lblInfo = new Label
            {
                Text = "💡 Le configurazioni verranno salvate in formato JSON",
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = Color.FromArgb(180, 180, 180),
                Location = new Point(20, 510),
                AutoSize = true
            };
            this.Controls.Add(lblInfo);
        }

        private void CreateImportUI()
        {
            lblFile = new Label
            {
                Text = "File JSON:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 120),
                AutoSize = true
            };
            this.Controls.Add(lblFile);

            txtFilePath = new TextBox
            {
                Location = new Point(20, 150),
                Size = new Size(520, 30),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10),
                ReadOnly = true
            };
            this.Controls.Add(txtFilePath);

            btnBrowse = new Button
            {
                Text = "📁 Sfoglia",
                Location = new Point(550, 148),
                Size = new Size(120, 34),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnBrowse.FlatAppearance.BorderSize = 0;
            btnBrowse.Click += BtnBrowse_Click;
            this.Controls.Add(btnBrowse);

            lblSelect = new Label
            {
                Text = "Emittenti da importare:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 200),
                AutoSize = true
            };
            this.Controls.Add(lblSelect);

            lstStations = new CheckedListBox
            {
                Location = new Point(20, 230),
                Size = new Size(650, 220),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10),
                CheckOnClick = true,
                Enabled = false
            };
            this.Controls.Add(lstStations);

            btnSelectAll = new Button
            {
                Text = "✓ Seleziona Tutti",
                Location = new Point(20, 460),
                Size = new Size(140, 35),
                BackColor = Color.FromArgb(0, 150, 136),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Enabled = false
            };
            btnSelectAll.FlatAppearance.BorderSize = 0;
            btnSelectAll.Click += (s, e) => SetAllChecked(true);
            this.Controls.Add(btnSelectAll);

            btnDeselectAll = new Button
            {
                Text = "✖ Deseleziona Tutti",
                Location = new Point(170, 460),
                Size = new Size(150, 35),
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Enabled = false
            };
            btnDeselectAll.FlatAppearance.BorderSize = 0;
            btnDeselectAll.Click += (s, e) => SetAllChecked(false);
            this.Controls.Add(btnDeselectAll);

            chkOverwriteExisting = new CheckBox
            {
                Text = "Sovrascrivi emittenti esistenti (genera nuovi ID se deselezionato)",
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = Color.White,
                Location = new Point(20, 510),
                Size = new Size(500, 25),
                Checked = false
            };
            this.Controls.Add(chkOverwriteExisting);
        }

        private void LoadData()
        {
            if (!_isImportMode)
            {
                _stations = StationRegistry.LoadAllStations();

                lstStations.Items.Clear();
                foreach (var station in _stations)
                {
                    lstStations.Items.Add($"📻 {station.Name} ({station.DatabasePath})", true);
                }

                if (_stations.Count == 0)
                {
                    MessageBox.Show(
                        LanguageManager.GetString("StationImportExport.Message.NoStationsToExport"),
                        LanguageManager.GetString("StationImportExport.Title.NoStations"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    this.Close();
                }
            }
        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = LanguageManager.GetString("StationImportExport.Dialog.JSONFilter");
                ofd.Title = LanguageManager.GetString("StationImportExport.Dialog.SelectFile");

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtFilePath.Text = ofd.FileName;
                    LoadImportFile(ofd.FileName);
                }
            }
        }

        private void LoadImportFile(string filePath)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;

                string json = File.ReadAllText(filePath);
                _importedStations = Newtonsoft.Json.JsonConvert.DeserializeObject<List<StationConfig>>(json);

                if (_importedStations == null || _importedStations.Count == 0)
                {
                    MessageBox.Show(
                        LanguageManager.GetString("StationImportExport.Error.InvalidFile"),
                        LanguageManager.GetString("StationImportExport.Title.InvalidFile"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                lstStations.Items.Clear();
                lstStations.Enabled = true;
                btnSelectAll.Enabled = true;
                btnDeselectAll.Enabled = true;

                foreach (var station in _importedStations)
                {
                    string status = StationRegistry.LoadStation(station.Id) != null
                        ? " ⚠️ [" + LanguageManager.GetString("StationImportExport.Status.AlreadyExists") + "]"
                        : "";
                    lstStations.Items.Add($"📻 {station.Name} ({station.DatabasePath}){status}", true);
                }

                lblSubtitle.Text = string.Format(
                    LanguageManager.GetString("StationImportExport.Message.FoundStations"),
                    _importedStations.Count);
                lblSubtitle.ForeColor = AppTheme.LEDGreen;

                Console.WriteLine($"[StationExportImport] ✅ Caricate {_importedStations.Count} emittenti da {filePath}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(LanguageManager.GetString("StationImportExport.Error.ReadFile"), ex.Message),
                    LanguageManager.GetString("Common.Error"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                Console.WriteLine($"[StationExportImport] ❌ Errore import: {ex.Message}");
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void SetAllChecked(bool isChecked)
        {
            for (int i = 0; i < lstStations.Items.Count; i++)
            {
                lstStations.SetItemChecked(i, isChecked);
            }
        }

        private void BtnExecute_Click(object sender, EventArgs e)
        {
            if (_isImportMode)
            {
                ExecuteImport();
            }
            else
            {
                ExecuteExport();
            }
        }

        private void ExecuteExport()
        {
            if (lstStations.CheckedItems.Count == 0)
            {
                MessageBox.Show(
                    LanguageManager.GetString("StationImportExport.Validation.SelectStationToExport"),
                    LanguageManager.GetString("StationImportExport.Title.NoSelection"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "File JSON (*.json)|*.json";
                sfd.FileName = $"AirManager_Stations_{DateTime.Now:yyyy-MM-dd}.json";
                sfd.Title = LanguageManager.GetString("StationImportExport.Dialog.SaveTitle");

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        this.Cursor = Cursors.WaitCursor;

                        var selectedIds = new List<string>();
                        for (int i = 0; i < lstStations.CheckedItems.Count; i++)
                        {
                            int index = lstStations.CheckedIndices[i];
                            selectedIds.Add(_stations[index].Id);
                        }

                        string json = StationRegistry.ExportStations(selectedIds);

                        if (string.IsNullOrEmpty(json))
                        {
                            MessageBox.Show(
                                LanguageManager.GetString("StationImportExport.Error.ExportFailed"),
                                LanguageManager.GetString("Common.Error"),
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                            return;
                        }

                        File.WriteAllText(sfd.FileName, json);

                        MessageBox.Show(
                            string.Format(LanguageManager.GetString("StationImportExport.Message.ExportSuccess"),
                                selectedIds.Count, sfd.FileName),
                            LanguageManager.GetString("StationImportExport.Title.ExportComplete"),
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);

                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            string.Format(LanguageManager.GetString("StationImportExport.Error.Export"), ex.Message),
                            LanguageManager.GetString("Common.Error"),
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                    finally
                    {
                        this.Cursor = Cursors.Default;
                    }
                }
            }
        }

        private void ExecuteImport()
        {
            if (_importedStations == null || _importedStations.Count == 0)
            {
                MessageBox.Show(
                    LanguageManager.GetString("StationImportExport.Validation.NoFileLoaded"),
                    LanguageManager.GetString("StationImportExport.Title.FileMissing"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (lstStations.CheckedItems.Count == 0)
            {
                MessageBox.Show(
                    LanguageManager.GetString("StationImportExport.Validation.SelectStationToImport"),
                    LanguageManager.GetString("StationImportExport.Title.NoSelection"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            try
            {
                this.Cursor = Cursors.WaitCursor;

                var selectedStations = new List<StationConfig>();
                for (int i = 0; i < lstStations.CheckedItems.Count; i++)
                {
                    int index = lstStations.CheckedIndices[i];
                    selectedStations.Add(_importedStations[index]);
                }

                int imported = 0;
                int skipped = 0;

                foreach (var station in selectedStations)
                {
                    var existing = StationRegistry.LoadStation(station.Id);

                    if (existing != null && !chkOverwriteExisting.Checked)
                    {
                        station.Id = Guid.NewGuid().ToString();
                        Console.WriteLine($"[StationExportImport] ⚠️ Emittente '{station.Name}' già esiste, generato nuovo ID: {station.Id}");
                    }

                    if (StationRegistry.SaveStation(station))
                    {
                        imported++;
                    }
                    else
                    {
                        skipped++;
                    }
                }

                string message = string.Format(
                    LanguageManager.GetString("StationImportExport.Message.ImportComplete"),
                    imported, skipped);

                if (imported > 0)
                {
                    message += "\n\n" + LanguageManager.GetString("StationImportExport.Message.StationsAvailable");
                }

                MessageBox.Show(
                    message,
                    LanguageManager.GetString("StationImportExport.Title.ImportComplete"),
                    MessageBoxButtons.OK,
                    imported > 0 ? MessageBoxIcon.Information : MessageBoxIcon.Warning);

                if (imported > 0)
                {
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(LanguageManager.GetString("StationImportExport.Error.Import"), ex.Message),
                    LanguageManager.GetString("Common.Error"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // ✅ DISOTTOSCRIVI EVENTO
            LanguageManager.LanguageChanged -= (s, e2) => ApplyLanguage();

            base.OnFormClosing(e);
        }
    }
}