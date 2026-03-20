using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using TrafficForm.App;
using TrafficForm.Domain;
using TrafficForm.UI;

namespace TrafficForm
{
    public partial class Form1
    {
        private enum RightPanelMode
        {
            Traffic,
            Cctv
        }

        private const string CctvMarkerSelectedEventFlag = "cctv-selected";
        private const string TrafficPanelModeText = "혼잡도 모드";
        private const string CctvPanelModeText = "CCTV 모드";
        private const int RightPanelModeCardMinHeight = 118;

        private readonly RequestCctvByPosService? _requestCctvByPosService;
        private readonly Dictionary<string, CctvListControl> _cctvControlMap = new Dictionary<string, CctvListControl>(StringComparer.Ordinal);
        private readonly List<VdsTrafficResult> _latestTrafficResults = new List<VdsTrafficResult>();
        private readonly List<CctvInfo> _latestCctvResults = new List<CctvInfo>();

        private readonly Panel _rightPanelHeaderPanel = new Panel();
        private readonly TableLayoutPanel _rightPanelModeLayout = new TableLayoutPanel();
        private readonly Label _rightPanelModeTitleLabel = new Label();
        private readonly Label _rightPanelModeHintLabel = new Label();
        private readonly Label _rightPanelModeLabel = new Label();
        private readonly ComboBox _rightPanelModeComboBox = new ComboBox();

        private RightPanelMode _rightPanelMode = RightPanelMode.Traffic;
        private bool _isCctvLookupInProgress;
        private int _cctvLookupRequestVersion;
        private bool _isCctvPopupOpen;
        private CctvListControl? _selectedCctvControl;

        private void InitializeRightPanelModeUi()
        {
            _rightPanelHeaderPanel.SuspendLayout();
            filterPanel.SuspendLayout();

            filterPanel.FlowDirection = FlowDirection.TopDown;
            filterPanel.WrapContents = false;
            filterPanel.AutoScroll = true;
            filterPanel.Padding = new Padding(8);
            filterPanel.BackColor = Color.FromArgb(246, 247, 249);
            filterPanel.Controls.Clear();
            filterPanel.SizeChanged -= FilterPanel_SizeChanged;
            filterPanel.SizeChanged += FilterPanel_SizeChanged;

            _rightPanelHeaderPanel.Dock = DockStyle.None;
            _rightPanelHeaderPanel.Height = RightPanelModeCardMinHeight;
            _rightPanelHeaderPanel.Margin = new Padding(0, 0, 0, 10);
            _rightPanelHeaderPanel.Padding = new Padding(10);
            _rightPanelHeaderPanel.BackColor = Color.White;
            _rightPanelHeaderPanel.BorderStyle = BorderStyle.FixedSingle;

            _rightPanelModeLayout.Dock = DockStyle.Fill;
            _rightPanelModeLayout.ColumnCount = 1;
            _rightPanelModeLayout.ColumnStyles.Clear();
            _rightPanelModeLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _rightPanelModeLayout.RowCount = 4;
            _rightPanelModeLayout.RowStyles.Clear();
            _rightPanelModeLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _rightPanelModeLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _rightPanelModeLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _rightPanelModeLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _rightPanelModeLayout.BackColor = Color.Transparent;

            _rightPanelModeTitleLabel.AutoSize = true;
            _rightPanelModeTitleLabel.Text = "조회 패널 모드";
            _rightPanelModeTitleLabel.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
            _rightPanelModeTitleLabel.ForeColor = Color.FromArgb(33, 37, 41);
            _rightPanelModeTitleLabel.Margin = Padding.Empty;

            _rightPanelModeHintLabel.AutoSize = true;
            _rightPanelModeHintLabel.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular, GraphicsUnit.Point);
            _rightPanelModeHintLabel.ForeColor = Color.FromArgb(96, 103, 112);
            _rightPanelModeHintLabel.Margin = new Padding(0, 4, 0, 6);

            _rightPanelModeLabel.AutoSize = true;
            _rightPanelModeLabel.Text = "표시 유형";
            _rightPanelModeLabel.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular, GraphicsUnit.Point);
            _rightPanelModeLabel.ForeColor = Color.FromArgb(61, 67, 74);
            _rightPanelModeLabel.Margin = Padding.Empty;

            _rightPanelModeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            _rightPanelModeComboBox.Dock = DockStyle.Top;
            _rightPanelModeComboBox.Height = 30;
            _rightPanelModeComboBox.Margin = new Padding(0, 4, 0, 0);

            _rightPanelModeComboBox.Items.Clear();
            _rightPanelModeComboBox.Items.Add(TrafficPanelModeText);
            _rightPanelModeComboBox.Items.Add(CctvPanelModeText);
            _rightPanelModeComboBox.SelectedIndexChanged -= RightPanelModeComboBox_SelectedIndexChanged;
            _rightPanelModeComboBox.SelectedIndexChanged += RightPanelModeComboBox_SelectedIndexChanged;
            _rightPanelModeComboBox.SelectedItem = TrafficPanelModeText;

            _rightPanelHeaderPanel.Controls.Clear();
            _rightPanelModeLayout.Controls.Clear();
            _rightPanelModeLayout.Controls.Add(_rightPanelModeTitleLabel, 0, 0);
            _rightPanelModeLayout.Controls.Add(_rightPanelModeHintLabel, 0, 1);
            _rightPanelModeLayout.Controls.Add(_rightPanelModeLabel, 0, 2);
            _rightPanelModeLayout.Controls.Add(_rightPanelModeComboBox, 0, 3);
            _rightPanelHeaderPanel.Controls.Add(_rightPanelModeLayout);

            UpdateRightPanelModeHint();

            if (!filterPanel.Controls.Contains(_rightPanelHeaderPanel))
            {
                filterPanel.Controls.Add(_rightPanelHeaderPanel);
            }

            UpdateLeftPanelModeLayout();

            filterPanel.ResumeLayout();
            _rightPanelHeaderPanel.ResumeLayout();
        }

        private void FilterPanel_SizeChanged(object? sender, EventArgs e)
        {
            UpdateLeftPanelModeLayout();
        }

        private void UpdateLeftPanelModeLayout()
        {
            int availableWidth = Math.Max(120, filterPanel.ClientSize.Width - filterPanel.Padding.Horizontal - 1);
            _rightPanelHeaderPanel.Width = availableWidth;

            int contentWidth = Math.Max(80, availableWidth - _rightPanelHeaderPanel.Padding.Horizontal - 2);
            _rightPanelModeLayout.MaximumSize = new Size(contentWidth, 0);
            _rightPanelModeHintLabel.MaximumSize = new Size(contentWidth, 0);

            _rightPanelModeLayout.PerformLayout();
            int preferredContentHeight = _rightPanelModeLayout.GetPreferredSize(new Size(contentWidth, 0)).Height;
            int preferredPanelHeight = preferredContentHeight + _rightPanelHeaderPanel.Padding.Vertical + 2;
            _rightPanelHeaderPanel.Height = Math.Max(RightPanelModeCardMinHeight, preferredPanelHeight);
        }

        private void UpdateRightPanelModeHint()
        {
            _rightPanelModeHintLabel.Text = _rightPanelMode == RightPanelMode.Cctv
                ? "좌표 선택 시 CCTV 마커/카드를 표시합니다."
                : "좌표 선택 시 혼잡도(VDS) 목록을 표시합니다.";

            if (_rightPanelHeaderPanel.IsHandleCreated)
            {
                UpdateLeftPanelModeLayout();
            }
        }

        private async void RightPanelModeComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            RightPanelMode nextMode = string.Equals(
                _rightPanelModeComboBox.SelectedItem as string,
                CctvPanelModeText,
                StringComparison.Ordinal)
                ? RightPanelMode.Cctv
                : RightPanelMode.Traffic;

            await SetRightPanelModeAsync(nextMode);
        }

        private async Task SetRightPanelModeAsync(RightPanelMode nextMode)
        {
            if (_rightPanelMode == nextMode)
            {
                return;
            }

            if (nextMode == RightPanelMode.Cctv)
            {
                System.Threading.Interlocked.Increment(ref _trafficLookupRequestVersion);
            }
            else
            {
                System.Threading.Interlocked.Increment(ref _cctvLookupRequestVersion);
            }

            _rightPanelMode = nextMode;
            UpdateRightPanelModeHint();
            await RenderActiveRightPanelModeAsync();

            if (_rightPanelMode == RightPanelMode.Cctv)
            {
                SetStatusMessage("패널 모드가 CCTV 모드로 전환되었습니다.", false);
            }
            else
            {
                SetStatusMessage("패널 모드가 혼잡도 모드로 전환되었습니다.", false);
            }
        }

        private async Task RenderActiveRightPanelModeAsync()
        {
            if (_rightPanelMode == RightPanelMode.Cctv)
            {
                await ShowCctvPanel(_latestCctvResults);
            }
            else
            {
                await ShowHighwayPanel(_latestTrafficResults);
            }
        }

        private string GetCurrentPanelModeDisplayText()
        {
            return _rightPanelMode == RightPanelMode.Cctv ? CctvPanelModeText : TrafficPanelModeText;
        }

        private static string NormalizeSelectionMessage(string message)
        {
            return message
                .Replace("lat", "Latitude", StringComparison.Ordinal)
                .Replace("lon", "Longitude", StringComparison.Ordinal)
                .Replace("minLon", "MinLongitude", StringComparison.Ordinal)
                .Replace("minLat", "MinLatitude", StringComparison.Ordinal)
                .Replace("maxLon", "MaxLongitude", StringComparison.Ordinal)
                .Replace("maxLat", "MaxLatitude", StringComparison.Ordinal);
        }

        private void CacheLatestTrafficResults(IEnumerable<VdsTrafficResult> results)
        {
            _latestTrafficResults.Clear();
            _latestTrafficResults.AddRange(results);
        }

        private void CacheLatestCctvResults(IEnumerable<CctvInfo> results)
        {
            _latestCctvResults.Clear();
            _latestCctvResults.AddRange(results);
        }

        private async Task UpdateSelectedPosCctvInfoFromMessage(string message)
        {
            if (_requestCctvByPosService == null)
            {
                SetStatusMessage("CCTV 조회 서비스가 초기화되지 않았습니다.", false);
                return;
            }

            string normalized = NormalizeSelectionMessage(message);
            UpdateSelectedPosCctvInfoCommand? data = JsonSerializer.Deserialize<UpdateSelectedPosCctvInfoCommand>(normalized);

            if (data == null)
            {
                SetStatusMessage("조회 실패: CCTV 좌표 정보를 해석할 수 없습니다.", false);
                return;
            }

            int requestVersion = System.Threading.Interlocked.Increment(ref _cctvLookupRequestVersion);

            _isCctvLookupInProgress = true;
            _mapInteractionModeComboBox.Enabled = false;
            _rightPanelModeComboBox.Enabled = false;
            SetStatusMessage("좌표를 확인했습니다. 주변 고속도로의 CCTV를 조회 중입니다...", true);

            try
            {
                HighwayCctvSelection selection = await _requestCctvByPosService.GetNearbyHighwayCctv(data);

                if (requestVersion != _cctvLookupRequestVersion || _rightPanelMode != RightPanelMode.Cctv)
                {
                    return;
                }

                CacheLatestCctvResults(selection.CctvInfos);

                SetStatusMessage($"{selection.HighwayName} CCTV를 지도와 목록에 표시 중입니다...", true);
                await ShowCctvPanel(_latestCctvResults);
                SetStatusMessage($"조회 완료: {selection.HighwayName} CCTV {selection.CctvInfos.Count}건을 표시했습니다.", false);
            }
            catch (Exception exception)
            {
                SetStatusMessage($"CCTV 조회 실패: {exception.Message}", false);
                Debug.WriteLine(exception.Message);
            }
            finally
            {
                _isCctvLookupInProgress = false;
                _mapInteractionModeComboBox.Enabled = true;
                _rightPanelModeComboBox.Enabled = true;
            }
        }

        private async Task ShowCctvPanel(List<CctvInfo> results)
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

            HashSet<string> renderedCctvIds = new HashSet<string>(StringComparer.Ordinal);
            flowLayoutPanel1.SuspendLayout();

            foreach (CctvInfo result in results)
            {
                if (!renderedCctvIds.Add(result.CctvId))
                {
                    continue;
                }

                CctvListControl control = new CctvListControl(result);
                control.CardClicked += CctvListControl_CardClicked;

                flowLayoutPanel1.Controls.Add(control);
                _cctvControlMap[result.CctvId] = control;

                if (webView21.CoreWebView2 != null)
                {
                    string markerLatitude = result.Location.Latitude.ToString(CultureInfo.InvariantCulture);
                    string markerLongitude = result.Location.Longitude.ToString(CultureInfo.InvariantCulture);
                    string markerId = EscapeJavaScriptString(result.CctvId);
                    string markerText = EscapeJavaScriptString(result.Name);
                    await webView21.CoreWebView2.ExecuteScriptAsync($"addCctvMarker('{markerId}', {markerLatitude}, {markerLongitude}, '{markerText}')");
                }
            }

            highwaylistContainer.PerformLayout();
            flowLayoutPanel1.ResumeLayout();
            flowLayoutPanel1.PerformLayout();
            EnsureRightPanelVisible();
            ResizeRightPanelCards();
        }

        private async Task HighlightSelectedCctvControlFromMessage(string message)
        {
            string? cctvId = JsonNode.Parse(message)?["id"]?.GetValue<string>();
            if (!string.IsNullOrWhiteSpace(cctvId) && _cctvControlMap.TryGetValue(cctvId, out CctvListControl? control))
            {
                SelectCctvControl(control);
            }
            else
            {
                SelectCctvControl(null);
            }

            await Task.CompletedTask;
        }

        private bool IsCctvSelectedEvent(string message)
        {
            try
            {
                string? type = JsonNode.Parse(message)?["type"]?.GetValue<string>();
                return string.Equals(type, CctvMarkerSelectedEventFlag, StringComparison.Ordinal);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);
                return false;
            }
        }

        private void SelectTrafficControl(HighwayListControl? control)
        {
            if (ReferenceEquals(_selectedControl, control))
            {
                return;
            }

            _selectedControl?.ClearHighlight();
            _selectedControl = control;

            if (_selectedControl != null && flowLayoutPanel1.Controls.Contains(_selectedControl))
            {
                _selectedControl.Highlight();
                flowLayoutPanel1.ScrollControlIntoView(_selectedControl);
            }
        }

        private void SelectCctvControl(CctvListControl? control)
        {
            if (ReferenceEquals(_selectedCctvControl, control))
            {
                return;
            }

            _selectedCctvControl?.ClearHighlight();
            _selectedCctvControl = control;

            if (_selectedCctvControl != null && flowLayoutPanel1.Controls.Contains(_selectedCctvControl))
            {
                _selectedCctvControl.Highlight();
                flowLayoutPanel1.ScrollControlIntoView(_selectedCctvControl);
            }
        }

        private async void CctvListControl_CardClicked(object? sender, CctvCardClickedEventArgs eventArgs)
        {
            if (_cctvControlMap.TryGetValue(eventArgs.CctvId, out CctvListControl? selectedControl))
            {
                SelectCctvControl(selectedControl);
            }

            try
            {
                await FocusCctvMarkerOnMapAsync(eventArgs.CctvId);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);
            }

            if (!TryValidateCctvUrl(eventArgs.StreamUrl, out string validationMessage))
            {
                SetStatusMessage($"CCTV URL 검증 실패: {validationMessage}", false);
                return;
            }

            if (_isCctvPopupOpen)
            {
                SetStatusMessage("이미 CCTV 재생 창이 열려 있습니다.", false);
                return;
            }

            _isCctvPopupOpen = true;
            try
            {
                using CctvPlayerPopupForm popup = new CctvPlayerPopupForm(eventArgs.DisplayName, eventArgs.StreamUrl)
                {
                    StartPosition = FormStartPosition.CenterParent
                };

                popup.ShowDialog(this);
                SetStatusMessage($"CCTV 재생 창을 닫았습니다: {eventArgs.DisplayName}", false);
            }
            catch (Exception exception)
            {
                SetStatusMessage($"CCTV 재생 창 열기에 실패했습니다: {exception.Message}", false);
                Debug.WriteLine(exception.Message);
            }
            finally
            {
                _isCctvPopupOpen = false;
            }
        }

        private async Task FocusCctvMarkerOnMapAsync(string cctvId)
        {
            if (webView21.CoreWebView2 == null || string.IsNullOrWhiteSpace(cctvId))
            {
                return;
            }

            string escapedId = EscapeJavaScriptString(cctvId);
            await webView21.CoreWebView2.ExecuteScriptAsync($"highlightCctvMarker('{escapedId}')");
        }

        private static bool TryValidateCctvUrl(string cctvUrl, out string message)
        {
            if (string.IsNullOrWhiteSpace(cctvUrl))
            {
                message = "CCTV URL이 비어 있습니다.";
                return false;
            }

            if (!Uri.TryCreate(cctvUrl, UriKind.Absolute, out Uri? parsedUri))
            {
                message = "유효한 절대 URL 형식이 아닙니다.";
                return false;
            }

            if (!string.Equals(parsedUri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(parsedUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                message = "HTTP/HTTPS URL만 허용됩니다.";
                return false;
            }

            if (parsedUri.IsLoopback)
            {
                message = "로컬 호스트 URL은 허용되지 않습니다.";
                return false;
            }

            message = string.Empty;
            return true;
        }

        private static string EscapeJavaScriptString(string value)
        {
            return value
                .Replace("\\", "\\\\", StringComparison.Ordinal)
                .Replace("'", "\\'", StringComparison.Ordinal)
                .Replace("\r", "\\r", StringComparison.Ordinal)
                .Replace("\n", "\\n", StringComparison.Ordinal);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            foreach (HighwayListControl control in flowLayoutPanel1.Controls.OfType<HighwayListControl>())
            {
                control.ClearHighlight();
            }

            foreach (CctvListControl control in flowLayoutPanel1.Controls.OfType<CctvListControl>())
            {
                control.CardClicked -= CctvListControl_CardClicked;
            }

            base.OnFormClosed(e);
        }
    }
}
