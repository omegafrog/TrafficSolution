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
        private readonly RequestTrafficByPosService _requestTrafficByPosService;
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
        private const string DefaultMapModeText = "일반 모드";
        private const string NearbyHighwayLookupModeText = "주변 고속도로 선택 모드";

        private bool _isTrafficLookupInProgress;
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
            SetStatusMessage("모드를 선택하세요.", false);
        }

        public Form1(RequestTrafficByPosService requestTrafficByPosService)
        {
            InitializeComponent();
            _requestTrafficByPosService = requestTrafficByPosService;
            InitializeStatusStripUi();
            InitializeMapModeUi();
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
                SetStatusMessage("주변 고속도로 선택 모드입니다. 지도를 클릭하세요.", false);
            }
            else
            {
                SetStatusMessage("일반 모드입니다. 지도 클릭 조회가 비활성화되었습니다.", false);
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

            // 기존 마커 제거
            function clearMarkers() {
              custommarkers.forEach(m => map.removeLayer(m));
              custommarkers = [];
            }

            function clearSegments() {
              customsegments.forEach(s => map.removeLayer(s));
              customsegments = [];
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
            flowLayoutPanel1.Controls.Clear();
            _controlMap.Clear();
            await webView21.CoreWebView2.ExecuteScriptAsync("clearMarkers()");
            await webView21.CoreWebView2.ExecuteScriptAsync("clearSegments()");
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

                string markerLatitudeText = markerLatitude.ToString(CultureInfo.InvariantCulture);
                string markerLongitudeText = markerLongitude.ToString(CultureInfo.InvariantCulture);
                await webView21.CoreWebView2.ExecuteScriptAsync($"addMarker('{result.VdsId}' ,{markerLatitudeText}, {markerLongitudeText}, '{result.VdsId}')");

                if (result.ResponsibilitySegment.Count > 1)
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
            foreach (var control in controls)
            {
                control.Width = flowLayoutPanel1.ClientSize.Width - flowLayoutPanel1.Margin.Horizontal;
            }

            //highwaylistContainer.SplitterDistance = highwaylistContainer.Width - detailPanelWidth;
            detailPanelOpen = true;
            highwaylistContainer.Panel2Collapsed= false;
            
        }

        private void HideHighwayPanel()
        {
            if (!detailPanelOpen) return;

            //highwaylistContainer.SplitterDistance = 0;
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

                if (_isTrafficLookupInProgress)
                {
                    SetStatusMessage("이미 조회 중입니다. 잠시만 기다려주세요.", true);
                    return;
                }

                await UpdateSelectedPosTrafficInfoFromMessage(message);
            }else if (IsVdsSelectedEvent(message))
            {
                await HighlightSelectedVdsControlFromMessage(message);
            }

        }

        private Task HighlightSelectedVdsControlFromMessage(string message)
        {
            string? vdsId = JsonNode.Parse(message)?["id"]?.GetValue<string>();
            if (vdsId != null && _controlMap.TryGetValue(vdsId, out HighwayListControl? control))
            {
                control.Highlight();
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

        private async Task UpdateSelectedPosTrafficInfoFromMessage(string message)
        {
            message = message
                            .Replace("lat", "Latitude").Replace("lon", "Longitude")
                            .Replace("minLon", "MinLongitude")
                            .Replace("minLat", "MinLatitude")
                            .Replace("maxLon", "MaxLongitude")
                            .Replace("maxLat", "MaxLatitude");
            UpdateSelectedPosTrafficInfoCommand? data = System.Text.Json.JsonSerializer.Deserialize<UpdateSelectedPosTrafficInfoCommand>(message);

            if (data == null)
            {
                SetStatusMessage("조회 실패: 좌표 정보를 해석할 수 없습니다.", false);
                return;
            }

            _isTrafficLookupInProgress = true;
            _mapInteractionModeComboBox.Enabled = false;
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

                SetStatusMessage("지도와 목록을 업데이트하는 중입니다...", true);
                await ShowHighwayPanel(results);
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
