using AirManager.Services;
using AirManager.Themes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace AirManager.Forms
{
    public partial class VideoConvertionForm : Form
    {
        // ── File list grid ──
        private DataGridView dgvFiles;

        // ── Buttons ──
        private Button btnImportFiles;
        private Button btnRemoveSelected;
        private Button btnConvert;
        private Button btnClose;
        private Button btnBrowseOutput;

        // ── Conversion options ──
        private ComboBox cmbOutputFormat;
        private ComboBox cmbResolution;
        private ComboBox cmbVideoBitrate;
        private ComboBox cmbAudioBitrate;
        private ComboBox cmbFrameRate;

        // ── Labels ──
        private Label lblTitle;
        private Label lblOutputFormat;
        private Label lblResolution;
        private Label lblVideoBitrate;
        private Label lblAudioBitrate;
        private Label lblFrameRate;
        private Label lblOutputPath;
        private Label lblTrimStart;
        private Label lblTrimEnd;
        private Label lblStatus;
        private Label lblFileCount;

        // ── Trim inputs ──
        private TextBox txtTrimStart;
        private TextBox txtTrimEnd;

        // ── Output path ──
        private TextBox txtOutputPath;

        // ── Progress ──
        private ProgressBar progressConversion;
        private Label lblProgress;

        // ── Panels ──
        private Panel panelHeader;
        private Panel panelGrid;
        private Panel panelOutput;
        private Panel panelButtons;

        // ── Group Boxes ──
        private GroupBox grpConversionOptions;
        private GroupBox grpPreEditing;

        // ── Internal data ──
        private List<VideoFileInfo> _importedFiles;

        public VideoConvertionForm()
        {
            InitializeComponent();

            _importedFiles = new List<VideoFileInfo>();

            this.BackColor = AppTheme.BgDark;
            this.ForeColor = AppTheme.TextPrimary;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            BuildUI();
            ApplyTheme();
            ApplyLanguage();

            LanguageManager.LanguageChanged += (s, e) => ApplyLanguage();
        }

        #region UI Construction

        private void BuildUI()
        {
            this.SuspendLayout();

            // ── Header Panel ──
            panelHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = AppTheme.BgPanel,
                Padding = new Padding(15, 0, 15, 0)
            };

            lblTitle = new Label
            {
                Text = "🎬 Video Convertion",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = AppTheme.TextPrimary,
                Dock = DockStyle.Left,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(0, 10, 0, 0)
            };
            panelHeader.Controls.Add(lblTitle);

            lblFileCount = new Label
            {
                Text = "0 files",
                Font = new Font("Segoe UI", 10),
                ForeColor = AppTheme.TextSecondary,
                Dock = DockStyle.Right,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleRight,
                Padding = new Padding(0, 15, 0, 0)
            };
            panelHeader.Controls.Add(lblFileCount);

            // ── Grid Panel ──
            panelGrid = new Panel
            {
                Location = new Point(10, 55),
                Size = new Size(1080, 270),
                BackColor = AppTheme.BgPanel
            };

            BuildDataGridView();
            panelGrid.Controls.Add(dgvFiles);

            // ── Conversion Options Group ──
            grpConversionOptions = new GroupBox
            {
                Location = new Point(10, 330),
                Size = new Size(540, 200),
                ForeColor = AppTheme.TextPrimary,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = AppTheme.BgPanel
            };

            BuildConversionOptions();

            // ── Pre-Editing Group ──
            grpPreEditing = new GroupBox
            {
                Location = new Point(560, 330),
                Size = new Size(530, 100),
                ForeColor = AppTheme.TextPrimary,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = AppTheme.BgPanel
            };

            BuildTrimSection();

            // ── Output Path Panel ──
            panelOutput = new Panel
            {
                Location = new Point(560, 435),
                Size = new Size(530, 95),
                BackColor = AppTheme.BgPanel
            };

            BuildOutputSection();

            // ── Progress Section ──
            progressConversion = new ProgressBar
            {
                Location = new Point(10, 545),
                Size = new Size(890, 28),
                Style = ProgressBarStyle.Continuous,
                Value = 0
            };

            lblProgress = new Label
            {
                Location = new Point(910, 545),
                Size = new Size(180, 28),
                Text = "0%",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = AppTheme.TextSecondary,
                TextAlign = ContentAlignment.MiddleLeft
            };

            lblStatus = new Label
            {
                Location = new Point(10, 578),
                Size = new Size(890, 22),
                Text = "",
                Font = new Font("Segoe UI", 9),
                ForeColor = AppTheme.TextSecondary,
                TextAlign = ContentAlignment.MiddleLeft
            };

            // ── Button Panel ──
            panelButtons = new Panel
            {
                Location = new Point(10, 610),
                Size = new Size(1080, 65),
                BackColor = AppTheme.BgDark
            };

            BuildButtons();

            // ── Add controls to form ──
            this.Controls.Add(panelHeader);
            this.Controls.Add(panelGrid);
            this.Controls.Add(grpConversionOptions);
            this.Controls.Add(grpPreEditing);
            this.Controls.Add(panelOutput);
            this.Controls.Add(progressConversion);
            this.Controls.Add(lblProgress);
            this.Controls.Add(lblStatus);
            this.Controls.Add(panelButtons);

            this.ResumeLayout(false);
        }

        private void BuildDataGridView()
        {
            dgvFiles = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = true,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                EnableHeadersVisualStyles = false,
                Font = new Font("Segoe UI", 9)
            };

            dgvFiles.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colFileName",
                HeaderText = "File Name",
                FillWeight = 35
            });
            dgvFiles.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colDuration",
                HeaderText = "Duration",
                FillWeight = 12
            });
            dgvFiles.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colResolution",
                HeaderText = "Resolution",
                FillWeight = 13
            });
            dgvFiles.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colSize",
                HeaderText = "Size",
                FillWeight = 12
            });
            dgvFiles.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colFormat",
                HeaderText = "Format",
                FillWeight = 10
            });
            dgvFiles.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colStatus",
                HeaderText = "Status",
                FillWeight = 18
            });
        }

        private void BuildConversionOptions()
        {
            Font labelFont = new Font("Segoe UI", 9);
            Font comboFont = new Font("Segoe UI", 9);

            int labelX = 15;
            int comboX = 150;
            int rowHeight = 32;
            int startY = 28;

            // Output Format
            lblOutputFormat = new Label
            {
                Location = new Point(labelX, startY + 3),
                Size = new Size(130, 22),
                Font = labelFont,
                ForeColor = AppTheme.TextSecondary,
                TextAlign = ContentAlignment.MiddleLeft
            };

            cmbOutputFormat = new ComboBox
            {
                Location = new Point(comboX, startY),
                Size = new Size(120, 26),
                Font = comboFont,
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat
            };
            cmbOutputFormat.Items.AddRange(new object[] { "MP4", "AVI", "MKV" });
            cmbOutputFormat.SelectedIndex = 0;

            // Resolution
            lblResolution = new Label
            {
                Location = new Point(labelX, startY + rowHeight + 3),
                Size = new Size(130, 22),
                Font = labelFont,
                ForeColor = AppTheme.TextSecondary,
                TextAlign = ContentAlignment.MiddleLeft
            };

            cmbResolution = new ComboBox
            {
                Location = new Point(comboX, startY + rowHeight),
                Size = new Size(160, 26),
                Font = comboFont,
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat
            };
            cmbResolution.Items.AddRange(new object[]
            {
                "Original", "1920x1080", "1280x720", "854x480", "640x360"
            });
            cmbResolution.SelectedIndex = 0;

            // Video Bitrate
            lblVideoBitrate = new Label
            {
                Location = new Point(labelX, startY + rowHeight * 2 + 3),
                Size = new Size(130, 22),
                Font = labelFont,
                ForeColor = AppTheme.TextSecondary,
                TextAlign = ContentAlignment.MiddleLeft
            };

            cmbVideoBitrate = new ComboBox
            {
                Location = new Point(comboX, startY + rowHeight * 2),
                Size = new Size(120, 26),
                Font = comboFont,
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat
            };
            cmbVideoBitrate.Items.AddRange(new object[] { "Auto", "1000k", "2000k", "4000k", "8000k" });
            cmbVideoBitrate.SelectedIndex = 0;

            // Audio Bitrate
            lblAudioBitrate = new Label
            {
                Location = new Point(300, startY + 3),
                Size = new Size(100, 22),
                Font = labelFont,
                ForeColor = AppTheme.TextSecondary,
                TextAlign = ContentAlignment.MiddleLeft
            };

            cmbAudioBitrate = new ComboBox
            {
                Location = new Point(410, startY),
                Size = new Size(110, 26),
                Font = comboFont,
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat
            };
            cmbAudioBitrate.Items.AddRange(new object[] { "128k", "192k", "256k", "320k" });
            cmbAudioBitrate.SelectedIndex = 1;

            // Frame Rate
            lblFrameRate = new Label
            {
                Location = new Point(300, startY + rowHeight + 3),
                Size = new Size(100, 22),
                Font = labelFont,
                ForeColor = AppTheme.TextSecondary,
                TextAlign = ContentAlignment.MiddleLeft
            };

            cmbFrameRate = new ComboBox
            {
                Location = new Point(410, startY + rowHeight),
                Size = new Size(110, 26),
                Font = comboFont,
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat
            };
            cmbFrameRate.Items.AddRange(new object[] { "Original", "25 fps", "30 fps" });
            cmbFrameRate.SelectedIndex = 0;

            grpConversionOptions.Controls.AddRange(new Control[]
            {
                lblOutputFormat, cmbOutputFormat,
                lblResolution, cmbResolution,
                lblVideoBitrate, cmbVideoBitrate,
                lblAudioBitrate, cmbAudioBitrate,
                lblFrameRate, cmbFrameRate
            });
        }

        private void BuildTrimSection()
        {
            Font labelFont = new Font("Segoe UI", 9);
            Font inputFont = new Font("Consolas", 10);

            lblTrimStart = new Label
            {
                Location = new Point(15, 30),
                Size = new Size(100, 22),
                Font = labelFont,
                ForeColor = AppTheme.TextSecondary,
                TextAlign = ContentAlignment.MiddleLeft
            };

            txtTrimStart = new TextBox
            {
                Location = new Point(120, 28),
                Size = new Size(100, 26),
                Font = inputFont,
                Text = "00:00:00",
                TextAlign = HorizontalAlignment.Center,
                MaxLength = 8
            };

            lblTrimEnd = new Label
            {
                Location = new Point(250, 30),
                Size = new Size(100, 22),
                Font = labelFont,
                ForeColor = AppTheme.TextSecondary,
                TextAlign = ContentAlignment.MiddleLeft
            };

            txtTrimEnd = new TextBox
            {
                Location = new Point(355, 28),
                Size = new Size(100, 26),
                Font = inputFont,
                Text = "00:00:00",
                TextAlign = HorizontalAlignment.Center,
                MaxLength = 8
            };

            grpPreEditing.Controls.AddRange(new Control[]
            {
                lblTrimStart, txtTrimStart,
                lblTrimEnd, txtTrimEnd
            });
        }

        private void BuildOutputSection()
        {
            Font labelFont = new Font("Segoe UI", 9);

            lblOutputPath = new Label
            {
                Location = new Point(10, 10),
                Size = new Size(510, 22),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = AppTheme.TextPrimary,
                TextAlign = ContentAlignment.MiddleLeft
            };

            txtOutputPath = new TextBox
            {
                Location = new Point(10, 38),
                Size = new Size(420, 26),
                Font = labelFont,
                ReadOnly = true
            };
            txtOutputPath.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);

            btnBrowseOutput = new Button
            {
                Location = new Point(440, 36),
                Size = new Size(80, 30),
                Font = labelFont,
                Text = "📂 Browse",
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnBrowseOutput.FlatAppearance.BorderSize = 0;
            btnBrowseOutput.Click += BtnBrowseOutput_Click;

            panelOutput.Controls.AddRange(new Control[]
            {
                lblOutputPath, txtOutputPath, btnBrowseOutput
            });
        }

        private void BuildButtons()
        {
            Font btnFont = new Font("Segoe UI", 10, FontStyle.Bold);
            int btnHeight = 40;
            int btnY = 12;

            btnImportFiles = new Button
            {
                Location = new Point(0, btnY),
                Size = new Size(180, btnHeight),
                Font = btnFont,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnImportFiles.FlatAppearance.BorderSize = 0;
            btnImportFiles.Click += BtnImportFiles_Click;

            btnRemoveSelected = new Button
            {
                Location = new Point(190, btnY),
                Size = new Size(180, btnHeight),
                Font = btnFont,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRemoveSelected.FlatAppearance.BorderSize = 0;
            btnRemoveSelected.Click += BtnRemoveSelected_Click;

            btnConvert = new Button
            {
                Location = new Point(700, btnY),
                Size = new Size(180, btnHeight),
                Font = btnFont,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnConvert.FlatAppearance.BorderSize = 0;
            btnConvert.Click += BtnConvert_Click;

            btnClose = new Button
            {
                Location = new Point(890, btnY),
                Size = new Size(180, btnHeight),
                Font = btnFont,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += BtnClose_Click;

            panelButtons.Controls.AddRange(new Control[]
            {
                btnImportFiles, btnRemoveSelected, btnConvert, btnClose
            });
        }

        #endregion

        #region Theme

        private void ApplyTheme()
        {
            // Form
            this.BackColor = AppTheme.BgDark;
            this.ForeColor = AppTheme.TextPrimary;

            // Header
            panelHeader.BackColor = AppTheme.BgPanel;
            lblTitle.ForeColor = AppTheme.TextPrimary;
            lblFileCount.ForeColor = AppTheme.TextSecondary;

            // Grid
            dgvFiles.BackgroundColor = AppTheme.BgDark;
            dgvFiles.GridColor = AppTheme.GridBorder;
            dgvFiles.DefaultCellStyle.BackColor = AppTheme.GridRowEven;
            dgvFiles.DefaultCellStyle.ForeColor = AppTheme.TextPrimary;
            dgvFiles.DefaultCellStyle.SelectionBackColor = AppTheme.GridSelection;
            dgvFiles.DefaultCellStyle.SelectionForeColor = AppTheme.TextPrimary;
            dgvFiles.AlternatingRowsDefaultCellStyle.BackColor = AppTheme.BgDark;
            dgvFiles.AlternatingRowsDefaultCellStyle.ForeColor = AppTheme.TextPrimary;
            dgvFiles.ColumnHeadersDefaultCellStyle.BackColor = AppTheme.GridHeader;
            dgvFiles.ColumnHeadersDefaultCellStyle.ForeColor = AppTheme.TextPrimary;
            dgvFiles.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            dgvFiles.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            // Panels
            panelGrid.BackColor = AppTheme.BgPanel;
            grpConversionOptions.BackColor = AppTheme.BgPanel;
            grpConversionOptions.ForeColor = AppTheme.TextPrimary;
            grpPreEditing.BackColor = AppTheme.BgPanel;
            grpPreEditing.ForeColor = AppTheme.TextPrimary;
            panelOutput.BackColor = AppTheme.BgPanel;
            panelButtons.BackColor = AppTheme.BgDark;

            // ComboBoxes
            StyleComboBox(cmbOutputFormat);
            StyleComboBox(cmbResolution);
            StyleComboBox(cmbVideoBitrate);
            StyleComboBox(cmbAudioBitrate);
            StyleComboBox(cmbFrameRate);

            // TextBoxes
            StyleTextBox(txtTrimStart);
            StyleTextBox(txtTrimEnd);
            StyleTextBox(txtOutputPath);

            // Buttons
            btnImportFiles.BackColor = AppTheme.ButtonPrimary;
            btnImportFiles.ForeColor = Color.White;

            btnRemoveSelected.BackColor = AppTheme.ButtonDanger;
            btnRemoveSelected.ForeColor = Color.White;

            btnConvert.BackColor = AppTheme.ButtonSuccess;
            btnConvert.ForeColor = Color.White;

            btnClose.BackColor = AppTheme.BgLight;
            btnClose.ForeColor = AppTheme.TextPrimary;

            btnBrowseOutput.BackColor = AppTheme.ButtonSecondary;
            btnBrowseOutput.ForeColor = Color.White;

            // Progress
            lblProgress.ForeColor = AppTheme.TextSecondary;
            lblStatus.ForeColor = AppTheme.TextSecondary;
        }

        private void StyleComboBox(ComboBox cmb)
        {
            cmb.BackColor = AppTheme.BgInput;
            cmb.ForeColor = AppTheme.TextPrimary;
        }

        private void StyleTextBox(TextBox txt)
        {
            txt.BackColor = AppTheme.BgInput;
            txt.ForeColor = AppTheme.TextPrimary;
            txt.BorderStyle = BorderStyle.FixedSingle;
        }

        #endregion

        #region Language

        private void ApplyLanguage()
        {
            this.Text = "🎬 " + LanguageManager.GetString("VideoConvertion.Title", "Video Convertion");

            lblTitle.Text = "🎬 " + LanguageManager.GetString("VideoConvertion.Title", "Video Convertion");
            lblFileCount.Text = string.Format(
                LanguageManager.GetString("VideoConvertion.FileCount", "{0} files"),
                _importedFiles.Count);

            // Grid columns
            dgvFiles.Columns["colFileName"].HeaderText =
                LanguageManager.GetString("VideoConvertion.Column.FileName", "File Name");
            dgvFiles.Columns["colDuration"].HeaderText =
                LanguageManager.GetString("VideoConvertion.Column.Duration", "Duration");
            dgvFiles.Columns["colResolution"].HeaderText =
                LanguageManager.GetString("VideoConvertion.Column.Resolution", "Resolution");
            dgvFiles.Columns["colSize"].HeaderText =
                LanguageManager.GetString("VideoConvertion.Column.Size", "Size");
            dgvFiles.Columns["colFormat"].HeaderText =
                LanguageManager.GetString("VideoConvertion.Column.Format", "Format");
            dgvFiles.Columns["colStatus"].HeaderText =
                LanguageManager.GetString("VideoConvertion.Column.Status", "Status");

            // Conversion options
            grpConversionOptions.Text = "⚙ " +
                LanguageManager.GetString("VideoConvertion.Group.ConversionOptions", "Conversion Options");
            lblOutputFormat.Text =
                LanguageManager.GetString("VideoConvertion.Label.OutputFormat", "Output Format:");
            lblResolution.Text =
                LanguageManager.GetString("VideoConvertion.Label.Resolution", "Resolution:");
            lblVideoBitrate.Text =
                LanguageManager.GetString("VideoConvertion.Label.VideoBitrate", "Video Bitrate:");
            lblAudioBitrate.Text =
                LanguageManager.GetString("VideoConvertion.Label.AudioBitrate", "Audio Bitrate:");
            lblFrameRate.Text =
                LanguageManager.GetString("VideoConvertion.Label.FrameRate", "Frame Rate:");

            // Pre-editing
            grpPreEditing.Text = "✂ " +
                LanguageManager.GetString("VideoConvertion.Group.PreEditing", "Pre-Editing (Trim)");
            lblTrimStart.Text =
                LanguageManager.GetString("VideoConvertion.Label.TrimStart", "Start Time:");
            lblTrimEnd.Text =
                LanguageManager.GetString("VideoConvertion.Label.TrimEnd", "End Time:");

            // Output path
            lblOutputPath.Text = "📂 " +
                LanguageManager.GetString("VideoConvertion.Label.OutputPath", "Output Path");

            // Buttons
            btnImportFiles.Text = "📥 " +
                LanguageManager.GetString("VideoConvertion.Button.ImportFiles", "Import Files");
            btnRemoveSelected.Text = "🗑 " +
                LanguageManager.GetString("VideoConvertion.Button.RemoveSelected", "Remove Selected");
            btnConvert.Text = "▶ " +
                LanguageManager.GetString("VideoConvertion.Button.Convert", "Convert");
            btnClose.Text = "✖ " +
                LanguageManager.GetString("Common.Close", "Close");
            btnBrowseOutput.Text = "📂 " +
                LanguageManager.GetString("VideoConvertion.Button.Browse", "Browse");
        }

        #endregion

        #region Event Handlers

        private void BtnImportFiles_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = LanguageManager.GetString("VideoConvertion.Dialog.ImportTitle", "Select Video Files");
                ofd.Filter = LanguageManager.GetString("VideoConvertion.Dialog.Filter", "Video Files") +
                    "|*.mp4;*.avi;*.mkv;*.mov;*.wmv;*.flv;*.webm|" +
                    LanguageManager.GetString("VideoConvertion.Dialog.AllFiles", "All Files") + "|*.*";
                ofd.Multiselect = true;

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    foreach (string file in ofd.FileNames)
                    {
                        if (_importedFiles.Any(f => f.FilePath.Equals(file, StringComparison.OrdinalIgnoreCase)))
                            continue;

                        var info = new VideoFileInfo(file);
                        _importedFiles.Add(info);
                    }

                    RefreshGrid();
                    UpdateFileCount();
                    UpdateStatus(string.Format(
                        LanguageManager.GetString("VideoConvertion.Status.Imported", "{0} file(s) imported"),
                        ofd.FileNames.Length));
                }
            }
        }

        private void BtnRemoveSelected_Click(object sender, EventArgs e)
        {
            if (dgvFiles.SelectedRows.Count == 0)
            {
                MessageBox.Show(
                    LanguageManager.GetString("VideoConvertion.Message.SelectFiles", "Please select files to remove."),
                    LanguageManager.GetString("Common.Warning", "Warning"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            var indicesToRemove = new List<int>();
            foreach (DataGridViewRow row in dgvFiles.SelectedRows)
            {
                indicesToRemove.Add(row.Index);
            }

            foreach (int index in indicesToRemove.OrderByDescending(i => i))
            {
                if (index >= 0 && index < _importedFiles.Count)
                    _importedFiles.RemoveAt(index);
            }

            RefreshGrid();
            UpdateFileCount();
            UpdateStatus(LanguageManager.GetString("VideoConvertion.Status.Removed", "Selected files removed"));
        }

        private async void BtnConvert_Click(object sender, EventArgs e)
        {
            if (_importedFiles.Count == 0)
            {
                MessageBox.Show(
                    LanguageManager.GetString("VideoConvertion.Message.NoFiles", "No files to convert. Please import video files first."),
                    LanguageManager.GetString("Common.Warning", "Warning"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (!Directory.Exists(txtOutputPath.Text))
            {
                MessageBox.Show(
                    LanguageManager.GetString("VideoConvertion.Message.InvalidPath", "Please select a valid output path."),
                    LanguageManager.GetString("Common.Error", "Error"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            // Validate trim times
            if (!IsValidTimeFormat(txtTrimStart.Text) || !IsValidTimeFormat(txtTrimEnd.Text))
            {
                MessageBox.Show(
                    LanguageManager.GetString("VideoConvertion.Message.InvalidTime", "Invalid time format. Use hh:mm:ss."),
                    LanguageManager.GetString("Common.Error", "Error"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            SetConvertingState(true);

            try
            {
                int total = _importedFiles.Count;

                for (int i = 0; i < total; i++)
                {
                    var file = _importedFiles[i];
                    int progressValue = (int)(((double)(i + 1) / total) * 100);

                    UpdateStatus(string.Format(
                        LanguageManager.GetString("VideoConvertion.Status.Converting", "Converting: {0} ({1}/{2})"),
                        file.FileName, i + 1, total));

                    // Update grid row status
                    if (i < dgvFiles.Rows.Count)
                    {
                        dgvFiles.Rows[i].Cells["colStatus"].Value =
                            LanguageManager.GetString("VideoConvertion.Status.InProgress", "Converting...");
                        dgvFiles.Rows[i].Cells["colStatus"].Style.ForeColor = AppTheme.Warning;
                    }

                    // Simulate conversion delay
                    await System.Threading.Tasks.Task.Delay(500);

                    progressConversion.Value = progressValue;
                    lblProgress.Text = progressValue + "%";

                    // Mark as done
                    if (i < dgvFiles.Rows.Count)
                    {
                        dgvFiles.Rows[i].Cells["colStatus"].Value =
                            LanguageManager.GetString("VideoConvertion.Status.Done", "✔ Done");
                        dgvFiles.Rows[i].Cells["colStatus"].Style.ForeColor = AppTheme.Success;
                    }
                }

                progressConversion.Value = 100;
                lblProgress.Text = "100%";
                UpdateStatus(LanguageManager.GetString("VideoConvertion.Status.Complete", "Conversion complete!"));

                MessageBox.Show(
                    string.Format(
                        LanguageManager.GetString("VideoConvertion.Message.Complete", "{0} file(s) converted successfully."),
                        total),
                    LanguageManager.GetString("VideoConvertion.Title", "Video Convertion"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            finally
            {
                SetConvertingState(false);
            }
        }

        private void BtnBrowseOutput_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.Description = LanguageManager.GetString("VideoConvertion.Dialog.OutputFolder", "Select Output Folder");
                fbd.SelectedPath = txtOutputPath.Text;

                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    txtOutputPath.Text = fbd.SelectedPath;
                }
            }
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        #endregion

        #region Helpers

        private void RefreshGrid()
        {
            dgvFiles.Rows.Clear();

            foreach (var file in _importedFiles)
            {
                dgvFiles.Rows.Add(
                    file.FileName,
                    file.Duration,
                    file.Resolution,
                    file.FileSize,
                    file.Format,
                    LanguageManager.GetString("VideoConvertion.Status.Pending", "Pending")
                );
            }
        }

        private void UpdateFileCount()
        {
            lblFileCount.Text = string.Format(
                LanguageManager.GetString("VideoConvertion.FileCount", "{0} files"),
                _importedFiles.Count);
        }

        private void UpdateStatus(string message)
        {
            lblStatus.Text = message;
        }

        private void SetConvertingState(bool converting)
        {
            btnImportFiles.Enabled = !converting;
            btnRemoveSelected.Enabled = !converting;
            btnConvert.Enabled = !converting;
            btnBrowseOutput.Enabled = !converting;
            cmbOutputFormat.Enabled = !converting;
            cmbResolution.Enabled = !converting;
            cmbVideoBitrate.Enabled = !converting;
            cmbAudioBitrate.Enabled = !converting;
            cmbFrameRate.Enabled = !converting;
            txtTrimStart.Enabled = !converting;
            txtTrimEnd.Enabled = !converting;

            if (!converting)
            {
                progressConversion.Value = 0;
                lblProgress.Text = "0%";
            }
        }

        private bool IsValidTimeFormat(string time)
        {
            if (string.IsNullOrWhiteSpace(time))
                return false;

            string[] parts = time.Split(':');
            if (parts.Length != 3)
                return false;

            return int.TryParse(parts[0], out int h) && h >= 0 && h <= 99 &&
                   int.TryParse(parts[1], out int m) && m >= 0 && m <= 59 &&
                   int.TryParse(parts[2], out int s) && s >= 0 && s <= 59;
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            int order = 0;
            double size = bytes;

            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size /= 1024;
            }

            return $"{size:0.##} {sizes[order]}";
        }

        #endregion

        #region VideoFileInfo Class

        private class VideoFileInfo
        {
            public string FilePath { get; set; }
            public string FileName { get; set; }
            public string Duration { get; set; }
            public string Resolution { get; set; }
            public string FileSize { get; set; }
            public string Format { get; set; }

            public VideoFileInfo(string filePath)
            {
                FilePath = filePath;
                FileName = Path.GetFileName(filePath);
                Format = Path.GetExtension(filePath).TrimStart('.').ToUpperInvariant();

                // Populate file size from actual file
                try
                {
                    var fi = new FileInfo(filePath);
                    FileSize = FormatSize(fi.Length);
                }
                catch
                {
                    FileSize = "N/A";
                }

                // Placeholder metadata (actual extraction would require FFprobe/MediaInfo)
                Duration = GeneratePlaceholderDuration();
                Resolution = GeneratePlaceholderResolution();
            }

            private string FormatSize(long bytes)
            {
                string[] sizes = { "B", "KB", "MB", "GB" };
                int order = 0;
                double size = bytes;
                while (size >= 1024 && order < sizes.Length - 1)
                {
                    order++;
                    size /= 1024;
                }
                return $"{size:0.##} {sizes[order]}";
            }

            private string GeneratePlaceholderDuration()
            {
                // Estimate based on file size (~2MB per minute for typical video)
                try
                {
                    var fi = new FileInfo(FilePath);
                    long estimatedSeconds = fi.Length / (2 * 1024 * 1024) * 60;
                    if (estimatedSeconds < 10) estimatedSeconds = 30;
                    TimeSpan ts = TimeSpan.FromSeconds(estimatedSeconds);
                    return ts.ToString(@"hh\:mm\:ss");
                }
                catch
                {
                    return "00:00:00";
                }
            }

            private string GeneratePlaceholderResolution()
            {
                return "1920x1080";
            }
        }

        #endregion
    }
}
