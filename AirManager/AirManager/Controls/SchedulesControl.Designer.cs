using System;
using System.Drawing;
using System.Windows.Forms;
using AirManager.Themes;

namespace AirManager.Controls
{
    partial class SchedulesControl
    {
        private System.ComponentModel.IContainer components = null;

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

            this.Name = "SchedulesControl";
            this.Size = new Size(600, 800);
            this.BackColor = AppTheme.BgLight;

            this.ResumeLayout(false);
        }
    }
}
