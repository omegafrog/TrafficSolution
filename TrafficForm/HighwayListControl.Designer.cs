namespace TrafficForm
{
    partial class HighwayListControl
    {
        /// <summary> 
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 구성 요소 디자이너에서 생성한 코드

        /// <summary> 
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            panel1 = new Panel();
            tableLayoutPanel1 = new TableLayoutPanel();
            pictureBox1 = new PictureBox();
            panel2 = new Panel();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.BorderStyle = BorderStyle.FixedSingle;
            panel1.Controls.Add(panel2);
            panel1.Controls.Add(tableLayoutPanel1);
            panel1.Controls.Add(pictureBox1);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(5, 5);
            panel1.Name = "panel1";
            panel1.Size = new Size(270, 84);
            panel1.TabIndex = 0;
            panel1.Paint += panel1_Paint;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.Dock = DockStyle.Right;
            tableLayoutPanel1.Location = new Point(97, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 48.2142868F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 51.7857132F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            tableLayoutPanel1.Size = new Size(171, 82);
            tableLayoutPanel1.TabIndex = 1;
            // 
            // pictureBox1
            // 
            pictureBox1.Dock = DockStyle.Left;
            pictureBox1.Location = new Point(0, 0);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(91, 82);
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            // 
            // panel2
            // 
            panel2.BackColor = SystemColors.ControlLight;
            panel2.Dock = DockStyle.Left;
            panel2.Location = new Point(91, 0);
            panel2.Name = "panel2";
            panel2.Size = new Size(5, 82);
            panel2.TabIndex = 2;
            panel2.Visible = false;
            // 
            // HighwayListControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(panel1);
            Name = "HighwayListControl";
            Padding = new Padding(5);
            Size = new Size(280, 94);
            panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private PictureBox pictureBox1;
        private TableLayoutPanel tableLayoutPanel1;
        private Panel panel2;
    }
}
