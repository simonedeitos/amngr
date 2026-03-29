using System;
using System.Drawing;
using System.Windows.Forms;
using AirManager.Services;

namespace AirManager.Controls
{
    public partial class ProgrammingControl : UserControl
    {
        public event EventHandler<string> StatusChanged;

        public ProgrammingControl()
        {
            InitializeComponent();

            clocksControl.StatusChanged += (s, msg) => StatusChanged?.Invoke(this, msg);
            schedulesControl.StatusChanged += (s, msg) => StatusChanged?.Invoke(this, msg);

            this.Load += ProgrammingControl_Load;
        }

        private void ProgrammingControl_Load(object sender, EventArgs e)
        {
            // Set the splitter position to 50% after the control is loaded and sized
            if (splitContainer.Width > 0)
                splitContainer.SplitterDistance = splitContainer.Width / 2;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            if (splitContainer != null && splitContainer.Width > 0)
                splitContainer.SplitterDistance = splitContainer.Width / 2;
        }

        public void RefreshAll()
        {
            clocksControl?.RefreshClocks();
            schedulesControl?.RefreshSchedules();
        }
    }
}
