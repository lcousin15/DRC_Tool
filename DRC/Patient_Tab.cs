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
using System.Net;

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
        AUC_Report _auc_report_tab;
        PathwayTab pathway_form;

        private List<string> list_cpds = new List<string>();

        public void Reset()
        {
            tableLayoutPanel1.Controls.Clear();
        }

        public void draw_compound(string BATCH_ID)
        {
            _main_tab.draw_compound(BATCH_ID);
        }

        private void exportReportToolStripMenuItem_Click(object sender, EventArgs e)
        {

            Form fc = Application.OpenForms["AUC_Report"];

            if (fc != null)
            {
                _auc_report_tab.auc_report();
            }
            else
            {
                _auc_report_tab = new AUC_Report(_main_tab);
                _auc_report_tab.auc_report();
            }
        }


        public void get_pathway(int first_cpd_number)
        {
            this.toolStripProgressBar1.Visible = true;

            //LocusID = 100133941;
            List<string> ListPathway = new List<string>();
            List<string> ListTarget = new List<string>();

            Dictionary<string, List<string>> Path_target = new Dictionary<string, List<string>>();
            int idx = 0;
            foreach (var CPD in list_cpds.Take(first_cpd_number))
            {

                this.toolStripProgressBar1.Value = idx * 100 / (list_cpds.Count - 1);
                idx++;
                Console.WriteLine("------ CPD : " + CPD);
                string getvars = "/find/drug/" + CPD;
                WebRequest req = WebRequest.Create(string.Format("http://rest.kegg.jp" + getvars)) as WebRequest;
                req.Method = "GET";


                HttpWebResponse response2 = req.GetResponse() as HttpWebResponse;
                StreamReader reader = new StreamReader(response2.GetResponseStream());

                string CPD_KEGG = "";

                CPD_KEGG = reader.ReadLine().Split('\t')[0];

                reader.Close();
                response2.Close();

                if (CPD_KEGG != "")
                {

                    Console.WriteLine("------ CPD : " + CPD + "------ BATCH_ID : " + CPD_KEGG);

                    string getvars3 = "/get/" + CPD_KEGG;
                    WebRequest req3 = WebRequest.Create(string.Format("http://rest.kegg.jp" + getvars3)) as WebRequest;
                    req3.Method = "GET";


                    HttpWebResponse response3 = req3.GetResponse() as HttpWebResponse;
                    StreamReader reader3 = new StreamReader(response3.GetResponseStream());

                    List<string> toto = reader3.ReadToEnd().Split(' ').ToList();
                    foreach (string item in toto)
                    {

                        if (item.Contains("hsa0"))
                        {

                            ListPathway.Add(item.Substring(0, 8));
                            List<string> targetss = (item.Substring(9, item.Length - 10)).Split('+').ToList();
                            if (Path_target.ContainsKey(item.Substring(0, 8)))
                            {
                                List<string> temp = Path_target[item.Substring(0, 8)];
                                temp.AddRange(targetss.Distinct().ToList());
                                Path_target[item.Substring(0, 8)] = temp.Distinct().ToList();
                            }

                            else
                            {
                                Path_target[item.Substring(0, 8)] = targetss.Distinct().ToList();

                            }
                        }
                    }

                    reader3.Close();
                    response3.Close();

                }

            }

            var ordered = Path_target.OrderByDescending(x => x.Value.Count);
            int kl = 0;

            foreach (KeyValuePair<string, List<string>> item in ordered)
            {
                Console.WriteLine("------ PATHWAYS : " + item.Key);


                string hgf = "https://www.kegg.jp/kegg-bin/show_pathway?org_name=hsadd&map=" + item.Key + "&multi_query=";
                foreach (string target in item.Value)
                {
                    hgf += target + "+%23FF0000/";
                }
                System.Diagnostics.Process.Start(hgf);



            }

            Console.WriteLine("------ NUMBER OF PATHWAYS : " + kl);

            this.toolStripProgressBar1.Visible = false;
        }

        private void Find_Pathways(List<string> CPDS)
        {

            list_cpds = CPDS;

            Form fc = Application.OpenForms["PathwayTab"];

            if (fc == null)
                pathway_form = new PathwayTab(this);

            pathway_form.Show();
        }

        private void displayPathwaysToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<string> CPDS = new List<string>();

            Dictionary<string, Chart_Patient> auc_by_descriptor = _main_tab.get_charts_auc();
            Dictionary<string, double> cpd_auc = auc_by_descriptor[auc_by_descriptor.First().Key].get_auc_values();
            foreach (KeyValuePair<string, double> elem in cpd_auc)
            {
                CPDS.Add(elem.Key);
            }

            Find_Pathways(CPDS);
        }

    }

    public class Chart_Patient
    {
        Patient_Tab _form_patient; //= new Patient_Tab();

        private Chart chart;

        private Color chart_color;

        private Dictionary<string, double> dict_auc_cpds = new Dictionary<string, double>();
        private Dictionary<string, double> dict_auc_errors_cpds = new Dictionary<string, double>();

        private Dictionary<string, List<DataGridViewRow>> raw_data = new Dictionary<string, List<DataGridViewRow>>();
        private Dictionary<string, string> cpd_target = new Dictionary<string, string>();

        //private List<double> x = new List<double>();
        private List<double> y = new List<double>();
        private List<string> cpd_labels = new List<string>();
        private List<double> error_y = new List<double>();

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

        public Dictionary<string, double> get_auc_values()
        {
            return dict_auc_cpds;
        }

        public Dictionary<string, double> get_auc_error_values()
        {
            return dict_auc_errors_cpds;
        }

        public Chart_Patient(Dictionary<string, double> auc_descriptor, Dictionary<string, double> auc_descriptor_error, Dictionary<string, List<DataGridViewRow>> raw_data_dict, Dictionary<string, string> target, string descriptor_name, Color color, Patient_Tab f_patient, int number_charts, string graph)
        {
            chart = new Chart();

            descriptor = descriptor_name;

            chart_number = number_charts;

            _form_patient = f_patient;

            chart_color = color;

            dict_auc_cpds = auc_descriptor;
            dict_auc_errors_cpds = auc_descriptor_error;

            raw_data = raw_data_dict;
            cpd_target = target;

            graph_type = graph;

            ChartArea chartArea = new ChartArea();
            Series series1 = new Series();
            Series serie_error_bars = new Series();

            //chartArea.Position.Auto = false;
            Axis yAxis = new Axis(chartArea, AxisName.Y);
            Axis xAxis = new Axis(chartArea, AxisName.X);

            //chartArea.AxisY.IsLogarithmic = true;
            //chartArea.AxisY.LabelStyle.Format = "E2";

            //chartArea.AxisX.MajorGrid.LineWidth = 0;
            //chartArea.AxisY.MajorGrid.LineWidth = 0;

            chartArea.AxisX.Title = "Compound";
            if (graph_type == "auc") chartArea.AxisY.Title = "AUC";
            else if (graph_type == "z-score") chartArea.AxisY.Title = "AUC (Z-Score)";

            //if (max_y < 1.0) chartArea.AxisY.Maximum = 1.0;

            chartArea.Name = "AUC_" + descriptor;

            chart.ChartAreas.Add(chartArea);
            chart.Name = "AUC_" + descriptor;

            chart.Location = new System.Drawing.Point(250, 100);

            series1.ChartType = SeriesChartType.Point;
            series1.MarkerStyle = MarkerStyle.Circle;
            series1.Name = "Series1";

            serie_error_bars.ChartType = SeriesChartType.ErrorBar;
            serie_error_bars.MarkerStyle = MarkerStyle.Circle;
            serie_error_bars.Name = "Error_Bars";

            chart.Series.Add(series1);
            chart.Series.Add(serie_error_bars);

            chart.MouseDown += new System.Windows.Forms.MouseEventHandler(this.chart1_MouseDown);
            chart.MouseMove += new System.Windows.Forms.MouseEventHandler(this.chart1_MouseMove);
            chart.PostPaint += new EventHandler<ChartPaintEventArgs>(this.chart1_PostPaint);

            chart.Size = new System.Drawing.Size(1100, 500);

            chart.Titles.Add("AUC " + descriptor.ToString());

            process_data();
            draw_chart();
        }

        private void process_data()
        {

            //dict_auc_cpds = dict_auc_cpds.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

            foreach (KeyValuePair<string, double> item in dict_auc_cpds)
            {
                cpd_labels.Add(item.Key);
                y.Add(item.Value);
                error_y.Add(dict_auc_errors_cpds[item.Key]);
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

        private void chart1_PostPaint(object sender, System.Windows.Forms.DataVisualization.Charting.ChartPaintEventArgs e)
        {
            if (graph_type == "z-score")
            {
                Chart my_chart = (Chart)sender;
                ChartArea area = my_chart.ChartAreas[0];

                Axis ax = chart.ChartAreas[0].AxisX;
                Axis ay = chart.ChartAreas[0].AxisY;

                Graphics graph = e.ChartGraphics.Graphics;

                PointF point1 = PointF.Empty;
                PointF point2 = PointF.Empty;

                point1.X = (float)ax.ValueToPixelPosition(chart.ChartAreas[0].AxisX.Minimum);
                point1.Y = (float)ay.ValueToPixelPosition(-1.0);
                point2.X = (float)ax.ValueToPixelPosition(chart.ChartAreas[0].AxisX.Maximum);
                point2.Y = (float)ay.ValueToPixelPosition(-1.0);

                float[] dashValues = { 2, 2, 2, 2 };
                Pen blackPen = new Pen(Color.DimGray, 0.25f);
                blackPen.DashPattern = dashValues;

                graph.DrawLine(blackPen, point1, point2);
            }
        }

        private void draw_chart()
        {
            if (graph_type == "auc") chart.Titles[0].Text = "AUC | " + descriptor.ToString();
            else if (graph_type == "z-score") chart.Titles[0].Text = "AUC (Z-Score) | " + descriptor.ToString();

            chart.Series["Series1"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
            chart.Series["Series1"].Points.DataBindXY(cpd_labels, y);
            chart.Series["Series1"].Color = chart_color;
            chart.Series["Series1"].MarkerSize = 7;

            chart.Series["Error_Bars"].ChartType = SeriesChartType.ErrorBar;
            chart.Series["Error_Bars"].MarkerBorderColor = Color.Black;
            chart.Series["Error_Bars"].MarkerSize = 7;
            chart.Series["Error_Bars"].YValuesPerPoint = 3;
            chart.Series["Error_Bars"].BorderColor = Color.Black;
            chart.Series["Error_Bars"].Color = Color.Black;
            chart.Series["Error_Bars"].ShadowOffset = 1;
            chart.Series["Error_Bars"].MarkerStyle = MarkerStyle.None;
            chart.Series["Error_Bars"]["PointWidth"] = "0.25";

            //double error = 100;

            for (int i = 0; i < y.Count; ++i)
            {
                double centerY = y[i];
                double lowerErrorY = centerY - error_y[i];
                double upperErrorY = centerY + error_y[i];
                chart.Series["Error_Bars"].Points.AddXY(cpd_labels[i], centerY, lowerErrorY, upperErrorY);
            }

            //System.Drawing.Font chtFont = new System.Drawing.Font("Arial", 9, FontStyle.Regular);
            //chart.ChartAreas[0].AxisX.LabelStyle.Font = chtFont;

            //_form_patient.tableLayoutPanel1.RowCount = chart_number;
            //_form_patient.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, (float)(1.0/(double)chart_number)));

            _form_patient.tableLayoutPanel1.Controls.Add(chart);
            chart.Anchor = (AnchorStyles.Bottom | AnchorStyles.Top);
        }

        public string save_image(string path)
        {
            //chart.Width = 2200;
            //chart.Height = 1000;
            //draw_chart();

            string descriptor_name = descriptor.Replace(@"/", @"_");
            string output_image = path + "\\AUC_" + descriptor_name;

            if (graph_type == "auc") output_image += ".png";
            else if (graph_type == "z-score") output_image += "_z_score.png";

            // System.Diagnostics.Debug.WriteLine("Write Image = " + output_image);me
            chart.SaveImage(output_image, ChartImageFormat.Png);

            //chart.Width = 1100;
            //chart.Height = 500;

            return output_image;
        }

        private List<string> get_cpd_wells(string BATCH_ID)
        {
            HashSet<string> wells_id = new HashSet<string>();

            foreach (DataGridViewRow row in raw_data[BATCH_ID])
            {
                wells_id.Add(row.Cells["Well"].Value.ToString());
            }

            return wells_id.ToList();
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

                        // check if the cursor is really close to the point (2 pixels around the point)
                        if (Math.Abs(pos.X - pointXPixel) < 10 && Math.Abs(pos.Y - pointYPixel) < 10)
                        {
                            double point_x = prop.XValue;
                            double point_y = prop.YValues[0];

                            int index = y.FindIndex(a => a < point_y + 1E-8 && a > point_y - 1E-8);
                            string cpd = cpd_labels[index];

                            List<string> wells = get_cpd_wells(cpd);

                            string list_wells = "Wells = ";

                            for (int i = 0; i < wells.Count; ++i)
                            {
                                if (i < wells.Count - 1) list_wells += wells[i] + " , ";
                                else list_wells += wells[i];
                            }

                            //List<string> targets = get_targets(cpd);
                            string targets = "";
                            if(cpd_target.ContainsKey(cpd)) targets = cpd_target[cpd];

                            if (graph_type == "auc") tooltip.Show("CPD = " + cpd + " | AUC = " + prop.YValues[0].ToString("N2") + "\n" + list_wells + "\n" + "Targets = " + targets, this.chart, pos.X, pos.Y - 15);
                            else if (graph_type == "z-score") tooltip.Show("CPD = " + cpd + " | Z-Score = " + prop.YValues[0].ToString("N2") + "\n" + list_wells + "\n" + "Targets = " + targets, this.chart, pos.X, pos.Y - 15);

                        }
                    }
                }
            }
        }


        private void chart1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
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

                                //Console.WriteLine(cpd);

                                _form_patient.draw_compound(cpd);

                            }
                        }
                    }
                }
            }
        }
        /*
        private List<string> get_targets(string BATCH_ID)
        {
            Dictionary<string, List<string>> Path_target = new Dictionary<string, List<string>>();

            Console.WriteLine("------ CPD : " + BATCH_ID);
            string getvars = "/find/drug/" + BATCH_ID;
            WebRequest req = WebRequest.Create(string.Format("http://rest.kegg.jp" + getvars)) as WebRequest;
            req.Method = "GET";


            HttpWebResponse response2 = req.GetResponse() as HttpWebResponse;
            StreamReader reader = new StreamReader(response2.GetResponseStream());

            string CPD_KEGG = "";

            CPD_KEGG = reader.ReadLine().Split('\t')[0];

            reader.Close();
            response2.Close();

            if (CPD_KEGG != "")
            {

                Console.WriteLine("------ CPD : " + BATCH_ID + "------ BATCH_ID : " + CPD_KEGG);

                string getvars3 = "/get/" + CPD_KEGG;
                WebRequest req3 = WebRequest.Create(string.Format("http://rest.kegg.jp" + getvars3)) as WebRequest;
                req3.Method = "GET";


                HttpWebResponse response3 = req3.GetResponse() as HttpWebResponse;
                StreamReader reader3 = new StreamReader(response3.GetResponseStream());

                List<string> toto = reader3.ReadToEnd().Split(' ').ToList();
                foreach (string item in toto)
                {

                    if (item.Contains("hsa0"))
                    {
                        List<string> targetss = (item.Substring(9, item.Length - 10)).Split('+').ToList();
                        if (Path_target.ContainsKey(item.Substring(0, 8)))
                        {
                            List<string> temp = Path_target[item.Substring(0, 8)];
                            temp.AddRange(targetss.Distinct().ToList());
                            Path_target[item.Substring(0, 8)] = temp.Distinct().ToList();
                        }

                        else
                        {
                            Path_target[item.Substring(0, 8)] = targetss.Distinct().ToList();

                        }
                    }
                }

                reader3.Close();
                response3.Close();

            }


            Dictionary<string, List<string>> orderedDict = Path_target; //.OrderByDescending(x => x.Value.Count);
            List<string> str = new List<string>();

            foreach (KeyValuePair<string, List<string>> elem in orderedDict)
            {
                Console.WriteLine(elem.Key);
                str = elem.Value;
                foreach (string current_str in str) Console.WriteLine("..... " + current_str);
            }

            return str; // Not yet the good value, just a test
        }
        */
    }
}

