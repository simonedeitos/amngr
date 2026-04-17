using System;
using System.Windows.Forms;
using AirManager.Forms;

namespace AirManager.Controls
{
    public class PlaylistEditorControl : UserControl
    {
        private PlaylistEditorForm _hostedEditor;

        public PlaylistEditorControl()
        {
            Dock = DockStyle.Fill;
            Load += PlaylistEditorControl_Load;
        }

        private void PlaylistEditorControl_Load(object sender, EventArgs e)
        {
            if (_hostedEditor != null)
                return;

            _hostedEditor = new PlaylistEditorForm
            {
                TopLevel = false,
                FormBorderStyle = FormBorderStyle.None,
                Dock = DockStyle.Fill
            };

            Controls.Clear();
            Controls.Add(_hostedEditor);
            _hostedEditor.Show();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_hostedEditor != null)
                {
                    _hostedEditor.Dispose();
                    _hostedEditor = null;
                }
            }
            base.Dispose(disposing);
        }
    }
}
