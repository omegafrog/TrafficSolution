using TrafficForm.Domain;

namespace TrafficForm
{
    public partial class HighwayListControl : UserControl
    {
        private static readonly Color IdleBackgroundColor = Color.White;
        private static readonly Color IdleAccentColor = Color.FromArgb(223, 229, 238);
        private static readonly Color IdleBorderColor = Color.FromArgb(207, 215, 227);
        private static readonly Color HighlightBackgroundColor = Color.FromArgb(255, 241, 220);
        private static readonly Color HighlightAccentColor = Color.FromArgb(226, 116, 34);
        private static readonly Color HighlightBorderColor = Color.FromArgb(206, 98, 18);

        private readonly Label _titleLabel = new Label();
        private readonly Label _speedLabel = new Label();
        private readonly Label _trafficLevelLabel = new Label();
        private bool _highlighted;

        public HighwayListControl()
        {
            InitializeComponent();
            InitializeLabelUi();
            ApplySelectionStyle();
        }

        public HighwayListControl(VdsTrafficResult result)
        {
            InitializeComponent();
            InitializeLabelUi();
            Bind(result);
            ApplySelectionStyle();
        }

        internal void Bind(VdsTrafficResult result)
        {
            _titleLabel.Text = result.Location.Name;
            _speedLabel.Text = $"평균 속도 {result.Speed:0.#}km/h";
            _trafficLevelLabel.Text = $"혼잡도 {TrafficLevelPolicy.CalculateTrafficLevel(result).ToDisplayString()}";
        }

        internal void SetHighlighted(bool highlighted)
        {
            if (_highlighted == highlighted)
            {
                if (highlighted)
                {
                    ScrollIntoView();
                }

                return;
            }

            _highlighted = highlighted;
            ApplySelectionStyle();

            if (highlighted)
            {
                ScrollIntoView();
            }
        }

        internal void Highlight()
        {
            SetHighlighted(true);
        }

        internal void ClearHighlight()
        {
            SetHighlighted(false);
        }

        private void InitializeLabelUi()
        {
            Margin = new Padding(0, 0, 0, 12);

            tableLayoutPanel1.Controls.Clear();

            _titleLabel.AutoSize = true;
            _titleLabel.Dock = DockStyle.Fill;
            _titleLabel.Margin = Padding.Empty;
            _titleLabel.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold, GraphicsUnit.Point);
            _titleLabel.ForeColor = Color.FromArgb(33, 40, 52);

            _speedLabel.AutoSize = true;
            _speedLabel.Dock = DockStyle.Fill;
            _speedLabel.Margin = new Padding(0, 4, 0, 0);
            _speedLabel.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            _speedLabel.ForeColor = Color.FromArgb(67, 76, 90);

            _trafficLevelLabel.AutoSize = true;
            _trafficLevelLabel.Dock = DockStyle.Fill;
            _trafficLevelLabel.Margin = new Padding(0, 2, 0, 0);
            _trafficLevelLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            _trafficLevelLabel.ForeColor = Color.FromArgb(45, 55, 72);

            tableLayoutPanel1.Controls.Add(_titleLabel, 0, 0);
            tableLayoutPanel1.Controls.Add(_speedLabel, 0, 1);
            tableLayoutPanel1.Controls.Add(_trafficLevelLabel, 0, 2);
        }

        private void ApplySelectionStyle()
        {
            panel1.BackColor = _highlighted ? HighlightBackgroundColor : IdleBackgroundColor;
            panel2.BackColor = _highlighted ? HighlightAccentColor : IdleAccentColor;
            panel1.Invalidate();
        }

        private void ScrollIntoView()
        {
            if (Parent is ScrollableControl scrollableParent)
            {
                scrollableParent.ScrollControlIntoView(this);
            }
        }

        private void label3_Click_1(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            Color borderColor = _highlighted ? HighlightBorderColor : IdleBorderColor;
            float borderWidth = _highlighted ? 2f : 1f;
            using Pen borderPen = new Pen(borderColor, borderWidth);
            Rectangle borderRectangle = new Rectangle(0, 0, panel1.ClientSize.Width - 1, panel1.ClientSize.Height - 1);
            e.Graphics.DrawRectangle(borderPen, borderRectangle);

        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
