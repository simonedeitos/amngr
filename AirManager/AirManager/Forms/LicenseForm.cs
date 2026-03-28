using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;
using AirManager.Services.Licensing;
using AirManager.Themes;

namespace AirManager.Forms
{
    public partial class LicenseForm : Form
    {
        private readonly LicenseManager _licenseManager;
        public bool LicenseActivated { get; private set; } = false;

        // UI Controls
        private Panel mainPanel = null!;
        private Label titleLabel = null!;
        private Label subtitleLabel = null!;
        private Label serialLabel = null!;
        private TextBox serialTextBox = null!;
        private Button activateButton = null!;
        private Button exitButton = null!;
        private Label statusLabel = null!;
        private ProgressBar progressBar = null!;
        private Label hardwareIdLabel = null!;
        private Panel headerPanel = null!;

        public LicenseForm()
        {
            InitializeComponent();
            _licenseManager = LicenseManager.Instance;
            SetupUI();
        }

        private void SetupUI()
        {
            // Form settings
            this.Text = "AirManager - License Activation";
            this.Size = new Size(520, 420);
            this.MinimumSize = new Size(520, 420);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = AppTheme.BgLight;
            this.ForeColor = AppTheme.TextPrimary;

            // Header panel
            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = AppTheme.BgPanel,
                Padding = new Padding(20, 15, 20, 10)
            };
            headerPanel.Paint += (s, e) =>
            {
                using (var pen = new Pen(AppTheme.AccentSecondary, 2))
                {
                    e.Graphics.DrawLine(pen, 0, headerPanel.Height - 1, headerPanel.Width, headerPanel.Height - 1);
                }
            };

            // Title
            titleLabel = new Label
            {
                Text = "🔐 AirManager License",
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = AppTheme.TextPrimary,
                AutoSize = true,
                Location = new Point(20, 12)
            };
            headerPanel.Controls.Add(titleLabel);

            // Subtitle
            subtitleLabel = new Label
            {
                Text = "Enter your serial code to unlock AirManager",
                Font = new Font("Segoe UI", 9f),
                ForeColor = AppTheme.TextSecondary,
                AutoSize = true,
                Location = new Point(22, 48)
            };
            headerPanel.Controls.Add(subtitleLabel);

            this.Controls.Add(headerPanel);

            // Main panel
            mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(30, 20, 30, 20),
                BackColor = AppTheme.BgLight
            };
            this.Controls.Add(mainPanel);

            int yPos = 15;

            // Serial code label
            serialLabel = new Label
            {
                Text = "Serial Code:",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = AppTheme.TextPrimary,
                Location = new Point(5, yPos),
                AutoSize = true
            };
            mainPanel.Controls.Add(serialLabel);

            yPos += 28;

            // Serial code textbox
            serialTextBox = new TextBox
            {
                Font = new Font("Consolas", 13f),
                ForeColor = AppTheme.TextPrimary,
                BackColor = AppTheme.BgInput,
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(5, yPos),
                Size = new Size(440, 32),
                MaxLength = 19,
                CharacterCasing = CharacterCasing.Upper,
                PlaceholderText = "AMG-XXXX-XXXX-XXXX"
            };
            serialTextBox.TextChanged += SerialTextBox_TextChanged;
            serialTextBox.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    ActivateButton_Click(s!, e);
                }
            };
            mainPanel.Controls.Add(serialTextBox);

            yPos += 48;

            // Activate button
            activateButton = new Button
            {
                Text = "🔓  Activate License",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = AppTheme.TextPrimary,
                BackColor = AppTheme.AccentSecondary,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(440, 42),
                Location = new Point(5, yPos),
                Cursor = Cursors.Hand,
                Enabled = false
            };
            activateButton.FlatAppearance.BorderSize = 0;
            activateButton.Click += ActivateButton_Click;
            activateButton.MouseEnter += (s, e) =>
            {
                if (activateButton.Enabled)
                    activateButton.BackColor = AppTheme.Lighten(AppTheme.AccentSecondary, 15);
            };
            activateButton.MouseLeave += (s, e) =>
            {
                if (activateButton.Enabled)
                    activateButton.BackColor = AppTheme.AccentSecondary;
            };
            mainPanel.Controls.Add(activateButton);

            yPos += 55;

            // Progress bar
            progressBar = new ProgressBar
            {
                Style = ProgressBarStyle.Marquee,
                MarqueeAnimationSpeed = 30,
                Location = new Point(5, yPos),
                Size = new Size(440, 4),
                Visible = false
            };
            mainPanel.Controls.Add(progressBar);

            yPos += 15;

            // Status label
            statusLabel = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 9f),
                ForeColor = AppTheme.TextSecondary,
                Location = new Point(5, yPos),
                Size = new Size(440, 40),
                TextAlign = ContentAlignment.MiddleCenter
            };
            mainPanel.Controls.Add(statusLabel);

            yPos += 50;

            // Hardware ID label
            string hwId = HardwareIdentifier.GetHardwareId();
            hardwareIdLabel = new Label
            {
                Text = $"Hardware ID: {hwId}",
                Font = new Font("Consolas", 7.5f),
                ForeColor = AppTheme.TextSecondary,
                Location = new Point(5, yPos),
                Size = new Size(440, 18),
                TextAlign = ContentAlignment.MiddleCenter
            };
            mainPanel.Controls.Add(hardwareIdLabel);

            yPos += 30;

            // Exit button
            exitButton = new Button
            {
                Text = "Exit",
                Font = new Font("Segoe UI", 9f),
                ForeColor = AppTheme.TextSecondary,
                BackColor = AppTheme.BgPanel,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(100, 32),
                Location = new Point(175, yPos),
                Cursor = Cursors.Hand
            };
            exitButton.FlatAppearance.BorderColor = AppTheme.BorderLight;
            exitButton.FlatAppearance.BorderSize = 1;
            exitButton.Click += (s, e) =>
            {
                LicenseActivated = false;
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };
            mainPanel.Controls.Add(exitButton);

            // Bring main panel to front so it renders after header
            mainPanel.BringToFront();
        }

        private void SerialTextBox_TextChanged(object? sender, EventArgs e)
        {
            string text = serialTextBox.Text.Trim();
            bool validFormat = AirManager.Models.LicenseInfo.IsValidSerialFormat(text);

            activateButton.Enabled = validFormat;

            if (validFormat)
            {
                serialTextBox.ForeColor = AppTheme.Success;
                activateButton.BackColor = AppTheme.AccentSecondary;
            }
            else if (text.Length > 0)
            {
                serialTextBox.ForeColor = AppTheme.TextPrimary;
                activateButton.BackColor = AppTheme.ButtonDisabled;
            }
            else
            {
                serialTextBox.ForeColor = AppTheme.TextPrimary;
                activateButton.BackColor = AppTheme.ButtonDisabled;
            }
        }

        private async void ActivateButton_Click(object? sender, EventArgs e)
        {
            string serial = serialTextBox.Text.Trim().ToUpper();

            if (!AirManager.Models.LicenseInfo.IsValidSerialFormat(serial))
            {
                SetStatus("❌ Invalid serial code format", AppTheme.Danger);
                return;
            }

            // Disable UI during activation
            SetUIEnabled(false);
            progressBar.Visible = true;
            SetStatus("🔄 Activating license...", AppTheme.Info);

            try
            {
                var (success, message) = await _licenseManager.ActivateLicenseAsync(serial);

                if (success)
                {
                    SetStatus("✅ " + message, AppTheme.Success);
                    activateButton.Text = "✅  License Activated!";
                    activateButton.BackColor = AppTheme.Success;
                    LicenseActivated = true;

                    // Close after short delay
                    await Task.Delay(1500);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    SetStatus("❌ " + message, AppTheme.Danger);
                    SetUIEnabled(true);
                }
            }
            catch (Exception ex)
            {
                SetStatus($"❌ Error: {ex.Message}", AppTheme.Danger);
                SetUIEnabled(true);
            }
            finally
            {
                progressBar.Visible = false;
            }
        }

        private void SetStatus(string text, Color color)
        {
            if (statusLabel.InvokeRequired)
            {
                statusLabel.Invoke(new Action(() =>
                {
                    statusLabel.Text = text;
                    statusLabel.ForeColor = color;
                }));
            }
            else
            {
                statusLabel.Text = text;
                statusLabel.ForeColor = color;
            }
        }

        private void SetUIEnabled(bool enabled)
        {
            serialTextBox.Enabled = enabled;
            activateButton.Enabled = enabled;
        }

    
    }
}
