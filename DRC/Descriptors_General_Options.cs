using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DRC
{
    public partial class Descriptors_General_Options : Form
    {
        MainTab _main_tab;

        public Descriptors_General_Options(MainTab main_tab)
        {
            InitializeComponent();
            _main_tab = main_tab;
        }

        private double bnd_min_x;
        private double bnd_max_x;
        private double bnd_min_y;
        private double bnd_max_y;

        private double window_min_x;
        private double window_max_x;
        private double window_min_y;
        private double window_max_y;

        private void btn_ok_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btn_apply_Click(object sender, EventArgs e)
        {
            double idx = 0;
            double descriptor_number = _main_tab.get_descriptor_list().Count;

            toolStripProgressBar1.Visible = true;

            foreach (string item in _main_tab.get_descriptor_list())
            {
                //-------------------------- Fit Bounds Parameters --------------------------//

                //-------------------------- Window Parameters --------------------------//

                if (this.panel2.Controls.ContainsKey("txt_box_window_min_x_descriptor_" + item))
                {
                    TextBox txt_box = this.panel2.Controls["txt_box_window_min_x_descriptor_" + item] as TextBox;
                    bool is_converted = double.TryParse(txt_box.Text.ToString(), out window_min_x);
                }

                if (this.panel2.Controls.ContainsKey("txt_box_window_max_x_descriptor_" + item))
                {
                    TextBox txt_box = this.panel2.Controls["txt_box_window_max_x_descriptor_" + item] as TextBox;
                    bool is_converted = double.TryParse(txt_box.Text.ToString(), out window_max_x);
                }

                if (this.panel2.Controls.ContainsKey("txt_box_window_min_y_descriptor_" + item))
                {
                    TextBox txt_box = this.panel2.Controls["txt_box_window_min_y_descriptor_" + item] as TextBox;
                    bool is_converted = double.TryParse(txt_box.Text.ToString(), out window_min_y);
                }

                if (this.panel2.Controls.ContainsKey("txt_box_window_max_y_descriptor_" + item))
                {
                    TextBox txt_box = this.panel2.Controls["txt_box_window_max_y_descriptor_" + item] as TextBox;
                    bool is_converted = double.TryParse(txt_box.Text.ToString(), out window_max_y);
                }

                _main_tab.apply_descritpor_general_scale(item, window_min_x, window_max_x, window_min_y, window_max_y); // item = descriptor name

                idx++;
                toolStripProgressBar1.Value = (int)(100 * idx / descriptor_number);

            }

            toolStripProgressBar1.Visible = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            double idx = 0;
            double descriptor_number = _main_tab.get_descriptor_list().Count;

            toolStripProgressBar1.Visible = true;

            foreach (string item in _main_tab.get_descriptor_list())
            {

                if (this.panel1.Controls.ContainsKey("txt_box_bnd_min_x_descriptor_" + item))
                {
                    TextBox txt_box = this.panel1.Controls["txt_box_bnd_min_x_descriptor_" + item] as TextBox;
                    bool is_converted = double.TryParse(txt_box.Text.ToString(), out bnd_min_x);
                }

                if (this.panel1.Controls.ContainsKey("txt_box_bnd_max_x_descriptor_" + item))
                {
                    TextBox txt_box = this.panel1.Controls["txt_box_bnd_max_x_descriptor_" + item] as TextBox;
                    bool is_converted = double.TryParse(txt_box.Text.ToString(), out bnd_max_x);
                }

                if (this.panel1.Controls.ContainsKey("txt_box_bnd_min_y_descriptor_" + item))
                {
                    TextBox txt_box = this.panel1.Controls["txt_box_bnd_min_y_descriptor_" + item] as TextBox;
                    bool is_converted = double.TryParse(txt_box.Text.ToString(), out bnd_min_y);
                }

                if (this.panel1.Controls.ContainsKey("txt_box_bnd_max_y_descriptor_" + item))
                {
                    TextBox txt_box = this.panel1.Controls["txt_box_bnd_max_y_descriptor_" + item] as TextBox;
                    bool is_converted = double.TryParse(txt_box.Text.ToString(), out bnd_max_y);
                }

                _main_tab.apply_descritpor_general_bounds(item, bnd_min_x, bnd_max_x, bnd_min_y, bnd_max_y); // item = descriptor name

                idx++;
                toolStripProgressBar1.Value = (int)(100 * idx / descriptor_number);
            }

            toolStripProgressBar1.Visible = false;
        }
    };

}
