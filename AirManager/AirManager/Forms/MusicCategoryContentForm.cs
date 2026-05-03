using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using AirManager.Services.Database;
using AirManager.Services;
using AirManager.Models;
using AirManager.Themes;
using NAudio.Wave;

namespace AirManager.Forms
{
    public class MusicCategoryContentForm : Form
    {
        // ── Left panel controls ──────────────────────────────────────────────
        private Panel pnlLeftContainer;
        private FlowLayoutPanel pnlCategoryButtons;
        private Panel pnlAddCategory;
        private TextBox txtNewCategoryName;
        private Button btnAddCategory;

        // ── Grid ─────────────────────────────────────────────────────────────
        private CategoryContentDataGridView dgvTracks;

        // ── Mini player ──────────────────────────────────────────────────────
        private Panel pnlMiniPlayer;
        private Button btnPlayStop;
        private TrackBar trackPosition;
        private Label lblTimeCounter;
        private Label lblPreviewTitle;
        private Button btnClosePlayer;
        private System.Windows.Forms.Timer _previewTimer;
        private WaveOutEvent _previewPlayer;
        private AudioFileReader _previewAudioFile;
        private bool _isPreviewPlaying = false;
        private bool _userIsDraggingSlider = false;
        private int _previewSessionCounter = 0;
        private readonly object _previewLock = new object();
        private int _previewMarkerIN = 0;

        // ── Data ─────────────────────────────────────────────────────────────
        private List<MusicEntry> _allMusic = new List<MusicEntry>();
        private List<CategoryEntry> _allCategories = new List<CategoryEntry>();
        private string _selectedCategoryName = null; // null = Non Categorizzate
        private Button _selectedButton = null;

        // ── Drag & Drop ──────────────────────────────────────────────────────
        private Point _dragStartPoint;
        private bool _isDragging = false;

        // ── Sorting ──────────────────────────────────────────────────────────
        private string _sortColumnName = null;
        private bool _sortAscending = true;

        // ── Constructor ──────────────────────────────────────────────────────

        public MusicCategoryContentForm()
        {
            InitializeLayout();
            InitializeMiniPlayer();
            ApplyLanguage();
            RefreshAll();

            CategoryManagerForm.CategoriesChanged += OnCategoriesChanged;
            LanguageManager.LanguageChanged += OnLanguageChanged;
        }

        private void OnLanguageChanged(object sender, EventArgs e)
        {
            ApplyLanguage();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                CategoryManagerForm.CategoriesChanged -= OnCategoriesChanged;
                LanguageManager.LanguageChanged -= OnLanguageChanged;
                StopPreview();
                _previewTimer?.Dispose();
            }
            base.Dispose(disposing);
        }

        private void OnCategoriesChanged(object sender, EventArgs e)
        {
            if (this.IsHandleCreated)
                this.BeginInvoke(new Action(RefreshAll));
        }

        // ── Layout Initialization ────────────────────────────────────────────

        private void InitializeLayout()
        {
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new Size(900, 550);
            this.Size = new Size(1200, 700);
            this.BackColor = AppTheme.BgLight;
            this.ForeColor = AppTheme.TextPrimary;

            // ── Left container ──────────────────────────────────────────────
            pnlLeftContainer = new Panel
            {
                Dock = DockStyle.Left,
                Width = 220,
                BackColor = AppTheme.BgLight
            };

            // Add-category area at the bottom of left container
            pnlAddCategory = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 75,
                BackColor = AppTheme.BgLight,
                Padding = new Padding(5, 5, 5, 5)
            };

            txtNewCategoryName = new TextBox
            {
                Font = new Font("Segoe UI", 9),
                Dock = DockStyle.Top,
                Height = 24,
                BackColor = AppTheme.BgInput,
                ForeColor = AppTheme.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle
            };
            pnlAddCategory.Controls.Add(txtNewCategoryName);

            btnAddCategory = new Button
            {
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Dock = DockStyle.Bottom,
                Height = 32,
                BackColor = AppTheme.ButtonPrimary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnAddCategory.FlatAppearance.BorderSize = 0;
            btnAddCategory.Click += BtnAddCategory_Click;
            pnlAddCategory.Controls.Add(btnAddCategory);

            pnlLeftContainer.Controls.Add(pnlAddCategory);

            // Separator between buttons and add-area
            var pnlSep = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 1,
                BackColor = AppTheme.BorderLight
            };
            pnlLeftContainer.Controls.Add(pnlSep);

            // Category buttons FlowLayoutPanel
            pnlCategoryButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = AppTheme.BgLight,
                Padding = new Padding(0)
            };
            pnlLeftContainer.Controls.Add(pnlCategoryButtons);

            // ── Grid ────────────────────────────────────────────────────────
            dgvTracks = new CategoryContentDataGridView
            {
                Dock = DockStyle.Fill,
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
                AllowUserToResizeRows = false,
                ScrollBars = ScrollBars.Both,
                AllowDrop = false
            };
            dgvTracks.RowTemplate.Height = 35;
            dgvTracks.Font = new Font("Segoe UI", 10F);

            dgvTracks.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(50, 50, 50);
            dgvTracks.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvTracks.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvTracks.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvTracks.ColumnHeadersDefaultCellStyle.Padding = new Padding(5);

            dgvTracks.DefaultCellStyle.BackColor = Color.FromArgb(40, 40, 40);
            dgvTracks.DefaultCellStyle.ForeColor = Color.White;
            dgvTracks.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 215);
            dgvTracks.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvTracks.DefaultCellStyle.Padding = new Padding(5);

            dgvTracks.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(35, 35, 35);
            dgvTracks.EnableHeadersVisualStyles = false;

            // Columns – same as ArchiveControl for Music
            dgvTracks.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Video",
                HeaderText = "🎬",
                Width = 40,
                MinimumWidth = 40,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                SortMode = DataGridViewColumnSortMode.Programmatic,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });
            dgvTracks.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Artist",
                FillWeight = 20,
                SortMode = DataGridViewColumnSortMode.Programmatic,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft }
            });
            dgvTracks.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Title",
                FillWeight = 30,
                SortMode = DataGridViewColumnSortMode.Programmatic,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft }
            });
            dgvTracks.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Genre",
                FillWeight = 12,
                SortMode = DataGridViewColumnSortMode.Programmatic,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });
            dgvTracks.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Year",
                FillWeight = 8,
                SortMode = DataGridViewColumnSortMode.Programmatic,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });
            dgvTracks.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Duration",
                FillWeight = 8,
                SortMode = DataGridViewColumnSortMode.Programmatic,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });
            dgvTracks.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Intro",
                FillWeight = 7,
                SortMode = DataGridViewColumnSortMode.Programmatic,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });
            dgvTracks.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Category",
                FillWeight = 12,
                SortMode = DataGridViewColumnSortMode.Programmatic,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });
            dgvTracks.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "AddedDate",
                FillWeight = 10,
                SortMode = DataGridViewColumnSortMode.Programmatic,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            // Context menu – only Preview and ShowPlayHistory
            var contextMenu = new ContextMenuStrip();
            var miPreview = new ToolStripMenuItem { Tag = "ctx_preview" };
            miPreview.Click += MenuPreview_Click;
            contextMenu.Items.Add(miPreview);
            var miHistory = new ToolStripMenuItem { Tag = "ctx_history" };
            miHistory.Click += MenuShowHistory_Click;
            contextMenu.Items.Add(miHistory);
            contextMenu.Opening += ContextMenu_Opening;
            dgvTracks.ContextMenuStrip = contextMenu;

            dgvTracks.ColumnHeaderMouseClick += DgvTracks_ColumnHeaderMouseClick;
            dgvTracks.SelectionChanged += DgvTracks_SelectionChanged;
            dgvTracks.MouseDown += DgvTracks_MouseDown;
            dgvTracks.MouseMove += DgvTracks_MouseMove;

            // Add to form (order matters for docking)
            this.Controls.Add(dgvTracks);
            this.Controls.Add(pnlLeftContainer);
        }

        private void InitializeMiniPlayer()
        {
            pnlMiniPlayer = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 70,
                BackColor = Color.FromArgb(30, 30, 30),
                Visible = false
            };

            lblPreviewTitle = new Label
            {
                Text = "",
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
                Size = new Size(500, 30),
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
            pnlMiniPlayer.Controls.Add(trackPosition);

            lblTimeCounter = new Label
            {
                Text = "00:00 / 00:00",
                Location = new Point(580, 38),
                Size = new Size(120, 20),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.White
            };
            pnlMiniPlayer.Controls.Add(lblTimeCounter);

            btnClosePlayer = new Button
            {
                Text = "✖",
                Size = new Size(25, 25),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.FromArgb(200, 0, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnClosePlayer.FlatAppearance.BorderSize = 0;
            btnClosePlayer.Click += (s, e) => StopPreview();
            pnlMiniPlayer.Controls.Add(btnClosePlayer);

            pnlMiniPlayer.Resize += (s, e) =>
            {
                btnClosePlayer.Location = new Point(pnlMiniPlayer.Width - 30, 5);
                int trackW = pnlMiniPlayer.Width - 250;
                if (trackW < 50) trackW = 50;
                trackPosition.Size = new Size(trackW, 30);
                lblTimeCounter.Location = new Point(pnlMiniPlayer.Width - 160, 38);
            };

            _previewTimer = new System.Windows.Forms.Timer { Interval = 100 };
            _previewTimer.Tick += PreviewTimer_Tick;

            this.Controls.Add(pnlMiniPlayer);
        }

        // ── Language ──────────────────────────────────────────────────────────

        private void ApplyLanguage()
        {
            this.Text = LanguageManager.GetString("MusicCategoryContent.Title", "🎵 CONTENUTO CATEGORIE MUSICA");

            if (txtNewCategoryName != null)
                txtNewCategoryName.PlaceholderText = LanguageManager.GetString("MusicCategoryContent.NewCategoryPlaceholder", "➕ Nuova categoria...");

            if (btnAddCategory != null)
                btnAddCategory.Text = LanguageManager.GetString("MusicCategoryContent.AddCategory", "+ Aggiungi");

            if (lblPreviewTitle != null && !_isPreviewPlaying)
                lblPreviewTitle.Text = LanguageManager.GetString("Archive.NoPreview", "Nessun file in preascolto");

            if (dgvTracks != null)
            {
                if (dgvTracks.Columns.Contains("Artist"))
                    dgvTracks.Columns["Artist"].HeaderText = LanguageManager.GetString("Archive.ColumnArtist", "Artista");
                if (dgvTracks.Columns.Contains("Title"))
                    dgvTracks.Columns["Title"].HeaderText = LanguageManager.GetString("Archive.ColumnTitle", "Titolo");
                if (dgvTracks.Columns.Contains("Genre"))
                    dgvTracks.Columns["Genre"].HeaderText = LanguageManager.GetString("Archive.ColumnGenre", "Genere");
                if (dgvTracks.Columns.Contains("Year"))
                    dgvTracks.Columns["Year"].HeaderText = LanguageManager.GetString("Archive.ColumnYear", "Anno");
                if (dgvTracks.Columns.Contains("Duration"))
                    dgvTracks.Columns["Duration"].HeaderText = LanguageManager.GetString("Archive.ColumnDuration", "Durata");
                if (dgvTracks.Columns.Contains("Intro"))
                    dgvTracks.Columns["Intro"].HeaderText = LanguageManager.GetString("Archive.ColumnIntro", "Intro");
                if (dgvTracks.Columns.Contains("Category"))
                    dgvTracks.Columns["Category"].HeaderText = LanguageManager.GetString("Archive.ColumnCategory", "Categoria");
                if (dgvTracks.Columns.Contains("AddedDate"))
                    dgvTracks.Columns["AddedDate"].HeaderText = LanguageManager.GetString("Archive.ColumnAdded", "Aggiunto");
            }

            if (dgvTracks?.ContextMenuStrip != null)
            {
                foreach (ToolStripItem item in dgvTracks.ContextMenuStrip.Items)
                {
                    if (item.Tag?.ToString() == "ctx_preview")
                        item.Text = "🎧 " + LanguageManager.GetString("Archive.Preview", "Preascolto");
                    else if (item.Tag?.ToString() == "ctx_history")
                        item.Text = "📋 " + LanguageManager.GetString("Archive.ShowPlayHistory", "Mostra Storico Passaggi");
                }
            }
        }

        // ── Data Loading ──────────────────────────────────────────────────────

        private void RefreshAll()
        {
            _allMusic = DbcManager.LoadFromCsv<MusicEntry>("Music.dbc");
            _allCategories = DbcManager.LoadFromCsv<CategoryEntry>("Categories.dbc")
                .OrderBy(c => c.CategoryName)
                .ToList();

            // If selected category was deleted, fall back to Non Categorizzate
            if (_selectedCategoryName != null)
            {
                bool stillExists = _allCategories.Any(c =>
                    string.Equals(c.CategoryName, _selectedCategoryName, StringComparison.OrdinalIgnoreCase));
                if (!stillExists)
                    _selectedCategoryName = null;
            }

            RebuildCategoryPanel();
            LoadTracksForSelectedCategory();
        }

        private void RebuildCategoryPanel()
        {
            pnlCategoryButtons.Controls.Clear();
            _selectedButton = null;

            int btnWidth = Math.Max(100, pnlCategoryButtons.ClientSize.Width - 4);

            string uncategorizedLabel = LanguageManager.GetString("MusicCategoryContent.Uncategorized", "🚫 Non Categorizzate");
            int uncategorizedCount = _allMusic.Count(m => string.IsNullOrWhiteSpace(m.Categories));

            // First button: Non Categorizzate (always at top)
            var btnUncat = CreateCategoryButton($"{uncategorizedLabel} ({uncategorizedCount})", null, btnWidth);
            pnlCategoryButtons.Controls.Add(btnUncat);
            if (_selectedCategoryName == null)
                SelectButton(btnUncat, null);

            // Other categories in alphabetical order
            foreach (var cat in _allCategories)
            {
                int count = _allMusic.Count(m =>
                    SplitCategories(m.Categories).Any(c =>
                        string.Equals(c, cat.CategoryName, StringComparison.OrdinalIgnoreCase)));

                var btn = CreateCategoryButton($"{cat.CategoryName} ({count})", cat.CategoryName, btnWidth);
                pnlCategoryButtons.Controls.Add(btn);

                if (string.Equals(_selectedCategoryName, cat.CategoryName, StringComparison.OrdinalIgnoreCase))
                    SelectButton(btn, cat.CategoryName);
            }
        }

        private Button CreateCategoryButton(string text, string categoryName, int width)
        {
            var btn = new Button
            {
                Text = text,
                Tag = categoryName,
                Width = width,
                Height = 40,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                BackColor = AppTheme.Surface,
                ForeColor = AppTheme.TextPrimary,
                TextAlign = ContentAlignment.MiddleLeft,
                Cursor = Cursors.Hand,
                AllowDrop = true,
                Padding = new Padding(8, 0, 0, 0),
                Margin = new Padding(0)
            };
            btn.FlatAppearance.BorderSize = 0;

            btn.Click += (s, e) =>
            {
                _selectedCategoryName = categoryName;
                SelectButton(btn, categoryName);
                LoadTracksForSelectedCategory();
            };

            // Drag & Drop: "Non Categorizzate" (categoryName == null) is NOT a valid drop target
            btn.DragEnter += (s, e) =>
            {
                if (categoryName == null || !e.Data.GetDataPresent("MusicEntryList"))
                {
                    e.Effect = DragDropEffects.None;
                    return;
                }
                e.Effect = DragDropEffects.Move;
                btn.BackColor = AppTheme.Primary;
                btn.ForeColor = Color.White;
            };

            btn.DragLeave += (s, e) => RestoreButtonColor(btn, categoryName);

            btn.DragDrop += (s, e) => HandleDrop(btn, e);

            return btn;
        }

        private void SelectButton(Button btn, string categoryName)
        {
            if (_selectedButton != null && _selectedButton != btn)
            {
                _selectedButton.BackColor = AppTheme.Surface;
                _selectedButton.ForeColor = AppTheme.TextPrimary;
            }
            btn.BackColor = AppTheme.Primary;
            btn.ForeColor = Color.White;
            _selectedButton = btn;
            _selectedCategoryName = categoryName;
        }

        private void RestoreButtonColor(Button btn, string categoryName)
        {
            bool isSelected = (categoryName == null && _selectedCategoryName == null)
                || string.Equals(categoryName, _selectedCategoryName, StringComparison.OrdinalIgnoreCase);

            btn.BackColor = isSelected ? AppTheme.Primary : AppTheme.Surface;
            btn.ForeColor = isSelected ? Color.White : AppTheme.TextPrimary;
        }

        private void LoadTracksForSelectedCategory()
        {
            dgvTracks.Rows.Clear();

            // Update sort glyph
            foreach (DataGridViewColumn col in dgvTracks.Columns)
                col.HeaderCell.SortGlyphDirection = SortOrder.None;
            if (!string.IsNullOrEmpty(_sortColumnName) && dgvTracks.Columns.Contains(_sortColumnName))
                dgvTracks.Columns[_sortColumnName].HeaderCell.SortGlyphDirection =
                    _sortAscending ? SortOrder.Ascending : SortOrder.Descending;

            IEnumerable<MusicEntry> filtered;
            if (_selectedCategoryName == null)
            {
                // Non Categorizzate: entries with empty/null/whitespace Categories
                filtered = _allMusic.Where(m => string.IsNullOrWhiteSpace(m.Categories));
            }
            else
            {
                filtered = _allMusic.Where(m =>
                    SplitCategories(m.Categories).Any(c =>
                        string.Equals(c, _selectedCategoryName, StringComparison.OrdinalIgnoreCase)));
            }

            var list = filtered.ToList();

            // Apply sort
            if (!string.IsNullOrEmpty(_sortColumnName))
            {
                IEnumerable<MusicEntry> sorted = _sortColumnName switch
                {
                    "Artist"    => _sortAscending ? list.OrderBy(m => m.Artist ?? "")     : (IEnumerable<MusicEntry>)list.OrderByDescending(m => m.Artist ?? ""),
                    "Title"     => _sortAscending ? list.OrderBy(m => m.Title ?? "")      : list.OrderByDescending(m => m.Title ?? ""),
                    "Genre"     => _sortAscending ? list.OrderBy(m => m.Genre ?? "")      : list.OrderByDescending(m => m.Genre ?? ""),
                    "Year"      => _sortAscending ? list.OrderBy(m => m.Year)             : list.OrderByDescending(m => m.Year),
                    "Duration"  => _sortAscending ? list.OrderBy(m => m.Duration)         : list.OrderByDescending(m => m.Duration),
                    "Category"  => _sortAscending ? list.OrderBy(m => m.Categories ?? "") : list.OrderByDescending(m => m.Categories ?? ""),
                    "AddedDate" => _sortAscending ? list.OrderBy(m => m.AddedDate ?? "")  : list.OrderByDescending(m => m.AddedDate ?? ""),
                    _           => list
                };
                list = sorted.ToList();
            }

            foreach (var entry in list)
            {
                int displayDurationMs = entry.MarkerMIX > entry.MarkerIN
                    ? entry.MarkerMIX - entry.MarkerIN
                    : entry.Duration - entry.MarkerIN;
                int introMs = Math.Max(0, entry.MarkerINTRO - entry.MarkerIN);

                int rowIndex = dgvTracks.Rows.Add(
                    GetVideoIcon(entry),
                    entry.Artist ?? "",
                    entry.Title ?? "",
                    entry.Genre ?? "",
                    entry.Year > 0 ? entry.Year.ToString() : "",
                    FormatDurationMs(displayDurationMs),
                    $"{introMs / 1000}s",
                    entry.Categories ?? "",
                    entry.AddedDate ?? ""
                );
                dgvTracks.Rows[rowIndex].Tag = entry;
            }
        }

        // ── Column Header Sorting ─────────────────────────────────────────────

        private void DgvTracks_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            string colName = dgvTracks.Columns[e.ColumnIndex].Name;
            if (_sortColumnName == colName)
                _sortAscending = !_sortAscending;
            else
            {
                _sortColumnName = colName;
                _sortAscending = true;
            }
            LoadTracksForSelectedCategory();
        }

        // ── Selection Changed ─────────────────────────────────────────────────

        private void DgvTracks_SelectionChanged(object sender, EventArgs e)
        {
            if (_isPreviewPlaying)
                StopPreview();
        }

        // ── Context Menu ──────────────────────────────────────────────────────

        private void ContextMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Only show menu for single-row selection
            if (dgvTracks.SelectedRows.Count != 1)
            {
                e.Cancel = true;
            }
        }

        private void MenuPreview_Click(object sender, EventArgs e)
        {
            if (dgvTracks.SelectedRows.Count == 0) return;

            var row = dgvTracks.SelectedRows[0];
            if (!(row.Tag is MusicEntry entry)) return;

            if (string.IsNullOrEmpty(entry.FilePath) || !File.Exists(entry.FilePath))
            {
                MessageBox.Show(
                    LanguageManager.GetString("Archive.FileNotFound", "File non trovato!"),
                    LanguageManager.GetString("Common.Error", "Errore"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            StartPreview(entry);
        }

        private void MenuShowHistory_Click(object sender, EventArgs e)
        {
            if (dgvTracks.SelectedRows.Count == 0) return;

            var row = dgvTracks.SelectedRows[0];
            if (!(row.Tag is MusicEntry entry)) return;

            using (var form = new TrackHistoryForm(entry.Artist, entry.Title))
            {
                form.ShowDialog(this);
            }
        }

        // ── Add Category ──────────────────────────────────────────────────────

        private void BtnAddCategory_Click(object sender, EventArgs e)
        {
            string name = txtNewCategoryName.Text.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show(
                    LanguageManager.GetString("MusicCategoryContent.CategoryEmpty", "Il nome della categoria non può essere vuoto."),
                    LanguageManager.GetString("Common.Warning", "Attenzione"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            bool exists = _allCategories.Any(c =>
                string.Equals(c.CategoryName, name, StringComparison.OrdinalIgnoreCase));

            if (exists)
            {
                MessageBox.Show(
                    LanguageManager.GetString("MusicCategoryContent.CategoryExists", "Esiste già una categoria con questo nome."),
                    LanguageManager.GetString("Common.Warning", "Attenzione"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            DbcManager.Insert("Categories.dbc", new CategoryEntry
            {
                CategoryName = name,
                Color = "#607D8B",
                IgnoreHourlySeparation = 0
            });

            txtNewCategoryName.Text = "";
            RefreshAll();
        }

        // ── Drag & Drop ───────────────────────────────────────────────────────

        private void DgvTracks_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                _dragStartPoint = e.Location;
        }

        private void DgvTracks_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left || _isDragging) return;

            if (Math.Abs(e.X - _dragStartPoint.X) < SystemInformation.DragSize.Width &&
                Math.Abs(e.Y - _dragStartPoint.Y) < SystemInformation.DragSize.Height)
                return;

            if (dgvTracks.SelectedRows.Count == 0) return;

            var selectedEntries = dgvTracks.SelectedRows
                .Cast<DataGridViewRow>()
                .Where(r => r.Tag is MusicEntry)
                .Select(r => (MusicEntry)r.Tag)
                .ToList();

            if (selectedEntries.Count == 0) return;

            _isDragging = true;
            try
            {
                var dragData = new DataObject();
                dragData.SetData("MusicEntryList", selectedEntries);
                dgvTracks.DoDragDrop(dragData, DragDropEffects.Move);
            }
            finally
            {
                _isDragging = false;
            }
        }

        private void HandleDrop(Button targetButton, DragEventArgs e)
        {
            string targetCategoryName = targetButton.Tag as string;

            // "Non Categorizzate" is not a valid drop target
            if (targetCategoryName == null) return;

            var entries = e.Data.GetData("MusicEntryList") as List<MusicEntry>;
            if (entries == null || entries.Count == 0) return;

            // Restore button colour first
            RestoreButtonColor(targetButton, targetCategoryName);

            string title = LanguageManager.GetString("MusicCategoryContent.MoveOrCopyTitle", "Sposta o Copia?");
            string message = string.Format(
                LanguageManager.GetString("MusicCategoryContent.MoveOrCopyMessage",
                    "Vuoi spostare o copiare i {0} brani selezionati nella categoria '{1}'?"),
                entries.Count, targetCategoryName);

            string moveText   = LanguageManager.GetString("MusicCategoryContent.Move",   "📦 Sposta");
            string copyText   = LanguageManager.GetString("MusicCategoryContent.Copy",   "📋 Copia");
            string cancelText = LanguageManager.GetString("Common.Cancel", "Annulla");

            DialogResult result;
            using (var dlg = new MoveOrCopyDialog(title, message, moveText, copyText, cancelText))
            {
                result = dlg.ShowDialog(this);
            }

            if (result == DialogResult.Cancel) return;

            bool isMove = (result == DialogResult.Yes);

            foreach (var entry in entries)
            {
                if (isMove)
                {
                    if (_selectedCategoryName == null)
                    {
                        // Coming from "Non Categorizzate" – just add target category
                        var cats = SplitCategories(entry.Categories).ToList();
                        if (!cats.Any(c => string.Equals(c, targetCategoryName, StringComparison.OrdinalIgnoreCase)))
                            cats.Add(targetCategoryName);
                        entry.Categories = string.Join(";", cats);
                    }
                    else
                    {
                        // Remove source category, add target
                        var cats = SplitCategories(entry.Categories).ToList();
                        cats.RemoveAll(c => string.Equals(c, _selectedCategoryName, StringComparison.OrdinalIgnoreCase));
                        if (!cats.Any(c => string.Equals(c, targetCategoryName, StringComparison.OrdinalIgnoreCase)))
                            cats.Add(targetCategoryName);
                        entry.Categories = string.Join(";", cats);
                    }
                }
                else
                {
                    // Copy: just add target category if not already present
                    var cats = SplitCategories(entry.Categories).ToList();
                    if (!cats.Any(c => string.Equals(c, targetCategoryName, StringComparison.OrdinalIgnoreCase)))
                        cats.Add(targetCategoryName);
                    entry.Categories = string.Join(";", cats);
                }

                DbcManager.Update("Music.dbc", entry);
            }

            RefreshAll();
        }

        // ── Mini Player ───────────────────────────────────────────────────────

        private void StartPreview(MusicEntry entry)
        {
            try { StopPreview(); } catch { }

            int session;
            lock (_previewLock)
            {
                _previewSessionCounter++;
                session = _previewSessionCounter;
            }

            try
            {
                _previewMarkerIN = entry.MarkerIN;
                string title = $"{entry.Artist} - {entry.Title}";

                _previewAudioFile = new AudioFileReader(entry.FilePath);

                // Start from MarkerIN
                if (entry.MarkerIN > 0)
                    _previewAudioFile.CurrentTime = TimeSpan.FromMilliseconds(entry.MarkerIN);

                _previewPlayer = new WaveOutEvent();
                _previewPlayer.Init(_previewAudioFile);

                _previewPlayer.PlaybackStopped += (s, ev) =>
                {
                    lock (_previewLock)
                    {
                        if (session != _previewSessionCounter) return;
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
                            int minVal = _previewMarkerIN / 1000;
                            if (minVal >= trackPosition.Minimum && minVal <= trackPosition.Maximum)
                                trackPosition.Value = minVal;
                            lblTimeCounter.Text = "00:00 / 00:00";
                        }));
                    }
                };

                _previewPlayer.Play();
                _isPreviewPlaying = true;
                btnPlayStop.Text = "⏸";
                btnPlayStop.BackColor = Color.FromArgb(200, 150, 0);
                lblPreviewTitle.Text = title;

                int totalSeconds = Math.Max(1, (int)_previewAudioFile.TotalTime.TotalSeconds);
                int minSeconds = entry.MarkerIN / 1000;
                trackPosition.Minimum = minSeconds;
                trackPosition.Maximum = totalSeconds;
                trackPosition.Value = minSeconds;

                pnlMiniPlayer.Visible = true;
                pnlMiniPlayer.BringToFront();
                _previewTimer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(LanguageManager.GetString("Archive.PreviewError", "Errore preascolto:\n{0}"), ex.Message),
                    LanguageManager.GetString("Common.Error", "Errore"),
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
                            _previewPlayer.Stop();
                    }
                    catch { }

                    try { _previewPlayer.Dispose(); } catch { }
                    _previewPlayer = null;
                }

                if (_previewAudioFile != null)
                {
                    try { _previewAudioFile.Dispose(); } catch { }
                    _previewAudioFile = null;
                }

                _isPreviewPlaying = false;
                _userIsDraggingSlider = false;

                if (btnPlayStop != null)
                {
                    btnPlayStop.Text = "▶";
                    btnPlayStop.BackColor = Color.FromArgb(0, 200, 0);
                }

                if (trackPosition != null)
                {
                    trackPosition.Minimum = 0;
                    trackPosition.Maximum = 1000;
                    trackPosition.Value = 0;
                }

                if (lblTimeCounter != null)
                    lblTimeCounter.Text = "00:00 / 00:00";

                if (lblPreviewTitle != null)
                    lblPreviewTitle.Text = LanguageManager.GetString("Archive.NoPreview", "Nessun file in preascolto");

                if (pnlMiniPlayer != null)
                    pnlMiniPlayer.Visible = false;
            }
            catch { }
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
            catch
            {
                StopPreview();
            }
        }

        private void TrackPosition_MouseDown(object sender, MouseEventArgs e)
        {
            if (_previewAudioFile == null) return;

            _userIsDraggingSlider = true;
            TrackBar track = (TrackBar)sender;
            double percentage = (double)e.X / track.Width;
            int newValue = (int)(percentage * (track.Maximum - track.Minimum)) + track.Minimum;
            newValue = Math.Max(track.Minimum, Math.Min(track.Maximum, newValue));
            track.Value = newValue;
            _previewAudioFile.CurrentTime = TimeSpan.FromSeconds(newValue);
        }

        private void TrackPosition_MouseUp(object sender, MouseEventArgs e)
        {
            if (_previewAudioFile == null) return;

            _userIsDraggingSlider = false;
            TrackBar track = (TrackBar)sender;
            _previewAudioFile.CurrentTime = TimeSpan.FromSeconds(track.Value);
        }

        private void PreviewTimer_Tick(object sender, EventArgs e)
        {
            if (_previewAudioFile == null || _userIsDraggingSlider) return;

            try
            {
                int currentSec = (int)_previewAudioFile.CurrentTime.TotalSeconds;
                int totalSec   = (int)_previewAudioFile.TotalTime.TotalSeconds;

                int clampedVal = Math.Max(trackPosition.Minimum, Math.Min(trackPosition.Maximum, currentSec));
                if (Math.Abs(trackPosition.Value - clampedVal) > 0)
                    trackPosition.Value = clampedVal;

                lblTimeCounter.Text = $"{FormatTime(currentSec)} / {FormatTime(totalSec)}";
            }
            catch
            {
                _previewTimer.Stop();
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static string[] SplitCategories(string field)
        {
            if (string.IsNullOrWhiteSpace(field)) return Array.Empty<string>();
            return field.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(c => c.Trim())
                        .Where(c => !string.IsNullOrEmpty(c))
                        .ToArray();
        }

        private static string GetVideoIcon(MusicEntry entry)
        {
            if (entry.VideoSource == VideoSourceType.StaticVideo && !string.IsNullOrEmpty(entry.VideoFilePath))
                return "🎬";
            if (entry.VideoSource == VideoSourceType.BufferVideo)
                return "🖼️";
            return "🎵";
        }

        private static string FormatDurationMs(int ms)
        {
            if (ms <= 0) return "00:00";
            int totalSec = ms / 1000;
            int min = totalSec / 60;
            int sec = totalSec % 60;
            return $"{min:D2}:{sec:D2}";
        }

        private static string FormatTime(int totalSeconds)
        {
            if (totalSeconds < 0) totalSeconds = 0;
            int min = totalSeconds / 60;
            int sec = totalSeconds % 60;
            return $"{min:D2}:{sec:D2}";
        }

        // ── Inner classes ─────────────────────────────────────────────────────

        private class CategoryContentDataGridView : DataGridView
        {
            protected override void OnMouseDown(MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Right)
                {
                    var hitTest = HitTest(e.X, e.Y);
                    if (hitTest.RowIndex >= 0 && hitTest.RowIndex < Rows.Count
                        && Rows[hitTest.RowIndex].Selected)
                    {
                        // Right-click on already-selected row: keep multi-selection intact
                        return;
                    }
                }
                base.OnMouseDown(e);
            }
        }
    }

    // ── Move or Copy Dialog ───────────────────────────────────────────────────

    internal class MoveOrCopyDialog : Form
    {
        public MoveOrCopyDialog(string title, string message, string moveText, string copyText, string cancelText)
        {
            this.Text = title;
            this.Size = new Size(440, 190);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = AppTheme.BgLight;
            this.ForeColor = AppTheme.TextPrimary;

            var lblMessage = new Label
            {
                Text = message,
                Location = new Point(12, 12),
                Size = new Size(410, 50),
                ForeColor = AppTheme.TextPrimary
            };
            this.Controls.Add(lblMessage);

            var btnMove = new Button
            {
                Text = moveText,
                Location = new Point(12, 90),
                Size = new Size(120, 36),
                DialogResult = DialogResult.Yes,
                FlatStyle = FlatStyle.Flat,
                BackColor = AppTheme.ButtonPrimary,
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnMove.FlatAppearance.BorderSize = 0;
            this.Controls.Add(btnMove);

            var btnCopy = new Button
            {
                Text = copyText,
                Location = new Point(145, 90),
                Size = new Size(120, 36),
                DialogResult = DialogResult.No,
                FlatStyle = FlatStyle.Flat,
                BackColor = AppTheme.ButtonSecondary,
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnCopy.FlatAppearance.BorderSize = 0;
            this.Controls.Add(btnCopy);

            var btnCancel = new Button
            {
                Text = cancelText,
                Location = new Point(278, 90),
                Size = new Size(120, 36),
                DialogResult = DialogResult.Cancel,
                FlatStyle = FlatStyle.Flat,
                BackColor = AppTheme.ButtonDanger,
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            this.Controls.Add(btnCancel);

            this.AcceptButton = btnMove;
            this.CancelButton = btnCancel;
        }
    }
}
