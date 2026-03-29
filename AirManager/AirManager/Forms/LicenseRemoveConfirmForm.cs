using System;
using System.Drawing;
using System.Windows.Forms;
using AirManager.Services;
using AirManager.Services.Licensing;

namespace AirManager.Forms
{
    public partial class LicenseRemoveConfirmForm : Form
    {
        private string _licenseDetail;

        public LicenseRemoveConfirmForm(string licenseDetail)
        {
            InitializeComponent();
            _licenseDetail = licenseDetail;
            BuildUI();
        }

        private void BuildUI()
        {
            this.Text = LanguageManager.GetString("LicenseRemoveConfirm.Title", "Rimuovi Licenza");
            this.Size = new Size(520, 260);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(30, 30, 30);

            var lblIcon = new Label
            {
                Text = "⚠️",
                Font = new Font("Segoe UI Emoji", 28F),
                ForeColor = Color.Orange,
                Location = new Point(20, 20),
                Size = new Size(60, 50),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblIcon);

            var lblTitle = new Label
            {
                Text = LanguageManager.GetString("LicenseRemoveConfirm.Question", "Sei sicuro di voler rimuovere la licenza?"),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(90, 28),
                Size = new Size(400, 30),
                TextAlign = ContentAlignment.MiddleLeft
            };
            this.Controls.Add(lblTitle);

            var pnlInfo = new Panel
            {
                Location = new Point(20, 80),
                Size = new Size(460, 60),
                BackColor = Color.FromArgb(80, 70, 20),
                BorderStyle = BorderStyle.FixedSingle
            };
            var lblDetail = new Label
            {
                Text = _licenseDetail,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(255, 230, 100),
                Dock = DockStyle.Fill,
                Padding = new Padding(8, 8, 8, 8),
                TextAlign = ContentAlignment.MiddleLeft
            };
            pnlInfo.Controls.Add(lblDetail);
            this.Controls.Add(pnlInfo);

            var btnRemove = new Button
            {
                Text = LanguageManager.GetString("LicenseRemoveConfirm.BtnRemove", "Sì, rimuovi la licenza"),
                Location = new Point(20, 160),
                Size = new Size(210, 40),
                BackColor = Color.FromArgb(180, 40, 40),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                DialogResult = DialogResult.OK,
                Cursor = Cursors.Hand
            };
            btnRemove.FlatAppearance.BorderSize = 0;
            this.Controls.Add(btnRemove);

            var btnCancel = new Button
            {
                Text = LanguageManager.GetString("Common.Cancel", "Annulla"),
                Location = new Point(250, 160),
                Size = new Size(120, 40),
                BackColor = Color.FromArgb(70, 70, 70),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F),
                DialogResult = DialogResult.Cancel,
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            this.Controls.Add(btnCancel);

            this.AcceptButton = btnRemove;
            this.CancelButton = btnCancel;
        }
    }
}
