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
using LumenWorks.Framework.IO.Csv;
using System.IO;
using Excel = Microsoft.Office.Interop.Excel;

namespace DRC
{
    public partial class Correlations_Tab : Form
    {
        public Correlations_Tab()
        {
            InitializeComponent();
        }

        public RawData_Correlations_Tab f8 = new RawData_Correlations_Tab();
        public Export_Excel_Tab f9 = new Export_Excel_Tab();

        CachedCsvReader csv;

        List<Chart_Correlations> charts = new List<Chart_Correlations>();

        private double fold_number;

        private List<string> list_items_1 = new List<string>();
        private List<string> list_items_2 = new List<string>();

        private string label_exp_1;
        private string label_exp_2;

        private string fileName1;
        private string fileName2;

        private void Reset()
        {
            checkedListBox1.Items.Clear();

            f8.dataGridView1.DataSource = null;
            f8.dataGridView1.Rows.Clear();

            f8.dataGridView2.DataSource = null;
            f8.dataGridView2.Rows.Clear();

            if (charts.Count > 0) charts.Clear();

            list_items_1.Clear();
            list_items_2.Clear();

            label_exp_1 = "";
            label_exp_2 = "";

            fileName1 = "";
            fileName2 = "";
        }

        private void read_Data_1()
        {
            //f8.Show();

            f8.dataGridView1.DataSource = csv;

            foreach (DataGridViewColumn col in f8.dataGridView1.Columns)
            {
                string col_name = col.HeaderText;

                if (col_name != "Run" && col_name != "BATCH_ID" && col_name != "CPD_ID")
                {
                    if (col_name.Contains("EC_50")) list_items_1.Add(col_name); //checkedListBox1.Items.Add(col_name);
                }
            }
        }

        private void read_Data_2()
        {
            //f8.Show();

            f8.dataGridView2.DataSource = csv;

            foreach (DataGridViewColumn col in f8.dataGridView2.Columns)
            {
                string col_name = col.HeaderText;

                if (col_name != "Run" && col_name != "BATCH_ID" && col_name != "CPD_ID")
                {
                    if (col_name.Contains("EC_50")) list_items_2.Add(col_name); //checkedListBox1.Items.Add(col_name);
                }
            }

        }

        public void set_Labels(string lbl_exp_1, string lbl_exp_2)
        {
            label_exp_1 = lbl_exp_1;
            label_exp_2 = lbl_exp_2;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // load file
            Reset();

            openFileDialog1.Filter = "CSV Files (*.csv)|*.csv";

            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.Text = openFileDialog1.FileName;

                fileName1 = openFileDialog1.FileName.Split('\\').Last();

                System.IO.StreamReader sr = new System.IO.StreamReader(openFileDialog1.FileName);
                csv = new CachedCsvReader(sr, true);

                read_Data_1();
            }

            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.Text = openFileDialog1.FileName;

                fileName2 = openFileDialog1.FileName.Split('\\').Last();

                System.IO.StreamReader sr = new System.IO.StreamReader(openFileDialog1.FileName);
                csv = new CachedCsvReader(sr, true);

                read_Data_2();
            }

            foreach (string item in list_items_1)
            {
                if (list_items_2.Contains(item))
                {
                    checkedListBox1.Items.Add(item);
                }
            }

            Correlations_Exp_Name form_label = new Correlations_Exp_Name(this, fileName1, fileName2);

            form_label.Visible = true;

            return;
        }

        private void button1_Click(object sender, EventArgs e)
        {

            fold_number = double.Parse(numericUpDown1.Value.ToString());
            // draw correlations

            tableLayoutPanel1.Controls.Clear();

            int item_row = 0;

            foreach (string item in checkedListBox1.CheckedItems)
            {

                string run_description_1 = "";
                string run_description_2 = "";

                //// Statistics :
                Dictionary<string, int> dict_stat_not_fitted = new Dictionary<string, int>();   // statistics on R/N for the moment
                Dictionary<string, int> dict_not_fitted_1 = new Dictionary<string, int>();
                Dictionary<string, int> dict_not_fitted_2 = new Dictionary<string, int>();
                //string item_stats = "G/N";

                Dictionary<string, double> dict_var_1 = new Dictionary<string, double>();
                foreach (DataGridViewRow row in f8.dataGridView1.Rows)
                {

                    string cpd = "";

                    if (f8.dataGridView1.Columns.Contains("BATCH_ID")) cpd = row.Cells["BATCH_ID"].Value.ToString();
                    else if (f8.dataGridView1.Columns.Contains("CPD_ID")) cpd = row.Cells["CPD_ID"].Value.ToString();

                    string val_str = row.Cells[item].Value.ToString();
                    run_description_1 = label_exp_1;

                    double value = 0;
                    if (!val_str.Contains("Not Fitted") && !val_str.Contains("Inactive"))
                    {
                        value = double.Parse(row.Cells[item].Value.ToString());
                        dict_var_1.Add(cpd, value);
                    }
                    else
                    {
                        if (item.Contains(item))
                        {
                            dict_not_fitted_1.Add(cpd, 1);

                            if (dict_stat_not_fitted.ContainsKey(cpd))
                            {
                                dict_stat_not_fitted[cpd] = 2;
                            }
                            else
                            {
                                dict_stat_not_fitted.Add(cpd, 1);
                            }
                        }
                    }
                }

                Dictionary<string, double> dict_var_2 = new Dictionary<string, double>();
                foreach (DataGridViewRow row in f8.dataGridView2.Rows)
                {
                    string cpd = "";

                    if (f8.dataGridView2.Columns.Contains("BATCH_ID")) cpd = row.Cells["BATCH_ID"].Value.ToString();
                    else if (f8.dataGridView2.Columns.Contains("CPD_ID")) cpd = row.Cells["CPD_ID"].Value.ToString();

                    string val_str = row.Cells[item].Value.ToString();
                    run_description_2 = label_exp_2;

                    double value = 0;
                    if (!val_str.Contains("Not Fitted") && !val_str.Contains("Inactive"))
                    {
                        value = double.Parse(row.Cells[item].Value.ToString());
                        dict_var_2.Add(cpd, value);
                    }
                    else
                    {
                        if (item.Contains(item))
                        {
                            dict_not_fitted_2.Add(cpd, 1);

                            if (dict_stat_not_fitted.ContainsKey(cpd))
                            {
                                dict_stat_not_fitted[cpd] = 2;
                            }
                            else
                            {
                                dict_stat_not_fitted.Add(cpd, 1);
                            }
                        }
                    }
                }

                // Statistics (only on "R/N" for the moment) :

                int in_3_folds = 0;
                int out_3_folds = 0;

                foreach (var it in dict_var_1)
                {
                    string cpd = it.Key;
                    double ec_50_1 = it.Value;

                    double ec_50_2 = 0;
                    if (dict_var_2.ContainsKey(cpd))
                    {
                        ec_50_2 = dict_var_2[cpd];
                    }
                    else continue;

                    if ((ec_50_2 / ec_50_1) < fold_number && (ec_50_2 / ec_50_1) > 1.0 / fold_number)
                    {
                        in_3_folds++;
                    }
                    else
                    {
                        out_3_folds++;
                    }

                }

                int Fitted_Not_Fitted = 0;
                int Not_Fitted_Not_Fitted = 0;

                foreach (var iter in dict_stat_not_fitted)
                {
                    string cpd = iter.Key;
                    int nb_not_fitted = iter.Value;

                    if (nb_not_fitted == 1)
                    {
                        if ((dict_not_fitted_1.ContainsKey(cpd) && dict_var_2.ContainsKey(cpd)) || (dict_not_fitted_2.ContainsKey(cpd) && dict_var_1.ContainsKey(cpd)))
                        {
                            Console.WriteLine(cpd);
                            Fitted_Not_Fitted++;
                        }
                    }
                    if (nb_not_fitted == 2) Not_Fitted_Not_Fitted++;
                }

                //dataGridView1.ResumeLayout();
                if (dataGridView1.ColumnCount == 0)
                {
                    dataGridView1.Columns.Add("Column0", "Descriptor");
                    dataGridView1.Columns.Add("Column1", "In " + fold_number.ToString() + "-Folds");
                    dataGridView1.Columns.Add("Column2", "Out " + fold_number.ToString() + "-Folds");
                    dataGridView1.Columns.Add("Column3", "Fitted / Not Fitted");
                    dataGridView1.Columns.Add("Column4", "Not Fitted / Not Fitted");
                }
                else
                {
                    dataGridView1.Columns["Column0"].HeaderText = "Descriptor";
                    dataGridView1.Columns["Column1"].HeaderText = "In " + fold_number.ToString() + "-Folds";
                    dataGridView1.Columns["Column2"].HeaderText = "Out " + fold_number.ToString() + "-Folds";
                    dataGridView1.Columns["Column3"].HeaderText = "Fitted / Not Fitted";
                    dataGridView1.Columns["Column4"].HeaderText = "Not Fitted / Not Fitted";
                }


                double total = in_3_folds + out_3_folds + Fitted_Not_Fitted + Not_Fitted_Not_Fitted;

                dataGridView1.Rows.Add();
                dataGridView1.Rows[item_row].Cells[0].Value = item;
                dataGridView1.Rows[item_row].Cells[1].Value = in_3_folds.ToString() + " (" + (100.0 * in_3_folds / total).ToString("N1") + "%)";
                dataGridView1.Rows[item_row].Cells[2].Value = out_3_folds.ToString() + " (" + (100.0 * out_3_folds / total).ToString("N1") + "%)";
                dataGridView1.Rows[item_row].Cells[3].Value = Fitted_Not_Fitted.ToString() + " (" + (100.0 * Fitted_Not_Fitted / total).ToString("N1") + "%)";
                dataGridView1.Rows[item_row].Cells[4].Value = Not_Fitted_Not_Fitted.ToString() + " (" + (100.0 * Not_Fitted_Not_Fitted / total).ToString("N1") + "%)";

                Color color = Color.Blue;
                if (item.Contains("Nuclei")) color = Color.Blue;
                if (item.Contains("R/N") || item.Contains("R")) color = Color.Red;
                if (item.Contains("G/N") || item.Contains("G")) color = Color.Green;
                if (item.Contains("LDA_1") || item.Contains("LDA")) color = Color.Black;

                Chart_Correlations chart_correl = new Chart_Correlations(dict_var_1, dict_var_2, item, label_exp_1, label_exp_2, color, this, fold_number);
                charts.Add(chart_correl);

                item_row += 1;
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string path = folderBrowserDialog1.SelectedPath;

                foreach (Chart_Correlations chart in charts)
                {
                    chart.save_image(path);
                }

                MessageBox.Show("Figures Saved.");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {

            Dictionary<string, List<double>> row_folds = new Dictionary<string, List<double>>();
            Dictionary<string, List<int>> cpd_descriptor_index = new Dictionary<string, List<int>>();

            //Getting the location and file name of the excel to save from user. 
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "Excel Documents (*.xlsx)|*.xlsx";
            saveDialog.FileName = "Correlation_Report.xlsx";
            saveDialog.FilterIndex = 2;

            if (saveDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string path = folderBrowserDialog1.SelectedPath;

                int index = 0;
                foreach (Chart_Correlations chart in charts)
                {
                    Dictionary<string, double> is_3_folds = chart.get_is_3_folds();

                    foreach (KeyValuePair<string, double> item in is_3_folds)
                    {
                        string cpd = item.Key;
                        double folds = item.Value;

                        if (row_folds.ContainsKey(cpd))
                        {
                            row_folds[cpd].Add(folds);

                            if (cpd_descriptor_index.ContainsKey(cpd))
                            {
                                cpd_descriptor_index[cpd].Add(index);
                            }
                            else
                            {
                                cpd_descriptor_index[cpd] = new List<int>();
                                cpd_descriptor_index[cpd].Add(index);
                            }
                        }
                        else
                        {
                            row_folds[cpd] = new List<double>();
                            row_folds[cpd].Add(folds);

                            if (cpd_descriptor_index.ContainsKey(cpd))
                            {
                                cpd_descriptor_index[cpd].Add(index);
                            }
                            else
                            {
                                cpd_descriptor_index[cpd] = new List<int>();
                                cpd_descriptor_index[cpd].Add(index);
                            }
                        }

                    }
                    index++;
                }

                //DataGridView dataGridView1 = new DataGridView();
                f9.dataGridView1.ColumnCount = charts.Count + 1;

                int col_index = 0;
                foreach (DataGridViewColumn col in f9.dataGridView1.Columns)
                {
                    if (col_index < charts.Count + 1) f9.dataGridView1.Columns[col_index].HeaderText = f8.dataGridView1.Columns[col_index].HeaderText;
                    col_index++;
                }

                foreach (KeyValuePair<string, List<double>> item in row_folds)
                {
                    var index_row = f9.dataGridView1.Rows.Add();  //(DataGridViewRow)f9.dataGridView1.Rows[0].Clone();
                    DataGridViewRow current_row = f9.dataGridView1.Rows[index_row];

                    string cpd = item.Key;
                    List<double> folds = item.Value;

                    current_row.Cells[0].Value = cpd;

                    for (int i = 0; i < folds.Count(); i++)
                    {
                        current_row.Cells[cpd_descriptor_index[cpd][i] + 1].Value = folds[i];

                        if (folds[i] <= fold_number && folds[i] > 1.0 / fold_number) current_row.Cells[cpd_descriptor_index[cpd][i] + 1].Style.BackColor = Color.LightGreen;
                        else current_row.Cells[cpd_descriptor_index[cpd][i] + 1].Style.BackColor = Color.Tomato;
                    }

                    //f9.dataGridView1.Rows.Add(current_row);
                }

                //f9.Show();

                // Creating a Excel object. 
                Microsoft.Office.Interop.Excel._Application excel = new Microsoft.Office.Interop.Excel.Application();
                Microsoft.Office.Interop.Excel._Workbook workbook = excel.Workbooks.Add(Type.Missing);
                Microsoft.Office.Interop.Excel._Worksheet worksheet = null;

                try
                {
                    worksheet = workbook.ActiveSheet;

                    worksheet.Name = "ExportedFromDatGrid";

                    int cellRowIndex = 1;
                    int cellColumnIndex = 1;

                    //Loop through each row and read value from each column. 
                    for (int i = 0; i < f9.dataGridView1.Rows.Count - 1; i++)
                    {
                        for (int j = 0; j < f9.dataGridView1.Columns.Count; j++)
                        {
                            // Excel index starts from 1,1. As first Row would have the Column headers, adding a condition check. 
                            if (cellRowIndex == 1)
                            {
                                worksheet.Cells[cellRowIndex, cellColumnIndex] = f9.dataGridView1.Columns[j].HeaderText;
                                if(j==1) worksheet.Cells[cellRowIndex, cellColumnIndex] = f9.dataGridView1.Columns[j].HeaderText + " [Folds]";
                                worksheet.Cells[cellRowIndex, cellColumnIndex].Interior.Color = Color.LightGray;
                                worksheet.Cells[cellRowIndex, cellColumnIndex].Borders.Weight = 1d;
                            }


                            if (j > 0)
                            {
                                worksheet.Cells[cellRowIndex + 1, cellColumnIndex] = f9.dataGridView1.Rows[i].Cells[j].Value; //Convert.ToDouble(dataGridViewExport.Rows[i].Cells[j].Value);
                                worksheet.Cells[cellRowIndex + 1, cellColumnIndex].Interior.Color = f9.dataGridView1.Rows[i].Cells[j].Style.BackColor;                                                                                   //worksheet.Cells[cellRowIndex + 1, cellColumnIndex].NumberFormat = "0.00E+00";
                                if (f9.dataGridView1.Rows[i].Cells[j].Value == null || f9.dataGridView1.Rows[i].Cells[j].Value == DBNull.Value
                                    || String.IsNullOrWhiteSpace(f9.dataGridView1.Rows[i].Cells[j].Value.ToString()))
                                {
                                    worksheet.Cells[cellRowIndex + 1, cellColumnIndex].Interior.Color = Color.White;
                                }
                            }
                            if (j == 0)
                            {
                                worksheet.Cells[cellRowIndex + 1, cellColumnIndex] = f9.dataGridView1.Rows[i].Cells[j].Value;
                                worksheet.Cells[cellRowIndex + 1, cellColumnIndex].Interior.Color = Color.LightGray;
                                worksheet.Cells[cellRowIndex + 1, cellColumnIndex].Borders.Weight = 1d;
                            }
                            worksheet.Cells[cellRowIndex + 1, cellColumnIndex].Style.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                            worksheet.Cells[cellRowIndex + 1, cellColumnIndex].Style.VerticalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;


                            cellColumnIndex++;
                        }
                        cellColumnIndex = 1;
                        cellRowIndex++;

                        worksheet.UsedRange.Columns.AutoFit();
                        worksheet.UsedRange.Rows.AutoFit();
                    }

                    workbook.SaveAs(saveDialog.FileName);
                    MessageBox.Show("Report Generated.");
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    excel.Quit();
                    workbook = null;
                    excel = null;
                }

            }
        }

    }

    public class Chart_Correlations
    {
        Correlations_Tab _form7 = new Correlations_Tab();

        private Chart chart;

        private Color chart_color;

        Dictionary<string, double> dict_x = new Dictionary<string, double>();
        Dictionary<string, double> dict_y = new Dictionary<string, double>();

        private List<double> x = new List<double>();
        private List<double> y = new List<double>();

        private List<string> cpd_list = new List<string>();

        private List<double> x_line_1 = new List<double>();
        private List<double> y_line_1 = new List<double>();

        private List<double> x_line_2 = new List<double>();
        private List<double> y_line_2 = new List<double>();

        private List<double> x_line_3 = new List<double>();
        private List<double> y_line_3 = new List<double>();

        private double min_x;
        private double max_x;
        private double min_y;
        private double max_y;

        private string descriptor;

        private string exp_run_1;
        private string exp_run_2;

        double fold_number;

        Dictionary<string, double> cpd_3_folds = new Dictionary<string, double>();

        public Dictionary<string, double> get_is_3_folds()
        {
            return cpd_3_folds;
        }

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

        public Chart_Correlations()
        {
        }

        public Chart_Correlations(Dictionary<string, double> ec_50_1, Dictionary<string, double> ec_50_2, string descrip, string run_1, string run_2, Color color, Correlations_Tab form, double folds)
        {
            _form7 = form;

            chart = new Chart();

            descriptor = descrip;
            chart_color = color;

            exp_run_1 = run_1;
            exp_run_2 = run_2;

            dict_x = ec_50_1;
            dict_y = ec_50_2;

            fold_number = folds;

            ChartArea chartArea = new ChartArea();
            Series series1 = new Series();
            Series series2 = new Series();
            Series series3 = new Series();
            Series series4 = new Series();

            //chartArea.Position.Auto = false;
            Axis yAxis = new Axis(chartArea, AxisName.Y);
            Axis xAxis = new Axis(chartArea, AxisName.X);

            chartArea.AxisX.IsLogarithmic = true;
            chartArea.AxisY.IsLogarithmic = true;

            chartArea.AxisX.LabelStyle.Format = "E2";
            chartArea.AxisY.LabelStyle.Format = "E2";

            chartArea.AxisX.MajorGrid.LineWidth = 0;
            chartArea.AxisY.MajorGrid.LineWidth = 0;

            chartArea.AxisX.Title = "EC_50 " + run_1;
            chartArea.AxisY.Title = "EC_50 " + run_2;

            //if (max_y < 1.0) chartArea.AxisY.Maximum = 1.0;

            chartArea.Name = descriptor;

            chart.ChartAreas.Add(chartArea);
            chart.Name = descriptor;

            chart.Location = new System.Drawing.Point(250, 100);

            series1.ChartType = SeriesChartType.Point;
            series2.ChartType = SeriesChartType.Line;
            series3.ChartType = SeriesChartType.Line;
            series4.ChartType = SeriesChartType.Line;

            series1.MarkerStyle = MarkerStyle.Circle;

            series1.Name = "Series1";
            series2.Name = "Series2";
            series3.Name = "Series3";
            series4.Name = "Series4";

            chart.Series.Add(series1);
            chart.Series.Add(series2);
            chart.Series.Add(series3);
            chart.Series.Add(series4);

            chart.MouseMove += new System.Windows.Forms.MouseEventHandler(this.chart1_MouseMove);

            chart.Size = new System.Drawing.Size(550, 350);

            chart.Titles.Add("Correlations");

            process_data();
            draw_chart();
        }

        private void process_data()
        {
            foreach (KeyValuePair<string, double> item in dict_x)
            {
                string cpd = item.Key;

                if (dict_y.ContainsKey(cpd))
                {
                    double value_1 = item.Value;
                    double value_2 = dict_y[cpd];

                    x.Add(value_1);
                    y.Add(value_2);

                    cpd_list.Add(cpd);

                    double folds_0 = value_2 / value_1;
                    //double folds_1 = value_1 / value_2;
                    double folds = folds_0; // Math.Max(folds_0, folds_1);

                    //bool is_in_3_folds = value_2 >= 1.0/3.0*value_1 && value_2 <= 3.0 * value_1;
                    cpd_3_folds[cpd] = folds;
                }
            }

            if (x.Count > 0)
            {
                min_x = 0.5 * MinA<double>(x.ToArray());
                max_x = 2.0 * MaxA<double>(x.ToArray());
            }
            else
            {
                min_x = 0.000001;
                max_x = 1.0;
            }

            if (y.Count > 0)
            {
                min_y = 0.5 * MinA<double>(y.ToArray());
                max_y = 2.0 * MaxA<double>(y.ToArray());
            }
            else
            {
                min_y = 0.000001;
                max_y = 1.0;
            }
            //int a = (int)Math.Abs(Math.Floor(Math.Log10(min_x)));
            //double b = Math.Round(min_x, (int)Math.Abs(Math.Floor(Math.Log10(min_x))));

            //chart.ChartAreas[0].AxisX.Minimum = Math.Round(min_x, (int)Math.Abs(Math.Floor(Math.Log10(min_x))));
            //chart.ChartAreas[0].AxisY.Minimum = Math.Round(min_y, (int)Math.Abs(Math.Floor(Math.Log10(min_y))));

            //chart.ChartAreas[0].AxisX.Maximum = Math.Round(max_x, (int)Math.Abs(Math.Floor(Math.Log10(max_x))));
            //chart.ChartAreas[0].AxisY.Maximum = Math.Round(max_y, (int)Math.Abs(Math.Floor(Math.Log10(max_y))));

            //chart.ChartAreas[0].AxisX.Minimum = Math.Pow(10, Math.Floor(Math.Log10(min_x)));
            //chart.ChartAreas[0].AxisY.Minimum = Math.Pow(10, Math.Floor(Math.Log10(min_y)));

            //chart.ChartAreas[0].AxisX.Maximum = Math.Pow(10, Math.Ceiling(Math.Log10(max_x)));
            //chart.ChartAreas[0].AxisY.Maximum = Math.Pow(10, Math.Ceiling(Math.Log10(max_y)));

            int fixed_min_x = (int)Math.Floor(Math.Log10(min_x));
            int fixed_max_x = (int)Math.Ceiling(Math.Log10(max_x));

            int fixed_min_y = (int)Math.Floor(Math.Log10(min_y));
            int fixed_max_y = (int)Math.Ceiling(Math.Log10(max_y));

            //int fixed_min = Math.Min(fixed_min_x, fixed_min_y);
            //int fixed_max = Math.Max(fixed_max_x, fixed_max_y);

            min_x = Math.Pow(10, fixed_min_x);
            max_x = Math.Pow(10, fixed_max_x);
            min_y = Math.Pow(10, fixed_min_y);
            max_y = Math.Pow(10, fixed_max_y);

            chart.ChartAreas[0].AxisX.Minimum = min_x;
            chart.ChartAreas[0].AxisY.Minimum = min_y;

            chart.ChartAreas[0].AxisX.Maximum = max_x;
            chart.ChartAreas[0].AxisY.Maximum = max_y;

            chart.ChartAreas[0].AxisX.IsLogarithmic = true;
            chart.ChartAreas[0].AxisY.IsLogarithmic = true;
            chart.ChartAreas[0].AxisX.LogarithmBase = 10;
            chart.ChartAreas[0].AxisY.LogarithmBase = 10;

            //chart.ChartAreas[0].AxisX.LabelStyle.Interval = 0.5;
            //chart.ChartAreas[0].AxisY.LabelStyle.Interval = 0.5;

            //chart.ChartAreas[0].AxisX.Minimum = min_x;
            //chart.ChartAreas[0].AxisY.Minimum = min_y;

            //chart.ChartAreas[0].AxisX.Maximum = max_x;
            //chart.ChartAreas[0].AxisY.Maximum = max_y;

            chart.ChartAreas[0].AxisX.Interval = 1;
            chart.ChartAreas[0].AxisY.Interval = 1;

            ////chart.ChartAreas[0].AxisX.MajorTickMark.Interval = 0.5;
            ////chart.ChartAreas[0].AxisY.MajorTickMark.Interval = 0.5;

            chart.ChartAreas[0].AxisX.MinorGrid.Enabled = true;
            chart.ChartAreas[0].AxisX.MinorGrid.Interval = 1;
            chart.ChartAreas[0].AxisX.MinorGrid.LineDashStyle = ChartDashStyle.Dot;
            chart.ChartAreas[0].AxisX.MinorGrid.LineColor = Color.Gray;

            chart.ChartAreas[0].AxisY.MinorGrid.Enabled = true;
            chart.ChartAreas[0].AxisY.MinorGrid.Interval = 1;
            chart.ChartAreas[0].AxisY.MinorGrid.LineDashStyle = ChartDashStyle.Dot;
            chart.ChartAreas[0].AxisY.MinorGrid.LineColor = Color.Gray;

        }

        private List<double> linspace(double min, double max, int steps)
        {
            List<double> values = new List<double>();

            for (int i = 0; i <= steps; ++i)
            {
                values.Add(min + (max - min) * i / (double)steps);
            }

            return values;
        }

        private void draw_chart()
        {
            chart.Titles[0].Text = descriptor;

            chart.Series["Series1"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
            chart.Series["Series1"].Points.DataBindXY(x, y);
            chart.Series["Series1"].Color = chart_color;

            _form7.tableLayoutPanel1.Controls.Add(chart);
            chart.Anchor = (AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top);

            x_line_1 = linspace(min_x, max_x, 2);
            y_line_1 = x_line_1;

            chart.Series["Series2"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chart.Series["Series2"].Points.DataBindXY(x_line_1, y_line_1);
            chart.Series["Series2"].Color = Color.Black;

            foreach (double elem in y_line_1)
            {
                y_line_2.Add(1.0 / fold_number * elem);
                y_line_3.Add(fold_number * elem);
            }

            chart.Series["Series3"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chart.Series["Series3"].Points.DataBindXY(x_line_1, y_line_2);
            //chart.Series["Series3"].BorderDashStyle = ChartDashStyle.DashDotDot;
            chart.Series["Series3"].Color = Color.Gray;

            chart.Series["Series4"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chart.Series["Series4"].Points.DataBindXY(x_line_1, y_line_3);
            //chart.Series["Series4"].BorderDashStyle = ChartDashStyle.Dot;
            chart.Series["Series4"].Color = Color.Gray;

        }

        public string save_image(string path)
        {
            string descriptor_name = descriptor.Replace(@"/", @"_");
            string output_image = path + "/Correlations_" + descriptor_name + "_run_" + exp_run_1 + "_vs_" + exp_run_2 + ".bmp";

            // System.Diagnostics.Debug.WriteLine("Write Image = " + output_image);me
            chart.SaveImage(output_image, ChartImageFormat.Bmp);

            return output_image;
        }

        Point? prevPosition = null;
        ToolTip tooltip = new ToolTip();

        void chart1_MouseMove(object sender, MouseEventArgs e)
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
                        var pointXPixel = result.ChartArea.AxisX.ValueToPixelPosition(prop.XValue);
                        var pointYPixel = result.ChartArea.AxisY.ValueToPixelPosition(prop.YValues[0]);

                        // check if the cursor is really close to the point (2 pixels around the point)
                        if (Math.Abs(pos.X - pointXPixel) < 2 && Math.Abs(pos.Y - pointYPixel) < 2)
                        {
                            double point_x = prop.XValue;
                            double point_y = prop.YValues[0];

                            int index = y.FindIndex(a => a < point_y + 1E-15 && a > point_y - 1E-15);
                            string cpd = cpd_list[index];

                            tooltip.Show("CPD = " + cpd + ", X=" + prop.XValue + ", Y=" + prop.YValues[0], this.chart, pos.X, pos.Y - 15);
                        }
                    }
                }
            }
        }

    }
}
