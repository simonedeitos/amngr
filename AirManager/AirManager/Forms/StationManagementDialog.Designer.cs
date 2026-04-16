namespace AirManager.Forms
{
    partial class StationManagementDialog
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StationManagementDialog));
            SuspendLayout();
            // 
            // StationManagementDialog
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1200, 800);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "StationManagementDialog";
            Text = "Gestione Emittenti";
            ResumeLayout(false);
        }
    }
}