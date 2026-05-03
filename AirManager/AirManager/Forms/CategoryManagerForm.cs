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

namespace AirManager.Forms
{
    public class CategoryManagerForm : Form
    {
        // ── Controls ────────────────────────────────────────────────────────
        private TableLayoutPanel tableMain;

        // Left panel (Music)
        private Label lblMusicHeader;
        private DataGridView dgvMusic;
        private FlowLayoutPanel pnlMusicButtons;
        private Button btnMusicRename;
        private Button btnMusicDelete;

        // Right panel (Clips)
        private Label lblClipsHeader;
        private DataGridView dgvClips;
        private FlowLayoutPanel pnlClipsButtons;
        private Button btnClipsRename;
        private Button btnClipsDelete;

        // Bottom bar
        private Panel bottomPanel;
        private Button btnRefresh;
        private Button btnClose;

        // ── Constructor ─────────────────────────────────────────────────────

        public CategoryManagerForm()
        {
            InitializeComponents();
            ApplyTheme();
            LoadData();

            LanguageManager.LanguageChanged += (s, e) => ApplyLanguage();
        }

        // ── Initialization ───────────────────────────────────────────────────

        private void InitializeComponents()
        {
            this.Text = LanguageManager.GetString("CategoryManager.Title", "🗂️ MODIFICA CATEGORIE");
            this.Size = new Size(800, 500);
            this.MinimumSize = new Size(640, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;

            // ── Bottom bar ──────────────────────────────────────────────────
            bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 46,
                Padding = new Padding(6, 6, 6, 6)
            };

            btnRefresh = CreateButton(LanguageManager.GetString("CategoryManager.Refresh", "🔄 Aggiorna"));
            btnClose = CreateButton(LanguageManager.GetString("Common.Close", "✖ Chiudi"));

            btnRefresh.Click += (s, e) => LoadData();
            btnClose.Click += (s, e) => this.Close();

            var bottomFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true
            };
            bottomFlow.Controls.Add(btnRefresh);
            bottomFlow.Controls.Add(btnClose);
            bottomPanel.Controls.Add(bottomFlow);

            // ── TableLayoutPanel: two equal columns ─────────────────────────
            tableMain = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(6)
            };
            tableMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            // ── Left panel (Music) ──────────────────────────────────────────
            var panelMusic = new Panel { Dock = DockStyle.Fill };

            lblMusicHeader = new Label
            {
                Text = LanguageManager.GetString("CategoryManager.MusicCategories", "🎵 CATEGORIE MUSICA"),
                Dock = DockStyle.Top,
                Height = 26,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(4, 0, 0, 0)
            };

            dgvMusic = CreateGrid();
            dgvMusic.SelectionChanged += (s, e) => UpdateMusicButtons();

            pnlMusicButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 36,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(0, 4, 0, 0)
            };
            btnMusicRename = CreateButton(LanguageManager.GetString("CategoryManager.Rename", "✏️ Rinomina"));
            btnMusicDelete = CreateButton(LanguageManager.GetString("CategoryManager.Delete", "🗑️ Elimina"));
            btnMusicRename.Enabled = false;
            btnMusicDelete.Enabled = false;
            btnMusicRename.Click += (s, e) => RenameCategory(dgvMusic, isMusic: true);
            btnMusicDelete.Click += (s, e) => DeleteCategory(dgvMusic, isMusic: true);
            pnlMusicButtons.Controls.Add(btnMusicRename);
            pnlMusicButtons.Controls.Add(btnMusicDelete);

            panelMusic.Controls.Add(dgvMusic);
            panelMusic.Controls.Add(pnlMusicButtons);
            panelMusic.Controls.Add(lblMusicHeader);

            // ── Right panel (Clips) ─────────────────────────────────────────
            var panelClips = new Panel { Dock = DockStyle.Fill };

            lblClipsHeader = new Label
            {
                Text = LanguageManager.GetString("CategoryManager.ClipsCategories", "⚡ CATEGORIE CLIPS"),
                Dock = DockStyle.Top,
                Height = 26,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(4, 0, 0, 0)
            };

            dgvClips = CreateGrid();
            dgvClips.SelectionChanged += (s, e) => UpdateClipsButtons();

            pnlClipsButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 36,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(0, 4, 0, 0)
            };
            btnClipsRename = CreateButton(LanguageManager.GetString("CategoryManager.Rename", "✏️ Rinomina"));
            btnClipsDelete = CreateButton(LanguageManager.GetString("CategoryManager.Delete", "🗑️ Elimina"));
            btnClipsRename.Enabled = false;
            btnClipsDelete.Enabled = false;
            btnClipsRename.Click += (s, e) => RenameCategory(dgvClips, isMusic: false);
            btnClipsDelete.Click += (s, e) => DeleteCategory(dgvClips, isMusic: false);
            pnlClipsButtons.Controls.Add(btnClipsRename);
            pnlClipsButtons.Controls.Add(btnClipsDelete);

            panelClips.Controls.Add(dgvClips);
            panelClips.Controls.Add(pnlClipsButtons);
            panelClips.Controls.Add(lblClipsHeader);

            tableMain.Controls.Add(panelMusic, 0, 0);
            tableMain.Controls.Add(panelClips, 1, 0);

            this.Controls.Add(tableMain);
            this.Controls.Add(bottomPanel);
        }

        private DataGridView CreateGrid()
        {
            var dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 9F)
            };

            string colCategory = LanguageManager.GetString("CategoryManager.ColumnCategory", "Categoria");
            string colCount = LanguageManager.GetString("CategoryManager.ColumnCount", "Elementi");

            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colCategory",
                HeaderText = colCategory,
                FillWeight = 70
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colCount",
                HeaderText = colCount,
                FillWeight = 30
            });

            return dgv;
        }

        private Button CreateButton(string text)
        {
            return new Button
            {
                Text = text,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                Font = new Font("Segoe UI", 9F),
                Padding = new Padding(6, 0, 6, 0),
                AutoSize = true
            };
        }

        // ── Theme ────────────────────────────────────────────────────────────

        private void ApplyTheme()
        {
            this.BackColor = AppTheme.BgLight;
            this.ForeColor = AppTheme.TextPrimary;
            ApplyThemeToControls(this);
        }

        private void ApplyThemeToControls(Control parent)
        {
            foreach (Control ctrl in parent.Controls)
            {
                ctrl.BackColor = AppTheme.BgLight;
                ctrl.ForeColor = AppTheme.TextPrimary;

                if (ctrl is DataGridView dgv)
                {
                    dgv.BackgroundColor = AppTheme.BgLight;
                    dgv.GridColor = Color.FromArgb(70, 70, 75);
                    dgv.DefaultCellStyle.BackColor = AppTheme.BgLight;
                    dgv.DefaultCellStyle.ForeColor = AppTheme.TextPrimary;
                    dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 122, 204);
                    dgv.DefaultCellStyle.SelectionForeColor = Color.White;
                    dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(37, 37, 38);
                    dgv.ColumnHeadersDefaultCellStyle.ForeColor = AppTheme.TextPrimary;
                    dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                    dgv.EnableHeadersVisualStyles = false;
                }
                else if (ctrl is Button btn)
                {
                    btn.BackColor = Color.FromArgb(37, 37, 38);
                    btn.ForeColor = AppTheme.TextPrimary;
                }

                if (ctrl.Controls.Count > 0)
                    ApplyThemeToControls(ctrl);
            }
        }

        // ── Language ─────────────────────────────────────────────────────────

        private void ApplyLanguage()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(ApplyLanguage));
                return;
            }

            this.Text = LanguageManager.GetString("CategoryManager.Title", "🗂️ MODIFICA CATEGORIE");
            lblMusicHeader.Text = LanguageManager.GetString("CategoryManager.MusicCategories", "🎵 CATEGORIE MUSICA");
            lblClipsHeader.Text = LanguageManager.GetString("CategoryManager.ClipsCategories", "⚡ CATEGORIE CLIPS");

            string rename = LanguageManager.GetString("CategoryManager.Rename", "✏️ Rinomina");
            string delete = LanguageManager.GetString("CategoryManager.Delete", "🗑️ Elimina");
            btnMusicRename.Text = rename;
            btnMusicDelete.Text = delete;
            btnClipsRename.Text = rename;
            btnClipsDelete.Text = delete;
            btnRefresh.Text = LanguageManager.GetString("CategoryManager.Refresh", "🔄 Aggiorna");
            btnClose.Text = "✖ " + LanguageManager.GetString("Common.Close", "Chiudi");

            if (dgvMusic.Columns.Count >= 2)
            {
                dgvMusic.Columns["colCategory"].HeaderText = LanguageManager.GetString("CategoryManager.ColumnCategory", "Categoria");
                dgvMusic.Columns["colCount"].HeaderText = LanguageManager.GetString("CategoryManager.ColumnCount", "Elementi");
            }
            if (dgvClips.Columns.Count >= 2)
            {
                dgvClips.Columns["colCategory"].HeaderText = LanguageManager.GetString("CategoryManager.ColumnCategory", "Categoria");
                dgvClips.Columns["colCount"].HeaderText = LanguageManager.GetString("CategoryManager.ColumnCount", "Elementi");
            }
        }

        // ── Data Loading ─────────────────────────────────────────────────────

        private void LoadData()
        {
            try
            {
                var categories = DbcManager.LoadFromCsv<CategoryEntry>("Categories.dbc");
                var musicEntries = DbcManager.LoadFromCsv<MusicEntry>("Music.dbc");
                var clipEntries = DbcManager.LoadFromCsv<ClipEntry>("Clips.dbc");

                PopulateGrid(dgvMusic, categories, musicEntries.Select(m => m.Categories).ToList());
                PopulateGrid(dgvClips, categories, clipEntries.Select(c => c.Categories).ToList());

                UpdateMusicButtons();
                UpdateClipsButtons();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(LanguageManager.GetString("CategoryManager.LoadError", "❌ Errore caricamento categorie:\n{0}"), ex.Message),
                    LanguageManager.GetString("Common.Error", "Errore"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void PopulateGrid(DataGridView dgv, List<CategoryEntry> categories, List<string> categoriesFields)
        {
            dgv.Rows.Clear();
            if (categories.Count == 0)
            {
                var row = dgv.Rows[dgv.Rows.Add()];
                row.Cells[0].Value = LanguageManager.GetString("CategoryManager.NoCategories", "Nessuna categoria trovata");
                row.Cells[1].Value = "";
                return;
            }

            foreach (var cat in categories.OrderBy(c => c.CategoryName))
            {
                string name = cat.CategoryName;
                int count = categoriesFields.Count(field =>
                    !string.IsNullOrEmpty(field) &&
                    SplitCategories(field).Any(c => string.Equals(c, name, StringComparison.OrdinalIgnoreCase)));
                dgv.Rows.Add(name, count);
            }
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static string[] SplitCategories(string field)
        {
            if (string.IsNullOrEmpty(field)) return Array.Empty<string>();
            return field.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim())
                        .Where(s => !string.IsNullOrEmpty(s))
                        .ToArray();
        }

        private static string RemoveCategoryFromField(string field, string categoryToRemove)
        {
            var parts = SplitCategories(field)
                .Where(c => !string.Equals(c, categoryToRemove, StringComparison.OrdinalIgnoreCase))
                .ToArray();
            return string.Join(";", parts);
        }

        private static string ReplaceCategoryInField(string field, string oldName, string newName)
        {
            var parts = SplitCategories(field)
                .Select(c => string.Equals(c, oldName, StringComparison.OrdinalIgnoreCase) ? newName : c)
                .ToArray();
            return string.Join(";", parts);
        }

        private string GetSelectedCategory(DataGridView dgv)
        {
            if (dgv.SelectedRows.Count == 0) return null;
            var val = dgv.SelectedRows[0].Cells[0].Value;
            return val?.ToString();
        }

        private void UpdateMusicButtons()
        {
            bool hasSelection = dgvMusic.SelectedRows.Count > 0;
            btnMusicRename.Enabled = hasSelection;
            btnMusicDelete.Enabled = hasSelection;
        }

        private void UpdateClipsButtons()
        {
            bool hasSelection = dgvClips.SelectedRows.Count > 0;
            btnClipsRename.Enabled = hasSelection;
            btnClipsDelete.Enabled = hasSelection;
        }

        // ── Playlist scanning ─────────────────────────────────────────────────

        private string[] GetPlaylistFiles()
        {
            string playlistDir = Path.Combine(DbcManager.GetDatabasePath(), "Playlist");
            if (!Directory.Exists(playlistDir)) return Array.Empty<string>();
            return Directory.GetFiles(playlistDir, "*.airpls", SearchOption.TopDirectoryOnly);
        }

        // ── Delete logic ──────────────────────────────────────────────────────

        private void DeleteCategory(DataGridView dgv, bool isMusic)
        {
            string categoryName = GetSelectedCategory(dgv);
            if (string.IsNullOrEmpty(categoryName)) return;

            // Count usages
            int usageCount = 0;
            if (isMusic)
            {
                var entries = DbcManager.LoadFromCsv<MusicEntry>("Music.dbc");
                usageCount = entries.Count(e => SplitCategories(e.Categories)
                    .Any(c => string.Equals(c, categoryName, StringComparison.OrdinalIgnoreCase)));
            }
            else
            {
                var entries = DbcManager.LoadFromCsv<ClipEntry>("Clips.dbc");
                usageCount = entries.Count(e => SplitCategories(e.Categories)
                    .Any(c => string.Equals(c, categoryName, StringComparison.OrdinalIgnoreCase)));
            }

            string confirmMsg = string.Format(
                LanguageManager.GetString("CategoryManager.ConfirmDelete",
                    "Eliminando '{0}' verrà rimossa da {1} brani/clip e da tutte le playlist.\n\nQuesta operazione non può essere annullata.\n\nContinuare?"),
                categoryName, usageCount);

            var result = MessageBox.Show(
                confirmMsg,
                LanguageManager.GetString("CategoryManager.ConfirmDeleteTitle", "Conferma Eliminazione"),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes) return;

            try
            {
                int updatedCount = 0;

                // a) Update Music or Clips entries
                if (isMusic)
                {
                    var entries = DbcManager.LoadFromCsv<MusicEntry>("Music.dbc");
                    foreach (var entry in entries)
                    {
                        if (SplitCategories(entry.Categories).Any(c =>
                            string.Equals(c, categoryName, StringComparison.OrdinalIgnoreCase)))
                        {
                            entry.Categories = RemoveCategoryFromField(entry.Categories, categoryName);
                            DbcManager.Update("Music.dbc", entry);
                            updatedCount++;
                        }
                    }
                }
                else
                {
                    var entries = DbcManager.LoadFromCsv<ClipEntry>("Clips.dbc");
                    foreach (var entry in entries)
                    {
                        if (SplitCategories(entry.Categories).Any(c =>
                            string.Equals(c, categoryName, StringComparison.OrdinalIgnoreCase)))
                        {
                            entry.Categories = RemoveCategoryFromField(entry.Categories, categoryName);
                            DbcManager.Update("Clips.dbc", entry);
                            updatedCount++;
                        }
                    }
                }

                // b) Scan playlist files
                foreach (string filePath in GetPlaylistFiles())
                {
                    try
                    {
                        var playlist = AirPlaylist.Load(filePath);
                        bool changed = false;
                        if (playlist.Items != null)
                        {
                            foreach (var item in playlist.Items)
                            {
                                if (string.Equals(item.CategoryName, categoryName, StringComparison.OrdinalIgnoreCase))
                                {
                                    item.CategoryName = "";
                                    changed = true;
                                }
                                if (string.Equals(item.RuleCategoryName, categoryName, StringComparison.OrdinalIgnoreCase))
                                {
                                    item.RuleCategoryName = "";
                                    changed = true;
                                }
                            }
                        }
                        if (changed)
                        {
                            playlist.Save(filePath);
                            updatedCount++;
                        }
                    }
                    catch (Exception exFile)
                    {
                        Console.WriteLine($"[CategoryManager] ⚠️ Errore playlist {filePath}: {exFile.Message}");
                    }
                }

                // c) Delete from Categories.dbc
                var categories = DbcManager.LoadFromCsv<CategoryEntry>("Categories.dbc");
                var toDelete = categories.FirstOrDefault(c =>
                    string.Equals(c.CategoryName, categoryName, StringComparison.OrdinalIgnoreCase));
                if (toDelete != null)
                    DbcManager.Delete<CategoryEntry>("Categories.dbc", toDelete.ID);

                MessageBox.Show(
                    string.Format(LanguageManager.GetString("CategoryManager.DeleteSuccess",
                        "✅ Categoria '{0}' eliminata ({1} file aggiornati)."), categoryName, updatedCount),
                    LanguageManager.GetString("Common.Success", "Successo"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(LanguageManager.GetString("CategoryManager.DeleteError",
                        "❌ Errore durante l'eliminazione:\n{0}"), ex.Message),
                    LanguageManager.GetString("Common.Error", "Errore"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // ── Rename logic ──────────────────────────────────────────────────────

        private void RenameCategory(DataGridView dgv, bool isMusic)
        {
            string oldName = GetSelectedCategory(dgv);
            if (string.IsNullOrEmpty(oldName)) return;

            // Ask for new name
            string newName = ShowInputDialog(
                LanguageManager.GetString("CategoryManager.EnterNewName", "Inserisci il nuovo nome per la categoria:"),
                LanguageManager.GetString("CategoryManager.RenameTitle", "Rinomina Categoria"),
                oldName);

            if (newName == null) return; // cancelled
            newName = newName.Trim();

            if (string.IsNullOrEmpty(newName))
            {
                MessageBox.Show(
                    LanguageManager.GetString("CategoryManager.NewNameEmpty", "Il nuovo nome non può essere vuoto."),
                    LanguageManager.GetString("Common.Warning", "Avviso"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            // Validate: not existing
            var categories = DbcManager.LoadFromCsv<CategoryEntry>("Categories.dbc");
            if (categories.Any(c => string.Equals(c.CategoryName, newName, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(c.CategoryName, oldName, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show(
                    LanguageManager.GetString("CategoryManager.NewNameExists", "Esiste già una categoria con questo nome."),
                    LanguageManager.GetString("Common.Warning", "Avviso"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            // Count affected
            int usageCount = 0;
            if (isMusic)
            {
                var entries = DbcManager.LoadFromCsv<MusicEntry>("Music.dbc");
                usageCount = entries.Count(e => SplitCategories(e.Categories)
                    .Any(c => string.Equals(c, oldName, StringComparison.OrdinalIgnoreCase)));
            }
            else
            {
                var entries = DbcManager.LoadFromCsv<ClipEntry>("Clips.dbc");
                usageCount = entries.Count(e => SplitCategories(e.Categories)
                    .Any(c => string.Equals(c, oldName, StringComparison.OrdinalIgnoreCase)));
            }

            string confirmMsg = string.Format(
                LanguageManager.GetString("CategoryManager.ConfirmRename",
                    "La categoria '{0}' sarà rinominata in '{1}' in {2} brani/clip e in tutte le playlist.\n\nContinuare?"),
                oldName, newName, usageCount);

            var result = MessageBox.Show(
                confirmMsg,
                LanguageManager.GetString("CategoryManager.ConfirmRenameTitle", "Conferma Ridenominazione"),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes) return;

            try
            {
                int updatedCount = 0;

                // a) Update Music or Clips entries
                if (isMusic)
                {
                    var entries = DbcManager.LoadFromCsv<MusicEntry>("Music.dbc");
                    foreach (var entry in entries)
                    {
                        if (SplitCategories(entry.Categories).Any(c =>
                            string.Equals(c, oldName, StringComparison.OrdinalIgnoreCase)))
                        {
                            entry.Categories = ReplaceCategoryInField(entry.Categories, oldName, newName);
                            DbcManager.Update("Music.dbc", entry);
                            updatedCount++;
                        }
                    }
                }
                else
                {
                    var entries = DbcManager.LoadFromCsv<ClipEntry>("Clips.dbc");
                    foreach (var entry in entries)
                    {
                        if (SplitCategories(entry.Categories).Any(c =>
                            string.Equals(c, oldName, StringComparison.OrdinalIgnoreCase)))
                        {
                            entry.Categories = ReplaceCategoryInField(entry.Categories, oldName, newName);
                            DbcManager.Update("Clips.dbc", entry);
                            updatedCount++;
                        }
                    }
                }

                // b) Scan playlist files
                foreach (string filePath in GetPlaylistFiles())
                {
                    try
                    {
                        var playlist = AirPlaylist.Load(filePath);
                        bool changed = false;
                        if (playlist.Items != null)
                        {
                            foreach (var item in playlist.Items)
                            {
                                if (string.Equals(item.CategoryName, oldName, StringComparison.OrdinalIgnoreCase))
                                {
                                    item.CategoryName = newName;
                                    changed = true;
                                }
                                if (string.Equals(item.RuleCategoryName, oldName, StringComparison.OrdinalIgnoreCase))
                                {
                                    item.RuleCategoryName = newName;
                                    changed = true;
                                }
                            }
                        }
                        if (changed)
                        {
                            playlist.Save(filePath);
                            updatedCount++;
                        }
                    }
                    catch (Exception exFile)
                    {
                        Console.WriteLine($"[CategoryManager] ⚠️ Errore playlist {filePath}: {exFile.Message}");
                    }
                }

                // c) Update CategoryEntry
                var catEntry = categories.FirstOrDefault(c =>
                    string.Equals(c.CategoryName, oldName, StringComparison.OrdinalIgnoreCase));
                if (catEntry != null)
                {
                    catEntry.CategoryName = newName;
                    DbcManager.Update("Categories.dbc", catEntry);
                }

                MessageBox.Show(
                    string.Format(LanguageManager.GetString("CategoryManager.RenameSuccess",
                        "✅ Categoria rinominata con successo ({0} file aggiornati)."), updatedCount),
                    LanguageManager.GetString("Common.Success", "Successo"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(LanguageManager.GetString("CategoryManager.RenameError",
                        "❌ Errore durante la ridenominazione:\n{0}"), ex.Message),
                    LanguageManager.GetString("Common.Error", "Errore"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // ── Input Dialog ──────────────────────────────────────────────────────

        private string ShowInputDialog(string prompt, string title, string defaultValue = "")
        {
            using (var dlg = new RenameInputDialog(prompt, title, defaultValue))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                    return dlg.InputValue;
                return null;
            }
        }
    }

    // ── Rename Input Dialog ───────────────────────────────────────────────────

    internal class RenameInputDialog : Form
    {
        public string InputValue { get; private set; }

        private Label lblPrompt;
        private TextBox txtInput;
        private Button btnOk;
        private Button btnCancel;

        public RenameInputDialog(string prompt, string title, string defaultValue)
        {
            this.Text = title;
            this.Size = new Size(400, 160);
            this.MinimumSize = new Size(300, 140);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = AppTheme.BgLight;
            this.ForeColor = AppTheme.TextPrimary;

            lblPrompt = new Label
            {
                Text = prompt,
                Dock = DockStyle.Top,
                Height = 40,
                Font = new Font("Segoe UI", 9F),
                Padding = new Padding(8, 8, 8, 0),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft
            };
            lblPrompt.BackColor = AppTheme.BgLight;
            lblPrompt.ForeColor = AppTheme.TextPrimary;

            txtInput = new TextBox
            {
                Text = defaultValue,
                Font = new Font("Segoe UI", 9F),
                BackColor = Color.FromArgb(37, 37, 38),
                ForeColor = AppTheme.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle,
                Left = 8,
                Top = 48,
                Width = 370,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            btnOk = new Button
            {
                Text = LanguageManager.GetString("Common.OK", "OK"),
                DialogResult = DialogResult.OK,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                Font = new Font("Segoe UI", 9F),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                Left = 214,
                Top = 82,
                Width = 80,
                Height = 28
            };

            btnCancel = new Button
            {
                Text = LanguageManager.GetString("Common.Cancel", "Annulla"),
                DialogResult = DialogResult.Cancel,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                Font = new Font("Segoe UI", 9F),
                BackColor = Color.FromArgb(37, 37, 38),
                ForeColor = AppTheme.TextPrimary,
                Left = 300,
                Top = 82,
                Width = 80,
                Height = 28
            };

            btnOk.Click += (s, e) => { InputValue = txtInput.Text; };

            this.Controls.AddRange(new Control[] { lblPrompt, txtInput, btnOk, btnCancel });
            this.AcceptButton = btnOk;
            this.CancelButton = btnCancel;
        }
    }
}
