using System;
using System.Drawing;
using System.Windows.Forms;
using AirManager.Controls;
using AirManager.Services;
using AirManager.Themes;

namespace AirManager.Forms
{
    public partial class StationManagementDialog : Form
    {
        private StationManagerControl _managerControl;

        public StationManagementDialog()
        {
            InitializeComponent();
            InitializeCustomUI();
        }

        private void InitializeCustomUI()
        {
            this.Text = "⚙️ " + LanguageManager.GetString("StationManager.HeaderTitle", "Station Management");
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = AppTheme.BgLight;

            _managerControl = new StationManagerControl();
            _managerControl.Dock = DockStyle.Fill;
            this.Controls.Add(_managerControl);

            // ✅ BOTTONE CHIUDI
            Button btnClose = new Button
            {
                Text = "✖ Chiudi",
                Size = new Size(120, 40),
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Location = new Point(1050, 710)
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) =>
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            };
            this.Controls.Add(btnClose);
            btnClose.BringToFront();
        }
    }
}