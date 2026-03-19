using Microsoft.Web.WebView2.Core;
using System.Diagnostics;
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
        private readonly string POS_SELECTED_EVENT_FLAG = "pos-selected";
        private readonly string VDS_MARKER_SELECTED_EVENT_FLAG = "vds-selected";
        public Form1(RequestTrafficByPosService requestTrafficByPosService)
        {
            InitializeComponent();
            InitializeWebView();
            //list펴기ToolStripMenuItem.Click += (s, e) => ShowHighwayPanel();
            list접기ToolStripMenuItem.Click += (s, e) => HideHighwayPanel();
            _requestTrafficByPosService = requestTrafficByPosService;
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

            //List<Location> locs = await _publicTrafficApi.findAllVdiLoc();
            List<Location> locs = new List<Location>();
            LoadMapHtml(locs);

            //foreach (Location location in locs) {

            //    await webView21.CoreWebView2.ExecuteScriptAsync(
            //        $"moveAndAddMarker({location.Latitude}, {location.Longitude}, '{location.Name}')");
            //}


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
                map.on('click', function(e) {
                const bounds = map.getBounds();
                const data = {
                    type: "{{POS_SELECTED_EVENT_FLAG}}",
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
                    type: "{{VDS_MARKER_SELECTED_EVENT_FLAG}}",
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

        private void ShowHighwayPanel(List<VdsTrafficResult> results)
        {
            flowLayoutPanel1.Controls.Clear();
            _controlMap.Clear();
            webView21.CoreWebView2.ExecuteScriptAsync($"clearMarkers()");
            webView21.CoreWebView2.ExecuteScriptAsync($"clearSegments()");
            List<HighwayListControl> controls = new List<HighwayListControl>();
            HashSet<string> renderedVdsIds = new HashSet<string>();
            foreach (VdsTrafficResult result in results)
            {
                if (!renderedVdsIds.Add(result.VdsId))
                {
                    continue;
                }

                HighwayListControl control = new(result){};
                flowLayoutPanel1.Controls.Add(control);
                controls.Add(control);
                webView21.CoreWebView2.ExecuteScriptAsync($"addMarker('{result.VdsId}' ,{result.Location.Latitude}, {result.Location.Longitude}, '{result.VdsId}')");

                if (result.ResponsibilitySegment.Count > 1)
                {
                    string segmentPointsJson = JsonSerializer.Serialize(result.ResponsibilitySegment.Select(point => new
                    {
                        latitude = point.Latitude,
                        longitude = point.Longitude
                    }));
                    string color = TrafficLevelPolicy.GetColorHex(result.TrafficLevel);
                    webView21.CoreWebView2.ExecuteScriptAsync($"addSegment({segmentPointsJson}, '{color}')");
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
                return type.Equals(VDS_MARKER_SELECTED_EVENT_FLAG);
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
                return type.Equals(POS_SELECTED_EVENT_FLAG);
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
            if (data != null)
            {
                List<VdsTrafficResult> results = new List<VdsTrafficResult>();
                Dictionary<int, List<VdsTrafficResult>> highWays = await _requestTrafficByPosService.GetAdjacentHighWays(data);
                foreach (int highwayId in highWays.Keys)
                {
                    results.AddRange(highWays[highwayId]);
                }
                ShowHighwayPanel(results);

            }
            else
            {
                throw new PosNotValidException("위도와 경도 정보가 유효하지 않습니다.");
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
