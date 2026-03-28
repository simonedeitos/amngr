using AirManager.Forms;
using AirManager.Services;
using AirManager.Services.Database;
using AirManager.Themes;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace AirManager.Controls
{
    public partial class ArchiveControl : UserControl
    {
        private string _archiveType;
        private DataGridView dgvArchive;
        private Panel headerPanel;
        private Label lblHeader;
        private TextBox txtSearch;
        private ComboBox cmbGenreFilter;
        private ComboBox cmbCategoryFilter;
        private Button btnClearFilters;
        private Button btnZoomIn;
        private Button btnZoomOut;
        private Button btnImport;
        private Button btnEdit;
        private Button btnDelete;
        private Button btnRefresh;
        private List<MusicEntry> _allMusicData;
        private List<ClipEntry> _allClipsData;
        private float _currentFontSize = 10F;

        private Panel pnlMiniPlayer;
        private Button btnPlayStop;
        private TrackBar trackPosition;
        private Label lblTimeCounter;
        private Label lblPreviewTitle;
        private WaveOutEvent _previewPlayer;
        private AudioFileReader _previewAudioFile;
        private System.Windows.Forms.Timer _previewTimer;
        private bool _isPreviewPlaying = false;
        private bool _userIsDraggingSlider = false;

        private int _previewSessionCounter = 0;
        private readonly object _previewLock = new object();

        // ✅ CONTEXT MENU ITEMS (per traduzione)
        private ToolStripMenuItem menuItemPreview;
        private ToolStripMenuItem menuItemBatchEdit;
        private ToolStripMenuItem menuItemCopyToStations;
        private ToolStripMenuItem menuItemShowFolder;
        private ToolStripMenuItem menuItemExportFiles;

        public event EventHandler<string> StatusChanged;

        public ArchiveControl(string archiveType)
        {
            InitializeComponent();
            _archiveType = archiveType;
            _allMusicData = new List<MusicEntry>();
            _allClipsData = new List<ClipEntry>();
            InitializeUI();
            InitializeMiniPlayer();
            ApplyLanguage(); // ✅ APPLICA LINGUA INIZIALE
            RefreshArchive();

            // ✅ SOTTOSCRIVI EVENTO CAMBIO LINGUA
            LanguageManager.LanguageChanged += (s, e) => ApplyLanguage();
        }

        /// <summary>
        /// ✅ APPLICA LE TRADUZIONI DA LanguageManager
        /// </summary>
        private void ApplyLanguage()
        {
            // ✅ HEADER
            UpdateHeaderCount(_archiveType == "Musica" ? _allMusicData.Count : _allClipsData.Count);

            // ✅ BOTTONI HEADER
            btnImport.Text = "📥 " + LanguageManager.GetString("Archive.Import");
            btnEdit.Text = "✏️ " + LanguageManager.GetString("Common.Edit");
            btnDelete.Text = "🗑️ " + LanguageManager.GetString("Common.Delete");

            // ✅ SEARCH & FILTERS
            txtSearch.PlaceholderText = "🔍 " + LanguageManager.GetString("Archive.SearchPlaceholder");

            string allGenresText = LanguageManager.GetString("Archive.Filter.AllGenres");
            string allCategoriesText = LanguageManager.GetString("Archive.Filter.AllCategories");

            // Mantieni selezione corrente
            string currentGenre = cmbGenreFilter.SelectedItem?.ToString();
            string currentCategory = cmbCategoryFilter.SelectedItem?.ToString();

            // Aggiorna primo item
            if (cmbGenreFilter.Items.Count > 0)
                cmbGenreFilter.Items[0] = allGenresText;

            if (cmbCategoryFilter.Items.Count > 0)
                cmbCategoryFilter.Items[0] = allCategoriesText;

            // Ripristina selezione
            if (currentGenre == "Tutti i Generi" || currentGenre == "All Genres")
                cmbGenreFilter.SelectedIndex = 0;

            if (currentCategory == "Tutte le Categorie" || currentCategory == "All Categories")
                cmbCategoryFilter.SelectedIndex = 0;

            btnClearFilters.Text = "✖ " + LanguageManager.GetString("Archive.ResetFilters");

            // ✅ DATAGRIDVIEW HEADERS
            if (_archiveType == "Musica")
            {
                dgvArchive.Columns["Artist"].HeaderText = LanguageManager.GetString("Archive.Column.Artist");
                dgvArchive.Columns["Title"].HeaderText = LanguageManager.GetString("Archive.Column.Title");
                dgvArchive.Columns["Genre"].HeaderText = LanguageManager.GetString("Archive.Column.Genre");
                dgvArchive.Columns["Year"].HeaderText = LanguageManager.GetString("Archive.Column.Year");
                dgvArchive.Columns["Duration"].HeaderText = LanguageManager.GetString("Archive.Column.Duration");
                dgvArchive.Columns["Intro"].HeaderText = LanguageManager.GetString("Archive.Column.Intro");
                dgvArchive.Columns["Category"].HeaderText = LanguageManager.GetString("Archive.Column.Category");
                dgvArchive.Columns["AddedDate"].HeaderText = LanguageManager.GetString("Archive.Column.Added");
            }
            else
            {
                dgvArchive.Columns["Title"].HeaderText = LanguageManager.GetString("Archive.Column.Title");
                dgvArchive.Columns["Genre"].HeaderText = LanguageManager.GetString("Archive.Column.Genre");
                dgvArchive.Columns["Duration"].HeaderText = LanguageManager.GetString("Archive.Column.Duration");
                dgvArchive.Columns["Intro"].HeaderText = LanguageManager.GetString("Archive.Column.Intro");
                dgvArchive.Columns["Category"].HeaderText = LanguageManager.GetString("Archive.Column.Category");
                dgvArchive.Columns["AddedDate"].HeaderText = LanguageManager.GetString("Archive.Column.Added");
            }

            // ✅ CONTEXT MENU
            menuItemPreview.Text = "🎧 " + LanguageManager.GetString("Archive.ContextMenu.Preview");
            menuItemBatchEdit.Text = "✏️ " + LanguageManager.GetString("Archive.ContextMenu.BatchEdit");
            menuItemCopyToStations.Text = "📋 " + LanguageManager.GetString("Archive.ContextMenu.CopyToStations");
            menuItemShowFolder.Text = "📁 " + LanguageManager.GetString("Archive.ContextMenu.ShowFolder");
            menuItemExportFiles.Text = "📤 " + LanguageManager.GetString("Archive.ContextMenu.ExportFiles");

            // ✅ MINIPLAYER
            if (!_isPreviewPlaying)
            {
                lblPreviewTitle.Text = LanguageManager.GetString("Archive.MiniPlayer.NoFileLoaded");
            }

            Console.WriteLine($"[ArchiveControl] ✅ Lingua applicata: {LanguageManager.CurrentLanguage}");
        }

        private double NormalizeDuration(double duration)
        {
            if (duration > 1000)
            {
                return duration / 1000.0;
            }
            return duration;
        }

        private string FormatDuration(double duration)
        {
            double seconds = NormalizeDuration(duration);
            int minutes = (int)(seconds / 60);
            int secs = (int)(seconds % 60);
            return $"{minutes: 00}:{secs:00}";
        }

        private void InitializeUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = AppTheme.BgLight;
            this.Padding = new Padding(0);

            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = AppTheme.BgDark,
                Padding = new Padding(15, 10, 15, 10)
            };

            lblHeader = new Label
            {
                Text = $"📂 ARCHIVIO {_archiveType.ToUpper()} • 0 elementi",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(15, 12),
                AutoSize = true
            };
            headerPanel.Controls.Add(lblHeader);

            btnImport = new Button
            {
                Text = "📥 Importa",
                Location = new Point(15, 45),
                Size = new Size(110, 30),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnImport.FlatAppearance.BorderSize = 0;
            btnImport.Click += BtnImport_Click;
            headerPanel.Controls.Add(btnImport);

            btnEdit = new Button
            {
                Text = "✏️ Modifica",
                Location = new Point(135, 45),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(255, 140, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnEdit.FlatAppearance.BorderSize = 0;
            btnEdit.Click += BtnEdit_Click;
            headerPanel.Controls.Add(btnEdit);

            btnDelete = new Button
            {
                Text = "🗑️ Elimina",
                Location = new Point(245, 45),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnDelete.FlatAppearance.BorderSize = 0;
            btnDelete.Click += BtnDelete_Click;
            headerPanel.Controls.Add(btnDelete);

            btnRefresh = new Button
            {
                Text = "🔄",
                Location = new Point(355, 45),
                Size = new Size(40, 30),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12),
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (s, e) => RefreshArchive();
            headerPanel.Controls.Add(btnRefresh);

            txtSearch = new TextBox
            {
                PlaceholderText = "🔍 Cerca artista o titolo...",
                Font = new Font("Segoe UI", 10),
                Size = new Size(300, 25),
                BackColor = AppTheme.BgInput,
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            txtSearch.TextChanged += (s, e) => ApplyFilters();
            headerPanel.Controls.Add(txtSearch);

            btnZoomOut = new Button
            {
                Text = "➖ A",
                Size = new Size(45, 25),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnZoomOut.FlatAppearance.BorderSize = 0;
            btnZoomOut.Click += (s, e) =>
            {
                if (_currentFontSize > 8)
                {
                    _currentFontSize -= 1;
                    UpdateFontSize();
                }
            };
            headerPanel.Controls.Add(btnZoomOut);

            btnZoomIn = new Button
            {
                Text = "➕ A",
                Size = new Size(45, 25),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnZoomIn.FlatAppearance.BorderSize = 0;
            btnZoomIn.Click += (s, e) =>
            {
                if (_currentFontSize < 16)
                {
                    _currentFontSize += 1;
                    UpdateFontSize();
                }
            };
            headerPanel.Controls.Add(btnZoomIn);

            cmbGenreFilter = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9),
                Size = new Size(130, 25),
                BackColor = AppTheme.BgInput,
                ForeColor = Color.White
            };
            cmbGenreFilter.Items.Add("Tutti i Generi");
            cmbGenreFilter.SelectedIndex = 0;
            cmbGenreFilter.SelectedIndexChanged += (s, e) => ApplyFilters();
            headerPanel.Controls.Add(cmbGenreFilter);

            cmbCategoryFilter = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9),
                Size = new Size(130, 25),
                BackColor = AppTheme.BgInput,
                ForeColor = Color.White
            };
            cmbCategoryFilter.Items.Add("Tutte le Categorie");
            cmbCategoryFilter.SelectedIndex = 0;
            cmbCategoryFilter.SelectedIndexChanged += (s, e) => ApplyFilters();
            headerPanel.Controls.Add(cmbCategoryFilter);

            btnClearFilters = new Button
            {
                Text = "✖ Reset",
                Size = new Size(70, 25),
                BackColor = Color.Gray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnClearFilters.FlatAppearance.BorderSize = 0;
            btnClearFilters.Click += (s, e) =>
            {
                txtSearch.Text = "";
                cmbGenreFilter.SelectedIndex = 0;
                cmbCategoryFilter.SelectedIndex = 0;
            };
            headerPanel.Controls.Add(btnClearFilters);

            headerPanel.Resize += (s, e) => RepositionHeaderControls();
            RepositionHeaderControls();

            this.Controls.Add(headerPanel);

            dgvArchive = new DataGridView
            {
                Location = new Point(0, 80),
                Size = new Size(this.Width, this.Height - 80),
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
                Font = new Font("Segoe UI", _currentFontSize),
                AllowUserToResizeRows = false,
                ScrollBars = ScrollBars.Both
            };

            dgvArchive.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(50, 50, 50);
            dgvArchive.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvArchive.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvArchive.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvArchive.ColumnHeadersDefaultCellStyle.Padding = new Padding(5);

            dgvArchive.DefaultCellStyle.BackColor = Color.FromArgb(40, 40, 40);
            dgvArchive.DefaultCellStyle.ForeColor = Color.White;
            dgvArchive.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 215);
            dgvArchive.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvArchive.DefaultCellStyle.Padding = new Padding(5);

            dgvArchive.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(35, 35, 35);
            dgvArchive.EnableHeadersVisualStyles = false;

            if (_archiveType == "Musica")
            {
                dgvArchive.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "Artist",
                    HeaderText = "Artista",
                    FillWeight = 20,
                    DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft }
                });

                dgvArchive.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "Title",
                    HeaderText = "Titolo",
                    FillWeight = 30,
                    DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft }
                });

                dgvArchive.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "Genre",
                    HeaderText = "Genere",
                    FillWeight = 12,
                    DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
                });

                dgvArchive.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "Year",
                    HeaderText = "Anno",
                    FillWeight = 8,
                    DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
                });

                dgvArchive.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "Duration",
                    HeaderText = "Durata",
                    FillWeight = 8,
                    DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
                });

                dgvArchive.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "Intro",
                    HeaderText = "Intro",
                    FillWeight = 7,
                    DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
                });

                dgvArchive.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "Category",
                    HeaderText = "Categoria",
                    FillWeight = 12,
                    DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
                });

                dgvArchive.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "AddedDate",
                    HeaderText = "Aggiunto",
                    FillWeight = 10,
                    DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
                });
            }
            else
            {
                dgvArchive.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "Title",
                    HeaderText = "Titolo",
                    FillWeight = 35,
                    DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft }
                });

                dgvArchive.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "Genre",
                    HeaderText = "Genere",
                    FillWeight = 15,
                    DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
                });

                dgvArchive.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "Duration",
                    HeaderText = "Durata",
                    FillWeight = 10,
                    DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
                });

                dgvArchive.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "Intro",
                    HeaderText = "Intro",
                    FillWeight = 8,
                    DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
                });

                dgvArchive.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "Category",
                    HeaderText = "Categoria",
                    FillWeight = 15,
                    DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
                });

                dgvArchive.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "AddedDate",
                    HeaderText = "Aggiunto",
                    FillWeight = 12,
                    DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
                });
            }

            // ✅ CONTEXT MENU CON RIFERIMENTI GLOBALI
            ContextMenuStrip contextMenu = new ContextMenuStrip();

            menuItemPreview = new ToolStripMenuItem("🎧 Preascolto", null, MenuPreview_Click);
            contextMenu.Items.Add(menuItemPreview);
            contextMenu.Items.Add(new ToolStripSeparator());

            menuItemBatchEdit = new ToolStripMenuItem("✏️ Modifica Genere/Categoria", null, MenuBatchEdit_Click);
            contextMenu.Items.Add(menuItemBatchEdit);

            contextMenu.Items.Add(new ToolStripSeparator());

            menuItemCopyToStations = new ToolStripMenuItem("📋 Copia su altre emittenti...", null, MenuCopyToStations_Click);
            contextMenu.Items.Add(menuItemCopyToStations);

            contextMenu.Items.Add(new ToolStripSeparator());

            menuItemShowFolder = new ToolStripMenuItem("📁 Mostra cartella file", null, MenuShowFolder_Click);
            contextMenu.Items.Add(menuItemShowFolder);

            menuItemExportFiles = new ToolStripMenuItem("📤 Esporta file selezionati", null, MenuExportFiles_Click);
            contextMenu.Items.Add(menuItemExportFiles);

            dgvArchive.ContextMenuStrip = contextMenu;

            dgvArchive.CellDoubleClick += DgvArchive_CellDoubleClick;
            dgvArchive.KeyDown += DgvArchive_KeyDown;

            this.Controls.Add(dgvArchive);
        }

        private void RepositionHeaderControls()
        {
            const int MARGIN = 15;
            int panelWidth = headerPanel.Width;

            int x1 = panelWidth - btnZoomIn.Width - MARGIN;
            btnZoomIn.Location = new Point(x1, 14);

            x1 -= (btnZoomOut.Width + 5);
            btnZoomOut.Location = new Point(x1, 14);

            x1 -= (txtSearch.Width + 15);
            txtSearch.Location = new Point(x1, 12);

            int x2 = panelWidth - btnClearFilters.Width - MARGIN;
            btnClearFilters.Location = new Point(x2, 48);

            x2 -= (cmbCategoryFilter.Width + 10);
            cmbCategoryFilter.Location = new Point(x2, 48);

            x2 -= (cmbGenreFilter.Width + 10);
            cmbGenreFilter.Location = new Point(x2, 48);
        }

        private void UpdateFontSize()
        {
            dgvArchive.Font = new Font("Segoe UI", _currentFontSize);
            dgvArchive.RowTemplate.Height = (int)(_currentFontSize * 3.5);
            dgvArchive.Refresh();
        }

        private void UpdateHeaderCount(int count)
        {
            string archiveTypeTranslated = _archiveType == "Musica"
                ? LanguageManager.GetString("Archive.Type.Music")
                : LanguageManager.GetString("Archive.Type.Clips");

            lblHeader.Text = string.Format(
                LanguageManager.GetString("Archive.Header.Format"),
                archiveTypeTranslated.ToUpper(),
                count
            );
            lblHeader.ForeColor = count > 0 ? Color.White : Color.Gray;
        }

        private void MenuPreview_Click(object sender, EventArgs e)
        {
            if (dgvArchive.SelectedRows.Count == 0) return;

            DataGridViewRow selectedRow = dgvArchive.SelectedRows[0];
            object entryData = selectedRow.Tag;

            string filePath = "";
            string title = "";

            if (entryData is MusicEntry musicEntry)
            {
                filePath = musicEntry.FilePath;
                title = $"{musicEntry.Artist} - {musicEntry.Title}";
            }
            else if (entryData is ClipEntry clipEntry)
            {
                filePath = clipEntry.FilePath;
                title = clipEntry.Title;
            }

            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                StartPreviewFromExternal(filePath, title);
            }
            else
            {
                MessageBox.Show(
                    LanguageManager.GetString("Archive.Error.FileNotFound"),
                    LanguageManager.GetString("Common.Error"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void MenuBatchEdit_Click(object sender, EventArgs e)
        {
            if (dgvArchive.SelectedRows.Count == 0)
            {
                MessageBox.Show(
                    LanguageManager.GetString("Archive.Validation.SelectAtLeastOne"),
                    LanguageManager.GetString("Common.Warning"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            using (var batchForm = new BatchEditForm(_archiveType))
            {
                if (batchForm.ShowDialog() == DialogResult.OK)
                {
                    ApplyBatchEdit(batchForm.ModifyGenre, batchForm.NewGenre, batchForm.ModifyCategory, batchForm.NewCategory);
                }
            }
        }

        private void ApplyBatchEdit(bool modifyGenre, string newGenre, bool modifyCategory, string newCategory)
        {
            if (!modifyGenre && !modifyCategory)
            {
                MessageBox.Show(
                    LanguageManager.GetString("Archive.Validation.NoModification"),
                    LanguageManager.GetString("Common.Warning"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            this.Cursor = Cursors.WaitCursor;
            int updated = 0;
            int errors = 0;

            try
            {
                foreach (DataGridViewRow row in dgvArchive.SelectedRows)
                {
                    try
                    {
                        if (_archiveType == "Musica" && row.Tag is MusicEntry musicEntry)
                        {
                            bool changed = false;

                            if (modifyGenre && !string.IsNullOrWhiteSpace(newGenre))
                            {
                                if (UpdateGenreInFile(musicEntry.FilePath, newGenre))
                                {
                                    musicEntry.Genre = newGenre;
                                    changed = true;
                                }
                            }

                            if (modifyCategory && !string.IsNullOrWhiteSpace(newCategory))
                            {
                                musicEntry.Categories = newCategory;
                                changed = true;
                            }

                            if (changed && DbcManager.Update("Music.dbc", musicEntry))
                            {
                                updated++;
                            }
                        }
                        else if (_archiveType == "Clips" && row.Tag is ClipEntry clipEntry)
                        {
                            bool changed = false;

                            if (modifyGenre && !string.IsNullOrWhiteSpace(newGenre))
                            {
                                if (UpdateGenreInFile(clipEntry.FilePath, newGenre))
                                {
                                    clipEntry.Genre = newGenre;
                                    changed = true;
                                }
                            }

                            if (modifyCategory && !string.IsNullOrWhiteSpace(newCategory))
                            {
                                clipEntry.Categories = newCategory;
                                changed = true;
                            }

                            if (changed && DbcManager.Update("Clips.dbc", clipEntry))
                            {
                                updated++;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[BatchEdit] Errore: {ex.Message}");
                        errors++;
                    }
                }

                RefreshArchive();

                MessageBox.Show(
                    string.Format(LanguageManager.GetString("Archive.Message.BatchEditComplete"), updated, errors),
                    LanguageManager.GetString("Archive.Title.BatchEditComplete"),
                    MessageBoxButtons.OK,
                    errors > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private bool UpdateGenreInFile(string filePath, string newGenre)
        {
            try
            {
                if (!File.Exists(filePath))
                    return false;

                var tagFile = TagLib.File.Create(filePath);
                tagFile.Tag.Genres = new[] { newGenre };
                tagFile.Save();
                tagFile.Dispose();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UpdateGenre] ❌ Errore:  {ex.Message}");
                return false;
            }
        }

        // ✅ COPY TO OTHER STATIONS

        private void MenuCopyToStations_Click(object sender, EventArgs e)
        {
            if (dgvArchive.SelectedRows.Count == 0)
            {
                MessageBox.Show(
                    LanguageManager.GetString("Archive.Validation.SelectAtLeastOne"),
                    LanguageManager.GetString("Common.Warning"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            // Collect selected entry IDs
            var selectedIds = new List<int>();
            foreach (DataGridViewRow row in dgvArchive.SelectedRows)
            {
                if (row.Tag is MusicEntry musicEntry)
                    selectedIds.Add(musicEntry.ID);
                else if (row.Tag is ClipEntry clipEntry)
                    selectedIds.Add(clipEntry.ID);
            }

            if (selectedIds.Count == 0) return;

            using (var copyForm = new CopyToStationsForm(selectedIds, _archiveType))
            {
                if (copyForm.ShowDialog() == DialogResult.OK)
                {
                    PerformCopyToStations(copyForm);
                }
            }
        }

        private void PerformCopyToStations(CopyToStationsForm copyForm)
        {
            this.Cursor = Cursors.WaitCursor;
            int totalCopied = 0;
            int totalErrors = 0;
            string dbcFileName = _archiveType == "Musica" ? "Music.dbc" : "Clips.dbc";

            try
            {
                // Load source entries
                var sourceEntries = new List<object>();
                foreach (DataGridViewRow row in dgvArchive.SelectedRows)
                {
                    if (row.Tag != null)
                        sourceEntries.Add(row.Tag);
                }

                foreach (string stationId in copyForm.SelectedStationIds)
                {
                    try
                    {
                        var station = StationRegistry.LoadStation(stationId);
                        if (station == null || string.IsNullOrEmpty(station.DatabasePath))
                        {
                            Console.WriteLine($"[CopyToStations] ⚠️ Stazione non trovata o senza path: {stationId}");
                            totalErrors++;
                            continue;
                        }

                        // Ensure target database directory exists
                        if (!Directory.Exists(station.DatabasePath))
                        {
                            Directory.CreateDirectory(station.DatabasePath);
                        }

                        if (_archiveType == "Musica")
                        {
                            totalCopied += CopyMusicEntries(sourceEntries, station.DatabasePath, dbcFileName, copyForm);
                        }
                        else
                        {
                            totalCopied += CopyClipEntries(sourceEntries, station.DatabasePath, dbcFileName, copyForm);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[CopyToStations] ❌ Errore stazione {stationId}: {ex.Message}");
                        totalErrors++;
                    }
                }

                MessageBox.Show(
                    string.Format(LanguageManager.GetString("Archive.Message.CopyComplete"),
                        totalCopied, copyForm.SelectedStationIds.Count, totalErrors),
                    LanguageManager.GetString("Archive.Title.CopyComplete"),
                    MessageBoxButtons.OK,
                    totalErrors > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);

                StatusChanged?.Invoke(this, string.Format(
                    LanguageManager.GetString("Archive.Status.Copied"),
                    totalCopied, copyForm.SelectedStationIds.Count));
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private int CopyMusicEntries(List<object> sourceEntries, string targetDbPath, string dbcFileName, CopyToStationsForm copyForm)
        {
            var targetEntries = DbcManager.LoadFromCsvPath<MusicEntry>(targetDbPath, dbcFileName);
            int copied = 0;

            foreach (var entry in sourceEntries)
            {
                if (entry is not MusicEntry source) continue;

                // Find existing entry by FilePath in target
                var existing = targetEntries.FirstOrDefault(t => t.FilePath == source.FilePath);

                if (existing != null)
                {
                    // Update only selected fields
                    if (copyForm.CopyGenre) existing.Genre = source.Genre;
                    if (copyForm.CopyCategories) existing.Categories = source.Categories;
                    if (copyForm.CopyMarkers)
                    {
                        existing.MarkerIN = source.MarkerIN;
                        existing.MarkerINTRO = source.MarkerINTRO;
                        existing.MarkerMIX = source.MarkerMIX;
                        existing.MarkerOUT = source.MarkerOUT;
                    }
                    if (copyForm.CopyHours) existing.ValidHours = source.ValidHours;
                    if (copyForm.CopyDays) existing.ValidDays = source.ValidDays;
                    if (copyForm.CopyMonths) existing.ValidMonths = source.ValidMonths;
                }
                else
                {
                    // Create new entry with all base data + selected fields
                    var newEntry = new MusicEntry
                    {
                        FilePath = source.FilePath,
                        Artist = source.Artist,
                        Title = source.Title,
                        Album = source.Album,
                        Year = source.Year,
                        Duration = source.Duration,
                        FileSize = source.FileSize,
                        Format = source.Format,
                        Bitrate = source.Bitrate,
                        SampleRate = source.SampleRate,
                        Channels = source.Channels,
                        AddedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    };

                    if (copyForm.CopyGenre) newEntry.Genre = source.Genre;
                    if (copyForm.CopyCategories) newEntry.Categories = source.Categories;
                    if (copyForm.CopyMarkers)
                    {
                        newEntry.MarkerIN = source.MarkerIN;
                        newEntry.MarkerINTRO = source.MarkerINTRO;
                        newEntry.MarkerMIX = source.MarkerMIX;
                        newEntry.MarkerOUT = source.MarkerOUT;
                    }
                    if (copyForm.CopyHours) newEntry.ValidHours = source.ValidHours;
                    if (copyForm.CopyDays) newEntry.ValidDays = source.ValidDays;
                    if (copyForm.CopyMonths) newEntry.ValidMonths = source.ValidMonths;

                    // Assign new ID
                    int newId = targetEntries.Count > 0 ? targetEntries.Max(x => x.ID) + 1 : 1;
                    newEntry.ID = newId;
                    targetEntries.Add(newEntry);
                }

                copied++;
            }

            DbcManager.SaveToCsvPath(targetDbPath, dbcFileName, targetEntries);
            return copied;
        }

        private int CopyClipEntries(List<object> sourceEntries, string targetDbPath, string dbcFileName, CopyToStationsForm copyForm)
        {
            var targetEntries = DbcManager.LoadFromCsvPath<ClipEntry>(targetDbPath, dbcFileName);
            int copied = 0;

            foreach (var entry in sourceEntries)
            {
                if (entry is not ClipEntry source) continue;

                var existing = targetEntries.FirstOrDefault(t => t.FilePath == source.FilePath);

                if (existing != null)
                {
                    if (copyForm.CopyGenre) existing.Genre = source.Genre;
                    if (copyForm.CopyCategories) existing.Categories = source.Categories;
                    if (copyForm.CopyMarkers)
                    {
                        existing.MarkerIN = source.MarkerIN;
                        existing.MarkerINTRO = source.MarkerINTRO;
                        existing.MarkerMIX = source.MarkerMIX;
                        existing.MarkerOUT = source.MarkerOUT;
                    }
                    if (copyForm.CopyHours) existing.ValidHours = source.ValidHours;
                    if (copyForm.CopyDays) existing.ValidDays = source.ValidDays;
                    if (copyForm.CopyMonths) existing.ValidMonths = source.ValidMonths;
                }
                else
                {
                    var newEntry = new ClipEntry
                    {
                        FilePath = source.FilePath,
                        Title = source.Title,
                        Duration = source.Duration,
                        AddedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    };

                    if (copyForm.CopyGenre) newEntry.Genre = source.Genre;
                    if (copyForm.CopyCategories) newEntry.Categories = source.Categories;
                    if (copyForm.CopyMarkers)
                    {
                        newEntry.MarkerIN = source.MarkerIN;
                        newEntry.MarkerINTRO = source.MarkerINTRO;
                        newEntry.MarkerMIX = source.MarkerMIX;
                        newEntry.MarkerOUT = source.MarkerOUT;
                    }
                    if (copyForm.CopyHours) newEntry.ValidHours = source.ValidHours;
                    if (copyForm.CopyDays) newEntry.ValidDays = source.ValidDays;
                    if (copyForm.CopyMonths) newEntry.ValidMonths = source.ValidMonths;

                    int newId = targetEntries.Count > 0 ? targetEntries.Max(x => x.ID) + 1 : 1;
                    newEntry.ID = newId;
                    targetEntries.Add(newEntry);
                }

                copied++;
            }

            DbcManager.SaveToCsvPath(targetDbPath, dbcFileName, targetEntries);
            return copied;
        }

        // ✅ SHOW FILE FOLDER

        private void MenuShowFolder_Click(object sender, EventArgs e)
        {
            if (dgvArchive.SelectedRows.Count == 0) return;

            DataGridViewRow selectedRow = dgvArchive.SelectedRows[0];
            string filePath = "";

            if (selectedRow.Tag is MusicEntry musicEntry)
                filePath = musicEntry.FilePath;
            else if (selectedRow.Tag is ClipEntry clipEntry)
                filePath = clipEntry.FilePath;

            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                string folder = Path.GetDirectoryName(filePath);
                if (Directory.Exists(folder))
                {
                    System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{filePath}\"");
                }
            }
            else
            {
                MessageBox.Show(
                    LanguageManager.GetString("Archive.Error.FileNotFound"),
                    LanguageManager.GetString("Common.Error"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // ✅ EXPORT SELECTED FILES

        private void MenuExportFiles_Click(object sender, EventArgs e)
        {
            if (dgvArchive.SelectedRows.Count == 0)
            {
                MessageBox.Show(
                    LanguageManager.GetString("Archive.Validation.SelectAtLeastOne"),
                    LanguageManager.GetString("Common.Warning"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            using (var fbd = new FolderBrowserDialog())
            {
                fbd.Description = LanguageManager.GetString("Archive.Export.SelectFolder");

                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    int exported = 0;
                    int errors = 0;

                    foreach (DataGridViewRow row in dgvArchive.SelectedRows)
                    {
                        string filePath = "";

                        if (row.Tag is MusicEntry musicEntry)
                            filePath = musicEntry.FilePath;
                        else if (row.Tag is ClipEntry clipEntry)
                            filePath = clipEntry.FilePath;

                        if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                        {
                            try
                            {
                                string destPath = Path.Combine(fbd.SelectedPath, Path.GetFileName(filePath));
                                File.Copy(filePath, destPath, true);
                                exported++;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"[ExportFiles] ❌ Errore: {ex.Message}");
                                errors++;
                            }
                        }
                        else
                        {
                            errors++;
                        }
                    }

                    MessageBox.Show(
                        string.Format(LanguageManager.GetString("Archive.Message.ExportComplete"), exported, errors),
                        LanguageManager.GetString("Archive.Title.ExportComplete"),
                        MessageBoxButtons.OK,
                        errors > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
                }
            }
        }

        // ✅ MINI PLAYER

        private void InitializeMiniPlayer()
        {
            Panel spacerPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 25,
                BackColor = AppTheme.BgLight
            };
            this.Controls.Add(spacerPanel);

            pnlMiniPlayer = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 70,
                BackColor = Color.FromArgb(30, 30, 30),
                Visible = false
            };
            this.Controls.Add(pnlMiniPlayer);
            pnlMiniPlayer.BringToFront();

            this.Controls.SetChildIndex(spacerPanel, 0);
            this.Controls.SetChildIndex(pnlMiniPlayer, 1);

            lblPreviewTitle = new Label
            {
                Text = "Nessun file in preascolto",
                Location = new Point(10, 8),
                Size = new Size(400, 20),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White
            };
            pnlMiniPlayer.Controls.Add(lblPreviewTitle);

            btnPlayStop = new Button
            {
                Text = "▶",
                Location = new Point(10, 32),
                Size = new Size(50, 30),
                BackColor = Color.FromArgb(0, 200, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnPlayStop.FlatAppearance.BorderSize = 0;
            btnPlayStop.Click += BtnPlayStop_Click;
            pnlMiniPlayer.Controls.Add(btnPlayStop);

            trackPosition = new TrackBar
            {
                Location = new Point(70, 32),
                Size = new Size(pnlMiniPlayer.Width - 250, 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Minimum = 0,
                Maximum = 1000,
                TickFrequency = 50,
                TickStyle = TickStyle.None,
                SmallChange = 1,
                LargeChange = 10
            };

            trackPosition.MouseDown += TrackPosition_MouseDown;
            trackPosition.MouseUp += TrackPosition_MouseUp;
            trackPosition.ValueChanged += TrackPosition_ValueChanged;

            pnlMiniPlayer.Controls.Add(trackPosition);

            lblTimeCounter = new Label
            {
                Text = "00:00 / 00:00",
                Location = new Point(pnlMiniPlayer.Width - 170, 38),
                Size = new Size(150, 20),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleRight
            };
            pnlMiniPlayer.Controls.Add(lblTimeCounter);

            Button btnClose = new Button
            {
                Text = "✖",
                Location = new Point(pnlMiniPlayer.Width - 35, 5),
                Size = new Size(25, 25),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.FromArgb(200, 0, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => StopPreview();
            pnlMiniPlayer.Controls.Add(btnClose);

            _previewTimer = new System.Windows.Forms.Timer { Interval = 100 };
            _previewTimer.Tick += PreviewTimer_Tick;
        }

        public void StartPreviewFromExternal(string filePath, string title)
        {
            if (!File.Exists(filePath))
            {
                MessageBox.Show(
                    LanguageManager.GetString("Archive.Error.FileNotFound"),
                    LanguageManager.GetString("Common.Error"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            StartPreview(filePath, title);
        }

        private void StartPreview(string filePath, string title)
        {
            int session;
            lock (_previewLock)
            {
                _previewSessionCounter++;
                session = _previewSessionCounter;
            }

            try
            {
                StopPreview();
            }
            catch { }

            try
            {
                int deviceNumber = SettingsForm.GetAudioDeviceNumber();

                _previewAudioFile = new AudioFileReader(filePath);
                _previewPlayer = new WaveOutEvent();

                if (deviceNumber >= 0)
                {
                    _previewPlayer.DeviceNumber = deviceNumber;
                    Console.WriteLine($"[ArchiveControl] 🔊 Usando audio device:  {deviceNumber}");
                }
                else
                {
                    Console.WriteLine($"[ArchiveControl] 🔊 Usando audio device di default");
                }

                _previewPlayer.PlaybackStopped += (s, e) =>
                {
                    lock (_previewLock)
                    {
                        if (session != _previewSessionCounter)
                        {
                            return;
                        }
                    }

                    if (this.IsHandleCreated)
                    {
                        this.BeginInvoke(new Action(() =>
                        {
                            _isPreviewPlaying = false;
                            btnPlayStop.Text = "▶";
                            btnPlayStop.BackColor = Color.FromArgb(0, 200, 0);
                            _previewTimer.Stop();
                            pnlMiniPlayer.Visible = false;
                            trackPosition.Value = 0;
                            lblTimeCounter.Text = "00:00 / 00:00";
                        }));
                    }
                };

                _previewPlayer.Init(_previewAudioFile);
                _previewPlayer.Play();

                _isPreviewPlaying = true;
                btnPlayStop.Text = "⏸";
                btnPlayStop.BackColor = Color.FromArgb(200, 150, 0);
                lblPreviewTitle.Text = title;

                int totalSeconds = Math.Max(1, (int)_previewAudioFile.TotalTime.TotalSeconds);
                trackPosition.Maximum = totalSeconds;
                trackPosition.Minimum = 0;
                trackPosition.Value = 0;

                pnlMiniPlayer.Visible = true;
                pnlMiniPlayer.BringToFront();

                _previewTimer.Start();

                Console.WriteLine($"[ArchiveControl] ✅ MiniPlayer mostrato per:  {title}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(LanguageManager.GetString("Archive.Error.PreviewFailed"), ex.Message),
                    LanguageManager.GetString("Common.Error"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                StopPreview();
            }
        }

        private void StopPreview()
        {
            try
            {
                lock (_previewLock)
                {
                    _previewSessionCounter++;
                }

                _previewTimer?.Stop();

                if (_previewPlayer != null)
                {
                    try
                    {
                        if (_previewPlayer.PlaybackState == PlaybackState.Playing ||
                            _previewPlayer.PlaybackState == PlaybackState.Paused)
                        {
                            _previewPlayer.Stop();
                        }
                    }
                    catch { }

                    try
                    {
                        _previewPlayer.Dispose();
                    }
                    catch { }

                    _previewPlayer = null;
                }

                if (_previewAudioFile != null)
                {
                    try
                    {
                        _previewAudioFile.Dispose();
                    }
                    catch { }

                    _previewAudioFile = null;
                }

                _isPreviewPlaying = false;
                _userIsDraggingSlider = false;
                btnPlayStop.Text = "▶";
                btnPlayStop.BackColor = Color.FromArgb(0, 200, 0);
                trackPosition.Value = 0;
                lblTimeCounter.Text = "00:00 / 00:00";
                pnlMiniPlayer.Visible = false;

                Console.WriteLine("[ArchiveControl] ✅ MiniPlayer nascosto");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Preview] Errore durante stop: {ex.Message}");
            }
        }

        private void BtnPlayStop_Click(object sender, EventArgs e)
        {
            if (_previewPlayer == null) return;

            try
            {
                if (_isPreviewPlaying)
                {
                    _previewPlayer.Pause();
                    _isPreviewPlaying = false;
                    btnPlayStop.Text = "▶";
                    btnPlayStop.BackColor = Color.FromArgb(0, 200, 0);
                    _previewTimer.Stop();
                }
                else
                {
                    _previewPlayer.Play();
                    _isPreviewPlaying = true;
                    btnPlayStop.Text = "⏸";
                    btnPlayStop.BackColor = Color.FromArgb(200, 150, 0);
                    _previewTimer.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Preview] Errore toggle play/pause: {ex.Message}");
                StopPreview();
            }
        }

        private void TrackPosition_MouseDown(object sender, MouseEventArgs e)
        {
            if (_previewAudioFile == null) return;

            _userIsDraggingSlider = true;

            TrackBar track = (TrackBar)sender;
            double percentage = (double)e.X / track.Width;
            int newValue = (int)(percentage * track.Maximum);
            newValue = Math.Max(track.Minimum, Math.Min(track.Maximum, newValue));

            track.Value = newValue;

            double seconds = newValue;
            _previewAudioFile.CurrentTime = TimeSpan.FromSeconds(seconds);
        }

        private void TrackPosition_MouseUp(object sender, MouseEventArgs e)
        {
            if (_previewAudioFile == null) return;

            _userIsDraggingSlider = false;

            TrackBar track = (TrackBar)sender;
            double seconds = track.Value;
            _previewAudioFile.CurrentTime = TimeSpan.FromSeconds(seconds);
        }

        private void TrackPosition_ValueChanged(object sender, EventArgs e)
        {
            if (_previewAudioFile == null) return;

            if (_userIsDraggingSlider)
            {
                TrackBar track = (TrackBar)sender;
                int currentSeconds = track.Value;
                int totalSeconds = (int)_previewAudioFile.TotalTime.TotalSeconds;

                lblTimeCounter.Text = $"{FormatTime(currentSeconds)} / {FormatTime(totalSeconds)}";
            }
        }

        private void PreviewTimer_Tick(object sender, EventArgs e)
        {
            if (_previewAudioFile != null && !_userIsDraggingSlider)
            {
                try
                {
                    double currentSeconds = _previewAudioFile.CurrentTime.TotalSeconds;
                    double totalSeconds = _previewAudioFile.TotalTime.TotalSeconds;

                    int newValue = (int)Math.Round(currentSeconds);

                    if (newValue < trackPosition.Minimum) newValue = trackPosition.Minimum;
                    if (newValue > trackPosition.Maximum) newValue = trackPosition.Maximum;

                    if (Math.Abs(trackPosition.Value - newValue) > 0)
                    {
                        trackPosition.Value = newValue;
                    }

                    lblTimeCounter.Text = $"{FormatTime((int)currentSeconds)} / {FormatTime((int)totalSeconds)}";
                }
                catch
                {
                    _previewTimer.Stop();
                }
            }
        }

        private string FormatTime(int totalSeconds)
        {
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            return $"{minutes:00}:{seconds:00}";
        }

        // ✅ DATAGRIDVIEW EVENTS

        private void DgvArchive_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow selectedRow = dgvArchive.Rows[e.RowIndex];
            object entryData = selectedRow.Tag;

            if (entryData is MusicEntry musicEntry)
            {
                OpenMusicEditor(musicEntry);
            }
            else if (entryData is ClipEntry clipEntry)
            {
                OpenClipEditor(clipEntry);
            }
        }

        private void DgvArchive_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                BtnDelete_Click(null, null);
            }
        }

        // ✅ BUTTON HANDLERS

        private void BtnImport_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = string.Format(LanguageManager.GetString("Archive.Dialog.SelectFiles"), _archiveType);
                ofd.Filter = "Audio Files|*.mp3;*.wav;*.flac;*.m4a;*.wma|All Files|*.*";
                ofd.Multiselect = true;

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    ImportFiles(ofd.FileNames);
                }
            }
        }

        private void ImportFiles(string[] filePaths)
        {
            if (filePaths.Length == 0) return;

            this.Cursor = Cursors.WaitCursor;
            int imported = 0;
            int errors = 0;

            try
            {
                foreach (string filePath in filePaths)
                {
                    try
                    {
                        if (_archiveType == "Musica")
                        {
                            var musicEntry = CreateMusicEntryFromFile(filePath);
                            if (DbcManager.Insert("Music.dbc", musicEntry))
                                imported++;
                            else
                                errors++;
                        }
                        else
                        {
                            var clipEntry = CreateClipEntryFromFile(filePath);
                            if (DbcManager.Insert("Clips.dbc", clipEntry))
                                imported++;
                            else
                                errors++;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Errore import {filePath}: {ex.Message}");
                        errors++;
                    }
                }

                RefreshArchive();

                MessageBox.Show(
                    string.Format(LanguageManager.GetString("Archive.Message.ImportComplete"), imported, errors),
                    LanguageManager.GetString("Archive.Title.ImportComplete"),
                    MessageBoxButtons.OK,
                    errors > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);

                StatusChanged?.Invoke(this, string.Format(
                    LanguageManager.GetString("Archive.Status.Imported"),
                    imported));
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private MusicEntry CreateMusicEntryFromFile(string filePath)
        {
            string artist = "Unknown Artist";
            string title = Path.GetFileNameWithoutExtension(filePath);
            int year = DateTime.Now.Year;
            string genre = "Unknown";
            int duration = 0;

            try
            {
                TagLib.File tagFile = TagLib.File.Create(filePath);

                if (!string.IsNullOrEmpty(tagFile.Tag.FirstPerformer))
                    artist = tagFile.Tag.FirstPerformer;

                if (!string.IsNullOrEmpty(tagFile.Tag.Title))
                    title = tagFile.Tag.Title;

                if (tagFile.Tag.Year > 0)
                    year = (int)tagFile.Tag.Year;

                if (!string.IsNullOrEmpty(tagFile.Tag.FirstGenre))
                    genre = tagFile.Tag.FirstGenre;

                duration = (int)tagFile.Properties.Duration.TotalSeconds;

                tagFile.Dispose();
            }
            catch
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                if (fileName.Contains(" - "))
                {
                    string[] parts = fileName.Split(new[] { " - " }, 2, StringSplitOptions.None);
                    artist = parts[0].Trim();
                    title = parts[1].Trim();
                }

                try
                {
                    using (var reader = new AudioFileReader(filePath))
                    {
                        duration = (int)reader.TotalTime.TotalSeconds;
                    }
                }
                catch { duration = 180; }
            }

            int durationMs = duration * 1000;

            return new MusicEntry
            {
                FilePath = filePath,
                Artist = artist,
                Title = title,
                Album = "",
                Genre = genre,
                Year = year,
                Duration = duration,
                FileSize = 0,
                Format = "",
                Bitrate = 0,
                SampleRate = 0,
                Channels = 0,
                Categories = "",
                MarkerIN = 0,
                MarkerINTRO = 0,
                MarkerMIX = durationMs,
                MarkerOUT = durationMs,
                ValidMonths = "1;2;3;4;5;6;7;8;9;10;11;12",
                ValidDays = "Monday;Tuesday;Wednesday;Thursday;Friday;Saturday;Sunday",
                ValidHours = "0;1;2;3;4;5;6;7;8;9;10;11;12;13;14;15;16;17;18;19;20;21;22;23",
                ValidFrom = DateTime.Now.ToString("yyyy-MM-dd"),
                ValidTo = DateTime.Now.AddYears(100).ToString("yyyy-MM-dd"),
                AddedDate = DateTime.Now.ToString("yyyy-MM-dd"),
                LastPlayed = "",
                PlayCount = 0
            };
        }

        private ClipEntry CreateClipEntryFromFile(string filePath)
        {
            string title = Path.GetFileNameWithoutExtension(filePath);
            string genre = "Jingle";
            int duration = 0;

            try
            {
                TagLib.File tagFile = TagLib.File.Create(filePath);

                if (!string.IsNullOrEmpty(tagFile.Tag.Title))
                    title = tagFile.Tag.Title;

                if (!string.IsNullOrEmpty(tagFile.Tag.FirstGenre))
                    genre = tagFile.Tag.FirstGenre;

                duration = (int)tagFile.Properties.Duration.TotalSeconds;
                tagFile.Dispose();
            }
            catch
            {
                try
                {
                    using (var reader = new AudioFileReader(filePath))
                    {
                        duration = (int)reader.TotalTime.TotalSeconds;
                    }
                }
                catch { duration = 30; }
            }

            int durationMs = duration * 1000;

            return new ClipEntry
            {
                FilePath = filePath,
                Title = title,
                Genre = genre,
                Duration = duration,
                Categories = "",
                MarkerIN = 0,
                MarkerINTRO = 0,
                MarkerMIX = durationMs,
                MarkerOUT = durationMs,
                ValidMonths = "1;2;3;4;5;6;7;8;9;10;11;12",
                ValidDays = "Monday;Tuesday;Wednesday;Thursday;Friday;Saturday;Sunday",
                ValidHours = "0;1;2;3;4;5;6;7;8;9;10;11;12;13;14;15;16;17;18;19;20;21;22;23",
                ValidFrom = DateTime.Now.ToString("yyyy-MM-dd"),
                ValidTo = DateTime.Now.AddYears(100).ToString("yyyy-MM-dd"),
                AddedDate = DateTime.Now.ToString("yyyy-MM-dd"),
                LastPlayed = "",
                PlayCount = 0
            };
        }

        private void OpenMusicEditor(MusicEntry entry)
        {
            if (entry.ID <= 0)
            {
                MessageBox.Show(
                    LanguageManager.GetString("Archive.Error.InvalidID"),
                    LanguageManager.GetString("Common.Error"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            using (var editorForm = new MusicEditorForm(entry))
            {
                if (editorForm.ShowDialog() == DialogResult.OK)
                {
                    RefreshArchive();
                    StatusChanged?.Invoke(this, LanguageManager.GetString("Archive.Status.TrackUpdated"));
                }
            }
        }

        private void OpenClipEditor(ClipEntry entry)
        {
            if (entry.ID <= 0)
            {
                MessageBox.Show(
                    LanguageManager.GetString("Archive.Error.InvalidID"),
                    LanguageManager.GetString("Common.Error"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            var musicEntry = new MusicEntry
            {
                ID = entry.ID,
                FilePath = entry.FilePath,
                Artist = "Jingle",
                Title = entry.Title,
                Album = "",
                Genre = entry.Genre,
                Categories = entry.Categories,
                Year = DateTime.Now.Year,
                Duration = entry.Duration,
                FileSize = 0,
                Format = "",
                Bitrate = 0,
                SampleRate = 0,
                Channels = 0,
                MarkerIN = entry.MarkerIN,
                MarkerINTRO = entry.MarkerINTRO,
                MarkerMIX = entry.MarkerMIX,
                MarkerOUT = entry.MarkerOUT,
                ValidMonths = entry.ValidMonths,
                ValidDays = entry.ValidDays,
                ValidHours = entry.ValidHours,
                ValidFrom = entry.ValidFrom,
                ValidTo = entry.ValidTo,
                AddedDate = entry.AddedDate,
                LastPlayed = entry.LastPlayed,
                PlayCount = entry.PlayCount
            };

            using (var editorForm = new MusicEditorForm(musicEntry, isClip: true))
            {
                if (editorForm.ShowDialog() == DialogResult.OK)
                {
                    entry.Title = musicEntry.Title;
                    entry.Genre = musicEntry.Genre;
                    entry.Categories = musicEntry.Categories;
                    entry.Duration = musicEntry.Duration;
                    entry.MarkerIN = musicEntry.MarkerIN;
                    entry.MarkerINTRO = musicEntry.MarkerINTRO;
                    entry.MarkerMIX = musicEntry.MarkerMIX;
                    entry.MarkerOUT = musicEntry.MarkerOUT;
                    entry.ValidMonths = musicEntry.ValidMonths;
                    entry.ValidDays = musicEntry.ValidDays;
                    entry.ValidHours = musicEntry.ValidHours;
                    entry.ValidFrom = musicEntry.ValidFrom;
                    entry.ValidTo = musicEntry.ValidTo;

                    DbcManager.Update("Clips.dbc", entry);

                    RefreshArchive();
                    StatusChanged?.Invoke(this, LanguageManager.GetString("Archive.Status.JingleUpdated"));
                }
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgvArchive.SelectedRows.Count == 0)
            {
                MessageBox.Show(
                    LanguageManager.GetString("Archive.Validation.SelectToEdit"),
                    LanguageManager.GetString("Common.Warning"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (dgvArchive.SelectedRows.Count == 1)
            {
                DgvArchive_CellDoubleClick(null, new DataGridViewCellEventArgs(0, dgvArchive.SelectedRows[0].Index));
            }
            else
            {
                MenuBatchEdit_Click(null, null);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvArchive.SelectedRows.Count == 0)
            {
                MessageBox.Show(
                    LanguageManager.GetString("Archive.Validation.SelectToDelete"),
                    LanguageManager.GetString("Common.Warning"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show(
                string.Format(LanguageManager.GetString("Archive.Message.ConfirmDelete"), dgvArchive.SelectedRows.Count),
                LanguageManager.GetString("Archive.Title.ConfirmDelete"),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                int deleted = 0;

                foreach (DataGridViewRow row in dgvArchive.SelectedRows)
                {
                    object entryData = row.Tag;
                    int id = 0;

                    if (entryData is MusicEntry musicEntry)
                        id = musicEntry.ID;
                    else if (entryData is ClipEntry clipEntry)
                        id = clipEntry.ID;

                    if (id > 0)
                    {
                        if (_archiveType == "Musica")
                        {
                            if (DbcManager.Delete<MusicEntry>("Music.dbc", id))
                                deleted++;
                        }
                        else
                        {
                            if (DbcManager.Delete<ClipEntry>("Clips.dbc", id))
                                deleted++;
                        }
                    }
                }

                RefreshArchive();
                StatusChanged?.Invoke(this, string.Format(
                    LanguageManager.GetString("Archive.Status.Deleted"),
                    deleted));
            }
        }

        public void RefreshArchive()
        {
            try
            {
                dgvArchive.Rows.Clear();

                if (_archiveType == "Musica")
                {
                    _allMusicData = DbcManager.LoadFromCsv<MusicEntry>("Music.dbc");
                    LoadGenresAndCategories(_allMusicData);
                }
                else
                {
                    _allClipsData = DbcManager.LoadFromCsv<ClipEntry>("Clips.dbc");
                    LoadGenresAndCategoriesClips(_allClipsData);
                }

                ApplyFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(LanguageManager.GetString("Archive.Error.LoadFailed"), _archiveType, ex.Message),
                    LanguageManager.GetString("Common.Error"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void LoadGenresAndCategories(List<MusicEntry> data)
        {
            var genres = data.Select(m => m.Genre ?? "").Where(g => !string.IsNullOrEmpty(g)).Distinct().OrderBy(g => g).ToList();
            var categories = data.Select(m => m.Categories ?? "").Where(c => !string.IsNullOrEmpty(c)).Distinct().OrderBy(c => c).ToList();

            string currentGenre = cmbGenreFilter.SelectedItem?.ToString();
            string currentCategory = cmbCategoryFilter.SelectedItem?.ToString();

            cmbGenreFilter.Items.Clear();
            cmbGenreFilter.Items.Add(LanguageManager.GetString("Archive.Filter.AllGenres"));
            foreach (var genre in genres)
                cmbGenreFilter.Items.Add(genre);

            cmbCategoryFilter.Items.Clear();
            cmbCategoryFilter.Items.Add(LanguageManager.GetString("Archive.Filter.AllCategories"));
            foreach (var category in categories)
                cmbCategoryFilter.Items.Add(category);

            cmbGenreFilter.SelectedItem = currentGenre ?? cmbGenreFilter.Items[0];
            cmbCategoryFilter.SelectedItem = currentCategory ?? cmbCategoryFilter.Items[0];

            if (cmbGenreFilter.SelectedIndex < 0) cmbGenreFilter.SelectedIndex = 0;
            if (cmbCategoryFilter.SelectedIndex < 0) cmbCategoryFilter.SelectedIndex = 0;
        }

        private void LoadGenresAndCategoriesClips(List<ClipEntry> data)
        {
            var genres = data.Select(c => c.Genre ?? "").Where(g => !string.IsNullOrEmpty(g)).Distinct().OrderBy(g => g).ToList();
            var categories = data.Select(c => c.Categories ?? "").Where(c => !string.IsNullOrEmpty(c)).Distinct().OrderBy(c => c).ToList();

            cmbGenreFilter.Items.Clear();
            cmbGenreFilter.Items.Add(LanguageManager.GetString("Archive.Filter.AllGenres"));
            foreach (var genre in genres)
                cmbGenreFilter.Items.Add(genre);

            cmbCategoryFilter.Items.Clear();
            cmbCategoryFilter.Items.Add(LanguageManager.GetString("Archive.Filter.AllCategories"));
            foreach (var category in categories)
                cmbCategoryFilter.Items.Add(category);

            cmbGenreFilter.SelectedIndex = 0;
            cmbCategoryFilter.SelectedIndex = 0;
        }

        private void ApplyFilters()
        {
            dgvArchive.Rows.Clear();

            string searchText = txtSearch.Text.ToLower();
            string selectedGenre = cmbGenreFilter.SelectedItem?.ToString() ?? cmbGenreFilter.Items[0].ToString();
            string selectedCategory = cmbCategoryFilter.SelectedItem?.ToString() ?? cmbCategoryFilter.Items[0].ToString();

            string allGenresKey = LanguageManager.GetString("Archive.Filter.AllGenres");
            string allCategoriesKey = LanguageManager.GetString("Archive.Filter.AllCategories");

            if (_archiveType == "Musica")
            {
                var filtered = _allMusicData.AsEnumerable();

                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    filtered = filtered.Where(m =>
                        (m.Artist ?? "").ToLower().Contains(searchText) ||
                        (m.Title ?? "").ToLower().Contains(searchText));
                }

                if (selectedGenre != allGenresKey)
                    filtered = filtered.Where(m => m.Genre == selectedGenre);

                if (selectedCategory != allCategoriesKey)
                    filtered = filtered.Where(m => m.Categories == selectedCategory);

                foreach (var entry in filtered)
                {
                    int rowIndex = dgvArchive.Rows.Add(
                        entry.Artist ?? "",
                        entry.Title ?? "",
                        entry.Genre ?? "",
                                                entry.Year.ToString(),
                        FormatDuration(entry.Duration),
                        $"{NormalizeDuration(entry.MarkerINTRO):F0}s",
                        entry.Categories ?? "",
                        entry.AddedDate ?? ""
                    );

                    dgvArchive.Rows[rowIndex].Tag = entry;
                }

                UpdateHeaderCount(filtered.Count());
                StatusChanged?.Invoke(this, string.Format(
                    LanguageManager.GetString("Archive.Status.FilteredMusic"),
                    filtered.Count(),
                    _allMusicData.Count));
            }
            else
            {
                var filtered = _allClipsData.AsEnumerable();

                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    filtered = filtered.Where(c => (c.Title ?? "").ToLower().Contains(searchText));
                }

                if (selectedGenre != allGenresKey)
                    filtered = filtered.Where(c => c.Genre == selectedGenre);

                if (selectedCategory != allCategoriesKey)
                    filtered = filtered.Where(c => c.Categories == selectedCategory);

                foreach (var entry in filtered)
                {
                    int rowIndex = dgvArchive.Rows.Add(
                        entry.Title ?? "",
                        entry.Genre ?? "",
                        FormatDuration(entry.Duration),
                        $"{NormalizeDuration(entry.MarkerINTRO):F0}s",
                        entry.Categories ?? "",
                        entry.AddedDate ?? ""
                    );

                    dgvArchive.Rows[rowIndex].Tag = entry;
                }

                UpdateHeaderCount(filtered.Count());
                StatusChanged?.Invoke(this, string.Format(
                    LanguageManager.GetString("Archive.Status.FilteredClips"),
                    filtered.Count(),
                    _allClipsData.Count));
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // ✅ DISOTTOSCRIVI EVENTO
                LanguageManager.LanguageChanged -= (s, e) => ApplyLanguage();

                _previewTimer?.Stop();
                _previewTimer?.Dispose();
                _previewPlayer?.Stop();
                _previewPlayer?.Dispose();
                _previewAudioFile?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}