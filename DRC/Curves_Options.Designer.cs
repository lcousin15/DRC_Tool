namespace DRC
{
    partial class Curves_Options
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
            this.btn_close = new System.Windows.Forms.Button();
            this.btn_apply = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.tb_min_x = new System.Windows.Forms.TextBox();
            this.tb_max_x = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tb_min_y = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tb_max_y = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.btn_change_color = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btn_close
            // 
            this.btn_close.Location = new System.Drawing.Point(15, 178);
            this.btn_close.Name = "btn_close";
            this.btn_close.Size = new System.Drawing.Size(174, 23);
            this.btn_close.TabIndex = 0;
            this.btn_close.Text = "Close";
            this.btn_close.UseVisualStyleBackColor = true;
            this.btn_close.Click += new System.EventHandler(this.btn_close_Click);
            // 
            // btn_apply
            // 
            this.btn_apply.Location = new System.Drawing.Point(15, 143);
            this.btn_apply.Name = "btn_apply";
            this.btn_apply.Size = new System.Drawing.Size(84, 23);
            this.btn_apply.TabIndex = 1;
            this.btn_apply.Text = "Apply";
            this.btn_apply.UseVisualStyleBackColor = true;
            this.btn_apply.Click += new System.EventHandler(this.button2_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(34, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Min X";
            // 
            // tb_min_x
            // 
            this.tb_min_x.Location = new System.Drawing.Point(58, 19);
            this.tb_min_x.Name = "tb_min_x";
            this.tb_min_x.Size = new System.Drawing.Size(100, 20);
            this.tb_min_x.TabIndex = 3;
            // 
            // tb_max_x
            // 
            this.tb_max_x.Location = new System.Drawing.Point(58, 45);
            this.tb_max_x.Name = "tb_max_x";
            this.tb_max_x.Size = new System.Drawing.Size(100, 20);
            this.tb_max_x.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(37, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Max X";
            // 
            // tb_min_y
            // 
            this.tb_min_y.Location = new System.Drawing.Point(58, 82);
            this.tb_min_y.Name = "tb_min_y";
            this.tb_min_y.Size = new System.Drawing.Size(100, 20);
            this.tb_min_y.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 85);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(34, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Min Y";
            // 
            // tb_max_y
            // 
            this.tb_max_y.Location = new System.Drawing.Point(58, 108);
            this.tb_max_y.Name = "tb_max_y";
            this.tb_max_y.Size = new System.Drawing.Size(100, 20);
            this.tb_max_y.TabIndex = 9;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 111);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(37, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Max Y";
            // 
            // btn_change_color
            // 
            this.btn_change_color.Location = new System.Drawing.Point(111, 143);
            this.btn_change_color.Name = "btn_change_color";
            this.btn_change_color.Size = new System.Drawing.Size(78, 23);
            this.btn_change_color.TabIndex = 10;
            this.btn_change_color.Text = "Change color";
            this.btn_change_color.UseVisualStyleBackColor = true;
            this.btn_change_color.Click += new System.EventHandler(this.btn_change_color_Click);
            // 
            // Curves_Options
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(199, 210);
            this.Controls.Add(this.btn_change_color);
            this.Controls.Add(this.tb_max_y);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.tb_min_y);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.tb_max_x);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tb_min_x);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btn_apply);
            this.Controls.Add(this.btn_close);
            this.Name = "Curves_Options";
            this.Text = "Curve Options";
            this.Load += new System.EventHandler(this.Curves_Options_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btn_close;
        private System.Windows.Forms.Button btn_apply;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tb_min_x;
        private System.Windows.Forms.TextBox tb_max_x;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tb_min_y;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tb_max_y;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btn_change_color;
    }
}