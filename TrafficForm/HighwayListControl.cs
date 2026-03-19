using TrafficForm.Domain;

namespace TrafficForm
{
    public partial class HighwayListControl : UserControl
    {

        public HighwayListControl()
        {
            InitializeComponent();
        }

        public HighwayListControl(VdsTrafficResult result)
        {
            InitializeComponent();
            tableLayoutPanel1.Controls.Add(new Label() { Text = result.Location.Name}, 0,0);
            tableLayoutPanel1.Controls.Add(new Label() { Text = $"평균 속도:{result.Speed.ToString()}" }, 1,0);
            tableLayoutPanel1.Controls.Add(new Label() { Text = $"혼잡도:{TrafficLevelPolicy.CalculateTrafficLevel(result).ToDisplayString()}" }, 2,0);

        }

        private void label3_Click_1(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
        private void label2_Click_1(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        internal void Highlight()
        {
            panel2.Visible = true;

        }

        internal void ClearHighlight()
        {
            panel2.Visible = false;
        }
    }
}
