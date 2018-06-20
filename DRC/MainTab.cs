//using Accord.MachineLearning.DecisionTrees;
//using Accord.MachineLearning.DecisionTrees.Learning;
using Accord.MachineLearning.Clustering;
using Accord.Math;
using Accord.Statistics.Analysis;
using Accord.Statistics.Models.Regression.Linear;
using Emgu.CV;
using Emgu.CV.Structure;
using LumenWorks.Framework.IO.Csv;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Threading.Tasks;
using System.Globalization;
using System.Collections;
using System.Data;

namespace DRC
{

    public partial class MainTab : Form
    {
        //public bool imgCpdsViewOption = false;

        public class RowComparer : System.Collections.IComparer
        {
            private static int sortOrderModifier = 1;

            public RowComparer(SortOrder sortOrder)
            {
                if (sortOrder == SortOrder.Descending)
                {
                    sortOrderModifier = -1;
                }
                else if (sortOrder == SortOrder.Ascending)
                {
                    sortOrderModifier = 1;
                }
            }

            public int Compare(object x, object y)
            {
                DataGridViewRow DataGridViewRow1 = (DataGridViewRow)x;
                DataGridViewRow DataGridViewRow2 = (DataGridViewRow)y;

                // Try to sort based on the Last Name column.
                int CompareResult = System.Decimal.Compare(
                    Decimal.Parse(DataGridViewRow1.Cells["Concentration"].Value.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture),
                    Decimal.Parse(DataGridViewRow2.Cells["Concentration"].Value.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture));

                // If the Last Names are equal, sort based on the First Name.
                if (CompareResult == 0)
                {
                    CompareResult = System.String.Compare(
                        DataGridViewRow1.Cells["Plate"].Value.ToString(),
                        DataGridViewRow2.Cells["Plate"].Value.ToString());
                }
                return CompareResult * sortOrderModifier;
            }
        }

        public MainTab()
        {
            InitializeComponent();

            curve_color.Add(Color.Blue);
            curve_color.Add(Color.Red);
            curve_color.Add(Color.Green);
            curve_color.Add(Color.Black);
            curve_color.Add(Color.SaddleBrown);
            curve_color.Add(Color.OrangeRed);
            curve_color.Add(Color.DarkBlue);
            curve_color.Add(Color.DodgerBlue);
            curve_color.Add(Color.Tan);
            curve_color.Add(Color.DimGray);
        }

        public CPD_Tab f2;
        public Clustering_Tab f6;
        public DRC_Overlap_Tab f10;
        public ViewList_CPD_Tab f11;
        public ViewImages_Options_Tab f13;
        public CPD_Time_Line TimeLine;


        public void SetForm()
        {
            f2 = new CPD_Tab(this);
            f6 = new Clustering_Tab(this);
            f10 = new DRC_Overlap_Tab(this);
            f11 = new ViewList_CPD_Tab(this);
            TimeLine = new CPD_Time_Line(this);
        }

        public RawData_Tab f3 = new RawData_Tab();
        public RawDataDRC_Tab f4 = new RawDataDRC_Tab();
        public Export_Tab f5 = new Export_Tab();
        public Correlations_Tab f7 = new Correlations_Tab();

        public ViewCPD_Images_Tab f12 = new ViewCPD_Images_Tab();

        private string current_cpd_id;
        private Dictionary<string, int> cpd_row_index = new Dictionary<string, int>();
        public List<string> list_cpd;
        private int output_parameter_number;
        private int descritpor_number;
        private List<string> descriptor_list;

        private List<string> deslected_data_descriptor;
        private List<string> status_ec_50_descritpor;

        private string input_filename;
        private string output_filename;

        private Dictionary<string, List<Chart_DRC>> descriptors_chart = new Dictionary<string, List<Chart_DRC>>();
        private Dictionary<string, List<Chart_DRC_Overlap>> descriptors_chart_overlap = new Dictionary<string, List<Chart_DRC_Overlap>>();

        private string current_compound;
        private int compound_index_table;

        private Dictionary<string, List<double>> cpd_clustering = new Dictionary<string, List<double>>();

        public Dictionary<string, List<double>> get_cpd_clustering()
        {
            return cpd_clustering;
        }

        CachedCsvReader csv;

        private bool is_with_plate;
        //private bool is_with_exp;

        private Random rnd = new Random();

        List<List<string>> CPD_ID_List = new List<List<string>>();
        //List<List<int>> Exp_ID_List = new List<List<int>>();

        SortedDictionary<string, SortedDictionary<string, List<string>>> dict_plate_well_files = new SortedDictionary<string, SortedDictionary<string, List<string>>>();
        // plate, well path

        Dictionary<string, DataTable> data_dict = new Dictionary<string, DataTable>(); // file --> DataTable
        Dictionary<string, HashSet<string>> cpd_link = new Dictionary<string, HashSet<string>>(); // cpd id --> file
        List<string> time_line_selected_descriptors = new List<string>();

        Dictionary<string, Dictionary<string, Chart_DRC_Time_Line>> charts_time_line = new Dictionary<string, Dictionary<string, Chart_DRC_Time_Line>>(); // cpd_id, descriptor, chart

        public bool view_images_per_concentration;

        public int cpd_low_thr_ch1 = -1;
        public int cpd_low_thr_ch2 = -1;
        public int cpd_low_thr_ch3 = -1;
        public int cpd_low_thr_ch4 = -1;

        public int cpd_high_thr_ch1 = -1;
        public int cpd_high_thr_ch2 = -1;
        public int cpd_high_thr_ch3 = -1;
        public int cpd_high_thr_ch4 = -1;
        public int cpd_img_scale = -1;
        public int cpd_replicate = -1;
        public int cpd_color_format = -1;
        public int cpd_segm_method = -1;
        public bool set_param_cpd = false;

        List<Color> curve_color = new List<Color>();

        public int get_descriptors_number()
        {
            return descriptor_list.Count();
        }

        public int get_descriptors_number_time_line()
        {
            return time_line_selected_descriptors.Count();
        }

        public Dictionary<string, List<Chart_DRC>> get_descriptors_chart()
        {
            return descriptors_chart;
        }

        private void read_Data()
        {

            //f3.Show();
            f3.Hide();
            f3.dataGridView1.DataSource = csv;
            f4.dataGridView1.DataSource = csv;

            List<string> CPD_ID = new List<string>();
            deslected_data_descriptor = new List<string>();
            status_ec_50_descritpor = new List<string>();

            if (f3.dataGridView1.ColumnCount < 5 || !f3.dataGridView1.Columns.Contains("CPD_ID") || !f3.dataGridView1.Columns.Contains("Concentration")
                || !f3.dataGridView1.Columns.Contains("Plate") || !f3.dataGridView1.Columns.Contains("Well"))
            {
                System.Windows.Forms.MessageBox.Show("The file must contain at least these 5 columns : \n {[Plate, Well, Concentration, CPD_ID], Descr_0,...}", "Error",
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return;
            }

            //if (!f3.dataGridView1.Columns.Contains("CPD_ID"))
            //{
            //    System.Windows.Forms.MessageBox.Show("CPD_ID column doesn't exist.""Error",
            //        System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            //    return;
            //}

            //if (!f3.dataGridView1.Columns.Contains("Concentration"))
            //{
            //    System.Windows.Forms.MessageBox.Show("Concentration column doesn't exist.""Error",
            //        System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            //    return;
            //}

            foreach (DataGridViewRow row in f3.dataGridView1.Rows)
            {
                if (f3.dataGridView1.Columns.Contains("Plate") && is_with_plate == true)
                {
                    CPD_ID.Add(row.Cells["CPD_ID"].Value.ToString() + "_" + row.Cells["Plate"].Value.ToString());
                }
                else CPD_ID.Add(row.Cells["CPD_ID"].Value.ToString());
            }

            var unique_items = new HashSet<string>(CPD_ID);
            comboBox1.DataSource = unique_items.ToList<string>();

            foreach (DataGridViewColumn col in f3.dataGridView1.Columns)
            {
                string col_name = col.HeaderText;

                if (col_name != "Plate" && col_name != "Well" && col_name != "Concentration" && col_name != "Run"
                    && col_name != "CPD_ID" && col_name != "Class" && !col_name.StartsWith("Deselected") && col_name != "Status")
                {
                    checkedListBox1.Items.Add(col_name);
                }

                if (col_name.StartsWith("Deselected"))
                {
                    deslected_data_descriptor.Add(col_name);
                }

                if (col_name.StartsWith("Status"))
                {
                    status_ec_50_descritpor.Add(col_name);
                }
            }

            list_cpd = unique_items.ToList<string>();

        }

        private void Reset()
        {
            current_cpd_id = "";
            cpd_row_index.Clear();
            list_cpd = new List<string>();
            list_cpd.Clear();
            output_parameter_number = 0;
            descritpor_number = 0;
            descriptor_list = new List<string>();
            descriptor_list.Clear();

            //deslected_data_descriptor.Clear();
            deslected_data_descriptor = new List<string>();

            //status_ec_50_descritpor.Clear();
            status_ec_50_descritpor = new List<string>();

            CPD_ID_List.Clear();

            input_filename = "";
            output_filename = "";

            descriptors_chart.Clear();
            descriptors_chart_overlap.Clear();

            current_compound = "";
            compound_index_table = 0;

            checkedListBox1.Items.Clear();
        }

        private void Form_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Move;
            else
                e.Effect = DragDropEffects.None;
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            Reset();
            comboBox1.Visible = true;

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                string file = files[0];

                this.Text = file;

                TextReader tr = new StreamReader(file);

                if (files[0].Remove(0, files[0].Length - 4) == ".csv")
                {
                    csv = new CachedCsvReader(tr, true);

                    read_Data();
                }

            }
            return;
        }

        void comboBox1_SelectionChangeCommited(object sender, EventArgs e)
        {
            string CPD = comboBox1.SelectedItem.ToString();
            current_cpd_id = CPD;

            if (CPD == "DMSO" || CPD == "Untreated")
                return;

            tableLayoutPanel1.Controls.Clear();

            //int test_modified = 0;

            if (descriptors_chart.Count == 0) return;

            List<Chart_DRC> list_chart = descriptors_chart[current_cpd_id];
            foreach (Chart_DRC current_chart in list_chart)
            {
                current_chart.draw_DRC(false, true);
                //test_modified += Convert.ToInt32(current_chart.is_data_modified());
            }

            //int k = 0;
            //foreach (DataGridViewRow row2 in f2.dataGridView2.Rows)
            //{
            //    string compound = row2.Cells[0].Value.ToString();
            //    if (current_cpd_id == compound) break;
            //    k++;
            //}
            //int row_index = k;

            //if (test_modified > 0)
            //{
            //    f2.dataGridView2.Rows[row_index].DefaultCellStyle.BackColor = Color.LightSeaGreen;
            //}
            //else
            //{
            //    f2.dataGridView2.Rows[row_index].DefaultCellStyle.BackColor = Color.White;
            //}

        }

        public void dataGridView2_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= f2.dataGridView2.Rows.Count - 1) return;

            string CPD = f2.dataGridView2.Rows[e.RowIndex].Cells[0].Value.ToString();
            comboBox1.Text = CPD;

            if (CPD == "DMSO" || CPD == "Untreated")
                return;

            tableLayoutPanel1.Controls.Clear();

            List<Chart_DRC> list_chart = descriptors_chart[CPD];

            //tableLayoutPanel1.Controls.Clear();

            //int test_modified = 0;

            foreach (Chart_DRC current_chart in list_chart)
            {
                current_chart.draw_DRC(false, true);
                //test_modified += Convert.ToInt32(current_chart.is_data_modified());
            }

            //int k = 0;
            //foreach (DataGridViewRow row2 in f2.dataGridView2.Rows)
            //{
            //    string compound = row2.Cells[0].Value.ToString();
            //    if (current_cpd_id == compound) break;
            //    k++;
            //}
            //int row_index = k;

            //if (test_modified > 0)
            //{
            //    f2.dataGridView2.Rows[row_index].DefaultCellStyle.BackColor = Color.LightSeaGreen;
            //}
            //else
            //{
            //    f2.dataGridView2.Rows[row_index].DefaultCellStyle.BackColor = Color.White;
            //}

        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //check_last_points();

            exportDataToolStripMenuItem_Click(sender, e);

            if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string path = folderBrowserDialog1.SelectedPath;

                //DataGridView dataGridViewExport = new DataGridView();
                //this.Controls.Add(dataGridViewExport);

                f5.Text = this.Text;

                f5.dataGridViewExport.Rows.Clear();

                f5.dataGridViewExport.ColumnCount = 1 + 3 * descriptor_list.Count;

                f5.dataGridViewExport.Columns[0].Name = "CPD_ID";

                int i = 0;
                foreach (string elem in descriptor_list)
                {

                    DataGridViewImageColumn img = new DataGridViewImageColumn();
                    f5.dataGridViewExport.Columns.Insert(4 * i + 1, img);

                    i++;
                }

                i = 0;
                foreach (string elem in descriptor_list)
                {
                    f5.dataGridViewExport.Columns[4 * i + 1].Name = elem;
                    f5.dataGridViewExport.Columns[4 * i + 2].Name = "Estimation";
                    f5.dataGridViewExport.Columns[4 * i + 3].Name = "EC_50 " + elem;
                    f5.dataGridViewExport.Columns[4 * i + 4].Name = "Top " + elem;

                    i++;
                }
                toolStripProgressBar1.Visible = true;
                for (var idx = 0; idx < list_cpd.Count; idx++)
                {
                    toolStripProgressBar1.Value = idx * 100 / (list_cpd.Count - 1);
                    //toolStripStatusLabel1.Text = toolStripProgressBar1.Value.ToString();
                    //toolStripStatusLabel1.Visible=true;
                    string cpd_id = list_cpd[idx].ToString();

                    if (cpd_id.Contains("DMSO") || cpd_id.Contains("Untreated"))
                        continue;

                    tableLayoutPanel1.Controls.Clear();

                    List<Chart_DRC> list_chart = descriptors_chart[cpd_id];
                    List<string> list_images = new List<string>();

                    foreach (Chart_DRC current_chart in list_chart)
                    {
                        string image_path = current_chart.save_image(path);
                        list_images.Add(image_path);
                    }

                    // Export
                    //
                    // CPD_ID | Image Nuc. | EC_50 Nuc. Or Not Fitted Green/Red Cell | Image R | EC_50 R or Not Fitted Green/Red Cell | etc... 
                    //

                    int index = f5.dataGridViewExport.Rows.Add();
                    f5.dataGridViewExport.Rows[index].Cells[0].Value = cpd_id;

                    int i_img = 0;
                    foreach (Chart_DRC current_chart in list_chart)
                    {
                        double[] fit_params = current_chart.get_Fit_Parameters();
                        bool not_fitted = current_chart.is_Fitted();
                        bool inactive = current_chart.is_Inactive();
                        bool last_2_points_text = current_chart.check_ec50_exact();

                        double current_top = fit_params[1];
                        double current_ec_50 = fit_params[2];

                        Image image = Image.FromFile(list_images[i_img]);

                        //f5.dataGridViewExport.Rows[index].Height = 
                        f5.dataGridViewExport.Rows[index].Cells[i_img * 4 + 1].Value = image;
                        if (!not_fitted || !inactive)
                        {
                            if (last_2_points_text == true)
                            {
                                f5.dataGridViewExport.Rows[index].Cells[i_img * 4 + 2].Value = "=";
                                f5.dataGridViewExport.Rows[index].Cells[i_img * 4 + 2].Style.BackColor = Color.Green;

                                f5.dataGridViewExport.Rows[index].Cells[i_img * 4 + 3].Value = Math.Pow(10, current_ec_50).ToString("E2");
                                f5.dataGridViewExport.Rows[index].Cells[i_img * 4 + 3].Style.BackColor = Color.Green;

                                f5.dataGridViewExport.Rows[index].Cells[i_img * 4 + 4].Value = current_top.ToString("E2");
                                f5.dataGridViewExport.Rows[index].Cells[i_img * 4 + 4].Style.BackColor = Color.Green;
                            }
                            else
                            {
                                f5.dataGridViewExport.Rows[index].Cells[i_img * 4 + 2].Value = ">";
                                f5.dataGridViewExport.Rows[index].Cells[i_img * 4 + 2].Style.BackColor = Color.LimeGreen;

                                f5.dataGridViewExport.Rows[index].Cells[i_img * 4 + 3].Value = Math.Pow(10, current_ec_50).ToString("E2");
                                f5.dataGridViewExport.Rows[index].Cells[i_img * 4 + 3].Style.BackColor = Color.LimeGreen;

                                f5.dataGridViewExport.Rows[index].Cells[i_img * 4 + 4].Value = current_top.ToString("E2");
                                f5.dataGridViewExport.Rows[index].Cells[i_img * 4 + 4].Style.BackColor = Color.LimeGreen;
                            }
                        }
                        if (not_fitted)
                        {
                            f5.dataGridViewExport.Rows[index].Cells[i_img * 4 + 2].Value = "";
                            f5.dataGridViewExport.Rows[index].Cells[i_img * 4 + 2].Style.BackColor = Color.Tomato;

                            f5.dataGridViewExport.Rows[index].Cells[i_img * 4 + 3].Value = "Not Fitted";
                            f5.dataGridViewExport.Rows[index].Cells[i_img * 4 + 3].Style.BackColor = Color.Tomato;

                            f5.dataGridViewExport.Rows[index].Cells[i_img * 4 + 4].Value = "Not Fitted";
                            f5.dataGridViewExport.Rows[index].Cells[i_img * 4 + 4].Style.BackColor = Color.Tomato;
                        }
                        if (inactive)
                        {
                            f5.dataGridViewExport.Rows[index].Cells[i_img * 4 + 2].Value = "";
                            f5.dataGridViewExport.Rows[index].Cells[i_img * 4 + 2].Style.BackColor = Color.Orange;

                            f5.dataGridViewExport.Rows[index].Cells[i_img * 4 + 3].Value = "Inactive";
                            f5.dataGridViewExport.Rows[index].Cells[i_img * 4 + 3].Style.BackColor = Color.Orange;

                            f5.dataGridViewExport.Rows[index].Cells[i_img * 4 + 4].Value = "Inactive";
                            f5.dataGridViewExport.Rows[index].Cells[i_img * 4 + 4].Style.BackColor = Color.Orange;
                        }

                        i_img++;

                    }

                }

                toolStripProgressBar1.Visible = false;
                f5.Show();
                MessageBox.Show("Images generated.");
            }

        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            comboBox1.Visible = true;

            openFileDialog1.Filter = "CSV Files (*.csv)|*.csv";
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Reset();

                this.Text = openFileDialog1.FileName;

                System.IO.StreamReader sr = new System.IO.StreamReader(openFileDialog1.FileName);
                csv = new CachedCsvReader(sr, true);

                is_with_plate = false;

                read_Data();

            }

            return;
        }

        private void draw_drc()
        {
            descriptors_chart.Clear();

            if (f3.dataGridView1.RowCount < 1)
            {
                MessageBox.Show("Data is empty.");
                return;
            }

            SetForm();
            f2.Show();

            f2.Text = this.Text;

            //comboBox1.SelectionChanged += new SelectionChangedEventHandler(comboBox1_SelectionChanged);

            int checked_items = checkedListBox1.CheckedItems.Count;

            descritpor_number = checked_items;

            output_parameter_number = 5;

            f2.dataGridView2.ColumnCount = 2 + output_parameter_number * checked_items;

            f2.dataGridView2.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders;

            f2.dataGridView2.Columns[0].Name = "CPD_ID";

            descriptor_list = new List<string>();

            Dictionary<string, List<double>> data_descriptor = new Dictionary<string, List<double>>();
            Dictionary<string, List<string>> deselected_data_descriptor = new Dictionary<string, List<string>>();

            int j = 0;
            foreach (var item in checkedListBox1.CheckedItems)
            {
                f2.dataGridView2.Columns[j * output_parameter_number + 1].Name = "EC_50 " + item.ToString();
                f2.dataGridView2.Columns[j * output_parameter_number + 2].Name = "Bottom " + item.ToString();
                f2.dataGridView2.Columns[j * output_parameter_number + 3].Name = "Top " + item.ToString();
                f2.dataGridView2.Columns[j * output_parameter_number + 4].Name = "Slope " + item.ToString();
                f2.dataGridView2.Columns[j * output_parameter_number + 5].Name = "R2 " + item.ToString();

                //descriptor_dict.Add(j, item.ToString());
                descriptor_list.Add(item.ToString());

                data_descriptor[item.ToString()] = new List<double>();

                j++;
            }

            //ArrayList row_list = new ArrayList();

            for (var idx = 0; idx < list_cpd.Count; idx++)
            {

                f2.toolStripProgressBar1.Value = idx * 100 / (list_cpd.Count);
                string cpd_id = list_cpd[idx].ToString();

                if (cpd_id.Contains("DMSO") || cpd_id.Contains("Untreated"))
                    continue;

                // Add chart

                List<double> concentrations = new List<double>();
                List<double> concentrations_log = new List<double>();

                Dictionary<string, string> ec_50_status = new Dictionary<string, string>();

                List<DataGridViewRow> raw_data_rows = new List<DataGridViewRow>();

                data_descriptor.Clear();
                deselected_data_descriptor.Clear();

                foreach (DataGridViewRow row in f3.dataGridView1.Rows)
                {
                    string cpd_string = "";

                    if (is_with_plate == true) cpd_string = row.Cells["CPD_ID"].Value.ToString() + "_" + row.Cells["Plate"].Value.ToString();
                    else cpd_string = row.Cells["CPD_ID"].Value.ToString();

                    if (cpd_string == cpd_id)
                    {
                        raw_data_rows.Add(row);

                        foreach (var item in checkedListBox1.CheckedItems)
                        {
                            string descriptor_name = item.ToString();
                            if (data_descriptor.ContainsKey(descriptor_name))
                            {
                                data_descriptor[descriptor_name].Add(double.Parse(row.Cells[item.ToString()].Value.ToString()));
                            }
                            else
                            {
                                data_descriptor[descriptor_name] = new List<double>();
                                data_descriptor[descriptor_name].Add(double.Parse(row.Cells[item.ToString()].Value.ToString()));
                            }

                        }

                        foreach (string item in deslected_data_descriptor)
                        {
                            string name = item.ToString();
                            string descriptor_name = name.Remove(0, 11);
                            if (deselected_data_descriptor.ContainsKey(descriptor_name))
                            {
                                deselected_data_descriptor[descriptor_name].Add(row.Cells[item.ToString()].Value.ToString());
                            }
                            else
                            {
                                deselected_data_descriptor[descriptor_name] = new List<string>();
                                deselected_data_descriptor[descriptor_name].Add(row.Cells[item.ToString()].Value.ToString());
                            }
                        }

                        foreach (string item in status_ec_50_descritpor)
                        {
                            string name = item.ToString();
                            string descriptor_name = name.Remove(0, 7);
                            ec_50_status[descriptor_name] = row.Cells["Status_" + descriptor_name].Value.ToString();
                        }

                        concentrations.Add(double.Parse(row.Cells["Concentration"].Value.ToString()));
                        concentrations_log.Add(Math.Log10(double.Parse(row.Cells["Concentration"].Value.ToString())));
                    }
                }

                // Loop descriptors :

                List<Chart_DRC> list_DRC_cpd = new List<Chart_DRC>();
                List<double> row_params = new List<double>();

                Color my_color;
                foreach (KeyValuePair<string, List<double>> item in data_descriptor)
                {
                    string descriptor_name = item.Key;
                    if (descriptor_name == "Nuclei")
                    {
                        my_color = Color.Blue;
                        curve_color.RemoveAll(x => x == my_color);
                    }
                    if (descriptor_name == "R/N" || descriptor_name == "R")
                    {
                        my_color = Color.Red;
                        curve_color.RemoveAll(x => x == my_color);
                    }
                    if (descriptor_name == "G/N" || descriptor_name == "G")
                    {
                        my_color = Color.Green;
                        curve_color.RemoveAll(x => x == my_color);
                    }
                    if (descriptor_name == "LDA_1")
                    {
                        my_color = Color.Black;
                        curve_color.RemoveAll(x => x == my_color);
                    }
                }

                int descriptor_index = 0;
                foreach (KeyValuePair<string, List<double>> item in data_descriptor)
                {
                    string descriptor_name = item.Key;
                    List<double> data = item.Value;

                    //List<Color> myColors = typeof(Color).GetProperties(BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.Public)
                    //     .Select(c => (Color)c.GetValue(null, null))
                    //     .ToList();

                    //Color color = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
                    //Color color = myColors[descriptor_index];

                    bool color_used = false;

                    Color color = Color.Blue;

                    if (descriptor_name == "Nuclei")
                    {
                        color = Color.Blue;
                        color_used = true;
                    }
                    if (descriptor_name == "R/N" || descriptor_name == "R")
                    {
                        color = Color.Red;
                        color_used = true;
                    }
                    if (descriptor_name == "G/N" || descriptor_name == "G")
                    {
                        color = Color.Green;
                        color_used = true;
                    }
                    if (descriptor_name == "LDA_1")
                    {
                        color = Color.Black;
                        color_used = true;
                    }

                    if (descriptor_index < curve_color.Count && !color_used)
                    {
                        if (descriptor_index == 0) color = curve_color[0];
                        else if (descriptor_index == 1) color = curve_color[1];
                        else if (descriptor_index == 2) color = curve_color[2];
                        else if (descriptor_index == 3) color = curve_color[3];
                        else if (descriptor_index == 4) color = curve_color[4];
                        else if (descriptor_index == 5) color = curve_color[5];
                        else if (descriptor_index == 6) color = curve_color[6];
                        else if (descriptor_index == 7) color = curve_color[7];
                        else if (descriptor_index == 8) color = curve_color[8];
                        else if (descriptor_index == 9) color = curve_color[9];
                    }

                    List<string> deselected = new List<string>();
                    if (deselected_data_descriptor.ContainsKey(descriptor_name)) deselected = deselected_data_descriptor[descriptor_name];

                    string chart_ec_50_status;
                    if (ec_50_status.ContainsKey(descriptor_name)) chart_ec_50_status = ec_50_status[descriptor_name];
                    else chart_ec_50_status = "=";

                    Chart_DRC chart_drc = new Chart_DRC(cpd_id, descriptor_name, 100, ref concentrations, ref concentrations_log, ref data, color,
                        descriptor_index, deselected, chart_ec_50_status, this);

                    chart_drc.set_Raw_Data(raw_data_rows);

                    double[] parameters = chart_drc.get_Fit_Parameters();
                    double r2 = chart_drc.get_R2();

                    list_DRC_cpd.Add(chart_drc);

                    row_params.Add(double.Parse(Math.Pow(10, parameters[2]).ToString("E2")));
                    if (parameters[1] > parameters[0])
                    {
                        row_params.Add(double.Parse(parameters[0].ToString("E2")));
                        row_params.Add(double.Parse(parameters[1].ToString("E2")));
                    }
                    else
                    {
                        row_params.Add(double.Parse(parameters[1].ToString("E2")));
                        row_params.Add(double.Parse(parameters[0].ToString("E2")));
                    }
                    row_params.Add(double.Parse(parameters[3].ToString("E2")));
                    row_params.Add(double.Parse(r2.ToString("E2")));


                    descriptor_index++;
                }

                descriptors_chart.Add(cpd_id, list_DRC_cpd);

                DataGridViewRow current_row = (DataGridViewRow)f2.dataGridView2.Rows[0].Clone();

                for (int i = 0; i < row_params.Count() + 1; i++)
                {
                    if (i == 0) current_row.Cells[i].Value = cpd_id;
                    if (i > 0) current_row.Cells[i].Value = row_params[i - 1];
                }

                f2.dataGridView2.Rows.Add(current_row);

                foreach (Chart_DRC chart_iteam in list_DRC_cpd)
                {
                    chart_iteam.Is_Modified();
                }

            }

            f2.toolStripProgressBar1.Visible = false;

        }

        private void drawDRCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            draw_drc();
        }

        private void exportDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string filename = saveFileDialog1.FileName;

                DataGridView dataGridView4 = new DataGridView();
                dataGridView4.ColumnCount = f3.dataGridView1.ColumnCount; // + descritpor_number;

                int col_index = 0;
                foreach (DataGridViewColumn col in f3.dataGridView1.Columns)
                {
                    dataGridView4.Columns[col_index].Name = col.Name;
                    col_index++;
                }

                //int col_already_present = 0;
                //int new_columns = 0;

                for (int descriptor_index = 0; descriptor_index < descritpor_number; descriptor_index++)
                {
                    string column_name = "Deselected_" + descriptor_list[descriptor_index];

                    if (f3.dataGridView1.Columns.Contains(column_name))
                    {
                        foreach (DataGridViewRow myRow in f3.dataGridView1.Rows)
                        {
                            myRow.Cells[column_name].Value = null;
                        }

                        //col_already_present++;
                        f3.dataGridView1.Columns.Remove(f3.dataGridView1.Columns[column_name]);
                        //dataGridView4.ColumnCount -= 1;
                    }
                    else
                    {
                        dataGridView4.ColumnCount += 1;
                        dataGridView4.Columns[col_index].Name = column_name;
                        //new_columns++;
                        col_index++;
                    }

                    //int content = dataGridView4.ColumnCount;
                }

                for (int descriptor_index = 0; descriptor_index < descritpor_number; descriptor_index++)
                {
                    string column_name = "Status_" + descriptor_list[descriptor_index];

                    if (f3.dataGridView1.Columns.Contains(column_name))
                    {
                        foreach (DataGridViewRow myRow in f3.dataGridView1.Rows)
                        {
                            myRow.Cells[column_name].Value = null;
                        }

                        f3.dataGridView1.Columns.Remove(f3.dataGridView1.Columns[column_name]);
                    }
                    else
                    {
                        dataGridView4.ColumnCount += 1;
                        dataGridView4.Columns[col_index].Name = column_name;
                        col_index++;
                    }

                }

                for (var idx = 0; idx < list_cpd.Count; idx++)
                {
                    string cpd_id = list_cpd[idx].ToString();

                    if (cpd_id.Contains("DMSO") || cpd_id.Contains("Untreated"))
                        continue;

                    List<Chart_DRC> list_chart = descriptors_chart[cpd_id];

                    List<DataGridViewRow> raw_data_cpd;

                    raw_data_cpd = list_chart[0].get_Raw_Data().ToList();

                    Dictionary<int, DataGridViewRow> chart_row_data = new Dictionary<int, DataGridViewRow>();

                    int j = 0;

                    foreach (DataGridViewRow item in raw_data_cpd)
                    {
                        DataGridViewRow current_row = (DataGridViewRow)raw_data_cpd[j].Clone();

                        for (int index = 0; index < raw_data_cpd[j].Cells.Count; index++)
                        {
                            current_row.Cells[index].Value = raw_data_cpd[j].Cells[index].Value;
                        }

                        chart_row_data.Add(j, current_row);
                        j++;
                    }


                    foreach (Chart_DRC current_chart in list_chart)
                    {

                        string descriptor_name = current_chart.get_Descriptor_Name();

                        List<bool> removed_raw_data_cpd = new List<bool>();

                        removed_raw_data_cpd = current_chart.get_Removed_Raw_Data().ToList();
                        bool not_fitted = current_chart.is_Fitted();
                        bool inactive = current_chart.is_Inactive();

                        int k = 0;

                        foreach (bool elem in removed_raw_data_cpd)
                        {

                            DataGridViewTextBoxCell newCell = new DataGridViewTextBoxCell();
                            if (!not_fitted || !inactive) newCell.Value = Convert.ToString(elem);
                            if (not_fitted) newCell.Value = "Not Fitted";
                            if (inactive) newCell.Value = "Inactive";

                            chart_row_data[k].Cells.Add(newCell);

                            ++k;
                        }

                    }

                    foreach (Chart_DRC current_chart in list_chart)
                    {
                        string descriptor_name = current_chart.get_Descriptor_Name();

                        List<bool> removed_raw_data_cpd = new List<bool>();

                        removed_raw_data_cpd = current_chart.get_Removed_Raw_Data().ToList();
                        bool status_ec_50 = current_chart.check_ec50_exact();

                        int k = 0;
                        foreach (bool elem in removed_raw_data_cpd)
                        {

                            DataGridViewTextBoxCell newCell = new DataGridViewTextBoxCell();
                            if (status_ec_50) newCell.Value = "=";
                            else newCell.Value = ">";

                            chart_row_data[k].Cells.Add(newCell);

                            ++k;
                        }
                    }

                    foreach (KeyValuePair<int, DataGridViewRow> item in chart_row_data)
                    {
                        dataGridView4.Rows.Add(chart_row_data[item.Key]);
                    }

                }

                int columnCount = dataGridView4.ColumnCount; // - 2*col_already_present + new_columns;
                string columnNames = "";

                string[] output = new string[dataGridView4.RowCount + 1];
                for (int i = 0; i < columnCount; i++)
                {
                    if (i < columnCount - 1) columnNames += dataGridView4.Columns[i].Name.ToString() + ",";
                    if (i == columnCount - 1) columnNames += dataGridView4.Columns[i].Name.ToString();
                }
                output[0] += columnNames;
                for (int i = 1; (i - 1) < dataGridView4.RowCount; i++)
                {
                    if (dataGridView4.Rows[i - 1].Cells[0].Value == null) continue;

                    for (int j = 0; j < columnCount; j++)
                    {
                        //System.Diagnostics.Debug.WriteLine("Index, Write = " + i.ToString() + "-->" + dataGridView4.Rows[i - 1].Cells[j].Value.ToString() + ",");
                        if (j < columnCount - 1) output[i] += dataGridView4.Rows[i - 1].Cells[j].Value.ToString() + ",";
                        if (j == columnCount - 1) output[i] += dataGridView4.Rows[i - 1].Cells[j].Value.ToString();
                    }
                }
                System.IO.File.WriteAllLines(saveFileDialog1.FileName, output);
                MessageBox.Show("File was generated.");

            }
        }

        private void rawDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            f4.Show();
            f4.Text = this.Text;
        }

        private void loadWithPlateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "CSV Files (*.csv)|*.csv";
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Reset();

                this.Text = openFileDialog1.FileName;

                System.IO.StreamReader sr = new System.IO.StreamReader(openFileDialog1.FileName);
                csv = new CachedCsvReader(sr, true);

                is_with_plate = true;

                read_Data();
            }

            return;
        }

        private void correlationsToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        //-----------------------------------------------------------------------------------------------//
        //-------------------------------------- Load Overlap -------------------------------------------//
        //-----------------------------------------------------------------------------------------------//

        public void dataGridView3_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            string CPD = f10.dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString();

            if (CPD == "DMSO" || CPD == "Untreated")
                return;

            tableLayoutPanel1.Controls.Clear();

            List<Chart_DRC_Overlap> list_chart_overlap = descriptors_chart_overlap[CPD];

            foreach (Chart_DRC_Overlap current_chart_overlap in list_chart_overlap)
            {
                current_chart_overlap.draw_DRC();
            }
        }

        private void pCAToolStripMenuItem_Click(object sender, EventArgs e)
        {

            // Get the data from the table :
            //double[][] dataValues = new double[f2.dataGridView2.Rows.Count-1][];
            List<double[]> dataValues = new List<double[]>();

            cpd_clustering.Clear();
            cpd_clustering = new Dictionary<string, List<double>>();

            List<string> cpd_name = new List<string>();

            foreach (DataGridViewRow row in f2.dataGridView2.Rows)
            {
                int i = row.Index;

                List<double> current_row = new List<double>();

                for (int j = 0; j < f2.dataGridView2.Columns.Count - 1; j++)
                {
                    if (j != 0 && j < f2.dataGridView2.Columns.Count && i < f2.dataGridView2.Rows.Count - 1 && !(f2.dataGridView2.Columns[j].Name.Contains("R2")))
                    {
                        if (f2.dataGridView2.Rows[i].Cells[j].Value != "Not Fitted" || f2.dataGridView2.Rows[i].Cells[j].Value != "Inactive")
                        {
                            current_row.Add((double)f2.dataGridView2.Rows[i].Cells[j].Value);
                        }
                        else
                        {
                            current_row.Add(-1);
                        }
                    }
                }

                if (i < f2.dataGridView2.Rows.Count - 1)
                {
                    if (current_row.Count == descriptor_list.Count * 4)
                    {
                        dataValues.Add(current_row.ToArray());
                        cpd_name.Add(f2.dataGridView2.Rows[i].Cells[0].Value.ToString());
                    }
                }
            }

            // PCA :

            PrincipalComponentAnalysis PCA = new PrincipalComponentAnalysis()
            {
                Method = PrincipalComponentMethod.Center,
                Whiten = true
            };

            MultivariateLinearRegression transform = PCA.Learn(dataValues.ToArray());

            // NumberOfOutputs to the desired components:
            PCA.NumberOfOutputs = 2;
            double[][] output_pca = PCA.Transform(dataValues.ToArray());


            // Fill output Lists: 

            List<double> X_tranform_PCA = new List<double>();
            List<double> Y_tranform_PCA = new List<double>();

            for (int k = 0; k < output_pca.GetLength(0); k++)
            {
                X_tranform_PCA.Add(output_pca[k][0]);
                Y_tranform_PCA.Add(output_pca[k][1]);

                List<double> point = new List<double>();
                point.Add(output_pca[k][0]);
                point.Add(output_pca[k][1]);

                cpd_clustering[cpd_name[k]] = point;
            }

            f6.chart1.Titles["Title1"].Text = "PCA";

            f6.chart1.Series["Series1"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
            // Reset Chart
            f6.chart1.Series["Series1"].Points.Clear();

            f6.chart1.Series["Series1"].Points.DataBindXY(X_tranform_PCA, Y_tranform_PCA);
            //f6.chart1.Series["Series1"].Points.DataBindXY(X_tranform_PCA, Y_tranform_PCA);

            f6.chart1.Series["Series1"].Color = Color.Red;

            f6.Show();

        }

        private void tSNEToolStripMenuItem_Click(object sender, EventArgs e)
        {

            // Get the data from the table :
            //double[][] dataValues = new double[f2.dataGridView2.Rows.Count-1][];
            List<double[]> dataValues = new List<double[]>();

            cpd_clustering.Clear();
            cpd_clustering = new Dictionary<string, List<double>>();

            List<string> cpd_name = new List<string>();

            foreach (DataGridViewRow row in f2.dataGridView2.Rows)
            {
                int i = row.Index;

                //double[] current_row = new double[f2.dataGridView2.Columns.Count-2-descriptor_list.Count];
                List<double> current_row = new List<double>();

                //foreach (DataGridViewColumn col in f2.dataGridView2.Columns)
                //{
                //    int j = col.Index;
                //    if(j!=0 && j<f2.dataGridView2.Columns.Count) current_row[j] = (double)f2.dataGridView2.Rows[i].Cells[j].Value;
                //}

                for (int j = 0; j < f2.dataGridView2.Columns.Count - 1; j++)
                {
                    if (j != 0 && j < f2.dataGridView2.Columns.Count && i < f2.dataGridView2.Rows.Count - 1 && !(f2.dataGridView2.Columns[j].Name.Contains("R2")))
                    {
                        if (f2.dataGridView2.Rows[i].Cells[j].Value != "Not Fitted" || f2.dataGridView2.Rows[i].Cells[j].Value != "Inactive") current_row.Add((double)f2.dataGridView2.Rows[i].Cells[j].Value);
                        else current_row.Add(-1);
                    }
                }

                if (i < f2.dataGridView2.Rows.Count - 1)
                {
                    if (current_row.Count == descriptor_list.Count * 4)
                    {
                        dataValues.Add(current_row.ToArray());
                        cpd_name.Add(f2.dataGridView2.Rows[i].Cells[0].Value.ToString());
                    }
                }
            }

            // T-SNE :

            Accord.Math.Random.Generator.Seed = 0;
            TSNE tSNE = new TSNE()
            {
                NumberOfOutputs = 2,
                Perplexity = 5,
                Theta = 0.5
            };

            double[][] output_tsne = tSNE.Transform(dataValues.ToArray());

            List<double> X_tranform_TSNE = new List<double>();
            List<double> Y_tranform_TSNE = new List<double>();

            for (int k = 0; k < output_tsne.GetLength(0); k++)
            {
                X_tranform_TSNE.Add(output_tsne[k][0]);
                Y_tranform_TSNE.Add(output_tsne[k][1]);

                List<double> point = new List<double>();
                point.Add(output_tsne[k][0]);
                point.Add(output_tsne[k][1]);

                cpd_clustering[cpd_name[k]] = point;
            }

            //f6.chart1.ChartAreas.Add(chartArea);
            f6.chart1.Titles["Title1"].Text = "T-SNE";

            //series1.ChartType = SeriesChartType.Point;
            //series1.MarkerStyle = MarkerStyle.Circle;

            //series1.Name = "Series1";
            //f6.chart1.Series.Add(series1);

            f6.chart1.Series["Series1"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
            // Reset Chart
            f6.chart1.Series["Series1"].Points.Clear();

            f6.chart1.Series["Series1"].Points.DataBindXY(X_tranform_TSNE, Y_tranform_TSNE);
            //f6.chart1.Series["Series1"].Points.DataBindXY(X_tranform_PCA, Y_tranform_PCA);

            f6.chart1.Series["Series1"].Color = Color.Red;

            f6.Show();

        }

        private void correlationsToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            Form fc = Application.OpenForms["Correlations_Tab"];

            if (fc == null)
                f7 = new Correlations_Tab();

            f7.Show();
        }

        private void loadCurvesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Overlap DRC :

            Reset();
            comboBox1.Visible = false;

            openFileDialog1.Filter = "CSV Files (*.csv)|*.csv";

            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.Text = openFileDialog1.FileName;

                System.IO.StreamReader sr = new System.IO.StreamReader(openFileDialog1.FileName);
                csv = new CachedCsvReader(sr, true);

                read_Data_Exp();
            }

            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.Text = openFileDialog1.FileName;

                System.IO.StreamReader sr = new System.IO.StreamReader(openFileDialog1.FileName);
                csv = new CachedCsvReader(sr, true);

                read_Data_Exp();
            }

            return;
        }

        private void read_Data_Exp()
        {
            f3.Hide();

            if (f3.dataGridView1.Rows.Count < 1)
            {
                f3.dataGridView1.DataSource = csv;

                CPD_ID_List.Clear();

                List<List<string>> deslected_data_descriptor_list = new List<List<string>>();

                if (f3.dataGridView1.ColumnCount < 5)
                {
                    System.Windows.Forms.MessageBox.Show("The file should contain at least 5 columns\n Plate,Well,Concentration,CPD_ID,Descr_0,...");
                    return;
                }

                if (!f3.dataGridView1.Columns.Contains("CPD_ID"))
                {
                    System.Windows.Forms.MessageBox.Show("CPD_ID column doesn't exist.");
                    return;
                }

                if (!f3.dataGridView1.Columns.Contains("Concentration"))
                {
                    System.Windows.Forms.MessageBox.Show("Concentration column doesn't exist.");
                    return;
                }

                List<string> CPD_ID = new List<string>();

                foreach (DataGridViewRow row in f3.dataGridView1.Rows)
                {
                    CPD_ID.Add(row.Cells["CPD_ID"].Value.ToString());
                }

                CPD_ID_List.Add(CPD_ID);

                // Features checkbox dataGridView1 :
                foreach (DataGridViewColumn col in f3.dataGridView1.Columns)
                {
                    string col_name = col.HeaderText;
                    if (col_name != "Plate" && col_name != "Well" && col_name != "Concentration" && col_name != "Run" && col_name != "CPD_ID" && col_name != "Class" && !col_name.StartsWith("Deselected"))
                    {
                        checkedListBox1.Items.Add(col_name);
                    }
                    if (col_name.StartsWith("Deselected"))
                    {
                        deslected_data_descriptor.Add(col_name);
                    }
                }

            }
            else
            {
                f3.dataGridView2.DataSource = csv;

                List<string> CPD_ID_2 = new List<string>();

                foreach (DataGridViewRow row in f3.dataGridView2.Rows)
                {
                    CPD_ID_2.Add(row.Cells["CPD_ID"].Value.ToString());
                }

                CPD_ID_List.Add(CPD_ID_2);
            }

            if (CPD_ID_List.Count == 2)
            {
                var unique_items_1 = new HashSet<string>(CPD_ID_List[0]);
                var unique_items_2 = new HashSet<string>(CPD_ID_List[1]);

                var unique_cpd_id = unique_items_1.Intersect(unique_items_2);

                comboBox1.DataSource = unique_cpd_id.ToList<string>();
                list_cpd = unique_cpd_id.ToList<string>();
            }

        }

        private void drawCurvesToolStripMenuItem_Click(object sender, EventArgs e)
        {

            descriptors_chart.Clear();

            if (f3.dataGridView1.RowCount < 1)
            {
                MessageBox.Show("Data 1 is empty.");
                return;
            }

            if (f3.dataGridView2.RowCount < 1)
            {
                MessageBox.Show("Data 2 is empty.");
                return;
            }

            SetForm();
            //f3.Show();
            f10.Show();

            f10.Text = this.Text;

            //comboBox1.SelectionChanged += new SelectionChangedEventHandler(comboBox1_SelectionChanged);

            int checked_items = checkedListBox1.CheckedItems.Count;

            descritpor_number = checked_items;

            output_parameter_number = 5;

            f10.dataGridView1.ColumnCount = 1 + CPD_ID_List.Count() * output_parameter_number * checked_items;

            f10.dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders;

            f10.dataGridView1.Columns[0].Name = "CPD_ID";

            descriptor_list = new List<string>();

            Dictionary<string, List<double>> data_descriptor_1 = new Dictionary<string, List<double>>();
            Dictionary<string, List<double>> data_descriptor_2 = new Dictionary<string, List<double>>();


            //Dictionary<string, List<string>> deselected_data_descriptor = new Dictionary<string, List<string>>();


            int j = 0;

            foreach (var item in checkedListBox1.CheckedItems)
            {
                for (int k = 0; k < CPD_ID_List.Count; ++k)
                {
                    f10.dataGridView1.Columns[k + 1 + j * output_parameter_number * 2].Name = "EC_50 " + item.ToString() + "  " + k.ToString();
                    f10.dataGridView1.Columns[k + 3 + j * output_parameter_number * 2].Name = "Bottom " + item.ToString() + "  " + k.ToString();
                    f10.dataGridView1.Columns[k + 5 + j * output_parameter_number * 2].Name = "Top " + item.ToString() + "  " + k.ToString();
                    f10.dataGridView1.Columns[k + 7 + j * output_parameter_number * 2].Name = "Slope " + item.ToString() + "  " + k.ToString();
                    f10.dataGridView1.Columns[k + 9 + j * output_parameter_number * 2].Name = "R2 " + item.ToString() + "  " + k.ToString();

                    //descriptor_dict.Add(j, item.ToString());
                    if (k == 0) descriptor_list.Add(item.ToString());

                    data_descriptor_1[item.ToString()] = new List<double>();
                    data_descriptor_2[item.ToString()] = new List<double>();
                }
                j++;
            }

            for (var idx = 0; idx < list_cpd.Count; idx++)
            {
                string cpd_id = list_cpd[idx].ToString();

                if (cpd_id.Contains("DMSO") || cpd_id.Contains("Untreated"))
                    continue;

                // Add chart

                Dictionary<string, List<double>> concentrations_1 = new Dictionary<string, List<double>>();
                Dictionary<string, List<double>> concentrations_log_1 = new Dictionary<string, List<double>>();

                Dictionary<string, List<double>> concentrations_2 = new Dictionary<string, List<double>>();
                Dictionary<string, List<double>> concentrations_log_2 = new Dictionary<string, List<double>>();

                //List<DataGridViewRow> raw_data_rows = new List<DataGridViewRow>();

                data_descriptor_1.Clear();
                data_descriptor_2.Clear();

                //deselected_data_descriptor.Clear();

                foreach (DataGridViewRow row in f3.dataGridView1.Rows)
                {
                    string cpd_string = "";

                    cpd_string = row.Cells["CPD_ID"].Value.ToString();

                    if (cpd_string == cpd_id)
                    {
                        //raw_data_rows.Add(row);

                        foreach (string item in checkedListBox1.CheckedItems)
                        {
                            string col_deselected = item;
                            foreach (string col in deslected_data_descriptor)
                            {
                                if (col.Contains(item)) col_deselected = col;
                            }


                            //if (row.Cells[col_deselected].Value.ToString() != "Not Fitted" && row.Cells[col_deselected].Value.ToString() != "True")
                            {
                                string descriptor_name = item.ToString();
                                if (data_descriptor_1.ContainsKey(descriptor_name))
                                {
                                    data_descriptor_1[descriptor_name].Add(double.Parse(row.Cells[descriptor_name].Value.ToString()));
                                }
                                else
                                {
                                    data_descriptor_1[descriptor_name] = new List<double>();
                                    data_descriptor_1[descriptor_name].Add(double.Parse(row.Cells[descriptor_name].Value.ToString()));
                                }

                                if (concentrations_1.ContainsKey(descriptor_name))
                                {
                                    concentrations_1[descriptor_name].Add(double.Parse(row.Cells["Concentration"].Value.ToString()));
                                }
                                else
                                {
                                    concentrations_1[descriptor_name] = new List<double>();
                                    concentrations_1[descriptor_name].Add(double.Parse(row.Cells["Concentration"].Value.ToString()));
                                }

                                if (concentrations_log_1.ContainsKey(descriptor_name))
                                {
                                    concentrations_log_1[descriptor_name].Add(Math.Log10(double.Parse(row.Cells["Concentration"].Value.ToString())));
                                }
                                else
                                {
                                    concentrations_log_1[descriptor_name] = new List<double>();
                                    concentrations_log_1[descriptor_name].Add(Math.Log10(double.Parse(row.Cells["Concentration"].Value.ToString())));
                                }
                            }
                        }

                    }
                }

                foreach (DataGridViewRow row in f3.dataGridView2.Rows)
                {
                    string cpd_string = "";

                    cpd_string = row.Cells["CPD_ID"].Value.ToString();

                    if (cpd_string == cpd_id)
                    {
                        //raw_data_rows.Add(row);

                        foreach (string item in checkedListBox1.CheckedItems)
                        {
                            string col_deselected = item;
                            foreach (string col in deslected_data_descriptor)
                            {
                                if (col.Contains(item)) col_deselected = col;
                            }

                            //if (row.Cells[col_deselected].Value.ToString() != "Not Fitted" && row.Cells[col_deselected].Value.ToString() != "True")
                            {
                                string descriptor_name = item.ToString();
                                if (data_descriptor_2.ContainsKey(descriptor_name))
                                {
                                    data_descriptor_2[descriptor_name].Add(double.Parse(row.Cells[descriptor_name].Value.ToString()));
                                }
                                else
                                {
                                    data_descriptor_2[descriptor_name] = new List<double>();
                                    data_descriptor_2[descriptor_name].Add(double.Parse(row.Cells[descriptor_name].Value.ToString()));
                                }

                                if (concentrations_2.ContainsKey(descriptor_name))
                                {
                                    concentrations_2[descriptor_name].Add(double.Parse(row.Cells["Concentration"].Value.ToString()));
                                }
                                else
                                {
                                    concentrations_2[descriptor_name] = new List<double>();
                                    concentrations_2[descriptor_name].Add(double.Parse(row.Cells["Concentration"].Value.ToString()));
                                }

                                if (concentrations_log_2.ContainsKey(descriptor_name))
                                {
                                    concentrations_log_2[descriptor_name].Add(Math.Log10(double.Parse(row.Cells["Concentration"].Value.ToString())));
                                }
                                else
                                {
                                    concentrations_log_2[descriptor_name] = new List<double>();
                                    concentrations_log_2[descriptor_name].Add(Math.Log10(double.Parse(row.Cells["Concentration"].Value.ToString())));
                                }
                            }
                        }

                    }
                }

                // Loop descriptors :

                List<Chart_DRC_Overlap> list_DRC_cpd = new List<Chart_DRC_Overlap>();
                List<double> row_params = new List<double>();

                Color my_color;
                foreach (string item in checkedListBox1.CheckedItems)
                {
                    string descriptor_name = item;
                    if (descriptor_name == "Nuclei")
                    {
                        my_color = Color.Blue;
                        curve_color.RemoveAll(x => x == my_color);
                    }
                    if (descriptor_name == "R/N" || descriptor_name == "R")
                    {
                        my_color = Color.Red;
                        curve_color.RemoveAll(x => x == my_color);
                    }
                    if (descriptor_name == "G/N" || descriptor_name == "G")
                    {
                        my_color = Color.Green;
                        curve_color.RemoveAll(x => x == my_color);
                    }
                    if (descriptor_name == "LDA_1")
                    {
                        my_color = Color.Black;
                        curve_color.RemoveAll(x => x == my_color);
                    }
                }

                int descriptor_index = 0;
                foreach (string item in checkedListBox1.CheckedItems)
                {
                    string descriptor_name = item;
                    List<double> data_1 = new List<double>();
                    List<double> data_2 = new List<double>();

                    if (data_descriptor_1.ContainsKey(descriptor_name)) data_1 = data_descriptor_1[descriptor_name];
                    if (data_descriptor_2.ContainsKey(descriptor_name)) data_2 = data_descriptor_2[descriptor_name];

                    if (data_1.Count > 0 && data_2.Count > 0)
                    {
                        //List<Color> myColors = typeof(Color).GetProperties(BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.Public)
                        //                        .Select(c => (Color)c.GetValue(null, null))
                        //                        .ToList();

                        //Color color = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
                        //Color color = myColors[descriptor_index];
                        bool color_used = false;

                        Color color = Color.Blue;

                        if (descriptor_name == "Nuclei")
                        {
                            color = Color.Blue;
                            color_used = true;
                        }
                        if (descriptor_name == "R/N" || descriptor_name == "R")
                        {
                            color = Color.Red;
                            color_used = true;
                        }
                        if (descriptor_name == "G/N" || descriptor_name == "G")
                        {
                            color = Color.Green;
                            color_used = true;
                        }
                        if (descriptor_name == "LDA_1")
                        {
                            color = Color.Black;
                            color_used = true;
                        }

                        if (descriptor_index < curve_color.Count && !color_used)
                        {
                            if (descriptor_index == 0) color = curve_color[0];
                            else if (descriptor_index == 1) color = curve_color[1];
                            else if (descriptor_index == 2) color = curve_color[2];
                            else if (descriptor_index == 3) color = curve_color[3];
                            else if (descriptor_index == 4) color = curve_color[4];
                            else if (descriptor_index == 5) color = curve_color[5];
                            else if (descriptor_index == 6) color = curve_color[6];
                            else if (descriptor_index == 7) color = curve_color[7];
                            else if (descriptor_index == 8) color = curve_color[8];
                            else if (descriptor_index == 9) color = curve_color[9];
                        }


                        //List<string> deselected = new List<string>();
                        //if (deselected_data_descriptor.ContainsKey(descriptor_name)) deselected = deselected_data_descriptor[descriptor_name];
                        List<double> conc_1 = concentrations_1[descriptor_name];
                        List<double> conc_2 = concentrations_2[descriptor_name];

                        List<double> conc_1_log = concentrations_log_1[descriptor_name];
                        List<double> conc_2_log = concentrations_log_2[descriptor_name];

                        Chart_DRC_Overlap chart_drc_overlap = new Chart_DRC_Overlap(cpd_id, descriptor_name, 100, ref conc_1, ref conc_1_log,
                        ref conc_2, ref conc_2_log, ref data_1, ref data_2, color, descriptor_index, this);

                        double[] parameters_1 = chart_drc_overlap.get_Fit_Parameters_1();
                        double[] parameters_2 = chart_drc_overlap.get_Fit_Parameters_2();

                        double r2_1 = chart_drc_overlap.get_R2_1();
                        double r2_2 = chart_drc_overlap.get_R2_2();

                        list_DRC_cpd.Add(chart_drc_overlap);

                        row_params.Add(double.Parse(Math.Pow(10, parameters_1[2]).ToString("E2")));
                        row_params.Add(double.Parse(Math.Pow(10, parameters_2[2]).ToString("E2")));

                        double bottom_1 = 0;
                        double bottom_2 = 0;

                        double top_1 = 0;
                        double top_2 = 0;

                        if (parameters_1[1] > parameters_1[0])
                        {
                            bottom_1 = double.Parse(parameters_1[0].ToString("E2"));
                            top_1 = double.Parse(parameters_1[1].ToString("E2"));
                        }
                        else
                        {
                            bottom_1 = double.Parse(parameters_1[1].ToString("E2"));
                            top_1 = double.Parse(parameters_1[0].ToString("E2"));
                        }

                        if (parameters_2[1] > parameters_2[0])
                        {
                            bottom_2 = double.Parse(parameters_2[0].ToString("E2"));
                            top_2 = double.Parse(parameters_2[1].ToString("E2"));
                        }
                        else
                        {
                            bottom_2 = double.Parse(parameters_2[1].ToString("E2"));
                            top_2 = double.Parse(parameters_2[0].ToString("E2"));
                        }

                        row_params.Add(bottom_1);
                        row_params.Add(bottom_2);

                        row_params.Add(top_1);
                        row_params.Add(top_2);

                        row_params.Add(double.Parse(parameters_1[3].ToString("E2")));
                        row_params.Add(double.Parse(parameters_2[3].ToString("E2")));

                        row_params.Add(double.Parse(r2_1.ToString("E2")));
                        row_params.Add(double.Parse(r2_2.ToString("E2")));

                    }

                    else
                    {
                        for (int i = 0; i < 2 * output_parameter_number; i++) row_params.Add(-1);
                    }

                    descriptor_index++;
                }

                descriptors_chart_overlap.Add(cpd_id, list_DRC_cpd);

                DataGridViewRow current_row = (DataGridViewRow)f10.dataGridView1.Rows[0].Clone();

                for (int i = 0; i < row_params.Count() + 1; i++)
                {
                    if (i == 0) current_row.Cells[i].Value = cpd_id;
                    if (i > 0)
                    {
                        if (row_params[i - 1] == -1) current_row.Cells[i].Value = "";
                        else current_row.Cells[i].Value = row_params[i - 1];
                    }
                }

                f10.dataGridView1.Rows.Add(current_row);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // threshold R2
            double r2_threshold = double.Parse(this.numericUpDown1.Value.ToString());

            for (var idx = 0; idx < list_cpd.Count; idx++)
            {
                string cpd_id = list_cpd[idx].ToString();

                if (cpd_id == "DMSO" || cpd_id == "Untreated")
                    continue;

                List<Chart_DRC> list_chart = descriptors_chart[cpd_id];

                foreach (Chart_DRC current_chart in list_chart)
                {
                    current_chart.threshold_r2(r2_threshold);
                    current_chart.Is_Modified();
                }
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            // threshold Inactive
            double inactive_threshold = double.Parse(this.numericUpDown2.Value.ToString());

            for (var idx = 0; idx < list_cpd.Count; idx++)
            {
                string cpd_id = list_cpd[idx].ToString();

                if (cpd_id == "DMSO" || cpd_id == "Untreated")
                    continue;

                List<Chart_DRC> list_chart = descriptors_chart[cpd_id];

                foreach (Chart_DRC current_chart in list_chart)
                {
                    current_chart.threshold_inactive(inactive_threshold);
                    current_chart.Is_Modified();
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            // threshold Inactive
            double median_treshold = double.Parse(this.numericUpDown4.Value.ToString());

            for (var idx = 0; idx < list_cpd.Count; idx++)
            {
                string cpd_id = list_cpd[idx].ToString();

                if (cpd_id == "DMSO" || cpd_id == "Untreated")
                    continue;

                List<Chart_DRC> list_chart = descriptors_chart[cpd_id];

                foreach (Chart_DRC current_chart in list_chart)
                {
                    current_chart.remove_outlier_median(median_treshold);
                    current_chart.Is_Modified();
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            draw_drc();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            view_images_per_concentration = true;
            f12.view_images_per_concentration = true;

            check_images();
        }

        private void checkImagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            view_images_per_concentration = true;
            f12.view_images_per_concentration = true;

            check_images();
        }

        static List<string> DirSearch(string sDir)
        {
            List<string> Files = new List<string>();

            //Console.WriteLine(sDir);
            if (sDir == "") return null;

            foreach (string file in Directory.EnumerateFiles(sDir, "*.tif", SearchOption.AllDirectories))
            {
                //Console.WriteLine(file);
                Files.Add(file);
            }

            return Files;
        }

        public void check_images()
        {

            dict_plate_well_files.Clear();

            string savePath = "";
            if (folderBrowserDialog2.SelectedPath == "")
            {
                folderBrowserDialog2.SelectedPath = "Z:\\BTSData\\MeasurementData\\";
            }

            if (folderBrowserDialog2.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                savePath = folderBrowserDialog2.SelectedPath;
            }

            Console.WriteLine(savePath);

            List<string> list_img_path = DirSearch(savePath);

            if (list_img_path == null) return;

            foreach (string file in list_img_path)
            {
                string[] splitted_file = file.Split('_');
                string well = splitted_file[splitted_file.Count() - 2];

                string[] splitted_file_plate = file.Split('\\');
                string plate = splitted_file_plate[splitted_file_plate.Count() - 2];

                //Console.WriteLine(plate);
                //Console.WriteLine(well);
                if (file.Contains("F001"))
                {
                    if (dict_plate_well_files.Keys.Contains(plate)) // check if a part of plate name is in the path
                    {
                        SortedDictionary<string, List<string>> dict_well_files = dict_plate_well_files.FirstOrDefault(kvp => kvp.Key.Contains(plate)).Value;
                        //SortedDictionary<string, List<string>> dict_well_files = dict_plate_well_files[plate];

                        if (dict_well_files.ContainsKey(well))
                        {
                            dict_well_files[well].Add(file);
                        }
                        else
                        {
                            List<string> list_files = new List<string>();
                            list_files.Add(file);
                            dict_well_files.Add(well, list_files);
                        }
                    }
                    else
                    {
                        SortedDictionary<string, List<string>> dict_well_files = new SortedDictionary<string, List<string>>();

                        List<string> my_list = new List<string>();
                        my_list.Add(file);

                        dict_well_files.Add(well, my_list);

                        dict_plate_well_files[plate] = dict_well_files;
                    }
                }
            }

            //// Print
            //foreach (var plate in dict_plate_well_files)
            //{
            //    Console.WriteLine("Plate = " + plate.Key);

            //    Dictionary<string, List<string>> dict_well_files = plate.Value;
            //    foreach (var well in dict_well_files)
            //    {
            //        Console.WriteLine("---- Well = " + well.Key);
            //        List<string> files = well.Value;
            //        foreach (var file in files)
            //        {
            //            Console.WriteLine("--------- File = " + file);
            //        }
            //    }
            //}
            f11 = new ViewList_CPD_Tab(this);
            f11.Visible = true;

            f11.dataGridView1.ColumnCount = 1;
            f11.dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders;
            f11.dataGridView1.Columns[0].Name = "CPD_ID";

            if (list_cpd != null && list_cpd.Count > 0)
            {
                for (var idx = 0; idx < list_cpd.Count; idx++)
                {
                    int index = f11.dataGridView1.Rows.Add();

                    f11.dataGridView1.Rows[index].Cells[0].Value = list_cpd[idx];
                    f11.dataGridView1.Rows[idx].Cells[0].Style.BackColor = Color.LightGray;
                }
            }
            else return;
        }

        //public static Bitmap MatToBitmap(Mat image)
        //{
        //    return OpenCvSharp.Extensions.BitmapConverter.ToBitmap(image);
        //} 
        /// <summary>
        /// 
        /// </summary>
        /// <param name="imageIn"></param>
        /// <returns></returns>
        public byte[] imageToByteArray(System.Drawing.Image imageIn)
        {
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
            return ms.ToArray();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void load_cpd_images(object sender, DataGridViewCellEventArgs e)
        {
            f11.Visible = true;

            Form fc = Application.OpenForms["ViewCPD_Images_Tab"];

            if (fc == null)
                f12 = new ViewCPD_Images_Tab();

            if (view_images_per_concentration == true)
            {
                f12.dataGridView1.Rows.Clear();
                f12.dataGridView1.Columns.Clear();
                f12.dataGridView1.Refresh();
            }

            f12.view_images_per_concentration = view_images_per_concentration;

            f12.Visible = true;

            string cpd_id = f11.dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString();

            f12.Text = cpd_id;

            f13 = new ViewImages_Options_Tab(this, cpd_id);

            //f13.Visible = false;
            //f13.comboBox2.SelectedIndex = 1;
            //f13.comboBox3.SelectedIndex = 0;

            f13.Visible = true;

        }

        public void load_cpd_images(string cpd_id, bool view_options)
        {
            f11.Visible = false;

            Form fc = Application.OpenForms["ViewCPD_Images_Tab"];

            if (fc == null)
                f12 = new ViewCPD_Images_Tab();

            f12.Text = cpd_id;

            f12.view_images_per_concentration = view_options; // true in cpd main tab

            if (view_images_per_concentration == true)
            {
                f12.dataGridView1.Rows.Clear();
                f12.dataGridView1.Columns.Clear();
                f12.dataGridView1.Refresh();
            }

            f12.Visible = true;

            f13 = new ViewImages_Options_Tab(this, cpd_id);
            f13.Visible = true;

        }

        public void load_cpd_images(List<string> list_cpd_id)
        {
            f11.Visible = true;

            Form fc = Application.OpenForms["ViewCPD_Images_Tab"];

            if (fc == null)
                f12 = new ViewCPD_Images_Tab();

            f12.Text = "Compounds Hits";

            f12.view_images_per_concentration = view_images_per_concentration;

            if (view_images_per_concentration == true)
            {
                f12.dataGridView1.Rows.Clear();
                f12.dataGridView1.Columns.Clear();
                f12.dataGridView1.Refresh();
            }

            f12.Visible = true;

            f13 = new ViewImages_Options_Tab(this, list_cpd_id);
            f13.Visible = true;

            //for(int k = 1; k <list_cpd_id.Count; ++k) draw_images(list_cpd_id[k]);

        }

        public void clear_data_grid_cpd()
        {
            Form fc = Application.OpenForms["ViewCPD_Images_Tab"];

            if (fc != null)
            {
                f12.dataGridView1.Rows.Clear();
                f12.dataGridView1.Columns.Clear();
                f12.dataGridView1.Refresh();
                f12.toolStripProgressBar1.Value = 0;
            }
        }

        public void draw_list_cpds(List<string> list_cpd)
        {
            int progress = 0;
            foreach (string cpd in list_cpd)
            {
                draw_images(cpd);
                progress++;
                f12.toolStripProgressBar1.Value = progress * 100 / list_cpd.Count;
            }
        }

        private void copy_data_grid_view(ref DataGridView dataGridView1, ref DataGridView dataGridView2)
        {
            dataGridView2.DataSource = null;
            dataGridView2.Rows.Clear();

            if (dataGridView2.Columns.Count == 0)
            {
                foreach (DataGridViewColumn dgvc in dataGridView1.Columns)
                {
                    dataGridView2.Columns.Add(dgvc.Clone() as DataGridViewColumn);
                }
                int index_col = 0;
                foreach (DataGridViewColumn dgvc in dataGridView1.Columns)
                {
                    dataGridView2.Columns[index_col].Name = dgvc.Name;
                    dataGridView2.Columns[index_col].HeaderText = dgvc.HeaderText;
                    index_col++;
                }
            }

            DataGridViewRow row2 = new DataGridViewRow();

            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                row2 = (DataGridViewRow)dataGridView1.Rows[i].Clone();
                int intColIndex = 0;

                foreach (DataGridViewCell cell in dataGridView1.Rows[i].Cells)
                {
                    row2.Cells[intColIndex].Value = cell.Value;
                    intColIndex++;
                }
                dataGridView2.Rows.Add(row2);
            }
        }

        public void draw_images(string cpd_id)
        {

            //f3.dataGridView1.Sort(f3.dataGridView1.Columns["Concentration"], System.ComponentModel.ListSortDirection.Descending);

            List<string> plates = new List<string>();
            List<string> wells = new List<string>();
            //SortedDictionary<string, string> concentrations = new SortedDictionary<string, string>();
            List<double> concentrations = new List<double>();
            //List<double> row_concentrations = new List<double>();
            Dictionary<string, List<double>> descriptors_dict = new Dictionary<string, List<double>>();
            //f3.dataGridView1.Columns.Add(new DataGridViewTextBoxColumn());
            //f3.dataGridView1.Columns[c.ColumnCount - 1].ValueType = typeof(double);
            //f3.dataGridView1.Columns[f3.dataGridView1.ColumnCount - 1].Name = "ConcNum";

            //f3.dataGridView1.Columns.Add(new DataGridViewTextBoxColumn());
            //f3.dataGridView1.Columns[f3.dataGridView1.ColumnCount - 1].Name = "Plates";
            f3.dataGridView1.AllowUserToAddRows = false;
            f3.dataGridView2.AllowUserToAddRows = false;

            copy_data_grid_view(ref f3.dataGridView1, ref f3.dataGridView2);

            f3.dataGridView2.Refresh();
            f3.dataGridView2.Sort(new RowComparer(SortOrder.Descending));

            copy_data_grid_view(ref f3.dataGridView2, ref f3.dataGridView1);
            f3.dataGridView1.Refresh();

            ////f3.Show();

            foreach (DataGridViewRow row in f3.dataGridView1.Rows)
            {
                string current_cpd = row.Cells["CPD_ID"].Value.ToString();
                if (current_cpd == cpd_id)
                {
                    plates.Add(row.Cells["Plate"].Value.ToString());
                    wells.Add(row.Cells["Well"].Value.ToString());
                    concentrations.Add(double.Parse(row.Cells["Concentration"].Value.ToString()));

                    foreach (DataGridViewColumn col in f3.dataGridView1.Columns)
                    {
                        string col_name = col.HeaderText;
                        if (col_name != "CPD_ID" && col_name != "Plate" && col_name != "Well" && col_name != "Concentration" && col_name != "Class")
                        {
                            if (descriptors_dict.Keys.Contains(col_name))
                            {
                                descriptors_dict[col_name].Add(double.Parse(row.Cells[col_name].Value.ToString()));
                            }
                            else
                            {
                                List<double> my_list = new List<double>();
                                my_list.Add(double.Parse(row.Cells[col_name].Value.ToString()));
                                descriptors_dict[col_name] = my_list;
                            }
                        }
                    }
                }
            }

            //plates.Sort();
            //wells.Sort();
            //concentrations.Sort((a, b) => b.CompareTo(a));

            List<string> current_plates = plates.Distinct().ToList();
            List<string> current_wells = wells.Distinct().ToList();

            int rows = dict_plate_well_files.Keys.Distinct().ToList().Count() * (int)f13.numericUpDown6.Value;
            int total_plate_nb = Math.Min(current_plates.Count() * (int)f13.numericUpDown6.Value, rows);
            rows = total_plate_nb;

            int cols = (int)(current_wells.Count() / (double)f13.numericUpDown6.Value);

            int concentrations_per_cpd = concentrations.Count() / total_plate_nb;

            //f12.dataGridView1.Columns.Add("Concentration","Concentration");

            if (view_images_per_concentration == true)
            {

                f12.dataGridView1.Columns.Add(new DataGridViewTextBoxColumn());
                f12.dataGridView1.Columns[0].Name = "Plate";

                for (int i = 1; i < cols + 1; i++)
                {
                    DataGridViewImageColumn img = new DataGridViewImageColumn();
                    f12.dataGridView1.Columns.Insert(i, img);
                }

                f12.dataGridView1.RowCount = rows;
            }
            else
            {
                if (f12.dataGridView1.ColumnCount == 0)
                {
                    f12.dataGridView1.Columns.Add(new DataGridViewTextBoxColumn());
                    DataGridViewImageColumn img = new DataGridViewImageColumn();
                    f12.dataGridView1.Columns.Insert(1, img);
                    f12.dataGridView1.Columns.Add(new DataGridViewTextBoxColumn());

                    f12.dataGridView1.Columns[0].Name = "CPD_ID";
                    f12.dataGridView1.Columns[1].Name = "Image";
                    f12.dataGridView1.Columns[2].Name = "Concentration";

                    foreach (var item in descriptors_dict)
                    {
                        string col_name = item.Key;

                        DataGridViewTextBoxColumn new_col = new DataGridViewTextBoxColumn();
                        new_col.Name = col_name;

                        f12.dataGridView1.Columns.Add(new_col);
                    }

                    f12.dataGridView1.AllowUserToAddRows = false;
                }
            }

            foreach (DataGridViewColumn col in f12.dataGridView1.Columns)
            {
                col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }

            //f12.dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders;
            //List<string> list_plates = new List<string>(this.dict_plate_well_files.Keys);

            int image_width = 0;
            int image_height = 0;

            //List<string> concentration_ordered = new List<string>();
            int counter = 0;

            for (int i = 0; i < wells.Count(); i++)
            {
                f12.toolStripProgressBar1.Value = (i + 1) * 100 / wells.Count();

                List<string> files = new List<string>();

                string current_plate = plates[i];
                List<string> list_plates = dict_plate_well_files.Keys.ToList();

                bool test_plate = false;

                foreach (string plate_name in list_plates)
                {
                    if (plate_name.Contains(current_plate)) test_plate = true;
                }

                if (test_plate)
                {
                    SortedDictionary<string, List<string>> dict_well = dict_plate_well_files.FirstOrDefault(kvp => kvp.Key.Contains(plates[i])).Value;

                    files = dict_well[wells[i]];
                    counter++;
                    //row_concentrations.Add(concentrations[i]);
                }
                else
                {
                    continue;
                    //System.Windows.Forms.MessageBox.Show("Wrong Location or Plate name.");
                    //return;
                }

                Emgu.CV.Util.VectorOfMat channels = new Emgu.CV.Util.VectorOfMat();

                int size_channel = files.Count();
                files.Sort();

                foreach (string file in files)
                {
                    string method_norm = f13.comboBox3.SelectedItem.ToString();

                    Mat temp = CvInvoke.Imread(file, Emgu.CV.CvEnum.ImreadModes.AnyDepth);

                    if (method_norm == "Saturate")
                    {

                        ushort low_thr_ch1 = (ushort)f13.numericUpDown1.Value;
                        ushort low_thr_ch2 = (ushort)f13.numericUpDown8.Value;
                        ushort low_thr_ch3 = (ushort)f13.numericUpDown9.Value;
                        ushort low_thr_ch4 = (ushort)f13.numericUpDown10.Value;

                        ushort thr_ch1 = (ushort)f13.numericUpDown2.Value;
                        ushort thr_ch2 = (ushort)f13.numericUpDown3.Value;
                        ushort thr_ch3 = (ushort)f13.numericUpDown4.Value;
                        ushort thr_ch4 = (ushort)f13.numericUpDown5.Value;

                        int chan = 0;
                        if (file.Contains("Z01C01")) chan = 1;
                        else if (file.Contains("Z01C02")) chan = 2;
                        else if (file.Contains("Z01C03")) chan = 3;
                        else if (file.Contains("Z01C04")) chan = 4;

                        unsafe
                        {
                            ushort* data = (ushort*)temp.DataPointer;

                            if (chan == 1)
                            {
                                for (int idx = 0; idx < temp.Cols * temp.Rows; idx++)
                                {
                                    ushort px_value = data[idx];
                                    if (px_value < low_thr_ch1) data[idx] = 0;
                                    else if (px_value >= thr_ch1) data[idx] = thr_ch1;
                                    data[idx] = (ushort)(65535 * (double)(data[idx]) / (double)thr_ch1);
                                }
                            }
                            else if (chan == 2)
                            {
                                for (int idx = 0; idx < temp.Cols * temp.Rows; idx++)
                                {
                                    ushort px_value = data[idx];
                                    if (px_value < low_thr_ch2) data[idx] = 0;
                                    else if (px_value >= thr_ch2) data[idx] = thr_ch2;
                                    data[idx] = (ushort)(65535 * (double)(data[idx]) / (double)thr_ch2);
                                }
                            }
                            else if (chan == 3)
                            {
                                for (int idx = 0; idx < temp.Cols * temp.Rows; idx++)
                                {
                                    ushort px_value = data[idx];
                                    if (px_value < low_thr_ch3) data[idx] = 0;
                                    else if (px_value >= thr_ch3) data[idx] = thr_ch3;
                                    data[idx] = (ushort)(65535 * (double)(data[idx]) / (double)thr_ch3);
                                }
                            }
                            else if (chan == 4)
                            {
                                for (int idx = 0; idx < temp.Cols * temp.Rows; idx++)
                                {
                                    ushort px_value = data[idx];
                                    if (px_value < low_thr_ch4) data[idx] = 0;
                                    else if (px_value >= thr_ch4) data[idx] = thr_ch4;
                                    data[idx] = (ushort)(65535 * (double)(data[idx]) / (double)thr_ch4);
                                }
                            }
                        }

                    }

                    Mat mat_8u = new Mat();
                    temp.ConvertTo(mat_8u, Emgu.CV.CvEnum.DepthType.Cv8U, 1.0 / 255.0);

                    temp.Dispose();

                    Mat dst_thr = new Mat();

                    if (method_norm == "Otsu")
                    {
                        CvInvoke.Threshold(mat_8u, dst_thr, 0, 255, Emgu.CV.CvEnum.ThresholdType.Binary | Emgu.CV.CvEnum.ThresholdType.Otsu);
                    }

                    if (method_norm == "Equal")
                    {
                        CvInvoke.EqualizeHist(mat_8u, dst_thr);
                    }

                    if (method_norm == "Saturate")
                    {
                        dst_thr = mat_8u.Clone();
                    }

                    mat_8u.Dispose();

                    Mat dst_resize = new Mat();

                    double scale_factor = 1.0 / (double)f13.numericUpDown7.Value;

                    CvInvoke.Resize(dst_thr, dst_resize, new Size(0, 0), scale_factor, scale_factor, Emgu.CV.CvEnum.Inter.Cubic);

                    dst_thr.Dispose();

                    image_width = dst_resize.Cols;
                    image_height = dst_resize.Rows;

                    channels.Push(dst_resize);
                }

                if (size_channel == 1)
                {
                    Matrix<byte> My_Matrix_Image = new Matrix<byte>(channels[0].Rows, channels[0].Cols);
                    My_Matrix_Image.SetZero();
                    Mat my_new_mat = My_Matrix_Image.Mat;

                    channels.Push(my_new_mat);
                    channels.Push(my_new_mat.Clone());
                }

                if (size_channel == 2)
                {
                    Matrix<byte> My_Matrix_Image = new Matrix<byte>(channels[0].Rows, channels[0].Cols);
                    My_Matrix_Image.SetZero();
                    Mat my_new_mat = My_Matrix_Image.Mat;

                    channels.Push(my_new_mat);
                }


                string color_format = f13.comboBox2.SelectedItem.ToString();

                //if (color_format == "Rgb")
                //{
                //    if (size_channel == 2)
                //    {
                //        Emgu.CV.Util.VectorOfMat channels_bgr = new Emgu.CV.Util.VectorOfMat();
                //        channels_bgr.Push(channels[1].Clone());
                //        channels_bgr.Push(channels[0].Clone());

                //        channels.Clear();
                //        channels = channels_bgr;
                //    }
                //    if (size_channel == 3)
                //    {
                //        Emgu.CV.Util.VectorOfMat channels_bgr = new Emgu.CV.Util.VectorOfMat();
                //        channels_bgr.Push(channels[2].Clone());
                //        channels_bgr.Push(channels[1].Clone());
                //        channels_bgr.Push(channels[0].Clone());

                //        channels.Clear();
                //        channels = channels_bgr;
                //    }
                //}

                if (color_format == "EMT")
                {
                    if (size_channel == 3)
                    {
                        Emgu.CV.Util.VectorOfMat channels_bgr = new Emgu.CV.Util.VectorOfMat();
                        channels_bgr.Push(channels[1].Clone());
                        channels_bgr.Push(channels[2].Clone());
                        channels_bgr.Push(channels[0].Clone());

                        channels.Clear();
                        channels = channels_bgr;
                    }
                    if (size_channel == 4)
                    {
                        Emgu.CV.Util.VectorOfMat channels_bgr = new Emgu.CV.Util.VectorOfMat();
                        channels_bgr.Push(channels[1].Clone());
                        channels_bgr.Push(channels[2].Clone());
                        channels_bgr.Push(channels[0].Clone());
                        //channels_bgr.Push(channels[3].Clone());

                        channels.Clear();
                        channels = channels_bgr;
                    }
                }

                Mat mat = new Mat();
                CvInvoke.Merge(channels, mat);

                channels.Clear();

                Bitmap my_bitmap = null;

                if (color_format == "Rgb")
                    my_bitmap = (mat.ToImage<Emgu.CV.Structure.Rgb, Byte>()).ToBitmap();

                if (color_format == "Bgr" || color_format == "EMT")
                    my_bitmap = (mat.ToImage<Emgu.CV.Structure.Bgr, Byte>()).ToBitmap();

                int replicate = (int)f13.numericUpDown6.Value;

                if (view_images_per_concentration == true)
                {
                    f12.dataGridView1.Rows[(counter - 1) % total_plate_nb].Cells[(counter - 1) / total_plate_nb + 1].Value = (Image)my_bitmap;
                    f12.dataGridView1.Rows[(counter - 1) % total_plate_nb].Cells[0].Value = plates[i];
                    if (replicate != 1) f12.dataGridView1.Columns[(counter - 1) / total_plate_nb + 1].Name = concentrations[((counter - 1) / total_plate_nb) * replicate].ToString();
                    else f12.dataGridView1.Columns[(counter - 1) / total_plate_nb + 1].Name = concentrations[((counter - 1)) * replicate].ToString();
                }
                else
                {
                    int index = f12.dataGridView1.Rows.Add(new DataGridViewRow());

                    f12.dataGridView1.Rows[index].Cells[0].Value = cpd_id;
                    f12.dataGridView1.Rows[index].Cells[0].Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

                    f12.dataGridView1.Rows[index].Cells[1].Value = (Image)my_bitmap;
                    f12.dataGridView1.Rows[index].Cells[2].Value = concentrations[i];
                    f12.dataGridView1.Rows[index].Cells[2].Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

                    foreach (var item in descriptors_dict)
                    {
                        string col_name = item.Key;
                        f12.dataGridView1.Rows[index].Cells[col_name].Value = item.Value[i];
                        f12.dataGridView1.Rows[index].Cells[col_name].Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    }
                }

                mat.Dispose();

                //Console.WriteLine("i = " + i + " cols/rows = " + cols.ToString() + "  " + rows.ToString());
            }

            //Graphics g = this.CreateGraphics();

            int height = image_height; // (int)(image_height / g.DpiY * 72.0f); //  g.DpiY
            int width = image_width; // (int)(image_width / g.DpiX * 72.0f); // image_width; g.DpiX

            for (int i = 0; i < f12.dataGridView1.Rows.Count; i++)
            {
                f12.dataGridView1.Rows[i].Height = height + 5;
            }

            if (view_images_per_concentration == true)
            {
                for (int j = 0; j < f12.dataGridView1.Columns.Count; j++)
                {
                    if (j == 0) f12.dataGridView1.Columns[j].Width = 125;
                    else f12.dataGridView1.Columns[j].Width = width + 5;
                }
            }
            else
            {
                f12.dataGridView1.Columns[0].Width = 125;
                f12.dataGridView1.Columns[1].Width = width + 5;
                f12.dataGridView1.Columns[2].Width = 125;
            }

        }

        private void button3_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) return;
        }

        // Hits Menu
        private void loadHitsToolStripMenuItem_Click(object sender, EventArgs e)
        {

            openFileDialog1.Filter = "CSV Files (*.csv)|*.csv";
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Reset();

                this.Text = openFileDialog1.FileName;

                System.IO.StreamReader sr = new System.IO.StreamReader(openFileDialog1.FileName);
                csv = new CachedCsvReader(sr, true);

                //f3.Show(); 
                f3.Hide();
                f3.dataGridView1.DataSource = csv;
                f4.dataGridView1.DataSource = csv;

                List<string> CPD_ID = new List<string>();
                deslected_data_descriptor = new List<string>();

                if (f3.dataGridView1.ColumnCount < 4 || !f3.dataGridView1.Columns.Contains("CPD_ID")
                    || !f3.dataGridView1.Columns.Contains("Plate") || !f3.dataGridView1.Columns.Contains("Well")
                    || !f3.dataGridView1.Columns.Contains("Concentration"))
                {
                    System.Windows.Forms.MessageBox.Show("The file must contain at least these 4 columns : \n [Plate, Well, CPD_ID, Concentration]", "Error",
                        System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    return;
                }

                foreach (DataGridViewRow row in f3.dataGridView1.Rows)
                {
                    CPD_ID.Add(row.Cells["CPD_ID"].Value.ToString());
                }

                var unique_items = new HashSet<string>(CPD_ID);
                list_cpd = unique_items.ToList<string>();

                view_images_per_concentration = false;
                check_images();
            }

        }

        private void dRCTimeLineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.SetForm();

            DataTable main_table = new DataTable();
            HashSet<string> main_cpds = new HashSet<string>();

            openFileDialog1.Filter = "CSV Files (*.csv)|*.csv";

            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.Text = openFileDialog1.FileName;

                System.IO.StreamReader sr = new System.IO.StreamReader(openFileDialog1.FileName);
                CachedCsvReader my_csv = new CachedCsvReader(sr, true);
                main_table.Load(my_csv);

                foreach (DataRow row in main_table.Rows)
                {
                    string cpd = row["compound_id"].ToString();
                    main_cpds.Add(cpd);
                }
            }

            if (folderBrowserDialog1.SelectedPath == "")
            {
                folderBrowserDialog1.SelectedPath = "P:\\EMT_DATA\\CSV_FOR_DRC_TOOL\\Data\\";
            }

            if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string path = folderBrowserDialog1.SelectedPath;

                string[] files = Directory.GetFiles(path, "*.csv", SearchOption.AllDirectories);

                //HashSet<string> global_cpd_set = new HashSet<string>();

                foreach (string file in files)
                {
                    TextReader tr = new StreamReader(file);
                    CachedCsvReader current_csv = new CachedCsvReader(tr, true);

                    DataTable table = new DataTable();
                    table.Load(current_csv);

                    data_dict.Add(file, table);

                    foreach (DataRow row in table.Rows)
                    {
                        string cpd = row["compound_id"].ToString();

                        if (cpd_link.ContainsKey(cpd))
                        {
                            cpd_link[cpd].Add(file);
                        }
                        else
                        {
                            HashSet<string> set_files = new HashSet<string>();
                            set_files.Add(file);
                            cpd_link[cpd] = set_files;
                        }

                    }

                    Console.WriteLine("Reading --> " + file);
                }

                //// Print the cpd link
                //foreach (KeyValuePair<string, HashSet<string> > elem in cpd_link)
                //{
                //    Console.WriteLine("CPD_ID : " + elem.Key);
                //    foreach(string current_file in elem.Value)
                //    {
                //        Console.WriteLine(" ------ File : " + current_file);
                //    }
                //}

                foreach (var item in cpd_link.Where(dict => !main_cpds.Contains(dict.Key)).ToList())
                {
                    cpd_link.Remove(item.Key);
                }

                Console.WriteLine(" CPDS NB = " + cpd_link.Count());

                TimeLine.dataGridView1.ColumnCount = 1;
                TimeLine.dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders;
                TimeLine.dataGridView1.Columns[0].Name = "CPD_ID";
                TimeLine.dataGridView1.AllowUserToAddRows = false;

                foreach (KeyValuePair<string, HashSet<string>> elem in cpd_link)
                {
                    int idx = TimeLine.dataGridView1.Rows.Add(new DataGridViewRow());
                    TimeLine.dataGridView1.Rows[idx].Cells[0].Value = elem.Key;
                    TimeLine.dataGridView1.Rows[idx].Cells[0].Style.BackColor = Color.LightBlue;
                }

                TimeLine.Visible = true;
            }
        }

        public void get_compound_data(string cpd_id)
        {
            tableLayoutPanel1.Controls.Clear();

            HashSet<string> file_list = cpd_link[cpd_id];

            Console.WriteLine(cpd_id);

            Dictionary<string, int> descriptor_occurence = new Dictionary<string, int>();

            foreach (string current_file in file_list)
            {
                DataTable my_table = data_dict[current_file]; // file --> DataTable

                for (int i = 0; i < my_table.Columns.Count; i++)
                {
                    string my_header = my_table.Columns[i].ColumnName.ToString();

                    if (descriptor_occurence.ContainsKey(my_header))
                    {
                        descriptor_occurence[my_header] += 1;
                    }
                    else
                    {
                        descriptor_occurence.Add(my_header, 1);
                    }
                }
            }

            time_line_selected_descriptors.Clear();

            foreach (KeyValuePair<string, int> descr in descriptor_occurence)
            {
                string the_descriptor = descr.Key;
                int occ_number = descr.Value;

                if (occ_number >= file_list.Count())
                {
                    if (the_descriptor != "Plate" && the_descriptor != "Well" && the_descriptor != "compound_id" && the_descriptor != "Class" && the_descriptor != "dose")
                    {
                        time_line_selected_descriptors.Add(the_descriptor);
                    }
                }
            }

            // Here we can select the files. To be implemented.

            TimeLine.checkedListBox1.Items.Clear();

            foreach (string current_file in file_list)
            {
                TimeLine.checkedListBox1.Items.Add(current_file);
            }
        }

        public void draw_cpd_list(string current_file, string cpd_id, bool checked_state)
        {
            Dictionary<string, List<double>> descriptor_data = new Dictionary<string, List<double>>();
            List<double> descriptor_concentrations = new List<double>();

            DataTable my_table = data_dict[current_file]; // file --> DataTable

            foreach (DataRow row in my_table.Rows)
            {
                if (row["compound_id"].ToString() == cpd_id)
                {
                    foreach (string descriptor in time_line_selected_descriptors)
                    {
                        double val;
                        bool test_double = Double.TryParse(row[descriptor].ToString(), out val);
                        if (test_double == false) continue;

                        if (descriptor_data.ContainsKey(descriptor))
                        {
                            descriptor_data[descriptor].Add(val);
                        }
                        else
                        {
                            List<double> descriptor_values = new List<double>();
                            descriptor_values.Add(val);
                            descriptor_data[descriptor] = descriptor_values;
                        }

                    }

                    double current_concentration = Double.Parse(row["dose"].ToString());
                    descriptor_concentrations.Add(current_concentration);
                }
            }

            //charts_time_line = new Dictionary<string, Dictionary<string, Chart_DRC_Time_Line>>>(); // cpd_id, descriptor, chart

            if (!charts_time_line.ContainsKey(cpd_id))
            {

                Dictionary<string, Chart_DRC_Time_Line> list_chart_descriptors = new Dictionary<string, Chart_DRC_Time_Line>();

                foreach (KeyValuePair<string, List<double>> elem in descriptor_data)
                {
                    Console.WriteLine(descriptor_concentrations.Count());

                    string descriptor = elem.Key;
                    List<double> y = elem.Value;

                    List<double> x_log = new List<double>();

                    foreach (double val in descriptor_concentrations) x_log.Add(Math.Log10(val));

                    Chart_DRC_Time_Line current_chart = new Chart_DRC_Time_Line(cpd_id, descriptor, 100, ref descriptor_concentrations, ref x_log, ref y, Color.Blue, this, current_file);
                    current_chart.draw_DRC();

                    list_chart_descriptors.Add(descriptor, current_chart);
                }

                charts_time_line[cpd_id] = list_chart_descriptors;
            }
            else
            {
                Dictionary<string, Chart_DRC_Time_Line> list_chart_descriptors = charts_time_line[cpd_id];

                foreach (KeyValuePair<string, Chart_DRC_Time_Line> elem in list_chart_descriptors)
                {
                    string descriptor = elem.Key;
                    List<string> file_names = elem.Value.get_filenames();

                    if (!file_names.Contains(current_file))
                    {
                        if (elem.Value.is_first_curve_drawn() == false)
                        {
                            List<double> y = descriptor_data[descriptor];

                            List<double> x_log = new List<double>();

                            foreach (double val in descriptor_concentrations) x_log.Add(Math.Log10(val));

                            Chart_DRC_Time_Line current_chart = new Chart_DRC_Time_Line(cpd_id, descriptor, 100, ref descriptor_concentrations, ref x_log, ref y, Color.Blue, this, current_file);
                            current_chart.draw_DRC();

                            charts_time_line[cpd_id][descriptor] = current_chart;
                        }
                        else
                        {

                            List<double> y = descriptor_data[descriptor];

                            List<double> x_log = new List<double>();
                            foreach (double val in descriptor_concentrations) x_log.Add(Math.Log10(val));

                            elem.Value.add_serie_points(current_file, ref descriptor_concentrations, ref x_log, ref y, Color.Blue);
                        }

                    }
                    else
                    {
                        elem.Value.remove_serie_points(current_file);
                    }
                }
            }
        }

        private void check_last_points()
        {
            toolStripProgressBar1.Visible = true;

            // threshold Inactive
            double last_points_treshold = double.Parse(this.numericUpDown5.Value.ToString());

            for (var idx = 0; idx < list_cpd.Count; idx++)
            {
                toolStripProgressBar1.Value = idx * 100 / (list_cpd.Count - 1);

                string cpd_id = list_cpd[idx].ToString();

                if (cpd_id == "DMSO" || cpd_id == "Untreated")
                    continue;

                List<Chart_DRC> list_chart = descriptors_chart[cpd_id];

                foreach (Chart_DRC current_chart in list_chart)
                {
                    current_chart.test_two_points_around_top(last_points_treshold);
                    current_chart.Is_Modified();
                }
            }

            toolStripProgressBar1.Visible = false;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            check_last_points();
        }

        private void MainTab_Load(object sender, EventArgs e)
        {

        }

        private void button7_Click(object sender, EventArgs e)
        {
            toolStripProgressBar1.Visible = true;

            // threshold Inactive
            double toxicity_treshold = double.Parse(this.numericUpDown6.Value.ToString());

            for (var idx = 0; idx < list_cpd.Count; idx++)
            {
                this.toolStripProgressBar1.Value = idx * 100 / (list_cpd.Count - 1);

                string cpd_id = list_cpd[idx].ToString();

                if (cpd_id == "DMSO" || cpd_id == "Untreated")
                    continue;

                List<Chart_DRC> list_chart = descriptors_chart[cpd_id];

                foreach (Chart_DRC current_chart in list_chart)
                {
                    current_chart.check_toxicity(toxicity_treshold);
                    current_chart.Is_Modified();
                }
            }

            toolStripProgressBar1.Visible = false;
        }

        private void advancedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            label7.Visible = true;
            numericUpDown6.Visible = true;
            button7.Visible = true;

            label2.Visible = true;
            numericUpDown2.Visible = true;
            button2.Visible = true;

            label6.Visible = true;
            numericUpDown5.Visible = true;
            button6.Visible = true;

            label5.Visible = true;
            numericUpDown4.Visible = true;
            button5.Visible = true;

            label1.Visible = true;
            numericUpDown1.Visible = true;
            button1.Visible = true;
        }
    }

    public class Chart_DRC_Overlap
    {
        MainTab _form1 = new MainTab();

        private Chart chart;

        private List<double> drc_points_x_1 = new List<double>();
        private List<double> drc_points_y_1 = new List<double>();

        private List<double> drc_points_x_2 = new List<double>();
        private List<double> drc_points_y_2 = new List<double>();

        private List<double> drc_points_x_1_log = new List<double>();
        private List<double> drc_points_x_2_log = new List<double>();

        private Color chart_color;

        //private List<double> x_concentrations;
        //private List<double> x_concentrations_log;
        //private List<double> y_response;

        private double[] fit_parameters_1 = new double[4];
        private double[] fit_parameters_2 = new double[4];

        private List<double> x_fit_1;
        private List<double> x_fit_log_1;
        private List<double> y_fit_1;

        private List<double> y_fit_2;

        private int step_curve;

        private double MinConcentrationLin;
        private double MaxConcentrationLin;

        private double r2_1;
        private double RelativeError_1;
        private double r2_2;
        private double RelativeError_2;

        private string compound_id;
        private string descriptor;

        private bool data_modified;

        private int descriptor_index;

        private bool not_fitted;

        private double min_y_1;
        private double min_y_2;

        private double max_y_1;
        private double max_y_2;

        List<DataGridViewRow> raw_data;
        List<double> y_raw_data;
        List<double> x_raw_data;

        List<bool> is_raw_data_removed;

        public bool is_Fitted()
        {
            return not_fitted;
        }

        public bool is_data_modified()
        {
            return data_modified;
        }

        public void set_Raw_Data(List<DataGridViewRow> data)
        {
            raw_data = data.ToList();

            y_raw_data = new List<double>();
            x_raw_data = new List<double>();

            foreach (DataGridViewRow item in raw_data)
            {
                y_raw_data.Add(double.Parse(item.Cells[descriptor].Value.ToString()));
                x_raw_data.Add(double.Parse(item.Cells["Concentration"].Value.ToString()));
            }
        }

        public List<DataGridViewRow> get_Raw_Data()
        {
            return raw_data;
        }

        public List<bool> get_Removed_Raw_Data()
        {
            return is_raw_data_removed;
        }

        public string get_Descriptor_Name()
        {
            return descriptor;
        }

        public double[] get_Fit_Parameters_1()
        {
            return fit_parameters_1;
        }

        public double[] get_Fit_Parameters_2()
        {
            return fit_parameters_2;
        }

        public double get_R2_1()
        {
            return r2_1;
        }

        public double get_R2_2()
        {
            return r2_2;
        }

        //private bool chart_already_loaded;

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

        public Chart_DRC_Overlap()
        {
        }

        public Chart_DRC_Overlap(string cpd, string descript, int step, ref List<double> x_1, ref List<double> x_log_1, ref List<double> x_2, ref List<double> x_log_2, ref List<double> y_1, ref List<double> y_2, Color color, int index, MainTab form)
        {
            _form1 = form;

            descriptor_index = index;

            compound_id = cpd;
            descriptor = descript;
            step_curve = step;
            chart_color = color;

            not_fitted = false;
            data_modified = false;

            drc_points_y_1 = y_1.ToList();
            drc_points_y_2 = y_2.ToList();

            drc_points_x_1_log = x_log_1.ToList();
            drc_points_x_2_log = x_log_2.ToList();

            drc_points_x_1 = x_1.ToList();
            drc_points_x_2 = x_2.ToList();

            double min_x_1 = MinA(x_1.ToArray());
            double min_x_2 = MinA(x_2.ToArray());

            double max_x_1 = MaxA(x_1.ToArray());
            double max_x_2 = MaxA(x_2.ToArray());

            min_y_1 = MinA(y_1.ToArray());
            min_y_2 = MinA(y_2.ToArray());

            max_y_1 = MaxA(y_1.ToArray());
            max_y_2 = MaxA(y_2.ToArray());

            MinConcentrationLin = Math.Min(min_x_1, min_x_2); //MinA(x_concentrations.ToArray());
            MaxConcentrationLin = Math.Max(max_x_1, max_x_2); //MaxA(x_concentrations.ToArray());

            x_fit_1 = new List<double>();
            x_fit_log_1 = new List<double>();
            y_fit_1 = new List<double>();
            y_fit_2 = new List<double>();

            is_raw_data_removed = new List<bool>();

            for (int j = 0; j < step_curve; j++)
            {
                x_fit_1.Add(MinConcentrationLin + j * (MaxConcentrationLin - MinConcentrationLin) / (double)step_curve);
                x_fit_log_1.Add(Math.Log10(MinConcentrationLin) + j * (Math.Log10(MaxConcentrationLin) - Math.Log10(MinConcentrationLin)) / (double)step_curve);
            }

            chart = new Chart();

            ChartArea chartArea = new ChartArea();
            Series series1 = new Series();
            Series series2 = new Series();
            Series series3 = new Series();
            Series series4 = new Series();

            //chartArea.Position.Auto = false;
            Axis yAxis = new Axis(chartArea, AxisName.Y);
            Axis xAxis = new Axis(chartArea, AxisName.X);

            chartArea.AxisX.LabelStyle.Format = "N2";
            chartArea.AxisX.Title = "Concentatrion";
            chartArea.AxisY.Title = "Response";

            //double max_y_1 = MaxA(drc_points_y_1.ToArray());
            //double max_y_2 = MaxA(drc_points_y_2.ToArray());
            double max_y = Math.Max(max_y_1, max_y_2);

            //if (max_y < 1.0) chartArea.AxisY.Maximum = 1.0;

            chartArea.Name = descriptor;

            chart.ChartAreas.Add(chartArea);
            chart.Name = descriptor;

            chart.Location = new System.Drawing.Point(250, 100);

            series1.ChartType = SeriesChartType.Point;
            series2.ChartType = SeriesChartType.Line;
            series3.ChartType = SeriesChartType.Point;
            series4.ChartType = SeriesChartType.Line;

            series1.MarkerStyle = MarkerStyle.Circle;
            series3.MarkerStyle = MarkerStyle.Circle;

            series1.Name = "Series1";
            series2.Name = "Series2";
            series3.Name = "Series3";
            series4.Name = "Series4";

            chart.Series.Add(series1);
            chart.Series.Add(series2);
            chart.Series.Add(series3);
            chart.Series.Add(series4);

            chart.Size = new System.Drawing.Size(550, 350);

            chart.Titles.Add("Title1");

            fit_DRC_1();
            fit_DRC_2();
        }

        private static void function_SigmoidInhibition(double[] c, double[] x, ref double func, object obj)
        {
            func = c[0] + ((c[1] - c[0]) / (1 + Math.Pow(10, (c[2] - x[0]) * c[3])));
        }

        private double Sigmoid(double[] c, double x)
        {
            double y = c[0] + ((c[1] - c[0]) / (1 + Math.Pow(10, (c[2] - x) * c[3])));
            return y;
        }

        private void fit_DRC_1()
        {
            double GlobalMax = double.MinValue;
            double MaxValues = MaxA(drc_points_y_1.ToArray());

            GlobalMax = MaxValues + 0.5 * Math.Abs(MaxValues);

            double GlobalMin = double.MaxValue;
            double MinValues = MinA(drc_points_y_1.ToArray());

            GlobalMin = MinValues - 0.5 * Math.Abs(MinValues);

            double BaseEC50 = Math.Log10(MaxConcentrationLin) - Math.Abs(Math.Log10(MaxConcentrationLin) - Math.Log10(MinConcentrationLin)) / 2.0;
            double[] c = new double[] { GlobalMin, GlobalMax, BaseEC50, 1 };

            double epsf = 0;
            double epsx = 0;

            int maxits = 0;
            int info;

            double[] bndl = null;
            double[] bndu = null;

            // boundaries
            bndu = new double[] { GlobalMax, GlobalMax, Math.Log10(MaxConcentrationLin) - 1.0, 100 };
            bndl = new double[] { GlobalMin, GlobalMin, Math.Log10(MinConcentrationLin) + 1.0, -100 };

            alglib.lsfitstate state;
            alglib.lsfitreport rep;
            double diffstep = 1e-12;

            // Fitting without weights
            //alglib.lsfitcreatefg(Concentrations, Values.ToArray(), c, false, out state);

            double[,] Concentration = new double[drc_points_x_1_log.Count(), 1];
            for (var i = 0; i < drc_points_x_1_log.Count(); ++i)
            {
                Concentration[i, 0] = drc_points_x_1_log[i];
            }

            int NumDimension = 1;
            alglib.lsfitcreatef(Concentration, drc_points_y_1.ToArray(), c, diffstep, out state);
            alglib.lsfitsetcond(state, epsx, maxits);
            alglib.lsfitsetbc(state, bndl, bndu);
            // alglib.lsfitsetscale(state, s);

            alglib.lsfitfit(state, function_SigmoidInhibition, null, null);
            alglib.lsfitresults(state, out info, out c, out rep);

            fit_parameters_1 = c;
            RelativeError_1 = rep.avgrelerror;
            r2_1 = rep.r2;

            y_fit_1.Clear();

            for (int IdxConc = 0; IdxConc < x_fit_log_1.Count; IdxConc++)
            {
                y_fit_1.Add(Sigmoid(c, x_fit_log_1[IdxConc]));
            }

        }

        private void fit_DRC_2()
        {
            double GlobalMax = double.MinValue;
            double MaxValues = MaxA(drc_points_y_2.ToArray());

            GlobalMax = MaxValues + 0.5 * Math.Abs(MaxValues);

            double GlobalMin = double.MaxValue;
            double MinValues = MinA(drc_points_y_2.ToArray());

            GlobalMin = MinValues - 0.5 * Math.Abs(MinValues);

            double BaseEC50 = Math.Log10(MaxConcentrationLin) - Math.Abs(Math.Log10(MaxConcentrationLin) - Math.Log10(MinConcentrationLin)) / 2.0;
            double[] c = new double[] { GlobalMin, GlobalMax, BaseEC50, 1 };

            double epsf = 0;
            double epsx = 0;

            int maxits = 0;
            int info;

            double[] bndl = null;
            double[] bndu = null;

            // boundaries
            bndu = new double[] { GlobalMax, GlobalMax, Math.Log10(MaxConcentrationLin), 100 };
            bndl = new double[] { GlobalMin, GlobalMin, Math.Log10(MinConcentrationLin), -100 };

            alglib.lsfitstate state;
            alglib.lsfitreport rep;
            double diffstep = 1e-12;

            // Fitting without weights
            //alglib.lsfitcreatefg(Concentrations, Values.ToArray(), c, false, out state);

            double[,] Concentration = new double[drc_points_x_2_log.Count(), 1];
            for (var i = 0; i < drc_points_x_2_log.Count(); ++i)
            {
                Concentration[i, 0] = drc_points_x_2_log[i];
            }

            int NumDimension = 1;
            alglib.lsfitcreatef(Concentration, drc_points_y_2.ToArray(), c, diffstep, out state);
            alglib.lsfitsetcond(state, epsx, maxits);
            alglib.lsfitsetbc(state, bndl, bndu);
            // alglib.lsfitsetscale(state, s);

            alglib.lsfitfit(state, function_SigmoidInhibition, null, null);
            alglib.lsfitresults(state, out info, out c, out rep);

            fit_parameters_2 = c;
            RelativeError_2 = rep.avgrelerror;
            r2_2 = rep.r2;

            y_fit_2.Clear();

            for (int IdxConc = 0; IdxConc < x_fit_log_1.Count; IdxConc++)
            {
                y_fit_2.Add(Sigmoid(c, x_fit_log_1[IdxConc]));
            }

        }

        public void draw_DRC()
        {
            string cpd = compound_id;

            fit_DRC_1();
            fit_DRC_2();

            chart.Titles["Title1"].Text = descriptor + " CPD=" + compound_id;

            //Color second_color = Color.Cyan;
            //if (chart_color == Color.Red) second_color = Color.Salmon;
            //if (chart_color == Color.Green) second_color = Color.LightGreen;
            //if (chart_color == Color.Black) second_color = Color.DarkGray;


            // Draw the first graph
            chart.Series["Series1"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
            chart.Series["Series1"].Points.DataBindXY(drc_points_x_1_log, drc_points_y_1);
            chart.Series["Series1"].Color = chart_color;

            chart.Series["Series2"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chart.Series["Series2"].Points.DataBindXY(x_fit_log_1, y_fit_1);
            chart.Series["Series2"].Color = chart_color;

            // Draw the second graph
            chart.Series["Series3"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
            chart.Series["Series3"].Points.DataBindXY(drc_points_x_2_log, drc_points_y_2);
            chart.Series["Series3"].Color = Color.DarkGray;

            chart.Series["Series4"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chart.Series["Series4"].Points.DataBindXY(x_fit_log_1, y_fit_2);
            chart.Series["Series4"].Color = Color.DarkGray;

            double ratio = 100.0 / (Math.Ceiling((double)_form1.get_descriptors_number() / 2.0));
            _form1.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, (float)ratio));

            _form1.tableLayoutPanel1.Controls.Add(chart);
            chart.Anchor = (AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top);
            //chart_already_loaded = true;
        }

        public string save_image(string path)
        {
            draw_DRC();
            string descriptor_name = descriptor.Replace(@"/", @"_");
            string output_image = path + "/CPD_" + compound_id + "_" + descriptor_name + ".bmp";

            System.Diagnostics.Debug.WriteLine("Write Image = " + output_image);
            chart.SaveImage(output_image, ChartImageFormat.Bmp);

            return output_image;
        }
    }


    public class Chart_DRC
    {
        MainTab _form1 = new MainTab();

        private Chart chart;

        private List<double> drc_points_x_enable = new List<double>();
        private List<double> drc_points_y_enable = new List<double>();

        List<double> drc_points_x_disable = new List<double>();
        List<double> drc_points_y_disable = new List<double>();

        private RectangleAnnotation annotation_ec50 = new RectangleAnnotation();
        private Color chart_color;

        private List<double> x_concentrations;
        private List<double> x_concentrations_log;

        private List<double> y_response;

        private double[] fit_parameters = new double[4];
        private List<double> x_fit;
        private List<double> x_fit_log;

        private List<double> y_fit_;
        private List<double> y_fit_log;

        private int step_curve;

        private double MinConcentrationLin;
        private double MaxConcentrationLin;

        private double r2;
        private double RelativeError;

        private string compound_id;
        private string descriptor;

        private bool data_modified;

        private int descriptor_index;

        private bool not_fitted;
        private bool inactive;
        private bool is_ec50_exact = true; // last 2 points method

        private bool not_fitted_init;
        private bool inactive_init;

        List<DataGridViewRow> raw_data;
        List<double> y_raw_data;
        List<double> x_raw_data;

        List<bool> is_raw_data_removed;

        private Curves_Options options_form;
        private Curve_Fit_Options options_fit_form;

        private double minX = -1;
        private double maxX = -1;

        private double min_bound_x = 0.0;
        private double max_bound_x = 0.0;
        private double min_bound_y = 0.0;
        private double max_bound_y = 0.0;

        private bool bound_auto = true;

        public void set_bound_status(bool status)
        {
            bound_auto = status;
        }

        public double get_min_bound_x()
        {
            return min_bound_x;
        }

        public double get_max_bound_x()
        {
            return max_bound_x;
        }

        public double get_min_bound_y()
        {
            return min_bound_y;
        }

        public double get_max_bound_y()
        {
            return max_bound_y;
        }

        public void set_min_bound_x(double x_min)
        {
            min_bound_x = x_min;
        }

        public void set_max_bound_x(double x_max)
        {
            max_bound_x = x_max;
        }

        public void set_min_bound_y(double y_min)
        {
            min_bound_y = y_min;
        }

        public void set_max_bound_y(double y_max)
        {
            max_bound_y = y_max;
        }

        public bool check_ec50_exact()
        {
            return is_ec50_exact;
        }

        public bool is_Fitted()
        {
            return not_fitted;
        }

        public bool is_Inactive()
        {
            return inactive;
        }

        public bool is_data_modified()
        {
            return data_modified;
        }

        public void set_Raw_Data(List<DataGridViewRow> data)
        {
            raw_data = data.ToList();

            y_raw_data = new List<double>();
            x_raw_data = new List<double>();

            foreach (DataGridViewRow item in raw_data)
            {
                y_raw_data.Add(double.Parse(item.Cells[descriptor].Value.ToString()));
                x_raw_data.Add(double.Parse(item.Cells["Concentration"].Value.ToString()));
            }
        }

        public List<DataGridViewRow> get_Raw_Data()
        {
            return raw_data;
        }

        public List<bool> get_Removed_Raw_Data()
        {
            return is_raw_data_removed;
        }

        public string get_Descriptor_Name()
        {
            return descriptor;
        }

        //private bool chart_already_loaded;

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

        public double get_R2()
        {
            return r2;
        }

        public Chart_DRC()
        {
        }

        public Chart_DRC(string cpd, string descript, int step, ref List<double> x, ref List<double> x_log, ref List<double> resp, Color color,
            int index, List<string> deselected, string ec_50_status, MainTab form)
        {
            _form1 = form;

            descriptor_index = index;

            compound_id = cpd;
            descriptor = descript;
            step_curve = step;
            chart_color = color;

            not_fitted = false;
            data_modified = false;

            if (ec_50_status == "=") is_ec50_exact = true;
            else if (ec_50_status == ">") is_ec50_exact = false;

            y_response = resp.ToList();
            drc_points_y_enable = resp.ToList();

            x_concentrations = x.ToList();
            drc_points_x_enable = x_log.ToList();
            x_concentrations_log = x_log.ToList();

            MinConcentrationLin = MinA(x_concentrations.ToArray());
            MaxConcentrationLin = MaxA(x_concentrations.ToArray());

            x_fit = new List<double>();
            x_fit_log = new List<double>();
            y_fit_log = new List<double>();

            is_raw_data_removed = new List<bool>();

            foreach (double data_point in y_response)
            {
                is_raw_data_removed.Add(false);
            }

            for (int index_deselect = 0; index_deselect < deselected.Count(); ++index_deselect)
            {
                if (deselected[index_deselect] == "TRUE" || deselected[index_deselect] == "True")
                {

                    drc_points_x_disable.Add(x_concentrations_log[index_deselect]);
                    drc_points_y_disable.Add(y_response[index_deselect]);

                    is_raw_data_removed[index_deselect] = true;

                    double point_y = y_response[index_deselect];

                    int remove_index = drc_points_y_enable.FindIndex(a => a < point_y + .000001 && a > point_y - .000001);


                    drc_points_x_enable.RemoveAt(remove_index); //Add(data_chart[i].XValue);
                    drc_points_y_enable.RemoveAt(remove_index); //Add(data_chart[i].YValues[0]);
                }

                if (deselected[0] == "Not Fitted")
                {
                    not_fitted = true;       // When first element is NOT FITTED all the columns are NOT FITTED (For the current descriptor)
                    not_fitted_init = true;
                }
                else not_fitted_init = false;

                if (deselected[0] == "Inactive")
                {
                    inactive = true;
                    inactive_init = true;
                }
                else inactive_init = false;

            }


            for (int j = 0; j < step_curve; j++)
            {
                //x_fit.Add(MinConcentrationLin + j * (MaxConcentrationLin - MinConcentrationLin) / (double)step_curve);
                x_fit_log.Add(Math.Log10(MinConcentrationLin) + j * (Math.Log10(MaxConcentrationLin) - Math.Log10(MinConcentrationLin)) / (double)step_curve);
                x_fit.Add(Math.Pow(10, Math.Log10(MinConcentrationLin) + j * (Math.Log10(MaxConcentrationLin) - Math.Log10(MinConcentrationLin)) / (double)step_curve));
            }

            chart = new Chart();

            ChartArea chartArea = new ChartArea();
            Series series1 = new Series();
            Series series2 = new Series();

            //chartArea.Position.Auto = false;
            Axis yAxis = new Axis(chartArea, AxisName.Y);
            Axis xAxis = new Axis(chartArea, AxisName.X);

            chartArea.AxisX.LabelStyle.Format = "N2";
            chartArea.AxisX.Title = "Concentatrion";
            chartArea.AxisY.Title = "Response";

            //double max_y = MaxA(y_response.ToArray());

            //if (max_y < 1.0) chartArea.AxisY.Maximum = 1.0;

            chartArea.Name = descriptor;

            chart.ChartAreas.Add(chartArea);
            chart.Name = descriptor;

            //each chart will be 43 units higher than the last
            chart.Location = new System.Drawing.Point(250, 100);

            series1.ChartType = SeriesChartType.Point;
            series2.ChartType = SeriesChartType.Line;

            series1.MarkerStyle = MarkerStyle.Circle;

            series1.Name = "Series1";
            series2.Name = "Series2";

            chart.Series.Add(series1);
            chart.Series.Add(series2);

            chart.Size = new System.Drawing.Size(550, 350);
            //chart.Visible = true;
            //chart.TabStop = false;
            //chart.Update();
            //chart.Show();

            //chart.Width = new Unit(300, System.Web.UI.WebControls.UnitType.Pixel);
            //chart.Height = new Unit(200, System.Web.UI.WebControls.UnitType.Pixel);

            chart.Titles.Add("Title1");

            chart.MouseUp += new System.Windows.Forms.MouseEventHandler(this.chart1_MouseUp);
            chart.MouseMove += new System.Windows.Forms.MouseEventHandler(this.chart1_MouseMove);
            chart.MouseDown += new System.Windows.Forms.MouseEventHandler(this.chart1_MouseDown);
            chart.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.chart1_MouseDoubleClick);
            chart.MouseClick += new System.Windows.Forms.MouseEventHandler(this.chart1_MouseClick);
            chart.MouseClick += new System.Windows.Forms.MouseEventHandler(this.chart1_MouseClickMenu);
            //chart.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.chart1_KeyPress);
            //chart.PrePaint += new System.Windows.Forms.chart ChartPaintEventArgs(this.Chart1_PrePaint);
            //Create a rectangle annotation

            //RectangleAnnotation annotationRectangle = new RectangleAnnotation();
            //annotation_ec50 = annotationRectangle;

            //chart.ChartAreas[0].AxisX.Minimum = -10;
            //chart.ChartAreas[0].AxisX.Maximum = -5;

            //chart.ChartAreas[0].AxisY.Minimum = -1;
            //chart.ChartAreas[0].AxisY.Maximum = +1;

            //draw_DRC(false, false);

            fit_DRC();
        }


        //protected void Chart1_PrePaint(object sender, ChartPaintEventArgs e)
        //{
        //    if (e.ChartElement is ChartArea)
        //    {
        //        var ta = new TextAnnotation();
        //        ta.Text = "Menu";
        //        ta.Width = e.Position.Width;
        //        ta.Height = e.Position.Height;
        //        ta.X = e.Position.X;
        //        ta.Y = e.Position.Y;
        //        ta.Font = new Font("Ms Sans Serif", 16, FontStyle.Bold);

        //        chart.Annotations.Add(ta);
        //    }
        //}

        private static void function_SigmoidInhibition(double[] c, double[] x, ref double func, object obj)
        {
            func = c[0] + ((c[1] - c[0]) / (1 + Math.Pow(10, (c[2] - x[0]) * c[3])));
        }

        private double Sigmoid(double[] c, double x)
        {
            double y = c[0] + ((c[1] - c[0]) / (1 + Math.Pow(10, (c[2] - x) * c[3])));
            return y;
        }

        private void fit_DRC()
        {
            double GlobalMax = double.MinValue;
            double MaxValues = MaxA(drc_points_y_enable.ToArray());

            GlobalMax = MaxValues + 0.5 * Math.Abs(MaxValues);

            double GlobalMin = double.MaxValue;
            double MinValues = MinA(drc_points_y_enable.ToArray());

            GlobalMin = MinValues - 0.5 * Math.Abs(MinValues);

            //if ((double)_form1.numericUpDown3.Value != 0)
            //{
            //    max_bound_y = (double)_form1.numericUpDown3.Value;
            //}

            double epsf = 0;
            double epsx = 0;

            int maxits = 0;
            int info;

            if (bound_auto)
            {
                min_bound_y = GlobalMin;
                max_bound_y = GlobalMax;

                min_bound_x = Math.Log10(MaxConcentrationLin) + 1.0;
                max_bound_x = Math.Log10(MinConcentrationLin) - 1.0;
            }

            double BaseEC50 = Math.Log10(MaxConcentrationLin) - Math.Abs(Math.Log10(MaxConcentrationLin) - Math.Log10(MinConcentrationLin)) / 2.0;
            double[] c = new double[] { min_bound_y, max_bound_y, BaseEC50, 1 };

            double[] bndl = null;
            double[] bndu = null;

            // boundaries
            bndu = new double[] { max_bound_y, max_bound_y, min_bound_x, +100.0 };
            bndl = new double[] { min_bound_y, min_bound_y, max_bound_x, -100.0 };

            alglib.lsfitstate state;
            alglib.lsfitreport rep;
            double diffstep = 1e-12;

            // Fitting without weights
            //alglib.lsfitcreatefg(Concentrations, Values.ToArray(), c, false, out state);

            double[,] Concentration = new double[drc_points_x_enable.Count(), 1];
            for (var i = 0; i < drc_points_x_enable.Count(); ++i)
            {
                Concentration[i, 0] = drc_points_x_enable[i];
            }

            int NumDimension = 1;
            alglib.lsfitcreatef(Concentration, drc_points_y_enable.ToArray(), c, diffstep, out state);
            alglib.lsfitsetcond(state, epsx, maxits);
            alglib.lsfitsetbc(state, bndl, bndu);
            // alglib.lsfitsetscale(state, s);

            alglib.lsfitfit(state, function_SigmoidInhibition, null, null);
            alglib.lsfitresults(state, out info, out c, out rep);

            fit_parameters = c;
            RelativeError = rep.avgrelerror;
            r2 = rep.r2;

            y_fit_log.Clear();

            for (int IdxConc = 0; IdxConc < x_fit.Count; IdxConc++)
            {
                y_fit_log.Add(Sigmoid(c, x_fit_log[IdxConc]));
            }

        }

        public void Is_Modified()
        {
            draw_DRC(false, false);
            /*
            int k = 0;
            foreach (DataGridViewRow row2 in _form1.f2.dataGridView2.Rows)
            {
                string compound = row2.Cells[0].Value.ToString();
                if (compound_id == compound) break;
                k++;
            }
            int row_index = k;

            // Redraw value in table
            _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 1].Value = double.Parse(Math.Pow(10, fit_parameters[2]).ToString("E2"));
            if (fit_parameters[0] < fit_parameters[1])
            {
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 2].Value = double.Parse(fit_parameters[0].ToString("E2"));
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 3].Value = double.Parse(fit_parameters[1].ToString("E2"));
            }
            else
            {
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 2].Value = double.Parse(fit_parameters[1].ToString("E2"));
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 3].Value = double.Parse(fit_parameters[0].ToString("E2"));
            }
            _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 4].Value = double.Parse(fit_parameters[3].ToString("E2"));
            _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 5].Value = double.Parse(r2.ToString("E2"));

            if (drc_points_x_disable.Count() > 0)
            {
                data_modified = true;
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 1].Style.BackColor = Color.LightSeaGreen;
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 2].Style.BackColor = Color.LightSeaGreen;
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 3].Style.BackColor = Color.LightSeaGreen;
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 4].Style.BackColor = Color.LightSeaGreen;
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 5].Style.BackColor = Color.LightSeaGreen;
            }
            else
            {
                data_modified = false;
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 1].Style.BackColor = Color.White;
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 2].Style.BackColor = Color.White;
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 3].Style.BackColor = Color.White;
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 4].Style.BackColor = Color.White;
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 5].Style.BackColor = Color.White;
            }

            if (not_fitted)
            {

                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 1].Value = "Not Fitted";
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 2].Value = "Not Fitted";
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 3].Value = "Not Fitted";
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 4].Value = "Not Fitted";
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 5].Value = "Not Fitted";

                data_modified = true;
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 1].Style.BackColor = Color.Tomato;
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 2].Style.BackColor = Color.Tomato;
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 3].Style.BackColor = Color.Tomato;
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 4].Style.BackColor = Color.Tomato;
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 5].Style.BackColor = Color.Tomato;

                annotation_ec50.Text = "EC_50 = Not Fitted";

                ((RectangleAnnotation)chart.Annotations["menu_inactive"]).ForeColor = Color.LightGray;
                ((RectangleAnnotation)chart.Annotations["menu_not_fitted"]).ForeColor = Color.Red;
            }

            if (inactive)
            {

                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 1].Value = "Inactive";
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 2].Value = "Inactive";
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 3].Value = "Inactive";
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 4].Value = "Inactive";
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 5].Value = "Inactive";

                data_modified = true;
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 1].Style.BackColor = Color.Orange;
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 2].Style.BackColor = Color.Orange;
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 3].Style.BackColor = Color.Orange;
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 4].Style.BackColor = Color.Orange;
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 5].Style.BackColor = Color.Orange;

                annotation_ec50.Text = "EC_50 = Inactive";

                ((RectangleAnnotation)chart.Annotations["menu_inactive"]).ForeColor = Color.Orange;
                ((RectangleAnnotation)chart.Annotations["menu_not_fitted"]).ForeColor = Color.LightGray;
            }
            */
            //if(is_ec50_exact == true) ((RectangleAnnotation)chart.Annotations["menu_ec_50_sup"]).Text = "=";
            //else ((RectangleAnnotation)chart.Annotations["menu_ec_50_sup"]).Text = ">";

        }

        public void threshold_r2(double thr)
        {
            draw_DRC(false, false);

            //double r2_threshold = double.Parse(_form1.numericUpDown1.Value.ToString());

            not_fitted = not_fitted_init;

            if (r2 < thr)
            {
                not_fitted = true;

                ((RectangleAnnotation)chart.Annotations["menu_inactive"]).ForeColor = Color.LightGray;
                ((RectangleAnnotation)chart.Annotations["menu_not_fitted"]).ForeColor = Color.Red;

                if (inactive_init == true)
                {
                    not_fitted = false;

                    ((RectangleAnnotation)chart.Annotations["menu_inactive"]).ForeColor = Color.Orange;
                    ((RectangleAnnotation)chart.Annotations["menu_not_fitted"]).ForeColor = Color.LightGray;
                }
            }

            //Is_Modified();
        }

        public void threshold_inactive(double thr)
        {
            draw_DRC(false, false);

            inactive = inactive_init;

            double min_max_activity = Math.Abs(fit_parameters[1]-fit_parameters[0]);

            if (min_max_activity < thr)
            {
                inactive = true;

                ((RectangleAnnotation)chart.Annotations["menu_inactive"]).ForeColor = Color.Orange;
                ((RectangleAnnotation)chart.Annotations["menu_not_fitted"]).ForeColor = Color.LightGray;

                if (not_fitted_init == true)
                {
                    inactive = false;

                    ((RectangleAnnotation)chart.Annotations["menu_inactive"]).ForeColor = Color.LightGray;
                    ((RectangleAnnotation)chart.Annotations["menu_not_fitted"]).ForeColor = Color.Red;
                }
            }

            //Is_Modified();
        }

        public void test_two_points_around_top(double thr_2_last_points)
        {
            // Get the bottom and the top :
            double top = double.Parse(fit_parameters[1].ToString());
            double bottom = double.Parse(fit_parameters[0].ToString());

            // sort then take to last concentration
            var orderedZip = drc_points_x_enable.Zip(drc_points_y_enable, (x, y) => new { x, y })
                                  .OrderBy(pair => pair.x)
                                  .ToList();

            drc_points_x_enable = orderedZip.Select(pair => pair.x).ToList();
            drc_points_y_enable = orderedZip.Select(pair => pair.y).ToList();

            SortedDictionary<double, List<double>> dict_points = new SortedDictionary<double, List<double>>();

            for (int i = 0; i < drc_points_x_enable.Count(); i++)
            {
                //Console.WriteLine("x,y = " + x_concentrations[i] + " , " + y_response[i]);
                if (dict_points.ContainsKey(drc_points_x_enable[i]))
                {
                    dict_points[drc_points_x_enable[i]].Add(drc_points_y_enable[i]);
                }
                else
                {
                    List<double> resp = new List<double>();
                    resp.Add(drc_points_y_enable[i]);
                    dict_points.Add(drc_points_x_enable[i], resp);
                }
            }

            //// Print the dictionary
            //foreach(KeyValuePair<double, List<double>> elem in dict_points)
            //{
            //    double conc = elem.Key;
            //    List<double> resp = elem.Value;

            //    Console.WriteLine("Cpd, Concentration = " + compound_id + " , " + conc);
            //    foreach(double val in resp)
            //    {
            //        Console.WriteLine("-------Response = " + val);
            //    }
            //}

            if (dict_points.Count() > 2)
            {
                double response_last_point = 0.0;

                foreach (double val in dict_points.Values.ElementAt(dict_points.Count() - 1))
                {
                    response_last_point += val;
                }

                response_last_point /= (double)(dict_points.Values.ElementAt(dict_points.Count() - 1).Count());

                double response_2_last_point = 0.0;

                foreach (double val in dict_points.Values.ElementAt(dict_points.Count() - 2))
                {
                    response_2_last_point += val;
                }

                response_2_last_point /= (double)(dict_points.Values.ElementAt(dict_points.Count() - 2).Count());

                double diff_top_last_point = Math.Abs(response_last_point - top);
                double diff_top_last_point2 = Math.Abs(response_2_last_point - top);

                if (diff_top_last_point >= thr_2_last_points * Math.Abs(top - bottom) || diff_top_last_point2 >= thr_2_last_points * Math.Abs(top - bottom))
                {
                    Console.WriteLine("Concentration = " + compound_id);
                    Console.WriteLine("Diff last point, last point 2, thr*top = " + diff_top_last_point + " , " + diff_top_last_point2 + " , " + thr_2_last_points * Math.Abs(top - bottom));

                    draw_DRC(false, false);

                    is_ec50_exact = false;
                    ((RectangleAnnotation)chart.Annotations["menu_ec_50_sup"]).Text = ">";

                    annotation_ec50.Text = "EC_50 > " + Math.Pow(10, fit_parameters[2]).ToString("E2") + " | R2 = " + r2.ToString("N2");
                }

            }

        }

        public void check_toxicity(double thr_toxicity)
        {
            // Compute the top :
            double top = 0.0;

            //if (fit_parameters[0] < fit_parameters[1])
            //{
            top = double.Parse(fit_parameters[1].ToString());
            //}
            //else
            //{
            //    top = double.Parse(fit_parameters[0].ToString());
            //}

            // sort then take to last concentration
            var orderedZip = drc_points_x_enable.Zip(drc_points_y_enable, (x, y) => new { x, y })
                                  .OrderBy(pair => pair.x)
                                  .ToList();

            drc_points_x_enable = orderedZip.Select(pair => pair.x).ToList();
            drc_points_y_enable = orderedZip.Select(pair => pair.y).ToList();

            SortedDictionary<double, List<double>> dict_points = new SortedDictionary<double, List<double>>();

            for (int i = 0; i < drc_points_x_enable.Count(); i++)
            {
                //Console.WriteLine("x,y = " + x_concentrations[i] + " , " + y_response[i]);
                if (dict_points.ContainsKey(drc_points_x_enable[i]))
                {
                    dict_points[drc_points_x_enable[i]].Add(drc_points_y_enable[i]);
                }
                else
                {
                    List<double> resp = new List<double>();
                    resp.Add(drc_points_y_enable[i]);
                    dict_points.Add(drc_points_x_enable[i], resp);
                }
            }

            //// Print the dictionary
            //foreach(KeyValuePair<double, List<double>> elem in dict_points)
            //{
            //    double conc = elem.Key;
            //    List<double> resp = elem.Value;

            //    Console.WriteLine("Cpd, Concentration = " + compound_id + " , " + conc);
            //    foreach(double val in resp)
            //    {
            //        Console.WriteLine("-------Response = " + val);
            //    }
            //}

            if (dict_points.Count() > 2)
            {
                double response_last_point = 0.0;

                foreach (double val in dict_points.Values.ElementAt(dict_points.Count() - 1))
                {
                    response_last_point += val;
                }

                response_last_point /= (double)(dict_points.Values.ElementAt(dict_points.Count() - 1).Count());

                double GlobalMax = double.MinValue;
                double MaxValues = MaxA(drc_points_y_enable.ToArray());
                GlobalMax = MaxValues;

                double GlobalMin = double.MaxValue;
                double MinValues = MinA(drc_points_y_enable.ToArray());
                GlobalMin = MinValues;

                double min_max_activity = Math.Abs(GlobalMax - GlobalMin);

                if (Math.Abs(response_last_point - top) >= thr_toxicity * min_max_activity)
                {
                    //Console.WriteLine("Concentration = " + compound_id);
                    //Console.WriteLine("diff, min_max*thr = " + Math.Abs(response_last_point - top) + " , " + thr_toxicity * min_max_activity);

                    double point_x = dict_points.Keys.ElementAt(dict_points.Count() - 1);
                    List<double> list_point_y = dict_points.Values.ElementAt(dict_points.Count() - 1);

                    foreach (double y_val in list_point_y)
                    {
                        // Remove Points enabled
                        if (!(drc_points_x_disable.Contains(point_x) && drc_points_y_disable.Contains(y_val)))
                        {
                            drc_points_x_disable.Add(point_x);
                            drc_points_y_disable.Add(y_val);

                            int index = drc_points_y_enable.FindIndex(a => a < y_val + .0000001 && a > y_val - .0000001);

                            drc_points_x_enable.RemoveAt(index);
                            drc_points_y_enable.RemoveAt(index);

                            int index_raw_data = y_raw_data.FindIndex(a => a < y_val + .0000001 && a > y_val - .0000001);
                            is_raw_data_removed[index_raw_data] = true;
                        }
                    }
                }

                /* // Second points --> better to do the method 2 times with only the last point
                double response_2_last_point = 0.0;

                foreach (double val in dict_points.Values.ElementAt(dict_points.Count() - 2))
                {
                    response_2_last_point += val;
                }

                response_2_last_point /= (double)(dict_points.Values.ElementAt(dict_points.Count() - 2).Count());

                if (response_2_last_point <= thr_toxicity * top)
                {
                    double point_x = dict_points.Keys.ElementAt(dict_points.Count() - 2);
                    List<double> list_point_y = dict_points.Values.ElementAt(dict_points.Count() - 2);

                    foreach (double y_val in list_point_y)
                    {
                        // Remove Points enabled
                        if (!(drc_points_x_disable.Contains(point_x) && drc_points_y_disable.Contains(y_val)))
                        {
                            drc_points_x_disable.Add(point_x);
                            drc_points_y_disable.Add(y_val);

                            int index = drc_points_y_enable.FindIndex(a => a < y_val + .0000001 && a > y_val - .0000001);

                            drc_points_x_enable.RemoveAt(index);
                            drc_points_y_enable.RemoveAt(index);

                            int index_raw_data = y_raw_data.FindIndex(a => a < y_val + .0000001 && a > y_val - .0000001);
                            is_raw_data_removed[index_raw_data] = true;
                        }
                    }
                }
                */

                draw_DRC(false, false);

            }
        }

        public void draw_DRC(bool if_report, bool add_chart)
        {
            string cpd = compound_id;

            fit_DRC();

            chart.Titles["Title1"].Text = descriptor + " CPD = " + compound_id;

            // Draw the first graph
            chart.Series["Series1"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
            chart.Series["Series1"].Points.DataBindXY(x_concentrations, y_response);
            chart.Series["Series1"].Color = chart_color;

            chart.Series["Series2"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chart.Series["Series2"].Points.DataBindXY(x_fit, y_fit_log);
            chart.Series["Series2"].Color = chart_color;

            //----------------------------- Axis Labels ---------------------------//

            double min_x = 0.0;
            double max_x = 0.0;

            if (x_concentrations_log.Count > 0)
            {
                min_x = (int)Math.Floor(MinA<double>(x_concentrations_log.ToArray()));
                max_x = (int)Math.Ceiling(MaxA<double>(x_concentrations_log.ToArray()));
            }
            else
            {
                max_x = -5.0;
                min_x = -8.0;
            }

            if (minX < -0.5) minX = Math.Pow(10, min_x);
            else minX = chart.ChartAreas[0].AxisX.Minimum;

            if (maxX < -0.5) maxX = Math.Pow(10, max_x);
            else maxX = chart.ChartAreas[0].AxisX.Maximum;

            chart.ChartAreas[0].AxisX.Minimum = minX;
            chart.ChartAreas[0].AxisX.Maximum = maxX;

            chart.ChartAreas[0].AxisX.IsLogarithmic = true;
            chart.ChartAreas[0].AxisX.LogarithmBase = 10;
            chart.ChartAreas[0].AxisX.LabelStyle.Format = "E2";

            // End Axis Labels.

            foreach (DataPoint dp in chart.Series["Series1"].Points)
            {
                double point_x = dp.XValue;
                double point_y = dp.YValues[0];

                if (drc_points_x_disable.Contains(Math.Log10(point_x)) && drc_points_y_disable.Contains(point_y))
                {
                    dp.Color = Color.LightGray;
                    //continue;
                }
                // Remove Points enabled
                if (!(drc_points_x_disable.Contains(Math.Log10(point_x)) && drc_points_y_disable.Contains(point_y)))

                {
                    dp.Color = chart_color;
                }
            }

            if (drc_points_x_disable.Count() == 0) data_modified = false;
            else data_modified = true;

            int k = 0;
            foreach (DataGridViewRow row2 in _form1.f2.dataGridView2.Rows)
            {
                string compound = row2.Cells[0].Value.ToString();
                if (compound_id == compound) break;
                k++;
            }
            int row_index = k;

            _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 1].Value = double.Parse(Math.Pow(10, fit_parameters[2]).ToString("E2"));
            if (fit_parameters[0] < fit_parameters[1])
            {
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 2].Value = double.Parse(fit_parameters[0].ToString("E2"));
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 3].Value = double.Parse(fit_parameters[1].ToString("E2"));
            }
            else
            {
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 2].Value = double.Parse(fit_parameters[1].ToString("E2"));
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 3].Value = double.Parse(fit_parameters[0].ToString("E2"));
            }
            _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 4].Value = double.Parse(fit_parameters[3].ToString("E2"));
            _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 5].Value = double.Parse(r2.ToString("E2"));


            if (drc_points_x_disable.Count() > 0)
            {
                data_modified = true;
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 1].Style.BackColor = Color.LightSeaGreen;
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 2].Style.BackColor = Color.LightSeaGreen;
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 3].Style.BackColor = Color.LightSeaGreen;
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 4].Style.BackColor = Color.LightSeaGreen;
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 5].Style.BackColor = Color.LightSeaGreen;
            }
            else
            {
                data_modified = false;
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 1].Style.BackColor = Color.White;
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 2].Style.BackColor = Color.White;
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 3].Style.BackColor = Color.White;
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 4].Style.BackColor = Color.White;
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 5].Style.BackColor = Color.White;
            }

            // Setup visual attributes
            annotation_ec50.Text = "EC_50 = " + Math.Pow(10, fit_parameters[2]).ToString("E2") + " | R2 = " + r2.ToString("N2");
            annotation_ec50.BackColor = Color.FromArgb(240, 240, 240);
            annotation_ec50.AnchorX = 40;

            // test bottom top
            double the_top = double.Parse(fit_parameters[1].ToString());
            double MaxValues = MaxA(drc_points_y_enable.ToArray());
            double MinValues = MinA(drc_points_y_enable.ToArray());

            if ((MaxValues - the_top) < (the_top - MinValues))
            {
                annotation_ec50.AnchorY = 25;
            }
            else
            {
                annotation_ec50.AnchorY = 80;
            }

            annotation_ec50.AllowSelecting = true;
            annotation_ec50.AllowResizing = true;
            annotation_ec50.AllowMoving = true;
            annotation_ec50.AllowAnchorMoving = true;

            // Add the annotation to the collection
            chart.Annotations.Clear();
            chart.Annotations.Add(annotation_ec50);

            if (not_fitted)
            {
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 1].Value = "Not Fitted";
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 2].Value = "Not Fitted";
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 3].Value = "Not Fitted";
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 4].Value = "Not Fitted";
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 5].Value = "Not Fitted";

                data_modified = true;
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 1].Style.BackColor = Color.Tomato;
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 2].Style.BackColor = Color.Tomato;
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 3].Style.BackColor = Color.Tomato;
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 4].Style.BackColor = Color.Tomato;
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 5].Style.BackColor = Color.Tomato;

                annotation_ec50.Text = "EC_50 = Not Fitted";
            }

            if (inactive)
            {

                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 1].Value = "Inactive";
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 2].Value = "Inactive";
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 3].Value = "Inactive";
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 4].Value = "Inactive";
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 5].Value = "Inactive";

                data_modified = true;
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 1].Style.BackColor = Color.Orange;
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 2].Style.BackColor = Color.Orange;
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 3].Style.BackColor = Color.Orange;
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 4].Style.BackColor = Color.Orange;
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 5].Style.BackColor = Color.Orange;

                annotation_ec50.Text = "EC_50 = Inactive";
            }

            //chart.Invalidate();
            //chart.Update();
            //chart.Show();

            if (add_chart)
            {
                double ratio = 100.0 / (Math.Ceiling((double)_form1.get_descriptors_number() / 2.0));
                _form1.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, (float)ratio));

                _form1.tableLayoutPanel1.Controls.Add(chart);
            }

            chart.Anchor = (AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top);
            //chart_already_loaded = true;

            //RectangleAnnotation annotation_text = new RectangleAnnotation();
            //RectangleAnnotation mytext = new RectangleAnnotation();

            if (if_report == false)
            {
                RectangleAnnotation mytext = new RectangleAnnotation();
                //mytext.Bottom = 10;
                mytext.Name = "mytext";
                mytext.Text = "+";
                mytext.AnchorX = 97.5;
                mytext.AnchorY = 5;
                mytext.Height = 5;
                mytext.Width = 4;
                mytext.ForeColor = Color.Blue;
                mytext.Font = new Font(mytext.Font.FontFamily, mytext.Font.Size + 5.0f, mytext.Font.Style);
                mytext.Visible = true;
                chart.Annotations.Add(mytext);

                RectangleAnnotation menu_fit = new RectangleAnnotation();
                menu_fit.Name = "menu_fit";
                menu_fit.Text = "F";
                menu_fit.AnchorX = 93.5;
                menu_fit.AnchorY = 5;
                menu_fit.Height = 5;
                menu_fit.Width = 4;
                menu_fit.ForeColor = Color.Blue;
                menu_fit.Font = new Font(menu_fit.Font.FontFamily, menu_fit.Font.Size, FontStyle.Bold);
                menu_fit.Visible = true;
                chart.Annotations.Add(menu_fit);

                RectangleAnnotation menu_ec_50_sup = new RectangleAnnotation();
                menu_ec_50_sup.Name = "menu_ec_50_sup";

                if (is_ec50_exact) menu_ec_50_sup.Text = "=";
                else menu_ec_50_sup.Text = ">";

                menu_ec_50_sup.AnchorX = 89.5;
                menu_ec_50_sup.AnchorY = 5;
                menu_ec_50_sup.Height = 5;
                menu_ec_50_sup.Width = 4;
                menu_ec_50_sup.ForeColor = Color.Blue;
                menu_ec_50_sup.Font = new Font(menu_ec_50_sup.Font.FontFamily, menu_ec_50_sup.Font.Size, FontStyle.Bold);
                menu_ec_50_sup.Visible = true;
                chart.Annotations.Add(menu_ec_50_sup);


                RectangleAnnotation menu_not_fitted = new RectangleAnnotation();
                menu_not_fitted.Name = "menu_not_fitted";
                menu_not_fitted.Text = "NF";
                menu_not_fitted.AnchorX = 3.0;
                menu_not_fitted.AnchorY = 5;
                menu_not_fitted.Height = 5;
                menu_not_fitted.Width = 5;
                menu_not_fitted.ForeColor = Color.LightGray;
                menu_not_fitted.Font = new Font(menu_not_fitted.Font.FontFamily, menu_not_fitted.Font.Size, FontStyle.Bold);
                menu_not_fitted.Visible = true;
                chart.Annotations.Add(menu_not_fitted);

                RectangleAnnotation menu_inactive = new RectangleAnnotation();
                menu_inactive.Name = "menu_inactive";
                menu_inactive.Text = "I";
                menu_inactive.AnchorX = 7.5;
                menu_inactive.AnchorY = 5;
                menu_inactive.Height = 5;
                menu_inactive.Width = 4;
                menu_inactive.ForeColor = Color.LightGray;
                menu_inactive.Font = new Font(menu_inactive.Font.FontFamily, menu_inactive.Font.Size, FontStyle.Bold);
                menu_inactive.Visible = true;
                chart.Annotations.Add(menu_inactive);

                if (inactive)
                {
                    ((RectangleAnnotation)chart.Annotations["menu_inactive"]).ForeColor = Color.Orange;
                    ((RectangleAnnotation)chart.Annotations["menu_not_fitted"]).ForeColor = Color.LightGray;
                }

                if (not_fitted)
                {
                    ((RectangleAnnotation)chart.Annotations["menu_inactive"]).ForeColor = Color.LightGray;
                    ((RectangleAnnotation)chart.Annotations["menu_not_fitted"]).ForeColor = Color.Red;
                }
            }

        }

        Point mdown = Point.Empty;
        List<DataPoint> selectedPoints = null;

        private void chart1_MouseDown(object sender, MouseEventArgs e)
        {
            mdown = e.Location;
            selectedPoints = new List<DataPoint>();
        }

        private void chart1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                chart.Refresh();
                using (Graphics g = chart.CreateGraphics())
                    g.DrawRectangle(Pens.Red, GetRectangle(mdown, e.Location));
            }
        }

        private void chart1_MouseUp(object sender, MouseEventArgs e)
        {
            double pointer_y = e.Y;

            if (e.Button == MouseButtons.Left && pointer_y > 18)
            {

                Axis ax = chart.ChartAreas[0].AxisX;
                Axis ay = chart.ChartAreas[0].AxisY;
                Rectangle rect = GetRectangle(mdown, e.Location);

                int counter_point_changed = 0;

                foreach (DataPoint dp in chart.Series["Series1"].Points)
                {
                    int x = (int)ax.ValueToPixelPosition(dp.XValue);
                    int y = (int)ay.ValueToPixelPosition(dp.YValues[0]);

                    if (rect.Contains(new Point(x, y)))
                    {
                        counter_point_changed++;
                    }
                }

                foreach (DataPoint dp in chart.Series["Series1"].Points)
                {
                    int x = (int)ax.ValueToPixelPosition(dp.XValue);
                    int y = (int)ay.ValueToPixelPosition(dp.YValues[0]);

                    double point_x = Math.Log10(dp.XValue);
                    double point_y = dp.YValues[0];

                    if (rect.Contains(new Point(x, y)))
                    {
                        if (drc_points_x_disable.Contains(point_x) && drc_points_y_disable.Contains(point_y) && counter_point_changed > 0)
                        {
                            int index = 0;

                            List<int> indices = new List<int>();
                            for (int i = 0; i < drc_points_y_disable.Count(); i++)
                                if (drc_points_y_disable[i] < point_y + 1e-12 && drc_points_y_disable[i] > point_y - 1e-12)
                                    indices.Add(i);

                            foreach (int idx in indices)
                            {
                                if (drc_points_x_disable[idx] < (point_x + 1e-12) && drc_points_x_disable[idx] > (point_x - 1e-12))
                                {
                                    index = idx;
                                    break;
                                }
                            }

                            // Add points enabled
                            drc_points_x_enable.Add(point_x);
                            drc_points_y_enable.Add(point_y);

                            // Remove Points enabled
                            drc_points_x_disable.RemoveAt(index);
                            drc_points_y_disable.RemoveAt(index);

                            dp.Color = chart_color;

                            //point_last_change[data_point_idx] = true;
                            counter_point_changed--;
                        }
                    }
                }

                foreach (DataPoint dp in chart.Series["Series1"].Points)
                {
                    int x = (int)ax.ValueToPixelPosition(dp.XValue);
                    int y = (int)ay.ValueToPixelPosition(dp.YValues[0]);

                    double point_x = Math.Log10(dp.XValue);
                    double point_y = dp.YValues[0];

                    if (rect.Contains(new Point(x, y)))
                    {
                        if (drc_points_x_enable.Contains(point_x) && drc_points_y_enable.Contains(point_y) && counter_point_changed > 0)
                        {

                            int index = 0;
                            bool test = false;

                            List<int> indices = new List<int>();
                            for (int i = 0; i < drc_points_y_enable.Count(); i++)
                                if (drc_points_y_enable[i] < point_y + 1e-12 && drc_points_y_enable[i] > point_y - 1e-12)
                                    indices.Add(i);

                            foreach(int idx in indices)
                            {
                                if (drc_points_x_enable[idx] < (point_x + 1e-12) && drc_points_x_enable[idx] > (point_x - 1e-12))
                                {
                                    index = idx;
                                    break;
                                }
                            }

                            drc_points_x_disable.Add(point_x);
                            drc_points_y_disable.Add(point_y);

                            drc_points_x_enable.RemoveAt(index); //Add(data_chart[i].XValue);
                            drc_points_y_enable.RemoveAt(index); //Add(data_chart[i].YValues[0]);
                            dp.Color = Color.LightGray;

                            int index_raw_data = 0;

                            List<int> indices_raw = new List<int>();

                            for (int i = 0; i < y_raw_data.Count(); i++)
                                if (y_raw_data[i] < point_y + 1e-12 && y_raw_data[i] > point_y - 1e-12)
                                    indices_raw.Add(i);

                            foreach (int idx in indices_raw)
                            {
                                if (x_raw_data[idx] < (point_x + 1e-12) && x_raw_data[idx] > (point_x - 1e-12))
                                {
                                    index_raw_data = idx;
                                    break;
                                }
                            }

                            is_raw_data_removed[index_raw_data] = true;

                            counter_point_changed--;

                            //y_raw_data.FindIndex(a => a < point_y + 1e-12 && a > point_y - 1e-12);
                            //test = false;
                            //do
                            //{
                            //    index_raw_data = y_raw_data.FindIndex(a => a < point_y + 1e-12 && a > point_y - 1e-12);
                            //    if (drc_points_x_disable[index] < (point_x + 1e-12) && drc_points_x_disable[index] > (point_x - 1e-12)) test = true;

                            //} while (test == false);

                        }
                    }
                }

                fit_DRC();
                chart.Series["Series2"].Points.DataBindXY(x_fit, y_fit_log);

                int k = 0;
                foreach (DataGridViewRow row2 in _form1.f2.dataGridView2.Rows)
                {
                    string compound = row2.Cells[0].Value.ToString();
                    if (compound_id == compound) break;
                    k++;
                }
                int row_index = k;

                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 1].Value = double.Parse(Math.Pow(10, fit_parameters[2]).ToString("E2"));
                if (fit_parameters[0] < fit_parameters[1])
                {
                    _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 2].Value = double.Parse(fit_parameters[0].ToString("E2"));
                    _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 3].Value = double.Parse(fit_parameters[1].ToString("E2"));
                }
                else
                {
                    _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 2].Value = double.Parse(fit_parameters[1].ToString("E2"));
                    _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 3].Value = double.Parse(fit_parameters[0].ToString("E2"));
                }
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 4].Value = double.Parse(fit_parameters[3].ToString("E2"));
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 5].Value = double.Parse(r2.ToString("E2"));

                not_fitted = false;
                inactive = false;

                if (drc_points_x_disable.Count() > 0)
                {
                    data_modified = true;
                    _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 1].Style.BackColor = Color.LightSeaGreen;
                    _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 2].Style.BackColor = Color.LightSeaGreen;
                    _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 3].Style.BackColor = Color.LightSeaGreen;
                    _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 4].Style.BackColor = Color.LightSeaGreen;
                    _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 5].Style.BackColor = Color.LightSeaGreen;
                }
                else
                {
                    data_modified = false;
                    _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 1].Style.BackColor = Color.White;
                    _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 2].Style.BackColor = Color.White;
                    _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 3].Style.BackColor = Color.White;
                    _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 4].Style.BackColor = Color.White;
                    _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 5].Style.BackColor = Color.White;
                }

                ((RectangleAnnotation)chart.Annotations["menu_inactive"]).ForeColor = Color.LightGray;
                ((RectangleAnnotation)chart.Annotations["menu_not_fitted"]).ForeColor = Color.LightGray;

                annotation_ec50.Text = "EC_50 = " + Math.Pow(10, fit_parameters[2]).ToString("E2") + " | R2 = " + r2.ToString("N2");
            }
        }

        static public Rectangle GetRectangle(Point p1, Point p2)
        {
            return new Rectangle(Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y),
                Math.Abs(p1.X - p2.X), Math.Abs(p1.Y - p2.Y));
        }

        public double[] get_Fit_Parameters()
        {
            return fit_parameters;
        }

        public string save_image(string path)
        {
            draw_DRC(true, true);
            string descriptor_name = descriptor.Replace(@"/", @"_");
            string output_image = path + "/CPD_" + compound_id + "_" + descriptor_name + ".bmp";

            //System.Diagnostics.Debug.WriteLine("Write Image = " + output_image);
            chart.SaveImage(output_image, ChartImageFormat.Bmp);

            return output_image;
        }

        private void chart1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {

                int k = 0;
                foreach (DataGridViewRow row2 in _form1.f2.dataGridView2.Rows)
                {
                    string compound = row2.Cells[0].Value.ToString();
                    if (compound_id == compound) break;
                    k++;
                }
                int row_index = k;

                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 1].Value = "Not Fitted";
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 2].Value = "Not Fitted";
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 3].Value = "Not Fitted";
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 4].Value = "Not Fitted";
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 5].Value = "Not Fitted";

                data_modified = true;
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 1].Style.BackColor = Color.Tomato;
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 2].Style.BackColor = Color.Tomato;
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 3].Style.BackColor = Color.Tomato;
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 4].Style.BackColor = Color.Tomato;
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 5].Style.BackColor = Color.Tomato;

                annotation_ec50.Text = "EC_50 = Not Fitted";

                not_fitted = true;

                not_fitted_init = true;
                inactive_init = false;

                inactive = false;

                ((RectangleAnnotation)chart.Annotations["menu_inactive"]).ForeColor = Color.LightGray;
                ((RectangleAnnotation)chart.Annotations["menu_not_fitted"]).ForeColor = Color.Red;

            }

        }

        private void chart1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {

                int k = 0;
                foreach (DataGridViewRow row2 in _form1.f2.dataGridView2.Rows)
                {
                    string compound = row2.Cells[0].Value.ToString();
                    if (compound_id == compound) break;
                    k++;
                }
                int row_index = k;

                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 1].Value = "Inactive";
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 2].Value = "Inactive";
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 3].Value = "Inactive";
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 4].Value = "Inactive";
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 5].Value = "Inactive";

                data_modified = true;
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 1].Style.BackColor = Color.Orange;
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 2].Style.BackColor = Color.Orange;
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 3].Style.BackColor = Color.Orange;
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 4].Style.BackColor = Color.Orange;
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 5].Style.BackColor = Color.Orange;

                annotation_ec50.Text = "EC_50 = Inactive";

                inactive = true;

                inactive_init = true;
                not_fitted_init = false;

                not_fitted = false;

                ((RectangleAnnotation)chart.Annotations["menu_inactive"]).ForeColor = Color.Orange;
                ((RectangleAnnotation)chart.Annotations["menu_not_fitted"]).ForeColor = Color.LightGray;

            }

            if (e.Button == MouseButtons.Middle)
            {
                ColorDialog dlg = new ColorDialog();
                dlg.ShowDialog();

                Color new_color = dlg.Color;

                chart_color = new_color;

                re_fill_color(chart_color);
            }

        }

        //private void chart1_KeyPress(object sender, KeyPressEventArgs e)
        //{
        //    if (((Control.ModifierKeys & Keys.Control) == Keys.Control) && (e.KeyChar == 'M' || e.KeyChar == 'm'))
        //    {
        //        MessageBox.Show("test");
        //    }
        //}

        public void re_fill_color(Color new_color)
        {
            foreach (DataPoint dp in chart.Series["Series1"].Points)
            {
                dp.Color = new_color;
            }

            chart_color = new_color;
            chart.Series["Series2"].Color = new_color;
        }

        public void chart1_MouseClickMenu(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                double pointer_x = e.X;
                double pointer_y = e.Y;

                //MessageBox.Show(pointer_x + " , " + pointer_y);

                if (pointer_x >= 462 && pointer_y <= 18)
                {
                    Form fc = Application.OpenForms["Curves_Options"];

                    if (fc == null)
                        options_form = new Curves_Options(this);


                    minX = chart.ChartAreas[0].AxisX.Minimum;
                    double minY = chart.ChartAreas[0].AxisY.Minimum;
                    maxX = chart.ChartAreas[0].AxisX.Maximum;
                    double maxY = chart.ChartAreas[0].AxisY.Maximum;

                    options_form.set_curve_params(minX, maxX, minY, maxY, chart_color);

                    options_form.Visible = true;

                }

                if (pointer_x >= 443 && pointer_x < 462 && pointer_y <= 18)
                {
                    Form fc = Application.OpenForms["Curves_Fit_Options"];

                    if (fc == null)
                        options_fit_form = new Curve_Fit_Options(this);

                    options_fit_form.Visible = true;
                }

                if (pointer_x >= 422 && pointer_x < 443 && pointer_y <= 18)
                {
                    if (is_ec50_exact == true)
                    {
                        is_ec50_exact = false;
                        ((RectangleAnnotation)chart.Annotations["menu_ec_50_sup"]).Text = ">";
                        annotation_ec50.Text = "EC_50 > " + Math.Pow(10, fit_parameters[2]).ToString("E2") + " | R2 = " + r2.ToString("N2");

                    }
                    else
                    {
                        is_ec50_exact = true;
                        ((RectangleAnnotation)chart.Annotations["menu_ec_50_sup"]).Text = "=";
                        annotation_ec50.Text = "EC_50 = " + Math.Pow(10, fit_parameters[2]).ToString("E2") + " | R2 = " + r2.ToString("N2");

                    }
                }

                if (pointer_x >= 2 && pointer_x < 27 && pointer_y <= 18)
                {
                    if (not_fitted == false)
                    {
                        int k = 0;
                        foreach (DataGridViewRow row2 in _form1.f2.dataGridView2.Rows)
                        {
                            string compound = row2.Cells[0].Value.ToString();
                            if (compound_id == compound) break;
                            k++;
                        }
                        int row_index = k;

                        _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 1].Value = "Not Fitted";
                        _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 2].Value = "Not Fitted";
                        _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 3].Value = "Not Fitted";
                        _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 4].Value = "Not Fitted";
                        _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 5].Value = "Not Fitted";

                        data_modified = true;
                        _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 1].Style.BackColor = Color.Tomato;
                        _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 2].Style.BackColor = Color.Tomato;
                        _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 3].Style.BackColor = Color.Tomato;
                        _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 4].Style.BackColor = Color.Tomato;
                        _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 5].Style.BackColor = Color.Tomato;

                        annotation_ec50.Text = "EC_50 = Not Fitted";

                        not_fitted = true;

                        not_fitted_init = true;
                        inactive_init = false;

                        inactive = false;

                        ((RectangleAnnotation)chart.Annotations["menu_not_fitted"]).ForeColor = Color.Red;
                        ((RectangleAnnotation)chart.Annotations["menu_inactive"]).ForeColor = Color.LightGray;

                    }
                    else
                    {
                        ((RectangleAnnotation)chart.Annotations["menu_not_fitted"]).ForeColor = Color.LightGray;

                        int k = 0;
                        foreach (DataGridViewRow row2 in _form1.f2.dataGridView2.Rows)
                        {
                            string compound = row2.Cells[0].Value.ToString();
                            if (compound_id == compound) break;
                            k++;
                        }
                        int row_index = k;

                        _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 1].Value = double.Parse(Math.Pow(10, fit_parameters[2]).ToString("E2"));
                        if (fit_parameters[0] < fit_parameters[1])
                        {
                            _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 2].Value = double.Parse(fit_parameters[0].ToString("E2"));
                            _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 3].Value = double.Parse(fit_parameters[1].ToString("E2"));
                        }
                        else
                        {
                            _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 2].Value = double.Parse(fit_parameters[1].ToString("E2"));
                            _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 3].Value = double.Parse(fit_parameters[0].ToString("E2"));
                        }
                        _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 4].Value = double.Parse(fit_parameters[3].ToString("E2"));
                        _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 5].Value = double.Parse(r2.ToString("E2"));

                        not_fitted = false;
                        inactive = false;

                        if (drc_points_x_disable.Count() > 0)
                        {
                            data_modified = true;
                            _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 1].Style.BackColor = Color.LightSeaGreen;
                            _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 2].Style.BackColor = Color.LightSeaGreen;
                            _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 3].Style.BackColor = Color.LightSeaGreen;
                            _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 4].Style.BackColor = Color.LightSeaGreen;
                            _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 5].Style.BackColor = Color.LightSeaGreen;
                        }
                        else
                        {
                            data_modified = false;
                            _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 1].Style.BackColor = Color.White;
                            _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 2].Style.BackColor = Color.White;
                            _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 3].Style.BackColor = Color.White;
                            _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 4].Style.BackColor = Color.White;
                            _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 5].Style.BackColor = Color.White;
                        }

                        annotation_ec50.Text = "EC_50 = " + Math.Pow(10, fit_parameters[2]).ToString("E2") + " | R2 = " + r2.ToString("N2");
                    }
                }

                if (pointer_x >= 27 && pointer_x < 47 && pointer_y <= 18)
                {
                    if (inactive == false)
                    {
                        int k = 0;
                        foreach (DataGridViewRow row2 in _form1.f2.dataGridView2.Rows)
                        {
                            string compound = row2.Cells[0].Value.ToString();
                            if (compound_id == compound) break;
                            k++;
                        }
                        int row_index = k;

                        _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 1].Value = "Inactive";
                        _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 2].Value = "Inactive";
                        _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 3].Value = "Inactive";
                        _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 4].Value = "Inactive";
                        _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 5].Value = "Inactive";

                        data_modified = true;
                        _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 1].Style.BackColor = Color.Orange;
                        _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 2].Style.BackColor = Color.Orange;
                        _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 3].Style.BackColor = Color.Orange;
                        _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 4].Style.BackColor = Color.Orange;
                        _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 5].Style.BackColor = Color.Orange;

                        annotation_ec50.Text = "EC_50 = Inactive";

                        inactive = true;

                        inactive_init = true;
                        not_fitted_init = false;

                        not_fitted = false;

                        ((RectangleAnnotation)chart.Annotations["menu_inactive"]).ForeColor = Color.Orange;
                        ((RectangleAnnotation)chart.Annotations["menu_not_fitted"]).ForeColor = Color.LightGray;

                    }
                    else
                    {
                        ((RectangleAnnotation)chart.Annotations["menu_inactive"]).ForeColor = Color.LightGray;

                        int k = 0;
                        foreach (DataGridViewRow row2 in _form1.f2.dataGridView2.Rows)
                        {
                            string compound = row2.Cells[0].Value.ToString();
                            if (compound_id == compound) break;
                            k++;
                        }
                        int row_index = k;

                        _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 1].Value = double.Parse(Math.Pow(10, fit_parameters[2]).ToString("E2"));
                        if (fit_parameters[0] < fit_parameters[1])
                        {
                            _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 2].Value = double.Parse(fit_parameters[0].ToString("E2"));
                            _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 3].Value = double.Parse(fit_parameters[1].ToString("E2"));
                        }
                        else
                        {
                            _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 2].Value = double.Parse(fit_parameters[1].ToString("E2"));
                            _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 3].Value = double.Parse(fit_parameters[0].ToString("E2"));
                        }
                        _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 4].Value = double.Parse(fit_parameters[3].ToString("E2"));
                        _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 5].Value = double.Parse(r2.ToString("E2"));

                        not_fitted = false;
                        inactive = false;

                        if (drc_points_x_disable.Count() > 0)
                        {
                            data_modified = true;
                            _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 1].Style.BackColor = Color.LightSeaGreen;
                            _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 2].Style.BackColor = Color.LightSeaGreen;
                            _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 3].Style.BackColor = Color.LightSeaGreen;
                            _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 4].Style.BackColor = Color.LightSeaGreen;
                            _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 5].Style.BackColor = Color.LightSeaGreen;
                        }
                        else
                        {
                            data_modified = false;
                            _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 1].Style.BackColor = Color.White;
                            _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 2].Style.BackColor = Color.White;
                            _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 3].Style.BackColor = Color.White;
                            _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 4].Style.BackColor = Color.White;
                            _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 5].Style.BackColor = Color.White;
                        }

                        annotation_ec50.Text = "EC_50 = " + Math.Pow(10, fit_parameters[2]).ToString("E2") + " | R2 = " + r2.ToString("N2");
                    }
                }

            }

        }

        public void change_params(double min_x, double max_x, double min_y, double max_y, Color my_color)
        {
            chart_color = my_color;

            chart.ChartAreas[0].AxisX.Minimum = min_x;
            chart.ChartAreas[0].AxisX.Maximum = max_x;
            chart.ChartAreas[0].AxisY.Minimum = min_y;
            chart.ChartAreas[0].AxisY.Maximum = max_y;
        }

        public void remove_outlier_median(double thresold_median)
        {

            Dictionary<double, List<double>> points_dict = new Dictionary<double, List<double>>();

            foreach (DataPoint dp in chart.Series["Series1"].Points)
            {
                double point_x = Math.Log10(dp.XValue);
                double point_y = dp.YValues[0];

                if (points_dict.ContainsKey(point_x))
                {
                    points_dict[point_x].Add(point_y);
                }
                else
                {
                    List<double> my_list = new List<double>();
                    my_list.Add(point_y);

                    points_dict[point_x] = my_list;
                }
            }

            int counter = 0;

            foreach (var item in points_dict)
            {
                double x_points = item.Key;
                List<double> y_points = item.Value;

                y_points.Sort();

                double median_value = 0;
                int count = y_points.Count;

                if (count % 2 == 0 && count > 0)
                {
                    // count is even, need to get the middle two elements, add them together, then divide by 2
                    double middleElement1 = y_points[(count / 2) - 1];
                    double middleElement2 = y_points[(count / 2)];
                    median_value = (middleElement1 + middleElement2) / 2;
                }
                else
                {
                    median_value = y_points[(count / 2)];
                }

                //Compute the Average      
                double avg = y_points.Average();
                double sum = y_points.Sum(d => Math.Pow(d - avg, 2));
                double std_dev = Math.Sqrt((sum) / (y_points.Count() - 1));

                //if (counter < 1)
                //{
                //    string serie_line = "Line_" + counter.ToString();
                //    chart.Series.Add(serie_line);
                //    chart.Series[serie_line].Points.Add(new DataPoint(/*Math.Log10(*/x_points/*)*/, median_value + thresold_median * std_dev));
                //    chart.Series[serie_line].Points.Add(new DataPoint(/*Math.Log10(*/x_points/*)*/, median_value - thresold_median * std_dev));

                //    chart.Series[serie_line].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                //}

                counter++;

                foreach (double current_y in y_points)
                {
                    bool point_exclusion = false;

                    if (current_y > (median_value + thresold_median * std_dev) || current_y < (median_value - thresold_median * std_dev)) point_exclusion = true;

                    // Remove Points enabled
                    if (!(drc_points_x_disable.Contains(x_points) && drc_points_y_disable.Contains(current_y)) && point_exclusion)
                    {
                        drc_points_x_disable.Add(x_points);
                        drc_points_y_disable.Add(current_y);

                        int index = drc_points_y_enable.FindIndex(a => a < current_y + .0000001 && a > current_y - .0000001);

                        drc_points_x_enable.RemoveAt(index); //Add(data_chart[i].XValue);
                        drc_points_y_enable.RemoveAt(index); //Add(data_chart[i].YValues[0]);

                        //chart.Series["Series1"].Points[point_index].Color = Color.LightGray;

                        int index_raw_data = y_raw_data.FindIndex(a => a < current_y + .0000001 && a > current_y - .0000001);
                        is_raw_data_removed[index_raw_data] = true;
                    }
                    else if ((drc_points_x_disable.Contains(x_points) && drc_points_y_disable.Contains(current_y)) && !point_exclusion)
                    {
                        drc_points_x_enable.Add(x_points);
                        drc_points_y_enable.Add(current_y);

                        int index = drc_points_y_disable.FindIndex(a => a < current_y + .0000001 && a > current_y - .0000001);

                        drc_points_x_disable.RemoveAt(index); //Add(data_chart[i].XValue);
                        drc_points_y_disable.RemoveAt(index); //Add(data_chart[i].YValues[0]);

                        //chart.Series["Series1"].Points[point_index].Color = Color.LightGray;

                        int index_raw_data = y_raw_data.FindIndex(a => a < current_y + .0000001 && a > current_y - .0000001);
                        is_raw_data_removed[index_raw_data] = false;

                    }
                }

            }

            foreach (DataPoint dp in chart.Series["Series1"].Points)
            {
                double point_x = dp.XValue;
                double point_y = dp.YValues[0];

                if (drc_points_x_disable.Contains(Math.Log10(point_x)) && drc_points_y_disable.Contains(point_y))
                {
                    dp.Color = Color.LightGray;
                    //continue;
                }
                // Remove Points enabled
                if (!(drc_points_x_disable.Contains(Math.Log10(point_x)) && drc_points_y_disable.Contains(point_y)))
                {
                    dp.Color = chart_color;
                }
            }

            fit_DRC();
            chart.Series["Series2"].Points.DataBindXY(x_fit, y_fit_log);

            int k = 0;
            foreach (DataGridViewRow row2 in _form1.f2.dataGridView2.Rows)
            {
                string compound = row2.Cells[0].Value.ToString();
                if (compound_id == compound) break;
                k++;
            }
            int row_index = k;

            _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 1].Value = double.Parse(Math.Pow(10, fit_parameters[2]).ToString("E2"));
            if (fit_parameters[0] < fit_parameters[1])
            {
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 2].Value = double.Parse(fit_parameters[0].ToString("E2"));
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 3].Value = double.Parse(fit_parameters[1].ToString("E2"));
            }
            else
            {
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 2].Value = double.Parse(fit_parameters[1].ToString("E2"));
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 3].Value = double.Parse(fit_parameters[0].ToString("E2"));
            }
            _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 4].Value = double.Parse(fit_parameters[3].ToString("E2"));
            _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 5].Value = double.Parse(r2.ToString("E2"));

            not_fitted = false;
            inactive = false;

            if (drc_points_x_disable.Count() > 0)
            {
                data_modified = true;
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 1].Style.BackColor = Color.LightSeaGreen;
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 2].Style.BackColor = Color.LightSeaGreen;
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 3].Style.BackColor = Color.LightSeaGreen;
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 4].Style.BackColor = Color.LightSeaGreen;
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 5].Style.BackColor = Color.LightSeaGreen;
            }
            else
            {
                data_modified = false;
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 1].Style.BackColor = Color.White;
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 2].Style.BackColor = Color.White;
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 3].Style.BackColor = Color.White;
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 4].Style.BackColor = Color.White;
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 5].Style.BackColor = Color.White;
            }

            annotation_ec50.Text = "EC_50 = " + Math.Pow(10, fit_parameters[2]).ToString("E2") + " | R2 = " + r2.ToString("N2");
        }

    }


    public class Chart_DRC_Time_Line
    {
        MainTab _form1 = new MainTab();

        private Chart chart;

        private Dictionary<string, List<double>> drc_points_x = new Dictionary<string, List<double>>();
        private Dictionary<string, List<double>> drc_points_y = new Dictionary<string, List<double>>();
        private Dictionary<string, List<double>> drc_points_x_log = new Dictionary<string, List<double>>();

        private Dictionary<string, Color> chart_colors = new Dictionary<string, Color>();
        private List<string> filenames = new List<string>();

        private double[] fit_parameters = new double[4];

        private List<double> x_fit;
        private List<double> x_fit_log;
        private List<double> y_fit;

        private int step_curve;

        private double MinConcentrationLin;
        private double MaxConcentrationLin;

        private double r2;
        private double RelativeError;

        private string compound_id;
        private string descriptor;

        private bool first_curve_drawn = false;

        private bool not_fitted;

        private string file_name;

        private int series_number;

        private double min_y;
        private double max_y;

        private List<Color> curve_color = new List<Color>();

        //List<DataGridViewRow> raw_data;
        //List<double> y_raw_data;

        public bool is_Fitted()
        {
            return not_fitted;
        }

        public bool is_first_curve_drawn()
        {
            return first_curve_drawn;
        }

        public string get_Descriptor_Name()
        {
            return descriptor;
        }

        public double[] get_Fit_Parameters()
        {
            return fit_parameters;
        }

        public double get_R2()
        {
            return r2;
        }

        public List<string> get_filenames()
        {
            return filenames;
        }

        //private bool chart_already_loaded;

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

        public Chart_DRC_Time_Line() { }

        public Chart_DRC_Time_Line(string cpd, string descript, int step, ref List<double> x, ref List<double> x_log, ref List<double> y, Color color, MainTab form, string filename)
        {
            curve_color.Add(Color.Blue);
            curve_color.Add(Color.Red);
            curve_color.Add(Color.Green);
            curve_color.Add(Color.Black);
            curve_color.Add(Color.SaddleBrown);
            curve_color.Add(Color.OrangeRed);
            curve_color.Add(Color.DarkBlue);
            curve_color.Add(Color.DodgerBlue);
            curve_color.Add(Color.Tan);
            curve_color.Add(Color.DimGray);

            series_number = 1;

            _form1 = form;

            file_name = filename;
            filenames.Add(filename);

            compound_id = cpd;
            descriptor = descript;
            step_curve = step;
            chart_colors[file_name] = color;

            not_fitted = false;
            first_curve_drawn = true;

            drc_points_y[file_name] = y.ToList();

            drc_points_x_log[file_name] = x_log.ToList();

            drc_points_x[file_name] = x.ToList();

            double min_x = MinA(x.ToArray());
            double max_x = MaxA(x.ToArray());

            min_y = MinA(y.ToArray());
            max_y = MaxA(y.ToArray());

            MinConcentrationLin = min_x;
            MaxConcentrationLin = max_x;

            x_fit = new List<double>();
            x_fit_log = new List<double>();
            y_fit = new List<double>();

            for (int j = 0; j < step_curve; j++)
            {
                x_fit.Add(MinConcentrationLin + j * (MaxConcentrationLin - MinConcentrationLin) / (double)step_curve);
                x_fit_log.Add(Math.Log10(MinConcentrationLin) + j * (Math.Log10(MaxConcentrationLin) - Math.Log10(MinConcentrationLin)) / (double)step_curve);
            }

            chart = new Chart();

            ChartArea chartArea = new ChartArea();
            Series series1 = new Series();
            Series series2 = new Series();

            Axis yAxis = new Axis(chartArea, AxisName.Y);
            Axis xAxis = new Axis(chartArea, AxisName.X);

            chartArea.AxisX.LabelStyle.Format = "N2";
            chartArea.AxisX.Title = "Concentatrion";
            chartArea.AxisY.Title = "Response";

            //if (max_y < 1.0) chartArea.AxisY.Maximum = 1.0;

            chartArea.Name = descriptor;

            chart.ChartAreas.Add(chartArea);
            chart.Name = descriptor;

            chart.Location = new System.Drawing.Point(250, 100);

            series1.ChartType = SeriesChartType.Point;
            series2.ChartType = SeriesChartType.Line;

            series1.MarkerStyle = MarkerStyle.Circle;

            series1.Name = "DRC_Points";
            series2.Name = "DRC_Fit";

            chart.Series.Add(series1);
            chart.Series.Add(series2);

            chart.Size = new System.Drawing.Size(550, 350);

            chart.Titles.Add("Title1");

            fit_DRC();
        }

        public void add_serie_points(string file, ref List<double> x, ref List<double> x_log, ref List<double> y, Color color)
        {
            filenames.Add(file);

            series_number += 1;

            drc_points_x[file] = x;
            drc_points_x_log[file] = x_log;

            drc_points_y[file] = y;
            chart_colors[file] = color;

            Series series_new_points = new Series();

            series_new_points.ChartType = SeriesChartType.Point;
            series_new_points.MarkerStyle = MarkerStyle.Circle;
            series_new_points.Name = file;

            chart.Series.Add(series_new_points);

            draw_DRC();
        }

        public void remove_serie_points(string file)
        {
            if (file == file_name) return;

            if (drc_points_x.ContainsKey(file))
            {
                series_number -= 1;

                drc_points_x.Remove(file);
                drc_points_x_log.Remove(file);

                drc_points_y.Remove(file);
                chart_colors.Remove(file);

                chart.Series.Remove(chart.Series[file]);

                filenames.RemoveAll(p => p == file);
            }

            draw_DRC();
        }

        private static void function_SigmoidInhibition(double[] c, double[] x, ref double func, object obj)
        {
            func = c[0] + ((c[1] - c[0]) / (1 + Math.Pow(10, (c[2] - x[0]) * c[3])));
        }

        private double Sigmoid(double[] c, double x)
        {
            double y = c[0] + ((c[1] - c[0]) / (1 + Math.Pow(10, (c[2] - x) * c[3])));
            return y;
        }

        private void fit_DRC()
        {
            double GlobalMax = double.MinValue;
            double MaxValues = MaxA(drc_points_y[file_name].ToArray());

            GlobalMax = MaxValues + 0.5 * Math.Abs(MaxValues);

            double GlobalMin = double.MaxValue;
            double MinValues = MinA(drc_points_y[file_name].ToArray());

            GlobalMin = MinValues - 0.5 * Math.Abs(MinValues);

            //if ((double)_form1.numericUpDown3.Value != 0)
            //{
            //    GlobalMax = (double)_form1.numericUpDown3.Value;
            //}

            double BaseEC50 = Math.Log10(MaxConcentrationLin) - Math.Abs(Math.Log10(MaxConcentrationLin) - Math.Log10(MinConcentrationLin)) / 2.0;
            double[] c = new double[] { GlobalMin, GlobalMax, BaseEC50, 1 };

            double epsf = 0;
            double epsx = 0;

            int maxits = 0;
            int info;

            double[] bndl = null;
            double[] bndu = null;

            // boundaries
            bndu = new double[] { GlobalMax, GlobalMax, Math.Log10(MaxConcentrationLin) + 1.0, +100 };
            bndl = new double[] { GlobalMin, GlobalMin, Math.Log10(MinConcentrationLin) - 1.0, -100 };

            alglib.lsfitstate state;
            alglib.lsfitreport rep;
            double diffstep = 1e-12;

            // Fitting without weights
            //alglib.lsfitcreatefg(Concentrations, Values.ToArray(), c, false, out state);

            double[,] Concentration = new double[drc_points_x_log[file_name].Count(), 1];
            for (var i = 0; i < drc_points_x_log[file_name].Count(); ++i)
            {
                Concentration[i, 0] = drc_points_x_log[file_name][i];
            }

            int NumDimension = 1;
            alglib.lsfitcreatef(Concentration, drc_points_y[file_name].ToArray(), c, diffstep, out state);
            alglib.lsfitsetcond(state, epsx, maxits);
            alglib.lsfitsetbc(state, bndl, bndu);
            // alglib.lsfitsetscale(state, s);

            alglib.lsfitfit(state, function_SigmoidInhibition, null, null);
            alglib.lsfitresults(state, out info, out c, out rep);

            fit_parameters = c;
            RelativeError = rep.avgrelerror;
            r2 = rep.r2;

            y_fit.Clear();

            for (int IdxConc = 0; IdxConc < x_fit_log.Count; IdxConc++)
            {
                y_fit.Add(Sigmoid(c, x_fit_log[IdxConc]));
            }

        }

        public void draw_DRC()
        {
            string cpd = compound_id;

            fit_DRC();

            chart.Titles["Title1"].Text = descriptor + " CPD=" + compound_id;

            // Draw the first graph
            chart.Series["DRC_Points"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
            chart.Series["DRC_Points"].Points.DataBindXY(drc_points_x_log[file_name], drc_points_y[file_name]);
            chart.Series["DRC_Points"].Color = chart_colors[file_name];

            chart.Series["DRC_Fit"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chart.Series["DRC_Fit"].Points.DataBindXY(x_fit_log, y_fit);
            chart.Series["DRC_Fit"].Color = chart_colors[file_name];

            // Draw the other graph
            int counter_color = 0;

            foreach (KeyValuePair<string, List<double>> elem in drc_points_x)
            {
                if (elem.Key != file_name)
                {
                    chart.Series[elem.Key].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
                    chart.Series[elem.Key].Points.DataBindXY(drc_points_x_log[elem.Key], drc_points_y[elem.Key]);

                    if (counter_color + 1 >= curve_color.Count())
                    {
                        chart.Series[elem.Key].Color = curve_color[0];
                    }
                    else chart.Series[elem.Key].Color = curve_color[counter_color + 1];

                    counter_color++;
                }
            }

            double ratio = 100.0 / (Math.Ceiling((double)_form1.get_descriptors_number_time_line() / 2.0));
            _form1.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, (float)ratio));

            _form1.tableLayoutPanel1.Controls.Add(chart);
            chart.Anchor = (AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top);
        }

        public string save_image(string path)
        {
            draw_DRC();
            string descriptor_name = descriptor.Replace(@"/", @"_");
            string output_image = path + "/CPD_" + compound_id + "_" + descriptor_name + ".bmp";

            System.Diagnostics.Debug.WriteLine("Write Image = " + output_image);
            chart.SaveImage(output_image, ChartImageFormat.Bmp);

            return output_image;
        }
    }
}