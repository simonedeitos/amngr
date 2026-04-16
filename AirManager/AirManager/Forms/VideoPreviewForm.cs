using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using LibVLCSharp.Shared;
using AirManager.Services;

namespace AirManager.Forms
{
    public class VideoPreviewForm : Form
    {
        private LibVLC _vlcLib;
        private LibVLCSharp.Shared.MediaPlayer _vlcMediaPlayer;
        private LibVLCSharp.WinForms.VideoView _videoView;
        private string _videoPath;

        public VideoPreviewForm(string videoPath)
        {
            _videoPath = videoPath;
            this.Text = "📺 " + LanguageManager.GetString("VideoPreview.Title", "Video Preview");
            this.Size = new Size(640, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            this.BackColor = Color.Black;
            this.ShowInTaskbar = false;
            this.TopMost = true;
            InitializeVLC();
        }

        private void InitializeVLC()
        {
            try { LibVLCSharp.Shared.Core.Initialize(); } catch { }

            _vlcLib = new LibVLC("--no-audio", "--no-osd", "--no-stats", "--quiet", "--avcodec-fast", "--avcodec-threads=2");
            _vlcMediaPlayer = new LibVLCSharp.Shared.MediaPlayer(_vlcLib);
            _videoView = new LibVLCSharp.WinForms.VideoView
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Black,
                MediaPlayer = _vlcMediaPlayer
            };
            this.Controls.Add(_videoView);

            var media = new Media(_vlcLib, new Uri(Path.GetFullPath(_videoPath)));
            _vlcMediaPlayer.Media = media;
            _vlcMediaPlayer.Playing += (s, ev) => { _vlcMediaPlayer.Mute = true; };

            _vlcMediaPlayer.Play();
            Task.Delay(200).ContinueWith(_ =>
            {
                try { if (_vlcMediaPlayer != null && _vlcMediaPlayer.IsPlaying) _vlcMediaPlayer.SetPause(true); } catch { }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public void SyncPlay()
        {
            if (_vlcMediaPlayer == null) return;
            try { if (!_vlcMediaPlayer.IsPlaying) _vlcMediaPlayer.Play(); } catch { }
        }

        public void SyncPause()
        {
            if (_vlcMediaPlayer == null) return;
            try { if (_vlcMediaPlayer.IsPlaying) _vlcMediaPlayer.SetPause(true); } catch { }
        }

        public void SyncStop()
        {
            if (_vlcMediaPlayer == null) return;
            try
            {
                _vlcMediaPlayer.Stop();
                var media = new Media(_vlcLib, new Uri(Path.GetFullPath(_videoPath)));
                _vlcMediaPlayer.Media = media;
                _vlcMediaPlayer.Play();
                Task.Delay(200).ContinueWith(_ =>
                {
                    try { if (_vlcMediaPlayer != null && _vlcMediaPlayer.IsPlaying) _vlcMediaPlayer.SetPause(true); } catch { }
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
            catch { }
        }

        public void SyncSeek(int audioMs)
        {
            if (_vlcMediaPlayer == null) return;
            try
            {
                if (_vlcMediaPlayer.Length > 0)
                {
                    long seekMs = Math.Max(0L, Math.Min((long)audioMs, _vlcMediaPlayer.Length));
                    _vlcMediaPlayer.Time = seekMs;
                }
            }
            catch { }
        }

        private void InitializeComponent()
        {

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try { if (_vlcMediaPlayer != null) { if (_vlcMediaPlayer.IsPlaying) _vlcMediaPlayer.Stop(); _vlcMediaPlayer.Dispose(); _vlcMediaPlayer = null; } } catch { }
                try { _videoView?.Dispose(); _videoView = null; } catch { }
                try { _vlcLib?.Dispose(); _vlcLib = null; } catch { }
            }
            base.Dispose(disposing);
        }
    }
}
