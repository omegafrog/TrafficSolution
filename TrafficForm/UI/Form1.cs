using Microsoft.Web.WebView2.Core;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using TrafficForm.App;
using TrafficForm.Domain;
using TrafficForm.UI;

namespace TrafficForm
{
    public partial class Form1 : Form
    {
        private readonly RequestTrafficByPosService? _requestTrafficByPosService;
        private readonly FavoriteService? _favoriteService;
        private RoadNameSearchService? _roadNameSearchService;
        private readonly Dictionary<string, HighwayListControl> _controlMap = new Dictionary<string, HighwayListControl>();
        private readonly Dictionary<string, int> _latestVdsHighwayNumberById = new Dictionary<string, int>(StringComparer.Ordinal);
        private readonly Dictionary<int, string> _latestRoadSearchHighwayNames = new Dictionary<int, string>();
        private readonly List<int> _latestTrafficHighwayNumbers = new List<int>();
        private int _fixedLeftPanelWidth;
        private const int FixedRightPanelWidth = 520;
        private const int ReducedRightPanelWidth = 320;
        private HighwayListControl? _selectedControl;
        private string? _selectedTrafficVdsId;
        private UpdateSelectedPosTrafficInfoCommand? _latestTrafficSelectionCommand;
        private readonly Panel _searchSummaryPanel = new Panel();
        private readonly Label _searchSummaryTitleLabel = new Label();
        private readonly Label _searchSummaryCountLabel = new Label();
        private readonly Label _searchSummaryDetailLabel = new Label();
        private readonly Panel _roadNameSearchPanel = new Panel();
        private readonly TableLayoutPanel _roadNameSearchLayout = new TableLayoutPanel();
        private readonly Label _roadNameSearchTitleLabel = new Label();
        private readonly Label _roadNameSearchHintLabel = new Label();
        private readonly Label _roadNameSearchLabel = new Label();
        private readonly TextBox _roadNameSearchTextBox = new TextBox();
        private readonly Button _roadNameSearchButton = new Button();
        private readonly ToolStripStatusLabel _statusMessageLabel = new ToolStripStatusLabel
        {
            Spring = true,
            TextAlign = ContentAlignment.MiddleLeft
        };

        private readonly ToolStripProgressBar _statusProgressBar = new ToolStripProgressBar
        {
            Style = ProgressBarStyle.Marquee,
            MarqueeAnimationSpeed = 30,
            Width = 180,
            Visible = false
        };

        private readonly ToolStripComboBox _mapInteractionModeComboBox = new ToolStripComboBox
        {
            Name = "mapInteractionModeComboBox",
            DropDownStyle = ComboBoxStyle.DropDownList,
            AutoSize = false,
            Width = 220
        };

        private const string PosSelectedEventFlag = "pos-selected";
        private const string VdsMarkerSelectedEventFlag = "vds-selected";
        private const string SelectionClearedEventFlag = "selection-cleared";
        private const string VdsMarkerSelectionClearedEventFlag = "vds-selection-cleared";
        private const string DefaultMapModeText = "일반 모드";
        private const string NearbyHighwayLookupModeText = "주변 고속도로 선택 모드";
        private const string RoadNameSearchButtonText = "검색";

        private bool _isTrafficLookupInProgress;
        private int _trafficLookupRequestVersion;
        private MapInteractionMode _mapInteractionMode = MapInteractionMode.None;
        private bool _roadNameSearchUiInitialized;

        private enum MapInteractionMode
        {
            None,
            NearbyHighwayLookup
        }

        public Form1()
        {
            InitializeComponent();
            InitializeStatusStripUi();
            InitializeMapModeUi();
            InitializeRoadNameSearchUi();
            InitializeRightPanelModeUi();
            InitializeHighwayListPanelUi();
            InitializeFavoritesPanelUi();
            SetStatusMessage("모드를 선택하세요.", false);
        }

        public Form1(
            RequestTrafficByPosService requestTrafficByPosService,
            RequestCctvByPosService requestCctvByPosService,
            FavoriteService favoriteService,
            RoadNameSearchService roadNameSearchService)
        {
            InitializeComponent();
            _requestTrafficByPosService = requestTrafficByPosService;
            _requestCctvByPosService = requestCctvByPosService;
            _favoriteService = favoriteService;
            _roadNameSearchService = roadNameSearchService;
            InitializeStatusStripUi();
            InitializeMapModeUi();
            InitializeRoadNameSearchUi();
            InitializeRightPanelModeUi();
            InitializeHighwayListPanelUi();
            InitializeFavoritesPanelUi();
            SetStatusMessage("모드를 선택하세요.", false);
            InitializeWebView();
            _ = LoadFavoritesFromStoreAsync();
            //list펴기ToolStripMenuItem.Click += (s, e) => ShowHighwayPanel();
            list접기ToolStripMenuItem.Click += (s, e) => HideHighwayPanel();
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }

        private void splitContainer1_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void webView21_Click(object sender, EventArgs e)
        {

        }

        private async void InitializeWebView()
        {
            webView21.Dock = DockStyle.Fill;
            splitContainer1.Panel2.Controls.Add(webView21);
            await webView21.EnsureCoreWebView2Async(null);
            webView21.CoreWebView2.WebMessageReceived -= WebView21_WebMessageReceived;
            webView21.CoreWebView2.WebMessageReceived += WebView21_WebMessageReceived;
            webView21.CoreWebView2.NavigationCompleted -= CoreWebView2_NavigationCompleted;
            webView21.CoreWebView2.NavigationCompleted += CoreWebView2_NavigationCompleted;

            //List<Location> locs = await _publicTrafficApi.findAllVdiLoc();
            List<Location> locs = new List<Location>();
            LoadMapHtml(locs);

            //foreach (Location location in locs) {

            //    await webView21.CoreWebView2.ExecuteScriptAsync(
            //        $"moveAndAddMarker({location.Latitude}, {location.Longitude}, '{location.Name}')");
            //}


        }

        private void InitializeStatusStripUi()
        {
            statusStrip1.Items.Clear();
            statusStrip1.Items.Add(_statusMessageLabel);
            statusStrip1.Items.Add(_statusProgressBar);
        }

        private void InitializeMapModeUi()
        {
            toolStrip1.Items.Clear();
            toolStrip1.Items.Add(new ToolStripLabel("지도 모드"));

            _mapInteractionModeComboBox.Items.Clear();
            _mapInteractionModeComboBox.Items.Add(DefaultMapModeText);
            _mapInteractionModeComboBox.Items.Add(NearbyHighwayLookupModeText);
            _mapInteractionModeComboBox.SelectedIndexChanged -= MapInteractionModeComboBox_SelectedIndexChanged;
            _mapInteractionModeComboBox.SelectedIndexChanged += MapInteractionModeComboBox_SelectedIndexChanged;
            _mapInteractionModeComboBox.SelectedItem = DefaultMapModeText;

            toolStrip1.Items.Add(_mapInteractionModeComboBox);
            InitializeToolboxFavoritesTabs();
        }

        private void InitializeRoadNameSearchUi()
        {
            if (_roadNameSearchUiInitialized)
            {
                return;
            }

            _roadNameSearchPanel.Dock = DockStyle.Top;
            _roadNameSearchPanel.Height = 112;
            _roadNameSearchPanel.Margin = Padding.Empty;
            _roadNameSearchPanel.Padding = new Padding(12, 10, 12, 10);
            _roadNameSearchPanel.BackColor = Color.FromArgb(246, 247, 249);
            _roadNameSearchPanel.BorderStyle = BorderStyle.FixedSingle;

            _roadNameSearchLayout.Dock = DockStyle.Fill;
            _roadNameSearchLayout.ColumnCount = 3;
            _roadNameSearchLayout.RowCount = 3;
            _roadNameSearchLayout.ColumnStyles.Clear();
            _roadNameSearchLayout.RowStyles.Clear();
            _roadNameSearchLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            _roadNameSearchLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _roadNameSearchLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            _roadNameSearchLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _roadNameSearchLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _roadNameSearchLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            _roadNameSearchTitleLabel.AutoSize = true;
            _roadNameSearchTitleLabel.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
            _roadNameSearchTitleLabel.ForeColor = Color.FromArgb(33, 37, 41);
            _roadNameSearchTitleLabel.Margin = Padding.Empty;
            _roadNameSearchTitleLabel.Text = "도로명 검색";

            _roadNameSearchHintLabel.AutoSize = true;
            _roadNameSearchHintLabel.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular, GraphicsUnit.Point);
            _roadNameSearchHintLabel.ForeColor = Color.FromArgb(96, 103, 112);
            _roadNameSearchHintLabel.Margin = new Padding(0, 4, 0, 8);

            _roadNameSearchLabel.AutoSize = true;
            _roadNameSearchLabel.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular, GraphicsUnit.Point);
            _roadNameSearchLabel.ForeColor = Color.FromArgb(61, 67, 74);
            _roadNameSearchLabel.Margin = new Padding(0, 4, 10, 0);
            _roadNameSearchLabel.Text = "도로명";

            _roadNameSearchTextBox.Dock = DockStyle.Top;
            _roadNameSearchTextBox.Height = 28;
            _roadNameSearchTextBox.Margin = new Padding(0, 0, 10, 0);
            _roadNameSearchTextBox.PlaceholderText = "예: 경부고속도로";
            _roadNameSearchTextBox.KeyDown += RoadNameSearchTextBox_KeyDown;

            _roadNameSearchButton.AutoSize = false;
            _roadNameSearchButton.Size = new Size(88, 28);
            _roadNameSearchButton.Margin = Padding.Empty;
            _roadNameSearchButton.Text = RoadNameSearchButtonText;
            _roadNameSearchButton.Click += RoadNameSearchButton_Click;

            _roadNameSearchLayout.Controls.Clear();
            _roadNameSearchLayout.Controls.Add(_roadNameSearchTitleLabel, 0, 0);
            _roadNameSearchLayout.SetColumnSpan(_roadNameSearchTitleLabel, 3);
            _roadNameSearchLayout.Controls.Add(_roadNameSearchHintLabel, 0, 1);
            _roadNameSearchLayout.SetColumnSpan(_roadNameSearchHintLabel, 3);
            _roadNameSearchLayout.Controls.Add(_roadNameSearchLabel, 0, 2);
            _roadNameSearchLayout.Controls.Add(_roadNameSearchTextBox, 1, 2);
            _roadNameSearchLayout.Controls.Add(_roadNameSearchButton, 2, 2);

            _roadNameSearchPanel.Controls.Clear();
            _roadNameSearchPanel.Controls.Add(_roadNameSearchLayout);

            if (!splitContainer1.Panel2.Controls.Contains(_roadNameSearchPanel))
            {
                splitContainer1.Panel2.Controls.Add(_roadNameSearchPanel);
            }

            splitContainer1.Panel2.Controls.SetChildIndex(_roadNameSearchPanel, 0);
            _roadNameSearchUiInitialized = true;
            UpdateRoadNameSearchHint();
        }

        private void InitializeHighwayListPanelUi()
        {
            Color panelBackground = Color.FromArgb(243, 246, 251);

            highwaylistContainer.IsSplitterFixed = true;
            splitContainer1.FixedPanel = FixedPanel.Panel1;
            splitContainer1.IsSplitterFixed = true;

            if (_fixedLeftPanelWidth <= 0)
            {
                _fixedLeftPanelWidth = Math.Max(140, splitContainer1.SplitterDistance);
            }

            splitContainer1.Panel1MinSize = _fixedLeftPanelWidth;
            splitContainer1.SizeChanged -= SplitContainer1_SizeChanged;
            splitContainer1.SizeChanged += SplitContainer1_SizeChanged;
            LockLeftPanelWidth();

            highwaylistContainer.Panel2.BackColor = panelBackground;

            flowLayoutPanel1.BackColor = panelBackground;
            flowLayoutPanel1.Padding = new Padding(12, 10, 12, 12);
            flowLayoutPanel1.Margin = Padding.Empty;
            flowLayoutPanel1.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanel1.WrapContents = false;
            flowLayoutPanel1.Dock = DockStyle.Fill;
            flowLayoutPanel1.SizeChanged -= FlowLayoutPanel1_SizeChanged;
            flowLayoutPanel1.SizeChanged += FlowLayoutPanel1_SizeChanged;

            _searchSummaryPanel.BackColor = Color.FromArgb(227, 236, 250);
            _searchSummaryPanel.BorderStyle = BorderStyle.FixedSingle;
            _searchSummaryPanel.Dock = DockStyle.Top;
            _searchSummaryPanel.Height = 90;
            _searchSummaryPanel.Margin = Padding.Empty;

            _searchSummaryTitleLabel.AutoSize = true;
            _searchSummaryTitleLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            _searchSummaryTitleLabel.ForeColor = Color.FromArgb(46, 59, 79);
            _searchSummaryTitleLabel.Location = new Point(10, 9);
            _searchSummaryTitleLabel.Text = "VDS 검색 결과";

            _searchSummaryCountLabel.AutoSize = true;
            _searchSummaryCountLabel.Font = new Font("Segoe UI", 16F, FontStyle.Bold, GraphicsUnit.Point);
            _searchSummaryCountLabel.ForeColor = Color.FromArgb(26, 79, 161);
            _searchSummaryCountLabel.Location = new Point(8, 26);
            _searchSummaryCountLabel.Text = "0건";

            _searchSummaryDetailLabel.AutoEllipsis = true;
            _searchSummaryDetailLabel.ForeColor = Color.FromArgb(67, 76, 92);
            _searchSummaryDetailLabel.Location = new Point(10, 63);
            _searchSummaryDetailLabel.Size = new Size(Math.Max(180, highwaylistContainer.Panel2.Width - 22), 18);
            _searchSummaryDetailLabel.Text = "선택된 VDS가 없습니다.";

            _searchSummaryPanel.Controls.Clear();
            _searchSummaryPanel.Controls.Add(_searchSummaryTitleLabel);
            _searchSummaryPanel.Controls.Add(_searchSummaryCountLabel);
            _searchSummaryPanel.Controls.Add(_searchSummaryDetailLabel);

            if (!highwaylistContainer.Panel2.Controls.Contains(_searchSummaryPanel))
            {
                highwaylistContainer.Panel2.Controls.Add(_searchSummaryPanel);
            }

            if (highwaylistContainer.Panel2.Controls.Contains(flowLayoutPanel1))
            {
                highwaylistContainer.Panel2.Controls.SetChildIndex(flowLayoutPanel1, 0);
            }

            if (highwaylistContainer.Panel2.Controls.Contains(_searchSummaryPanel))
            {
                highwaylistContainer.Panel2.Controls.SetChildIndex(
                    _searchSummaryPanel,
                    highwaylistContainer.Panel2.Controls.Count - 1);
            }

            highwaylistContainer.Panel2.PerformLayout();
            UpdateSearchSummary(0, 0);
            highwaylistContainer.Panel2Collapsed = true;
            detailPanelOpen = false;
            InitializeRightEdgeToggleButton();
            UpdateRightPanelToggleButtonText();
        }

        private void SplitContainer1_SizeChanged(object? sender, EventArgs e)
        {
            LockLeftPanelWidth();
        }

        private void LockLeftPanelWidth()
        {
            if (_fixedLeftPanelWidth <= 0 || splitContainer1.Width <= 0)
            {
                return;
            }

            int minimumLeft = Math.Max(120, splitContainer1.Panel1MinSize);
            int maximumLeft = Math.Max(
                minimumLeft,
                splitContainer1.Width - splitContainer1.Panel2MinSize - splitContainer1.SplitterWidth);

            int targetLeft = Math.Min(_fixedLeftPanelWidth, maximumLeft);
            if (targetLeft < minimumLeft)
            {
                targetLeft = minimumLeft;
            }

            if (targetLeft != splitContainer1.SplitterDistance)
            {
                splitContainer1.SplitterDistance = targetLeft;
            }
        }

        private async void MapInteractionModeComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            MapInteractionMode nextMode = string.Equals(
                _mapInteractionModeComboBox.SelectedItem as string,
                NearbyHighwayLookupModeText,
                StringComparison.Ordinal)
                ? MapInteractionMode.NearbyHighwayLookup
                : MapInteractionMode.None;

            await SetMapInteractionModeAsync(nextMode);
        }

        private async Task SetMapInteractionModeAsync(MapInteractionMode mapInteractionMode)
        {
            _mapInteractionMode = mapInteractionMode;
            await UpdateMapCursorAsync();

            if (_mapInteractionMode == MapInteractionMode.NearbyHighwayLookup)
            {
                SetStatusMessage($"좌표 선택 모드입니다. {GetCurrentPanelModeDisplayText()}에서 지도를 클릭하세요.", false);
            }
            else
            {
                SetStatusMessage($"지도 모드입니다. {GetCurrentPanelModeDisplayText()} 좌표 조회가 비활성화되었습니다.", false);
            }
        }

        private async Task UpdateMapCursorAsync()
        {
            if (webView21.CoreWebView2 == null)
            {
                return;
            }

            bool isLookupMode = _mapInteractionMode == MapInteractionMode.NearbyHighwayLookup;
            string script = $"setPosSelectionMode({isLookupMode.ToString().ToLowerInvariant()});";
            try
            {
                await webView21.CoreWebView2.ExecuteScriptAsync(script);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);
            }
        }

        private void SetStatusMessage(string message, bool showBusyIndicator)
        {
            _statusMessageLabel.Text = $"[{DateTime.Now:HH:mm:ss}] {message}";
            _statusProgressBar.Visible = showBusyIndicator;
        }

        private void SetLookupUiBusy(bool isBusy)
        {
            _mapInteractionModeComboBox.Enabled = !isBusy;
            _rightPanelModeComboBox.Enabled = !isBusy;
            _roadNameSearchButton.Enabled = !isBusy;
            _roadNameSearchTextBox.Enabled = !isBusy;
        }

        private async void RoadNameSearchButton_Click(object? sender, EventArgs e)
        {
            await ExecuteRoadNameSearchAsync();
        }

        private async void RoadNameSearchTextBox_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            e.Handled = true;
            e.SuppressKeyPress = true;
            await ExecuteRoadNameSearchAsync();
        }

        private async Task ExecuteRoadNameSearchAsync()
        {
            if (_roadNameSearchService == null)
            {
                SetStatusMessage("도로명 검색 서비스가 초기화되지 않았습니다.", false);
                return;
            }

            string roadName = _roadNameSearchTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(roadName))
            {
                SetStatusMessage("도로명을 입력해 주세요.", false);
                return;
            }

            if (webView21.CoreWebView2 == null)
            {
                SetStatusMessage("지도가 아직 준비되지 않았습니다.", false);
                return;
            }

            MapViewSnapshot? snapshot = await CaptureCurrentMapViewSnapshotAsync();
            if (snapshot == null)
            {
                SetStatusMessage("현재 지도 범위를 읽지 못했습니다.", false);
                return;
            }

            CurrentMode mode = _rightPanelMode == RightPanelMode.Cctv
                ? CurrentMode.Cctv
                : CurrentMode.Traffic;

            RoadNameSearchCommand command = new RoadNameSearchCommand(
                roadName,
                new MapBounds
                {
                    MinLongitude = snapshot.MinLongitude,
                    MinLatitude = snapshot.MinLatitude,
                    MaxLongitude = snapshot.MaxLongitude,
                    MaxLatitude = snapshot.MaxLatitude
                },
                mode);

            _roadNameSearchButton.Enabled = false;
            _roadNameSearchTextBox.Enabled = false;
            SetStatusMessage($"도로명 '{roadName}'을(를) 검색 중입니다...", true);

            try
            {
                RoadSearchDispatchResult result = await _roadNameSearchService.SearchRoadByNameAsync(command);
                await FocusRoadNameCandidateAsync(snapshot, result.Candidate);

                string message = result.CreateSelectionMessage();
                if (result.IsCctvMode)
                {
                    await UpdateSelectedPosCctvInfoFromMessage(message);
                }
                else
                {
                    await UpdateSelectedPosTrafficInfoFromMessage(message);
                }

                SetStatusMessage($"도로명 검색 완료: {result.Candidate.HighwayName}", false);
            }
            catch (Exception exception)
            {
                SetStatusMessage($"도로명 검색 실패: {exception.Message}", false);
                Debug.WriteLine(exception.Message);
            }
            finally
            {
                _roadNameSearchButton.Enabled = true;
                _roadNameSearchTextBox.Enabled = true;
            }
        }

        private async Task FocusRoadNameCandidateAsync(MapViewSnapshot snapshot, RoadNameCandidate candidate)
        {
            if (webView21.CoreWebView2 == null)
            {
                return;
            }

            string latitude = candidate.Latitude.ToString(CultureInfo.InvariantCulture);
            string longitude = candidate.Longitude.ToString(CultureInfo.InvariantCulture);
            string zoom = snapshot.ZoomLevel.ToString(CultureInfo.InvariantCulture);
            string minLongitude = snapshot.MinLongitude.ToString(CultureInfo.InvariantCulture);
            string minLatitude = snapshot.MinLatitude.ToString(CultureInfo.InvariantCulture);
            string maxLongitude = snapshot.MaxLongitude.ToString(CultureInfo.InvariantCulture);
            string maxLatitude = snapshot.MaxLatitude.ToString(CultureInfo.InvariantCulture);

            await webView21.CoreWebView2.ExecuteScriptAsync(
                $"setMapViewFromFavorite({latitude}, {longitude}, {zoom}, {minLongitude}, {minLatitude}, {maxLongitude}, {maxLatitude});");
        }

        private void UpdateRoadNameSearchHint()
        {
            if (!_roadNameSearchUiInitialized)
            {
                return;
            }

            _roadNameSearchHintLabel.Text = _rightPanelMode == RightPanelMode.Cctv
                ? "현재 CCTV 모드입니다. 도로명을 입력하면 CCTV 조회 흐름으로 연결됩니다."
                : "현재 혼잡도 모드입니다. 도로명을 입력하면 VDS 조회 흐름으로 연결됩니다.";
        }

        private async void CoreWebView2_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (!e.IsSuccess)
            {
                SetStatusMessage("지도 로딩에 실패했습니다.", false);
                return;
            }

            await UpdateMapCursorAsync();
            SetStatusMessage("지도가 준비되었습니다.", false);
        }
        private void LoadMapHtml(List<Location> locs)
        {
            var markerData = locs.Select(x => new
            {
                latitude = x.Latitude,
                longitude = x.Longitude,
                name = x.Name
            });

            string json = "[]";
            //System.Text.Json.JsonSerializer.Serialize(markerData);
            string html = $$"""
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset="utf-8" />
                <title>Leaflet Test</title>
                <meta name="viewport" content="width=device-width, initial-scale=1.0">

                <link rel="stylesheet" href="https://unpkg.com/leaflet@1.9.4/dist/leaflet.css" />
                <script src="https://unpkg.com/leaflet@1.9.4/dist/leaflet.js"></script>

                <style>
                html, body, #map {
                    width: 100%;
                    height: 100%;
                    margin: 0;
                }
                </style>
            </head>
            <body>
                <div id="map"></div>

                <script>
                const map = L.map('map').setView([37.5665, 126.9780], 12);

                L.tileLayer('http://localhost:8080/tile/{z}/{x}/{y}.png', {
                    maxZoom: 20,
                    attribution: '© OpenStreetMap'
                }).addTo(map);
                let isPosSelectionMode = false;

                function applyMapCursor() {
                    map.getContainer().style.cursor = isPosSelectionMode ? 'crosshair' : '';
                }

                function setPosSelectionMode(enabled) {
                    isPosSelectionMode = Boolean(enabled);
                    applyMapCursor();
                }

                function getMapViewState() {
                    const bounds = map.getBounds();
                    const center = map.getCenter();

                    return {
                        lat: center.lat,
                        lon: center.lng,
                        zoom: map.getZoom(),
                        minLon: bounds.getWest(),
                        minLat: bounds.getSouth(),
                        maxLon: bounds.getEast(),
                        maxLat: bounds.getNorth()
                    };
                }

                function setMapViewFromFavorite(lat, lon, zoom, minLon, minLat, maxLon, maxLat) {
                    const hasBounds =
                        Number.isFinite(minLon)
                        && Number.isFinite(minLat)
                        && Number.isFinite(maxLon)
                        && Number.isFinite(maxLat);

                    if (hasBounds) {
                        map.fitBounds([[minLat, minLon], [maxLat, maxLon]]);
                    }

                    if (Number.isFinite(lat) && Number.isFinite(lon)) {
                        if (Number.isFinite(zoom)) {
                            map.setView([lat, lon], zoom);
                        } else {
                            map.panTo([lat, lon]);
                        }
                    }
                }

                applyMapCursor();

                map.on('click', function(e) {
                window.chrome.webview.postMessage({
                    type: "{{VdsMarkerSelectionClearedEventFlag}}"
                });

                if (!isPosSelectionMode) {
                    window.chrome.webview.postMessage({
                        type: "{{SelectionClearedEventFlag}}"
                    });
                    return;
                }

                const bounds = map.getBounds();
                const data = {
                    type: "{{PosSelectedEventFlag}}",
                    lat: e.latlng.lat,
                    lon: e.latlng.lng,
                    minLon: bounds.getWest(),
                    minLat: bounds.getSouth(),
                    maxLon: bounds.getEast(),
                    maxLat: bounds.getNorth()
                };
                window.chrome.webview.postMessage(JSON.stringify(data));
            });
                     // 마커저장용
            let custommarkers = [];
            let customsegments = [];
            let customCctvMarkers = [];
            let cctvMarkerById = {};
            let selectedCctvMarkerId = null;

            const defaultCctvMarkerStyle = {
              radius: 8,
              color: '#1b5e20',
              fillColor: '#66bb6a',
              fillOpacity: 0.95,
              weight: 2
            };

            const highlightedCctvMarkerStyle = {
              radius: 11,
              color: '#ef6c00',
              fillColor: '#ffcc80',
              fillOpacity: 1,
              weight: 3
            };

            // 기본 마커 추가
            function addMarker(vdsId, lat, lon, text) {
              const marker = L.marker([lat, lon]).addTo(map);

              if (text) {
                marker.bindPopup(text);
              }
              marker.on('click', function(e){
                L.DomEvent.stopPropagation(e);
                window.chrome.webview.postMessage({
                    type: "{{VdsMarkerSelectedEventFlag}}",
                    id: vdsId
                });
              });

              custommarkers.push(marker);
              return marker;
            }

            function addSegment(points, color) {
              if (!Array.isArray(points) || points.length < 2) {
                return null;
              }

              const latlngs = points
                .filter(p => p && typeof p.latitude === 'number' && typeof p.longitude === 'number')
                .map(p => [p.latitude, p.longitude]);

              if (latlngs.length < 2) {
                return null;
              }

              const segment = L.polyline(latlngs, {
                color: color || '#6d6d6d',
                weight: 7,
                opacity: 0.9,
                lineCap: 'round'
              }).addTo(map);

              customsegments.push(segment);
              return segment;
            }

            function addCctvMarker(cctvId, lat, lon, text) {
              const marker = L.circleMarker([lat, lon], defaultCctvMarkerStyle).addTo(map);

              if (text) {
                marker.bindPopup(text);
              }

              cctvMarkerById[cctvId] = marker;

              marker.on('click', function(e){
                L.DomEvent.stopPropagation(e);
                focusCctvMarker(cctvId, false);
                window.chrome.webview.postMessage({
                    type: "{{CctvMarkerSelectedEventFlag}}",
                    id: cctvId
                });
              });

              customCctvMarkers.push(marker);
              return marker;
            }

            function focusCctvMarker(cctvId, openPopup) {
              if (selectedCctvMarkerId && cctvMarkerById[selectedCctvMarkerId]) {
                cctvMarkerById[selectedCctvMarkerId].setStyle(defaultCctvMarkerStyle);
              }

              if (!cctvId) {
                selectedCctvMarkerId = null;
                return;
              }

              const marker = cctvMarkerById[cctvId];
              if (!marker) {
                selectedCctvMarkerId = null;
                return;
              }

              marker.setStyle(highlightedCctvMarkerStyle);
              marker.bringToFront();
              if (openPopup && marker.getPopup()) {
                marker.openPopup();
              }
              map.panTo(marker.getLatLng());
              selectedCctvMarkerId = cctvId;
            }

            function highlightCctvMarker(cctvId) {
              focusCctvMarker(cctvId, true);
            }

            function clearHighlightedCctvMarker() {
              focusCctvMarker(null, false);
            }

            // 기존 마커 제거
            function clearMarkers() {
              custommarkers.forEach(m => map.removeLayer(m));
              custommarkers = [];
            }

            function clearSegments() {
              customsegments.forEach(s => map.removeLayer(s));
              customsegments = [];
            }

            function clearCctvMarkers() {
              customCctvMarkers.forEach(m => map.removeLayer(m));
              customCctvMarkers = [];
              cctvMarkerById = {};
              selectedCctvMarkerId = null;
            }

            // 특정 위치로 이동하면서 마커 추가
            //function moveAndAddMarker(lat, lon, text) {
            //  map.setView([lat, lon], 15);
            //  addMarker(lat, lon, text);
            //}

            //// json 데이터 기반 모두 마커찍기
            //const markers = {{json}};

            //markers.forEach(item => {
            //  const marker = L.marker([item.latitude, item.longitude]).addTo(map);
            //  if (item.name) {
            //    marker.bindPopup(item.name);
            //  }
            //});


                </script>
            </body>
            </html>
            """;

            webView21.NavigateToString(html);

        }

        private void flowLayoutPanel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void webView21_Click_1(object sender, EventArgs e)
        {

        }

        private void splitContainer2_Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void FlowLayoutPanel1_SizeChanged(object? sender, EventArgs e)
        {
            ResizeRightPanelCards();
            _searchSummaryDetailLabel.Width = Math.Max(180, highwaylistContainer.Panel2.Width - 22);
        }

        private void UpdateSearchSummary(int totalCount, int displayedCount)
        {
            _searchSummaryCountLabel.Text = $"{displayedCount:N0}건";

            if (displayedCount == 0)
            {
                _searchSummaryDetailLabel.Text = "조회된 VDS가 없습니다.";
                return;
            }

            _searchSummaryDetailLabel.Text = $"조회 {totalCount:N0}건, 목록 표시 {displayedCount:N0}건";
        }

        private void ResizeRightPanelCards()
        {
            if (flowLayoutPanel1.Controls.Count == 0)
            {
                return;
            }

            int targetWidth = CalculateRightPanelCardWidth();

            foreach (Control control in flowLayoutPanel1.Controls)
            {
                if (control is HighwayListControl || control is CctvListControl)
                {
                    control.Width = targetWidth;
                }
            }
        }

        private void SelectControl(HighwayListControl? control)
        {
            if (_selectedControl == control)
            {
                if (_selectedControl != null && flowLayoutPanel1.Controls.Contains(_selectedControl))
                {
                    _selectedControl.SetHighlighted(true);
                    flowLayoutPanel1.ScrollControlIntoView(_selectedControl);
                    UpdateSearchSummary(_controlMap.Count, _controlMap.Count);
                }

                return;
            }

            _selectedControl?.ClearHighlight();
            _selectedControl = control;

            if (_selectedControl != null && flowLayoutPanel1.Controls.Contains(_selectedControl))
            {
                _selectedControl.SetHighlighted(true);
                flowLayoutPanel1.ScrollControlIntoView(_selectedControl);
            }

            if (_selectedControl == null)
            {
                UpdateSearchSummary(_controlMap.Count, _controlMap.Count);
            }
        }

        private void ClearSelectedControl()
        {
            SelectControl(null);
        }

        private bool detailPanelOpen = false;
        private int detailPanelWidth = 320;

        private static (double Latitude, double Longitude) OffsetOverlappedMarker(double latitude, double longitude, int overlapIndex)
        {
            if (overlapIndex <= 0)
            {
                return (latitude, longitude);
            }

            double radius = 0.00012;
            double angle = (Math.PI / 3.0) * overlapIndex;
            double adjustedLatitude = latitude + radius * Math.Sin(angle);
            double adjustedLongitude = longitude + radius * Math.Cos(angle);
            return (adjustedLatitude, adjustedLongitude);
        }

        private async Task ShowHighwayPanel(List<VdsTrafficResult> results)
        {
            foreach (CctvListControl existingCctvControl in flowLayoutPanel1.Controls.OfType<CctvListControl>())
            {
                existingCctvControl.CardClicked -= CctvListControl_CardClicked;
            }

            detailPanelOpen = true;
            detailPanelWidth = ReducedRightPanelWidth;
            SetRightPanelContentMode(RightPanelContentMode.Results);
            EnsureRightPanelVisible();

            SelectTrafficControl(null);
            SelectCctvControl(null);
            flowLayoutPanel1.Controls.Clear();
            _controlMap.Clear();
            _cctvControlMap.Clear();

            if (webView21.CoreWebView2 != null)
            {
                await webView21.CoreWebView2.ExecuteScriptAsync("clearMarkers()");
                await webView21.CoreWebView2.ExecuteScriptAsync("clearSegments()");
                await webView21.CoreWebView2.ExecuteScriptAsync("clearCctvMarkers()");
            }

            HashSet<string> renderedVdsIds = new HashSet<string>();
            Dictionary<string, int> markerOverlapCountByCoordinate = new Dictionary<string, int>();

            flowLayoutPanel1.SuspendLayout();

            foreach (VdsTrafficResult result in results)
            {
                if (!renderedVdsIds.Add(result.VdsId))
                {
                    continue;
                }

                HighwayListControl control = new(result){};
                flowLayoutPanel1.Controls.Add(control);

                string coordinateKey = $"{result.Location.Latitude:F6},{result.Location.Longitude:F6}";
                markerOverlapCountByCoordinate.TryGetValue(coordinateKey, out int overlapIndex);
                markerOverlapCountByCoordinate[coordinateKey] = overlapIndex + 1;

                (double markerLatitude, double markerLongitude) = OffsetOverlappedMarker(
                    result.Location.Latitude,
                    result.Location.Longitude,
                    overlapIndex);

                if (webView21.CoreWebView2 != null)
                {
                    string markerLatitudeText = markerLatitude.ToString(CultureInfo.InvariantCulture);
                    string markerLongitudeText = markerLongitude.ToString(CultureInfo.InvariantCulture);
                    string markerId = EscapeJavaScriptString(result.VdsId);
                    await webView21.CoreWebView2.ExecuteScriptAsync($"addMarker('{markerId}' ,{markerLatitudeText}, {markerLongitudeText}, '{markerId}')");
                }

                if (webView21.CoreWebView2 != null && result.ResponsibilitySegment.Count > 1)
                {
                    string segmentPointsJson = JsonSerializer.Serialize(result.ResponsibilitySegment.Select(point => new
                    {
                        latitude = point.Latitude,
                        longitude = point.Longitude
                    }));
                    string color = TrafficLevelPolicy.GetColorHex(result.TrafficLevel);
                    await webView21.CoreWebView2.ExecuteScriptAsync($"addSegment({segmentPointsJson}, '{color}')");
                }

                _controlMap[result.VdsId] = control;
            }

            highwaylistContainer.PerformLayout();
            EnsureRightPanelVisible();
            flowLayoutPanel1.ResumeLayout();
            flowLayoutPanel1.PerformLayout();

            ResizeRightPanelCards();
            UpdateSearchSummary(results.Count, _controlMap.Count);

            highwaylistContainer.SplitterDistance = highwaylistContainer.Width - detailPanelWidth;
        }

        private int CalculateRightPanelCardWidth()
        {
            int verticalScrollbarWidth = flowLayoutPanel1.VerticalScroll.Visible
                ? SystemInformation.VerticalScrollBarWidth
                : 0;

            int calculatedWidth = flowLayoutPanel1.ClientSize.Width
                - flowLayoutPanel1.Padding.Horizontal
                - verticalScrollbarWidth
                - 4;

            return Math.Max(236, calculatedWidth);
        }

        private void EnsureRightPanelVisible()
        {
            bool wasCollapsed = highwaylistContainer.Panel2Collapsed;

            if (wasCollapsed)
            {
                highwaylistContainer.Panel2Collapsed = false;
            }

            int containerWidth = highwaylistContainer.ClientSize.Width;
            if (containerWidth <= 0)
            {
                return;
            }

            int desiredRightPanelWidth = Math.Max(220, detailPanelWidth);
            int preferredSplitterDistance = containerWidth - desiredRightPanelWidth - highwaylistContainer.SplitterWidth;

            int minSplitterDistance = Math.Max(120, highwaylistContainer.Panel1MinSize);
            int maxSplitterDistance = Math.Max(
                minSplitterDistance,
                containerWidth - highwaylistContainer.Panel2MinSize - highwaylistContainer.SplitterWidth);

            int boundedSplitterDistance = Math.Min(Math.Max(preferredSplitterDistance, minSplitterDistance), maxSplitterDistance);
            if (boundedSplitterDistance != highwaylistContainer.SplitterDistance)
            {
                highwaylistContainer.SplitterDistance = boundedSplitterDistance;
            }

            // Preserve detailPanelWidth set by caller (ReducedRightPanelWidth or FixedRightPanelWidth)

            LockLeftPanelWidth();
            UpdateRightPanelToggleButtonText();
        }

        private void HighwaylistContainer_SplitterMoved(object? sender, SplitterEventArgs e)
        {
            if (!highwaylistContainer.Panel2Collapsed && highwaylistContainer.Panel2.Width > 0)
            {
                detailPanelWidth = Math.Max(220, highwaylistContainer.Panel2.Width);
            }
        }

        private void HideHighwayPanel()
        {
            if (!detailPanelOpen) return;

            //highwaylistContainer.SplitterDistance = 0;
            SelectTrafficControl(null);
            SelectCctvControl(null);
            detailPanelOpen = false;
            highwaylistContainer.Panel2Collapsed = true;
            UpdateRightPanelToggleButtonText();

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void list출력ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
        private async void WebView21_WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {

            string message = e.WebMessageAsJson.Replace("\\\"", "\"").Trim('"');
            if (IsPosSelectedEvent(message))
            {
                if (_mapInteractionMode != MapInteractionMode.NearbyHighwayLookup)
                {
                    SetStatusMessage("일반 모드입니다. '주변 고속도로 선택 모드'에서만 조회할 수 있습니다.", false);
                    return;
                }

                if (_isTrafficLookupInProgress && _rightPanelMode == RightPanelMode.Traffic)
                {
                    SetStatusMessage("이미 혼잡도 조회 중입니다. 잠시만 기다려주세요.", true);
                    return;
                }

                if (_isCctvLookupInProgress && _rightPanelMode == RightPanelMode.Cctv)
                {
                    SetStatusMessage("이미 CCTV 조회 중입니다. 잠시만 기다려주세요.", true);
                    return;
                }

                if (_rightPanelMode == RightPanelMode.Cctv)
                {
                    await UpdateSelectedPosCctvInfoFromMessage(message);
                }
                else
                {
                    await UpdateSelectedPosTrafficInfoFromMessage(message);
                }
            }
            else if (IsVdsSelectedEvent(message))
            {
                if (_rightPanelMode != RightPanelMode.Traffic)
                {
                    return;
                }

                await HighlightSelectedVdsControlFromMessage(message);
            }
            else if (IsCctvSelectedEvent(message))
            {
                if (_rightPanelMode != RightPanelMode.Cctv)
                {
                    return;
                }

                await HighlightSelectedCctvControlFromMessage(message);
            }
            else if (IsSelectionClearedEvent(message))
            {
                SelectTrafficControl(null);
                SelectCctvControl(null);

                if (webView21.CoreWebView2 != null)
                {
                    await webView21.CoreWebView2.ExecuteScriptAsync("clearHighlightedCctvMarker()");
                }
            }
            else if (IsVdsSelectionClearedEvent(message))
            {
                SelectTrafficControl(null);
            }

        }

        private Task HighlightSelectedVdsControlFromMessage(string message)
        {
            string? vdsId = JsonNode.Parse(message)?["id"]?.GetValue<string>();
            if (string.IsNullOrWhiteSpace(vdsId))
            {
                SelectTrafficControl(null);
                return Task.CompletedTask;
            }

            if (_controlMap.TryGetValue(vdsId, out HighwayListControl? control))
            {
                SelectTrafficControl(control);
                _searchSummaryDetailLabel.Text = $"선택된 VDS: {vdsId}";
            }
            else
            {
                SelectTrafficControl(null);
            }

            return Task.CompletedTask;
        }

        private bool IsVdsSelectedEvent(string message)
        {
            try
            {
                var node = JsonNode.Parse(message)?["type"];
                if (node == null)
                    return false;
                var type = node.GetValue<string>();
                type.Trim("\"");
                return type.Equals(VdsMarkerSelectedEventFlag, StringComparison.Ordinal);
            }catch(Exception e)
            {
                Debug.WriteLine(e.Message);
                return false;
            }
        }

        private bool IsVdsSelectionClearedEvent(string message)
        {
            try
            {
                var node = JsonNode.Parse(message)?["type"];
                if (node == null)
                    return false;
                var type = node.GetValue<string>();
                type.Trim("\"");
                return type.Equals(VdsMarkerSelectionClearedEventFlag, StringComparison.Ordinal);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return false;
            }
        }

        private bool IsPosSelectedEvent(string message)
        {
            try
            {
                var node = JsonNode.Parse(message)?["type"];
                if (node == null)
                    return false;
                var type = node.GetValue<string>();
                type.Trim("\"");
                return type.Equals(PosSelectedEventFlag, StringComparison.Ordinal);
            }catch(Exception e)
            {
                Debug.WriteLine(e.Message);
                return false;
            }
        }

        private bool IsSelectionClearedEvent(string message)
        {
            try
            {
                string? type = JsonNode.Parse(message)?["type"]?.GetValue<string>();
                return string.Equals(type, SelectionClearedEventFlag, StringComparison.Ordinal);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);
                return false;
            }
        }

        private async Task UpdateSelectedPosTrafficInfoFromMessage(string message)
        {
            string normalized = NormalizeSelectionMessage(message);
            UpdateSelectedPosTrafficInfoCommand? data = JsonSerializer.Deserialize<UpdateSelectedPosTrafficInfoCommand>(normalized);

            if (data == null)
            {
                SetStatusMessage("조회 실패: 좌표 정보를 해석할 수 없습니다.", false);
                return;
            }

            _latestRoadSearchHighwayNames.Clear();
            await RunTrafficLookupAsync(data);
        }

        private async Task RunTrafficLookupAsync(
            UpdateSelectedPosTrafficInfoCommand command,
            IReadOnlyList<int>? selectedHighwayNumbers = null)
        {
            if (_requestTrafficByPosService == null)
            {
                SetStatusMessage("혼잡도 조회 서비스가 초기화되지 않았습니다.", false);
                return;
            }

            if (_isTrafficLookupInProgress)
            {
                SetStatusMessage("이미 혼잡도 조회 중입니다. 잠시만 기다려주세요.", true);
                return;
            }

            int requestVersion = System.Threading.Interlocked.Increment(ref _trafficLookupRequestVersion);
            _isTrafficLookupInProgress = true;
            SetLookupUiBusy(true);
            SetStatusMessage(
                selectedHighwayNumbers == null
                    ? "좌표를 확인했습니다. 주변 고속도로를 조회 중입니다..."
                    : "검색 결과 고속도로의 혼잡도를 조회 중입니다...",
                true);

            try
            {
                Dictionary<int, List<VdsTrafficResult>> trafficByHighway = selectedHighwayNumbers == null
                    ? await _requestTrafficByPosService.GetAdjacentHighWays(command)
                    : await _requestTrafficByPosService.GetTrafficByHighwaysAsync(selectedHighwayNumbers, command);

                if (selectedHighwayNumbers != null)
                {
                    ApplySelectedHighwayNames(trafficByHighway);
                }

                CacheTrafficLookupContext(trafficByHighway, command);
                List<VdsTrafficResult> results = trafficByHighway.Values.SelectMany(items => items).ToList();

                if (requestVersion != _trafficLookupRequestVersion || _rightPanelMode != RightPanelMode.Traffic)
                {
                    return;
                }

                CacheLatestTrafficResults(results);
                SetStatusMessage("지도와 목록을 업데이트하는 중입니다...", true);
                await ShowHighwayPanel(_latestTrafficResults);
                SetStatusMessage($"조회 완료: {results.Count}건 VDS 정보를 표시했습니다.", false);
            }
            catch (Exception exception)
            {
                SetStatusMessage($"조회 실패: {exception.Message}", false);
                Debug.WriteLine(exception.Message);
            }
            finally
            {
                _isTrafficLookupInProgress = false;
                SetLookupUiBusy(false);
            }
        }

        private void CacheTrafficLookupContext(
            Dictionary<int, List<VdsTrafficResult>> trafficByHighway,
            UpdateSelectedPosTrafficInfoCommand command)
        {
            _latestTrafficHighwayNumbers.Clear();
            _latestVdsHighwayNumberById.Clear();

            foreach ((int highwayNo, List<VdsTrafficResult> trafficResults) in trafficByHighway.OrderBy(item => item.Key))
            {
                _latestTrafficHighwayNumbers.Add(highwayNo);

                foreach (VdsTrafficResult trafficResult in trafficResults)
                {
                    _latestVdsHighwayNumberById[trafficResult.VdsId] = highwayNo;
                }
            }

            _latestTrafficSelectionCommand = new UpdateSelectedPosTrafficInfoCommand(command.Latitude, command.Longitude)
            {
                MinLongitude = command.MinLongitude,
                MinLatitude = command.MinLatitude,
                MaxLongitude = command.MaxLongitude,
                MaxLatitude = command.MaxLatitude
            };
        }

        private void ClearTrafficLookupContext()
        {
            _latestTrafficHighwayNumbers.Clear();
            _latestVdsHighwayNumberById.Clear();
            _selectedTrafficVdsId = null;
            _latestTrafficSelectionCommand = null;
            CacheLatestTrafficResults(Array.Empty<VdsTrafficResult>());
        }

        private void ApplySelectedHighwayNames(Dictionary<int, List<VdsTrafficResult>> trafficByHighway)
        {
            foreach ((int highwayNo, List<VdsTrafficResult> trafficResults) in trafficByHighway)
            {
                if (!_latestRoadSearchHighwayNames.TryGetValue(highwayNo, out string? highwayName))
                {
                    continue;
                }

                foreach (VdsTrafficResult trafficResult in trafficResults)
                {
                    trafficResult.Location.Name = highwayName;
                }
            }
        }

        private static UpdateSelectedPosTrafficInfoCommand CreateTrafficLookupCommand(MapViewSnapshot snapshot)
        {
            return new UpdateSelectedPosTrafficInfoCommand(snapshot.Latitude, snapshot.Longitude)
            {
                MinLongitude = snapshot.MinLongitude,
                MinLatitude = snapshot.MinLatitude,
                MaxLongitude = snapshot.MaxLongitude,
                MaxLatitude = snapshot.MaxLatitude
            };
        }

        private static UpdateSelectedPosCctvInfoCommand CreateCctvLookupCommand(MapViewSnapshot snapshot)
        {
            return new UpdateSelectedPosCctvInfoCommand(snapshot.Latitude, snapshot.Longitude)
            {
                MinLongitude = snapshot.MinLongitude,
                MinLatitude = snapshot.MinLatitude,
                MaxLongitude = snapshot.MaxLongitude,
                MaxLatitude = snapshot.MaxLatitude
            };
        }

        private string? FindVdsIdByControl(HighwayListControl control)
        {
            foreach ((string vdsId, HighwayListControl mappedControl) in _controlMap)
            {
                if (ReferenceEquals(mappedControl, control))
                {
                    return vdsId;
                }
            }

            return null;
        }

        private void DumpControls(Control parent, int depth = 0)
        {
            string indent = new string(' ', depth * 2);
            Debug.WriteLine($"{indent}- Name={parent.Name}, Type={parent.GetType().Name}, Visible={parent.Visible}, Size={parent.Size}, Dock={parent.Dock}");

            foreach (Control child in parent.Controls)
            {
                DumpControls(child, depth + 1);
            }
        }
    }
}
