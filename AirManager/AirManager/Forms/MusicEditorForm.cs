using AirManager.Services;
using AirManager.Services.Database;
using AirManager.Themes;
using NAudio.Wave;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AirManager.Forms
{
    public partial class MusicEditorForm : Form
    {
        private MusicEntry _musicEntry;
        private AudioFileReader _audioReader;
        private WaveOutEvent _waveOut;
        private System.Windows.Forms.Timer _positionTimer;

        private float[] _waveformData;
        private Bitmap _waveformBitmap;
        private bool _isLoadingWaveform = false;
        private string _lastLoadedFile = "";
        private int _waveformSamples = 6000;

        private bool _isPlaying = false;
        private bool _isDraggingMarker = false;
        private string _draggingMarkerType = "";

        private CheckBox[] _chkMonths;
        private CheckBox[] _chkDays;
        private CheckBox[] _chkHours;
        private DateTimePicker _dtpValidFrom;
        private DateTimePicker _dtpValidTo;
        private CheckBox _chkEnableValidFrom;
        private CheckBox _chkEnableValidTo;

        private bool _isClip = false;
        private int _originalClipId = 0;

        private Button btnResetIn;
        private Button btnResetIntro;
        private Button btnResetMix;
        private Button btnResetOut;

        public MusicEditorForm(MusicEntry musicEntry, bool isClip = false)
        {
            InitializeComponent();

            _musicEntry = musicEntry;
            _isClip = isClip;

            if (_isClip)
            {
                _originalClipId = musicEntry.ID;
            }

            this.BackColor = AppTheme.BgDark;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            _dtpValidFrom = dtpValidFrom;
            _dtpValidTo = dtpValidTo;
            _chkEnableValidFrom = chkEnableValidFrom;
            _chkEnableValidTo = chkEnableValidTo;

            _chkEnableValidFrom.CheckedChanged += (s, e) =>
            {
                _dtpValidFrom.Enabled = _chkEnableValidFrom.Checked;
            };

            _chkEnableValidTo.CheckedChanged += (s, e) =>
            {
                _dtpValidTo.Enabled = _chkEnableValidTo.Checked;
            };

            ApplyTheme();
            CreateResetButtons();
            CreateValidityControls();
            LoadMetadata();
            ApplyLanguage();
            LoadAudioFile();

            _positionTimer = new System.Windows.Forms.Timer { Interval = 50 };
            _positionTimer.Tick += PositionTimer_Tick;

            LanguageManager.LanguageChanged += (s, e) => ApplyLanguage();
        }

        private void ApplyLanguage()
        {
            if (_isClip)
            {
                this.Text = string.Format(
                    LanguageManager.GetString("MusicEditor.Title.Jingle"),
                    _musicEntry.Title);
            }
            else
            {
                this.Text = string.Format(
                    LanguageManager.GetString("MusicEditor.Title.Music"),
                    _musicEntry.Artist,
                    _musicEntry.Title);
            }

            if (!_isPlaying)
            {
                btnPlay.Text = "▶ " + LanguageManager.GetString("MusicEditor.Play");
            }
            else
            {
                btnPlay.Text = "⏸ " + LanguageManager.GetString("MusicEditor.Pause");
            }

            btnStop.Text = "⏹ " + LanguageManager.GetString("MusicEditor.Stop");
            btnLoop.Text = "🔁 " + LanguageManager.GetString("MusicEditor.Loop");

            lblMarkerInLabel.Text = LanguageManager.GetString("MusicEditor.Marker.In");
            lblMarkerIntroLabel.Text = LanguageManager.GetString("MusicEditor.Marker.Intro");
            lblMarkerMixLabel.Text = LanguageManager.GetString("MusicEditor.Marker.Mix");
            lblMarkerOutLabel.Text = LanguageManager.GetString("MusicEditor.Marker.Out");

            lblTitle.Text = LanguageManager.GetString("MusicEditor.Label.Title");
            lblArtist.Text = LanguageManager.GetString("MusicEditor.Label.Artist");
            lblAlbum.Text = LanguageManager.GetString("MusicEditor.Label.Album");
            lblYear.Text = LanguageManager.GetString("MusicEditor.Label.Year");
            lblGenre.Text = LanguageManager.GetString("MusicEditor.Label.Genre");
            lblCategories.Text = LanguageManager.GetString("MusicEditor.Label.Categories");
            lblFilePath.Text = LanguageManager.GetString("MusicEditor.Label.FilePath");

            grpPeriod.Text = "📅 " + LanguageManager.GetString("MusicEditor.Group.ValidityPeriod");
            grpMonths.Text = "📆 " + LanguageManager.GetString("MusicEditor.Group.AllowedMonths");
            grpDays.Text = "📅 " + LanguageManager.GetString("MusicEditor.Group.AllowedDays");
            grpHours.Text = "🕐 " + LanguageManager.GetString("MusicEditor.Group.AllowedHours");

            chkEnableValidFrom.Text = LanguageManager.GetString("MusicEditor.ValidFrom");
            chkEnableValidTo.Text = LanguageManager.GetString("MusicEditor.ValidTo");

            btnSave.Text = "💾 " + LanguageManager.GetString("Common.Save");
            btnCancel.Text = "✖ " + LanguageManager.GetString("Common.Cancel");
        }

        private void ApplyTheme()
        {
            toolbarPanel.BackColor = Color.FromArgb(45, 45, 48);
            btnPlay.BackColor = Color.FromArgb(40, 160, 40);
            btnPlay.ForeColor = Color.White;
            btnStop.BackColor = Color.FromArgb(200, 50, 50);
            btnStop.ForeColor = Color.White;
            btnLoop.BackColor = Color.FromArgb(60, 60, 60);
            btnLoop.ForeColor = Color.White;

            leftPanel.BackColor = Color.FromArgb(30, 30, 30);
            lblCurrentPosition.BackColor = Color.Black;
            lblCurrentPosition.ForeColor = Color.Lime;
            lblCurrentPositionMs.BackColor = Color.Black;
            lblCurrentPositionMs.ForeColor = Color.Cyan;
            lblTotalDuration.BackColor = Color.Black;
            lblTotalDuration.ForeColor = Color.Orange;

            txtMarkerIn.BackColor = Color.Black;
            txtMarkerIn.ForeColor = Color.Red;
            txtMarkerIntro.BackColor = Color.Black;
            txtMarkerIntro.ForeColor = Color.Magenta;
            txtMarkerMix.BackColor = Color.Black;
            txtMarkerMix.ForeColor = Color.Yellow;
            txtMarkerOut.BackColor = Color.Black;
            txtMarkerOut.ForeColor = Color.FromArgb(255, 140, 0);

            picWaveform.BackColor = Color.Black;
            bottomPanel.BackColor = Color.FromArgb(240, 240, 240);

            btnSave.BackColor = AppTheme.Success;
            btnSave.ForeColor = Color.White;
            btnSave.FlatAppearance.BorderSize = 0;
            btnCancel.BackColor = AppTheme.Danger;
            btnCancel.ForeColor = Color.White;
            btnCancel.FlatAppearance.BorderSize = 0;
        }

        private void CreateResetButtons()
        {
            btnResetIn = new Button
            {
                Text = "✕",
                Location = new Point(345, 20),
                Size = new Size(25, 28),
                BackColor = Color.FromArgb(200, 50, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Arial", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnResetIn.FlatAppearance.BorderSize = 0;
            btnResetIn.Click += (s, e) => ResetMarker("In");
            leftPanel.Controls.Add(btnResetIn);

            btnResetIntro = new Button
            {
                Text = "✕",
                Location = new Point(345, 60),
                Size = new Size(25, 28),
                BackColor = Color.FromArgb(200, 50, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Arial", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnResetIntro.FlatAppearance.BorderSize = 0;
            btnResetIntro.Click += (s, e) => ResetMarker("Intro");
            leftPanel.Controls.Add(btnResetIntro);

            btnResetMix = new Button
            {
                Text = "✕",
                Location = new Point(345, 100),
                Size = new Size(25, 28),
                BackColor = Color.FromArgb(200, 50, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Arial", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnResetMix.FlatAppearance.BorderSize = 0;
            btnResetMix.Click += (s, e) => ResetMarker("Mix");
            leftPanel.Controls.Add(btnResetMix);

            btnResetOut = new Button
            {
                Text = "✕",
                Location = new Point(345, 140),
                Size = new Size(25, 28),
                BackColor = Color.FromArgb(200, 50, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Arial", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnResetOut.FlatAppearance.BorderSize = 0;
            btnResetOut.Click += (s, e) => ResetMarker("Out");
            leftPanel.Controls.Add(btnResetOut);
        }

        private void ResetMarker(string markerType)
        {
            int totalMs = _audioReader != null ? (int)_audioReader.TotalTime.TotalMilliseconds : 0;

            switch (markerType)
            {
                case "In":
                    _musicEntry.MarkerIN = 0;
                    txtMarkerIn.Text = FormatTime(0);
                    PlayFromMarker("In");
                    break;

                case "Intro":
                    _musicEntry.MarkerINTRO = 0;
                    txtMarkerIntro.Text = FormatTime(0);
                    PlayFromMarker("Intro");
                    break;

                case "Mix":
                    _musicEntry.MarkerMIX = totalMs;
                    txtMarkerMix.Text = FormatTime(totalMs);
                    _musicEntry.MarkerOUT = totalMs;
                    txtMarkerOut.Text = FormatTime(totalMs);
                    PlayFromMarker("Mix");
                    break;

                case "Out":
                    _musicEntry.MarkerOUT = totalMs;
                    txtMarkerOut.Text = FormatTime(totalMs);
                    PlayFromMarker("Out");
                    break;
            }

            picWaveform.Invalidate();
        }

        private void CreateValidityControls()
        {
            _chkMonths = new CheckBox[12];

            for (int i = 0; i < 12; i++)
            {
                string monthName = LanguageManager.GetString("MusicEditor.Month." + (i + 1).ToString());

                _chkMonths[i] = new CheckBox
                {
                    Text = monthName,
                    Location = new Point(8 + (i * 55), 20),
                    Size = new Size(48, 30),
                    Checked = true,
                    Appearance = Appearance.Button,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 8, FontStyle.Bold),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.DodgerBlue,
                    ForeColor = Color.Black
                };

                _chkMonths[i].CheckedChanged += (s, e) =>
                {
                    var chk = s as CheckBox;
                    chk.BackColor = chk.Checked ? Color.DodgerBlue : Color.LightGray;
                    chk.ForeColor = chk.Checked ? Color.Black : Color.DarkGray;
                };

                grpMonths.Controls.Add(_chkMonths[i]);
            }

            _chkDays = new CheckBox[7];

            for (int i = 0; i < 7; i++)
            {
                string dayName = LanguageManager.GetString("MusicEditor.Day." + i.ToString());

                _chkDays[i] = new CheckBox
                {
                    Text = dayName,
                    Location = new Point(8 + (i * 68), 20),
                    Size = new Size(60, 30),
                    Checked = true,
                    Appearance = Appearance.Button,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 8, FontStyle.Bold),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.Green,
                    ForeColor = Color.Black
                };

                _chkDays[i].CheckedChanged += (s, e) =>
                {
                    var chk = s as CheckBox;
                    chk.BackColor = chk.Checked ? Color.Green : Color.LightGray;
                    chk.ForeColor = chk.Checked ? Color.Black : Color.DarkGray;
                };

                grpDays.Controls.Add(_chkDays[i]);
            }

            _chkHours = new CheckBox[24];

            for (int i = 0; i < 24; i++)
            {
                _chkHours[i] = new CheckBox
                {
                    Text = i.ToString("D2"),  // ✅ CORRETTO: 00, 01, 02... 23
                    Location = new Point(8 + (i * 34), 18),
                    Size = new Size(30, 30),
                    Checked = true,
                    Appearance = Appearance.Button,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 8, FontStyle.Bold),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.Purple,
                    ForeColor = Color.Black
                };

                _chkHours[i].CheckedChanged += (s, e) =>
                {
                    var chk = s as CheckBox;
                    chk.BackColor = chk.Checked ? Color.Purple : Color.LightGray;
                    chk.ForeColor = chk.Checked ? Color.Black : Color.DarkGray;
                };

                grpHours.Controls.Add(_chkHours[i]);
            }
        }

        private void LoadMetadata()
        {
            txtMarkerIn.Text = FormatTime(_musicEntry.MarkerIN);
            txtMarkerIntro.Text = FormatTime(_musicEntry.MarkerINTRO);
            txtMarkerMix.Text = FormatTime(_musicEntry.MarkerMIX);
            txtMarkerOut.Text = FormatTime(_musicEntry.MarkerOUT);

            txtTitle.Text = _musicEntry.Title ?? "";
            txtArtist.Text = _musicEntry.Artist ?? "";
            txtFilePath.Text = _musicEntry.FilePath ?? "";
            txtAlbum.Text = _musicEntry.Album ?? "";
            numYear.Value = _musicEntry.Year > 0 ? _musicEntry.Year : DateTime.Now.Year;
            cmbGenre.Text = _musicEntry.Genre ?? "";
            txtCategories.Text = _musicEntry.Categories ?? "";

            cmbGenre.Items.Clear();
            cmbGenre.Items.AddRange(new string[] {
                "Pop", "Rock", "Dance", "Hip Hop", "R&B", "Indie", "Electronic",
                "Jazz", "Blues", "Country", "Reggae", "Latin", "Classical",
                "Metal", "Punk", "Folk", "Disco", "House", "Techno", "Trance", "Unknown"
            });

            LoadValidityData();
        }

        private void LoadValidityData()
        {
            if (!string.IsNullOrEmpty(_musicEntry.ValidFrom) &&
                DateTime.TryParse(_musicEntry.ValidFrom, out DateTime validFrom) &&
                validFrom.Year > 1900)
            {
                _chkEnableValidFrom.Checked = true;
                _dtpValidFrom.Value = validFrom;
            }

            if (!string.IsNullOrEmpty(_musicEntry.ValidTo) &&
                DateTime.TryParse(_musicEntry.ValidTo, out DateTime validTo) &&
                validTo.Year > 1900)
            {
                _chkEnableValidTo.Checked = true;
                _dtpValidTo.Value = validTo;
            }

            if (!string.IsNullOrEmpty(_musicEntry.ValidMonths))
            {
                var months = _musicEntry.ValidMonths.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                                         .Select(m => int.TryParse(m.Trim(), out int month) ? month : 0)
                                         .Where(m => m > 0 && m <= 12)
                                         .ToList();

                for (int i = 0; i < 12; i++)
                {
                    _chkMonths[i].Checked = months.Contains(i + 1);
                }
            }

            if (!string.IsNullOrEmpty(_musicEntry.ValidDays))
            {
                var days = _musicEntry.ValidDays.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(d => d.Trim())
                                      .ToList();
                string[] dayMap = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };

                for (int i = 0; i < 7; i++)
                {
                    _chkDays[i].Checked = days.Contains(dayMap[i]);
                }
            }

            if (!string.IsNullOrEmpty(_musicEntry.ValidHours))
            {
                var hours = _musicEntry.ValidHours.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                                        .Select(h => int.TryParse(h.Trim(), out int hour) ? hour : -1)
                                        .Where(h => h >= 0 && h < 24)
                                        .ToList();

                for (int i = 0; i < 24; i++)
                {
                    _chkHours[i].Checked = hours.Contains(i);
                }
            }
        }

        private async void LoadAudioFile()
        {
            try
            {
                if (!File.Exists(_musicEntry.FilePath))
                {
                    MessageBox.Show(
                        string.Format(LanguageManager.GetString("MusicEditor.Error.FileNotFound"), _musicEntry.FilePath),
                        LanguageManager.GetString("Common.Error"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                _audioReader = new AudioFileReader(_musicEntry.FilePath);
                _waveOut = new WaveOutEvent();

                _waveOut.PlaybackStopped += (s, e) =>
                {
                    if (this.IsHandleCreated && !this.IsDisposed)
                    {
                        this.Invoke(new Action(() =>
                        {
                            _isPlaying = false;
                            _positionTimer.Stop();
                            btnPlay.Text = "▶ " + LanguageManager.GetString("MusicEditor.Play");
                            btnPlay.BackColor = Color.FromArgb(40, 160, 40);
                            _audioReader.Position = 0;
                            lblCurrentPosition.Text = "00:00:00.000";
                            lblCurrentPositionMs.Text = "0 ms";
                            picWaveform.Invalidate();
                        }));
                    }
                };

                int deviceNumber = SettingsForm.GetAudioDeviceNumber();

                if (deviceNumber >= 0)
                {
                    _waveOut.DeviceNumber = deviceNumber;
                }

                _waveOut.Init(_audioReader);

                lblTotalDuration.Text = FormatTime((int)_audioReader.TotalTime.TotalMilliseconds);

                if (_musicEntry.MarkerOUT == 0)
                {
                    _musicEntry.MarkerOUT = (int)_audioReader.TotalTime.TotalMilliseconds;
                }

                txtMarkerOut.Text = FormatTime(_musicEntry.MarkerOUT);

                await GenerateWaveformAsync(_musicEntry.FilePath);
            }
            catch (Exception ex)
            {
                if (this.IsHandleCreated && !this.IsDisposed)
                {
                    MessageBox.Show(
                        string.Format(LanguageManager.GetString("MusicEditor.Error.LoadAudio"), ex.Message),
                        LanguageManager.GetString("Common.Error"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        private async Task GenerateWaveformAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || _isLoadingWaveform)
                return;

            if (_lastLoadedFile == filePath && _waveformBitmap != null)
                return;

            _isLoadingWaveform = true;
            _lastLoadedFile = filePath;

            _waveformBitmap?.Dispose();
            _waveformBitmap = null;

            await Task.Run(() =>
            {
                try
                {
                    using (var reader = new AudioFileReader(filePath))
                    {
                        var format = reader.WaveFormat;
                        long totalSamples = reader.Length / (format.BitsPerSample / 8);
                        int quickSamples = 800;
                        long samplesPerPoint = totalSamples / quickSamples;
                        long bytesPerPoint = samplesPerPoint * (format.BitsPerSample / 8);

                        float[] quickData = new float[quickSamples];
                        var sampleProvider = reader.ToSampleProvider();
                        float[] buffer = new float[8192];

                        System.Threading.Tasks.Parallel.For(0, quickSamples, new ParallelOptions { MaxDegreeOfParallelism = 4 }, i =>
                        {
                            try
                            {
                                long targetByte = i * bytesPerPoint;
                                if (targetByte < reader.Length - buffer.Length * (format.BitsPerSample / 8))
                                {
                                    lock (reader)
                                    {
                                        reader.Position = targetByte;
                                        int samplesRead = sampleProvider.Read(buffer, 0, buffer.Length);
                                        float max = 0f;
                                        for (int j = 0; j < samplesRead; j++)
                                        {
                                            float sample = Math.Abs(buffer[j]);
                                            if (sample > max) max = sample;
                                        }
                                        quickData[i] = max;
                                    }
                                }
                            }
                            catch { quickData[i] = 0f; }
                        });

                        _waveformData = quickData;
                        CreateWaveformBitmap();
                    }
                }
                catch { _waveformBitmap = null; }
            });

            if (this.IsHandleCreated && !this.IsDisposed)
            {
                this.Invoke(new Action(() => picWaveform.Invalidate()));
            }

            await Task.Run(() =>
            {
                try
                {
                    using (var reader = new AudioFileReader(filePath))
                    {
                        var format = reader.WaveFormat;
                        long totalSamples = reader.Length / (format.BitsPerSample / 8);
                        long samplesPerPoint = totalSamples / _waveformSamples;
                        long bytesPerPoint = samplesPerPoint * (format.BitsPerSample / 8);

                        float[] fullData = new float[_waveformSamples];
                        var sampleProvider = reader.ToSampleProvider();
                        float[] buffer = new float[8192];

                        int batchSize = 50;
                        int batches = (_waveformSamples + batchSize - 1) / batchSize;

                        System.Threading.Tasks.Parallel.For(0, batches, new ParallelOptions { MaxDegreeOfParallelism = 4 }, batchIndex =>
                        {
                            int start = batchIndex * batchSize;
                            int end = Math.Min(start + batchSize, _waveformSamples);

                            for (int i = start; i < end; i++)
                            {
                                try
                                {
                                    long targetByte = i * bytesPerPoint;
                                    if (targetByte < reader.Length - buffer.Length * (format.BitsPerSample / 8))
                                    {
                                        lock (reader)
                                        {
                                            reader.Position = targetByte;
                                            int samplesRead = sampleProvider.Read(buffer, 0, buffer.Length);
                                            float max = 0f;
                                            for (int j = 0; j < samplesRead; j++)
                                            {
                                                float sample = Math.Abs(buffer[j]);
                                                if (sample > max) max = sample;
                                            }
                                            fullData[i] = max;
                                        }
                                    }
                                }
                                catch { fullData[i] = 0f; }
                            }
                        });

                        _waveformData = fullData;
                        CreateWaveformBitmap();
                    }
                }
                catch { }
                finally
                {
                    _isLoadingWaveform = false;
                }
            });

            if (this.IsHandleCreated && !this.IsDisposed)
            {
                this.Invoke(new Action(() => picWaveform.Invalidate()));
            }
        }

        private void CreateWaveformBitmap()
        {
            if (_waveformData == null || _waveformData.Length == 0)
                return;

            try
            {
                int width = _waveformData.Length;
                int height = 350;

                _waveformBitmap?.Dispose();
                _waveformBitmap = new Bitmap(width, height);

                using (Graphics g = Graphics.FromImage(_waveformBitmap))
                {
                    g.Clear(Color.FromArgb(10, 10, 10));
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.CompositingQuality = CompositingQuality.HighQuality;

                    int midY = height / 2;

                    using (Pen penTop = new Pen(Color.FromArgb(0, 255, 100), 1.8f))
                    using (Pen penBottom = new Pen(Color.FromArgb(0, 200, 80), 1.8f))
                    {
                        for (int x = 0; x < _waveformData.Length; x++)
                        {
                            float amplitude = _waveformData[x] * (height / 2) * 0.98f;
                            g.DrawLine(penTop, x, midY, x, midY - amplitude);
                            g.DrawLine(penBottom, x, midY, x, midY + amplitude);
                        }
                    }

                    using (Pen penCenter = new Pen(Color.FromArgb(80, 80, 80), 1f))
                    {
                        g.DrawLine(penCenter, 0, midY, width, midY);
                    }
                }
            }
            catch { }
        }

        private string FormatTime(int milliseconds)
        {
            TimeSpan ts = TimeSpan.FromMilliseconds(Math.Abs(milliseconds));
            int totalHours = (int)ts.TotalHours;
            return string.Format("{0:D2}:{1:D2}:{2:D2}.{3:D3}",
                totalHours, ts.Minutes, ts.Seconds, ts.Milliseconds); // ✅ CORRETTO
        }

        private int ParseTime(string timeString)
        {
            try
            {
                string[] parts = timeString.Split(':');
                int hours = int.Parse(parts[0]);
                int minutes = int.Parse(parts[1]);
                string[] secParts = parts[2].Split('.');
                int seconds = int.Parse(secParts[0]);
                int milliseconds = int.Parse(secParts[1]);
                return (hours * 3600000) + (minutes * 60000) + (seconds * 1000) + milliseconds;
            }
            catch
            {
                return 0;
            }
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            if (_isPlaying)
            {
                _waveOut?.Pause();
                _isPlaying = false;
                _positionTimer.Stop();
                btnPlay.Text = "▶ " + LanguageManager.GetString("MusicEditor.Play");
                btnPlay.BackColor = Color.FromArgb(40, 160, 40);
            }
            else
            {
                _waveOut?.Play();
                _isPlaying = true;
                _positionTimer.Start();
                btnPlay.Text = "⏸ " + LanguageManager.GetString("MusicEditor.Pause");
                btnPlay.BackColor = Color.FromArgb(200, 100, 0);
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            _waveOut?.Stop();
            _audioReader.Position = 0;
            _isPlaying = false;
            _positionTimer.Stop();
            btnPlay.Text = "▶ " + LanguageManager.GetString("MusicEditor.Play");
            btnPlay.BackColor = Color.FromArgb(40, 160, 40);
            lblCurrentPosition.Text = "00:00:00.000";
            lblCurrentPositionMs.Text = "0 ms";
            picWaveform.Invalidate();
        }

        private void btnSetMarkerIn_Click(object sender, EventArgs e) => SetMarkerToCurrent("In");
        private void btnSetMarkerIntro_Click(object sender, EventArgs e) => SetMarkerToCurrent("Intro");
        private void btnSetMarkerMix_Click(object sender, EventArgs e) => SetMarkerToCurrent("Mix");
        private void btnSetMarkerOut_Click(object sender, EventArgs e) => SetMarkerToCurrent("Out");

        private void btnMarkerInUp_Click(object sender, EventArgs e)
        {
            _musicEntry.MarkerIN += 10;
            txtMarkerIn.Text = FormatTime(_musicEntry.MarkerIN);
            picWaveform.Invalidate();
            PlayFromMarker("In");
        }

        private void btnMarkerInDown_Click(object sender, EventArgs e)
        {
            _musicEntry.MarkerIN = Math.Max(0, _musicEntry.MarkerIN - 10);
            txtMarkerIn.Text = FormatTime(_musicEntry.MarkerIN);
            picWaveform.Invalidate();
            PlayFromMarker("In");
        }

        private void btnMarkerIntroUp_Click(object sender, EventArgs e)
        {
            _musicEntry.MarkerINTRO += 10;
            txtMarkerIntro.Text = FormatTime(_musicEntry.MarkerINTRO);
            picWaveform.Invalidate();
            PlayFromMarker("Intro");
        }

        private void btnMarkerIntroDown_Click(object sender, EventArgs e)
        {
            _musicEntry.MarkerINTRO = Math.Max(0, _musicEntry.MarkerINTRO - 10);
            txtMarkerIntro.Text = FormatTime(_musicEntry.MarkerINTRO);
            picWaveform.Invalidate();
            PlayFromMarker("Intro");
        }

        private void btnMarkerMixUp_Click(object sender, EventArgs e)
        {
            int offset = _musicEntry.MarkerOUT - _musicEntry.MarkerMIX;
            _musicEntry.MarkerMIX += 10;
            txtMarkerMix.Text = FormatTime(_musicEntry.MarkerMIX);
            _musicEntry.MarkerOUT = _musicEntry.MarkerMIX + offset;
            txtMarkerOut.Text = FormatTime(_musicEntry.MarkerOUT);
            picWaveform.Invalidate();
            PlayFromMarker("Mix");
        }

        private void btnMarkerMixDown_Click(object sender, EventArgs e)
        {
            int offset = _musicEntry.MarkerOUT - _musicEntry.MarkerMIX;
            _musicEntry.MarkerMIX = Math.Max(0, _musicEntry.MarkerMIX - 10);
            txtMarkerMix.Text = FormatTime(_musicEntry.MarkerMIX);
            _musicEntry.MarkerOUT = _musicEntry.MarkerMIX + offset;
            txtMarkerOut.Text = FormatTime(_musicEntry.MarkerOUT);
            picWaveform.Invalidate();
            PlayFromMarker("Mix");
        }

        private void btnMarkerOutUp_Click(object sender, EventArgs e)
        {
            _musicEntry.MarkerOUT += 10;
            txtMarkerOut.Text = FormatTime(_musicEntry.MarkerOUT);
            picWaveform.Invalidate();
            PlayFromMarker("Out");
        }

        private void btnMarkerOutDown_Click(object sender, EventArgs e)
        {
            _musicEntry.MarkerOUT = Math.Max(_musicEntry.MarkerMIX, _musicEntry.MarkerOUT - 10);
            txtMarkerOut.Text = FormatTime(_musicEntry.MarkerOUT);
            picWaveform.Invalidate();
            PlayFromMarker("Out");
        }

        private void btnPlayFromIn_Click(object sender, EventArgs e) => PlayFromMarker("In");
        private void btnPlayFromIntro_Click(object sender, EventArgs e) => PlayFromMarker("Intro");
        private void btnPlayFromMix_Click(object sender, EventArgs e) => PlayFromMarker("Mix");
        private void btnPlayFromOut_Click(object sender, EventArgs e) => PlayFromMarker("Out");

        private void SetMarkerToCurrent(string markerType)
        {
            if (_audioReader == null) return;

            int currentMs = (int)_audioReader.CurrentTime.TotalMilliseconds;

            switch (markerType)
            {
                case "In":
                    _musicEntry.MarkerIN = currentMs;
                    txtMarkerIn.Text = FormatTime(currentMs);
                    break;
                case "Intro":
                    _musicEntry.MarkerINTRO = currentMs;
                    txtMarkerIntro.Text = FormatTime(currentMs);
                    break;
                case "Mix":
                    int offset = _musicEntry.MarkerOUT - _musicEntry.MarkerMIX;
                    _musicEntry.MarkerMIX = currentMs;
                    txtMarkerMix.Text = FormatTime(currentMs);
                    _musicEntry.MarkerOUT = currentMs + offset;
                    txtMarkerOut.Text = FormatTime(_musicEntry.MarkerOUT);
                    break;
                case "Out":
                    _musicEntry.MarkerOUT = Math.Max(currentMs, _musicEntry.MarkerMIX);
                    txtMarkerOut.Text = FormatTime(_musicEntry.MarkerOUT);
                    break;
            }

            picWaveform.Invalidate();
        }

        private void PlayFromMarker(string markerType)
        {
            if (_audioReader == null) return;

            int ms = 0;

            switch (markerType)
            {
                case "In":
                    ms = _musicEntry.MarkerIN;
                    break;
                case "Intro":
                    ms = _musicEntry.MarkerINTRO;
                    break;
                case "Mix":
                    ms = _musicEntry.MarkerMIX;
                    break;
                case "Out":
                    ms = _musicEntry.MarkerOUT;
                    break;
            }

            _audioReader.CurrentTime = TimeSpan.FromMilliseconds(ms);

            if (!_isPlaying)
            {
                btnPlay_Click(null, null);
            }
        }

        private void PositionTimer_Tick(object sender, EventArgs e)
        {
            if (_audioReader != null)
            {
                int currentMs = (int)_audioReader.CurrentTime.TotalMilliseconds;
                lblCurrentPosition.Text = FormatTime(currentMs);
                lblCurrentPositionMs.Text = currentMs.ToString() + " ms";
                picWaveform.Invalidate();
            }
        }

        private void picWaveform_Paint(object sender, PaintEventArgs e)
        {
            if (_waveformBitmap != null)
            {
                e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.DrawImage(_waveformBitmap, 0, 0, picWaveform.Width, picWaveform.Height);
            }

            if (_audioReader != null)
            {
                int totalMs = (int)_audioReader.TotalTime.TotalMilliseconds;

                DrawMarkerWithLabel(e.Graphics, _musicEntry.MarkerIN, totalMs, Color.FromArgb(255, 50, 50), "IN", 3, false);
                DrawMarkerWithLabel(e.Graphics, _musicEntry.MarkerINTRO, totalMs, Color.FromArgb(255, 0, 255), "INTRO", picWaveform.Height / 2, false);
                DrawMarkerWithLabel(e.Graphics, _musicEntry.MarkerMIX, totalMs, Color.FromArgb(255, 255, 0), "MIX", 3, true);
                DrawMarkerWithLabel(e.Graphics, _musicEntry.MarkerOUT, totalMs, Color.FromArgb(255, 140, 0), "OUT", picWaveform.Height - 25, true);

                int currentMs = (int)_audioReader.CurrentTime.TotalMilliseconds;
                float xPos = (float)currentMs / totalMs * picWaveform.Width;
                using (Pen pen = new Pen(Color.White, 2.5f))
                {
                    e.Graphics.DrawLine(pen, xPos, 0, xPos, picWaveform.Height);
                }
            }
        }

        private void DrawMarkerWithLabel(Graphics g, int markerMs, int totalMs, Color color, string label, int labelYPos, bool labelOnLeft)
        {
            if (totalMs == 0) return;

            float xPos = (float)markerMs / totalMs * picWaveform.Width;

            using (Pen pen = new Pen(color, 2.5f))
            {
                g.DrawLine(pen, xPos, 0, xPos, picWaveform.Height);
            }

            if (!string.IsNullOrEmpty(label))
            {
                using (Font font = new Font("Segoe UI", 8, FontStyle.Bold))
                {
                    SizeF textSize = g.MeasureString(label, font);
                    int labelWidth = (int)textSize.Width + 8;
                    int labelHeight = (int)textSize.Height + 4;

                    float labelX = labelOnLeft ? xPos - labelWidth - 5 : xPos + 5;

                    RectangleF labelRect = new RectangleF(labelX, labelYPos, labelWidth, labelHeight);
                    using (SolidBrush bgBrush = new SolidBrush(color))
                    {
                        g.FillRectangle(bgBrush, labelRect);
                    }

                    using (Pen borderPen = new Pen(Color.Black, 1))
                    {
                        g.DrawRectangle(borderPen, labelRect.X, labelRect.Y, labelRect.Width, labelRect.Height);
                    }

                    using (SolidBrush textBrush = new SolidBrush(Color.Black))
                    {
                        g.DrawString(label, font, textBrush, labelX + 4, labelYPos + 2);
                    }
                }
            }
        }

        private void picWaveform_MouseDown(object sender, MouseEventArgs e)
        {
            if (_audioReader == null) return;

            int totalMs = (int)_audioReader.TotalTime.TotalMilliseconds;

            if (CheckMarkerLabelClick(e.X, e.Y, totalMs, _musicEntry.MarkerOUT, "OUT", true)) return;
            if (CheckMarkerLabelClick(e.X, e.Y, totalMs, _musicEntry.MarkerMIX, "MIX", true)) return;
            if (CheckMarkerLabelClick(e.X, e.Y, totalMs, _musicEntry.MarkerINTRO, "INTRO", false)) return;
            if (CheckMarkerLabelClick(e.X, e.Y, totalMs, _musicEntry.MarkerIN, "IN", false)) return;

            int clickMs = (int)((float)e.X / picWaveform.Width * totalMs);
            _audioReader.CurrentTime = TimeSpan.FromMilliseconds(clickMs);
            picWaveform.Invalidate();
        }

        private bool CheckMarkerLabelClick(int mouseX, int mouseY, int totalMs, int markerMs, string markerType, bool labelOnLeft)
        {
            float xPos = (float)markerMs / totalMs * picWaveform.Width;

            if (IsInsideLabel(mouseX, mouseY, xPos, markerType, labelOnLeft))
            {
                _isDraggingMarker = true;
                _draggingMarkerType = markerType;
                picWaveform.Cursor = Cursors.SizeWE;
                return true;
            }

            return false;
        }

        private bool IsInsideLabel(int mouseX, int mouseY, float xPos, string markerType, bool labelOnLeft)
        {
            int labelYPos;

            switch (markerType.ToUpper())
            {
                case "IN":
                case "MIX":
                    labelYPos = 3;
                    break;
                case "INTRO":
                    labelYPos = picWaveform.Height / 2;
                    break;
                case "OUT":
                    labelYPos = picWaveform.Height - 25;
                    break;
                default:
                    labelYPos = 3;
                    break;
            }

            using (Font font = new Font("Segoe UI", 8, FontStyle.Bold))
            using (Graphics g = picWaveform.CreateGraphics())
            {
                SizeF textSize = g.MeasureString(markerType.ToUpper(), font);
                int labelWidth = (int)textSize.Width + 8;
                int labelHeight = (int)textSize.Height + 4;

                float labelX = labelOnLeft ? xPos - labelWidth - 5 : xPos + 5;

                RectangleF labelRect = new RectangleF(labelX, labelYPos, labelWidth, labelHeight);
                return labelRect.Contains(mouseX, mouseY);
            }
        }

        private void picWaveform_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDraggingMarker || _audioReader == null) return;

            int totalMs = (int)_audioReader.TotalTime.TotalMilliseconds;
            int newMs = (int)((float)e.X / picWaveform.Width * totalMs);
            newMs = Math.Max(0, Math.Min(newMs, totalMs));

            switch (_draggingMarkerType)
            {
                case "IN":
                    _musicEntry.MarkerIN = newMs;
                    txtMarkerIn.Text = FormatTime(newMs);
                    break;

                case "INTRO":
                    _musicEntry.MarkerINTRO = newMs;
                    txtMarkerIntro.Text = FormatTime(newMs);
                    break;

                case "MIX":
                    int offset = _musicEntry.MarkerOUT - _musicEntry.MarkerMIX;
                    _musicEntry.MarkerMIX = newMs;
                    txtMarkerMix.Text = FormatTime(newMs);
                    _musicEntry.MarkerOUT = newMs + offset;
                    txtMarkerOut.Text = FormatTime(_musicEntry.MarkerOUT);
                    break;

                case "OUT":
                    _musicEntry.MarkerOUT = Math.Max(newMs, _musicEntry.MarkerMIX);
                    txtMarkerOut.Text = FormatTime(_musicEntry.MarkerOUT);
                    break;
            }

            picWaveform.Invalidate();
        }

        private void picWaveform_MouseUp(object sender, MouseEventArgs e)
        {
            if (_isDraggingMarker)
            {
                PlayFromMarker(_draggingMarkerType);
            }

            _isDraggingMarker = false;
            _draggingMarkerType = "";
            picWaveform.Cursor = Cursors.Default;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                _musicEntry.Title = txtTitle.Text ?? "";
                _musicEntry.Artist = txtArtist.Text ?? "";
                _musicEntry.Album = txtAlbum.Text ?? "";
                _musicEntry.Year = (int)numYear.Value;
                _musicEntry.Genre = cmbGenre.Text ?? "";
                _musicEntry.Categories = txtCategories.Text ?? "";

                _musicEntry.MarkerIN = ParseTime(txtMarkerIn.Text);
                _musicEntry.MarkerINTRO = ParseTime(txtMarkerIntro.Text);
                _musicEntry.MarkerMIX = ParseTime(txtMarkerMix.Text);
                _musicEntry.MarkerOUT = ParseTime(txtMarkerOut.Text);

                _musicEntry.ValidFrom = _chkEnableValidFrom.Checked ? _dtpValidFrom.Value.ToString("yyyy-MM-dd") : "";
                _musicEntry.ValidTo = _chkEnableValidTo.Checked ? _dtpValidTo.Value.ToString("yyyy-MM-dd") : "";

                var selectedMonths = _chkMonths
                    .Select((chk, index) => chk.Checked ? (index + 1).ToString() : null)
                    .Where(m => m != null)
                    .ToList();
                _musicEntry.ValidMonths = selectedMonths.Count > 0 ? string.Join(";", selectedMonths) : "1;2;3;4;5;6;7;8;9;10;11;12";

                string[] dayMap = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
                var selectedDays = _chkDays
                    .Select((chk, index) => chk.Checked ? dayMap[index] : null)
                    .Where(d => d != null)
                    .ToList();
                _musicEntry.ValidDays = selectedDays.Count > 0 ? string.Join(";", selectedDays) : "Sunday;Monday;Tuesday;Wednesday;Thursday;Friday;Saturday";

                var selectedHours = _chkHours
                    .Select((chk, index) => chk.Checked ? index.ToString() : null)
                    .Where(h => h != null)
                    .ToList();
                _musicEntry.ValidHours = selectedHours.Count > 0 ? string.Join(";", selectedHours) : "0;1;2;3;4;5;6;7;8;9;10;11;12;13;14;15;16;17;18;19;20;21;22;23";

                if (_audioReader != null)
                {
                    _musicEntry.Duration = (int)_audioReader.TotalTime.TotalSeconds;
                }

                if (!_isClip)
                {
                    try
                    {
                        if (File.Exists(_musicEntry.FilePath))
                        {
                            var tagFile = TagLib.File.Create(_musicEntry.FilePath);
                            tagFile.Tag.Title = _musicEntry.Title;
                            tagFile.Tag.Performers = new[] { _musicEntry.Artist };
                            tagFile.Tag.Album = _musicEntry.Album;
                            tagFile.Tag.Year = (uint)_musicEntry.Year;
                            tagFile.Tag.Genres = new[] { _musicEntry.Genre };
                            tagFile.Save();
                            tagFile.Dispose();
                        }
                    }
                    catch { }
                }

                bool success;

                if (_isClip)
                {
                    var clipEntry = new ClipEntry
                    {
                        ID = _originalClipId,
                        FilePath = _musicEntry.FilePath,
                        Title = _musicEntry.Title,
                        Genre = _musicEntry.Genre,
                        Categories = _musicEntry.Categories,
                        Duration = _musicEntry.Duration,
                        MarkerIN = _musicEntry.MarkerIN,
                        MarkerINTRO = _musicEntry.MarkerINTRO,
                        MarkerMIX = _musicEntry.MarkerMIX,
                        MarkerOUT = _musicEntry.MarkerOUT,
                        ValidMonths = _musicEntry.ValidMonths,
                        ValidDays = _musicEntry.ValidDays,
                        ValidHours = _musicEntry.ValidHours,
                        ValidFrom = _musicEntry.ValidFrom,
                        ValidTo = _musicEntry.ValidTo,
                        AddedDate = _musicEntry.AddedDate,
                        LastPlayed = _musicEntry.LastPlayed,
                        PlayCount = _musicEntry.PlayCount
                    };

                    success = DbcManager.Update("Clips.dbc", clipEntry);
                }
                else
                {
                    success = DbcManager.Update("Music.dbc", _musicEntry);
                }

                if (success)
                {
                    MessageBox.Show(
                        LanguageManager.GetString("MusicEditor.Message.SaveSuccess"),
                        LanguageManager.GetString("MusicEditor.Title.SaveSuccess"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show(
                        LanguageManager.GetString("MusicEditor.Error.SaveFailed"),
                        LanguageManager.GetString("Common.Error"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(LanguageManager.GetString("MusicEditor.Error.Exception"), ex.Message),
                    LanguageManager.GetString("Common.Error"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                LanguageManager.LanguageChanged -= (s, e) => ApplyLanguage();

                _positionTimer?.Stop();
                _positionTimer?.Dispose();
                _waveOut?.Stop();
                _waveOut?.Dispose();
                _audioReader?.Dispose();
                _waveformBitmap?.Dispose();

                if (components != null)
                {
                    components.Dispose();
                }
            }

            base.Dispose(disposing);
        }
    }
}