namespace AirManager.Forms
{
    partial class MusicStatisticsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MusicStatisticsForm));
            SuspendLayout();
            // 
            // MusicStatisticsForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(284, 261);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "MusicStatisticsForm";
            ResumeLayout(false);
        }
    }
}
