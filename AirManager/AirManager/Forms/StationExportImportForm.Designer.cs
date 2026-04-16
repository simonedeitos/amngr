namespace AirManager.Forms
{
    partial class StationExportImportForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StationExportImportForm));
            SuspendLayout();
            // 
            // StationExportImportForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(700, 650);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "StationExportImportForm";
            Text = "Export/Import Emittenti";
            ResumeLayout(false);
        }
    }
}