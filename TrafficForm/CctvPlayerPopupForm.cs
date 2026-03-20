using LibVLCSharp.Shared;
using LibVLCSharp.WinForms;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TrafficForm
{
    public partial class CctvPlayerPopupForm : Form
    {
        private string _displayName;
        private string _cctvUrl;

        private LibVLC? _libVLC;
        private MediaPlayer? _mediaPlayer;
        private VideoView? _videoView;
        public CctvPlayerPopupForm(string displayName, string cctvUrl)
        {
            _displayName = displayName;
            _cctvUrl = cctvUrl;

            InitializeComponent();
            if (_displayName == null || _cctvUrl == null)
                throw new Exception("displayName or cctvUrl cannot be null.");
            string safeDisplayName = string.IsNullOrWhiteSpace(_displayName) ? "CCTV" : _displayName.Trim();
            _ = _cctvUrl;

            Text = $"CCTV 재생 - {safeDisplayName} : {_cctvUrl}";
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(700, 460);
            Size = new Size(1100, 760);
            BackColor = Color.FromArgb(24, 28, 36);

            Load += CctvPlayerPopupForm_Load;
            FormClosed += CctvPlayerPopupForm_FormClosed;
        }
        public async void CctvPlayerPopupForm_Load(object? sender, EventArgs e)
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

            var media = new Media(_libVLC, new Uri(_cctvUrl));
            _mediaPlayer.Play(media);
        }

        private void CctvPlayerPopupForm_FormClosed(object? sender, FormClosedEventArgs e)
        {
            if (_videoView != null)
            {
                _videoView.MediaPlayer = null;
            }

            _mediaPlayer?.Stop();
            _mediaPlayer?.Dispose();
            _libVLC?.Dispose();
            _videoView?.Dispose();
        }
    }

}
