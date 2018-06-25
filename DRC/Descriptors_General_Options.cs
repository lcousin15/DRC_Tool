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

            foreach (string item in _main_tab.get_descriptor_list())
            {
                //-------------------------- Fit Bounds Parameters --------------------------//

                if (this.Controls.ContainsKey("txt_box_bnd_min_x_descriptor_" + item))
                {
                    TextBox txt_box = this.Controls["txt_box_bnd_min_x_descriptor_" + item] as TextBox;
                    bool is_converted = double.TryParse(txt_box.Text.ToString(), out bnd_min_x);
                }

                if (this.Controls.ContainsKey("txt_box_bnd_max_x_descriptor_" + item))
                {
                    TextBox txt_box = this.Controls["txt_box_bnd_max_x_descriptor_" + item] as TextBox;
                    bool is_converted = double.TryParse(txt_box.Text.ToString(), out bnd_max_x);
                }

                if (this.Controls.ContainsKey("txt_box_bnd_min_y_descriptor_" + item))
                {
                    TextBox txt_box = this.Controls["txt_box_bnd_min_y_descriptor_" + item] as TextBox;
                    bool is_converted = double.TryParse(txt_box.Text.ToString(), out bnd_min_y);
                }

                if (this.Controls.ContainsKey("txt_box_bnd_max_y_descriptor_" + item))
                {
                    TextBox txt_box = this.Controls["txt_box_bnd_max_y_descriptor_" + item] as TextBox;
                    bool is_converted = double.TryParse(txt_box.Text.ToString(), out bnd_max_y);
                }

                //-------------------------- Window Parameters --------------------------//

                if (this.Controls.ContainsKey("txt_box_window_min_x_descriptor_" + item))
                {
                    TextBox txt_box = this.Controls["txt_box_window_min_x_descriptor_" + item] as TextBox;
                    bool is_converted = double.TryParse(txt_box.Text.ToString(), out window_min_x);
                }

                if (this.Controls.ContainsKey("txt_box_window_max_x_descriptor_" + item))
                {
                    TextBox txt_box = this.Controls["txt_box_window_max_x_descriptor_" + item] as TextBox;
                    bool is_converted = double.TryParse(txt_box.Text.ToString(), out window_max_x);
                }

                if (this.Controls.ContainsKey("txt_box_window_min_y_descriptor_" + item))
                {
                    TextBox txt_box = this.Controls["txt_box_window_min_y_descriptor_" + item] as TextBox;
                    bool is_converted = double.TryParse(txt_box.Text.ToString(), out window_min_y);
                }

                if (this.Controls.ContainsKey("txt_box_window_max_y_descriptor_" + item))
                {
                    TextBox txt_box = this.Controls["txt_box_window_max_y_descriptor_" + item] as TextBox;
                    bool is_converted = double.TryParse(txt_box.Text.ToString(), out window_max_y);
                }
   
            }

            _main_tab.apply_descritpor_general_options(bnd_min_x, bnd_max_x, bnd_min_y, bnd_max_y,
                window_min_x, window_max_x, window_min_y, window_max_y);

            this.Close();
        }
    };

}
