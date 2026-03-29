using System;
using System.Drawing;
using System.Windows.Forms;
using AirManager.Themes;

namespace AirManager.Controls
{
    partial class ProgrammingControl
    {
        private System.ComponentModel.IContainer components = null;
        private SplitContainer splitContainer;
        private ClocksControl clocksControl;
        private SchedulesControl schedulesControl;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
                clocksControl?.Dispose();
                schedulesControl?.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.SuspendLayout();

            this.Name = "ProgrammingControl";
            this.Size = new Size(1200, 800);
            this.BackColor = AppTheme.BgLight;

            this.splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                BackColor = AppTheme.BgDark,
                BorderStyle = BorderStyle.None
            };

            this.clocksControl = new ClocksControl
            {
                Dock = DockStyle.Fill
            };

            this.schedulesControl = new SchedulesControl
            {
                Dock = DockStyle.Fill
            };

            this.splitContainer.Panel1.Controls.Add(this.clocksControl);
            this.splitContainer.Panel2.Controls.Add(this.schedulesControl);

            this.Controls.Add(this.splitContainer);

            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
