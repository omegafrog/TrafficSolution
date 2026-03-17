using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using TrafficForm.Domain;

namespace TrafficForm
{
    public partial class HighwayListControl : UserControl
    {

        private bool _highlited;
        public HighwayListControl()
        {
            InitializeComponent();
        }

        public HighwayListControl(VdsTrafficResult result)
        {
            InitializeComponent();
            tableLayoutPanel1.Controls.Add(new Label() { Text=result.Location.Name}, 0,0);
            tableLayoutPanel1.Controls.Add(new Label() { Text = result.Speed.ToString() }, 1,0);
            tableLayoutPanel1.Controls.Add(new Label() { Text = result.Occupancy.ToString() }, 2,0);

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
            _highlited = true;
            panel2.Visible = true;

        }
    }
}
