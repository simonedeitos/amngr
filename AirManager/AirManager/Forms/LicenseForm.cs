using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using AirManager.Services.Licensing;
using AirManager.Themes;

namespace AirManager.Forms
{
    public partial class LicenseForm : Form
    {
        public LicenseForm()
        {
            InitializeComponent();
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "License Activation";
            this.ClientSize = new Size(620, 480);
            this.MinimumSize = new Size(640, 520);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = AppTheme.LicBgLight;

            // ===== HEADER =====
            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 110,
                BackColor = AppTheme.Primary
            };
            this.Controls.Add(headerPanel);

            Label lblIcon = new Label
            {
                Text = "🔐",
                Font = new Font("Segoe UI", 30),
                ForeColor = AppTheme.TextInverse,
                Location = new Point(20, 20),
                AutoSize = true
            };
            headerPanel.Controls.Add(lblIcon);

            Label lblTitle = new Label
            {
                Text = "License Activation",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = AppTheme.TextInverse,
                AutoSize = false,
                Size = new Size(460, 42),
                Location = new Point(82, 18),
                TextAlign = ContentAlignment.MiddleLeft
            };
            headerPanel.Controls.Add(lblTitle);

            Label lblSubtitle = new Label
            {
                Text = "Enter your serial code to unlock AirManager",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(200, 230, 255),
                AutoSize = false,
                Size = new Size(520, 22),
                Location = new Point(82, 66),
                TextAlign = ContentAlignment.MiddleLeft
            };
            headerPanel.Controls.Add(lblSubtitle);

            // ===== MAIN CARD =====
            int margin = 20;
            int cardTop = 110 + margin;

            Panel cardPanel = new Panel
            {
                Location = new Point(margin, cardTop),
                Size = new Size(580, 220),
                BackColor = AppTheme.LicSurface,
                Padding = new Padding(24)
            };
            cardPanel.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var pen = new Pen(AppTheme.LicBorderLight, 1))
                    e.Graphics.DrawRectangle(pen, 0, 0, cardPanel.Width - 1, cardPanel.Height - 1);
            };
            this.Controls.Add(cardPanel);

            // Owner Name Label
            Label lblOwner = new Label
            {
                Text = "Name / Company",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = AppTheme.LicTextSecondary,
                Location = new Point(24, 18),
                AutoSize = true
            };
            cardPanel.Controls.Add(lblOwner);

            TextBox txtOwner = new TextBox
            {
                Name = "txtOwner",
                Font = new Font("Segoe UI", 11),
                Location = new Point(24, 40),
                Size = new Size(532, 34),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = AppTheme.LicBgLight
            };
            cardPanel.Controls.Add(txtOwner);

            // Serial Label
            Label lblSerial = new Label
            {
                Text = "Serial Code",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = AppTheme.LicTextSecondary,
                Location = new Point(24, 90),
                AutoSize = true
            };
            cardPanel.Controls.Add(lblSerial);

            TextBox txtSerial = new TextBox
            {
                Name = "txtSerial",
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                Location = new Point(24, 112),
                Size = new Size(532, 36),
                MaxLength = 18,
                CharacterCasing = CharacterCasing.Upper,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = AppTheme.LicBgLight
            };
            cardPanel.Controls.Add(txtSerial);

            Label lblFormat = new Label
            {
                Text = $"Format: {Models.LicenseInfo.SERIAL_PREFIX}XXXX-XXXX-XXXX",
                Font = new Font("Segoe UI", 8, FontStyle.Italic),
                ForeColor = AppTheme.LicTextSecondary,
                Location = new Point(24, 160),
                AutoSize = true
            };
            cardPanel.Controls.Add(lblFormat);

            // ===== ACTIVATE BUTTON =====
            int btnActivateTop = cardTop + cardPanel.Height + margin;
            Button btnActivate = new Button
            {
                Name = "btnActivate",
                Text = "🔓  Activate License",
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                Location = new Point(margin, btnActivateTop),
                Size = new Size(580, 52),
                BackColor = AppTheme.Success,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnActivate.FlatAppearance.BorderSize = 0;
            btnActivate.Click += (s, e) => BtnActivate_Click(txtOwner.Text, txtSerial.Text);
            this.Controls.Add(btnActivate);

            // ===== INFO LABEL =====
            int infoTop = btnActivateTop + btnActivate.Height + 14;
            Label lblInfo = new Label
            {
                Text = "AirManager requires a valid license to work.\nPurchase your license at store.airdirector.app",
                Font = new Font("Segoe UI", 8, FontStyle.Italic),
                ForeColor = AppTheme.LicTextSecondary,
                Location = new Point(margin, infoTop),
                Size = new Size(580, 36),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblInfo);

            // Adjust client height based on content
            this.ClientSize = new Size(620, infoTop + lblInfo.Height + margin);
        }

        private void BtnActivate_Click(string ownerName, string serial)
        {
            if (string.IsNullOrWhiteSpace(serial))
            {
                MessageBox.Show("Please enter the serial code", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool success = LicenseManager.ActivateLicense(serial, ownerName, out string errorMessage);

            if (success)
            {
                MessageBox.Show(
                    "License activated successfully!",
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show(
                    "Error during activation:\n\n" + errorMessage,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

    }
}
