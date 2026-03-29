using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using AirManager.Services;
using AirManager.Services.Database;
using AirManager.Themes;
using Microsoft.Win32;

namespace AirManager.Forms
{
    public partial class VideoAssociationDialog : Form
    {
        private Label lblTitle;
        private Label lblCurrentVideo;
        private Label lblVideoPath;
        private TextBox txtVideoPath;
        private Button btnBrowse;
        private Button btnPreview;
        private Button btnRemove;
        private Button btnOk;
        private Button btnCancel;
        private Panel pnlPreview;
        private PictureBox picPreview;

        private string _artist;
        private string _title;
        private string _currentVideoPath;

        public string SelectedVideoPath => txtVideoPath?.Text ?? "";

        public VideoAssociationDialog(string artist, string title, string currentVideoPath = "")
        {
            _artist = artist ?? "";
            _title = title ?? "";
            _currentVideoPath = currentVideoPath ?? "";

            InitializeComponent();
            InitializeCustomUI();
            ApplyLanguage();

            LanguageManager.LanguageChanged += OnLanguageChanged;
        }

        private void OnLanguageChanged(object? sender, EventArgs e)
        {
            ApplyLanguage();
        }

        private void ApplyLanguage()
        {
            this.Text = "🎬 " + LanguageManager.GetString("VideoAssociation.Title", "Associa Video");
            lblTitle.Text = $"🎬 {LanguageManager.GetString("VideoAssociation.Title", "Associa Video")}";
            lblCurrentVideo.Text = LanguageManager.GetString("VideoAssociation.CurrentVideo", "Video associato:");
            lblVideoPath.Text = LanguageManager.GetString("VideoAssociation.VideoPath", "Percorso video:");
            btnBrowse.Text = "📂 " + LanguageManager.GetString("VideoAssociation.Browse", "Sfoglia...");
            btnPreview.Text = "▶ " + LanguageManager.GetString("VideoAssociation.Preview", "Anteprima");
            btnRemove.Text = "🗑 " + LanguageManager.GetString("VideoAssociation.Remove", "Rimuovi");
            btnOk.Text = LanguageManager.GetString("Common.OK", "OK");
            btnCancel.Text = LanguageManager.GetString("Common.Cancel", "Annulla");
        }

        private void InitializeCustomUI()
        {
            this.Text = "🎬 Associa Video";
            this.Size = new Size(600, 420);
            this.MinimumSize = new Size(500, 380);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = AppTheme.BgLight;
            this.ForeColor = Color.White;

            // Header panel
            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 55,
                BackColor = AppTheme.BgDark
            };

            lblTitle = new Label
            {
                Text = "🎬 ASSOCIA VIDEO",
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(15, 8),
                AutoSize = true
            };
            headerPanel.Controls.Add(lblTitle);

            Label lblTrackInfo = new Label
            {
                Text = $"{_artist} - {_title}",
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                ForeColor = AppTheme.TextSecondary,
                Location = new Point(15, 32),
                AutoSize = true
            };
            headerPanel.Controls.Add(lblTrackInfo);
            this.Controls.Add(headerPanel);

            // Content panel
            Panel contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20, 15, 20, 10),
                BackColor = AppTheme.BgLight
            };

            int y = 15;

            lblCurrentVideo = new Label
            {
                Text = "Video associato:",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = AppTheme.TextSecondary,
                Location = new Point(20, y),
                AutoSize = true
            };
            contentPanel.Controls.Add(lblCurrentVideo);
            y += 25;

            Label lblCurrentPath = new Label
            {
                Text = string.IsNullOrEmpty(_currentVideoPath)
                    ? LanguageManager.GetString("VideoAssociation.NoVideo", "Nessun video associato")
                    : Path.GetFileName(_currentVideoPath),
                Font = new Font("Segoe UI", 9),
                ForeColor = string.IsNullOrEmpty(_currentVideoPath) ? AppTheme.TextDisabled : AppTheme.AccentPrimary,
                Location = new Point(20, y),
                AutoSize = true
            };
            contentPanel.Controls.Add(lblCurrentPath);
            y += 35;

            lblVideoPath = new Label
            {
                Text = "Percorso video:",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = AppTheme.TextSecondary,
                Location = new Point(20, y),
                AutoSize = true
            };
            contentPanel.Controls.Add(lblVideoPath);
            y += 25;

            txtVideoPath = new TextBox
            {
                Text = _currentVideoPath,
                Font = new Font("Segoe UI", 9),
                Location = new Point(20, y),
                Size = new Size(400, 25),
                BackColor = AppTheme.BgInput,
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            contentPanel.Controls.Add(txtVideoPath);

            btnBrowse = new Button
            {
                Text = "📂 Sfoglia...",
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                Size = new Size(110, 25),
                Location = new Point(430, y),
                BackColor = AppTheme.ButtonPrimary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnBrowse.FlatAppearance.BorderSize = 0;
            btnBrowse.Click += BtnBrowse_Click;
            contentPanel.Controls.Add(btnBrowse);
            y += 40;

            // Action buttons row
            btnPreview = new Button
            {
                Text = "▶ Anteprima",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Size = new Size(120, 30),
                Location = new Point(20, y),
                BackColor = AppTheme.ButtonInfo,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnPreview.FlatAppearance.BorderSize = 0;
            btnPreview.Click += BtnPreview_Click;
            contentPanel.Controls.Add(btnPreview);

            btnRemove = new Button
            {
                Text = "🗑 Rimuovi",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Size = new Size(110, 30),
                Location = new Point(150, y),
                BackColor = AppTheme.ButtonDanger,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRemove.FlatAppearance.BorderSize = 0;
            btnRemove.Click += BtnRemove_Click;
            contentPanel.Controls.Add(btnRemove);
            y += 45;

            // Preview area
            pnlPreview = new Panel
            {
                Location = new Point(20, y),
                Size = new Size(520, 80),
                BackColor = AppTheme.BgDark,
                BorderStyle = BorderStyle.FixedSingle
            };

            picPreview = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = AppTheme.BgDark
            };
            pnlPreview.Controls.Add(picPreview);

            Label lblPreviewPlaceholder = new Label
            {
                Text = LanguageManager.GetString("VideoAssociation.PreviewArea", "Area anteprima video"),
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                ForeColor = AppTheme.TextDisabled,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlPreview.Controls.Add(lblPreviewPlaceholder);
            contentPanel.Controls.Add(pnlPreview);

            this.Controls.Add(contentPanel);

            // Bottom button panel
            Panel bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = AppTheme.BgDark
            };

            btnCancel = new Button
            {
                Text = "Annulla",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Size = new Size(100, 32),
                Location = new Point(470, 9),
                BackColor = AppTheme.ButtonSecondary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.Cancel
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            bottomPanel.Controls.Add(btnCancel);

            btnOk = new Button
            {
                Text = "OK",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Size = new Size(100, 32),
                Location = new Point(360, 9),
                BackColor = AppTheme.ButtonSuccess,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnOk.FlatAppearance.BorderSize = 0;
            btnOk.Click += BtnOk_Click;
            bottomPanel.Controls.Add(btnOk);

            this.Controls.Add(bottomPanel);

            this.AcceptButton = btnOk;
            this.CancelButton = btnCancel;
        }

        private static string GetBufferVideoPath()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\AirManager");
                if (key != null)
                {
                    var value = key.GetValue("BufferVideoPath") as string;
                    if (!string.IsNullOrEmpty(value) && Directory.Exists(value))
                        return value;
                }
            }
            catch
            {
                // Registry access may fail
            }

            return @"C:\AirManager\BufferVideo";
        }

        private void BtnBrowse_Click(object? sender, EventArgs e)
        {
            string initialDir = GetBufferVideoPath();

            using var ofd = new OpenFileDialog
            {
                Title = LanguageManager.GetString("VideoAssociation.Browse.Title", "Seleziona file video"),
                Filter = "Video Files (*.mp4;*.avi;*.mkv;*.wmv;*.mov)|*.mp4;*.avi;*.mkv;*.wmv;*.mov|All Files (*.*)|*.*",
                InitialDirectory = Directory.Exists(initialDir) ? initialDir : ""
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                txtVideoPath.Text = ofd.FileName;
            }
        }

        private void BtnPreview_Click(object? sender, EventArgs e)
        {
            string path = txtVideoPath.Text.Trim();

            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                MessageBox.Show(
                    LanguageManager.GetString("VideoAssociation.Error.FileNotFound", "File video non trovato."),
                    LanguageManager.GetString("Common.Warning", "Attenzione"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using var previewForm = new VideoPreviewForm(path);
                previewForm.ShowDialog(this);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[VideoAssociationDialog] ❌ Errore anteprima: {ex.Message}");
                MessageBox.Show(
                    LanguageManager.GetString("VideoAssociation.Error.Preview", "Errore durante l'anteprima del video."),
                    LanguageManager.GetString("Common.Error", "Errore"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void BtnRemove_Click(object? sender, EventArgs e)
        {
            var result = MessageBox.Show(
                LanguageManager.GetString("VideoAssociation.Confirm.Remove", "Vuoi rimuovere l'associazione video?"),
                LanguageManager.GetString("Common.Confirm", "Conferma"),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                txtVideoPath.Text = "";
            }
        }

        private void BtnOk_Click(object? sender, EventArgs e)
        {
            string path = txtVideoPath.Text.Trim();

            if (!string.IsNullOrEmpty(path) && !File.Exists(path))
            {
                MessageBox.Show(
                    LanguageManager.GetString("VideoAssociation.Error.FileNotFound", "File video non trovato."),
                    LanguageManager.GetString("Common.Warning", "Attenzione"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
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
