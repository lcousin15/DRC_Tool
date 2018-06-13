using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace DRC
{
    public partial class Curves_Options : Form
    {
        public Curves_Options(Chart_DRC my_chart)
        {
            InitializeComponent();
            chart = my_chart;
        }

        private Chart_DRC chart;

        private double min_x;
        private double min_y;
        private double max_x;
        private double max_y;
        private Color my_color;

        public void set_curve_params(double minX, double maxX, double minY, double maxY, Color color)
        {
            min_x = minX;
            min_y = minY;
            max_x = maxX;
            max_y = maxY;
            my_color = color;

            tb_min_x.Text = min_x.ToString();
            tb_min_y.Text = min_y.ToString();
            tb_max_x.Text = max_x.ToString();
            tb_max_y.Text = max_y.ToString();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            min_x = double.Parse(tb_min_x.Text);
            max_x = double.Parse(tb_max_x.Text);
            min_y = double.Parse(tb_min_y.Text);
            max_y = double.Parse(tb_max_y.Text);

            chart.change_params(min_x, max_x, min_y, max_y, my_color);
    }

        private void Curves_Options_Load(object sender, EventArgs e)
        {

        }

        private void btn_change_color_Click(object sender, EventArgs e)
        {
            ColorDialog dlg = new ColorDialog();
            dlg.ShowDialog();

            Color new_color = dlg.Color;

            my_color = new_color;

            chart.re_fill_color(my_color);
        }

        private void btn_close_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
