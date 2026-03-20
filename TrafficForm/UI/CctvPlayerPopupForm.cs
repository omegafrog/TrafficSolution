using LibVLCSharp.Shared;
using LibVLCSharp.WinForms;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace TrafficForm
{
    public partial class CctvPlayerPopupForm : Form
    {
        private readonly string _displayName;
        private readonly string _cctvUrl;

        private LibVLC? _libVLC;
        private MediaPlayer? _mediaPlayer;
        private Media? _media;
        private VideoView? _videoView;
        private bool _isTeardownInProgress;
        private bool _allowClose;

        public CctvPlayerPopupForm(string displayName, string cctvUrl)
        {
            _displayName = displayName;
            _cctvUrl = cctvUrl;

            InitializeComponent();

            if (string.IsNullOrWhiteSpace(_cctvUrl))
            {
                throw new ArgumentException("cctvUrl cannot be empty.", nameof(cctvUrl));
            }

            string safeDisplayName = string.IsNullOrWhiteSpace(_displayName) ? "CCTV" : _displayName.Trim();

            Text = $"CCTV 재생 - {safeDisplayName} : {_cctvUrl}";
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(700, 460);
            Size = new Size(1100, 760);
            BackColor = Color.FromArgb(24, 28, 36);

            Load += CctvPlayerPopupForm_Load;
            FormClosing += CctvPlayerPopupForm_FormClosing;
        }

        private void CctvPlayerPopupForm_Load(object? sender, EventArgs e)
        {
            Core.Initialize();

            _videoView = new VideoView
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Black
            };

            Controls.Add(_videoView);

            _libVLC = new LibVLC(
                "--network-caching=1000"
            );

            _mediaPlayer = new MediaPlayer(_libVLC);
            _videoView.MediaPlayer = _mediaPlayer;

            _media = new Media(_libVLC, new Uri(_cctvUrl));
            _mediaPlayer.Play(_media);
        }

        private async void CctvPlayerPopupForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (_allowClose)
            {
                _isTeardownInProgress = false;
                return;
            }

            if (_isTeardownInProgress)
            {
                e.Cancel = true;
                return;
            }

            e.Cancel = true;
            _isTeardownInProgress = true;
            Hide();

            (MediaPlayer? mediaPlayer, Media? media, LibVLC? libVLC) = DetachPlaybackFromView();

            await Task.Run(() => DisposePlaybackResources(mediaPlayer, media, libVLC));

            _allowClose = true;
            if (!IsDisposed && IsHandleCreated)
            {
                BeginInvoke(new Action(Close));
            }
        }

        private (MediaPlayer? mediaPlayer, Media? media, LibVLC? libVLC) DetachPlaybackFromView()
        {
            VideoView? videoView = _videoView;
            MediaPlayer? mediaPlayer = _mediaPlayer;
            Media? media = _media;
            LibVLC? libVLC = _libVLC;

            _videoView = null;
            _mediaPlayer = null;
            _media = null;
            _libVLC = null;

            if (videoView != null)
            {
                try
                {
                    videoView.MediaPlayer = null;
                }
                catch (Exception exception)
                {
                    Debug.WriteLine(exception.Message);
                }

                try
                {
                    if (videoView.Parent != null)
                    {
                        videoView.Parent.Controls.Remove(videoView);
                    }
                }
                catch (Exception exception)
                {
                    Debug.WriteLine(exception.Message);
                }

                try
                {
                    videoView.Dispose();
                }
                catch (Exception exception)
                {
                    Debug.WriteLine(exception.Message);
                }
            }

            return (mediaPlayer, media, libVLC);
        }

        private static void DisposePlaybackResources(MediaPlayer? mediaPlayer, Media? media, LibVLC? libVLC)
        {
            try
            {
                mediaPlayer?.Stop();
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);
            }

            try
            {
                mediaPlayer?.Dispose();
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);
            }

            try
            {
                media?.Dispose();
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);
            }

            try
            {
                libVLC?.Dispose();
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);
            }
        }
    }

}
