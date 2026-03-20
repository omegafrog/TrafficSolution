using TrafficForm.Domain;

namespace TrafficForm
{
    public sealed class CctvListControl : UserControl
    {
        private static readonly Color HighlightBackgroundColor = Color.FromArgb(235, 247, 236);
        private static readonly Color IdleBackgroundColor = Color.FromArgb(255, 255, 255);
        private static readonly Color HighlightBorderColor = Color.FromArgb(37, 117, 40);
        private static readonly Color IdleBorderColor = Color.FromArgb(171, 184, 201);

        private readonly Panel _cardPanel = new Panel();
        private readonly Panel _highlightPanel = new Panel();
        private readonly TableLayoutPanel _layout = new TableLayoutPanel();
        private readonly Label _titleLabel = new Label();
        private readonly Label _locationLabel = new Label();
        private readonly Label _streamMetaLabel = new Label();

        private CctvInfo _cctvInfo;
        private bool _highlighted;

        public event EventHandler<CctvCardClickedEventArgs>? CardClicked;

        public CctvListControl(CctvInfo cctvInfo)
        {
            _cctvInfo = cctvInfo;

            BuildUi();
            Bind(cctvInfo);
            RegisterClickHandlers(_cardPanel);
            SetHighlighted(false);
        }

        internal string CctvId => _cctvInfo.CctvId;

        internal void Highlight()
        {
            SetHighlighted(true);
        }

        internal void ClearHighlight()
        {
            SetHighlighted(false);
        }

        internal void Bind(CctvInfo cctvInfo)
        {
            _cctvInfo = cctvInfo;

            string title = string.IsNullOrWhiteSpace(cctvInfo.Name)
                ? cctvInfo.CctvId
                : cctvInfo.Name;

            _titleLabel.Text = title;
            _locationLabel.Text = $"위치: {cctvInfo.Location.Latitude:F5}, {cctvInfo.Location.Longitude:F5}";

            string format = string.IsNullOrWhiteSpace(cctvInfo.Format) ? "Unknown" : cctvInfo.Format;
            string resolution = string.IsNullOrWhiteSpace(cctvInfo.Resolution) ? "-" : cctvInfo.Resolution;
            _streamMetaLabel.Text = $"형식: {format} / 해상도: {resolution}";
        }

        private void BuildUi()
        {
            SuspendLayout();

            Dock = DockStyle.Top;
            AutoSize = false;
            Size = new Size(220, 104);
            MinimumSize = new Size(140, 104);
            Height = 104;
            Margin = new Padding(0, 0, 0, 10);
            Padding = new Padding(5);

            _cardPanel.Dock = DockStyle.Fill;
            _cardPanel.BorderStyle = BorderStyle.None;
            _cardPanel.BackColor = IdleBackgroundColor;
            _cardPanel.Paint += CardPanel_Paint;

            _highlightPanel.Dock = DockStyle.Left;
            _highlightPanel.Width = 5;
            _highlightPanel.Visible = false;

            _layout.Dock = DockStyle.Fill;
            _layout.ColumnCount = 1;
            _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _layout.RowCount = 3;
            _layout.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));
            _layout.RowStyles.Add(new RowStyle(SizeType.Percent, 30F));
            _layout.RowStyles.Add(new RowStyle(SizeType.Percent, 30F));
            _layout.Padding = new Padding(8, 6, 8, 6);

            _titleLabel.AutoSize = true;
            _titleLabel.Dock = DockStyle.Fill;
            _titleLabel.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
            _titleLabel.ForeColor = Color.FromArgb(25, 34, 47);
            _titleLabel.Margin = Padding.Empty;

            _locationLabel.AutoSize = true;
            _locationLabel.Dock = DockStyle.Fill;
            _locationLabel.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular, GraphicsUnit.Point);
            _locationLabel.ForeColor = Color.FromArgb(52, 64, 82);
            _locationLabel.Margin = new Padding(0, 4, 0, 0);

            _streamMetaLabel.AutoSize = true;
            _streamMetaLabel.Dock = DockStyle.Fill;
            _streamMetaLabel.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular, GraphicsUnit.Point);
            _streamMetaLabel.ForeColor = Color.FromArgb(74, 87, 105);
            _streamMetaLabel.Margin = new Padding(0, 2, 0, 0);

            _layout.Controls.Add(_titleLabel, 0, 0);
            _layout.Controls.Add(_locationLabel, 0, 1);
            _layout.Controls.Add(_streamMetaLabel, 0, 2);

            _cardPanel.Controls.Add(_layout);
            _cardPanel.Controls.Add(_highlightPanel);

            Controls.Add(_cardPanel);

            ResumeLayout(false);
        }

        private void RegisterClickHandlers(Control control)
        {
            control.Click += CardControl_Click;

            foreach (Control child in control.Controls)
            {
                RegisterClickHandlers(child);
            }
        }

        private void CardControl_Click(object? sender, EventArgs e)
        {
            CardClicked?.Invoke(
                this,
                new CctvCardClickedEventArgs(_cctvInfo.CctvId, _titleLabel.Text, _cctvInfo.StreamUrl));
        }

        private void SetHighlighted(bool highlighted)
        {
            _highlighted = highlighted;
            _highlightPanel.Visible = highlighted;
            _cardPanel.BackColor = highlighted ? HighlightBackgroundColor : IdleBackgroundColor;
            _cardPanel.Invalidate();
        }

        private void CardPanel_Paint(object? sender, PaintEventArgs e)
        {
            Color borderColor = _highlighted ? HighlightBorderColor : IdleBorderColor;
            float borderWidth = _highlighted ? 2f : 1f;
            using Pen pen = new Pen(borderColor, borderWidth);
            Rectangle rectangle = new Rectangle(0, 0, _cardPanel.ClientSize.Width - 1, _cardPanel.ClientSize.Height - 1);
            e.Graphics.DrawRectangle(pen, rectangle);
        }
    }
}
