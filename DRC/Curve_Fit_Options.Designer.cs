namespace DRC
{
    partial class Curve_Fit_Options
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
            this.btn_OK = new System.Windows.Forms.Button();
            this.btn_apply = new System.Windows.Forms.Button();
            this.lbl_max_bound_y = new System.Windows.Forms.Label();
            this.lbl_min_bound_y = new System.Windows.Forms.Label();
            this.lbl_max_bound_x = new System.Windows.Forms.Label();
            this.lbl_min_bound_x = new System.Windows.Forms.Label();
            this.txt_max_bound_y = new System.Windows.Forms.TextBox();
            this.txt_min_bound_y = new System.Windows.Forms.TextBox();
            this.txt_max_bound_x = new System.Windows.Forms.TextBox();
            this.txt_min_bound_x = new System.Windows.Forms.TextBox();
            this.btn_reset = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btn_OK
            // 
            this.btn_OK.Location = new System.Drawing.Point(30, 148);
            this.btn_OK.Name = "btn_OK";
            this.btn_OK.Size = new System.Drawing.Size(156, 23);
            this.btn_OK.TabIndex = 21;
            this.btn_OK.Text = "Ok";
            this.btn_OK.UseVisualStyleBackColor = true;
            this.btn_OK.Click += new System.EventHandler(this.btn_OK_Click);
            // 
            // btn_apply
            // 
            this.btn_apply.Location = new System.Drawing.Point(30, 119);
            this.btn_apply.Name = "btn_apply";
            this.btn_apply.Size = new System.Drawing.Size(75, 23);
            this.btn_apply.TabIndex = 19;
            this.btn_apply.Text = "Apply";
            this.btn_apply.UseVisualStyleBackColor = true;
            this.btn_apply.Click += new System.EventHandler(this.btn_apply_Click);
            // 
            // lbl_max_bound_y
            // 
            this.lbl_max_bound_y.AutoSize = true;
            this.lbl_max_bound_y.Location = new System.Drawing.Point(12, 91);
            this.lbl_max_bound_y.Name = "lbl_max_bound_y";
            this.lbl_max_bound_y.Size = new System.Drawing.Size(80, 13);
            this.lbl_max_bound_y.TabIndex = 18;
            this.lbl_max_bound_y.Text = "Upper Bound Y";
            // 
            // lbl_min_bound_y
            // 
            this.lbl_min_bound_y.AutoSize = true;
            this.lbl_min_bound_y.Location = new System.Drawing.Point(12, 64);
            this.lbl_min_bound_y.Name = "lbl_min_bound_y";
            this.lbl_min_bound_y.Size = new System.Drawing.Size(80, 13);
            this.lbl_min_bound_y.TabIndex = 17;
            this.lbl_min_bound_y.Text = "Lower Bound Y";
            // 
            // lbl_max_bound_x
            // 
            this.lbl_max_bound_x.AutoSize = true;
            this.lbl_max_bound_x.Location = new System.Drawing.Point(12, 36);
            this.lbl_max_bound_x.Name = "lbl_max_bound_x";
            this.lbl_max_bound_x.Size = new System.Drawing.Size(80, 13);
            this.lbl_max_bound_x.TabIndex = 16;
            this.lbl_max_bound_x.Text = "Upper Bound X";
            // 
            // lbl_min_bound_x
            // 
            this.lbl_min_bound_x.AutoSize = true;
            this.lbl_min_bound_x.Location = new System.Drawing.Point(12, 9);
            this.lbl_min_bound_x.Name = "lbl_min_bound_x";
            this.lbl_min_bound_x.Size = new System.Drawing.Size(80, 13);
            this.lbl_min_bound_x.TabIndex = 15;
            this.lbl_min_bound_x.Text = "Lower Bound X";
            // 
            // txt_max_bound_y
            // 
            this.txt_max_bound_y.Location = new System.Drawing.Point(104, 88);
            this.txt_max_bound_y.Name = "txt_max_bound_y";
            this.txt_max_bound_y.Size = new System.Drawing.Size(100, 20);
            this.txt_max_bound_y.TabIndex = 14;
            // 
            // txt_min_bound_y
            // 
            this.txt_min_bound_y.Location = new System.Drawing.Point(104, 61);
            this.txt_min_bound_y.Name = "txt_min_bound_y";
            this.txt_min_bound_y.Size = new System.Drawing.Size(100, 20);
            this.txt_min_bound_y.TabIndex = 13;
            // 
            // txt_max_bound_x
            // 
            this.txt_max_bound_x.Location = new System.Drawing.Point(104, 33);
            this.txt_max_bound_x.Name = "txt_max_bound_x";
            this.txt_max_bound_x.Size = new System.Drawing.Size(100, 20);
            this.txt_max_bound_x.TabIndex = 12;
            // 
            // txt_min_bound_x
            // 
            this.txt_min_bound_x.Location = new System.Drawing.Point(104, 6);
            this.txt_min_bound_x.Name = "txt_min_bound_x";
            this.txt_min_bound_x.Size = new System.Drawing.Size(100, 20);
            this.txt_min_bound_x.TabIndex = 11;
            // 
            // btn_reset
            // 
            this.btn_reset.Location = new System.Drawing.Point(111, 119);
            this.btn_reset.Name = "btn_reset";
            this.btn_reset.Size = new System.Drawing.Size(75, 23);
            this.btn_reset.TabIndex = 22;
            this.btn_reset.Text = "Reset";
            this.btn_reset.UseVisualStyleBackColor = true;
            this.btn_reset.Click += new System.EventHandler(this.btn_reset_Click);
            // 
            // Curve_Fit_Options
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(215, 181);
            this.Controls.Add(this.btn_reset);
            this.Controls.Add(this.btn_OK);
            this.Controls.Add(this.btn_apply);
            this.Controls.Add(this.lbl_max_bound_y);
            this.Controls.Add(this.lbl_min_bound_y);
            this.Controls.Add(this.lbl_max_bound_x);
            this.Controls.Add(this.lbl_min_bound_x);
            this.Controls.Add(this.txt_max_bound_y);
            this.Controls.Add(this.txt_min_bound_y);
            this.Controls.Add(this.txt_max_bound_x);
            this.Controls.Add(this.txt_min_bound_x);
            this.Name = "Curve_Fit_Options";
            this.Text = "Curve_Fit_Options";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btn_OK;
        private System.Windows.Forms.Button btn_apply;
        private System.Windows.Forms.Label lbl_max_bound_y;
        private System.Windows.Forms.Label lbl_min_bound_y;
        private System.Windows.Forms.Label lbl_max_bound_x;
        private System.Windows.Forms.Label lbl_min_bound_x;
        private System.Windows.Forms.TextBox txt_max_bound_y;
        private System.Windows.Forms.TextBox txt_min_bound_y;
        private System.Windows.Forms.TextBox txt_max_bound_x;
        private System.Windows.Forms.TextBox txt_min_bound_x;
        private System.Windows.Forms.Button btn_reset;
    }
}