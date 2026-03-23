using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using TrafficForm.App;
using TrafficForm.Domain;

namespace TrafficForm;

public partial class Form1
{
    private enum RightPanelContentMode
    {
        Results,
        HighwayFavorites,
        CoordinateFavorites
    }

    private readonly Panel _topFavoritesPanel = new Panel();
    private readonly FlowLayoutPanel _topFavoritesButtonLayout = new FlowLayoutPanel();
    private readonly Button _rightPanelToggleButton = new Button();
    private readonly Button _highwayFavoritesTabButton = new Button();
    private readonly Button _coordinateFavoritesTabButton = new Button();
    private readonly Button _resultsTabButton = new Button();

    private readonly Panel _favoritesPanel = new Panel();
    private readonly Panel _favoritesHeaderPanel = new Panel();
    private readonly Label _favoritesTitleLabel = new Label();
    private readonly Label _favoritesHintLabel = new Label();
    private readonly Button _saveFavoriteButton = new Button();
    private readonly Button _removeFavoriteButton = new Button();
    private readonly TreeView _highwayFavoritesTreeView = new TreeView();
    private readonly ListView _coordinateFavoritesListView = new ListView();

    private UserFavorites _favoritesSnapshot = new UserFavorites();
    private RightPanelContentMode _rightPanelContentMode = RightPanelContentMode.Results;
    private bool _toolboxFavoritesInitialized;
    private bool _favoritesPanelInitialized;
    private bool _rightPanelEdgeToggleInitialized;

    private void InitializeToolboxFavoritesTabs()
    {
        if (!_toolboxFavoritesInitialized)
        {
            _topFavoritesPanel.Dock = DockStyle.None;
            _topFavoritesPanel.Height = 214;
            _topFavoritesPanel.Padding = new Padding(8, 6, 8, 6);
            _topFavoritesPanel.Margin = new Padding(0, 0, 0, 10);
            _topFavoritesPanel.BackColor = Color.FromArgb(246, 247, 249);

            _topFavoritesButtonLayout.Dock = DockStyle.Fill;
            _topFavoritesButtonLayout.FlowDirection = FlowDirection.TopDown;
            _topFavoritesButtonLayout.WrapContents = false;
            _topFavoritesButtonLayout.AutoScroll = false;

            ConfigureTopBarButton(_resultsTabButton, "조회\n결과");
            _resultsTabButton.Click += (_, _) => SetRightPanelContentMode(RightPanelContentMode.Results);

            ConfigureTopBarButton(_highwayFavoritesTabButton, "고속도로\n즐겨찾기");
            _highwayFavoritesTabButton.Click += (_, _) => SetRightPanelContentMode(RightPanelContentMode.HighwayFavorites);

            ConfigureTopBarButton(_coordinateFavoritesTabButton, "좌표\n즐겨찾기");
            _coordinateFavoritesTabButton.Click += (_, _) => SetRightPanelContentMode(RightPanelContentMode.CoordinateFavorites);

            _topFavoritesButtonLayout.Controls.Add(_resultsTabButton);
            _topFavoritesButtonLayout.Controls.Add(_highwayFavoritesTabButton);
            _topFavoritesButtonLayout.Controls.Add(_coordinateFavoritesTabButton);
            _topFavoritesPanel.Controls.Add(_topFavoritesButtonLayout);

            _toolboxFavoritesInitialized = true;
        }

        EnsureLeftFavoritesPanelAttached();

        UpdateRightPanelContentTabChecks();
        UpdateRightPanelToggleButtonText();
    }

    private void EnsureLeftFavoritesPanelAttached()
    {
        if (!_toolboxFavoritesInitialized)
        {
            return;
        }

        if (!filterPanel.Controls.Contains(_topFavoritesPanel))
        {
            filterPanel.Controls.Add(_topFavoritesPanel);
        }

        filterPanel.Controls.SetChildIndex(_topFavoritesPanel, 0);
        UpdateLeftPanelTopFavoritesLayout();
    }

    private static void ConfigureTopBarButton(Button button, string text)
    {
        button.AutoSize = false;
        button.Size = new Size(132, 42);
        button.Margin = new Padding(0, 0, 0, 6);
        button.Text = text;
        button.TextAlign = ContentAlignment.MiddleCenter;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.BorderColor = Color.FromArgb(191, 199, 213);
        button.BackColor = Color.White;
        button.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular, GraphicsUnit.Point);
    }

    private void UpdateLeftPanelTopFavoritesLayout()
    {
        if (!_toolboxFavoritesInitialized)
        {
            return;
        }

        int availableWidth = Math.Max(120, filterPanel.ClientSize.Width - filterPanel.Padding.Horizontal - 1);
        _topFavoritesPanel.Width = availableWidth;

        int buttonWidth = Math.Max(96, availableWidth - _topFavoritesPanel.Padding.Horizontal - 2);
        int buttonHeight = 42;

        Button[] buttons = [_resultsTabButton, _highwayFavoritesTabButton, _coordinateFavoritesTabButton];
        int totalHeight = 0;

        foreach (Button button in buttons)
        {
            button.Width = buttonWidth;
            button.Height = buttonHeight;
            totalHeight += buttonHeight + button.Margin.Bottom;
        }

        int panelHeight = _topFavoritesPanel.Padding.Vertical + totalHeight;
        _topFavoritesPanel.Height = Math.Max(150, panelHeight);
    }

    private void InitializeRightEdgeToggleButton()
    {
        if (_rightPanelEdgeToggleInitialized)
        {
            return;
        }

        _rightPanelToggleButton.AutoSize = false;
        _rightPanelToggleButton.Size = new Size(24, 56);
        _rightPanelToggleButton.Margin = Padding.Empty;
        _rightPanelToggleButton.Padding = Padding.Empty;
        _rightPanelToggleButton.FlatStyle = FlatStyle.Flat;
        _rightPanelToggleButton.FlatAppearance.BorderSize = 1;
        _rightPanelToggleButton.FlatAppearance.BorderColor = Color.FromArgb(176, 186, 201);
        _rightPanelToggleButton.BackColor = Color.FromArgb(250, 252, 255);
        _rightPanelToggleButton.ForeColor = Color.FromArgb(56, 68, 88);
        _rightPanelToggleButton.Font = new Font("Segoe UI Emoji", 9F, FontStyle.Regular, GraphicsUnit.Point);
        _rightPanelToggleButton.TextAlign = ContentAlignment.MiddleCenter;
        _rightPanelToggleButton.Cursor = Cursors.Hand;
        _rightPanelToggleButton.TabStop = false;
        _rightPanelToggleButton.Click += (_, _) => ToggleRightPanelVisibilityFromToolbox();

        if (!highwaylistContainer.Panel1.Controls.Contains(_rightPanelToggleButton))
        {
            highwaylistContainer.Panel1.Controls.Add(_rightPanelToggleButton);
        }

        highwaylistContainer.Panel1.SizeChanged += (_, _) => UpdateRightEdgeToggleButtonBounds();
        highwaylistContainer.SizeChanged += (_, _) => UpdateRightEdgeToggleButtonBounds();

        _rightPanelEdgeToggleInitialized = true;
        UpdateRightPanelToggleButtonText();
        UpdateRightEdgeToggleButtonBounds();
    }

    private void UpdateRightEdgeToggleButtonBounds()
    {
        if (!_rightPanelEdgeToggleInitialized)
        {
            return;
        }

        int x = Math.Max(0, highwaylistContainer.Panel1.ClientSize.Width - _rightPanelToggleButton.Width);
        int y = Math.Max(8, (highwaylistContainer.Panel1.ClientSize.Height - _rightPanelToggleButton.Height) / 2);
        _rightPanelToggleButton.Location = new Point(x, y);
        _rightPanelToggleButton.BringToFront();
    }

    private void InitializeFavoritesPanelUi()
    {
        if (_favoritesPanelInitialized)
        {
            return;
        }

        _favoritesPanel.Dock = DockStyle.Fill;
        _favoritesPanel.BackColor = Color.FromArgb(243, 246, 251);
        _favoritesPanel.Padding = new Padding(12, 10, 12, 12);
        _favoritesPanel.Visible = false;

        _favoritesHeaderPanel.Dock = DockStyle.Top;
        _favoritesHeaderPanel.Height = 88;
        _favoritesHeaderPanel.Padding = new Padding(8);
        _favoritesHeaderPanel.BackColor = Color.White;
        _favoritesHeaderPanel.BorderStyle = BorderStyle.FixedSingle;

        _favoritesTitleLabel.AutoSize = true;
        _favoritesTitleLabel.Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point);
        _favoritesTitleLabel.ForeColor = Color.FromArgb(33, 40, 52);
        _favoritesTitleLabel.Location = new Point(10, 10);
        _favoritesTitleLabel.Text = "즐겨찾기";

        _favoritesHintLabel.AutoSize = true;
        _favoritesHintLabel.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular, GraphicsUnit.Point);
        _favoritesHintLabel.ForeColor = Color.FromArgb(80, 88, 104);
        _favoritesHintLabel.Location = new Point(10, 32);
        _favoritesHintLabel.Text = "탭을 선택해 즐겨찾기를 저장하고 사용할 수 있습니다.";

        _saveFavoriteButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _saveFavoriteButton.Size = new Size(132, 28);
        _saveFavoriteButton.Location = new Point(10, 50);
        _saveFavoriteButton.Click += async (_, _) => await SaveCurrentFavoriteAsync();

        _removeFavoriteButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _removeFavoriteButton.Size = new Size(110, 28);
        _removeFavoriteButton.Location = new Point(148, 50);
        _removeFavoriteButton.Text = "선택 삭제";
        _removeFavoriteButton.Click += async (_, _) => await RemoveSelectedFavoriteAsync();

        _favoritesHeaderPanel.Controls.Add(_favoritesTitleLabel);
        _favoritesHeaderPanel.Controls.Add(_favoritesHintLabel);
        _favoritesHeaderPanel.Controls.Add(_saveFavoriteButton);
        _favoritesHeaderPanel.Controls.Add(_removeFavoriteButton);

        _highwayFavoritesTreeView.Dock = DockStyle.Fill;
        _highwayFavoritesTreeView.HideSelection = false;
        _highwayFavoritesTreeView.AfterSelect += (_, _) => UpdateFavoriteActionButtons();
        _highwayFavoritesTreeView.NodeMouseDoubleClick += async (_, eventArgs) => await TryApplyHighwayFavoriteFromNodeAsync(eventArgs.Node);

        _coordinateFavoritesListView.Dock = DockStyle.Fill;
        _coordinateFavoritesListView.FullRowSelect = true;
        _coordinateFavoritesListView.View = View.Details;
        _coordinateFavoritesListView.Columns.Add("이름", 130);
        _coordinateFavoritesListView.Columns.Add("좌표", 150);
        _coordinateFavoritesListView.Columns.Add("줌", 50);
        _coordinateFavoritesListView.Columns.Add("저장 시각", 120);
        _coordinateFavoritesListView.ItemSelectionChanged += (_, _) => UpdateFavoriteActionButtons();
        _coordinateFavoritesListView.ItemActivate += async (_, _) => await TryApplyCoordinateFavoriteAsync();

        _favoritesPanel.Controls.Add(_highwayFavoritesTreeView);
        _favoritesPanel.Controls.Add(_coordinateFavoritesListView);
        _favoritesPanel.Controls.Add(_favoritesHeaderPanel);

        if (!highwaylistContainer.Panel2.Controls.Contains(_favoritesPanel))
        {
            highwaylistContainer.Panel2.Controls.Add(_favoritesPanel);
        }

        _favoritesPanelInitialized = true;
        UpdateFavoriteActionButtons();
    }

    private async Task LoadFavoritesFromStoreAsync()
    {
        if (_favoriteService == null)
        {
            return;
        }

        try
        {
            _favoritesSnapshot = await _favoriteService.GetFavoritesAsync();
            RenderFavoritesCurrentMode();
        }
        catch (Exception exception)
        {
            SetStatusMessage($"즐겨찾기 로드 실패: {exception.Message}", false);
        }
    }

    private void SetRightPanelContentMode(RightPanelContentMode mode)
    {
        _rightPanelContentMode = mode;

        if (mode == RightPanelContentMode.Results)
        {
            _favoritesPanel.Visible = false;
            _searchSummaryPanel.Visible = true;
            flowLayoutPanel1.Visible = true;
            detailPanelWidth = ReducedRightPanelWidth;
        }
        else
        {
            _favoritesPanel.Visible = true;
            _searchSummaryPanel.Visible = false;
            flowLayoutPanel1.Visible = false;
            RenderFavoritesCurrentMode();
            detailPanelOpen = true;
            detailPanelWidth = FixedRightPanelWidth;
            EnsureRightPanelVisible();
        }

        UpdateRightPanelContentTabChecks();
    }

    private void UpdateRightPanelContentTabChecks()
    {
        ApplyTopBarButtonActiveStyle(_resultsTabButton, _rightPanelContentMode == RightPanelContentMode.Results);
        ApplyTopBarButtonActiveStyle(_highwayFavoritesTabButton, _rightPanelContentMode == RightPanelContentMode.HighwayFavorites);
        ApplyTopBarButtonActiveStyle(_coordinateFavoritesTabButton, _rightPanelContentMode == RightPanelContentMode.CoordinateFavorites);
    }

    private static void ApplyTopBarButtonActiveStyle(Button button, bool isActive)
    {
        if (isActive)
        {
            button.BackColor = Color.FromArgb(225, 236, 255);
            button.FlatAppearance.BorderColor = Color.FromArgb(72, 118, 255);
            button.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold, GraphicsUnit.Point);
            return;
        }

        button.BackColor = Color.White;
        button.FlatAppearance.BorderColor = Color.FromArgb(191, 199, 213);
        button.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular, GraphicsUnit.Point);
    }

    private void UpdateRightPanelToggleButtonText()
    {
        if (highwaylistContainer.Panel2Collapsed)
        {
            _rightPanelToggleButton.Text = "⏴";
        }
        else
        {
            _rightPanelToggleButton.Text = "⏵";
        }

        UpdateRightEdgeToggleButtonBounds();
    }

    private void ToggleRightPanelVisibilityFromToolbox()
    {
        if (highwaylistContainer.Panel2Collapsed)
        {
            detailPanelOpen = true;
            if (_rightPanelContentMode != RightPanelContentMode.Results)
            {
                detailPanelWidth = FixedRightPanelWidth;
            }
            EnsureRightPanelVisible();
        }
        else
        {
            detailPanelOpen = false;
            highwaylistContainer.Panel2Collapsed = true;
            UpdateRightPanelToggleButtonText();
        }
    }

    private void RenderFavoritesCurrentMode()
    {
        if (!_favoritesPanelInitialized)
        {
            return;
        }

        if (_rightPanelContentMode == RightPanelContentMode.HighwayFavorites)
        {
            _favoritesTitleLabel.Text = "고속도로 즐겨찾기";
            _favoritesHintLabel.Text = "고속도로 번호 기준 트리에서 하위 즐겨찾기를 선택하면 해당 뷰와 결과를 복원합니다.";
            _saveFavoriteButton.Text = "현재 고속도로 저장";
            _highwayFavoritesTreeView.Visible = true;
            _coordinateFavoritesListView.Visible = false;
            RenderHighwayFavoritesTree();
        }
        else if (_rightPanelContentMode == RightPanelContentMode.CoordinateFavorites)
        {
            _favoritesTitleLabel.Text = "좌표 즐겨찾기";
            _favoritesHintLabel.Text = "이름을 지정해 현재 지도 좌표/뷰를 저장하고 더블클릭으로 즉시 이동합니다.";
            _saveFavoriteButton.Text = "현재 좌표 저장";
            _highwayFavoritesTreeView.Visible = false;
            _coordinateFavoritesListView.Visible = true;
            RenderCoordinateFavoritesList();
        }

        UpdateFavoriteActionButtons();
    }

    private void RenderHighwayFavoritesTree()
    {
        _highwayFavoritesTreeView.BeginUpdate();
        _highwayFavoritesTreeView.Nodes.Clear();

        IEnumerable<IGrouping<int, HighwayFavorite>> grouped = _favoritesSnapshot.HighwayFavorites
            .OrderBy(item => item.HighwayNo)
            .ThenBy(item => item.SavedAt)
            .GroupBy(item => item.HighwayNo);

        foreach (IGrouping<int, HighwayFavorite> group in grouped)
        {
            TreeNode parentNode = new TreeNode($"{group.Key}번 고속도로");

            foreach (HighwayFavorite favorite in group.OrderBy(item => item.SavedAt))
            {
                string coordinateText = $"{favorite.View.Latitude:F4}, {favorite.View.Longitude:F4}";
                TreeNode childNode = new TreeNode($"{favorite.Name} · {coordinateText}")
                {
                    Tag = favorite
                };
                parentNode.Nodes.Add(childNode);
            }

            parentNode.Expand();
            _highwayFavoritesTreeView.Nodes.Add(parentNode);
        }

        _highwayFavoritesTreeView.EndUpdate();
    }

    private void RenderCoordinateFavoritesList()
    {
        _coordinateFavoritesListView.BeginUpdate();
        _coordinateFavoritesListView.Items.Clear();

        foreach (CoordinateFavorite favorite in _favoritesSnapshot.CoordinateFavorites.OrderBy(item => item.Name, StringComparer.Ordinal))
        {
            ListViewItem item = new ListViewItem(favorite.Name)
            {
                Tag = favorite
            };

            item.SubItems.Add($"{favorite.View.Latitude:F4}, {favorite.View.Longitude:F4}");
            item.SubItems.Add(favorite.View.ZoomLevel.ToString(CultureInfo.InvariantCulture));
            item.SubItems.Add(favorite.SavedAt.LocalDateTime.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture));

            _coordinateFavoritesListView.Items.Add(item);
        }

        _coordinateFavoritesListView.EndUpdate();
    }

    private void UpdateFavoriteActionButtons()
    {
        if (_rightPanelContentMode == RightPanelContentMode.HighwayFavorites)
        {
            _removeFavoriteButton.Enabled =
                _highwayFavoritesTreeView.SelectedNode?.Tag is HighwayFavorite;
            return;
        }

        if (_rightPanelContentMode == RightPanelContentMode.CoordinateFavorites)
        {
            _removeFavoriteButton.Enabled =
                _coordinateFavoritesListView.SelectedItems.Count > 0
                && _coordinateFavoritesListView.SelectedItems[0].Tag is CoordinateFavorite;
            return;
        }

        _removeFavoriteButton.Enabled = false;
    }

    private async Task SaveCurrentFavoriteAsync()
    {
        if (_favoriteService == null)
        {
            SetStatusMessage("즐겨찾기 저장 서비스를 찾을 수 없습니다.", false);
            return;
        }

        try
        {
            if (_rightPanelContentMode == RightPanelContentMode.HighwayFavorites)
            {
                await SaveHighwayFavoriteAsync();
            }
            else if (_rightPanelContentMode == RightPanelContentMode.CoordinateFavorites)
            {
                await SaveCoordinateFavoriteAsync();
            }
        }
        catch (Exception exception)
        {
            SetStatusMessage($"즐겨찾기 저장 실패: {exception.Message}", false);
        }
    }

    private async Task SaveHighwayFavoriteAsync()
    {
        if (_favoriteService == null)
        {
            return;
        }

        int highwayNo = ResolveCurrentHighwayNoForFavorite();
        if (highwayNo <= 0)
        {
            SetStatusMessage("저장할 고속도로 번호를 찾지 못했습니다. 먼저 혼잡도 조회 후 항목을 선택해 주세요.", false);
            return;
        }

        MapViewSnapshot? snapshot = await CaptureCurrentMapViewSnapshotAsync();
        if (snapshot == null)
        {
            SetStatusMessage("현재 지도 뷰를 읽어올 수 없어 저장에 실패했습니다.", false);
            return;
        }

        if (_latestTrafficSelectionCommand != null)
        {
            snapshot.Latitude = _latestTrafficSelectionCommand.Latitude;
            snapshot.Longitude = _latestTrafficSelectionCommand.Longitude;
            snapshot.MinLongitude = _latestTrafficSelectionCommand.MinLongitude;
            snapshot.MinLatitude = _latestTrafficSelectionCommand.MinLatitude;
            snapshot.MaxLongitude = _latestTrafficSelectionCommand.MaxLongitude;
            snapshot.MaxLatitude = _latestTrafficSelectionCommand.MaxLatitude;
        }

        int sameHighwayCount = _favoritesSnapshot.HighwayFavorites.Count(item => item.HighwayNo == highwayNo);
        HighwayFavorite favorite = new HighwayFavorite
        {
            HighwayNo = highwayNo,
            Name = $"{highwayNo}번 즐겨찾기 {sameHighwayCount + 1}",
            View = snapshot,
            SavedAt = DateTimeOffset.UtcNow
        };

        await _favoriteService.AddHighwayFavoriteAsync(favorite);
        _favoritesSnapshot = await _favoriteService.GetFavoritesAsync();
        RenderFavoritesCurrentMode();
        SetStatusMessage($"{highwayNo}번 고속도로 즐겨찾기를 저장했습니다.", false);
    }

    private async Task SaveCoordinateFavoriteAsync()
    {
        if (_favoriteService == null)
        {
            return;
        }

        MapViewSnapshot? snapshot = await CaptureCurrentMapViewSnapshotAsync();
        if (snapshot == null)
        {
            SetStatusMessage("현재 지도 좌표를 가져오지 못했습니다.", false);
            return;
        }

        string defaultName = $"좌표 {DateTime.Now:MMdd-HHmm}";
        string? favoriteName = PromptForText("좌표 즐겨찾기 이름", "이름", defaultName);
        if (string.IsNullOrWhiteSpace(favoriteName))
        {
            SetStatusMessage("좌표 즐겨찾기 저장이 취소되었습니다.", false);
            return;
        }

        CoordinateFavorite favorite = new CoordinateFavorite
        {
            Name = favoriteName.Trim(),
            View = snapshot,
            SavedAt = DateTimeOffset.UtcNow
        };

        await _favoriteService.AddCoordinateFavoriteAsync(favorite);
        _favoritesSnapshot = await _favoriteService.GetFavoritesAsync();
        RenderFavoritesCurrentMode();
        SetStatusMessage($"좌표 즐겨찾기 '{favorite.Name}'를 저장했습니다.", false);
    }

    private int ResolveCurrentHighwayNoForFavorite()
    {
        if (!string.IsNullOrWhiteSpace(_selectedTrafficVdsId)
            && _latestVdsHighwayNumberById.TryGetValue(_selectedTrafficVdsId, out int selectedHighwayNo))
        {
            return selectedHighwayNo;
        }

        if (_latestTrafficHighwayNumbers.Count > 0)
        {
            return _latestTrafficHighwayNumbers[0];
        }

        return 0;
    }

    private async Task RemoveSelectedFavoriteAsync()
    {
        if (_favoriteService == null)
        {
            return;
        }

        try
        {
            if (_rightPanelContentMode == RightPanelContentMode.HighwayFavorites
                && _highwayFavoritesTreeView.SelectedNode?.Tag is HighwayFavorite highwayFavorite)
            {
                await _favoriteService.RemoveHighwayFavoriteAsync(highwayFavorite.FavoriteId);
                _favoritesSnapshot = await _favoriteService.GetFavoritesAsync();
                RenderFavoritesCurrentMode();
                SetStatusMessage("고속도로 즐겨찾기를 삭제했습니다.", false);
                return;
            }

            if (_rightPanelContentMode == RightPanelContentMode.CoordinateFavorites
                && _coordinateFavoritesListView.SelectedItems.Count > 0
                && _coordinateFavoritesListView.SelectedItems[0].Tag is CoordinateFavorite coordinateFavorite)
            {
                await _favoriteService.RemoveCoordinateFavoriteAsync(coordinateFavorite.FavoriteId);
                _favoritesSnapshot = await _favoriteService.GetFavoritesAsync();
                RenderFavoritesCurrentMode();
                SetStatusMessage("좌표 즐겨찾기를 삭제했습니다.", false);
            }
        }
        catch (Exception exception)
        {
            SetStatusMessage($"즐겨찾기 삭제 실패: {exception.Message}", false);
        }
    }

    private async Task TryApplyHighwayFavoriteFromNodeAsync(TreeNode? node)
    {
        if (node?.Tag is not HighwayFavorite favorite)
        {
            return;
        }

        await ApplyHighwayFavoriteAsync(favorite);
    }

    private async Task ApplyHighwayFavoriteAsync(HighwayFavorite favorite)
    {
        await ApplyMapViewSnapshotAsync(favorite.View);

        if (_requestTrafficByPosService == null)
        {
            SetStatusMessage("혼잡도 서비스를 사용할 수 없습니다. 지도 뷰만 이동했습니다.", false);
            return;
        }

        UpdateSelectedPosTrafficInfoCommand command = new UpdateSelectedPosTrafficInfoCommand(favorite.View.Latitude, favorite.View.Longitude)
        {
            MinLongitude = favorite.View.MinLongitude,
            MinLatitude = favorite.View.MinLatitude,
            MaxLongitude = favorite.View.MaxLongitude,
            MaxLatitude = favorite.View.MaxLatitude
        };

        _isTrafficLookupInProgress = true;
        _mapInteractionModeComboBox.Enabled = false;
        _rightPanelModeComboBox.Enabled = false;
        SetStatusMessage($"{favorite.HighwayNo}번 고속도로 즐겨찾기를 조회 중입니다...", true);

        try
        {
            Dictionary<int, List<VdsTrafficResult>> highways = await _requestTrafficByPosService.GetAdjacentHighWays(command);
            CacheTrafficLookupContext(highways, command);

            List<VdsTrafficResult> results = highways.TryGetValue(favorite.HighwayNo, out List<VdsTrafficResult>? selectedHighwayResults)
                ? selectedHighwayResults
                : highways.Values.SelectMany(items => items).ToList();

            CacheLatestTrafficResults(results);
            _rightPanelMode = RightPanelMode.Traffic;
            _rightPanelModeComboBox.SelectedItem = TrafficPanelModeText;
            await ShowHighwayPanel(results);
            SetStatusMessage($"즐겨찾기 적용 완료: {results.Count}건을 표시했습니다.", false);
        }
        catch (Exception exception)
        {
            SetStatusMessage($"즐겨찾기 조회 실패: {exception.Message}", false);
        }
        finally
        {
            _isTrafficLookupInProgress = false;
            _mapInteractionModeComboBox.Enabled = true;
            _rightPanelModeComboBox.Enabled = true;
            SetRightPanelContentMode(RightPanelContentMode.Results);
        }
    }

    private async Task TryApplyCoordinateFavoriteAsync()
    {
        if (_coordinateFavoritesListView.SelectedItems.Count == 0)
        {
            return;
        }

        if (_coordinateFavoritesListView.SelectedItems[0].Tag is not CoordinateFavorite favorite)
        {
            return;
        }

        await ApplyMapViewSnapshotAsync(favorite.View);
        SetStatusMessage($"좌표 즐겨찾기 '{favorite.Name}'로 이동했습니다.", false);
    }

    private async Task<MapViewSnapshot?> CaptureCurrentMapViewSnapshotAsync()
    {
        if (webView21.CoreWebView2 == null)
        {
            return null;
        }

        string rawScriptResult = await webView21.CoreWebView2.ExecuteScriptAsync("JSON.stringify(getMapViewState())");
        string? json = JsonSerializer.Deserialize<string>(rawScriptResult);
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        JsonNode? node = JsonNode.Parse(json);
        if (node == null)
        {
            return null;
        }

        return new MapViewSnapshot
        {
            Latitude = ReadDouble(node, "lat"),
            Longitude = ReadDouble(node, "lon"),
            ZoomLevel = (int)Math.Round(ReadDouble(node, "zoom"), MidpointRounding.AwayFromZero),
            MinLongitude = ReadDouble(node, "minLon"),
            MinLatitude = ReadDouble(node, "minLat"),
            MaxLongitude = ReadDouble(node, "maxLon"),
            MaxLatitude = ReadDouble(node, "maxLat")
        };
    }

    private static double ReadDouble(JsonNode node, string propertyName)
    {
        return node[propertyName]?.GetValue<double>() ?? 0;
    }

    private async Task ApplyMapViewSnapshotAsync(MapViewSnapshot snapshot)
    {
        if (webView21.CoreWebView2 == null)
        {
            return;
        }

        string lat = snapshot.Latitude.ToString(CultureInfo.InvariantCulture);
        string lon = snapshot.Longitude.ToString(CultureInfo.InvariantCulture);
        string zoom = snapshot.ZoomLevel.ToString(CultureInfo.InvariantCulture);
        string minLon = snapshot.MinLongitude.ToString(CultureInfo.InvariantCulture);
        string minLat = snapshot.MinLatitude.ToString(CultureInfo.InvariantCulture);
        string maxLon = snapshot.MaxLongitude.ToString(CultureInfo.InvariantCulture);
        string maxLat = snapshot.MaxLatitude.ToString(CultureInfo.InvariantCulture);

        await webView21.CoreWebView2.ExecuteScriptAsync($"setMapViewFromFavorite({lat}, {lon}, {zoom}, {minLon}, {minLat}, {maxLon}, {maxLat});");
    }

    private static string? PromptForText(string title, string labelText, string defaultValue)
    {
        using Form dialog = new Form
        {
            Text = title,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            StartPosition = FormStartPosition.CenterParent,
            MinimizeBox = false,
            MaximizeBox = false,
            ShowInTaskbar = false,
            Width = 380,
            Height = 160
        };

        Label label = new Label
        {
            Left = 12,
            Top = 14,
            Width = 340,
            Text = labelText
        };

        TextBox textBox = new TextBox
        {
            Left = 12,
            Top = 38,
            Width = 340,
            Text = defaultValue
        };

        Button okButton = new Button
        {
            Text = "확인",
            DialogResult = DialogResult.OK,
            Left = 196,
            Width = 75,
            Top = 76
        };

        Button cancelButton = new Button
        {
            Text = "취소",
            DialogResult = DialogResult.Cancel,
            Left = 277,
            Width = 75,
            Top = 76
        };

        dialog.Controls.Add(label);
        dialog.Controls.Add(textBox);
        dialog.Controls.Add(okButton);
        dialog.Controls.Add(cancelButton);
        dialog.AcceptButton = okButton;
        dialog.CancelButton = cancelButton;

        DialogResult result = dialog.ShowDialog();
        if (result != DialogResult.OK)
        {
            return null;
        }

        return textBox.Text;
    }
}
