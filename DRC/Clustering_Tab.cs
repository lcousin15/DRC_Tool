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
    public partial class Clustering_Tab : Form
    {

        MainTab _form1 = new MainTab();

        public Clustering_Tab(MainTab form)
        {
            InitializeComponent();
            _form1 = form;
        }

        private string search_cpd_point(Dictionary<string, List<double>> cpd_dict, double pos_x, double pos_y)
        {
            string BATCH_ID = "";

            foreach (KeyValuePair<string, List<double>> item in cpd_dict)
            {
                List<double> point_coord = item.Value;
                if(Math.Abs(point_coord[0]-pos_x)<0.001 && Math.Abs(point_coord[1] - pos_y) < 0.001) BATCH_ID = item.Key;
            }

            return BATCH_ID;
        }

        Point? prevPosition = null;
        ToolTip tooltip = new ToolTip();

        private void chart1_MouseMove(object sender, MouseEventArgs e)
        {
            var pos = e.Location;

            if (prevPosition.HasValue && pos == prevPosition.Value)
                return;

            Dictionary<string, List < double>> cpd_names = new Dictionary<string, List<double>>();
            cpd_names = _form1.get_cpd_clustering();

            tooltip.RemoveAll();
            prevPosition = pos;
            var results = chart1.HitTest(pos.X, pos.Y, false, ChartElementType.DataPoint);

            foreach (var result in results)
            {
                if (result.ChartElementType == ChartElementType.DataPoint)
                {
                    var prop = result.Object as DataPoint;
                    if (prop != null)
                    {
                        var pointXPixel = result.ChartArea.AxisX.ValueToPixelPosition(prop.XValue);
                        var pointYPixel = result.ChartArea.AxisY.ValueToPixelPosition(prop.YValues[0]);

                        // check if the cursor is really close to the point (1 pixel around the point)
                        if (Math.Abs(pos.X - pointXPixel) < 5 &&
                            Math.Abs(pos.Y - pointYPixel) < 5)
                        {
                            string cpd = search_cpd_point(cpd_names, prop.XValue, prop.YValues[0]);
                            tooltip.Show(cpd, this.chart1, pos.X, pos.Y + 15);
                        }
                    }
                }
            }
        }

        private void chart1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var pos = e.Location;

            Dictionary<string, List<double>> cpd_names = new Dictionary<string, List<double>>();
            cpd_names = _form1.get_cpd_clustering();

            var results = chart1.HitTest(pos.X, pos.Y, false, ChartElementType.DataPoint);

            foreach (var result in results)
            {
                if (result.ChartElementType == ChartElementType.DataPoint)
                {
                    var prop = result.Object as DataPoint;
                    if (prop != null)
                    {
                        var pointXPixel = result.ChartArea.AxisX.ValueToPixelPosition(prop.XValue);
                        var pointYPixel = result.ChartArea.AxisY.ValueToPixelPosition(prop.YValues[0]);

                        // check if the cursor is really close to the point (1 pixel around the point)
                        if (Math.Abs(pos.X - pointXPixel) < 5 &&
                            Math.Abs(pos.Y - pointYPixel) < 5)
                        {
                            string cpd = search_cpd_point(cpd_names, prop.XValue, prop.YValues[0]);

                            _form1.tableLayoutPanel1.Controls.Clear();

                            List<Chart_DRC> list_chart = _form1.get_descriptors_chart()[cpd];

                            foreach (Chart_DRC current_chart in list_chart)
                            {
                                current_chart.draw_DRC(false, true);
                            }
                        }
                    }
                }
            }
        }

        private void Form6_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }
    }
}
