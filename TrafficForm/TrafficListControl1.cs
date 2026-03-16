using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TrafficForm
{
    public partial class TrafficListControl1 : UserControl
    {
        public TrafficListControl1()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
        public void SetData(string name)
        {
            label1.Text = name;
        }
    }
}
