using Microsoft.Web.WebView2.WinForms;

namespace TrafficForm
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            InitializeWebView();
            TrafficListControl1 control = new TrafficListControl1();
            control.SetData("test");
            filterPanel.Controls.Add(control);
            list펴기ToolStripMenuItem.Click += (s, e) => ShowHighwayPanel();
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
            LoadMapHtml();
        }
        private void LoadMapHtml()
        {
            string html = """
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

        private void ShowHighwayPanel()
        {
            if (detailPanelOpen) return;

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
    }
}
