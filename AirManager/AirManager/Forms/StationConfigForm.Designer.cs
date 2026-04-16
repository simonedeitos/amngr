namespace AirManager.Forms
{
    partial class StationConfigForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StationConfigForm));
            SuspendLayout();
            // 
            // StationConfigForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(700, 600);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "StationConfigForm";
            Text = "Configurazione Emittente";
            ResumeLayout(false);
        }
    }
}