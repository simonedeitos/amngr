using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AirManager.Services;
using AirManager.Services.Database;
using AirManager.Themes;

namespace AirManager.Forms
{
    public partial class ExportMusicArchiveForm : Form
    {
        // Column checkboxes
        private CheckBox chkArtist;
        private CheckBox chkTitle;
        private CheckBox chkDuration;
        private CheckBox chkGenre;
        private CheckBox chkYear;
        private CheckBox chkIntro;
        private CheckBox chkCategories;
        private CheckBox chkDateAdded;
        private CheckBox chkPath;

        // Format radio buttons
        private RadioButton rdoCSV;
        private RadioButton rdoXLS;
        private RadioButton rdoPDF;

        // Action controls
        private Button btnExport;
        private Label lblStatus;

        // Group labels
        private GroupBox grpColumns;
        private GroupBox grpFormat;

        public ExportMusicArchiveForm()
        {
            InitializeComponent();
            ApplyLanguage();
            ApplyDarkTheme();
            LanguageManager.LanguageChanged += OnLanguageChanged;
        }

        private void OnLanguageChanged(object sender, EventArgs e)
        {
            ApplyLanguage();
        }

        private void ApplyLanguage()
        {
            this.Text = "📤 " + LanguageManager.GetString("ExportMusicArchive.Title", "Esporta Archivio Musicale");

            if (grpColumns != null)
                grpColumns.Text = LanguageManager.GetString("ExportMusicArchive.ColumnsGroup", "Colonne da esportare");

            if (chkArtist != null)
                chkArtist.Text = LanguageManager.GetString("ExportMusicArchive.ColArtist", "Artista");
            if (chkTitle != null)
                chkTitle.Text = LanguageManager.GetString("ExportMusicArchive.ColTitle", "Titolo");
            if (chkDuration != null)
                chkDuration.Text = LanguageManager.GetString("ExportMusicArchive.ColDuration", "Durata");
            if (chkGenre != null)
                chkGenre.Text = LanguageManager.GetString("ExportMusicArchive.ColGenre", "Genere");
            if (chkYear != null)
                chkYear.Text = LanguageManager.GetString("ExportMusicArchive.ColYear", "Anno");
            if (chkIntro != null)
                chkIntro.Text = LanguageManager.GetString("ExportMusicArchive.ColIntro", "Intro");
            if (chkCategories != null)
                chkCategories.Text = LanguageManager.GetString("ExportMusicArchive.ColCategories", "Categorie");
            if (chkDateAdded != null)
                chkDateAdded.Text = LanguageManager.GetString("ExportMusicArchive.ColDateAdded", "Data Aggiunta");
            if (chkPath != null)
                chkPath.Text = LanguageManager.GetString("ExportMusicArchive.ColPath", "Path");

            if (grpFormat != null)
                grpFormat.Text = LanguageManager.GetString("ExportMusicArchive.FormatGroup", "Formato di esportazione");

            if (rdoCSV != null)
                rdoCSV.Text = LanguageManager.GetString("ExportMusicArchive.FormatCSV", "CSV (separatore ;)");
            if (rdoXLS != null)
                rdoXLS.Text = LanguageManager.GetString("ExportMusicArchive.FormatXLS", "XLS (Excel)");
            if (rdoPDF != null)
                rdoPDF.Text = LanguageManager.GetString("ExportMusicArchive.FormatPDF", "PDF");

            if (btnExport != null)
                btnExport.Text = "📤 " + LanguageManager.GetString("ExportMusicArchive.BtnExport", "Esporta");
        }

        private void ApplyDarkTheme()
        {
            this.BackColor = Color.FromArgb(28, 28, 28);
            this.ForeColor = Color.White;

            ApplyThemeToControls(this.Controls);
        }

        private void ApplyThemeToControls(Control.ControlCollection controls)
        {
            foreach (Control ctrl in controls)
            {
                ctrl.ForeColor = Color.White;

                if (ctrl is GroupBox grp)
                {
                    grp.BackColor = Color.FromArgb(40, 40, 40);
                }
                else if (ctrl is Panel pnl)
                {
                    pnl.BackColor = Color.FromArgb(40, 40, 40);
                }
                else if (ctrl is CheckBox chk)
                {
                    chk.BackColor = Color.Transparent;
                }
                else if (ctrl is RadioButton rdo)
                {
                    rdo.BackColor = Color.Transparent;
                }
                else if (ctrl is Label lbl)
                {
                    lbl.BackColor = Color.Transparent;
                }

                if (ctrl.Controls.Count > 0)
                    ApplyThemeToControls(ctrl.Controls);
            }
        }

        private void InitializeComponent()
        {
            this.Size = new Size(520, 460);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Font = new Font("Segoe UI", 9);
            this.BackColor = Color.FromArgb(28, 28, 28);
            this.ForeColor = Color.White;

            // --- GroupBox Colonne ---
            grpColumns = new GroupBox
            {
                Text = "Colonne da esportare",
                Location = new Point(12, 12),
                Size = new Size(490, 200),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(40, 40, 40),
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            var columnFont = new Font("Segoe UI", 9);
            int colW = 150;
            int colH = 26;
            int margin = 10;

            chkArtist = new CheckBox { Text = "Artista", Checked = true, Location = new Point(margin, 25), Size = new Size(colW, colH), Font = columnFont, ForeColor = Color.White, BackColor = Color.Transparent };
            chkTitle = new CheckBox { Text = "Titolo", Checked = true, Location = new Point(margin, 55), Size = new Size(colW, colH), Font = columnFont, ForeColor = Color.White, BackColor = Color.Transparent };
            chkDuration = new CheckBox { Text = "Durata", Location = new Point(margin, 85), Size = new Size(colW, colH), Font = columnFont, ForeColor = Color.White, BackColor = Color.Transparent };
            chkGenre = new CheckBox { Text = "Genere", Location = new Point(margin, 115), Size = new Size(colW, colH), Font = columnFont, ForeColor = Color.White, BackColor = Color.Transparent };
            chkYear = new CheckBox { Text = "Anno", Location = new Point(margin, 145), Size = new Size(colW, colH), Font = columnFont, ForeColor = Color.White, BackColor = Color.Transparent };

            int col2X = margin + colW + 10;
            chkIntro = new CheckBox { Text = "Intro", Location = new Point(col2X, 25), Size = new Size(colW, colH), Font = columnFont, ForeColor = Color.White, BackColor = Color.Transparent };
            chkCategories = new CheckBox { Text = "Categorie", Location = new Point(col2X, 55), Size = new Size(colW, colH), Font = columnFont, ForeColor = Color.White, BackColor = Color.Transparent };
            chkDateAdded = new CheckBox { Text = "Data Aggiunta", Location = new Point(col2X, 85), Size = new Size(colW, colH), Font = columnFont, ForeColor = Color.White, BackColor = Color.Transparent };
            chkPath = new CheckBox { Text = "Path", Location = new Point(col2X, 115), Size = new Size(colW + 30, colH), Font = columnFont, ForeColor = Color.White, BackColor = Color.Transparent };

            grpColumns.Controls.AddRange(new Control[] { chkArtist, chkTitle, chkDuration, chkGenre, chkYear, chkIntro, chkCategories, chkDateAdded, chkPath });

            // --- GroupBox Formato ---
            grpFormat = new GroupBox
            {
                Text = "Formato di esportazione",
                Location = new Point(12, 220),
                Size = new Size(490, 80),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(40, 40, 40),
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            var radioFont = new Font("Segoe UI", 9);
            rdoCSV = new RadioButton { Text = "CSV (separatore ;)", Checked = true, Location = new Point(margin, 28), Size = new Size(160, colH), Font = radioFont, ForeColor = Color.White, BackColor = Color.Transparent };
            rdoXLS = new RadioButton { Text = "XLS (Excel)", Location = new Point(180, 28), Size = new Size(120, colH), Font = radioFont, ForeColor = Color.White, BackColor = Color.Transparent };
            rdoPDF = new RadioButton { Text = "PDF", Location = new Point(310, 28), Size = new Size(80, colH), Font = radioFont, ForeColor = Color.White, BackColor = Color.Transparent };

            grpFormat.Controls.AddRange(new Control[] { rdoCSV, rdoXLS, rdoPDF });

            // --- Export button ---
            btnExport = new Button
            {
                Text = "📤 Esporta",
                Location = new Point(12, 315),
                Size = new Size(180, 42),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnExport.FlatAppearance.BorderSize = 0;
            btnExport.Click += BtnExport_Click;

            // --- Status label ---
            lblStatus = new Label
            {
                Text = "",
                Location = new Point(12, 365),
                Size = new Size(490, 50),
                ForeColor = Color.FromArgb(180, 180, 180),
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 9)
            };

            this.Controls.AddRange(new Control[] { grpColumns, grpFormat, btnExport, lblStatus });
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            // 1. Verifica che almeno una colonna sia selezionata
            if (!chkArtist.Checked && !chkTitle.Checked && !chkDuration.Checked &&
                !chkGenre.Checked && !chkYear.Checked && !chkIntro.Checked &&
                !chkCategories.Checked && !chkDateAdded.Checked && !chkPath.Checked)
            {
                MessageBox.Show(
                    LanguageManager.GetString("ExportMusicArchive.SelectAtLeastOneColumn", "Seleziona almeno una colonna da esportare!"),
                    LanguageManager.GetString("Common.Warning", "Attenzione"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            // PDF non disponibile
            if (rdoPDF.Checked)
            {
                MessageBox.Show(
                    LanguageManager.GetString("ExportMusicArchive.PdfNotAvailable", "⚠️ Esportazione PDF non ancora disponibile. Installa una libreria PDF compatibile."),
                    LanguageManager.GetString("Common.Warning", "Attenzione"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            // 2. Leggi entries
            var entries = DbcManager.LoadFromCsv<MusicEntry>("Music.dbc");

            // 3. Mostra SaveFileDialog
            using (var dlg = new SaveFileDialog())
            {
                dlg.Title = LanguageManager.GetString("ExportMusicArchive.SaveDialogTitle", "Salva Archivio Musicale");

                if (rdoCSV.Checked)
                {
                    dlg.Filter = LanguageManager.GetString("ExportMusicArchive.FilterCSV", "File CSV (*.csv)|*.csv");
                    dlg.DefaultExt = "csv";
                }
                else // XLS
                {
                    dlg.Filter = LanguageManager.GetString("ExportMusicArchive.FilterXLS", "File Excel (*.xls)|*.xls");
                    dlg.DefaultExt = "xls";
                }

                if (dlg.ShowDialog() != DialogResult.OK)
                    return;

                try
                {
                    // 4. Esporta
                    int count = ExportData(entries, dlg.FileName);

                    string successMsg = string.Format(
                        LanguageManager.GetString("ExportMusicArchive.ExportSuccess", "✅ Archivio esportato con successo!\n\n{0} brani salvati in:\n{1}"),
                        count,
                        dlg.FileName);

                    lblStatus.Text = successMsg;
                    lblStatus.ForeColor = Color.FromArgb(40, 167, 69);

                    // 5. Messaggio di conferma
                    MessageBox.Show(
                        successMsg,
                        LanguageManager.GetString("Common.Success", "Successo"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    string errMsg = string.Format(
                        LanguageManager.GetString("ExportMusicArchive.ExportError", "❌ Errore durante l'esportazione:\n{0}"),
                        ex.Message);

                    lblStatus.Text = errMsg;
                    lblStatus.ForeColor = Color.FromArgb(220, 53, 69);

                    MessageBox.Show(
                        errMsg,
                        LanguageManager.GetString("Common.Error", "Errore"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        private int ExportData(System.Collections.Generic.List<MusicEntry> entries, string filePath)
        {
            var sb = new StringBuilder();

            // Header
            var headers = new System.Collections.Generic.List<string>();
            if (chkArtist.Checked) headers.Add(LanguageManager.GetString("ExportMusicArchive.ColArtist", "Artista"));
            if (chkTitle.Checked) headers.Add(LanguageManager.GetString("ExportMusicArchive.ColTitle", "Titolo"));
            if (chkDuration.Checked) headers.Add(LanguageManager.GetString("ExportMusicArchive.ColDuration", "Durata"));
            if (chkGenre.Checked) headers.Add(LanguageManager.GetString("ExportMusicArchive.ColGenre", "Genere"));
            if (chkYear.Checked) headers.Add(LanguageManager.GetString("ExportMusicArchive.ColYear", "Anno"));
            if (chkIntro.Checked) headers.Add(LanguageManager.GetString("ExportMusicArchive.ColIntro", "Intro"));
            if (chkCategories.Checked) headers.Add(LanguageManager.GetString("ExportMusicArchive.ColCategories", "Categorie"));
            if (chkDateAdded.Checked) headers.Add(LanguageManager.GetString("ExportMusicArchive.ColDateAdded", "Data Aggiunta"));
            if (chkPath.Checked) headers.Add(LanguageManager.GetString("ExportMusicArchive.ColPath", "Path"));

            sb.AppendLine(string.Join(";", headers));

            // Rows
            foreach (var entry in entries)
            {
                var cols = new System.Collections.Generic.List<string>();
                if (chkArtist.Checked) cols.Add(EscapeCsv(entry.Artist));
                if (chkTitle.Checked) cols.Add(EscapeCsv(entry.Title));
                if (chkDuration.Checked) cols.Add(EscapeCsv(FormatDurationMs(entry.Duration)));
                if (chkGenre.Checked) cols.Add(EscapeCsv(entry.Genre));
                if (chkYear.Checked) cols.Add(entry.Year.ToString());
                if (chkIntro.Checked) cols.Add(EscapeCsv(FormatDurationMs(entry.MarkerINTRO)));
                if (chkCategories.Checked) cols.Add(EscapeCsv(entry.Categories));
                if (chkDateAdded.Checked) cols.Add(EscapeCsv(entry.AddedDate));
                if (chkPath.Checked) cols.Add(EscapeCsv(entry.FilePath));

                sb.AppendLine(string.Join(";", cols));
            }

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
            return entries.Count;
        }

        private static string EscapeCsv(string value)
        {
            if (value == null) return "";
            if (value.Contains(';') || value.Contains('"') || value.Contains('\n'))
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            return value;
        }

        private static string FormatDurationMs(int durationMs)
        {
            double seconds = durationMs / 1000.0;
            int minutes = (int)(seconds / 60);
            int secs = (int)(seconds % 60);
            return $"{minutes:00}:{secs:00}";
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
