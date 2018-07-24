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
        AUC_Report _auc_report_tab = new AUC_Report(_main_tab);

        public void Reset()
        {
            tableLayoutPanel1.Controls.Clear();
        }

        public void draw_compound(string cpd_id)
        {
            _main_tab.draw_compound(cpd_id);
        }

        private void exportReportToolStripMenuItem_Click(object sender, EventArgs e)
        {

            Form fc = Application.OpenForms["AUC_Report"];

            if (fc != null)
            {
                auc_report();
            }
            else
            {
                _auc_report_tab = new AUC_Report(_main_tab);
                auc_report();
            }
        }

        private void auc_report()
        {

        }

        /*
        //public Progress progress;
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        private Image byteArrayToImage(byte[] bytesArr)
        {
            MemoryStream memstr = new MemoryStream(bytesArr);
            Image img = Image.FromStream(memstr);
            return img;
        }

        private byte[] imageToByteArray(System.Drawing.Image imageIn)
        {
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            return ms.ToArray();
        }

        private void AddImage(ExcelWorksheet oSheet, int rowIndex, int colIndex, Bitmap img, string name)
        {
            //Bitmap image = new Bitmap(img);
            ExcelPicture excelImage = null;

            excelImage = oSheet.Drawings.AddPicture(name, img);
            excelImage.From.Column = colIndex;
            excelImage.From.Row = rowIndex;
            excelImage.SetSize(485, 350);
        }

        private void saveToExcelToolStripMenuItem_Click(object sender, EventArgs e)
        {

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Excel Documents (*.xlsx)|*.xlsx";
            sfd.FileName = "DRC_Report.xlsx";
            if (sfd.ShowDialog() == DialogResult.OK)
            {

                ExcelPackage pck = new ExcelPackage();
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add("DRC_Report");

                int cellRowIndex = 1;
                int cellColumnIndex = 1;

                Graphics g = this.CreateGraphics();

                int image_width = 485;
                int image_height = 350;

                double height = (double)image_height / g.DpiY * 72.0f; //  g.DpiY
                double width = (double)image_width / g.DpiX * 72.0f / 5.1f; // image_width; g.DpiX

                for (int i = 1; i <= dataGridViewExport.Rows.Count; i++)
                {
                    if (i == 1) ws.Row(i).Height = 20;
                    else ws.Row(i).Height = height;
                }

                for (int j = 1; j <= dataGridViewExport.Columns.Count; j++)
                {
                    if ((j - 1) % 4 == 2) ws.Column(j).Width = width;
                    else ws.Column(j).Width = 15;
                    //if (j == 0) worksheet.Columns[j].ColumnWidth = 10;
                }

                toolStripProgressBar1.Visible = true;
                //Loop through each row and read value from each column. 
                for (int i = 0; i < dataGridViewExport.Rows.Count - 1; i++)
                {
                    toolStripProgressBar1.Value = i * 100 / (dataGridViewExport.Rows.Count - 1);


                    for (int j = 0; j < dataGridViewExport.Columns.Count; j++)
                    {
                        // Excel index starts from 1,1. As first Row would have the Column headers, adding a condition check. 
                        if (cellRowIndex == 1)
                        {
                            ws.Cells[cellRowIndex, cellColumnIndex].Value = dataGridViewExport.Columns[j].HeaderText;

                            ws.Cells[cellRowIndex, cellColumnIndex].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            ws.Cells[cellRowIndex, cellColumnIndex].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                            ws.Cells[cellRowIndex, cellColumnIndex].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Dotted);
                            ws.Cells[cellRowIndex, cellColumnIndex].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                            ws.Cells[cellRowIndex, cellColumnIndex].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                        }

                        if (dataGridViewExport.Rows[i].Cells[j].Value.ToString() == "System.Drawing.Bitmap")
                        {

                            Bitmap img = (Bitmap)(dataGridViewExport.Rows[i].Cells[j].Value);

                            string name_idx = (cellRowIndex * dataGridViewExport.Columns.Count + cellColumnIndex).ToString();

                            ExcelPicture excelImage = null;

                            excelImage = ws.Drawings.AddPicture(name_idx, img);
                            excelImage.From.Column = cellColumnIndex - 1;
                            excelImage.From.Row = cellRowIndex;
                            excelImage.SetSize(485, 350);


                        }
                        else
                        {

                            if (j > 0)
                            {
                                double current_value;
                                bool is_double = Double.TryParse(dataGridViewExport.Rows[i].Cells[j].Value.ToString(), out current_value);
                                if (is_double) ws.Cells[cellRowIndex + 1, cellColumnIndex].Value = (double)current_value; //Convert.ToDouble(dataGridViewExport.Rows[i].Cells[j].Value);
                                else ws.Cells[cellRowIndex + 1, cellColumnIndex].Value = dataGridViewExport.Rows[i].Cells[j].Value;

                                ws.Cells[cellRowIndex + 1, cellColumnIndex].Style.Numberformat.Format = "0.00E+00";

                                ws.Cells[cellRowIndex + 1, cellColumnIndex].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                ws.Cells[cellRowIndex + 1, cellColumnIndex].Style.Fill.BackgroundColor.SetColor(dataGridViewExport.Rows[i].Cells[j].Style.BackColor);
                                ws.Cells[cellRowIndex + 1, cellColumnIndex].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                ws.Cells[cellRowIndex + 1, cellColumnIndex].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                ws.Cells[cellRowIndex + 1, cellColumnIndex].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Dotted);

                            }
                            if (j == 0 || j == 1)
                            {

                                double current_value;
                                bool is_double = Double.TryParse(dataGridViewExport.Rows[i].Cells[j].Value.ToString(), out current_value);
                                if (is_double) ws.Cells[cellRowIndex + 1, cellColumnIndex].Value = (double)current_value; //Convert.ToDouble(dataGridViewExport.Rows[i].Cells[j].Value);
                                else ws.Cells[cellRowIndex + 1, cellColumnIndex].Value = dataGridViewExport.Rows[i].Cells[j].Value;

                                ws.Cells[cellRowIndex + 1, cellColumnIndex].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                ws.Cells[cellRowIndex + 1, cellColumnIndex].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                                ws.Cells[cellRowIndex + 1, cellColumnIndex].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                ws.Cells[cellRowIndex + 1, cellColumnIndex].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                ws.Cells[cellRowIndex + 1, cellColumnIndex].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Dotted);

                            }


                        }

                        cellColumnIndex++;
                    }
                    cellColumnIndex = 1;
                    cellRowIndex++;

                }

                toolStripProgressBar1.Visible = false;
                pck.SaveAs(new FileInfo(@"" + sfd.FileName));

                pck.Dispose();

                MessageBox.Show("Export Successful");

            }
            
        }*/

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
            if(graph_type == "auc") chartArea.AxisY.Title = "AUC";
            else if(graph_type == "z-score") chartArea.AxisY.Title = "AUC (Z-Score)";

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
            if (graph_type == "auc") chart.Titles[0].Text = "AUC | " + descriptor.ToString();
            else if (graph_type == "z-score") chart.Titles[0].Text = "AUC (Z-Score) | " + descriptor.ToString();

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

                        // check if the cursor is really close to the point (2 pixels around the point)
                        if (Math.Abs(pos.X - pointXPixel) < 10 && Math.Abs(pos.Y - pointYPixel) < 10)
                        {
                            double point_x = prop.XValue;
                            double point_y = prop.YValues[0];

                            int index = y.FindIndex(a => a < point_y + 1E-8 && a > point_y - 1E-8);
                            string cpd = cpd_labels[index];


                            if (graph_type == "auc") tooltip.Show("CPD = " + cpd + " | AUC = " + prop.YValues[0].ToString("N2"), this.chart, pos.X, pos.Y - 15);
                            else if (graph_type == "z-score") tooltip.Show("CPD = " + cpd + " | Z-Score = " + prop.YValues[0].ToString("N2"), this.chart, pos.X, pos.Y - 15);

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

