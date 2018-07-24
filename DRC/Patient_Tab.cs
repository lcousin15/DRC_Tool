using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;
using System.Windows.Forms.DataVisualization.Charting;
using System.IO;
using Excel = Microsoft.Office.Interop.Excel;

namespace DRC
{
    public partial class Patient_Tab : Form
    {
        public Patient_Tab(MainTab main_tab)
        {
            InitializeComponent();
            _main_tab = main_tab;
        }

        MainTab _main_tab;

        public void Reset()
        {
            tableLayoutPanel1.Controls.Clear();
        }

        public void draw_compound(string cpd_id)
        {
            _main_tab.draw_compound(cpd_id);
        }

    }

    public class Chart_Patient
    {
        Patient_Tab _form_patient; //= new Patient_Tab();

        private Chart chart;

        private Color chart_color;

        Dictionary<string, double> dict_auc_cpds = new Dictionary<string, double>();

        //private List<double> x = new List<double>();
        private List<double> y = new List<double>();
        private List<string> cpd_labels = new List<string>();

        private double min_x;
        private double max_x;
        private double min_y;
        private double max_y;

        private int chart_number;
        private string descriptor;
        private string graph_type;

        private T MinA<T>(T[] rest) where T : IComparable
        {
            T min = rest[0];
            foreach (T f in rest) if (f.CompareTo(min) < 0)
                    min = f;
            return min;
        }

        private T MaxA<T>(T[] rest) where T : IComparable
        {
            T max = rest[0];
            foreach (T f in rest) if (f.CompareTo(max) > 0)
                    max = f;
            return max;
        }

        public Chart_Patient()
        {
        }

        public string get_descriptor()
        {
            return descriptor;
        }

        public Chart_Patient(Dictionary<string, double> auc_descriptor, string descriptor_name, Color color, Patient_Tab f_patient, int number_charts, string graph)
        {
            chart = new Chart();

            descriptor = descriptor_name;

            chart_number = number_charts;

            _form_patient = f_patient;

            chart_color = color;

            dict_auc_cpds = auc_descriptor;

            graph_type = graph;

            ChartArea chartArea = new ChartArea();
            Series series1 = new Series();

            //chartArea.Position.Auto = false;
            Axis yAxis = new Axis(chartArea, AxisName.Y);
            Axis xAxis = new Axis(chartArea, AxisName.X);

            //chartArea.AxisY.IsLogarithmic = true;
            //chartArea.AxisY.LabelStyle.Format = "E2";

            //chartArea.AxisX.MajorGrid.LineWidth = 0;
            //chartArea.AxisY.MajorGrid.LineWidth = 0;

            chartArea.AxisX.Title = "Compound";
            chartArea.AxisY.Title = "AUC";

            //if (max_y < 1.0) chartArea.AxisY.Maximum = 1.0;

            chartArea.Name = "AUC_" + descriptor;

            chart.ChartAreas.Add(chartArea);
            chart.Name = "AUC_" + descriptor;

            chart.Location = new System.Drawing.Point(250, 100);

            series1.ChartType = SeriesChartType.Point;
            series1.MarkerStyle = MarkerStyle.Circle;
            series1.Name = "Series1";

            chart.Series.Add(series1);

            chart.MouseDown += new System.Windows.Forms.MouseEventHandler(this.chart1_MouseDown);
            chart.MouseMove += new System.Windows.Forms.MouseEventHandler(this.chart1_MouseMove);

            chart.Size = new System.Drawing.Size(1100, 500);

            chart.Titles.Add("AUC " + descriptor.ToString());

            process_data();
            draw_chart();
        }

        private void process_data()
        {

            foreach (KeyValuePair<string, double> item in dict_auc_cpds)
            {
                cpd_labels.Add(item.Key);
                y.Add(item.Value);
            }

            chart.ChartAreas[0].AxisX.MinorGrid.Enabled = true;
            chart.ChartAreas[0].AxisX.MinorGrid.Interval = 1;
            chart.ChartAreas[0].AxisX.MinorGrid.LineDashStyle = ChartDashStyle.Dot;
            chart.ChartAreas[0].AxisX.MinorGrid.LineColor = Color.LightGray;
            chart.ChartAreas[0].AxisX.LabelStyle.Angle = -90;
            chart.ChartAreas[0].AxisX.Interval = 1;

            chart.ChartAreas[0].AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dot;
            chart.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightGray;

            chart.ChartAreas[0].AxisY.MinorGrid.Enabled = true;
            chart.ChartAreas[0].AxisY.MinorGrid.Interval = 25;
            chart.ChartAreas[0].AxisY.MinorGrid.LineDashStyle = ChartDashStyle.Dot;
            chart.ChartAreas[0].AxisY.MinorGrid.LineColor = Color.LightGray;

            chart.ChartAreas[0].AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dot;
            chart.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;

        }

        private void draw_chart()
        {
            if (graph_type == "auc") chart.Titles[0].Text = "AUC " + descriptor.ToString();
            else if (graph_type == "z-score") chart.Titles[0].Text = "AUC Z-Score " + descriptor.ToString();

            chart.Series["Series1"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
            chart.Series["Series1"].Points.DataBindXY(cpd_labels, y);
            chart.Series["Series1"].Color = chart_color;
            chart.Series["Series1"].MarkerSize = 7;

            //_form_patient.tableLayoutPanel1.RowCount = chart_number;
            //_form_patient.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, (float)(1.0/(double)chart_number)));

            _form_patient.tableLayoutPanel1.Controls.Add(chart);
            chart.Anchor = (AnchorStyles.Bottom | AnchorStyles.Top);
        }

        public string save_image(string path)
        {
            string descriptor_name = descriptor.Replace(@"/", @"_");
            string output_image = path + "/AUC_" + descriptor_name + ".bmp";

            // System.Diagnostics.Debug.WriteLine("Write Image = " + output_image);me
            chart.SaveImage(output_image, ChartImageFormat.Bmp);

            return output_image;
        }

        Point? prevPosition = null;
        ToolTip tooltip = new ToolTip();

        private void chart1_MouseMove(object sender, MouseEventArgs e)
        {
            var pos = e.Location;
            if (prevPosition.HasValue && pos == prevPosition.Value)
                return;

            tooltip.RemoveAll();
            prevPosition = pos;

            var results = chart.HitTest(pos.X, pos.Y, false, ChartElementType.DataPoint);

            foreach (var result in results)
            {
                if (result.ChartElementType == ChartElementType.DataPoint)
                {
                    var prop = result.Object as DataPoint;
                    if (prop != null)
                    {

                        var index_lbls = (prop != null) ? chart.Series[0].Points.IndexOf(prop) : -1;

                        int pointXPixel = 0;
                        if (index_lbls > -1) pointXPixel = (int)chart.ChartAreas[0].AxisX.ValueToPixelPosition(index_lbls + 1);
                        //var pointXPixel = result.ChartArea.AxisX.ValueToPixelPosition(prop.XValue);
                        var pointYPixel = result.ChartArea.AxisY.ValueToPixelPosition(prop.YValues[0]);

                        //Console.WriteLine(pos.X + " , " + pointXPixel);
                        //Console.WriteLine(pos.Y + " , " + pointYPixel);

                        // check if the cursor is really close to the point (2 pixels around the point)
                        if (Math.Abs(pos.X - pointXPixel) < 10 && Math.Abs(pos.Y - pointYPixel) < 10)
                        {
                            double point_x = prop.XValue;
                            double point_y = prop.YValues[0];

                            int index = y.FindIndex(a => a < point_y + 1E-8 && a > point_y - 1E-8);
                            string cpd = cpd_labels[index];

                            Console.WriteLine("Survol");

                            if (graph_type == "auc") tooltip.Show("CPD = " + cpd + " | AUC = " + prop.YValues[0].ToString("N2"), this.chart, pos.X, pos.Y - 15);
                            else if (graph_type == "z-score") tooltip.Show("CPD = " + cpd + " | Z-Score = " + prop.YValues[0].ToString("N2"), this.chart, pos.X, pos.Y - 15);

                            //if(Control.MouseButtons == MouseButtons.Left)
                            //if (e.Button == MouseButtons.Left)
                            //{
                            //    Console.WriteLine(cpd);
                            //    _form_patient.draw_compound(cpd);
                            //}
                        }
                    }
                }
            }
        }


        private void chart1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Console.WriteLine("Click");

                //var pointYPixel = result.ChartArea.AxisY.ValueToPixelPosition(prop.YValues[0]);
                var results = chart.HitTest(e.X, e.Y, false, ChartElementType.DataPoint);

                foreach (var result in results)
                {
                    if (result.ChartElementType == ChartElementType.DataPoint)
                    {
                        var prop = result.Object as DataPoint;
                        if (prop != null)
                        {

                            var index_lbls = (prop != null) ? chart.Series[0].Points.IndexOf(prop) : -1;

                            int pointXPixel = 0;
                            if (index_lbls > -1) pointXPixel = (int)chart.ChartAreas[0].AxisX.ValueToPixelPosition(index_lbls + 1);
                            var pointYPixel = result.ChartArea.AxisY.ValueToPixelPosition(prop.YValues[0]);

                            // check if the cursor is really close to the point (2 pixels around the point)
                            if (Math.Abs(e.X - pointXPixel) < 10 && Math.Abs(e.Y - pointYPixel) < 10)
                            {
                                double point_x = prop.XValue;
                                double point_y = prop.YValues[0];

                                int index = y.FindIndex(a => a < point_y + 1E-8 && a > point_y - 1E-8);
                                string cpd = cpd_labels[index];

                                Console.WriteLine(cpd);

                                _form_patient.draw_compound(cpd);

                            }
                        }
                    }
                }
            }
        }
    }
}

