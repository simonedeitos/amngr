namespace AirManager.Forms
{
    partial class MusicEditorForm
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Panel toolbarPanel;
        private System.Windows.Forms.Button btnPlay;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnLoop;

        private System.Windows.Forms.Panel leftPanel;
        private System.Windows.Forms.Label lblCurrentPosition;
        private System.Windows.Forms.Label lblCurrentPositionMs;
        private System.Windows.Forms.Label lblTotalDuration;

        private System.Windows.Forms.Label lblMarkerInLabel;
        private System.Windows.Forms.TextBox txtMarkerIn;
        private System.Windows.Forms.Button btnSetMarkerIn;
        private System.Windows.Forms.Button btnMarkerInUp;
        private System.Windows.Forms.Button btnMarkerInDown;
        private System.Windows.Forms.Button btnPlayFromIn;

        private System.Windows.Forms.Label lblMarkerIntroLabel;
        private System.Windows.Forms.TextBox txtMarkerIntro;
        private System.Windows.Forms.Button btnSetMarkerIntro;
        private System.Windows.Forms.Button btnMarkerIntroUp;
        private System.Windows.Forms.Button btnMarkerIntroDown;
        private System.Windows.Forms.Button btnPlayFromIntro;

        private System.Windows.Forms.Label lblMarkerMixLabel;
        private System.Windows.Forms.TextBox txtMarkerMix;
        private System.Windows.Forms.Button btnSetMarkerMix;
        private System.Windows.Forms.Button btnMarkerMixUp;
        private System.Windows.Forms.Button btnMarkerMixDown;
        private System.Windows.Forms.Button btnPlayFromMix;

        private System.Windows.Forms.Label lblMarkerOutLabel;
        private System.Windows.Forms.TextBox txtMarkerOut;
        private System.Windows.Forms.Button btnSetMarkerOut;
        private System.Windows.Forms.Button btnMarkerOutUp;
        private System.Windows.Forms.Button btnMarkerOutDown;
        private System.Windows.Forms.Button btnPlayFromOut;

        // ✅ VU METER panel (sopra la waveform)
        private System.Windows.Forms.Panel vuMeterPanel;

        private System.Windows.Forms.PictureBox picWaveform;
        private System.Windows.Forms.Panel bottomPanel;

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.TextBox txtTitle;
        private System.Windows.Forms.Label lblArtist;
        private System.Windows.Forms.TextBox txtArtist;
        private System.Windows.Forms.Label lblAlbum;
        private System.Windows.Forms.TextBox txtAlbum;
        private System.Windows.Forms.Label lblYear;
        private System.Windows.Forms.NumericUpDown numYear;
        private System.Windows.Forms.Label lblGenre;
        private System.Windows.Forms.ComboBox cmbGenre;
        private System.Windows.Forms.Label lblCategories;
        private System.Windows.Forms.TextBox txtCategoriesDisplay;
        private System.Windows.Forms.Button btnCategoriesDropdown;
        private System.Windows.Forms.Label lblFeaturedArtists;
        private System.Windows.Forms.TextBox txtFeaturedArtistsDisplay;
        private System.Windows.Forms.Button btnFeaturedArtistsDropdown;
        private System.Windows.Forms.Label lblFilePath;
        private System.Windows.Forms.TextBox txtFilePath;

        private System.Windows.Forms.GroupBox grpPeriod;
        private System.Windows.Forms.CheckBox chkEnableValidFrom;
        private System.Windows.Forms.DateTimePicker dtpValidFrom;
        private System.Windows.Forms.CheckBox chkEnableValidTo;
        private System.Windows.Forms.DateTimePicker dtpValidTo;

        private System.Windows.Forms.GroupBox grpMonths;
        private System.Windows.Forms.GroupBox grpDays;
        private System.Windows.Forms.GroupBox grpHours;

        // ✅ VOLUME BOOST
        private System.Windows.Forms.GroupBox grpVolume;
        private System.Windows.Forms.TrackBar trkVolume;
        private System.Windows.Forms.Label lblVolumeDb;
        private System.Windows.Forms.Button btnApplyVolume;
        private System.Windows.Forms.CheckBox chkColoredPeaks;

        // ✅ ZOOM
        private System.Windows.Forms.Panel zoomPanel;
        private System.Windows.Forms.Label lblZoom;
        private System.Windows.Forms.TrackBar trkZoom;
        private System.Windows.Forms.Label lblZoomPercent;
        private System.Windows.Forms.HScrollBar hScrollWaveform;

        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;

        private void InitializeComponent()
        {
            toolbarPanel = new Panel();
            btnPlay = new Button();
            btnStop = new Button();
            btnLoop = new Button();
            zoomPanel = new Panel();
            lblZoom = new Label();
            trkZoom = new TrackBar();
            lblZoomPercent = new Label();
            grpVolume = new GroupBox();
            trkVolume = new TrackBar();
            lblVolumeDb = new Label();
            btnApplyVolume = new Button();
            chkColoredPeaks = new CheckBox();
            leftPanel = new Panel();
            lblCurrentPosition = new Label();
            lblCurrentPositionMs = new Label();
            lblTotalDuration = new Label();
            lblMarkerInLabel = new Label();
            txtMarkerIn = new TextBox();
            btnSetMarkerIn = new Button();
            btnMarkerInUp = new Button();
            btnMarkerInDown = new Button();
            btnPlayFromIn = new Button();
            lblMarkerIntroLabel = new Label();
            txtMarkerIntro = new TextBox();
            btnSetMarkerIntro = new Button();
            btnMarkerIntroUp = new Button();
            btnMarkerIntroDown = new Button();
            btnPlayFromIntro = new Button();
            lblMarkerMixLabel = new Label();
            txtMarkerMix = new TextBox();
            btnSetMarkerMix = new Button();
            btnMarkerMixUp = new Button();
            btnMarkerMixDown = new Button();
            btnPlayFromMix = new Button();
            lblMarkerOutLabel = new Label();
            txtMarkerOut = new TextBox();
            btnSetMarkerOut = new Button();
            btnMarkerOutUp = new Button();
            btnMarkerOutDown = new Button();
            btnPlayFromOut = new Button();
            vuMeterPanel = new Panel();
            picWaveform = new PictureBox();
            hScrollWaveform = new HScrollBar();
            bottomPanel = new Panel();
            lblTitle = new Label();
            txtTitle = new TextBox();
            lblArtist = new Label();
            txtArtist = new TextBox();
            lblAlbum = new Label();
            txtAlbum = new TextBox();
            lblYear = new Label();
            numYear = new NumericUpDown();
            lblGenre = new Label();
            cmbGenre = new ComboBox();
            lblCategories = new Label();
            txtCategoriesDisplay = new TextBox();
            btnCategoriesDropdown = new Button();
            lblFeaturedArtists = new Label();
            txtFeaturedArtistsDisplay = new TextBox();
            btnFeaturedArtistsDropdown = new Button();
            lblFilePath = new Label();
            txtFilePath = new TextBox();
            grpPeriod = new GroupBox();
            chkEnableValidFrom = new CheckBox();
            dtpValidFrom = new DateTimePicker();
            chkEnableValidTo = new CheckBox();
            dtpValidTo = new DateTimePicker();
            grpMonths = new GroupBox();
            grpDays = new GroupBox();
            grpHours = new GroupBox();
            btnSave = new Button();
            btnCancel = new Button();
            toolbarPanel.SuspendLayout();
            zoomPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trkZoom).BeginInit();
            grpVolume.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trkVolume).BeginInit();
            leftPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picWaveform).BeginInit();
            bottomPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numYear).BeginInit();
            grpPeriod.SuspendLayout();
            SuspendLayout();
            // 
            // toolbarPanel
            // 
            toolbarPanel.Controls.Add(btnPlay);
            toolbarPanel.Controls.Add(btnStop);
            toolbarPanel.Controls.Add(btnLoop);
            toolbarPanel.Controls.Add(zoomPanel);
            toolbarPanel.Controls.Add(grpVolume);
            toolbarPanel.Dock = DockStyle.Top;
            toolbarPanel.Location = new Point(0, 0);
            toolbarPanel.Name = "toolbarPanel";
            toolbarPanel.Size = new Size(1263, 55);
            toolbarPanel.TabIndex = 0;
            // 
            // btnPlay
            // 
            btnPlay.FlatStyle = FlatStyle.Flat;
            btnPlay.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnPlay.Location = new Point(15, 8);
            btnPlay.Name = "btnPlay";
            btnPlay.Size = new Size(90, 38);
            btnPlay.TabIndex = 0;
            btnPlay.Text = "▶ PLAY";
            btnPlay.UseVisualStyleBackColor = true;
            btnPlay.Click += btnPlay_Click;
            // 
            // btnStop
            // 
            btnStop.FlatStyle = FlatStyle.Flat;
            btnStop.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnStop.Location = new Point(115, 8);
            btnStop.Name = "btnStop";
            btnStop.Size = new Size(90, 38);
            btnStop.TabIndex = 1;
            btnStop.Text = "⏹ STOP";
            btnStop.UseVisualStyleBackColor = true;
            btnStop.Click += btnStop_Click;
            // 
            // btnLoop
            // 
            btnLoop.FlatStyle = FlatStyle.Flat;
            btnLoop.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnLoop.Location = new Point(1161, 8);
            btnLoop.Name = "btnLoop";
            btnLoop.Size = new Size(90, 38);
            btnLoop.TabIndex = 2;
            btnLoop.Text = "🔁 LOOP";
            btnLoop.UseVisualStyleBackColor = true;
            btnLoop.Visible = false;
            // 
            // zoomPanel
            // 
            zoomPanel.Controls.Add(lblZoom);
            zoomPanel.Controls.Add(trkZoom);
            zoomPanel.Controls.Add(lblZoomPercent);
            zoomPanel.Location = new Point(220, 3);
            zoomPanel.Name = "zoomPanel";
            zoomPanel.Size = new Size(300, 50);
            zoomPanel.TabIndex = 3;
            // 
            // lblZoom
            // 
            lblZoom.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblZoom.ForeColor = Color.White;
            lblZoom.Location = new Point(0, 15);
            lblZoom.Name = "lblZoom";
            lblZoom.Size = new Size(60, 20);
            lblZoom.TabIndex = 0;
            lblZoom.Text = "🔍 Zoom:";
            lblZoom.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // trkZoom
            // 
            trkZoom.LargeChange = 100;
            trkZoom.Location = new Point(62, 5);
            trkZoom.Maximum = 2000;
            trkZoom.Minimum = 100;
            trkZoom.Name = "trkZoom";
            trkZoom.Size = new Size(180, 45);
            trkZoom.SmallChange = 10;
            trkZoom.TabIndex = 0;
            trkZoom.TickFrequency = 100;
            trkZoom.Value = 100;
            // 
            // lblZoomPercent
            // 
            lblZoomPercent.Font = new Font("Consolas", 10F, FontStyle.Bold);
            lblZoomPercent.ForeColor = Color.Cyan;
            lblZoomPercent.Location = new Point(245, 15);
            lblZoomPercent.Name = "lblZoomPercent";
            lblZoomPercent.Size = new Size(55, 20);
            lblZoomPercent.TabIndex = 1;
            lblZoomPercent.Text = "100%";
            lblZoomPercent.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // grpVolume
            // 
            grpVolume.Controls.Add(trkVolume);
            grpVolume.Controls.Add(lblVolumeDb);
            grpVolume.Controls.Add(btnApplyVolume);
            grpVolume.Controls.Add(chkColoredPeaks);
            grpVolume.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            grpVolume.ForeColor = Color.White;
            grpVolume.Location = new Point(530, 0);
            grpVolume.Name = "grpVolume";
            grpVolume.Size = new Size(560, 50);
            grpVolume.TabIndex = 4;
            grpVolume.TabStop = false;
            grpVolume.Text = "🔊 Volume Boost";
            // 
            // trkVolume
            // 
            trkVolume.LargeChange = 3;
            trkVolume.Location = new Point(10, 18);
            trkVolume.Maximum = 20;
            trkVolume.Minimum = -20;
            trkVolume.Name = "trkVolume";
            trkVolume.Size = new Size(240, 45);
            trkVolume.TabIndex = 0;
            trkVolume.TickFrequency = 2;
            // 
            // lblVolumeDb
            // 
            lblVolumeDb.Font = new Font("Consolas", 11F, FontStyle.Bold);
            lblVolumeDb.ForeColor = Color.Lime;
            lblVolumeDb.Location = new Point(255, 20);
            lblVolumeDb.Name = "lblVolumeDb";
            lblVolumeDb.Size = new Size(65, 25);
            lblVolumeDb.TabIndex = 1;
            lblVolumeDb.Text = "0 dB";
            lblVolumeDb.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // btnApplyVolume
            // 
            btnApplyVolume.BackColor = Color.FromArgb(200, 120, 0);
            btnApplyVolume.Cursor = Cursors.Hand;
            btnApplyVolume.FlatAppearance.BorderSize = 0;
            btnApplyVolume.FlatStyle = FlatStyle.Flat;
            btnApplyVolume.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            btnApplyVolume.ForeColor = Color.White;
            btnApplyVolume.Location = new Point(325, 17);
            btnApplyVolume.Name = "btnApplyVolume";
            btnApplyVolume.Size = new Size(85, 30);
            btnApplyVolume.TabIndex = 1;
            btnApplyVolume.Text = "APPLY";
            btnApplyVolume.UseVisualStyleBackColor = false;
            // 
            // chkColoredPeaks
            // 
            chkColoredPeaks.BackColor = Color.Transparent;
            chkColoredPeaks.Cursor = Cursors.Hand;
            chkColoredPeaks.Font = new Font("Segoe UI", 8F);
            chkColoredPeaks.ForeColor = Color.White;
            chkColoredPeaks.Location = new Point(420, 19);
            chkColoredPeaks.Name = "chkColoredPeaks";
            chkColoredPeaks.Size = new Size(130, 22);
            chkColoredPeaks.TabIndex = 2;
            chkColoredPeaks.Text = "🎨 Colored peaks";
            chkColoredPeaks.UseVisualStyleBackColor = false;
            chkColoredPeaks.CheckedChanged += ChkColoredPeaks_CheckedChanged;
            // 
            // leftPanel
            // 
            leftPanel.Controls.Add(lblCurrentPosition);
            leftPanel.Controls.Add(lblCurrentPositionMs);
            leftPanel.Controls.Add(lblTotalDuration);
            leftPanel.Controls.Add(lblMarkerInLabel);
            leftPanel.Controls.Add(txtMarkerIn);
            leftPanel.Controls.Add(btnSetMarkerIn);
            leftPanel.Controls.Add(btnMarkerInUp);
            leftPanel.Controls.Add(btnMarkerInDown);
            leftPanel.Controls.Add(btnPlayFromIn);
            leftPanel.Controls.Add(lblMarkerIntroLabel);
            leftPanel.Controls.Add(txtMarkerIntro);
            leftPanel.Controls.Add(btnSetMarkerIntro);
            leftPanel.Controls.Add(btnMarkerIntroUp);
            leftPanel.Controls.Add(btnMarkerIntroDown);
            leftPanel.Controls.Add(btnPlayFromIntro);
            leftPanel.Controls.Add(lblMarkerMixLabel);
            leftPanel.Controls.Add(txtMarkerMix);
            leftPanel.Controls.Add(btnSetMarkerMix);
            leftPanel.Controls.Add(btnMarkerMixUp);
            leftPanel.Controls.Add(btnMarkerMixDown);
            leftPanel.Controls.Add(btnPlayFromMix);
            leftPanel.Controls.Add(lblMarkerOutLabel);
            leftPanel.Controls.Add(txtMarkerOut);
            leftPanel.Controls.Add(btnSetMarkerOut);
            leftPanel.Controls.Add(btnMarkerOutUp);
            leftPanel.Controls.Add(btnMarkerOutDown);
            leftPanel.Controls.Add(btnPlayFromOut);
            leftPanel.Dock = DockStyle.Left;
            leftPanel.Location = new Point(0, 55);
            leftPanel.Name = "leftPanel";
            leftPanel.Size = new Size(380, 355);
            leftPanel.TabIndex = 1;
            // 
            // lblCurrentPosition
            // 
            lblCurrentPosition.BorderStyle = BorderStyle.FixedSingle;
            lblCurrentPosition.Font = new Font("Consolas", 16F, FontStyle.Bold);
            lblCurrentPosition.Location = new Point(16, 182);
            lblCurrentPosition.Name = "lblCurrentPosition";
            lblCurrentPosition.Size = new Size(220, 40);
            lblCurrentPosition.TabIndex = 0;
            lblCurrentPosition.Text = "00:00:00.000";
            lblCurrentPosition.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblCurrentPositionMs
            // 
            lblCurrentPositionMs.BorderStyle = BorderStyle.FixedSingle;
            lblCurrentPositionMs.Font = new Font("Consolas", 10F, FontStyle.Bold);
            lblCurrentPositionMs.Location = new Point(246, 182);
            lblCurrentPositionMs.Name = "lblCurrentPositionMs";
            lblCurrentPositionMs.Size = new Size(120, 40);
            lblCurrentPositionMs.TabIndex = 1;
            lblCurrentPositionMs.Text = "0 ms";
            lblCurrentPositionMs.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblTotalDuration
            // 
            lblTotalDuration.BorderStyle = BorderStyle.FixedSingle;
            lblTotalDuration.Font = new Font("Consolas", 12F, FontStyle.Bold);
            lblTotalDuration.Location = new Point(15, 234);
            lblTotalDuration.Name = "lblTotalDuration";
            lblTotalDuration.Size = new Size(350, 35);
            lblTotalDuration.TabIndex = 2;
            lblTotalDuration.Text = "00:00:00.000";
            lblTotalDuration.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblMarkerInLabel
            // 
            lblMarkerInLabel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblMarkerInLabel.ForeColor = Color.Red;
            lblMarkerInLabel.Location = new Point(9, 15);
            lblMarkerInLabel.Name = "lblMarkerInLabel";
            lblMarkerInLabel.Size = new Size(50, 28);
            lblMarkerInLabel.TabIndex = 3;
            lblMarkerInLabel.Text = "IN";
            lblMarkerInLabel.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtMarkerIn
            // 
            txtMarkerIn.Font = new Font("Consolas", 12F);
            txtMarkerIn.Location = new Point(64, 15);
            txtMarkerIn.Name = "txtMarkerIn";
            txtMarkerIn.ReadOnly = true;
            txtMarkerIn.Size = new Size(141, 26);
            txtMarkerIn.TabIndex = 4;
            txtMarkerIn.Text = "00:00:00.000";
            txtMarkerIn.TextAlign = HorizontalAlignment.Center;
            // 
            // btnSetMarkerIn
            // 
            btnSetMarkerIn.BackColor = SystemColors.ControlLightLight;
            btnSetMarkerIn.FlatStyle = FlatStyle.Flat;
            btnSetMarkerIn.Font = new Font("Arial", 10F, FontStyle.Bold);
            btnSetMarkerIn.Location = new Point(213, 15);
            btnSetMarkerIn.Name = "btnSetMarkerIn";
            btnSetMarkerIn.Size = new Size(30, 28);
            btnSetMarkerIn.TabIndex = 5;
            btnSetMarkerIn.Text = "⬇";
            btnSetMarkerIn.UseVisualStyleBackColor = false;
            btnSetMarkerIn.Click += btnSetMarkerIn_Click;
            // 
            // btnMarkerInUp
            // 
            btnMarkerInUp.BackColor = SystemColors.ControlLightLight;
            btnMarkerInUp.FlatStyle = FlatStyle.Flat;
            btnMarkerInUp.Font = new Font("Arial", 7F, FontStyle.Bold);
            btnMarkerInUp.Location = new Point(274, 10);
            btnMarkerInUp.Name = "btnMarkerInUp";
            btnMarkerInUp.Size = new Size(22, 26);
            btnMarkerInUp.TabIndex = 6;
            btnMarkerInUp.Text = "▲";
            btnMarkerInUp.UseVisualStyleBackColor = false;
            btnMarkerInUp.Click += btnMarkerInUp_Click;
            // 
            // btnMarkerInDown
            // 
            btnMarkerInDown.BackColor = SystemColors.ControlLightLight;
            btnMarkerInDown.FlatStyle = FlatStyle.Flat;
            btnMarkerInDown.Font = new Font("Arial", 7F, FontStyle.Bold);
            btnMarkerInDown.Location = new Point(249, 20);
            btnMarkerInDown.Name = "btnMarkerInDown";
            btnMarkerInDown.Size = new Size(22, 26);
            btnMarkerInDown.TabIndex = 7;
            btnMarkerInDown.Text = "▼";
            btnMarkerInDown.UseVisualStyleBackColor = false;
            btnMarkerInDown.Click += btnMarkerInDown_Click;
            // 
            // btnPlayFromIn
            // 
            btnPlayFromIn.BackColor = Color.LawnGreen;
            btnPlayFromIn.FlatStyle = FlatStyle.Flat;
            btnPlayFromIn.Font = new Font("Arial", 10F, FontStyle.Bold);
            btnPlayFromIn.Location = new Point(301, 15);
            btnPlayFromIn.Name = "btnPlayFromIn";
            btnPlayFromIn.Size = new Size(30, 28);
            btnPlayFromIn.TabIndex = 8;
            btnPlayFromIn.Text = "▶";
            btnPlayFromIn.UseVisualStyleBackColor = false;
            btnPlayFromIn.Click += btnPlayFromIn_Click;
            // 
            // lblMarkerIntroLabel
            // 
            lblMarkerIntroLabel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblMarkerIntroLabel.ForeColor = Color.Magenta;
            lblMarkerIntroLabel.Location = new Point(-4, 55);
            lblMarkerIntroLabel.Name = "lblMarkerIntroLabel";
            lblMarkerIntroLabel.Size = new Size(63, 28);
            lblMarkerIntroLabel.TabIndex = 9;
            lblMarkerIntroLabel.Text = "INTRO";
            lblMarkerIntroLabel.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtMarkerIntro
            // 
            txtMarkerIntro.Font = new Font("Consolas", 12F);
            txtMarkerIntro.Location = new Point(64, 55);
            txtMarkerIntro.Name = "txtMarkerIntro";
            txtMarkerIntro.ReadOnly = true;
            txtMarkerIntro.Size = new Size(141, 26);
            txtMarkerIntro.TabIndex = 10;
            txtMarkerIntro.Text = "00:00:00.000";
            txtMarkerIntro.TextAlign = HorizontalAlignment.Center;
            // 
            // btnSetMarkerIntro
            // 
            btnSetMarkerIntro.BackColor = SystemColors.ControlLightLight;
            btnSetMarkerIntro.FlatStyle = FlatStyle.Flat;
            btnSetMarkerIntro.Font = new Font("Arial", 10F, FontStyle.Bold);
            btnSetMarkerIntro.Location = new Point(213, 55);
            btnSetMarkerIntro.Name = "btnSetMarkerIntro";
            btnSetMarkerIntro.Size = new Size(30, 28);
            btnSetMarkerIntro.TabIndex = 11;
            btnSetMarkerIntro.Text = "⬇";
            btnSetMarkerIntro.UseVisualStyleBackColor = false;
            btnSetMarkerIntro.Click += btnSetMarkerIntro_Click;
            // 
            // btnMarkerIntroUp
            // 
            btnMarkerIntroUp.BackColor = SystemColors.ControlLightLight;
            btnMarkerIntroUp.FlatStyle = FlatStyle.Flat;
            btnMarkerIntroUp.Font = new Font("Arial", 7F, FontStyle.Bold);
            btnMarkerIntroUp.Location = new Point(274, 50);
            btnMarkerIntroUp.Name = "btnMarkerIntroUp";
            btnMarkerIntroUp.Size = new Size(21, 26);
            btnMarkerIntroUp.TabIndex = 12;
            btnMarkerIntroUp.Text = "▲";
            btnMarkerIntroUp.UseVisualStyleBackColor = false;
            btnMarkerIntroUp.Click += btnMarkerIntroUp_Click;
            // 
            // btnMarkerIntroDown
            // 
            btnMarkerIntroDown.BackColor = SystemColors.ControlLightLight;
            btnMarkerIntroDown.FlatStyle = FlatStyle.Flat;
            btnMarkerIntroDown.Font = new Font("Arial", 7F, FontStyle.Bold);
            btnMarkerIntroDown.Location = new Point(249, 60);
            btnMarkerIntroDown.Name = "btnMarkerIntroDown";
            btnMarkerIntroDown.Size = new Size(22, 26);
            btnMarkerIntroDown.TabIndex = 13;
            btnMarkerIntroDown.Text = "▼";
            btnMarkerIntroDown.UseVisualStyleBackColor = false;
            btnMarkerIntroDown.Click += btnMarkerIntroDown_Click;
            // 
            // btnPlayFromIntro
            // 
            btnPlayFromIntro.BackColor = Color.LawnGreen;
            btnPlayFromIntro.FlatStyle = FlatStyle.Flat;
            btnPlayFromIntro.Font = new Font("Arial", 10F, FontStyle.Bold);
            btnPlayFromIntro.Location = new Point(301, 55);
            btnPlayFromIntro.Name = "btnPlayFromIntro";
            btnPlayFromIntro.Size = new Size(30, 28);
            btnPlayFromIntro.TabIndex = 14;
            btnPlayFromIntro.Text = "▶";
            btnPlayFromIntro.UseVisualStyleBackColor = false;
            btnPlayFromIntro.Click += btnPlayFromIntro_Click;
            // 
            // lblMarkerMixLabel
            // 
            lblMarkerMixLabel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblMarkerMixLabel.ForeColor = Color.Yellow;
            lblMarkerMixLabel.Location = new Point(9, 95);
            lblMarkerMixLabel.Name = "lblMarkerMixLabel";
            lblMarkerMixLabel.Size = new Size(50, 28);
            lblMarkerMixLabel.TabIndex = 15;
            lblMarkerMixLabel.Text = "MIX";
            lblMarkerMixLabel.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtMarkerMix
            // 
            txtMarkerMix.Font = new Font("Consolas", 12F);
            txtMarkerMix.Location = new Point(64, 95);
            txtMarkerMix.Name = "txtMarkerMix";
            txtMarkerMix.ReadOnly = true;
            txtMarkerMix.Size = new Size(141, 26);
            txtMarkerMix.TabIndex = 16;
            txtMarkerMix.Text = "00:00:00.000";
            txtMarkerMix.TextAlign = HorizontalAlignment.Center;
            // 
            // btnSetMarkerMix
            // 
            btnSetMarkerMix.BackColor = SystemColors.ControlLightLight;
            btnSetMarkerMix.FlatStyle = FlatStyle.Flat;
            btnSetMarkerMix.Font = new Font("Arial", 10F, FontStyle.Bold);
            btnSetMarkerMix.Location = new Point(213, 95);
            btnSetMarkerMix.Name = "btnSetMarkerMix";
            btnSetMarkerMix.Size = new Size(30, 28);
            btnSetMarkerMix.TabIndex = 17;
            btnSetMarkerMix.Text = "⬇";
            btnSetMarkerMix.UseVisualStyleBackColor = false;
            btnSetMarkerMix.Click += btnSetMarkerMix_Click;
            // 
            // btnMarkerMixUp
            // 
            btnMarkerMixUp.BackColor = SystemColors.ControlLightLight;
            btnMarkerMixUp.FlatStyle = FlatStyle.Flat;
            btnMarkerMixUp.Font = new Font("Arial", 7F, FontStyle.Bold);
            btnMarkerMixUp.Location = new Point(274, 90);
            btnMarkerMixUp.Name = "btnMarkerMixUp";
            btnMarkerMixUp.Size = new Size(22, 26);
            btnMarkerMixUp.TabIndex = 18;
            btnMarkerMixUp.Text = "▲";
            btnMarkerMixUp.UseVisualStyleBackColor = false;
            btnMarkerMixUp.Click += btnMarkerMixUp_Click;
            // 
            // btnMarkerMixDown
            // 
            btnMarkerMixDown.BackColor = SystemColors.ControlLightLight;
            btnMarkerMixDown.FlatStyle = FlatStyle.Flat;
            btnMarkerMixDown.Font = new Font("Arial", 7F, FontStyle.Bold);
            btnMarkerMixDown.Location = new Point(249, 100);
            btnMarkerMixDown.Name = "btnMarkerMixDown";
            btnMarkerMixDown.Size = new Size(22, 26);
            btnMarkerMixDown.TabIndex = 19;
            btnMarkerMixDown.Text = "▼";
            btnMarkerMixDown.UseVisualStyleBackColor = false;
            btnMarkerMixDown.Click += btnMarkerMixDown_Click;
            // 
            // btnPlayFromMix
            // 
            btnPlayFromMix.BackColor = Color.LawnGreen;
            btnPlayFromMix.FlatStyle = FlatStyle.Flat;
            btnPlayFromMix.Font = new Font("Arial", 10F, FontStyle.Bold);
            btnPlayFromMix.Location = new Point(301, 95);
            btnPlayFromMix.Name = "btnPlayFromMix";
            btnPlayFromMix.Size = new Size(30, 28);
            btnPlayFromMix.TabIndex = 20;
            btnPlayFromMix.Text = "▶";
            btnPlayFromMix.UseVisualStyleBackColor = false;
            btnPlayFromMix.Click += btnPlayFromMix_Click;
            // 
            // lblMarkerOutLabel
            // 
            lblMarkerOutLabel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblMarkerOutLabel.ForeColor = Color.FromArgb(255, 140, 0);
            lblMarkerOutLabel.Location = new Point(9, 135);
            lblMarkerOutLabel.Name = "lblMarkerOutLabel";
            lblMarkerOutLabel.Size = new Size(50, 28);
            lblMarkerOutLabel.TabIndex = 21;
            lblMarkerOutLabel.Text = "OUT";
            lblMarkerOutLabel.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtMarkerOut
            // 
            txtMarkerOut.Font = new Font("Consolas", 12F);
            txtMarkerOut.Location = new Point(64, 135);
            txtMarkerOut.Name = "txtMarkerOut";
            txtMarkerOut.ReadOnly = true;
            txtMarkerOut.Size = new Size(141, 26);
            txtMarkerOut.TabIndex = 22;
            txtMarkerOut.Text = "00:00:00.000";
            txtMarkerOut.TextAlign = HorizontalAlignment.Center;
            // 
            // btnSetMarkerOut
            // 
            btnSetMarkerOut.BackColor = SystemColors.ControlLightLight;
            btnSetMarkerOut.FlatStyle = FlatStyle.Flat;
            btnSetMarkerOut.Font = new Font("Arial", 10F, FontStyle.Bold);
            btnSetMarkerOut.Location = new Point(213, 135);
            btnSetMarkerOut.Name = "btnSetMarkerOut";
            btnSetMarkerOut.Size = new Size(30, 28);
            btnSetMarkerOut.TabIndex = 23;
            btnSetMarkerOut.Text = "⬇";
            btnSetMarkerOut.UseVisualStyleBackColor = false;
            btnSetMarkerOut.Click += btnSetMarkerOut_Click;
            // 
            // btnMarkerOutUp
            // 
            btnMarkerOutUp.BackColor = SystemColors.ControlLightLight;
            btnMarkerOutUp.FlatStyle = FlatStyle.Flat;
            btnMarkerOutUp.Font = new Font("Arial", 7F, FontStyle.Bold);
            btnMarkerOutUp.Location = new Point(274, 131);
            btnMarkerOutUp.Name = "btnMarkerOutUp";
            btnMarkerOutUp.Size = new Size(22, 26);
            btnMarkerOutUp.TabIndex = 24;
            btnMarkerOutUp.Text = "▲";
            btnMarkerOutUp.UseVisualStyleBackColor = false;
            btnMarkerOutUp.Click += btnMarkerOutUp_Click;
            // 
            // btnMarkerOutDown
            // 
            btnMarkerOutDown.BackColor = SystemColors.ControlLightLight;
            btnMarkerOutDown.FlatStyle = FlatStyle.Flat;
            btnMarkerOutDown.Font = new Font("Arial", 7F, FontStyle.Bold);
            btnMarkerOutDown.Location = new Point(249, 140);
            btnMarkerOutDown.Name = "btnMarkerOutDown";
            btnMarkerOutDown.Size = new Size(22, 26);
            btnMarkerOutDown.TabIndex = 25;
            btnMarkerOutDown.Text = "▼";
            btnMarkerOutDown.UseVisualStyleBackColor = false;
            btnMarkerOutDown.Click += btnMarkerOutDown_Click;
            // 
            // btnPlayFromOut
            // 
            btnPlayFromOut.BackColor = Color.LawnGreen;
            btnPlayFromOut.FlatStyle = FlatStyle.Flat;
            btnPlayFromOut.Font = new Font("Arial", 10F, FontStyle.Bold);
            btnPlayFromOut.Location = new Point(301, 135);
            btnPlayFromOut.Name = "btnPlayFromOut";
            btnPlayFromOut.Size = new Size(30, 28);
            btnPlayFromOut.TabIndex = 26;
            btnPlayFromOut.Text = "▶";
            btnPlayFromOut.UseVisualStyleBackColor = false;
            btnPlayFromOut.Click += btnPlayFromOut_Click;
            // 
            // vuMeterPanel
            // 
            vuMeterPanel.BackColor = Color.FromArgb(15, 15, 15);
            vuMeterPanel.Dock = DockStyle.Top;
            vuMeterPanel.Location = new Point(380, 55);
            vuMeterPanel.Name = "vuMeterPanel";
            vuMeterPanel.Size = new Size(883, 24);
            vuMeterPanel.TabIndex = 10;
            // 
            // picWaveform
            // 
            picWaveform.BorderStyle = BorderStyle.FixedSingle;
            picWaveform.Dock = DockStyle.Fill;
            picWaveform.Location = new Point(380, 79);
            picWaveform.Name = "picWaveform";
            picWaveform.Size = new Size(883, 311);
            picWaveform.TabIndex = 2;
            picWaveform.TabStop = false;
            picWaveform.Paint += picWaveform_Paint;
            picWaveform.MouseDown += picWaveform_MouseDown;
            picWaveform.MouseMove += picWaveform_MouseMove;
            picWaveform.MouseUp += picWaveform_MouseUp;
            // 
            // hScrollWaveform
            // 
            hScrollWaveform.Dock = DockStyle.Bottom;
            hScrollWaveform.Location = new Point(380, 390);
            hScrollWaveform.Name = "hScrollWaveform";
            hScrollWaveform.Size = new Size(883, 20);
            hScrollWaveform.TabIndex = 11;
            hScrollWaveform.Visible = false;
            // 
            // bottomPanel
            // 
            bottomPanel.AutoScroll = true;
            bottomPanel.Controls.Add(lblTitle);
            bottomPanel.Controls.Add(txtTitle);
            bottomPanel.Controls.Add(lblArtist);
            bottomPanel.Controls.Add(txtArtist);
            bottomPanel.Controls.Add(lblAlbum);
            bottomPanel.Controls.Add(txtAlbum);
            bottomPanel.Controls.Add(lblYear);
            bottomPanel.Controls.Add(numYear);
            bottomPanel.Controls.Add(lblGenre);
            bottomPanel.Controls.Add(cmbGenre);
            bottomPanel.Controls.Add(lblCategories);
            bottomPanel.Controls.Add(txtCategoriesDisplay);
            bottomPanel.Controls.Add(btnCategoriesDropdown);
            bottomPanel.Controls.Add(lblFeaturedArtists);
            bottomPanel.Controls.Add(txtFeaturedArtistsDisplay);
            bottomPanel.Controls.Add(btnFeaturedArtistsDropdown);
            bottomPanel.Controls.Add(lblFilePath);
            bottomPanel.Controls.Add(txtFilePath);
            bottomPanel.Controls.Add(grpPeriod);
            bottomPanel.Controls.Add(grpMonths);
            bottomPanel.Controls.Add(grpDays);
            bottomPanel.Controls.Add(grpHours);
            bottomPanel.Controls.Add(btnSave);
            bottomPanel.Controls.Add(btnCancel);
            bottomPanel.Dock = DockStyle.Bottom;
            bottomPanel.Location = new Point(0, 410);
            bottomPanel.Name = "bottomPanel";
            bottomPanel.Size = new Size(1263, 352);
            bottomPanel.TabIndex = 3;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblTitle.Location = new Point(15, 18);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(42, 15);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Titolo:";
            // 
            // txtTitle
            // 
            txtTitle.Font = new Font("Segoe UI", 10F);
            txtTitle.Location = new Point(100, 15);
            txtTitle.Name = "txtTitle";
            txtTitle.Size = new Size(420, 25);
            txtTitle.TabIndex = 1;
            // 
            // lblArtist
            // 
            lblArtist.AutoSize = true;
            lblArtist.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblArtist.Location = new Point(15, 53);
            lblArtist.Name = "lblArtist";
            lblArtist.Size = new Size(47, 15);
            lblArtist.TabIndex = 2;
            lblArtist.Text = "Artista:";
            // 
            // txtArtist
            // 
            txtArtist.Font = new Font("Segoe UI", 10F);
            txtArtist.Location = new Point(100, 50);
            txtArtist.Name = "txtArtist";
            txtArtist.Size = new Size(420, 25);
            txtArtist.TabIndex = 3;
            // 
            // lblAlbum
            // 
            lblAlbum.AutoSize = true;
            lblAlbum.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblAlbum.Location = new Point(540, 18);
            lblAlbum.Name = "lblAlbum";
            lblAlbum.Size = new Size(46, 15);
            lblAlbum.TabIndex = 4;
            lblAlbum.Text = "Album:";
            // 
            // txtAlbum
            // 
            txtAlbum.Font = new Font("Segoe UI", 10F);
            txtAlbum.Location = new Point(620, 15);
            txtAlbum.Name = "txtAlbum";
            txtAlbum.Size = new Size(280, 25);
            txtAlbum.TabIndex = 5;
            // 
            // lblYear
            // 
            lblYear.AutoSize = true;
            lblYear.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblYear.Location = new Point(920, 18);
            lblYear.Name = "lblYear";
            lblYear.Size = new Size(39, 15);
            lblYear.TabIndex = 6;
            lblYear.Text = "Anno:";
            // 
            // numYear
            // 
            numYear.Font = new Font("Segoe UI", 10F);
            numYear.Location = new Point(980, 15);
            numYear.Maximum = new decimal(new int[] { 2100, 0, 0, 0 });
            numYear.Minimum = new decimal(new int[] { 1900, 0, 0, 0 });
            numYear.Name = "numYear";
            numYear.Size = new Size(100, 25);
            numYear.TabIndex = 7;
            numYear.Value = new decimal(new int[] { 2024, 0, 0, 0 });
            // 
            // lblGenre
            // 
            lblGenre.AutoSize = true;
            lblGenre.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblGenre.Location = new Point(540, 53);
            lblGenre.Name = "lblGenre";
            lblGenre.Size = new Size(52, 15);
            lblGenre.TabIndex = 8;
            lblGenre.Text = "Genere:";
            // 
            // cmbGenre
            // 
            cmbGenre.Font = new Font("Segoe UI", 10F);
            cmbGenre.FormattingEnabled = true;
            cmbGenre.Location = new Point(620, 50);
            cmbGenre.Name = "cmbGenre";
            cmbGenre.Size = new Size(200, 25);
            cmbGenre.TabIndex = 9;
            // 
            // lblCategories
            // 
            lblCategories.AutoSize = true;
            lblCategories.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblCategories.Location = new Point(840, 53);
            lblCategories.Name = "lblCategories";
            lblCategories.Size = new Size(64, 15);
            lblCategories.TabIndex = 10;
            lblCategories.Text = "Categorie:";
            // 
            // txtCategoriesDisplay
            // 
            txtCategoriesDisplay.Cursor = Cursors.Hand;
            txtCategoriesDisplay.Font = new Font("Segoe UI", 10F);
            txtCategoriesDisplay.Location = new Point(920, 50);
            txtCategoriesDisplay.Name = "txtCategoriesDisplay";
            txtCategoriesDisplay.ReadOnly = true;
            txtCategoriesDisplay.Size = new Size(270, 25);
            txtCategoriesDisplay.TabIndex = 11;
            txtCategoriesDisplay.Click += txtCategoriesDisplay_Click;
            // 
            // btnCategoriesDropdown
            // 
            btnCategoriesDropdown.FlatStyle = FlatStyle.Flat;
            btnCategoriesDropdown.Font = new Font("Segoe UI", 9F);
            btnCategoriesDropdown.Location = new Point(1190, 50);
            btnCategoriesDropdown.Name = "btnCategoriesDropdown";
            btnCategoriesDropdown.Size = new Size(30, 25);
            btnCategoriesDropdown.TabIndex = 12;
            btnCategoriesDropdown.Text = "▼";
            btnCategoriesDropdown.UseVisualStyleBackColor = true;
            btnCategoriesDropdown.Click += btnCategoriesDropdown_Click;
            // 
            // lblFeaturedArtists
            // 
            lblFeaturedArtists.AutoSize = true;
            lblFeaturedArtists.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblFeaturedArtists.Location = new Point(15, 88);
            lblFeaturedArtists.Name = "lblFeaturedArtists";
            lblFeaturedArtists.Size = new Size(80, 15);
            lblFeaturedArtists.TabIndex = 15;
            lblFeaturedArtists.Text = "Artisti Feat.:";
            // 
            // txtFeaturedArtistsDisplay
            // 
            txtFeaturedArtistsDisplay.Cursor = Cursors.Hand;
            txtFeaturedArtistsDisplay.Font = new Font("Segoe UI", 10F);
            txtFeaturedArtistsDisplay.Location = new Point(100, 85);
            txtFeaturedArtistsDisplay.Name = "txtFeaturedArtistsDisplay";
            txtFeaturedArtistsDisplay.ReadOnly = true;
            txtFeaturedArtistsDisplay.Size = new Size(420, 25);
            txtFeaturedArtistsDisplay.TabIndex = 16;
            txtFeaturedArtistsDisplay.Click += txtFeaturedArtistsDisplay_Click;
            // 
            // btnFeaturedArtistsDropdown
            // 
            btnFeaturedArtistsDropdown.FlatStyle = FlatStyle.Flat;
            btnFeaturedArtistsDropdown.Font = new Font("Segoe UI", 9F);
            btnFeaturedArtistsDropdown.Location = new Point(520, 85);
            btnFeaturedArtistsDropdown.Name = "btnFeaturedArtistsDropdown";
            btnFeaturedArtistsDropdown.Size = new Size(30, 25);
            btnFeaturedArtistsDropdown.TabIndex = 17;
            btnFeaturedArtistsDropdown.Text = "▼";
            btnFeaturedArtistsDropdown.UseVisualStyleBackColor = true;
            btnFeaturedArtistsDropdown.Click += btnFeaturedArtistsDropdown_Click;
            // 
            // lblFilePath
            // 
            lblFilePath.AutoSize = true;
            lblFilePath.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblFilePath.Location = new Point(25, 296);
            lblFilePath.Name = "lblFilePath";
            lblFilePath.Size = new Size(64, 15);
            lblFilePath.TabIndex = 13;
            lblFilePath.Text = "File Audio:";
            // 
            // txtFilePath
            // 
            txtFilePath.Font = new Font("Consolas", 9F);
            txtFilePath.Location = new Point(110, 293);
            txtFilePath.Name = "txtFilePath";
            txtFilePath.ReadOnly = true;
            txtFilePath.Size = new Size(800, 22);
            txtFilePath.TabIndex = 14;
            // 
            // grpPeriod
            // 
            grpPeriod.Controls.Add(chkEnableValidFrom);
            grpPeriod.Controls.Add(dtpValidFrom);
            grpPeriod.Controls.Add(chkEnableValidTo);
            grpPeriod.Controls.Add(dtpValidTo);
            grpPeriod.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            grpPeriod.Location = new Point(15, 130);
            grpPeriod.Name = "grpPeriod";
            grpPeriod.Size = new Size(365, 74);
            grpPeriod.TabIndex = 14;
            grpPeriod.TabStop = false;
            grpPeriod.Text = "📅 Periodo Validità";
            // 
            // chkEnableValidFrom
            // 
            chkEnableValidFrom.AutoSize = true;
            chkEnableValidFrom.Font = new Font("Segoe UI", 8F);
            chkEnableValidFrom.Location = new Point(45, 20);
            chkEnableValidFrom.Name = "chkEnableValidFrom";
            chkEnableValidFrom.Size = new Size(40, 17);
            chkEnableValidFrom.TabIndex = 0;
            chkEnableValidFrom.Text = "Da";
            chkEnableValidFrom.UseVisualStyleBackColor = true;
            // 
            // dtpValidFrom
            // 
            dtpValidFrom.Enabled = false;
            dtpValidFrom.Font = new Font("Segoe UI", 8F);
            dtpValidFrom.Format = DateTimePickerFormat.Short;
            dtpValidFrom.Location = new Point(45, 43);
            dtpValidFrom.Name = "dtpValidFrom";
            dtpValidFrom.Size = new Size(120, 22);
            dtpValidFrom.TabIndex = 1;
            // 
            // chkEnableValidTo
            // 
            chkEnableValidTo.AutoSize = true;
            chkEnableValidTo.Font = new Font("Segoe UI", 8F);
            chkEnableValidTo.Location = new Point(217, 18);
            chkEnableValidTo.Name = "chkEnableValidTo";
            chkEnableValidTo.Size = new Size(33, 17);
            chkEnableValidTo.TabIndex = 2;
            chkEnableValidTo.Text = "A";
            chkEnableValidTo.UseVisualStyleBackColor = true;
            // 
            // dtpValidTo
            // 
            dtpValidTo.Enabled = false;
            dtpValidTo.Font = new Font("Segoe UI", 8F);
            dtpValidTo.Format = DateTimePickerFormat.Short;
            dtpValidTo.Location = new Point(217, 43);
            dtpValidTo.Name = "dtpValidTo";
            dtpValidTo.Size = new Size(120, 22);
            dtpValidTo.TabIndex = 3;
            // 
            // grpMonths
            // 
            grpMonths.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            grpMonths.Location = new Point(15, 210);
            grpMonths.Name = "grpMonths";
            grpMonths.Size = new Size(680, 55);
            grpMonths.TabIndex = 15;
            grpMonths.TabStop = false;
            grpMonths.Text = "📆 Mesi Consentiti";
            // 
            // grpDays
            // 
            grpDays.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            grpDays.Location = new Point(736, 210);
            grpDays.Name = "grpDays";
            grpDays.Size = new Size(500, 55);
            grpDays.TabIndex = 16;
            grpDays.TabStop = false;
            grpDays.Text = "📅 Giorni Consentiti";
            // 
            // grpHours
            // 
            grpHours.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            grpHours.Location = new Point(386, 143);
            grpHours.Name = "grpHours";
            grpHours.Size = new Size(850, 55);
            grpHours.TabIndex = 17;
            grpHours.TabStop = false;
            grpHours.Text = "🕐 Ore Consentite";
            // 
            // btnSave
            // 
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnSave.Location = new Point(1006, 280);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(110, 45);
            btnSave.TabIndex = 18;
            btnSave.Text = "💾 Salva";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // btnCancel
            // 
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnCancel.Location = new Point(1126, 280);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(110, 45);
            btnCancel.TabIndex = 19;
            btnCancel.Text = "✖ Annulla";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // MusicEditorForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(1263, 762);
            Controls.Add(picWaveform);
            Controls.Add(hScrollWaveform);
            Controls.Add(vuMeterPanel);
            Controls.Add(leftPanel);
            Controls.Add(bottomPanel);
            Controls.Add(toolbarPanel);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "MusicEditorForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "🎵 Music Editor Professional";
            toolbarPanel.ResumeLayout(false);
            zoomPanel.ResumeLayout(false);
            zoomPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)trkZoom).EndInit();
            grpVolume.ResumeLayout(false);
            grpVolume.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)trkVolume).EndInit();
            leftPanel.ResumeLayout(false);
            leftPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picWaveform).EndInit();
            bottomPanel.ResumeLayout(false);
            bottomPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numYear).EndInit();
            grpPeriod.ResumeLayout(false);
            grpPeriod.PerformLayout();
            ResumeLayout(false);
        }
    }
}
