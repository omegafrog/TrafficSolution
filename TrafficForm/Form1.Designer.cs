namespace TrafficForm
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            menuStrip1 = new MenuStrip();
            파일ToolStripMenuItem = new ToolStripMenuItem();
            종료ToolStripMenuItem = new ToolStripMenuItem();
            보기ToolStripMenuItem = new ToolStripMenuItem();
            검색패널표시ToolStripMenuItem = new ToolStripMenuItem();
            상세정보패널표시ToolStripMenuItem = new ToolStripMenuItem();
            상태바표시ToolStripMenuItem = new ToolStripMenuItem();
            지도ToolStripMenuItem = new ToolStripMenuItem();
            현재위치로이동ToolStripMenuItem = new ToolStripMenuItem();
            하이라이트초기화ToolStripMenuItem = new ToolStripMenuItem();
            지도초기화ToolStripMenuItem = new ToolStripMenuItem();
            데이터ToolStripMenuItem = new ToolStripMenuItem();
            새로고침ToolStripMenuItem = new ToolStripMenuItem();
            고속도로조회ToolStripMenuItem = new ToolStripMenuItem();
            도움말ToolStripMenuItem = new ToolStripMenuItem();
            정보ToolStripMenuItem = new ToolStripMenuItem();
            디버그ToolStripMenuItem = new ToolStripMenuItem();
            list출력ToolStripMenuItem = new ToolStripMenuItem();
            list펴기ToolStripMenuItem = new ToolStripMenuItem();
            list접기ToolStripMenuItem = new ToolStripMenuItem();
            toolStrip1 = new ToolStrip();
            statusStrip1 = new StatusStrip();
            highwaylistContainer = new SplitContainer();
            splitContainer1 = new SplitContainer();
            filterPanel = new FlowLayoutPanel();
            label1 = new Label();
            webView21 = new Microsoft.Web.WebView2.WinForms.WebView2();
            menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)highwaylistContainer).BeginInit();
            highwaylistContainer.Panel1.SuspendLayout();
            highwaylistContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            filterPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)webView21).BeginInit();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { 파일ToolStripMenuItem, 보기ToolStripMenuItem, 지도ToolStripMenuItem, 데이터ToolStripMenuItem, 도움말ToolStripMenuItem, 디버그ToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(1211, 24);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // 파일ToolStripMenuItem
            // 
            파일ToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { 종료ToolStripMenuItem });
            파일ToolStripMenuItem.Name = "파일ToolStripMenuItem";
            파일ToolStripMenuItem.Size = new Size(43, 20);
            파일ToolStripMenuItem.Text = "파일";
            // 
            // 종료ToolStripMenuItem
            // 
            종료ToolStripMenuItem.Name = "종료ToolStripMenuItem";
            종료ToolStripMenuItem.Size = new Size(98, 22);
            종료ToolStripMenuItem.Text = "종료";
            // 
            // 보기ToolStripMenuItem
            // 
            보기ToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { 검색패널표시ToolStripMenuItem, 상세정보패널표시ToolStripMenuItem, 상태바표시ToolStripMenuItem });
            보기ToolStripMenuItem.Name = "보기ToolStripMenuItem";
            보기ToolStripMenuItem.Size = new Size(43, 20);
            보기ToolStripMenuItem.Text = "보기";
            // 
            // 검색패널표시ToolStripMenuItem
            // 
            검색패널표시ToolStripMenuItem.Name = "검색패널표시ToolStripMenuItem";
            검색패널표시ToolStripMenuItem.Size = new Size(182, 22);
            검색패널표시ToolStripMenuItem.Text = "검색 패널 표시";
            // 
            // 상세정보패널표시ToolStripMenuItem
            // 
            상세정보패널표시ToolStripMenuItem.Name = "상세정보패널표시ToolStripMenuItem";
            상세정보패널표시ToolStripMenuItem.Size = new Size(182, 22);
            상세정보패널표시ToolStripMenuItem.Text = "상세 정보 패널 표시";
            // 
            // 상태바표시ToolStripMenuItem
            // 
            상태바표시ToolStripMenuItem.Name = "상태바표시ToolStripMenuItem";
            상태바표시ToolStripMenuItem.Size = new Size(182, 22);
            상태바표시ToolStripMenuItem.Text = "상태바 표시";
            // 
            // 지도ToolStripMenuItem
            // 
            지도ToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { 현재위치로이동ToolStripMenuItem, 하이라이트초기화ToolStripMenuItem, 지도초기화ToolStripMenuItem });
            지도ToolStripMenuItem.Name = "지도ToolStripMenuItem";
            지도ToolStripMenuItem.Size = new Size(43, 20);
            지도ToolStripMenuItem.Text = "지도";
            // 
            // 현재위치로이동ToolStripMenuItem
            // 
            현재위치로이동ToolStripMenuItem.Name = "현재위치로이동ToolStripMenuItem";
            현재위치로이동ToolStripMenuItem.Size = new Size(174, 22);
            현재위치로이동ToolStripMenuItem.Text = "현재 위치로 이동";
            // 
            // 하이라이트초기화ToolStripMenuItem
            // 
            하이라이트초기화ToolStripMenuItem.Name = "하이라이트초기화ToolStripMenuItem";
            하이라이트초기화ToolStripMenuItem.Size = new Size(174, 22);
            하이라이트초기화ToolStripMenuItem.Text = "하이라이트 초기화";
            // 
            // 지도초기화ToolStripMenuItem
            // 
            지도초기화ToolStripMenuItem.Name = "지도초기화ToolStripMenuItem";
            지도초기화ToolStripMenuItem.Size = new Size(174, 22);
            지도초기화ToolStripMenuItem.Text = "지도 초기화";
            // 
            // 데이터ToolStripMenuItem
            // 
            데이터ToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { 새로고침ToolStripMenuItem, 고속도로조회ToolStripMenuItem });
            데이터ToolStripMenuItem.Name = "데이터ToolStripMenuItem";
            데이터ToolStripMenuItem.Size = new Size(55, 20);
            데이터ToolStripMenuItem.Text = "데이터";
            // 
            // 새로고침ToolStripMenuItem
            // 
            새로고침ToolStripMenuItem.Name = "새로고침ToolStripMenuItem";
            새로고침ToolStripMenuItem.Size = new Size(150, 22);
            새로고침ToolStripMenuItem.Text = "새로고침";
            // 
            // 고속도로조회ToolStripMenuItem
            // 
            고속도로조회ToolStripMenuItem.Name = "고속도로조회ToolStripMenuItem";
            고속도로조회ToolStripMenuItem.Size = new Size(150, 22);
            고속도로조회ToolStripMenuItem.Text = "고속도로 조회";
            // 
            // 도움말ToolStripMenuItem
            // 
            도움말ToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { 정보ToolStripMenuItem });
            도움말ToolStripMenuItem.Name = "도움말ToolStripMenuItem";
            도움말ToolStripMenuItem.Size = new Size(55, 20);
            도움말ToolStripMenuItem.Text = "도움말";
            // 
            // 정보ToolStripMenuItem
            // 
            정보ToolStripMenuItem.Name = "정보ToolStripMenuItem";
            정보ToolStripMenuItem.Size = new Size(98, 22);
            정보ToolStripMenuItem.Text = "정보";
            // 
            // 디버그ToolStripMenuItem
            // 
            디버그ToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { list출력ToolStripMenuItem });
            디버그ToolStripMenuItem.Name = "디버그ToolStripMenuItem";
            디버그ToolStripMenuItem.Size = new Size(55, 20);
            디버그ToolStripMenuItem.Text = "디버그";
            // 
            // list출력ToolStripMenuItem
            // 
            list출력ToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { list펴기ToolStripMenuItem, list접기ToolStripMenuItem });
            list출력ToolStripMenuItem.Name = "list출력ToolStripMenuItem";
            list출력ToolStripMenuItem.Size = new Size(138, 22);
            list출력ToolStripMenuItem.Text = "highway list";
            list출력ToolStripMenuItem.Click += list출력ToolStripMenuItem_Click;
            // 
            // list펴기ToolStripMenuItem
            // 
            list펴기ToolStripMenuItem.Name = "list펴기ToolStripMenuItem";
            list펴기ToolStripMenuItem.Size = new Size(117, 22);
            list펴기ToolStripMenuItem.Text = "list 펴기";
            // 
            // list접기ToolStripMenuItem
            // 
            list접기ToolStripMenuItem.Name = "list접기ToolStripMenuItem";
            list접기ToolStripMenuItem.Size = new Size(117, 22);
            list접기ToolStripMenuItem.Text = "list 접기";
            // 
            // toolStrip1
            // 
            toolStrip1.Location = new Point(0, 24);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(1211, 25);
            toolStrip1.TabIndex = 1;
            toolStrip1.Text = "toolStrip1";
            // 
            // statusStrip1
            // 
            statusStrip1.Location = new Point(0, 607);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(1211, 22);
            statusStrip1.TabIndex = 2;
            statusStrip1.Text = "statusStrip1";
            // 
            // highwaylistContainer
            // 
            highwaylistContainer.Dock = DockStyle.Fill;
            highwaylistContainer.Location = new Point(0, 49);
            highwaylistContainer.Name = "highwaylistContainer";
            // 
            // highwaylistContainer.Panel1
            // 
            highwaylistContainer.Panel1.Controls.Add(splitContainer1);
            // 
            // highwaylistContainer.Panel2
            // 
            highwaylistContainer.Panel2.Paint += splitContainer2_Panel2_Paint;
            highwaylistContainer.Panel2Collapsed = true;
            highwaylistContainer.Panel2MinSize = 0;
            highwaylistContainer.Size = new Size(1211, 558);
            highwaylistContainer.SplitterDistance = 1071;
            highwaylistContainer.TabIndex = 4;
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(filterPanel);
            splitContainer1.Panel1.Paint += splitContainer1_Panel1_Paint;
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(webView21);
            splitContainer1.Size = new Size(1211, 558);
            splitContainer1.SplitterDistance = 229;
            splitContainer1.TabIndex = 4;
            // 
            // filterPanel
            // 
            filterPanel.Controls.Add(label1);
            filterPanel.Dock = DockStyle.Fill;
            filterPanel.Location = new Point(0, 0);
            filterPanel.Name = "filterPanel";
            filterPanel.Size = new Size(229, 558);
            filterPanel.TabIndex = 0;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            label1.AutoSize = true;
            label1.Location = new Point(3, 0);
            label1.Name = "label1";
            label1.Size = new Size(125, 15);
            label1.TabIndex = 0;
            label1.Text = "splitcontainer1.panel1";
            label1.Click += label1_Click;
            // 
            // webView21
            // 
            webView21.AllowExternalDrop = true;
            webView21.CreationProperties = null;
            webView21.DefaultBackgroundColor = Color.White;
            webView21.Dock = DockStyle.Fill;
            webView21.Location = new Point(0, 0);
            webView21.Name = "webView21";
            webView21.Size = new Size(978, 558);
            webView21.TabIndex = 0;
            webView21.ZoomFactor = 1D;
            webView21.Click += webView21_Click_1;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1211, 629);
            Controls.Add(highwaylistContainer);
            Controls.Add(statusStrip1);
            Controls.Add(toolStrip1);
            Controls.Add(menuStrip1);
            Name = "Form1";
            Text = "Form1";
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            highwaylistContainer.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)highwaylistContainer).EndInit();
            highwaylistContainer.ResumeLayout(false);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            filterPanel.ResumeLayout(false);
            filterPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)webView21).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menuStrip1;
        private ToolStrip toolStrip1;
        private StatusStrip statusStrip1;
        private ToolStripMenuItem 파일ToolStripMenuItem;
        private ToolStripMenuItem 종료ToolStripMenuItem;
        private ToolStripMenuItem 보기ToolStripMenuItem;
        private ToolStripMenuItem 검색패널표시ToolStripMenuItem;
        private ToolStripMenuItem 상세정보패널표시ToolStripMenuItem;
        private ToolStripMenuItem 상태바표시ToolStripMenuItem;
        private ToolStripMenuItem 지도ToolStripMenuItem;
        private ToolStripMenuItem 현재위치로이동ToolStripMenuItem;
        private ToolStripMenuItem 하이라이트초기화ToolStripMenuItem;
        private ToolStripMenuItem 지도초기화ToolStripMenuItem;
        private ToolStripMenuItem 데이터ToolStripMenuItem;
        private ToolStripMenuItem 새로고침ToolStripMenuItem;
        private ToolStripMenuItem 고속도로조회ToolStripMenuItem;
        private ToolStripMenuItem 도움말ToolStripMenuItem;
        private ToolStripMenuItem 정보ToolStripMenuItem;
        private SplitContainer highwaylistContainer;
        private SplitContainer splitContainer1;
        private FlowLayoutPanel filterPanel;
        private Microsoft.Web.WebView2.WinForms.WebView2 webView21;
        private ToolStripMenuItem 디버그ToolStripMenuItem;
        private ToolStripMenuItem list출력ToolStripMenuItem;
        private Label label1;
        private ToolStripMenuItem list펴기ToolStripMenuItem;
        private ToolStripMenuItem list접기ToolStripMenuItem;
    }
}
