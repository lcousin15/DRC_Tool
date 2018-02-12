namespace DRC
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadWithPlateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.drawDRCToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportDataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rawDataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clusteringToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pCAToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tSNEToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.correlationsToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.correlationsToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.curvesSuperpositionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadCurvesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.drawCurvesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.checkedListBox1 = new System.Windows.Forms.CheckedListBox();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.AutoScroll = true;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Location = new System.Drawing.Point(13, 49);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(900, 900);
            this.tableLayoutPanel1.TabIndex = 13;
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.DefaultExt = "csv";
            this.saveFileDialog1.FileName = "raw_data";
            // 
            // toolTip1
            // 
            this.toolTip1.IsBalloon = true;
            this.toolTip1.ShowAlways = true;
            this.toolTip1.Tag = "";
            this.toolTip1.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Warning;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.clusteringToolStripMenuItem,
            this.correlationsToolStripMenuItem1,
            this.curvesSuperpositionToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1084, 24);
            this.menuStrip1.TabIndex = 14;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadToolStripMenuItem,
            this.loadWithPlateToolStripMenuItem,
            this.drawDRCToolStripMenuItem,
            this.exportDataToolStripMenuItem,
            this.exportToolStripMenuItem,
            this.rawDataToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // loadToolStripMenuItem
            // 
            this.loadToolStripMenuItem.Name = "loadToolStripMenuItem";
            this.loadToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.L)));
            this.loadToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.loadToolStripMenuItem.Text = "Load";
            this.loadToolStripMenuItem.Click += new System.EventHandler(this.loadToolStripMenuItem_Click);
            // 
            // loadWithPlateToolStripMenuItem
            // 
            this.loadWithPlateToolStripMenuItem.Name = "loadWithPlateToolStripMenuItem";
            this.loadWithPlateToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E)));
            this.loadWithPlateToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.loadWithPlateToolStripMenuItem.Text = "Load with plate";
            this.loadWithPlateToolStripMenuItem.Click += new System.EventHandler(this.loadWithPlateToolStripMenuItem_Click);
            // 
            // drawDRCToolStripMenuItem
            // 
            this.drawDRCToolStripMenuItem.Name = "drawDRCToolStripMenuItem";
            this.drawDRCToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D)));
            this.drawDRCToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.drawDRCToolStripMenuItem.Text = "Draw DRC";
            this.drawDRCToolStripMenuItem.Click += new System.EventHandler(this.drawDRCToolStripMenuItem_Click);
            // 
            // exportDataToolStripMenuItem
            // 
            this.exportDataToolStripMenuItem.Name = "exportDataToolStripMenuItem";
            this.exportDataToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.exportDataToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.exportDataToolStripMenuItem.Text = "Export Data";
            this.exportDataToolStripMenuItem.Click += new System.EventHandler(this.exportDataToolStripMenuItem_Click);
            // 
            // exportToolStripMenuItem
            // 
            this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            this.exportToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
            this.exportToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.exportToolStripMenuItem.Text = "Export Report";
            this.exportToolStripMenuItem.Click += new System.EventHandler(this.exportToolStripMenuItem_Click);
            // 
            // rawDataToolStripMenuItem
            // 
            this.rawDataToolStripMenuItem.Name = "rawDataToolStripMenuItem";
            this.rawDataToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.R)));
            this.rawDataToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.rawDataToolStripMenuItem.Text = "Show Raw Data";
            this.rawDataToolStripMenuItem.Click += new System.EventHandler(this.rawDataToolStripMenuItem_Click);
            // 
            // clusteringToolStripMenuItem
            // 
            this.clusteringToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.pCAToolStripMenuItem,
            this.tSNEToolStripMenuItem});
            this.clusteringToolStripMenuItem.Name = "clusteringToolStripMenuItem";
            this.clusteringToolStripMenuItem.Size = new System.Drawing.Size(73, 20);
            this.clusteringToolStripMenuItem.Text = "Clustering";
            // 
            // pCAToolStripMenuItem
            // 
            this.pCAToolStripMenuItem.Name = "pCAToolStripMenuItem";
            this.pCAToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.pCAToolStripMenuItem.Text = "PCA";
            this.pCAToolStripMenuItem.Click += new System.EventHandler(this.pCAToolStripMenuItem_Click);
            // 
            // tSNEToolStripMenuItem
            // 
            this.tSNEToolStripMenuItem.Name = "tSNEToolStripMenuItem";
            this.tSNEToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.tSNEToolStripMenuItem.Text = "T-SNE";
            this.tSNEToolStripMenuItem.Click += new System.EventHandler(this.tSNEToolStripMenuItem_Click);
            // 
            // correlationsToolStripMenuItem1
            // 
            this.correlationsToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.correlationsToolStripMenuItem2});
            this.correlationsToolStripMenuItem1.Name = "correlationsToolStripMenuItem1";
            this.correlationsToolStripMenuItem1.Size = new System.Drawing.Size(83, 20);
            this.correlationsToolStripMenuItem1.Text = "Correlations";
            // 
            // correlationsToolStripMenuItem2
            // 
            this.correlationsToolStripMenuItem2.Name = "correlationsToolStripMenuItem2";
            this.correlationsToolStripMenuItem2.Size = new System.Drawing.Size(138, 22);
            this.correlationsToolStripMenuItem2.Text = "Correlations";
            this.correlationsToolStripMenuItem2.Click += new System.EventHandler(this.correlationsToolStripMenuItem2_Click);
            // 
            // curvesSuperpositionToolStripMenuItem
            // 
            this.curvesSuperpositionToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadCurvesToolStripMenuItem,
            this.drawCurvesToolStripMenuItem});
            this.curvesSuperpositionToolStripMenuItem.Name = "curvesSuperpositionToolStripMenuItem";
            this.curvesSuperpositionToolStripMenuItem.Size = new System.Drawing.Size(131, 20);
            this.curvesSuperpositionToolStripMenuItem.Text = "Curves Superposition";
            // 
            // loadCurvesToolStripMenuItem
            // 
            this.loadCurvesToolStripMenuItem.Name = "loadCurvesToolStripMenuItem";
            this.loadCurvesToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.loadCurvesToolStripMenuItem.Text = "Load curves";
            this.loadCurvesToolStripMenuItem.Click += new System.EventHandler(this.loadCurvesToolStripMenuItem_Click);
            // 
            // drawCurvesToolStripMenuItem
            // 
            this.drawCurvesToolStripMenuItem.Name = "drawCurvesToolStripMenuItem";
            this.drawCurvesToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.drawCurvesToolStripMenuItem.Text = "Draw Curves";
            this.drawCurvesToolStripMenuItem.Click += new System.EventHandler(this.drawCurvesToolStripMenuItem_Click);
            // 
            // comboBox1
            // 
            this.comboBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(952, 3);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(120, 21);
            this.comboBox1.TabIndex = 13;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectionChangeCommited);
            // 
            // checkedListBox1
            // 
            this.checkedListBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkedListBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.checkedListBox1.CheckOnClick = true;
            this.checkedListBox1.FormattingEnabled = true;
            this.checkedListBox1.HorizontalScrollbar = true;
            this.checkedListBox1.Location = new System.Drawing.Point(952, 49);
            this.checkedListBox1.Name = "checkedListBox1";
            this.checkedListBox1.Size = new System.Drawing.Size(120, 165);
            this.checkedListBox1.TabIndex = 14;
            // 
            // Form1
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(1084, 961);
            this.Controls.Add(this.checkedListBox1);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "DRC Main Tab";
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.Form1_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.Form_DragEnter);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        public System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem drawDRCToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportDataToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rawDataToolStripMenuItem;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.CheckedListBox checkedListBox1;
        private System.Windows.Forms.ToolStripMenuItem loadWithPlateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clusteringToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pCAToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tSNEToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem correlationsToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem correlationsToolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem curvesSuperpositionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadCurvesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem drawCurvesToolStripMenuItem;
    }
}

