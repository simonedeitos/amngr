using System;
using System.Drawing;
using System.Windows.Forms;
using AirManager.Themes;

namespace AirManager.Controls
{
    partial class ClocksControl
    {
        private System.ComponentModel.IContainer components = null;
        private FlowLayoutPanel flowClocks;
        private Panel headerPanel;
        private Label lblTitle;
        private Label lblStatus;
        private Label lblDefault;
        private Button btnNew;
        private Button btnRefresh;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
                AirManager.Services.LanguageManager.LanguageChanged -= OnLanguageChanged;
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.SuspendLayout();

            this.Name = "ClocksControl";
            this.Size = new Size(600, 800);
            this.BackColor = AppTheme.BgLight;

            this.flowClocks = new FlowLayoutPanel();
            this.flowClocks.Name = "flowClocks";
            this.flowClocks.Dock = DockStyle.Fill;
            this.flowClocks.AutoScroll = true;
            this.flowClocks.Padding = new Padding(15);
            this.flowClocks.BackColor = AppTheme.BgLight;
            this.flowClocks.FlowDirection = FlowDirection.TopDown;
            this.flowClocks.WrapContents = false;

            this.headerPanel = new Panel();
            this.headerPanel.Name = "headerPanel";
            this.headerPanel.Dock = DockStyle.Top;
            this.headerPanel.Height = 70;
            this.headerPanel.BackColor = AppTheme.Surface;
            this.headerPanel.Padding = new Padding(12, 8, 12, 8);

            this.lblTitle = new Label();
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Text = "🕐 GESTIONE CLOCK";
            this.lblTitle.Location = new Point(12, 8);
            this.lblTitle.Size = new Size(280, 26);
            this.lblTitle.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            this.lblTitle.ForeColor = AppTheme.Primary;
            this.lblTitle.BackColor = Color.Transparent;

            this.btnNew = new Button();
            this.btnNew.Name = "btnNew";
            this.btnNew.Text = "➕ Nuovo Clock";
            this.btnNew.Location = new Point(12, 38);
            this.btnNew.Size = new Size(130, 28);
            this.btnNew.BackColor = AppTheme.Success;
            this.btnNew.ForeColor = Color.White;
            this.btnNew.FlatStyle = FlatStyle.Flat;
            this.btnNew.FlatAppearance.BorderSize = 0;
            this.btnNew.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this.btnNew.Cursor = Cursors.Hand;
            this.btnNew.Click += new EventHandler(this.BtnNew_Click);

            this.btnRefresh = new Button();
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Text = "🔄 Aggiorna";
            this.btnRefresh.Location = new Point(150, 38);
            this.btnRefresh.Size = new Size(95, 28);
            this.btnRefresh.BackColor = AppTheme.Info;
            this.btnRefresh.ForeColor = Color.White;
            this.btnRefresh.FlatStyle = FlatStyle.Flat;
            this.btnRefresh.FlatAppearance.BorderSize = 0;
            this.btnRefresh.Font = new Font("Segoe UI", 9F);
            this.btnRefresh.Cursor = Cursors.Hand;
            this.btnRefresh.Click += new EventHandler(this.BtnRefresh_Click);

            this.lblDefault = new Label();
            this.lblDefault.Name = "lblDefault";
            this.lblDefault.Text = "⭐ Predefinito: Nessuno";
            this.lblDefault.Location = new Point(260, 42);
            this.lblDefault.AutoSize = true;
            this.lblDefault.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this.lblDefault.ForeColor = Color.FromArgb(255, 152, 0);
            this.lblDefault.BackColor = Color.Transparent;

            this.lblStatus = new Label();
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Text = "📊 Caricamento...";
            this.lblStatus.Location = new Point(550, 42);
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new Font("Segoe UI", 8F, FontStyle.Italic);
            this.lblStatus.ForeColor = AppTheme.TextSecondary;
            this.lblStatus.BackColor = Color.Transparent;

            this.headerPanel.Controls.Add(this.lblTitle);
            this.headerPanel.Controls.Add(this.lblDefault);
            this.headerPanel.Controls.Add(this.lblStatus);
            this.headerPanel.Controls.Add(this.btnNew);
            this.headerPanel.Controls.Add(this.btnRefresh);

            this.Controls.Add(this.flowClocks);
            this.Controls.Add(this.headerPanel);

            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
