using Microsoft.Web.WebView2.Core;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using TrafficForm.App;
using TrafficForm.Domain;

namespace TrafficForm
{
    public partial class Form1 : Form
    {
        private readonly RequestTrafficByPosService? _requestTrafficByPosService;
        private readonly Dictionary<string, HighwayListControl> _controlMap = new Dictionary<string, HighwayListControl>();
        private HighwayListControl? _selectedControl;
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
        private const string DefaultMapModeText = "일반 모드";
        private const string NearbyHighwayLookupModeText = "주변 고속도로 선택 모드";

        private bool _isTrafficLookupInProgress;
        private int _trafficLookupRequestVersion;
        private MapInteractionMode _mapInteractionMode = MapInteractionMode.None;

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
            InitializeRightPanelModeUi();
            SetStatusMessage("모드를 선택하세요.", false);
        }

        public Form1(RequestTrafficByPosService requestTrafficByPosService, RequestCctvByPosService requestCctvByPosService)
        {
            InitializeComponent();
            _requestTrafficByPosService = requestTrafficByPosService;
            _requestCctvByPosService = requestCctvByPosService;
            InitializeStatusStripUi();
            InitializeMapModeUi();
            InitializeRightPanelModeUi();
            SetStatusMessage("모드를 선택하세요.", false);
            InitializeWebView();
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

                applyMapCursor();

                map.on('click', function(e) {
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

            List<HighwayListControl> controls = new List<HighwayListControl>();
            HashSet<string> renderedVdsIds = new HashSet<string>();
            Dictionary<string, int> markerOverlapCountByCoordinate = new Dictionary<string, int>();
            foreach (VdsTrafficResult result in results)
            {
                if (!renderedVdsIds.Add(result.VdsId))
                {
                    continue;
                }

                HighwayListControl control = new(result){};
                flowLayoutPanel1.Controls.Add(control);
                controls.Add(control);

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
            flowLayoutPanel1.PerformLayout();
            EnsureRightPanelVisible();

            foreach (var control in controls)
            {
                control.Width = GetRightPanelCardWidth();
            }

            //highwaylistContainer.SplitterDistance = highwaylistContainer.Width - detailPanelWidth;
        }

        private int GetRightPanelCardWidth()
        {
            int currentWidth = flowLayoutPanel1.ClientSize.Width;
            int horizontalChrome = flowLayoutPanel1.Margin.Horizontal + flowLayoutPanel1.Padding.Horizontal + 8;
            int calculatedWidth = currentWidth - horizontalChrome;
            return Math.Max(140, calculatedWidth);
        }

        private void EnsureRightPanelVisible()
        {
            if (highwaylistContainer.Panel2Collapsed)
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
        }

        private void HideHighwayPanel()
        {
            if (!detailPanelOpen) return;

            //highwaylistContainer.SplitterDistance = 0;
            SelectTrafficControl(null);
            SelectCctvControl(null);
            detailPanelOpen = false;
            highwaylistContainer.Panel2Collapsed = true;

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

        }

        private Task HighlightSelectedVdsControlFromMessage(string message)
        {
            string? vdsId = JsonNode.Parse(message)?["id"]?.GetValue<string>();
            if (!string.IsNullOrWhiteSpace(vdsId) && _controlMap.TryGetValue(vdsId, out HighwayListControl? control))
            {
                SelectTrafficControl(control);
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
            if (_requestTrafficByPosService == null)
            {
                SetStatusMessage("혼잡도 조회 서비스가 초기화되지 않았습니다.", false);
                return;
            }

            string normalized = NormalizeSelectionMessage(message);
            UpdateSelectedPosTrafficInfoCommand? data = JsonSerializer.Deserialize<UpdateSelectedPosTrafficInfoCommand>(normalized);

            if (data == null)
            {
                SetStatusMessage("조회 실패: 좌표 정보를 해석할 수 없습니다.", false);
                return;
            }

            int requestVersion = System.Threading.Interlocked.Increment(ref _trafficLookupRequestVersion);

            _isTrafficLookupInProgress = true;
            _mapInteractionModeComboBox.Enabled = false;
            _rightPanelModeComboBox.Enabled = false;
            SetStatusMessage("좌표를 확인했습니다. 주변 고속도로를 조회 중입니다...", true);

            try
            {
                List<VdsTrafficResult> results = new List<VdsTrafficResult>();
                Dictionary<int, List<VdsTrafficResult>> highWays = await _requestTrafficByPosService.GetAdjacentHighWays(data);
                SetStatusMessage("조회 결과를 정리 중입니다...", true);

                foreach (int highwayId in highWays.Keys)
                {
                    results.AddRange(highWays[highwayId]);
                }

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
                _mapInteractionModeComboBox.Enabled = true;
                _rightPanelModeComboBox.Enabled = true;
            }

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
