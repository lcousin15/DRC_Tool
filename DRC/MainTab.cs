//using Accord.MachineLearning.DecisionTrees;
//using Accord.MachineLearning.DecisionTrees.Learning;
using Accord.MachineLearning.Clustering;
using Accord.Math;
using Accord.Statistics.Analysis;
using Accord.Statistics.Models.Regression.Linear;
using Accord.Math.Integration;
using LumenWorks.Framework.IO.Csv;
using Emgu.CV;
using Emgu.CV.Structure;
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
using System.Drawing.Drawing2D;

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

        public class RowComparer_Plate_Well : System.Collections.IComparer
        {
            private static int sortOrderModifier = 1;

            public RowComparer_Plate_Well(SortOrder sortOrder)
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
                int CompareResult = System.String.Compare(
                    DataGridViewRow1.Cells["Plate"].Value.ToString(),
                    DataGridViewRow2.Cells["Plate"].Value.ToString());

                // If the Last Names are equal, sort based on the First Name.
                if (CompareResult == 0)
                {
                    CompareResult = System.String.Compare(
                        DataGridViewRow1.Cells["Well"].Value.ToString(),
                        DataGridViewRow2.Cells["Well"].Value.ToString());
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

            dict_file_color = new Dictionary<string, Color>();
        }

        public CPD_Tab f2;
        public Clustering_Tab f6;
        public DRC_Overlap_Tab f10;
        public ViewList_CPD_Tab f11;
        public ViewImages_Options_Tab f13;
        public CPD_Time_Line TimeLine;
        public Export_Tab f5;
        public Patient_Tab form_patient;
        public Load_PS_Options Load_PS;
        public WellPlate_Viewer well_plate;
        public ViewCPD_Images_Tab f12;

        public void SetForm()
        {
            f2 = new CPD_Tab(this);
            f6 = new Clustering_Tab(this);
            f10 = new DRC_Overlap_Tab(this);
            f11 = new ViewList_CPD_Tab(this);
            TimeLine = new CPD_Time_Line(this);
            f5 = new Export_Tab(this);
            form_patient = new Patient_Tab(this);
            Load_PS = new Load_PS_Options(this);
            well_plate = new WellPlate_Viewer(this);
            f12 = new ViewCPD_Images_Tab(this);
        }

        public RawData_Tab f3 = new RawData_Tab();
        public RawDataDRC_Tab f4 = new RawDataDRC_Tab();
        public Correlations_Tab f7 = new Correlations_Tab();

        public Descriptors_General_Options descriptors_general_options_form;
        public Descriptors_Fix_Top_Options descriptors_fix_top_form;

        private string current_BATCH_ID;
        private Dictionary<string, int> cpd_row_index = new Dictionary<string, int>();
        public List<string> list_cpd;
        private int output_parameter_number;
        private int descritpor_number;
        private List<string> descriptor_list;

        private Dictionary<string, string> dict_cpd_BATCH_ID = new Dictionary<string, string>();

        private List<string> deslected_data_descriptor;
        private List<string> status_ec_50_descritpor;
        private List<string> bounds_descriptor;
        private List<string> fixed_top_descriptor;
        private List<string> data_modified_descriptor;

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

        public List<string> get_descriptor_list()
        {
            return descriptor_list;
        }

        CachedCsvReader csv;

        private bool is_with_plate;
        //private bool is_with_exp;

        private Random rnd = new Random();

        private List<List<string>> BATCH_ID_List = new List<List<string>>();
        //List<List<int>> Exp_ID_List = new List<List<int>>();

        private SortedDictionary<string, SortedDictionary<string, List<string>>> dict_plate_well_files = new SortedDictionary<string, SortedDictionary<string, List<string>>>();
        // plate, well path

        private Dictionary<string, DataTable> data_dict = new Dictionary<string, DataTable>(); // file --> DataTable
        private Dictionary<string, HashSet<string>> cpd_link = new Dictionary<string, HashSet<string>>(); // cpd id --> file
        public Dictionary<string, Color> dict_file_color; //= new Dictionary<string, Color>();

        private List<string> time_line_selected_descriptors = new List<string>();

        private Dictionary<string, string> template_plate_1 = new Dictionary<string, string>();
        private Dictionary<string, string> template_plate_2 = new Dictionary<string, string>();
        private Dictionary<string, Dictionary<string, double>> template_plate_concentration = new Dictionary<string, Dictionary<string, double>>();

        private Dictionary<string, Dictionary<string, Chart_DRC_Time_Line>> charts_time_line = new Dictionary<string, Dictionary<string, Chart_DRC_Time_Line>>(); // BATCH_ID, descriptor, chart

        private Dictionary<string, Chart_Patient> chart_auc = new Dictionary<string, Chart_Patient>();
        private Dictionary<string, Chart_Patient> chart_auc_z_score = new Dictionary<string, Chart_Patient>();

        private List<string> unique_plates = new List<string>();
        private List<double> ps_concentrations = new List<double>();
        private Dictionary<string, List<Chart_DRC>> dmso_charts = new Dictionary<string, List<Chart_DRC>>();

        private Dictionary<string, string> cpd_target = new Dictionary<string, string>();

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

        private List<Color> curve_color = new List<Color>();

        private double norm_integral;
        private bool is_patient = false;

        //public Dictionary<string, List<string>> list_img_path_by_cpd = new Dictionary<string, List<string>>();

        public Dictionary<string, Chart_Patient> get_charts_auc()
        {
            return chart_auc;
        }

        public Dictionary<string, Chart_Patient> get_charts_auc_z_score()
        {
            return chart_auc_z_score;
        }

        public Dictionary<string, string> get_ps_template_plate_1()
        {
            return template_plate_1;
        }
        public Dictionary<string, string> get_ps_template_plate_2()
        {
            return template_plate_2;
        }

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

        private double try_parse_string_in_double(string txt, int index, string descriptor, ref bool test)
        {
            double double_value = 0;

            if (Double.TryParse(txt, out double_value))
            {
                test = false;
                return double_value;
            }
            else
            {
                string message = "Error in Descriptor : " + descriptor + " / Row Index = " + (index + 2).ToString() + "\n Closing the program";
                const string caption = "Form Closing";
                var result = MessageBox.Show(message, caption,
                                             MessageBoxButtons.OK,
                                             MessageBoxIcon.Question);

                double_value = 1e-12;

                this.Reset();

                f2.Close();
                f6.Close();
                f10.Close();
                f11.Close();
                TimeLine.Close();
                f5.Close();
                form_patient.Close();
                Load_PS.Close();
                well_plate.Close();
                f12.Close();

                this.Close();

                test = true;

                return double_value;
            }
        }

        private void read_Data()
        {

            //f3.Show();
            f3.Hide();
            f3.dataGridView1.DataSource = csv;
            f4.dataGridView1.DataSource = csv;

            List<string> BATCH_ID = new List<string>();
            deslected_data_descriptor = new List<string>();
            status_ec_50_descritpor = new List<string>();
            bounds_descriptor = new List<string>();
            fixed_top_descriptor = new List<string>();
            data_modified_descriptor = new List<string>();

            if (!f3.dataGridView1.Columns.Contains("BATCH_ID") || !f3.dataGridView1.Columns.Contains("CPD_ID") && f3.dataGridView1.Columns.Contains("tags"))
            {
                //f3.dataGridView1.Columns.Add("BATCH_ID", "BATCH_ID");
                DataGridViewColumn new_col = ((DataGridViewColumn)f3.dataGridView1.Columns["tags"].Clone());
                new_col.Name = "BATCH_ID";
                new_col.HeaderText = "BATCH_ID";
                f3.dataGridView1.Columns.Add(new_col);

                DataGridViewColumn new_col2 = ((DataGridViewColumn)f3.dataGridView1.Columns["tags"].Clone());
                new_col2.Name = "CPD_ID";
                new_col2.HeaderText = "CPD_ID";
                f3.dataGridView1.Columns.Add(new_col2);
                //f3.dataGridView1.Columns["BATCH_ID"] = f3.dataGridView1.Columns["tags"];

            }

            if (f3.dataGridView1.ColumnCount < 5 || !f3.dataGridView1.Columns.Contains("BATCH_ID") || !f3.dataGridView1.Columns.Contains("Concentration")
                || !f3.dataGridView1.Columns.Contains("Plate") || !f3.dataGridView1.Columns.Contains("Well"))
            {
                System.Windows.Forms.MessageBox.Show("The file must contain at least these 6 columns : \n {[Plate, Well, Concentration, CPD_ID, BATCH_ID], Descr_0,...}", "Error",
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return;
            }

            //if (!f3.dataGridView1.Columns.Contains("BATCH_ID"))
            //{
            //    System.Windows.Forms.MessageBox.Show("BATCH_ID column doesn't exist.""Error",
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
                    BATCH_ID.Add(row.Cells["BATCH_ID"].Value.ToString() + "_" + row.Cells["Plate"].Value.ToString());
                }
                else BATCH_ID.Add(row.Cells["BATCH_ID"].Value.ToString());
            }

            var unique_items = new HashSet<string>(BATCH_ID);
            comboBox1.DataSource = unique_items.ToList<string>();

            foreach (DataGridViewColumn col in f3.dataGridView1.Columns)
            {
                string col_name = col.HeaderText;

                if (col_name != "Plate" && col_name != "Well" && col_name != "Concentration" && col_name != "Run"
                    && col_name != "CPD_ID" && col_name != "Class" && !col_name.StartsWith("Deselected") && col_name != "BATCH_ID"
                    && !col_name.StartsWith("Status") && !col_name.StartsWith("Bound") && !col_name.StartsWith("Fixed_Top")
                    && !col_name.StartsWith("Data_Modified"))
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

                if (col_name.StartsWith("Bound"))
                {
                    bounds_descriptor.Add(col_name);
                }

                if (col_name.StartsWith("Fixed_Top"))
                {
                    fixed_top_descriptor.Add(col_name);
                }
                if (col_name.StartsWith("Data_Modified"))
                {
                    data_modified_descriptor.Add(col_name);
                }
            }

            list_cpd = unique_items.ToList<string>();

        }

        private void Reset()
        {
            current_BATCH_ID = "";
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
            fixed_top_descriptor = new List<string>();
            data_modified_descriptor = new List<string>();

            BATCH_ID_List.Clear();

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
            current_BATCH_ID = CPD;

            if (CPD == "DMSO" || CPD == "Untreated")
                return;

            tableLayoutPanel1.Controls.Clear();

            //int test_modified = 0;

            if (descriptors_chart.Count == 0) return;

            List<Chart_DRC> list_chart = descriptors_chart[current_BATCH_ID];
            foreach (Chart_DRC current_chart in list_chart)
            {
                current_chart.draw_DRC(false, true);
                //test_modified += Convert.ToInt32(current_chart.is_data_modified());
            }

            //int k = 0;
            //foreach (DataGridViewRow row2 in f2.dataGridView2.Rows)
            //{
            //    string compound = row2.Cells[0].Value.ToString();
            //    if (current_BATCH_ID == compound) break;
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
            //    if (current_BATCH_ID == compound) break;
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

        public void draw_compound(string BATCH_ID)
        {
            if (BATCH_ID == "DMSO" || BATCH_ID == "Untreated")
                return;

            tableLayoutPanel1.Controls.Clear();

            List<Chart_DRC> list_chart = descriptors_chart[BATCH_ID];

            foreach (Chart_DRC current_chart in list_chart)
            {
                current_chart.draw_DRC(false, true);
            }
        }

        private static Image LoadImageNoLock(string path)
        {
            var stream = new MemoryStream(File.ReadAllBytes(path));
            return Image.FromStream(stream);
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

                f5.dataGridViewExport.ColumnCount = 2 + 3 * descriptor_list.Count;

                f5.dataGridViewExport.Columns[0].Name = "BATCH_ID";
                f5.dataGridViewExport.Columns[1].Name = "CPD_ID";

                int i = 0;
                foreach (string elem in descriptor_list)
                {

                    DataGridViewImageColumn img = new DataGridViewImageColumn();
                    f5.dataGridViewExport.Columns.Insert(4 * i + 2, img);

                    i++;
                }

                i = 0;
                foreach (string elem in descriptor_list)
                {
                    f5.dataGridViewExport.Columns[4 * i + 2].Name = elem;
                    f5.dataGridViewExport.Columns[4 * i + 3].Name = "Estimation";
                    f5.dataGridViewExport.Columns[4 * i + 4].Name = "EC_50 " + elem;
                    f5.dataGridViewExport.Columns[4 * i + 5].Name = "Top " + elem;

                    i++;
                }
                toolStripProgressBar1.Visible = true;
                for (var idx = 0; idx < list_cpd.Count; idx++)
                {
                    toolStripProgressBar1.Value = idx * 100 / (list_cpd.Count);
                    //toolStripStatusLabel1.Text = toolStripProgressBar1.Value.ToString();
                    //toolStripStatusLabel1.Visible=true;
                    string BATCH_ID = list_cpd[idx].ToString();

                    if (BATCH_ID.Contains("DMSO") || BATCH_ID.Contains("Untreated"))
                        continue;

                    tableLayoutPanel1.Controls.Clear();

                    List<Chart_DRC> list_chart = descriptors_chart[BATCH_ID];
                    List<string> list_images = new List<string>();

                    foreach (Chart_DRC current_chart in list_chart)
                    {
                        string image_path = current_chart.save_image(path);
                        list_images.Add(image_path);
                    }

                    // Export
                    //
                    // BATCH_ID | Image Nuc. | EC_50 Nuc. Or Not Fitted Green/Red Cell | Image R | EC_50 R or Not Fitted Green/Red Cell | etc... 
                    //

                    int index = f5.dataGridViewExport.Rows.Add();
                    f5.dataGridViewExport.Rows[index].Cells[0].Value = BATCH_ID;
                    f5.dataGridViewExport.Rows[index].Cells[1].Value = dict_cpd_BATCH_ID[BATCH_ID];

                    int i_img = 0;
                    foreach (Chart_DRC current_chart in list_chart)
                    {
                        double[] fit_params = current_chart.get_Fit_Parameters();
                        bool not_fitted = current_chart.is_Fitted();
                        bool inactive = current_chart.is_Inactive();
                        bool last_2_points_text = current_chart.check_ec50_exact();

                        double current_top = fit_params[1];
                        double current_ec_50 = fit_params[2];

                        Image image = LoadImageNoLock(list_images[i_img]);

                        //f5.dataGridViewExport.Rows[index].Height = 
                        f5.dataGridViewExport.Rows[index].Cells[i_img * 4 + 2].Value = image;
                        if (!not_fitted || !inactive)
                        {
                            if (last_2_points_text == true)
                            {
                                f5.dataGridViewExport.Rows[index].Cells[i_img * 4 + 3].Value = "=";
                                f5.dataGridViewExport.Rows[index].Cells[i_img * 4 + 3].Style.BackColor = Color.Green;

                                f5.dataGridViewExport.Rows[index].Cells[i_img * 4 + 4].Value = Math.Pow(10, current_ec_50).ToString("E2");
                                f5.dataGridViewExport.Rows[index].Cells[i_img * 4 + 4].Style.BackColor = Color.Green;

                                f5.dataGridViewExport.Rows[index].Cells[i_img * 4 + 5].Value = current_top.ToString("E2");
                                f5.dataGridViewExport.Rows[index].Cells[i_img * 4 + 5].Style.BackColor = Color.Green;
                            }
                            else
                            {
                                f5.dataGridViewExport.Rows[index].Cells[i_img * 4 + 3].Value = ">";
                                f5.dataGridViewExport.Rows[index].Cells[i_img * 4 + 3].Style.BackColor = Color.LimeGreen;

                                f5.dataGridViewExport.Rows[index].Cells[i_img * 4 + 4].Value = Math.Pow(10, current_ec_50).ToString("E2");
                                f5.dataGridViewExport.Rows[index].Cells[i_img * 4 + 4].Style.BackColor = Color.LimeGreen;

                                f5.dataGridViewExport.Rows[index].Cells[i_img * 4 + 5].Value = current_top.ToString("E2");
                                f5.dataGridViewExport.Rows[index].Cells[i_img * 4 + 5].Style.BackColor = Color.LimeGreen;
                            }
                        }
                        if (not_fitted)
                        {
                            f5.dataGridViewExport.Rows[index].Cells[i_img * 4 + 3].Value = "";
                            f5.dataGridViewExport.Rows[index].Cells[i_img * 4 + 3].Style.BackColor = Color.Tomato;

                            f5.dataGridViewExport.Rows[index].Cells[i_img * 4 + 4].Value = "Not Fitted";
                            f5.dataGridViewExport.Rows[index].Cells[i_img * 4 + 4].Style.BackColor = Color.Tomato;

                            f5.dataGridViewExport.Rows[index].Cells[i_img * 4 + 5].Value = "Not Fitted";
                            f5.dataGridViewExport.Rows[index].Cells[i_img * 4 + 5].Style.BackColor = Color.Tomato;
                        }
                        if (inactive)
                        {
                            f5.dataGridViewExport.Rows[index].Cells[i_img * 4 + 3].Value = "";
                            f5.dataGridViewExport.Rows[index].Cells[i_img * 4 + 3].Style.BackColor = Color.Orange;

                            f5.dataGridViewExport.Rows[index].Cells[i_img * 4 + 4].Value = "Inactive";
                            f5.dataGridViewExport.Rows[index].Cells[i_img * 4 + 4].Style.BackColor = Color.Orange;

                            f5.dataGridViewExport.Rows[index].Cells[i_img * 4 + 5].Value = "Inactive";
                            f5.dataGridViewExport.Rows[index].Cells[i_img * 4 + 5].Style.BackColor = Color.Orange;
                        }

                        i_img++;

                    }

                    foreach (string current_path in list_images) File.Delete(current_path);
                    //list_img_path_by_cpd.Add(BATCH_ID, list_images);
                }

                toolStripProgressBar1.Visible = false;
                f5.Show();
                MessageBox.Show("Images generated.");

                f5.saveToExcelToolStripMenuItem_Click(sender, e);
            }

        }

        public void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            comboBox1.Visible = true;

            openFileDialog1.Filter = "CSV Files (*.csv)|*.csv";
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Reset();

                this.Text = openFileDialog1.FileName;
                FileStream fs = new FileStream(openFileDialog1.FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                System.IO.StreamReader sr = new System.IO.StreamReader(fs);
                csv = new CachedCsvReader(sr, true);

                is_with_plate = false;

                read_Data();

            }

            return;
        }

        private void draw_drc()
        {

            if (is_patient) normalize_by_DMSO();

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

            f2.dataGridView2.Columns[0].Name = "BATCH_ID";

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
                string BATCH_ID = list_cpd[idx].ToString();

                if (BATCH_ID.Contains("DMSO") || BATCH_ID.Contains("Untreated"))
                    continue;

                // Add chart

                List<double> concentrations = new List<double>();
                List<double> concentrations_log = new List<double>();

                Dictionary<string, string> ec_50_status = new Dictionary<string, string>();
                Dictionary<string, string> fixed_top_status = new Dictionary<string, string>();
                Dictionary<string, string> data_modified_status = new Dictionary<string, string>();

                Dictionary<string, Dictionary<string, double>> fit_bounds = new Dictionary<string, Dictionary<string, double>>();

                List<DataGridViewRow> raw_data_rows = new List<DataGridViewRow>();

                data_descriptor.Clear();
                deselected_data_descriptor.Clear();

                foreach (DataGridViewRow row in f3.dataGridView1.Rows)
                {
                    string cpd_string = "";

                    if (is_with_plate == true) cpd_string = row.Cells["BATCH_ID"].Value.ToString() + "_" + row.Cells["Plate"].Value.ToString();
                    else cpd_string = row.Cells["BATCH_ID"].Value.ToString();

                    dict_cpd_BATCH_ID[row.Cells["BATCH_ID"].Value.ToString()] = row.Cells["CPD_ID"].Value.ToString();

                    if (cpd_string == BATCH_ID)
                    {
                        raw_data_rows.Add(row);

                        foreach (var item in checkedListBox1.CheckedItems)
                        {
                            string descriptor_name = item.ToString();
                            if (data_descriptor.ContainsKey(descriptor_name))
                            {
                                int row_index = row.Index;
                                bool test = false;

                                double cell_val = try_parse_string_in_double(row.Cells[item.ToString()].Value.ToString(), row_index, descriptor_name, ref test);

                                if (test) return;
                                data_descriptor[descriptor_name].Add(cell_val);
                            }
                            else
                            {
                                data_descriptor[descriptor_name] = new List<double>();

                                bool test = false;
                                int row_index = row.Index;

                                double cell_val = try_parse_string_in_double(row.Cells[item.ToString()].Value.ToString(), row_index, descriptor_name, ref test);

                                if (test) return;

                                data_descriptor[descriptor_name].Add(cell_val);
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

                        foreach (string item in bounds_descriptor)
                        {
                            string name = item.ToString();
                            string descriptor_name = name.Remove(0, 12);
                            string bound_name = name.Remove(0, 6);
                            int len = bound_name.Length;
                            bound_name = bound_name.Remove(5, len - 5);

                            if (fit_bounds.ContainsKey(descriptor_name))
                            {
                                Dictionary<string, double> bnd_temp = fit_bounds[descriptor_name];
                                if (bnd_temp.ContainsKey(bound_name))
                                {
                                    fit_bounds[descriptor_name][bound_name] = Double.Parse(row.Cells[name].Value.ToString());
                                }
                                else
                                {
                                    bnd_temp.Add(bound_name, Double.Parse(row.Cells[name].Value.ToString()));
                                }
                            }
                            else
                            {
                                Dictionary<string, Double> bnd_temp = new Dictionary<string, double>();
                                bnd_temp.Add(bound_name, Double.Parse(row.Cells[name].Value.ToString()));

                                fit_bounds.Add(descriptor_name, bnd_temp);
                            }

                        }

                        foreach (string item in fixed_top_descriptor)
                        {
                            string name = item.ToString();
                            string descriptor_name = name.Remove(0, 10);
                            fixed_top_status[descriptor_name] = row.Cells["Fixed_Top_" + descriptor_name].Value.ToString();
                        }

                        foreach (string item in data_modified_descriptor)
                        {
                            string name = item.ToString();
                            string descriptor_name = name.Remove(0, 14);
                            data_modified_status[descriptor_name] = row.Cells["Data_Modified_" + descriptor_name].Value.ToString();
                        }

                        int current_index = row.Index;
                        bool test_return = false;
                        double val = try_parse_string_in_double(row.Cells["Concentration"].Value.ToString(), current_index, "Concentration", ref test_return);

                        if (test_return) return;

                        concentrations.Add(val);
                        concentrations_log.Add(Math.Log10(val));
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

                    Dictionary<string, double> bounds = new Dictionary<string, double>();

                    if (fit_bounds.ContainsKey(descriptor_name)) bounds = fit_bounds[descriptor_name];

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

                    string fixed_top;
                    if (fixed_top_status.ContainsKey(descriptor_name)) fixed_top = fixed_top_status[descriptor_name];
                    else fixed_top = "Not Fixed";

                    string is_data_modified;
                    if (data_modified_status.ContainsKey(descriptor_name)) is_data_modified = data_modified_status[descriptor_name];
                    else is_data_modified = "FALSE";

                    Chart_DRC chart_drc = new Chart_DRC(BATCH_ID, descriptor_name, 250, ref concentrations, ref concentrations_log, ref data, color,
                        descriptor_index, deselected, chart_ec_50_status, bounds, fixed_top, is_data_modified, this, is_patient, true, true, true);

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

                descriptors_chart.Add(BATCH_ID, list_DRC_cpd);

                DataGridViewRow current_row = (DataGridViewRow)f2.dataGridView2.Rows[0].Clone();

                for (int i = 0; i < row_params.Count() + 1; i++)
                {
                    if (i == 0) current_row.Cells[i].Value = BATCH_ID;
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

                List<string> col_to_remove = new List<string>();

                int col_index = 0;
                foreach (DataGridViewColumn col in f3.dataGridView1.Columns)
                {
                    if (col.Name.StartsWith("Status_") || col.Name.StartsWith("Bound_") || col.Name.StartsWith("Deselected_")
                        || col.Name.StartsWith("Fixed_Top_") || col.Name.StartsWith("Data_Modified_"))
                    {
                        col_to_remove.Add(col.Name);
                        continue;
                    }

                    dataGridView4.Columns[col_index].Name = col.Name;
                    col_index++;
                }

                foreach (string col_name in col_to_remove)
                {
                    foreach (DataGridViewRow myRow in f3.dataGridView1.Rows)
                    {
                        myRow.Cells[col_name].Value = null;
                    }

                    f3.dataGridView1.Columns.Remove(f3.dataGridView1.Columns[col_name]);
                }

                dataGridView4.ColumnCount = col_index; // + descritpor_number;

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
                        //f3.dataGridView1.Columns.Remove(f3.dataGridView1.Columns[column_name]);
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

                        //f3.dataGridView1.Columns.Remove(f3.dataGridView1.Columns[column_name]);
                    }
                    else
                    {
                        dataGridView4.ColumnCount += 1;
                        dataGridView4.Columns[col_index].Name = column_name;
                        col_index++;
                    }

                }

                List<string> bnds_type = new List<string>();
                bnds_type.Add("min_x");
                bnds_type.Add("max_x");
                bnds_type.Add("min_y");
                bnds_type.Add("max_y");

                for (int descriptor_index = 0; descriptor_index < descritpor_number; descriptor_index++)
                {

                    foreach (string type in bnds_type)
                    {
                        string column_name = "Bound_" + type + "_" + descriptor_list[descriptor_index];

                        if (f3.dataGridView1.Columns.Contains(column_name))
                        {
                            foreach (DataGridViewRow myRow in f3.dataGridView1.Rows)
                            {
                                myRow.Cells[column_name].Value = null;
                            }

                            //f3.dataGridView1.Columns.Remove(f3.dataGridView1.Columns[column_name]);
                        }
                        else
                        {
                            dataGridView4.ColumnCount += 1;
                            dataGridView4.Columns[col_index].Name = column_name;
                            col_index++;
                        }
                    }
                }

                for (int descriptor_index = 0; descriptor_index < descritpor_number; descriptor_index++)
                {
                    string column_name = "Fixed_Top_" + descriptor_list[descriptor_index];

                    if (f3.dataGridView1.Columns.Contains(column_name))
                    {
                        foreach (DataGridViewRow myRow in f3.dataGridView1.Rows)
                        {
                            myRow.Cells[column_name].Value = null;
                        }

                        //f3.dataGridView1.Columns.Remove(f3.dataGridView1.Columns[column_name]);
                    }
                    else
                    {
                        dataGridView4.ColumnCount += 1;
                        dataGridView4.Columns[col_index].Name = column_name;
                        col_index++;
                    }
                }

                for (int descriptor_index = 0; descriptor_index < descritpor_number; descriptor_index++)
                {
                    string column_name = "Data_Modified_" + descriptor_list[descriptor_index];

                    if (f3.dataGridView1.Columns.Contains(column_name))
                    {
                        foreach (DataGridViewRow myRow in f3.dataGridView1.Rows)
                        {
                            myRow.Cells[column_name].Value = null;
                        }

                        //f3.dataGridView1.Columns.Remove(f3.dataGridView1.Columns[column_name]);
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
                    string BATCH_ID = list_cpd[idx].ToString();

                    if (BATCH_ID.Contains("DMSO") || BATCH_ID.Contains("Untreated"))
                        continue;

                    List<Chart_DRC> list_chart = descriptors_chart[BATCH_ID];

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

                    foreach (Chart_DRC current_chart in list_chart)
                    {
                        string descriptor_name = current_chart.get_Descriptor_Name();

                        List<bool> removed_raw_data_cpd = new List<bool>();

                        removed_raw_data_cpd = current_chart.get_Removed_Raw_Data().ToList();

                        double bound_x_min = current_chart.get_min_bound_x();
                        double bound_y_min = current_chart.get_min_bound_y();

                        double bound_x_max = current_chart.get_max_bound_x();
                        double bound_y_max = current_chart.get_max_bound_y();

                        int k = 0;
                        foreach (bool elem in removed_raw_data_cpd)
                        {

                            DataGridViewTextBoxCell newCell = new DataGridViewTextBoxCell();
                            newCell.Value = bound_x_min;

                            chart_row_data[k].Cells.Add(newCell);

                            ++k;
                        }

                        k = 0;
                        foreach (bool elem in removed_raw_data_cpd)
                        {

                            DataGridViewTextBoxCell newCell = new DataGridViewTextBoxCell();
                            newCell.Value = bound_x_max;

                            chart_row_data[k].Cells.Add(newCell);

                            ++k;
                        }

                        k = 0;
                        foreach (bool elem in removed_raw_data_cpd)
                        {

                            DataGridViewTextBoxCell newCell = new DataGridViewTextBoxCell();
                            newCell.Value = bound_y_min;

                            chart_row_data[k].Cells.Add(newCell);

                            ++k;
                        }

                        k = 0;
                        foreach (bool elem in removed_raw_data_cpd)
                        {

                            DataGridViewTextBoxCell newCell = new DataGridViewTextBoxCell();
                            newCell.Value = bound_y_max;

                            chart_row_data[k].Cells.Add(newCell);

                            ++k;
                        }
                    }

                    foreach (Chart_DRC current_chart in list_chart)
                    {

                        string descriptor_name = current_chart.get_Descriptor_Name();

                        List<bool> removed_raw_data_cpd = new List<bool>();

                        removed_raw_data_cpd = current_chart.get_Removed_Raw_Data().ToList();
                        bool is_top_fixed = current_chart.top_fixed();

                        int k = 0;

                        foreach (bool elem in removed_raw_data_cpd)
                        {

                            DataGridViewTextBoxCell newCell = new DataGridViewTextBoxCell();
                            if (is_top_fixed) newCell.Value = current_chart.get_top_fixed();
                            else newCell.Value = "Not Fixed";

                            chart_row_data[k].Cells.Add(newCell);

                            ++k;
                        }
                    }

                    foreach (Chart_DRC current_chart in list_chart)
                    {
                        string descriptor_name = current_chart.get_Descriptor_Name();

                        List<bool> removed_raw_data_cpd = new List<bool>();

                        removed_raw_data_cpd = current_chart.get_Removed_Raw_Data().ToList();
                        bool data_modified = current_chart.is_data_modified();

                        int k = 0;
                        foreach (bool elem in removed_raw_data_cpd)
                        {
                            DataGridViewTextBoxCell newCell = new DataGridViewTextBoxCell();
                            newCell.Value = data_modified;

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

        public void loadWithPlateToolStripMenuItem_Click(object sender, EventArgs e)
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
                        if ((f2.dataGridView2.Rows[i].Cells[j].Value.ToString() == "Not Fitted") || (f2.dataGridView2.Rows[i].Cells[j].Value.ToString() == "Inactive"))
                        {
                            current_row.Add(-1);
                        }
                        else
                        {
                            current_row.Add((double)f2.dataGridView2.Rows[i].Cells[j].Value);
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
                        if ((f2.dataGridView2.Rows[i].Cells[j].Value.ToString() == "Not Fitted") || (f2.dataGridView2.Rows[i].Cells[j].Value.ToString() == "Inactive")) current_row.Add(-1);
                        else current_row.Add((double)f2.dataGridView2.Rows[i].Cells[j].Value);
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

        public void correlationsToolStripMenuItem2_Click(object sender, EventArgs e)
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

                BATCH_ID_List.Clear();

                List<List<string>> deslected_data_descriptor_list = new List<List<string>>();

                if (f3.dataGridView1.ColumnCount < 5)
                {
                    System.Windows.Forms.MessageBox.Show("The file should contain at least 5 columns\n Plate,Well,Concentration,BATCH_ID,Descr_0,...");
                    return;
                }

                if (!f3.dataGridView1.Columns.Contains("BATCH_ID"))
                {
                    System.Windows.Forms.MessageBox.Show("BATCH_ID column doesn't exist.");
                    return;
                }

                if (!f3.dataGridView1.Columns.Contains("Concentration"))
                {
                    System.Windows.Forms.MessageBox.Show("Concentration column doesn't exist.");
                    return;
                }

                List<string> BATCH_ID = new List<string>();

                foreach (DataGridViewRow row in f3.dataGridView1.Rows)
                {
                    BATCH_ID.Add(row.Cells["BATCH_ID"].Value.ToString());
                }

                BATCH_ID_List.Add(BATCH_ID);

                // Features checkbox dataGridView1 :
                foreach (DataGridViewColumn col in f3.dataGridView1.Columns)
                {
                    string col_name = col.HeaderText;
                    if (col_name != "Plate" && col_name != "Well" && col_name != "Concentration" && col_name != "Run" && col_name != "BATCH_ID" && col_name != "Class" && !col_name.StartsWith("Deselected"))
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

                List<string> BATCH_ID_2 = new List<string>();

                foreach (DataGridViewRow row in f3.dataGridView2.Rows)
                {
                    BATCH_ID_2.Add(row.Cells["BATCH_ID"].Value.ToString());
                }

                BATCH_ID_List.Add(BATCH_ID_2);
            }

            if (BATCH_ID_List.Count == 2)
            {
                var unique_items_1 = new HashSet<string>(BATCH_ID_List[0]);
                var unique_items_2 = new HashSet<string>(BATCH_ID_List[1]);

                var unique_BATCH_ID = unique_items_1.Intersect(unique_items_2);

                comboBox1.DataSource = unique_BATCH_ID.ToList<string>();
                list_cpd = unique_BATCH_ID.ToList<string>();
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

            f10.dataGridView1.ColumnCount = 1 + BATCH_ID_List.Count() * output_parameter_number * checked_items;

            f10.dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders;

            f10.dataGridView1.Columns[0].Name = "BATCH_ID";

            descriptor_list = new List<string>();

            Dictionary<string, List<double>> data_descriptor_1 = new Dictionary<string, List<double>>();
            Dictionary<string, List<double>> data_descriptor_2 = new Dictionary<string, List<double>>();


            //Dictionary<string, List<string>> deselected_data_descriptor = new Dictionary<string, List<string>>();


            int j = 0;

            foreach (var item in checkedListBox1.CheckedItems)
            {
                for (int k = 0; k < BATCH_ID_List.Count; ++k)
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
                string BATCH_ID = list_cpd[idx].ToString();

                if (BATCH_ID.Contains("DMSO") || BATCH_ID.Contains("Untreated"))
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

                    cpd_string = row.Cells["BATCH_ID"].Value.ToString();

                    if (cpd_string == BATCH_ID)
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
                                    int row_index = row.Index;
                                    bool test = false;

                                    double cell_val = try_parse_string_in_double(row.Cells[descriptor_name].Value.ToString(), row_index, descriptor_name, ref test);

                                    if (test) return;

                                    data_descriptor_1[descriptor_name].Add(cell_val);
                                }
                                else
                                {
                                    data_descriptor_1[descriptor_name] = new List<double>();
                                    int row_index = row.Index;
                                    bool test = false;

                                    double cell_val = try_parse_string_in_double(row.Cells[descriptor_name].Value.ToString(), row_index, descriptor_name, ref test);

                                    if (test) return;

                                    data_descriptor_1[descriptor_name].Add(cell_val);
                                }

                                if (concentrations_1.ContainsKey(descriptor_name))
                                {
                                    int row_index = row.Index;
                                    bool test = false;

                                    double cell_val = try_parse_string_in_double(row.Cells["Concentration"].Value.ToString(), row_index, descriptor_name, ref test);

                                    if (test) return;

                                    concentrations_1[descriptor_name].Add(cell_val);
                                }
                                else
                                {
                                    concentrations_1[descriptor_name] = new List<double>();
                                    int row_index = row.Index;
                                    bool test = false;

                                    double cell_val = try_parse_string_in_double(row.Cells["Concentration"].Value.ToString(), row_index, descriptor_name, ref test);

                                    if (test) return;

                                    concentrations_1[descriptor_name].Add(cell_val);
                                }

                                if (concentrations_log_1.ContainsKey(descriptor_name))
                                {
                                    int row_index = row.Index;
                                    bool test = false;

                                    double cell_val = try_parse_string_in_double(row.Cells["Concentration"].Value.ToString(), row_index, "Concentration", ref test);

                                    if (test) return;

                                    concentrations_log_1[descriptor_name].Add(Math.Log10(cell_val));
                                }
                                else
                                {
                                    concentrations_log_1[descriptor_name] = new List<double>();
                                    int row_index = row.Index;

                                    bool test = false;

                                    double cell_val = try_parse_string_in_double(row.Cells["Concentration"].Value.ToString(), row_index, "Concentration", ref test);

                                    if (test) return;

                                    concentrations_log_1[descriptor_name].Add(Math.Log10(cell_val));
                                }
                            }
                        }

                    }
                }

                foreach (DataGridViewRow row in f3.dataGridView2.Rows)
                {
                    string cpd_string = "";

                    cpd_string = row.Cells["BATCH_ID"].Value.ToString();

                    if (cpd_string == BATCH_ID)
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
                                    int row_index = row.Index;
                                    bool test = false;

                                    double cell_val = try_parse_string_in_double(row.Cells[descriptor_name].Value.ToString(), row_index, descriptor_name, ref test);

                                    if (test) return;

                                    data_descriptor_2[descriptor_name].Add(cell_val);
                                }
                                else
                                {
                                    data_descriptor_2[descriptor_name] = new List<double>();
                                    bool test = false;

                                    int row_index = row.Index;
                                    double cell_val = try_parse_string_in_double(row.Cells[descriptor_name].Value.ToString(), row_index, descriptor_name, ref test);

                                    if (test) return;

                                    data_descriptor_2[descriptor_name].Add(cell_val);
                                }

                                if (concentrations_2.ContainsKey(descriptor_name))
                                {
                                    int row_index = row.Index;
                                    bool test = false;

                                    double cell_val = try_parse_string_in_double(row.Cells["Concentration"].Value.ToString(), row_index, "Concentration", ref test);

                                    if (test) return;

                                    concentrations_2[descriptor_name].Add(cell_val);
                                }
                                else
                                {
                                    concentrations_2[descriptor_name] = new List<double>();

                                    int row_index = row.Index;
                                    bool test = false;

                                    double cell_val = try_parse_string_in_double(row.Cells["Concentration"].Value.ToString(), row_index, "Concentration", ref test);

                                    if (test) return;

                                    concentrations_2[descriptor_name].Add(cell_val);
                                }

                                if (concentrations_log_2.ContainsKey(descriptor_name))
                                {
                                    int row_index = row.Index;
                                    bool test = false;

                                    double cell_val = try_parse_string_in_double(row.Cells["Concentration"].Value.ToString(), row_index, "Concentration", ref test);

                                    if (test) return;

                                    concentrations_log_2[descriptor_name].Add(Math.Log10(cell_val));
                                }
                                else
                                {
                                    concentrations_log_2[descriptor_name] = new List<double>();

                                    int row_index = row.Index;
                                    bool test = false;

                                    double cell_val = try_parse_string_in_double(row.Cells["Concentration"].Value.ToString(), row_index, "Concentration", ref test);

                                    if (test) return;

                                    concentrations_log_2[descriptor_name].Add(Math.Log10(cell_val));
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

                        Chart_DRC_Overlap chart_drc_overlap = new Chart_DRC_Overlap(BATCH_ID, descriptor_name, 100, ref conc_1, ref conc_1_log,
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

                descriptors_chart_overlap.Add(BATCH_ID, list_DRC_cpd);

                DataGridViewRow current_row = (DataGridViewRow)f10.dataGridView1.Rows[0].Clone();

                for (int i = 0; i < row_params.Count() + 1; i++)
                {
                    if (i == 0) current_row.Cells[i].Value = BATCH_ID;
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
            this.toolStripProgressBar1.Visible = true;

            // threshold R2
            double r2_threshold = double.Parse(this.numericUpDown1.Value.ToString());

            for (var idx = 0; idx < list_cpd.Count; idx++)
            {
                this.toolStripProgressBar1.Value = idx * 100 / (list_cpd.Count - 1);

                string BATCH_ID = list_cpd[idx].ToString();

                if (BATCH_ID == "DMSO" || BATCH_ID == "Untreated")
                    continue;

                List<Chart_DRC> list_chart = descriptors_chart[BATCH_ID];

                foreach (Chart_DRC current_chart in list_chart)
                {
                    current_chart.threshold_r2(r2_threshold);
                    current_chart.Is_Modified();
                }
            }

            this.toolStripProgressBar1.Visible = false;

        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.toolStripProgressBar1.Visible = true;

            // threshold Inactive
            double inactive_threshold = double.Parse(this.numericUpDown2.Value.ToString());

            for (var idx = 0; idx < list_cpd.Count; idx++)
            {
                this.toolStripProgressBar1.Value = idx * 100 / (list_cpd.Count - 1);

                string BATCH_ID = list_cpd[idx].ToString();

                if (BATCH_ID == "DMSO" || BATCH_ID == "Untreated")
                    continue;

                List<Chart_DRC> list_chart = descriptors_chart[BATCH_ID];

                foreach (Chart_DRC current_chart in list_chart)
                {
                    current_chart.threshold_inactive(inactive_threshold);
                    current_chart.Is_Modified();
                }
            }

            this.toolStripProgressBar1.Visible = false;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            // threshold Inactive
            double median_treshold = double.Parse(this.numericUpDown4.Value.ToString());
            // Threshold % actvity
            double thr_activity = double.Parse(this.numericUpDown3.Value.ToString());

            for (var idx = 0; idx < list_cpd.Count; idx++)
            {
                this.toolStripProgressBar1.Value = idx * 100 / (list_cpd.Count - 1);

                string BATCH_ID = list_cpd[idx].ToString();

                if (BATCH_ID == "DMSO" || BATCH_ID == "Untreated")
                    continue;

                List<Chart_DRC> list_chart = descriptors_chart[BATCH_ID];

                foreach (Chart_DRC current_chart in list_chart)
                {
                    current_chart.remove_outlier_median(median_treshold, thr_activity);
                    current_chart.Is_Modified();
                }
            }

            this.toolStripProgressBar1.Visible = false;
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

        public void checkImagesToolStripMenuItem_Click(object sender, EventArgs e)
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
            f11.dataGridView1.Columns[0].Name = "BATCH_ID";

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
        /// 

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
                f12 = new ViewCPD_Images_Tab(this);

            if (view_images_per_concentration == true)
            {
                f12.dataGridView1.Rows.Clear();
                f12.dataGridView1.Columns.Clear();
                f12.dataGridView1.Refresh();
            }

            f12.view_images_per_concentration = view_images_per_concentration;

            f12.Visible = true;

            string BATCH_ID = f11.dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString();

            f12.Text = BATCH_ID;

            f13 = new ViewImages_Options_Tab(this, BATCH_ID);

            //f13.Visible = false;
            //f13.comboBox2.SelectedIndex = 1;
            //f13.comboBox3.SelectedIndex = 0;

            f13.Visible = true;

        }

        public void load_cpd_images(string BATCH_ID, bool view_options)
        {
            f11.Visible = false;

            Form fc = Application.OpenForms["ViewCPD_Images_Tab"];

            if (fc == null)
                f12 = new ViewCPD_Images_Tab(this);

            f12.Text = BATCH_ID;

            f12.view_images_per_concentration = view_options; // true in cpd main tab

            if (view_images_per_concentration == true)
            {
                f12.dataGridView1.Rows.Clear();
                f12.dataGridView1.Columns.Clear();
                f12.dataGridView1.Refresh();
            }

            f12.Visible = true;

            f13 = new ViewImages_Options_Tab(this, BATCH_ID);
            f13.Visible = true;

        }

        public void load_cpd_images(List<string> list_BATCH_ID)       // need to debug this part--> add rows for each cpd
        {
            f11.Visible = true;

            Form fc = Application.OpenForms["ViewCPD_Images_Tab"];

            if (fc == null)
                f12 = new ViewCPD_Images_Tab(this);

            f12.Text = "Compounds Hits";

            f12.view_images_per_concentration = view_images_per_concentration;

            if (view_images_per_concentration == true)
            {
                f12.dataGridView1.Rows.Clear();
                f12.dataGridView1.Columns.Clear();
                f12.dataGridView1.Refresh();
            }

            f12.Visible = true;

            f13 = new ViewImages_Options_Tab(this, list_BATCH_ID);
            f13.Visible = true;

            //for(int k = 1; k <list_BATCH_ID.Count; ++k) draw_images(list_BATCH_ID[k]);

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
                draw_images(cpd, progress, list_cpd.Count);
                progress++;
                f12.toolStripProgressBar1.Value = progress * 100 / list_cpd.Count;

            }

            f12.toolStripProgressBar1.Visible = false;
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

        public void draw_images(string BATCH_ID, int BATCH_IDx, int cpd_nb)
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
            //f3.dataGridView1.AllowUserToAddRows = false;
            f3.dataGridView3.AllowUserToAddRows = false;

            copy_data_grid_view(ref f3.dataGridView1, ref f3.dataGridView3);

            //f3.dataGridView2.Refresh();
            //f3.dataGridView2.Sort(new RowComparer(SortOrder.Descending));

            //copy_data_grid_view(ref f3.dataGridView2, ref f3.dataGridView1);
            //f3.dataGridView1.Refresh();

            ////f3.Show();

            foreach (DataGridViewRow row in f3.dataGridView3.Rows)
            {
                string current_cpd = row.Cells["BATCH_ID"].Value.ToString();
                if (current_cpd == BATCH_ID)
                {
                    plates.Add(row.Cells["Plate"].Value.ToString());
                    wells.Add(row.Cells["Well"].Value.ToString());
                    concentrations.Add(double.Parse(row.Cells["Concentration"].Value.ToString()));

                    foreach (DataGridViewColumn col in f3.dataGridView1.Columns)
                    {
                        string col_name = col.HeaderText;
                        if (col_name != "BATCH_ID" && col_name != "Plate" && col_name != "Well" && col_name != "Concentration"
                            && col_name != "Class" && col_name != "CPD_ID" && !col_name.StartsWith("Status") && !col_name.StartsWith("Bound")
                            && !col_name.StartsWith("Fixed_Top") && !col_name.StartsWith("Data_Modified") && !col_name.StartsWith("Deselected"))
                        {
                            if (descriptors_dict.Keys.Contains(col_name))
                            {
                                if (row.Cells[col_name].Value.ToString() != "Inactive" && row.Cells[col_name].Value.ToString() != "Not Fitted")
                                {
                                    descriptors_dict[col_name].Add(double.Parse(row.Cells[col_name].Value.ToString()));
                                }
                            }
                            else
                            {
                                List<double> my_list = new List<double>();
                                if (row.Cells[col_name].Value.ToString() != "Inactive" && row.Cells[col_name].Value.ToString() != "Not Fitted")
                                {
                                    my_list.Add(double.Parse(row.Cells[col_name].Value.ToString()));
                                }
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
                if (BATCH_IDx == 0)
                {
                    f12.dataGridView1.Columns.Add(new DataGridViewTextBoxColumn());
                    f12.dataGridView1.Columns[0].Name = "CPD/Plate";

                    for (int i = 1; i < cols + 1; i++)
                    {
                        DataGridViewImageColumn img = new DataGridViewImageColumn();
                        f12.dataGridView1.Columns.Insert(i, img);
                    }

                    f12.dataGridView1.RowCount = cpd_nb * rows;
                }
            }
            else
            {
                if (f12.dataGridView1.ColumnCount == 0)
                {
                    f12.dataGridView1.Columns.Add(new DataGridViewTextBoxColumn());
                    DataGridViewImageColumn img = new DataGridViewImageColumn();
                    f12.dataGridView1.Columns.Insert(1, img);
                    f12.dataGridView1.Columns.Add(new DataGridViewTextBoxColumn());

                    f12.dataGridView1.Columns[0].Name = "BATCH_ID";
                    f12.dataGridView1.Columns[1].Name = "Image";
                    f12.dataGridView1.Columns[2].Name = "Concentration";

                    DataGridViewTextBoxColumn new_col_plate = new DataGridViewTextBoxColumn();
                    new_col_plate.Name = "Plate";
                    f12.dataGridView1.Columns.Add(new_col_plate);

                    DataGridViewTextBoxColumn new_col_well = new DataGridViewTextBoxColumn();
                    new_col_well.Name = "Well";
                    f12.dataGridView1.Columns.Add(new_col_well);

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

            f12.toolStripProgressBar1.Visible = true;

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
                    if (method_norm == "Raw")
                    {



                        //double minval, maxval;
                        //int[] minIdx = new int[1];
                        //int[] maxidx = new int[1];
                        //Emgu.CV.Structure.MCvScalar mean = new MCvScalar(0);
                        //Emgu.CV.Structure.MCvScalar std = new MCvScalar(0);
                        //Image<Gray, Byte> mask = temp.ToImage<Gray, Byte>().ThresholdBinary(new Gray(180),new Gray(1));

                        CvInvoke.Normalize(temp, mat_8u, 0, 255, Emgu.CV.CvEnum.NormType.MinMax);
                        //CvInvoke.MinMaxIdx(temp, out minval, out maxval, minIdx, maxidx);
                        //Image<Gray, float> tempbis = temp.ToImage<Gray, float>() * (1.0 / (mean.V0+3*std.V0));
                        //tempbis.Mat.ConvertTo(mat_8u, Emgu.CV.CvEnum.DepthType.Cv8U, 255.0);
                    }

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

                    if (method_norm == "Raw")
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

                if (color_format == "SMARCA2")
                {
                    Emgu.CV.Util.VectorOfMat channels_bgr = new Emgu.CV.Util.VectorOfMat();
                    channels_bgr.Push(channels[1].Clone());
                    channels_bgr.Push(channels[0].Clone());
                    channels_bgr.Push(channels[2].Clone());

                    channels.Clear();
                    channels = channels_bgr;
                }

                //CvInvoke.CvtColor(channels[0], channels[0], Emgu.CV.CvEnum.ColorConversion.Bgr2Hsv);
                //CvInvoke.CvtColor(channels[1], channels[1], Emgu.CV.CvEnum.ColorConversion.Bgr2Hsv);
                //CvInvoke.CvtColor(channels[2], channels[2], Emgu.CV.CvEnum.ColorConversion.Bgr2Hsv);

                List<byte> rgb_ch0 = f13.get_rgb_ch0();
                List<byte> rgb_ch1 = f13.get_rgb_ch1();
                List<byte> rgb_ch2 = f13.get_rgb_ch2();
                List<byte> rgb_ch3 = f13.get_rgb_ch3();

                byte color_r_ch0 = rgb_ch0[0];
                byte color_g_ch0 = rgb_ch0[1];
                byte color_b_ch0 = rgb_ch0[2];

                byte color_r_ch1 = rgb_ch1[0];
                byte color_g_ch1 = rgb_ch1[1];
                byte color_b_ch1 = rgb_ch1[2];

                byte color_r_ch2 = rgb_ch2[0];
                byte color_g_ch2 = rgb_ch2[1];
                byte color_b_ch2 = rgb_ch2[2];

                byte color_r_ch3 = rgb_ch3[0];
                byte color_g_ch3 = rgb_ch3[1];
                byte color_b_ch3 = rgb_ch3[2];

                //if (color_format == "SMARCA2")
                //{
                //    color_r_ch0 = 0;
                //    color_g_ch0 = 51;
                //    color_b_ch0 = 255;

                //    color_r_ch1 = 0;
                //    color_g_ch1 = 255;
                //    color_b_ch1 = 157;

                //    color_r_ch2 = 255;
                //    color_g_ch2 = 25;
                //    color_b_ch2 = 0;
                //}

                Emgu.CV.Util.VectorOfMat channels_mixed = new Emgu.CV.Util.VectorOfMat();

                for (int j = 0; j < channels.Size; ++j)
                {
                    channels_mixed.Push(channels[0].Clone());
                }

                unsafe
                {
                    byte* ch0_gray = null;
                    byte* ch1_gray = null;
                    byte* ch2_gray = null;
                    byte* ch3_gray = null;

                    byte* ch_b = (byte*)channels_mixed[0].DataPointer;
                    byte* ch_g = (byte*)channels_mixed[1].DataPointer;
                    byte* ch_r = (byte*)channels_mixed[2].DataPointer;

                    if (channels.Size >= 1)
                    {
                        ch0_gray = (byte*)channels[0].DataPointer;
                    }

                    if (channels.Size >= 2)
                    {
                        ch1_gray = (byte*)channels[1].DataPointer;
                    }

                    if (channels.Size >= 3)
                    {
                        ch2_gray = (byte*)channels[2].DataPointer;
                    }

                    if (channels.Size >= 4)
                    {
                        ch3_gray = (byte*)channels[3].DataPointer;
                    }

                    int value_r = 0;
                    int value_g = 0;
                    int value_b = 0;

                    for (int idx = 0; idx < channels[0].Cols * channels[0].Rows; idx++)
                    {

                        if (channels.Size == 1)
                        {
                            value_b = (byte)(color_b_ch0 * ch0_gray[idx] / 255.0);
                            value_g = (byte)(color_g_ch0 * ch0_gray[idx] / 255.0);
                            value_r = (byte)(color_r_ch0 * ch0_gray[idx] / 255.0);
                        }

                        if (channels.Size == 2)
                        {
                            value_b = (byte)(color_b_ch0 * ch0_gray[idx] / 255.0) + (byte)(color_b_ch1 * ch1_gray[idx] / 255.0);
                            value_g = (byte)(color_g_ch0 * ch0_gray[idx] / 255.0) + (byte)(color_g_ch1 * ch1_gray[idx] / 255.0);
                            value_r = (byte)(color_r_ch0 * ch0_gray[idx] / 255.0) + (byte)(color_r_ch1 * ch1_gray[idx] / 255.0);
                        }

                        if (channels.Size == 3)
                        {
                            value_b = (byte)(color_b_ch0 * ch0_gray[idx] / 255.0) + (byte)(color_b_ch1 * ch1_gray[idx] / 255.0) + (byte)(color_b_ch2 * ch2_gray[idx] / 255.0);
                            value_g = (byte)(color_g_ch0 * ch0_gray[idx] / 255.0) + (byte)(color_g_ch1 * ch1_gray[idx] / 255.0) + (byte)(color_g_ch2 * ch2_gray[idx] / 255.0);
                            value_r = (byte)(color_r_ch0 * ch0_gray[idx] / 255.0) + (byte)(color_r_ch1 * ch1_gray[idx] / 255.0) + (byte)(color_r_ch2 * ch2_gray[idx] / 255.0);
                        }

                        if (channels.Size == 4)
                        {
                            value_b = (byte)(color_b_ch0 * ch0_gray[idx] / 255.0) + (byte)(color_b_ch1 * ch1_gray[idx] / 255.0) + (byte)(color_b_ch2 * ch2_gray[idx] / 255.0) + (byte)(color_b_ch3 * ch3_gray[idx] / 255.0);
                            value_g = (byte)(color_g_ch0 * ch0_gray[idx] / 255.0) + (byte)(color_g_ch1 * ch1_gray[idx] / 255.0) + (byte)(color_g_ch2 * ch2_gray[idx] / 255.0) + (byte)(color_g_ch3 * ch3_gray[idx] / 255.0);
                            value_r = (byte)(color_r_ch0 * ch0_gray[idx] / 255.0) + (byte)(color_r_ch1 * ch1_gray[idx] / 255.0) + (byte)(color_r_ch2 * ch2_gray[idx] / 255.0) + (byte)(color_r_ch3 * ch3_gray[idx] / 255.0);
                        }

                        if (value_b <= 255) ch_b[idx] = (byte)value_b;
                        else ch_b[idx] = 255;

                        if (value_g <= 255) ch_g[idx] = (byte)value_g;
                        else ch_g[idx] = 255;

                        if (value_r <= 255) ch_r[idx] = (byte)value_r;
                        else ch_r[idx] = 255;
                    }
                }

                Mat mat = new Mat();
                CvInvoke.Merge(channels_mixed, mat);

                channels_mixed.Clear();
                channels.Clear();

                Bitmap my_bitmap = null;

                if (color_format == "Rgb")
                    my_bitmap = (mat.ToImage<Emgu.CV.Structure.Rgb, Byte>()).ToBitmap();

                if (color_format == "Bgr" || color_format == "EMT" || color_format == "SMARCA2")
                    my_bitmap = (mat.ToImage<Emgu.CV.Structure.Bgr, Byte>()).ToBitmap();

                int replicate = (int)f13.numericUpDown6.Value;

                if (view_images_per_concentration == true)
                {
                    f12.dataGridView1.Rows[rows * BATCH_IDx + (counter - 1) % total_plate_nb].Cells[0].Style.WrapMode = DataGridViewTriState.True;
                    f12.dataGridView1.Rows[rows * BATCH_IDx + (counter - 1) % total_plate_nb].Cells[0].Value = BATCH_ID + "\r\n" + "\r\n" + plates[i];
                    f12.dataGridView1.Rows[rows * BATCH_IDx + (counter - 1) % total_plate_nb].Cells[0].Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

                    f12.dataGridView1.Rows[rows * BATCH_IDx + (counter - 1) % total_plate_nb].Cells[(counter - 1) / total_plate_nb + 1].Value = (Image)my_bitmap;

                    if (replicate != 1) f12.dataGridView1.Columns[(counter - 1) / total_plate_nb + 1].Name = concentrations[((counter - 1) / total_plate_nb) * replicate].ToString();
                    else f12.dataGridView1.Columns[(counter - 1) / total_plate_nb + 1].Name = concentrations[((counter - 1)) * replicate].ToString();
                }
                else
                {
                    int index = f12.dataGridView1.Rows.Add(new DataGridViewRow());

                    f12.dataGridView1.Rows[index].Cells[0].Value = BATCH_ID;
                    f12.dataGridView1.Rows[index].Cells[0].Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

                    f12.dataGridView1.Rows[index].Cells[1].Value = (Image)my_bitmap;
                    f12.dataGridView1.Rows[index].Cells[2].Value = concentrations[i];
                    f12.dataGridView1.Rows[index].Cells[2].Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

                    f12.dataGridView1.Rows[index].Cells[3].Value = plates[i];
                    f12.dataGridView1.Rows[index].Cells[3].Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

                    f12.dataGridView1.Rows[index].Cells[4].Value = wells[i];
                    f12.dataGridView1.Rows[index].Cells[4].Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

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

            f12.toolStripProgressBar1.Visible = false;

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
        public void loadHitsToolStripMenuItem_Click(object sender, EventArgs e)
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

                List<string> BATCH_ID = new List<string>();
                deslected_data_descriptor = new List<string>();

                if (f3.dataGridView1.ColumnCount < 4 || !f3.dataGridView1.Columns.Contains("BATCH_ID")
                    || !f3.dataGridView1.Columns.Contains("Plate") || !f3.dataGridView1.Columns.Contains("Well")
                    || !f3.dataGridView1.Columns.Contains("Concentration"))
                {
                    System.Windows.Forms.MessageBox.Show("The file must contain at least these 4 columns : \n [Plate, Well, BATCH_ID, Concentration]", "Error",
                        System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    return;
                }

                foreach (DataGridViewRow row in f3.dataGridView1.Rows)
                {
                    BATCH_ID.Add(row.Cells["BATCH_ID"].Value.ToString());
                }

                var unique_items = new HashSet<string>(BATCH_ID);
                list_cpd = unique_items.ToList<string>();

                view_images_per_concentration = false;
                check_images();
            }

        }

        public void dRCTimeLineToolStripMenuItem_Click(object sender, EventArgs e)
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
                    string cpd = row["BATCH_ID"].ToString();
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
                    dict_file_color.Add(file, curve_color[dict_file_color.Count]);

                    foreach (DataRow row in table.Rows)
                    {
                        string cpd = row["BATCH_ID"].ToString();

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
                //    Console.WriteLine("BATCH_ID : " + elem.Key);
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
                TimeLine.dataGridView1.Columns[0].Name = "BATCH_ID";
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

        public void get_compound_data(string BATCH_ID)
        {
            tableLayoutPanel1.Controls.Clear();

            HashSet<string> file_list = cpd_link[BATCH_ID];

            Console.WriteLine(BATCH_ID);

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
                    if (the_descriptor != "BATCH_ID" && the_descriptor != "Plate" && the_descriptor != "Well" && the_descriptor != "Concentration"
                           && the_descriptor != "Class" && the_descriptor != "CPD_ID" && !the_descriptor.StartsWith("Status") && !the_descriptor.StartsWith("Bound")
                           && !the_descriptor.StartsWith("Fixed_Top") && !the_descriptor.StartsWith("Data_Modified") && !the_descriptor.StartsWith("Deselected"))
                    {
                        //    if (the_descriptor != "Plate" && the_descriptor != "Well" && the_descriptor != "BATCH_ID" && the_descriptor != "Class" && the_descriptor != "Concentration")
                        //{
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

        public void draw_cpd_list(string current_file, string BATCH_ID, bool checked_state)
        {
            Dictionary<string, List<double>> descriptor_data = new Dictionary<string, List<double>>();
            Dictionary<string, List<double>> descriptor_concentrations = new Dictionary<string, List<double>>();

            DataTable my_table = data_dict[current_file]; // file --> DataTable

            foreach (DataRow row in my_table.Rows)
            {
                if (row["BATCH_ID"].ToString() == BATCH_ID)
                {
                    foreach (string descriptor in time_line_selected_descriptors)
                    {
                        double val;
                        bool test_double = Double.TryParse(row[descriptor].ToString(), out val);
                        if (test_double == false) continue;

                        bool select_point = true;
                        if (my_table.Columns.Contains("Deselected_" + descriptor))
                        {
                            string value_str = row["Deselected_" + descriptor].ToString();
                            if (value_str == "TRUE" || value_str == "True" | value_str == "true") select_point = false;
                            else if (value_str == "FALSE" || value_str == "False" || value_str == "false") select_point = true;
                        }

                        if (descriptor_data.ContainsKey(descriptor))
                        {

                            if (select_point) descriptor_data[descriptor].Add(val);
                        }
                        else
                        {
                            if (select_point)
                            {
                                List<double> descriptor_values = new List<double>();
                                descriptor_values.Add(val);
                                descriptor_data[descriptor] = descriptor_values;
                            }
                        }
                        bool test = false;

                        double current_concentration = try_parse_string_in_double(row["Concentration"].ToString(), my_table.Rows.IndexOf(row), "Concentration", ref test);

                        if (test) return;

                        if (descriptor_concentrations.ContainsKey(descriptor))
                        {

                            if (select_point) descriptor_concentrations[descriptor].Add(current_concentration);
                        }
                        else
                        {
                            if (select_point)
                            {
                                List<double> descriptor_values = new List<double>();
                                descriptor_values.Add(current_concentration);
                                descriptor_concentrations[descriptor] = descriptor_values;
                            }
                        }

                    }


                }
            }

            //charts_time_line = new Dictionary<string, Dictionary<string, Chart_DRC_Time_Line>>>(); // BATCH_ID, descriptor, chart

            if (!charts_time_line.ContainsKey(BATCH_ID))
            {

                Dictionary<string, Chart_DRC_Time_Line> list_chart_descriptors = new Dictionary<string, Chart_DRC_Time_Line>();

                foreach (KeyValuePair<string, List<double>> elem in descriptor_data)
                {
                    string descriptor = elem.Key;
                    Console.WriteLine(descriptor_concentrations[descriptor].Count());

                    List<double> y = elem.Value;
                    List<double> concentrations = new List<double>();
                    List<double> x_log = new List<double>();

                    foreach (double val in descriptor_concentrations[descriptor])
                    {
                        x_log.Add(Math.Log10(val));
                        concentrations.Add(val);
                    }

                    Chart_DRC_Time_Line current_chart = new Chart_DRC_Time_Line(BATCH_ID, descriptor, 250, ref concentrations,
                        ref x_log, ref y, dict_file_color[current_file], this, current_file);
                    current_chart.draw_DRC();

                    list_chart_descriptors.Add(descriptor, current_chart);
                }

                charts_time_line[BATCH_ID] = list_chart_descriptors;
            }
            else
            {
                Dictionary<string, Chart_DRC_Time_Line> list_chart_descriptors = charts_time_line[BATCH_ID];

                foreach (KeyValuePair<string, Chart_DRC_Time_Line> elem in list_chart_descriptors)
                {
                    string descriptor = elem.Key;
                    List<string> file_names = elem.Value.get_filenames();

                    if (!file_names.Contains(current_file))
                    {
                        if (elem.Value.is_first_curve_drawn() == false)
                        {
                            List<double> y = descriptor_data[descriptor];
                            List<double> concentrations = new List<double>();
                            List<double> x_log = new List<double>();

                            foreach (double val in descriptor_concentrations[descriptor])
                            {
                                x_log.Add(Math.Log10(val));
                                concentrations.Add(val);
                            }
                            Chart_DRC_Time_Line current_chart = new Chart_DRC_Time_Line(BATCH_ID, descriptor, 100, ref concentrations, ref x_log, ref y, dict_file_color[current_file], this, current_file);
                            current_chart.draw_DRC();

                            charts_time_line[BATCH_ID][descriptor] = current_chart;
                        }
                        else
                        {

                            List<double> y = descriptor_data[descriptor];
                            List<double> concentrations = new List<double>();
                            List<double> x_log = new List<double>();

                            foreach (double val in descriptor_concentrations[descriptor])
                            {
                                x_log.Add(Math.Log10(val));
                                concentrations.Add(val);
                            }
                            elem.Value.add_serie_points(current_file, ref concentrations, ref x_log, ref y, dict_file_color[current_file]);
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

                string BATCH_ID = list_cpd[idx].ToString();

                if (BATCH_ID == "DMSO" || BATCH_ID == "Untreated")
                    continue;

                List<Chart_DRC> list_chart = descriptors_chart[BATCH_ID];

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

                string BATCH_ID = list_cpd[idx].ToString();

                if (BATCH_ID == "DMSO" || BATCH_ID == "Untreated")
                    continue;

                List<Chart_DRC> list_chart = descriptors_chart[BATCH_ID];

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

            label3.Visible = true;
            numericUpDown3.Visible = true;
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btn_descriptors_options_Click(object sender, EventArgs e)
        {
            Form fc = Application.OpenForms["Descriptors_General_Options"];

            if (fc == null)
            {
                descriptors_general_options_form = new Descriptors_General_Options(this);

                if (descriptors_chart.Count() < 1) return;

                Label label_bnd_min_x = new Label();
                label_bnd_min_x.Location = new Point(100, 20);
                label_bnd_min_x.Text = "Bound Min X";
                label_bnd_min_x.Name = "lbl_bnd_min_x";
                label_bnd_min_x.AutoSize = true;

                Label label_bnd_max_x = new Label();
                label_bnd_max_x.Location = new Point(250, 20);
                label_bnd_max_x.Text = "Bound Max X";
                label_bnd_max_x.Name = "lbl_bnd_max_x";

                label_bnd_max_x.AutoSize = true;

                Label label_bnd_min_y = new Label();
                label_bnd_min_y.Location = new Point(400, 20);
                label_bnd_min_y.Text = "Bound Min Y";
                label_bnd_min_y.Name = "lbl_bnd_min_y";
                label_bnd_min_y.AutoSize = true;

                Label label_bnd_max_y = new Label();
                label_bnd_max_y.Location = new Point(550, 20);
                label_bnd_max_y.Text = "Bound Max Y";
                label_bnd_min_x.Name = "lbl_bnd_max_y";
                label_bnd_max_y.AutoSize = true;

                //descriptors_general_options_form.Controls.Add(label_bnd_min_x);
                //descriptors_general_options_form.Controls.Add(label_bnd_max_x);
                //descriptors_general_options_form.Controls.Add(label_bnd_min_y);
                //descriptors_general_options_form.Controls.Add(label_bnd_max_y);

                descriptors_general_options_form.panel1.Controls.Add(label_bnd_min_x);
                descriptors_general_options_form.panel1.Controls.Add(label_bnd_max_x);
                descriptors_general_options_form.panel1.Controls.Add(label_bnd_min_y);
                descriptors_general_options_form.panel1.Controls.Add(label_bnd_max_y);

                int counter = 0;


                Dictionary<string, List<double>> dict_descriptor_min_bnd_x = new Dictionary<string, List<double>>();
                Dictionary<string, List<double>> dict_descriptor_max_bnd_x = new Dictionary<string, List<double>>();
                Dictionary<string, List<double>> dict_descriptor_min_bnd_y = new Dictionary<string, List<double>>();
                Dictionary<string, List<double>> dict_descriptor_max_bnd_y = new Dictionary<string, List<double>>();

                Dictionary<string, List<double>> dict_descriptor_min_window_x = new Dictionary<string, List<double>>();
                Dictionary<string, List<double>> dict_descriptor_max_window_x = new Dictionary<string, List<double>>();
                Dictionary<string, List<double>> dict_descriptor_min_window_y = new Dictionary<string, List<double>>();
                Dictionary<string, List<double>> dict_descriptor_max_window_y = new Dictionary<string, List<double>>();

                foreach (KeyValuePair<string, List<Chart_DRC>> elem in descriptors_chart)
                {
                    List<Chart_DRC> current_cpd_charts = elem.Value;

                    foreach (Chart_DRC current_chart in current_cpd_charts)
                    {
                        string descriptor_name = current_chart.get_Descriptor_Name();

                        // Min bound x :
                        if (dict_descriptor_min_bnd_x.ContainsKey(descriptor_name))
                        {
                            dict_descriptor_min_bnd_x[descriptor_name].Add(current_chart.get_min_bound_x());
                        }
                        else
                        {
                            List<double> list_min_x = new List<double>();
                            list_min_x.Add(current_chart.get_min_bound_x());
                            dict_descriptor_min_bnd_x[descriptor_name] = list_min_x;
                        }

                        // Max bound x :
                        if (dict_descriptor_max_bnd_x.ContainsKey(descriptor_name))
                        {
                            dict_descriptor_max_bnd_x[descriptor_name].Add(current_chart.get_max_bound_x());
                        }
                        else
                        {
                            List<double> list_max_x = new List<double>();
                            list_max_x.Add(current_chart.get_max_bound_x());
                            dict_descriptor_max_bnd_x[descriptor_name] = list_max_x;
                        }

                        // Min bound y :
                        if (dict_descriptor_min_bnd_y.ContainsKey(descriptor_name))
                        {
                            dict_descriptor_min_bnd_y[descriptor_name].Add(current_chart.get_min_bound_y());
                        }
                        else
                        {
                            List<double> list_min_y = new List<double>();
                            list_min_y.Add(current_chart.get_min_bound_y());
                            dict_descriptor_min_bnd_y[descriptor_name] = list_min_y;
                        }

                        // Max bound y :
                        if (dict_descriptor_max_bnd_y.ContainsKey(descriptor_name))
                        {
                            dict_descriptor_max_bnd_y[descriptor_name].Add(current_chart.get_max_bound_y());
                        }
                        else
                        {
                            List<double> list_max_y = new List<double>();
                            list_max_y.Add(current_chart.get_max_bound_y());
                            dict_descriptor_max_bnd_y[descriptor_name] = list_max_y;
                        }

                        // Min window x :
                        if (dict_descriptor_min_window_x.ContainsKey(descriptor_name))
                        {
                            dict_descriptor_min_window_x[descriptor_name].Add(current_chart.get_window_x_min());
                        }
                        else
                        {
                            List<double> list_min_x = new List<double>();
                            list_min_x.Add(current_chart.get_window_x_min());
                            dict_descriptor_min_window_x[descriptor_name] = list_min_x;
                        }

                        // Max window x :
                        if (dict_descriptor_max_window_x.ContainsKey(descriptor_name))
                        {
                            dict_descriptor_max_window_x[descriptor_name].Add(current_chart.get_window_x_max());
                        }
                        else
                        {
                            List<double> list_max_x = new List<double>();
                            list_max_x.Add(current_chart.get_window_x_max());
                            dict_descriptor_max_window_x[descriptor_name] = list_max_x;
                        }

                        // Min window y :
                        if (dict_descriptor_min_window_y.ContainsKey(descriptor_name))
                        {
                            dict_descriptor_min_window_y[descriptor_name].Add(current_chart.get_window_y_min());
                        }
                        else
                        {
                            List<double> list_min_y = new List<double>();
                            list_min_y.Add(current_chart.get_window_y_min());
                            dict_descriptor_min_window_y[descriptor_name] = list_min_y;
                        }

                        // Max window y :
                        if (dict_descriptor_max_window_y.ContainsKey(descriptor_name))
                        {
                            dict_descriptor_max_window_y[descriptor_name].Add(current_chart.get_window_y_max());
                        }
                        else
                        {
                            List<double> list_max_y = new List<double>();
                            list_max_y.Add(current_chart.get_window_y_max());
                            dict_descriptor_max_window_y[descriptor_name] = list_max_y;
                        }
                    }
                }

                List<Chart_DRC> list_chart = descriptors_chart[descriptors_chart.First().Key];

                foreach (Chart_DRC current_chart in list_chart)
                {
                    string descritpor_name = current_chart.get_Descriptor_Name();

                    Label new_label = new Label();
                    new_label.Location = new Point(10, 20 + (counter + 1) * 25);
                    new_label.Text = descritpor_name;
                    new_label.Name = "lbl_descriptor_" + descritpor_name;
                    new_label.AutoSize = true;

                    //descriptors_general_options_form.Controls.Add(new_label);
                    descriptors_general_options_form.panel1.Controls.Add(new_label);

                    TextBox text_box_bnd_min_x = new TextBox();
                    text_box_bnd_min_x.Location = new Point(90, 15 + (counter + 1) * 25);
                    text_box_bnd_min_x.Name = "txt_box_bnd_min_x_descriptor_" + descritpor_name;
                    text_box_bnd_min_x.Text = Math.Pow(10, dict_descriptor_min_bnd_x[descritpor_name].Min()).ToString();

                    //descriptors_general_options_form.Controls.Add(text_box_bnd_min_x);
                    descriptors_general_options_form.panel1.Controls.Add(text_box_bnd_min_x);

                    TextBox text_box_bnd_max_x = new TextBox();
                    text_box_bnd_max_x.Location = new Point(240, 15 + (counter + 1) * 25);
                    text_box_bnd_max_x.Name = "txt_box_bnd_max_x_descriptor_" + descritpor_name;
                    text_box_bnd_max_x.Text = Math.Pow(10, dict_descriptor_max_bnd_x[descritpor_name].Max()).ToString();

                    //descriptors_general_options_form.Controls.Add(text_box_bnd_max_x);
                    descriptors_general_options_form.panel1.Controls.Add(text_box_bnd_max_x);

                    TextBox text_box_bnd_min_y = new TextBox();
                    text_box_bnd_min_y.Location = new Point(390, 15 + (counter + 1) * 25);
                    text_box_bnd_min_y.Name = "txt_box_bnd_min_y_descriptor_" + descritpor_name;
                    text_box_bnd_min_y.Text = dict_descriptor_min_bnd_y[descritpor_name].Min().ToString();

                    //descriptors_general_options_form.Controls.Add(text_box_bnd_min_y);
                    descriptors_general_options_form.panel1.Controls.Add(text_box_bnd_min_y);

                    TextBox text_box_bnd_max_y = new TextBox();
                    text_box_bnd_max_y.Location = new Point(540, 15 + (counter + 1) * 25);
                    text_box_bnd_max_y.Name = "txt_box_bnd_max_y_descriptor_" + descritpor_name;
                    text_box_bnd_max_y.Text = dict_descriptor_max_bnd_y[descritpor_name].Max().ToString();

                    //descriptors_general_options_form.Controls.Add(text_box_bnd_max_y);
                    descriptors_general_options_form.panel1.Controls.Add(text_box_bnd_max_y);

                    counter++;
                }

                Label label_window_min_x = new Label();
                label_window_min_x.Location = new Point(100, 20);
                label_window_min_x.Text = "Window Min X";
                label_window_min_x.Name = "lbl_window_min_x";
                label_window_min_x.AutoSize = true;

                Label label_window_max_x = new Label();
                label_window_max_x.Location = new Point(250, 20);
                label_window_max_x.Text = "Window Max X";
                label_window_max_x.Name = "lbl_window_max_x";
                label_window_max_x.AutoSize = true;

                Label label_window_min_y = new Label();
                label_window_min_y.Location = new Point(400, 20);
                label_window_min_y.Text = "Window Min Y";
                label_window_min_y.Name = "lbl_window_min_y";
                label_window_min_y.AutoSize = true;

                Label label_window_max_y = new Label();
                label_window_max_y.Location = new Point(550, 20);
                label_window_max_y.Text = "Window Max Y";
                label_window_max_y.Name = "lbl_window_max_y";

                label_window_max_y.AutoSize = true;

                descriptors_general_options_form.panel2.Controls.Add(label_window_min_x);
                descriptors_general_options_form.panel2.Controls.Add(label_window_max_x);
                descriptors_general_options_form.panel2.Controls.Add(label_window_min_y);
                descriptors_general_options_form.panel2.Controls.Add(label_window_max_y);

                counter = 0;

                foreach (Chart_DRC current_chart in list_chart)
                {
                    string descritpor_name = current_chart.get_Descriptor_Name();

                    Label new_label = new Label();
                    new_label.Location = new Point(10, 20 + (counter + 1) * 25);
                    new_label.Text = descritpor_name;
                    new_label.Name = "lbl_window_descriptor_" + descritpor_name;
                    new_label.AutoSize = true;

                    descriptors_general_options_form.panel2.Controls.Add(new_label);

                    TextBox text_box_window_min_x = new TextBox();
                    text_box_window_min_x.Location = new Point(100, 20 + (counter + 1) * 25);
                    text_box_window_min_x.Name = "txt_box_window_min_x_descriptor_" + descritpor_name;
                    text_box_window_min_x.Text = dict_descriptor_min_window_x[descritpor_name].Min().ToString();

                    descriptors_general_options_form.panel2.Controls.Add(text_box_window_min_x);

                    TextBox text_box_window_max_x = new TextBox();
                    text_box_window_max_x.Location = new Point(250, 20 + (counter + 1) * 25);
                    text_box_window_max_x.Name = "txt_box_window_max_x_descriptor_" + descritpor_name;
                    text_box_window_max_x.Text = dict_descriptor_max_window_x[descritpor_name].Max().ToString();

                    descriptors_general_options_form.panel2.Controls.Add(text_box_window_max_x);

                    TextBox text_box_window_min_y = new TextBox();
                    text_box_window_min_y.Location = new Point(400, 20 + (counter + 1) * 25);
                    text_box_window_min_y.Name = "txt_box_window_min_y_descriptor_" + descritpor_name;
                    text_box_window_min_y.Text = dict_descriptor_min_window_y[descritpor_name].Min().ToString();

                    descriptors_general_options_form.panel2.Controls.Add(text_box_window_min_y);

                    TextBox text_box_window_max_y = new TextBox();
                    text_box_window_max_y.Location = new Point(550, 20 + (counter + 1) * 25);
                    text_box_window_max_y.Name = "txt_box_window_max_y_descriptor_" + descritpor_name;
                    text_box_window_max_y.Text = dict_descriptor_max_window_y[descritpor_name].Max().ToString();

                    descriptors_general_options_form.panel2.Controls.Add(text_box_window_max_y);

                    Button color_button = new Button();
                    color_button.Location = new Point(680, 20 + (counter + 1) * 25);
                    color_button.Name = "button_color_descriptor_" + descritpor_name;
                    color_button.Text = "Color";

                    color_button.Click += new EventHandler(this.btn_clicked);

                    descriptors_general_options_form.panel2.Controls.Add(color_button);

                    counter++;
                }

                descriptors_general_options_form.Visible = true;
            }
        }

        private void btn_clicked(object sender, EventArgs e)
        {
            Button btn = (Button)sender;

            foreach (string descriptor_name in descriptor_list)
            {
                if (btn.Name == "button_color_descriptor_" + descriptor_name)
                {
                    ColorDialog dlg = new ColorDialog();
                    dlg.ShowDialog();

                    Color new_color = dlg.Color;


                    foreach (KeyValuePair<string, List<Chart_DRC>> elem in descriptors_chart)
                    {
                        List<Chart_DRC> current_cpd_charts = elem.Value;

                        foreach (Chart_DRC current_chart in current_cpd_charts)
                        {
                            string current_descriptor = current_chart.get_Descriptor_Name();
                            if (current_descriptor == descriptor_name) current_chart.re_fill_color(new_color);
                        }
                    }
                }
            }

        }

        public void apply_descritpor_general_scale(string descriptor_name, double window_min_x, double window_max_x, double window_min_y, double window_max_y)
        {
            foreach (KeyValuePair<string, List<Chart_DRC>> elem in descriptors_chart)
            {
                string BATCH_ID = elem.Key;
                List<Chart_DRC> cpd_charts = elem.Value;

                foreach (Chart_DRC current_chart in cpd_charts)
                {
                    if (current_chart.get_Descriptor_Name() == descriptor_name)
                    {
                        current_chart.set_general_params(true);
                        current_chart.set_data_modified(true);

                        current_chart.set_window_x_min(window_min_x);
                        current_chart.set_window_x_max(window_max_x);
                        current_chart.set_window_y_min(window_min_y);
                        current_chart.set_window_y_max(window_max_y);

                        current_chart.draw_DRC(false, false);
                    }
                }
            }
        }

        public void apply_descritpor_general_bounds(string descriptor_name, double bnd_min_x, double bnd_max_x, double bnd_min_y, double bnd_max_y)
        {
            foreach (KeyValuePair<string, List<Chart_DRC>> elem in descriptors_chart)
            {
                string BATCH_ID = elem.Key;
                List<Chart_DRC> cpd_charts = elem.Value;

                foreach (Chart_DRC current_chart in cpd_charts)
                {
                    if (current_chart.get_Descriptor_Name() == descriptor_name)
                    {
                        current_chart.set_general_params(true);
                        current_chart.set_data_modified(true);

                        current_chart.set_min_bound_x(Math.Log10(bnd_min_x));
                        current_chart.set_max_bound_x(Math.Log10(bnd_max_x));
                        current_chart.set_min_bound_y(bnd_min_y);
                        current_chart.set_max_bound_y(bnd_max_y);

                        current_chart.draw_DRC(false, false);
                    }
                }
            }
        }


        public void apply_descriptor_fixed_top(string descriptor_name, double fixed_top)
        {
            foreach (KeyValuePair<string, List<Chart_DRC>> elem in descriptors_chart)
            {
                string BATCH_ID = elem.Key;
                List<Chart_DRC> cpd_charts = elem.Value;

                foreach (Chart_DRC current_chart in cpd_charts)
                {
                    if (current_chart.get_Descriptor_Name() == descriptor_name)
                    {
                        //current_chart.set_general_params(true);

                        current_chart.set_top_fixed(true);
                        current_chart.set_top_fixed_value(fixed_top);
                        current_chart.set_data_modified(true);

                        current_chart.draw_DRC(false, false);
                    }
                }
            }
        }

        private void btn_fix_top_bottom_Click(object sender, EventArgs e)
        {
            Form fc = Application.OpenForms["Descriptors_General_Options"];

            if (fc == null)
            {
                descriptors_fix_top_form = new Descriptors_Fix_Top_Options(this);

                Label label_window_min_x = new Label();
                label_window_min_x.Location = new Point(100, 20);
                label_window_min_x.Text = "Top/Bottom";
                label_window_min_x.Name = "lbl_fix_top_bottom";
                label_window_min_x.AutoSize = true;

                descriptors_fix_top_form.Controls.Add(label_window_min_x);

                int counter = 0;

                List<Chart_DRC> list_chart;
                if (descriptors_chart.Count() > 0) list_chart = descriptors_chart[descriptors_chart.First().Key];
                else return;

                foreach (Chart_DRC current_chart in list_chart)
                {
                    string descritpor_name = current_chart.get_Descriptor_Name();

                    Label new_label = new Label();
                    new_label.Location = new Point(10, 20 + (counter + 1) * 25);
                    new_label.Text = descritpor_name;
                    new_label.Name = "lbl_descriptor_" + descritpor_name;
                    new_label.AutoSize = true;

                    descriptors_fix_top_form.Controls.Add(new_label);

                    TextBox text_boxfix_top = new TextBox();
                    text_boxfix_top.Location = new Point(90, 15 + (counter + 1) * 25);
                    text_boxfix_top.Name = "txt_box_fix_top_descriptor_" + descritpor_name;

                    if (current_chart.top_fixed())
                    {
                        text_boxfix_top.Text = current_chart.get_top_fixed().ToString();
                    }
                    else
                    {
                        text_boxfix_top.Text = "";
                    }

                    descriptors_fix_top_form.Controls.Add(text_boxfix_top);

                    Button apply_button = new Button();
                    apply_button.Location = new Point(200, 15 + (counter + 1) * 25);
                    apply_button.Name = "button_apply_descriptor_" + descritpor_name;
                    apply_button.Text = "Apply";

                    apply_button.Click += new EventHandler(this.btn_clicked_fix_top);

                    descriptors_fix_top_form.Controls.Add(apply_button);

                    counter++;
                }

            }

            descriptors_fix_top_form.Visible = true;
        }

        private void btn_clicked_fix_top(object sender, EventArgs e)
        {
            Button btn = (Button)sender;

            foreach (string descriptor_name in descriptor_list)
            {
                if (btn.Name == "button_apply_descriptor_" + descriptor_name)
                {
                    double fixed_top = 0.0;

                    if (descriptors_fix_top_form.Controls.ContainsKey("txt_box_fix_top_descriptor_" + descriptor_name))
                    {
                        TextBox txt_box = descriptors_fix_top_form.Controls["txt_box_fix_top_descriptor_" + descriptor_name] as TextBox;

                        fixed_top = double.Parse(txt_box.Text.ToString());

                        apply_descriptor_fixed_top(descriptor_name, fixed_top);
                    }
                }
            }

        }

        private double Median(List<double> xs)
        {
            var ys = xs.OrderBy(x => x).ToList();
            double mid = (ys.Count - 1) / 2.0;
            return (ys[(int)(mid)] + ys[(int)(mid + 0.5)]) / 2;
        }

        public void loadPSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Patient Stratificaton :
            // BATCH_ID_List Table :

            Reset();
            SetForm();

            //string path_template = "";
            //string path_plate_1_1 = "";
            //string path_plate_1_2 = "";
            //string path_plate_2_1 = "";
            //string path_plate_2_2 = "";

            Form fc = Application.OpenForms["Load_PS_Options"];

            if (fc != null)
            {
                Load_PS.Show();

                //path_template = Load_PS.get_template_path();

                //path_plate_1_1 = Load_PS.get_plate_1_1_path();
                //path_plate_1_2 = Load_PS.get_plate_1_2_path();
                //path_plate_2_1 = Load_PS.get_plate_2_1_path();
                //path_plate_2_2 = Load_PS.get_plate_2_2_path();
            }
            else
            {
                Load_PS = new Load_PS_Options(this);
                Load_PS.Show();

                //path_template = Load_PS.get_template_path();

                //path_plate_1_1 = Load_PS.get_plate_1_1_path();
                //path_plate_1_2 = Load_PS.get_plate_1_2_path();
                //path_plate_2_1 = Load_PS.get_plate_2_1_path();
                //path_plate_2_2 = Load_PS.get_plate_2_2_path();
            }

        }

        public void process_template(string file)
        {
            btn_normalize.Enabled = true;
            is_patient = true;

            List<string> first_wells = new List<string>();
            List<string> drugs = new List<string>();
            List<string> drugs_kegg = new List<string>();
            List<string> targets = new List<string>();
            List<string> drug_plate = new List<string>();

            Dictionary<string, Dictionary<string, List<double>>> dict_concentrations_plate = new Dictionary<string, Dictionary<string, List<double>>>();


            System.IO.StreamReader sr = new System.IO.StreamReader(file);
            CachedCsvReader template_cpds_csv = new CachedCsvReader(sr, true);

            int fieldCount = template_cpds_csv.FieldCount;
            string[] headers = template_cpds_csv.GetFieldHeaders();

            while (template_cpds_csv.ReadNextRecord())
            {

                for (int i = 0; i < fieldCount; ++i)
                {
                    string col_name = headers[i];
                    string value = "";

                    if (col_name == "Plate")
                    {
                        if (template_cpds_csv[i].ToString() != "nan" && template_cpds_csv[i].ToString() != "inf")
                        {
                            value = template_cpds_csv[i].ToString();
                            drug_plate.Add(value);
                        }
                    }

                    if (col_name == "Well")
                    {
                        if (template_cpds_csv[i].ToString() != "nan" && template_cpds_csv[i].ToString() != "inf")
                        {
                            value = template_cpds_csv[i].ToString();

                            if (value.Length == 2)
                            {
                                value = value[0] + "0" + value[1];
                            }
                            first_wells.Add(value);

                            //int[] Pos = new int[2];

                            //Pos[1] = Convert.ToInt16(value[0]) - 64;
                            //Pos[0] = Convert.ToInt16(value.Remove(0, 1));

                        }
                    }

                    if (col_name == "Drug")
                    {
                        if (template_cpds_csv[i].ToString() != "nan" && template_cpds_csv[i].ToString() != "inf")
                        {
                            value = template_cpds_csv[i].ToString();
                            drugs.Add(value);

                        }
                    }

                    if (col_name == "Drug_Kegg")
                    {
                        if (template_cpds_csv[i].ToString() != "nan" && template_cpds_csv[i].ToString() != "inf")
                        {
                            value = template_cpds_csv[i].ToString();
                            drugs_kegg.Add(value);

                        }
                    }

                    if (col_name == "Target")
                    {
                        if (template_cpds_csv[i].ToString() != "nan" && template_cpds_csv[i].ToString() != "inf")
                        {
                            value = template_cpds_csv[i].ToString();
                            targets.Add(value);

                        }
                    }

                    if (col_name.Contains("Concentration_1"))
                    {

                        string plate_number = drug_plate[drug_plate.Count() - 1].ToString();

                        if (!dict_concentrations_plate.ContainsKey(plate_number))
                        {
                            Dictionary<string, List<double>> temp = new Dictionary<string, List<double>>();
                            dict_concentrations_plate[plate_number] = temp;
                        }

                        double conc_1;
                        double.TryParse(template_cpds_csv[i].ToString(), out conc_1);
                        if (dict_concentrations_plate[plate_number].ContainsKey(first_wells[first_wells.Count() - 1]))
                        {
                            dict_concentrations_plate[plate_number][first_wells[first_wells.Count() - 1]].Add(conc_1);
                        }
                        else
                        {
                            List<double> temp_list = new List<double>();
                            dict_concentrations_plate[plate_number][first_wells[first_wells.Count() - 1]] = temp_list;
                            dict_concentrations_plate[plate_number][first_wells[first_wells.Count() - 1]].Add(conc_1);
                        }
                    }

                    if (col_name.Contains("Concentration_2"))
                    {

                        string plate_number = drug_plate[drug_plate.Count() - 1].ToString();

                        if (!dict_concentrations_plate.ContainsKey(plate_number))
                        {
                            Dictionary<string, List<double>> temp = new Dictionary<string, List<double>>();
                            dict_concentrations_plate[plate_number] = temp;
                        }
                        double conc_2;
                        double.TryParse(template_cpds_csv[i].ToString(), out conc_2);
                        if (dict_concentrations_plate[plate_number].ContainsKey(first_wells[first_wells.Count() - 1]))
                        {
                            dict_concentrations_plate[plate_number][first_wells[first_wells.Count() - 1]].Add(conc_2);
                        }
                        else
                        {
                            List<double> temp_list = new List<double>();
                            dict_concentrations_plate[plate_number][first_wells[first_wells.Count() - 1]] = temp_list;
                            dict_concentrations_plate[plate_number][first_wells[first_wells.Count() - 1]].Add(conc_2);
                        }
                    }

                    if (col_name.Contains("Concentration_3"))
                    {

                        string plate_number = drug_plate[drug_plate.Count() - 1].ToString();

                        if (!dict_concentrations_plate.ContainsKey(plate_number))
                        {
                            Dictionary<string, List<double>> temp = new Dictionary<string, List<double>>();
                            dict_concentrations_plate[plate_number] = temp;
                        }
                        double conc_3;
                        double.TryParse(template_cpds_csv[i].ToString(), out conc_3);
                        if (dict_concentrations_plate[plate_number].ContainsKey(first_wells[first_wells.Count() - 1]))
                        {
                            dict_concentrations_plate[plate_number][first_wells[first_wells.Count() - 1]].Add(conc_3);
                        }
                        else
                        {
                            List<double> temp_list = new List<double>();
                            dict_concentrations_plate[plate_number][first_wells[first_wells.Count() - 1]] = temp_list;
                            dict_concentrations_plate[plate_number][first_wells[first_wells.Count() - 1]].Add(conc_3);
                        }
                    }

                    if (col_name.Contains("Concentration_4"))
                    {

                        string plate_number = drug_plate[drug_plate.Count() - 1].ToString();

                        if (!dict_concentrations_plate.ContainsKey(plate_number))
                        {
                            Dictionary<string, List<double>> temp = new Dictionary<string, List<double>>();
                            dict_concentrations_plate[plate_number] = temp;
                        }
                        double conc_4;
                        double.TryParse(template_cpds_csv[i].ToString(), out conc_4);
                        if (dict_concentrations_plate[plate_number].ContainsKey(first_wells[first_wells.Count() - 1]))
                        {
                            dict_concentrations_plate[plate_number][first_wells[first_wells.Count() - 1]].Add(conc_4);
                        }
                        else
                        {
                            List<double> temp_list = new List<double>();
                            dict_concentrations_plate[plate_number][first_wells[first_wells.Count() - 1]] = temp_list;
                            dict_concentrations_plate[plate_number][first_wells[first_wells.Count() - 1]].Add(conc_4);
                        }
                    }

                    if (col_name.Contains("Concentration_5"))
                    {

                        string plate_number = drug_plate[drug_plate.Count() - 1].ToString();

                        if (!dict_concentrations_plate.ContainsKey(plate_number))
                        {
                            Dictionary<string, List<double>> temp = new Dictionary<string, List<double>>();
                            dict_concentrations_plate[plate_number] = temp;
                        }
                        double conc_5;
                        double.TryParse(template_cpds_csv[i].ToString(), out conc_5);
                        if (dict_concentrations_plate[plate_number].ContainsKey(first_wells[first_wells.Count() - 1]))
                        {
                            dict_concentrations_plate[plate_number][first_wells[first_wells.Count() - 1]].Add(conc_5);
                        }
                        else
                        {
                            List<double> temp_list = new List<double>();
                            dict_concentrations_plate[plate_number][first_wells[first_wells.Count() - 1]] = temp_list;
                            dict_concentrations_plate[plate_number][first_wells[first_wells.Count() - 1]].Add(conc_5);
                        }
                    }

                    if (col_name.Contains("Concentration_6"))
                    {

                        string plate_number = drug_plate[drug_plate.Count() - 1].ToString();

                        if (!dict_concentrations_plate.ContainsKey(plate_number))
                        {
                            Dictionary<string, List<double>> temp = new Dictionary<string, List<double>>();
                            dict_concentrations_plate[plate_number] = temp;
                        }
                        double conc_6;
                        double.TryParse(template_cpds_csv[i].ToString(), out conc_6);
                        if (dict_concentrations_plate[plate_number].ContainsKey(first_wells[first_wells.Count() - 1]))
                        {
                            dict_concentrations_plate[plate_number][first_wells[first_wells.Count() - 1]].Add(conc_6);
                        }
                        else
                        {
                            List<double> temp_list = new List<double>();
                            dict_concentrations_plate[plate_number][first_wells[first_wells.Count() - 1]] = temp_list;
                            dict_concentrations_plate[plate_number][first_wells[first_wells.Count() - 1]].Add(conc_6);
                        }
                    }

                    if (col_name.Contains("Concentration_7"))
                    {

                        string plate_number = drug_plate[drug_plate.Count() - 1].ToString();

                        if (!dict_concentrations_plate.ContainsKey(plate_number))
                        {
                            Dictionary<string, List<double>> temp = new Dictionary<string, List<double>>();
                            dict_concentrations_plate[plate_number] = temp;
                        }
                        double conc_7;
                        double.TryParse(template_cpds_csv[i].ToString(), out conc_7);
                        if (dict_concentrations_plate[plate_number].ContainsKey(first_wells[first_wells.Count() - 1]))
                        {
                            dict_concentrations_plate[plate_number][first_wells[first_wells.Count() - 1]].Add(conc_7);
                        }
                        else
                        {
                            List<double> temp_list = new List<double>();
                            dict_concentrations_plate[plate_number][first_wells[first_wells.Count() - 1]] = temp_list;
                            dict_concentrations_plate[plate_number][first_wells[first_wells.Count() - 1]].Add(conc_7);
                        }

                    }

                }

            }

            template_plate_1 = new Dictionary<string, string>();
            template_plate_2 = new Dictionary<string, string>();

            template_plate_concentration = new Dictionary<string, Dictionary<string, double>>();

            //ps_concentrations.Add(30 * 1e-6);
            //ps_concentrations.Add(7.5 * 1e-6);
            //ps_concentrations.Add(1.875 * 1e-6);
            //ps_concentrations.Add(0.46875 * 1e-6);
            //ps_concentrations.Add(0.1171875 * 1e-6);
            //ps_concentrations.Add(0.029296875 * 1e-6);
            //ps_concentrations.Add(0.00732421875 * 1e-6);

            //norm_integral = (Math.Log10(ps_concentrations[0]) - Math.Log10(ps_concentrations[ps_concentrations.Count - 1]));

            List<string> first_letter = new List<string>();
            first_letter.Add("B");
            first_letter.Add("C");
            first_letter.Add("D");
            first_letter.Add("E");
            first_letter.Add("F");
            first_letter.Add("G");
            first_letter.Add("H");

            List<string> second_letter = new List<string>();
            second_letter.Add("I");
            second_letter.Add("J");
            second_letter.Add("K");
            second_letter.Add("L");
            second_letter.Add("M");
            second_letter.Add("N");
            second_letter.Add("O");

            Dictionary<string, string> cpd_position_1 = new Dictionary<string, string>();
            Dictionary<string, string> cpd_position_2 = new Dictionary<string, string>();
            cpd_target = new Dictionary<string, string>();

            for (int i = 0; i < first_wells.Count; ++i)
            {
                int plate = int.Parse(drug_plate[i]);
                string well = first_wells[i];
                string drug = drugs_kegg[i];
                string target = targets[i];

                if (plate == 1)
                {
                    if (drug != "Blank")
                    {
                        cpd_position_1[well] = drug;
                        cpd_target[drug] = target;
                    }
                    else
                    {
                        cpd_position_1[well] = "Untreated";
                    }
                }
                if (plate == 2)
                {
                    if (drug != "Blank")
                    {
                        cpd_position_2[well] = drug;
                        cpd_target[drug] = target;
                    }
                    else
                    {
                        cpd_position_2[well] = "Untreated";
                    }
                }

            }
            /*
            cpd_position_1["B02"] = "DMSO";
            cpd_position_1["B03"] = "Sunitinib Malate(Sutent)";
            cpd_position_1["B04"] = "Vismodegib (GDC-0449)";
            cpd_position_1["B05"] = "5-fu";
            cpd_position_1["B06"] = "Axitinib";
            cpd_position_1["B07"] = "Oxaliplatin";
            cpd_position_1["B08"] = "Dasatinib(BMS-354825)";
            cpd_position_1["B09"] = "Temsirolimus";
            cpd_position_1["B10"] = "Vemurafenib";
            cpd_position_1["B11"] = "Erlotinib HCl";
            cpd_position_1["B12"] = "Vandetanib";
            cpd_position_1["B13"] = "Ruxolitinib";
            cpd_position_1["B14"] = "Imatinib(Gleevec)";
            cpd_position_1["B15"] = "Gefitinib(Iressa)";
            cpd_position_1["B16"] = "ABT-888(Veliparib)";
            cpd_position_1["B17"] = "Dovitinib(TKI-258)";
            cpd_position_1["B18"] = "BGJ398(NVP-BGJ398)";
            cpd_position_1["B19"] = "AZD2014";
            cpd_position_1["B20"] = "LGK-974";
            cpd_position_1["B21"] = "AZD5363";
            cpd_position_1["B22"] = "CI-1033(Canertinib)";
            cpd_position_1["B23"] = "Tandutinib(MLN518)";
            cpd_position_1["I02"] = "Untreated";
            cpd_position_1["I03"] = "Everolimus(RAD001)";
            cpd_position_1["I04"] = "Dabrafenib";
            cpd_position_1["I05"] = "Bortezomib(Velcade)";
            cpd_position_1["I06"] = "Regorafenib";
            cpd_position_1["I07"] = "Lapatinib";
            cpd_position_1["I08"] = "Trametinib";
            cpd_position_1["I09"] = "Bosutinib";
            cpd_position_1["I10"] = "Nilotinib(AMN-107)";
            cpd_position_1["I11"] = "Crizotinib(PF-02341066)";
            cpd_position_1["I12"] = "Irinotecan";
            cpd_position_1["I13"] = "Ibrutinib";
            cpd_position_1["I14"] = "Carfilzomib";
            cpd_position_1["I15"] = "Pazopanib HCl";
            cpd_position_1["I16"] = "Foretinib(XL880)";
            cpd_position_1["I17"] = "AZD8931";
            cpd_position_1["I18"] = "INCB28060";
            cpd_position_1["I19"] = "LY2835219(Abemaciclib)";
            cpd_position_1["I20"] = "ABT-199(GDC-0199)";
            cpd_position_1["I21"] = "AZD6244(Selumetinib)";
            cpd_position_1["I22"] = "PD 0332991(Palbociclib) HCl";
            cpd_position_1["I23"] = "DMSO";
            */
            /*
            cpd_position_1["B02"] = "DMSO";
            cpd_position_1["B03"] = "Sutent";
            cpd_position_1["B04"] = "Vismodegib";
            cpd_position_1["B05"] = "5-fu";
            cpd_position_1["B06"] = "Axitinib";
            cpd_position_1["B07"] = "Oxaliplatin";
            cpd_position_1["B08"] = "Dasatinib";
            cpd_position_1["B09"] = "Temsirolimus";
            cpd_position_1["B10"] = "Vemurafenib";
            cpd_position_1["B11"] = "Erlotinib hydrochlorid";
            cpd_position_1["B12"] = "Vandetanib";
            cpd_position_1["B13"] = "Ruxolitinib";
            cpd_position_1["B14"] = "Imatinib";
            cpd_position_1["B15"] = "Gefitinib";
            cpd_position_1["B16"] = "Veliparib";
            cpd_position_1["B17"] = "Dovitinib";
            cpd_position_1["B18"] = "Infigratinib";
            cpd_position_1["B19"] = "Vistusertib";
            cpd_position_1["B20"] = "LGK-974";
            cpd_position_1["B21"] = "AZD5363";
            cpd_position_1["B22"] = "Canertinib";
            cpd_position_1["B23"] = "Tandutinib";
            cpd_position_1["I02"] = "Untreated";
            cpd_position_1["I03"] = "Everolimus";
            cpd_position_1["I04"] = "Dabrafenib";
            cpd_position_1["I05"] = "Bortezomib";
            cpd_position_1["I06"] = "Regorafenib";
            cpd_position_1["I07"] = "Lapatinib";
            cpd_position_1["I08"] = "Trametinib";
            cpd_position_1["I09"] = "Bosutinib";
            cpd_position_1["I10"] = "Nilotinib";
            cpd_position_1["I11"] = "Crizotinib";
            cpd_position_1["I12"] = "Irinotecan";
            cpd_position_1["I13"] = "Ibrutinib";
            cpd_position_1["I14"] = "Carfilzomib";
            cpd_position_1["I15"] = "Pazopanib hydrochlorid";
            cpd_position_1["I16"] = "Foretinib";
            cpd_position_1["I17"] = "Sapitinib";
            cpd_position_1["I18"] = "Capmatinib";
            cpd_position_1["I19"] = "Abemaciclib";
            cpd_position_1["I20"] = "Venetoclax";
            cpd_position_1["I21"] = "Selumetinib";
            cpd_position_1["I22"] = "Palbociclib hydrochlorid";
            cpd_position_1["I23"] = "DMSO";
            */

            foreach (KeyValuePair<string, string> elem in cpd_position_1)
            {
                string number = elem.Key[1].ToString() + elem.Key[2].ToString();
                string current_letter = elem.Key[0].ToString();

                if (first_letter.Contains(current_letter))
                {
                    foreach (string letter in first_letter)
                    {
                        template_plate_1[letter + number] = elem.Value;

                    }
                }

                if (second_letter.Contains(current_letter))
                {
                    foreach (string letter in second_letter)
                    {
                        template_plate_1[letter + number] = elem.Value;
                    }
                }

            }

            // Get the concentrations :

            List<string> plates = new List<string>();
            plates.Add("1");
            plates.Add("2");

            foreach (string plate_number in plates)
            {
                if (dict_concentrations_plate.ContainsKey(plate_number))
                {
                    foreach (KeyValuePair<string, string> elem in cpd_position_1) // cpd_position1 to get all the key (because first plate is totally filled)
                    {
                        string number = elem.Key[1].ToString() + elem.Key[2].ToString();
                        string current_letter = elem.Key[0].ToString();

                        if (dict_concentrations_plate[plate_number].ContainsKey(first_letter[0] + number))
                        {

                            if (first_letter.Contains(current_letter))
                            {
                                foreach (string letter in first_letter)
                                {

                                    if (!template_plate_concentration.ContainsKey(plate_number))
                                    {
                                        Dictionary<string, double> temp = new Dictionary<string, double>();
                                        template_plate_concentration[plate_number] = temp;
                                    }

                                    switch (letter)
                                    {
                                        case "B":
                                            template_plate_concentration[plate_number][letter + number] = dict_concentrations_plate[plate_number][first_letter[0] + number][0];
                                            break;
                                        case "C":
                                            template_plate_concentration[plate_number][letter + number] = dict_concentrations_plate[plate_number][first_letter[0] + number][1];
                                            break;
                                        case "D":
                                            template_plate_concentration[plate_number][letter + number] = dict_concentrations_plate[plate_number][first_letter[0] + number][2];
                                            break;
                                        case "E":
                                            template_plate_concentration[plate_number][letter + number] = dict_concentrations_plate[plate_number][first_letter[0] + number][3];
                                            break;
                                        case "F":
                                            template_plate_concentration[plate_number][letter + number] = dict_concentrations_plate[plate_number][first_letter[0] + number][4];
                                            break;
                                        case "G":
                                            template_plate_concentration[plate_number][letter + number] = dict_concentrations_plate[plate_number][first_letter[0] + number][5];
                                            break;
                                        case "H":
                                            template_plate_concentration[plate_number][letter + number] = dict_concentrations_plate[plate_number][first_letter[0] + number][6];
                                            break;
                                    }
                                }
                            }
                        }

                        if (dict_concentrations_plate[plate_number].ContainsKey(second_letter[0] + number))
                        {

                            if (second_letter.Contains(current_letter))
                            {

                                if (!template_plate_concentration.ContainsKey(plate_number))
                                {
                                    Dictionary<string, double> temp = new Dictionary<string, double>();
                                    template_plate_concentration[plate_number] = temp;
                                }

                                foreach (string letter in second_letter)
                                {

                                    switch (letter)
                                    {
                                        case "I":
                                            template_plate_concentration[plate_number][letter + number] = dict_concentrations_plate[plate_number][second_letter[0] + number][0];
                                            break;
                                        case "J":
                                            template_plate_concentration[plate_number][letter + number] = dict_concentrations_plate[plate_number][second_letter[0] + number][1];
                                            break;
                                        case "K":
                                            template_plate_concentration[plate_number][letter + number] = dict_concentrations_plate[plate_number][second_letter[0] + number][2];
                                            break;
                                        case "L":
                                            template_plate_concentration[plate_number][letter + number] = dict_concentrations_plate[plate_number][second_letter[0] + number][3];
                                            break;
                                        case "M":
                                            template_plate_concentration[plate_number][letter + number] = dict_concentrations_plate[plate_number][second_letter[0] + number][4];
                                            break;
                                        case "N":
                                            template_plate_concentration[plate_number][letter + number] = dict_concentrations_plate[plate_number][second_letter[0] + number][5];
                                            break;
                                        case "O":
                                            template_plate_concentration[plate_number][letter + number] = dict_concentrations_plate[plate_number][second_letter[0] + number][6];
                                            break;
                                    }
                                }

                            }
                        }
                    }
                }
            }

            /*
            cpd_position_2["B02"] = "DMSO";
            cpd_position_2["B03"] = "BKM120(NVP-BKM120)";
            cpd_position_2["B04"] = "Sotrastaurin(AEB071)";
            cpd_position_2["B05"] = "PF-04449913";
            cpd_position_2["B06"] = "BEZ235";
            cpd_position_2["B07"] = "Cabozantinib(XL184)";
            cpd_position_2["B08"] = "AZD4547";
            cpd_position_2["B09"] = "CO 1686";
            cpd_position_2["B10"] = "Afatinib(BIBW2992)";
            cpd_position_2["B11"] = "BMS-599626(AC480)";
            cpd_position_2["B12"] = "AEE788(NVP-AEE788)";
            cpd_position_2["B13"] = "PF-05212384(PKI-587)";
            cpd_position_2["B14"] = "LEE011(Ribociclib)";
            cpd_position_2["B15"] = "Panobinostat(LBH589)";
            cpd_position_2["B16"] = "Olaparib(AZD2281)";
            cpd_position_2["B17"] = "BYL719";
            cpd_position_2["B18"] = "XL147(pilaralisib)";
            cpd_position_2["B19"] = "Neratinib(HKI-272)";
            cpd_position_2["B20"] = "XL765(SAR245409)";
            cpd_position_2["B21"] = "Cediranib(AZD2171)";
            cpd_position_2["B22"] = "AUY922(NVP-AUY922)";
            cpd_position_2["B23"] = "Tivozanib(AV-951)";
            cpd_position_2["I02"] = "Untreated";
            cpd_position_2["I03"] = "LDE225(NVP-LDE225 Erismodegib)";
            cpd_position_2["I04"] = "Dacomitinib(PF299804 PF-00299804)";
            cpd_position_2["I05"] = "LDK378(Ceritinib)";
            cpd_position_2["I06"] = "RXDX-101";
            cpd_position_2["I23"] = "DMSO";
            */
            /*
            cpd_position_2["B02"] = "DMSO";
            cpd_position_2["B03"] = "Buparlisib";
            cpd_position_2["B04"] = "Sotrastaurin";
            cpd_position_2["B05"] = "Glasdegib";
            cpd_position_2["B06"] = "Dactolisib";
            cpd_position_2["B07"] = "Cabozantinib";
            cpd_position_2["B08"] = "AZD4547";
            cpd_position_2["B09"] = "Rociletinib";
            cpd_position_2["B10"] = "Afatinib";
            cpd_position_2["B11"] = "BMS-599626";
            cpd_position_2["B12"] = "AEE788";
            cpd_position_2["B13"] = "gedatolisib";
            cpd_position_2["B14"] = "Ribociclib";
            cpd_position_2["B15"] = "Panobinostat";
            cpd_position_2["B16"] = "Olaparib";
            cpd_position_2["B17"] = "Alpelisib";
            cpd_position_2["B18"] = "pilaralisib";
            cpd_position_2["B19"] = "Neratinib";
            cpd_position_2["B20"] = "Voxtalisib";
            cpd_position_2["B21"] = "Cediranib";
            cpd_position_2["B22"] = "Luminespib ";
            cpd_position_2["B23"] = "Tivozanib";
            cpd_position_2["I02"] = "Untreated";
            cpd_position_2["I03"] = "Erismodegib";
            cpd_position_2["I04"] = "Dacomitinib";
            cpd_position_2["I05"] = "Ceritinib";
            cpd_position_2["I06"] = "Entrectinib";
            cpd_position_2["I23"] = "DMSO";
            */

            foreach (KeyValuePair<string, string> elem in cpd_position_2)
            {
                string number = elem.Key[1].ToString() + elem.Key[2].ToString();
                string current_letter = elem.Key[0].ToString();

                if (first_letter.Contains(current_letter))
                {
                    foreach (string letter in first_letter)
                    {
                        template_plate_2[letter + number] = elem.Value;
                    }
                }

                if (second_letter.Contains(current_letter))
                {
                    foreach (string letter in second_letter)
                    {
                        template_plate_2[letter + number] = elem.Value;
                    }
                }
            }

        }

        public void process_data_PS(List<string> plate_paths, List<string> plate_names)
        {
            comboBox1.Visible = true;

            for (int k = 0; k < plate_paths.Count; ++k)
            {

                Reset();
                is_with_plate = false;

                string file = plate_paths[k];
                string plate_name = plate_names[k]; // option : rename the plate names (not implemented yet)

                if (k == 0)
                {
                    System.IO.StreamReader sr = new System.IO.StreamReader(file);
                    csv = new CachedCsvReader(sr, true);

                    int fieldCount = csv.FieldCount;

                    string[] headers = csv.GetFieldHeaders();

                    //for(int i=0 ; i<fieldCount ; ++i)
                    //{
                    //    f3.dataGridView1.Columns[i].HeaderText = headers[i];
                    //}

                    while (csv.ReadNextRecord())
                    {
                        DataGridViewRow my_row = new DataGridViewRow();

                        //for (int i = 0; i < fieldCount; i++)
                        //{
                        f3.dataGridView1.ColumnCount = headers.Count();

                        for (int i = 0; i < fieldCount; ++i)
                        {
                            f3.dataGridView1.Columns[i].HeaderText = headers[i];
                            f3.dataGridView1.Columns[i].Name = headers[i];

                            if (csv[i].ToString() != "nan" && csv[i].ToString() != "inf")
                            {
                                my_row.Cells.Add(new DataGridViewTextBoxCell { Value = csv[i] });
                            }
                            else
                            {
                                my_row.Cells.Add(new DataGridViewTextBoxCell { Value = "0.0" });
                            }
                        }

                        //my_row.SetValues(csv[0]);
                        //my_row.Cells[i].Value = csv[i];
                        //}

                        f3.dataGridView1.Rows.Add(my_row);
                    }


                }
                else if (k > 0)
                {
                    System.IO.StreamReader sr_2 = new System.IO.StreamReader(file);
                    CachedCsvReader csv_2 = new CachedCsvReader(sr_2, true);

                    int fieldCount = csv_2.FieldCount;

                    string[] headers = csv_2.GetFieldHeaders();

                    while (csv_2.ReadNextRecord())
                    {
                        DataGridViewRow my_row = new DataGridViewRow(); // (DataGridViewRow)f3.dataGridView1.Rows[0].Clone();
                                                                        //for (int i = 0; i < fieldCount; i++)
                                                                        //{
                                                                        //f3.dataGridView1.ColumnCount = headers.Count();

                        for (int i = 0; i < fieldCount; ++i)
                        {
                            //f3.dataGridView1.Columns[i].HeaderText = headers[i];
                            //my_row.Cells[headers[i]].Value = csv_2[i];
                            my_row.Cells.Add(new DataGridViewTextBoxCell());
                        }

                        f3.dataGridView1.Rows.Add(my_row);

                        for (int i = 0; i < fieldCount; ++i)
                        {
                            //f3.dataGridView1.Columns[i].HeaderText = headers[i];
                            if (csv_2[i].ToString() != "nan" && csv_2[i].ToString() != "inf")
                            {
                                f3.dataGridView1.Rows[f3.dataGridView1.RowCount - 1].Cells[headers[i]].Value = csv_2[i];
                            }
                            else
                            {
                                f3.dataGridView1.Rows[f3.dataGridView1.RowCount - 1].Cells[headers[i]].Value = "0.0";
                            }

                            //my_row.Cells.Add(new DataGridViewTextBoxCell());
                        }


                        //my_row.SetValues(csv[0]);
                        //my_row.Cells[i].Value = csv[i];
                        //}

                    }
                }

            }

            List<string> BATCH_ID = new List<string>();
            deslected_data_descriptor = new List<string>();
            status_ec_50_descritpor = new List<string>();
            bounds_descriptor = new List<string>();
            fixed_top_descriptor = new List<string>();
            data_modified_descriptor = new List<string>();

            if (f3.dataGridView1.ColumnCount < 3 || !f3.dataGridView1.Columns.Contains("Plate") || !f3.dataGridView1.Columns.Contains("Well"))
            {
                System.Windows.Forms.MessageBox.Show("The file must contain at least these 3 columns : \n {[Plate, Well, Descr_0,...}", "Error",
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return;
            }

            //f3.dataGridView1.Sort(f3.dataGridView1.Columns["Well"], System.ComponentModel.ListSortDirection.Ascending);
            f3.dataGridView1.Refresh();
            f3.dataGridView1.Sort(new RowComparer_Plate_Well(SortOrder.Ascending));

            f3.dataGridView1.ColumnCount += 3;
            f3.dataGridView1.Columns[f3.dataGridView1.ColumnCount - 3].HeaderText = "BATCH_ID";
            f3.dataGridView1.Columns[f3.dataGridView1.ColumnCount - 2].HeaderText = "CPD_ID";
            f3.dataGridView1.Columns[f3.dataGridView1.ColumnCount - 1].HeaderText = "Concentration";

            f3.dataGridView1.Columns[f3.dataGridView1.ColumnCount - 3].Name = "BATCH_ID";
            f3.dataGridView1.Columns[f3.dataGridView1.ColumnCount - 2].Name = "CPD_ID";
            f3.dataGridView1.Columns[f3.dataGridView1.ColumnCount - 1].Name = "Concentration";

            List<string> plates = new List<string>();
            foreach (DataGridViewRow row in f3.dataGridView1.Rows)
            {
                plates.Add(row.Cells["Plate"].Value.ToString());
            }

            unique_plates = new HashSet<string>(plates).ToList<string>();

            foreach (DataGridViewRow row in f3.dataGridView1.Rows)
            {
                string well = row.Cells["Well"].Value.ToString();

                if ((row.Cells["Plate"].Value.ToString().Contains("1-1") || row.Cells["Plate"].Value.ToString().Contains("1-2")))
                {
                    if (template_plate_1.ContainsKey(well))
                    {
                        row.Cells["BATCH_ID"].Value = template_plate_1[well];
                        row.Cells["CPD_ID"].Value = template_plate_1[well];
                        row.Cells["Concentration"].Value = template_plate_concentration["1"][well];
                    }
                    else
                    {
                        row.Cells["BATCH_ID"].Value = "Untreated";
                        row.Cells["CPD_ID"].Value = "Untreated";
                        row.Cells["Concentration"].Value = 0;
                    }
                }
                else if ((row.Cells["Plate"].Value.ToString().Contains("2-1") || row.Cells["Plate"].Value.ToString().Contains("2-2")))
                {
                    if (template_plate_1.ContainsKey(well))
                    {
                        row.Cells["BATCH_ID"].Value = template_plate_2[well];
                        row.Cells["CPD_ID"].Value = template_plate_2[well];
                        row.Cells["Concentration"].Value = template_plate_concentration["2"][well];
                    }
                    else
                    {
                        row.Cells["BATCH_ID"].Value = "Untreated";
                        row.Cells["CPD_ID"].Value = "Untreated";
                        row.Cells["Concentration"].Value = 0;
                    }
                }

            }

            foreach (DataGridViewRow row in f3.dataGridView1.Rows)
            {
                /* if (row.Cells["BATCH_ID"].Value.ToString() != "Empty") */
                BATCH_ID.Add(row.Cells["BATCH_ID"].Value.ToString());
            }

            var unique_items = new HashSet<string>(BATCH_ID);
            comboBox1.DataSource = unique_items.ToList<string>();

            foreach (DataGridViewColumn col in f3.dataGridView1.Columns)
            {
                string col_name = col.HeaderText;

                if (col_name != "Plate" && col_name != "Well" && col_name != "Concentration" && col_name != "Run"
                    && col_name != "CPD_ID" && col_name != "Class" && !col_name.StartsWith("Deselected") && col_name != "BATCH_ID"
                    && !col_name.StartsWith("Status") && !col_name.StartsWith("Bound") && !col_name.StartsWith("Fixed_Top")
                    && !col_name.StartsWith("Data_Modified"))
                {
                    checkedListBox1.Items.Add(col_name);
                }

            }

            list_cpd = unique_items.ToList<string>();
            bool fill_concentrations = true;

            //f3.Show();

        }

        private void select_DMSO()
        {
            dmso_charts.Clear();

            if (f3.dataGridView1.RowCount < 1)
            {
                MessageBox.Show("Data is empty.");
                return;
            }

            SetForm();
            //f2.Show();

            f2.Text = this.Text;

            //comboBox1.SelectionChanged += new SelectionChangedEventHandler(comboBox1_SelectionChanged);

            int checked_items = checkedListBox1.CheckedItems.Count;

            descritpor_number = checked_items;

            output_parameter_number = 5;

            f2.dataGridView2.ColumnCount = 2 + output_parameter_number * checked_items;

            f2.dataGridView2.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders;

            f2.dataGridView2.Columns[0].Name = "BATCH_ID";

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

            // Get DMSO values by plate and descriptors :

            Dictionary<string, Dictionary<string, List<double>>> DMSO_by_plate = new Dictionary<string, Dictionary<string, List<double>>>();
            Dictionary<string, Dictionary<string, List<DataGridViewRow>>> raw_data_rows = new Dictionary<string, Dictionary<string, List<DataGridViewRow>>>();

            foreach (DataGridViewRow row in f3.dataGridView1.Rows)
            {
                string current_plate = row.Cells["Plate"].Value.ToString();
                string current_cpd = row.Cells["BATCH_ID"].Value.ToString();

                foreach (string plate in unique_plates)
                {
                    if (plate == current_plate)
                    {
                        if (current_cpd == "DMSO")
                        {
                            foreach (var item in checkedListBox1.Items)
                            {
                                string descriptor_name = item.ToString();

                                if (DMSO_by_plate.ContainsKey(plate))
                                {
                                    if (DMSO_by_plate[plate].ContainsKey(descriptor_name))
                                    {
                                        int row_index = row.Index;
                                        bool test = false;

                                        double cell_val = try_parse_string_in_double(row.Cells[descriptor_name].Value.ToString(), row_index, descriptor_name, ref test);

                                        if (test) return;

                                        DMSO_by_plate[plate][descriptor_name].Add(cell_val);
                                    }
                                    else
                                    {
                                        DMSO_by_plate[plate][descriptor_name] = new List<double>();

                                        int row_index = row.Index;
                                        bool test = false;

                                        double cell_val = try_parse_string_in_double(row.Cells[descriptor_name].Value.ToString(), row_index, descriptor_name, ref test);

                                        if (test) return;

                                        DMSO_by_plate[plate][descriptor_name].Add(cell_val);
                                    }
                                }
                                else
                                {
                                    Dictionary<string, List<double>> temp_dict = new Dictionary<string, List<double>>();
                                    temp_dict[descriptor_name] = new List<double>();

                                    int row_index = row.Index;
                                    bool test = false;
                                    double cell_val = try_parse_string_in_double(row.Cells[descriptor_name].Value.ToString(), row_index, descriptor_name, ref test);

                                    if (test) return;

                                    temp_dict[descriptor_name].Add(cell_val);
                                    DMSO_by_plate[plate] = temp_dict;
                                }

                                // raw data :
                                if (raw_data_rows.ContainsKey(plate))
                                {
                                    if (raw_data_rows[plate].ContainsKey(descriptor_name))
                                    {
                                        raw_data_rows[plate][descriptor_name].Add(row);
                                    }
                                    else
                                    {
                                        raw_data_rows[plate][descriptor_name] = new List<DataGridViewRow>();
                                        raw_data_rows[plate][descriptor_name].Add(row);
                                    }
                                }
                                else
                                {
                                    Dictionary<string, List<DataGridViewRow>> temp_dict = new Dictionary<string, List<DataGridViewRow>>();
                                    temp_dict[descriptor_name] = new List<DataGridViewRow>();
                                    temp_dict[descriptor_name].Add(row);
                                    raw_data_rows[plate] = temp_dict;
                                }

                            }
                        }
                    }
                }
            }

            // Average DMSO by plate and descritpors :

            Dictionary<string, Dictionary<string, double>> DMSO_mean_plate_descriptor = new Dictionary<string, Dictionary<string, double>>();
            //dmso_charts = new Dictionary<string, List<Chart_DRC>>();

            foreach (KeyValuePair<string, Dictionary<string, List<double>>> elem in DMSO_by_plate)
            {
                string plate = elem.Key;
                Dictionary<string, List<double>> descriptors_values = elem.Value;

                List<double> ps_concentrations_bis = new List<double>();
                List<double> ps_concentrations_log = new List<double>();
                List<string> deselected = new List<string>();

                //int replicates = descriptors_values[checkedListBox1.CheckedItems[0].ToString()].Count / 7;

                //for (int i = 0; i < replicates; ++i)
                //{
                //    foreach (double item in ps_concentrations)
                //    {
                //        ps_concentrations_bis.Add(item);
                //        ps_concentrations_log.Add(Math.Log10(item));
                //        deselected.Add("FALSE");
                //    }
                //}

                List<double> row_params = new List<double>();

                int descriptor_index = 0;

                List<Chart_DRC> list_charts_one_dmso = new List<Chart_DRC>();

                foreach (string item in checkedListBox1.CheckedItems)
                //foreach (KeyValuePair<string, List<double>> descriptor_DMSO in descriptors_values)
                {
                    string chart_ec_50_status = "=";
                    string fixed_top = "Not Fixed";
                    Dictionary<string, double> bounds = new Dictionary<string, double>();

                    List<double> dmso_value_per_descriptor = descriptors_values[item];
                    List<string> list_wells = new List<string>();

                    List<DataGridViewRow> raw_data = raw_data_rows[plate][item];

                    ps_concentrations_bis.Clear();
                    ps_concentrations_log.Clear();

                    foreach (DataGridViewRow row in raw_data)
                    {
                        list_wells.Add(row.Cells["Well"].Value.ToString());

                        int row_index = row.Index;
                        bool test = false;

                        double cell_val = try_parse_string_in_double(row.Cells["Concentration"].Value.ToString(), row_index, "Concentration", ref test);

                        if (test) return;

                        ps_concentrations_bis.Add(cell_val);
                        ps_concentrations_log.Add(Math.Log10(cell_val));
                        deselected.Add("FALSE");
                    }

                    // ps_concentrations_log
                    // ps_concentrations
                    if (dmso_value_per_descriptor.Count() == 0) continue;

                    Chart_DRC chart_DMSO_per_plate = new Chart_DRC(plate + " DMSO", item, 50, ref ps_concentrations_bis, ref ps_concentrations_log,
                        ref dmso_value_per_descriptor, Color.Blue, descriptor_index, deselected, chart_ec_50_status, bounds, fixed_top, "FALSE", this,
                        false, false, false, false);

                    chart_DMSO_per_plate.set_dmso_wells(list_wells);
                    chart_DMSO_per_plate.set_Raw_Data(raw_data_rows[plate][item]);

                    double[] parameters = chart_DMSO_per_plate.get_Fit_Parameters();
                    double r2 = chart_DMSO_per_plate.get_R2();


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

                    list_charts_one_dmso.Add(chart_DMSO_per_plate);
                }

                dmso_charts.Add(plate, list_charts_one_dmso);

                DataGridViewRow current_row = (DataGridViewRow)f2.dataGridView2.Rows[0].Clone();

                for (int i = 0; i < row_params.Count() + 1; i++)
                {
                    if (i == 0) current_row.Cells[i].Value = plate + " DMSO";
                    if (i > 0) current_row.Cells[i].Value = row_params[i - 1];
                }

                f2.dataGridView2.Rows.Add(current_row);

                foreach (KeyValuePair<string, List<Chart_DRC>> item in dmso_charts)
                {
                    List<Chart_DRC> list_charts = item.Value;
                    foreach (Chart_DRC chart in list_charts)
                    {
                        chart.draw_DRC(false, true);
                    }
                }

            }
        }

        private void normalize_by_DMSO()
        {

            Dictionary<string, Dictionary<string, double>> DMSO_mean_plate_descriptor = new Dictionary<string, Dictionary<string, double>>();

            foreach (KeyValuePair<string, List<Chart_DRC>> item in dmso_charts)
            {
                List<Chart_DRC> list_charts = item.Value;
                foreach (Chart_DRC chart in list_charts)
                {
                    List<double> y_values_dmso = chart.get_enable_y_values();
                    string plate = chart.get_compound_id().ToString();
                    plate = plate.Substring(0, plate.Length - 5);
                    string descriptor = chart.get_Descriptor_Name();

                    double mean_DMSO_descriptor = y_values_dmso.Average(); // Median(y_values_dmso); //Median;

                    if (DMSO_mean_plate_descriptor.ContainsKey(plate))
                    {
                        DMSO_mean_plate_descriptor[plate][descriptor] = mean_DMSO_descriptor;
                    }
                    else
                    {
                        Dictionary<string, double> temp_dict = new Dictionary<string, double>();
                        temp_dict[descriptor] = mean_DMSO_descriptor;
                        DMSO_mean_plate_descriptor[plate] = temp_dict;
                    }

                }
            }

            foreach (DataGridViewRow row in f3.dataGridView1.Rows)
            {
                string current_plate = row.Cells["Plate"].Value.ToString();
                string current_cpd = row.Cells["BATCH_ID"].Value.ToString();

                foreach (string plate in unique_plates)
                {
                    if (plate == current_plate)
                    {
                        foreach (string cpd in list_cpd)
                        {
                            if (current_cpd == cpd)
                            {
                                foreach (string item in checkedListBox1.CheckedItems)
                                {
                                    int row_index = row.Index;
                                    bool test = false;

                                    double cell_val = try_parse_string_in_double(row.Cells[item].Value.ToString(), row_index, item, ref test);

                                    if (test) return;

                                    double current_value = cell_val;
                                    current_value /= DMSO_mean_plate_descriptor[plate][item];
                                    row.Cells[item].Value = current_value;
                                }
                            }
                        }
                    }
                }
            }


        }

        private static double StandardDeviation(List<double> numberSet, double divisor)
        {
            double mean = numberSet.Average();
            return Math.Sqrt(numberSet.Sum(x => Math.Pow(x - mean, 2)) / divisor);
        }

        public void compute_auc(string graph_type)
        {
            toolStripProgressBar1.Visible = true;

            Dictionary<string, Dictionary<string, double>> auc_dict = new Dictionary<string, Dictionary<string, double>>();
            Dictionary<string, Dictionary<string, double>> auc_error_dict = new Dictionary<string, Dictionary<string, double>>();
            Dictionary<string, Dictionary<string, List<DataGridViewRow>>> raw_data_dict = new Dictionary<string, Dictionary<string, List<DataGridViewRow>>>();

            for (var idx = 0; idx < list_cpd.Count; idx++)
            {
                this.toolStripProgressBar1.Value = idx * 100 / (list_cpd.Count - 1);

                string BATCH_ID = list_cpd[idx].ToString();

                if (BATCH_ID == "DMSO" || BATCH_ID == "Untreated")
                    continue;

                List<Chart_DRC> list_chart = descriptors_chart[BATCH_ID];

                foreach (Chart_DRC current_chart in list_chart)
                {
                    string descriptor = current_chart.get_Descriptor_Name();
                    double AUC = current_chart.compute_AUC();
                    double error_auc = current_chart.get_error_auc();

                    List<DataGridViewRow> list_raw_data = current_chart.get_Raw_Data();

                    if (auc_dict.ContainsKey(descriptor))
                    {
                        auc_dict[descriptor][BATCH_ID] = AUC;
                    }
                    else
                    {
                        Dictionary<string, double> temp_dict = new Dictionary<string, double>();
                        temp_dict[BATCH_ID] = AUC;
                        auc_dict[descriptor] = temp_dict;
                    }

                    if (auc_error_dict.ContainsKey(descriptor))
                    {
                        auc_error_dict[descriptor][BATCH_ID] = error_auc;
                    }
                    else
                    {
                        Dictionary<string, double> temp_dict = new Dictionary<string, double>();
                        temp_dict[BATCH_ID] = error_auc;
                        auc_error_dict[descriptor] = temp_dict;
                    }

                    if (raw_data_dict.ContainsKey(descriptor))
                    {
                        raw_data_dict[descriptor][BATCH_ID] = list_raw_data;
                    }
                    else
                    {
                        Dictionary<string, List<DataGridViewRow>> temp_dict = new Dictionary<string, List<DataGridViewRow>>();
                        temp_dict[BATCH_ID] = list_raw_data;
                        raw_data_dict[descriptor] = temp_dict;
                    }

                }
            }

            toolStripProgressBar1.Visible = false;

            if (graph_type == "auc")
            {
                // Display the AUC values :

                //Console.WriteLine("BATCH_ID" + "," + "Descriptor" + "," + "AUC");

                form_patient.Reset();
                chart_auc.Clear();

                foreach (KeyValuePair<string, Dictionary<string, double>> item in auc_dict)
                {
                    //Console.WriteLine("BATCH_ID : " + item.Key.ToString());

                    Dictionary<string, double> descriptor_auc = item.Value;
                    Dictionary<string, double> descriptor_auc_error = auc_error_dict[item.Key];

                    Chart_Patient chart = new Chart_Patient(descriptor_auc, descriptor_auc_error, raw_data_dict[item.Key], cpd_target, item.Key.ToString(), Color.Black, form_patient, auc_dict.Count, graph_type);
                    chart_auc.Add(item.Key.ToString(), chart);

                    //foreach (KeyValuePair<string, double> auc in descriptor_auc)
                    //{
                    //    //Console.WriteLine("------ Descriptor : " + auc.Key.ToString());
                    //    //Console.WriteLine("-----------------  AUC : " + auc.Value.ToString());
                    //    //Console.WriteLine(item.Key.ToString() + "," + auc.Key.ToString() + "," + auc.Value.ToString());
                    //}
                }
            }
            else if (graph_type == "z-score")
            {
                form_patient.Reset();
                chart_auc_z_score.Clear();

                Dictionary<string, Dictionary<string, double>> z_score_auc = new Dictionary<string, Dictionary<string, double>>();
                Dictionary<string, Dictionary<string, double>> z_score_auc_error = new Dictionary<string, Dictionary<string, double>>();


                foreach (KeyValuePair<string, Dictionary<string, double>> item in auc_dict)
                {
                    string descriptor = item.Key;
                    Dictionary<string, double> auc_values_by_cpd = item.Value;

                    List<double> auc_values = new List<double>();

                    foreach (KeyValuePair<string, double> val in auc_values_by_cpd)
                    {
                        string cpd = val.Key;
                        //if (cpd != "carfilzomib" && cpd != "Bortezomib") auc_values.Add(val.Value);
                        auc_values.Add(val.Value);
                    }

                    double mu = auc_values.Average();
                    double sigma = StandardDeviation(auc_values, (double)auc_values.Count - 1);

                    Dictionary<string, double> auc_errors = auc_error_dict[item.Key];

                    foreach (KeyValuePair<string, double> val in auc_values_by_cpd)
                    {
                        string BATCH_ID = val.Key;
                        double z_score = (val.Value - mu) / sigma;
                        double error_z_score = auc_errors[val.Key] / sigma;

                        if (z_score_auc.ContainsKey(descriptor))
                        {
                            z_score_auc[descriptor][BATCH_ID] = z_score;
                        }
                        else
                        {
                            Dictionary<string, double> temp_dict = new Dictionary<string, double>();
                            temp_dict[BATCH_ID] = z_score;
                            z_score_auc[descriptor] = temp_dict;
                        }

                        if (z_score_auc_error.ContainsKey(descriptor))
                        {
                            z_score_auc_error[descriptor][BATCH_ID] = error_z_score;
                        }
                        else
                        {
                            Dictionary<string, double> temp_dict = new Dictionary<string, double>();
                            temp_dict[BATCH_ID] = error_z_score;
                            z_score_auc_error[descriptor] = temp_dict;
                        }

                    }

                }

                foreach (KeyValuePair<string, Dictionary<string, double>> item in z_score_auc)
                {
                    Dictionary<string, double> descriptor_auc = item.Value;
                    Dictionary<string, double> descriptor_au_error = z_score_auc_error[item.Key];
                    Chart_Patient chart = new Chart_Patient(descriptor_auc, descriptor_au_error, raw_data_dict[item.Key], cpd_target, item.Key.ToString(), Color.Black, form_patient, auc_dict.Count, graph_type);
                    chart_auc_z_score.Add(item.Key.ToString(), chart);
                }
            }

            form_patient.Show();
        }


        private void computeAUCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // loop on charts to compute the AUC.

            Form fc = Application.OpenForms["Patient_Tab"];

            if (fc != null)
            {
                compute_auc("auc");
            }
            else
            {
                form_patient = new Patient_Tab(this);
                compute_auc("auc");
            }

        }

        private void computeAUCZScoreToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // loop on charts to compute the AUC.

            Form fc = Application.OpenForms["Patient_Tab"];

            if (fc != null)
            {
                compute_auc("z-score");
            }
            else
            {
                form_patient = new Patient_Tab(this);
                compute_auc("z-score");
            }

        }


        private void btn_normalize_Click(object sender, EventArgs e)
        {
            select_DMSO();
        }

        private void showToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form fc = Application.OpenForms["WellPlate_Viewer"];

            if (fc != null)
            {
                well_plate.Show();
                well_plate.draw();
            }
            else
            {
                well_plate = new WellPlate_Viewer(this);
                well_plate.Show();
                well_plate.draw();
            }
        }

        public void drawOverlap1FileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.SetForm();

            DataTable main_table = new DataTable();
            HashSet<string> main_cpds = new HashSet<string>();
            HashSet<string> main_plates = new HashSet<string>();

            openFileDialog1.Filter = "CSV Files (*.csv)|*.csv";

            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.Text = openFileDialog1.FileName;

                System.IO.StreamReader sr = new System.IO.StreamReader(openFileDialog1.FileName);
                CachedCsvReader my_csv = new CachedCsvReader(sr, true);
                main_table.Load(my_csv);

                foreach (DataRow row in main_table.Rows)
                {
                    string cpd = row["BATCH_ID"].ToString();
                    string plate = row["Plate"].ToString();
                    main_cpds.Add(cpd);
                    main_plates.Add(plate);
                }
            }

            foreach (string plate in main_plates)
            {
                dict_file_color.Add(plate, curve_color[dict_file_color.Count]);

                DataTable table = new DataTable();

                table = main_table.Copy();

                table.Rows.Clear();

                foreach (DataRow row_main in main_table.Rows)
                {
                    if (row_main["Plate"].ToString() == plate.ToString())
                    {
                        table.Rows.Add(row_main.ItemArray);
                    }
                }

                data_dict.Add(plate, table);

                foreach (DataRow row in table.Rows)
                {
                    string cpd = row["BATCH_ID"].ToString();

                    if (cpd_link.ContainsKey(cpd))
                    {
                        cpd_link[cpd].Add(plate);
                    }
                    else
                    {
                        HashSet<string> set_files = new HashSet<string>();
                        set_files.Add(plate);
                        cpd_link[cpd] = set_files;
                    }

                }

                Console.WriteLine("Reading --> " + plate);
            }

            //// Print the cpd link
            //foreach (KeyValuePair<string, HashSet<string> > elem in cpd_link)
            //{
            //    Console.WriteLine("BATCH_ID : " + elem.Key);
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
            TimeLine.dataGridView1.Columns[0].Name = "BATCH_ID";
            TimeLine.dataGridView1.AllowUserToAddRows = false;

            foreach (KeyValuePair<string, HashSet<string>> elem in cpd_link)
            {
                int idx = TimeLine.dataGridView1.Rows.Add(new DataGridViewRow());
                TimeLine.dataGridView1.Rows[idx].Cells[0].Value = elem.Key;
                TimeLine.dataGridView1.Rows[idx].Cells[0].Style.BackColor = Color.LightBlue;
            }

            TimeLine.Visible = true;

        }

        public void inactive_cpd(string batch_id)
        {
            foreach (KeyValuePair<string, List<Chart_DRC>> elem in descriptors_chart)
            {
                string BATCH_ID = elem.Key;
                List<Chart_DRC> cpd_charts = elem.Value;

                foreach (Chart_DRC current_chart in cpd_charts)
                {
                    if (current_chart.get_compound_id() == batch_id)
                    {
                        current_chart.set_inactive();
                    }
                }
            }


        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {

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
            descriptor_name = descriptor_name.Replace(@"\", @"_");

            string compound_id1 = compound_id.Replace(@"/", @"_");
            string compound_id2 = compound_id1.Replace(@"\", @"_");

            string output_image = path + "/CPD_" + compound_id2 + "_" + descriptor_name + ".bmp";

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

        private List<double> drc_points_x_disable = new List<double>();
        private List<double> drc_points_y_disable = new List<double>();

        private RectangleAnnotation annotation_ec50 = new RectangleAnnotation();
        private Color chart_color;

        private List<double> x_concentrations;
        private List<double> x_concentrations_log;

        private List<double> y_response;

        private double[] fit_parameters = new double[4];
        private List<double> x_fit;
        private List<double> x_fit_log;

        //private List<double> y_fit;
        private List<double> y_fit_log;

        private List<double> x_log_unique = new List<double>();

        private List<double> y_conf_int_born_sup = new List<double>();
        private List<double> y_conf_int_born_inf = new List<double>();

        private int step_curve;

        private double MinConcentrationLin;
        private double MaxConcentrationLin;

        private double r2;
        private double RelativeError;

        private double err_bottom;
        private double err_top;
        private double err_ec_50;
        private double err_slope;

        private string compound_id;
        private string descriptor;

        private bool data_modified;

        private int descriptor_index;

        private bool not_fitted;
        private bool inactive;
        private bool is_ec50_exact = true; // last 2 points method

        private bool not_fitted_init;
        private bool inactive_init;

        private double auc;
        private double error_auc;

        List<DataGridViewRow> raw_data;
        List<double> y_raw_data;
        List<double> x_raw_data;
        List<string> plate_per_point;
        List<string> well_per_point;

        List<bool> is_raw_data_removed;

        private Curves_Options options_form;
        private Curve_Fit_Options options_fit_form;

        private double minX = -1;
        private double maxX = -1;
        private double minY = -1e10;
        private double maxY = -1e10;

        private double min_bound_x = 0.0;
        private double max_bound_x = 0.0;
        private double min_bound_y = 0.0;
        private double max_bound_y = 0.0;

        private double fixed_top = 0.0;

        private Dictionary<string, double> fit_bounds;

        private bool manual_bounds = false;
        private bool bound_auto = true;

        private bool general_params;

        private bool is_top_fixed = false;

        private bool patient = false;
        private bool display_fit = true;
        private bool display_post_paint = true;
        private bool confidence_interval = true;
        private bool display_confidence_interval = true;
        private bool dmso = false;
        private bool fixed_y_max = false;

        private List<string> list_wells = new List<string>();

        public string get_compound_id()
        {
            return compound_id;
        }

        public bool top_fixed()
        {
            return is_top_fixed;
        }

        public double get_top_fixed()
        {
            return fixed_top;
        }

        public void set_top_fixed(bool test)
        {
            is_top_fixed = test;
        }

        public void set_top_fixed_value(double val)
        {
            fixed_top = val;
        }

        public void set_general_params(bool test)
        {
            general_params = test;

            if (test)
            {
                manual_bounds = false;
                bound_auto = false;
            }
        }

        public void set_bound_status(bool status)
        {
            bound_auto = status;
        }

        public void set_manual_bound(bool status)
        {
            manual_bounds = status;
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
            fit_bounds["min_x"] = x_min;

            if (general_params == false) manual_bounds = true;
        }

        public void set_max_bound_x(double x_max)
        {
            max_bound_x = x_max;
            fit_bounds["max_x"] = x_max;

            if (general_params == false) manual_bounds = true;
        }

        public void set_min_bound_y(double y_min)
        {
            min_bound_y = y_min;
            fit_bounds["min_y"] = y_min;

            if (general_params == false) manual_bounds = true;
        }

        public void set_max_bound_y(double y_max)
        {
            max_bound_y = y_max;
            fit_bounds["max_y"] = y_max;

            if (general_params == false) manual_bounds = true;
        }

        public double get_window_x_min()
        {
            return minX;
        }

        public double get_window_x_max()
        {
            return maxX;
        }

        public double get_window_y_min()
        {
            return chart.ChartAreas[0].AxisY.Minimum;
            //minY;
        }

        public double get_window_y_max()
        {
            return chart.ChartAreas[0].AxisY.Maximum;
        }

        public void set_window_x_min(double min_x)
        {
            minX = min_x;
        }

        public void set_window_x_max(double max_x)
        {
            maxX = max_x;
        }

        public void set_window_y_min(double min_y)
        {
            minY = min_y;
            chart.ChartAreas[0].AxisY.Minimum = minY;
        }

        public void set_window_y_max(double max_y)
        {
            fixed_y_max = true;
            maxY = max_y;
            chart.ChartAreas[0].AxisY.Maximum = max_y;
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

        public void set_data_modified(bool test)
        {
            data_modified = test;
        }

        public bool is_fit_modified()
        {
            if (general_params || is_top_fixed || manual_bounds) return true;
            else return false;
        }

        public void set_Raw_Data(List<DataGridViewRow> data)
        {
            raw_data = data.ToList();

            y_raw_data = new List<double>();
            x_raw_data = new List<double>();
            plate_per_point = new List<string>();
            well_per_point = new List<string>();

            foreach (DataGridViewRow item in raw_data)
            {
                y_raw_data.Add(double.Parse(item.Cells[descriptor].Value.ToString()));
                x_raw_data.Add(double.Parse(item.Cells["Concentration"].Value.ToString()));
                plate_per_point.Add(item.Cells["Plate"].Value.ToString());
                well_per_point.Add(item.Cells["Well"].Value.ToString());
            }
        }

        public void set_dmso_wells(List<string> wells)
        {
            dmso = true;
            list_wells = wells;
            chart.ChartAreas[0].AxisX.Title = "Controls";
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

        public List<double> get_enable_y_values()
        {
            return drc_points_y_enable;
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

        public double get_auc()
        {
            return auc;
        }

        public double get_error_auc()
        {
            return error_auc;
        }

        public Chart_DRC()
        {
        }

        public Chart_DRC(string cpd, string descript, int step, ref List<double> x, ref List<double> x_log, ref List<double> resp, Color color,
            int index, List<string> deselected, string ec_50_status, Dictionary<string, double> bounds, string fix_top, string if_modified, MainTab form,
            bool if_patient, bool if_fit, bool if_post_paint, bool confidence)
        {
            _form1 = form;

            patient = if_patient;
            display_fit = if_fit;
            display_post_paint = if_post_paint;
            confidence_interval = confidence;

            descriptor_index = index;

            compound_id = cpd;
            descriptor = descript;
            step_curve = step;
            chart_color = color;

            data_modified = false;
            if (if_modified == "True" || if_modified == "TRUE" || if_modified == "true") data_modified = true;

            fit_bounds = bounds;
            if (fit_bounds.Count() > 0)
            {
                set_bound_status(false);
                set_manual_bound(true);
            }

            not_fitted = false;

            if (ec_50_status == "=") is_ec50_exact = true;
            else if (ec_50_status == ">") is_ec50_exact = false;

            double fixed_top_val;
            if (fix_top != "Not Fixed")
            {
                bool is_converted = double.TryParse(fix_top, out fixed_top_val);

                fixed_top = fixed_top_val;

                set_bound_status(true);
                set_manual_bound(true);
                set_general_params(false);
                set_top_fixed(true);
                //set_data_modified(true);
            }
            else
            {
                set_top_fixed(false);
            }

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

                    //int remove_index = drc_points_y_enable.FindIndex(a => a < point_y + .000001 && a > point_y - .000001);

                    int remove_index = 0;

                    List<int> indices = new List<int>();
                    for (int i = 0; i < drc_points_y_enable.Count(); i++)
                        if (drc_points_y_enable[i] < point_y + 1e-6 && drc_points_y_enable[i] > point_y - 1e-6)
                            indices.Add(i);

                    foreach (int idx in indices)
                    {
                        if (drc_points_x_enable[idx] < (x_concentrations_log[index_deselect] + 1e-12) && drc_points_x_enable[idx] > (x_concentrations_log[index_deselect] - 1e-12))
                        {
                            remove_index = idx;
                            break;
                        }
                    }

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
            Series serie_mean = new Series();

            Series serie_ec_50_line_x = new Series();
            Series serie_ec_50_line_y = new Series();

            Series serie_born_inf = new Series();
            Series serie_born_sup = new Series();

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
            serie_mean.ChartType = SeriesChartType.Point;

            serie_ec_50_line_x.ChartType = SeriesChartType.Line;
            serie_ec_50_line_y.ChartType = SeriesChartType.Line;

            series1.MarkerStyle = MarkerStyle.Circle;
            serie_mean.MarkerStyle = MarkerStyle.Circle;

            serie_born_inf.ChartType = SeriesChartType.Line;
            serie_born_sup.ChartType = SeriesChartType.Line;

            series1.Name = "Series1";
            series2.Name = "Series2";
            serie_mean.Name = "Serie_Mean";

            serie_ec_50_line_x.Name = "line_ec_50_x";
            serie_ec_50_line_y.Name = "line_ec_50_y";

            serie_born_inf.Name = "Born_Inf";
            serie_born_sup.Name = "Born_Sup";

            chart.Series.Add(series1);
            chart.Series.Add(series2);
            chart.Series.Add(serie_mean);

            chart.Series.Add(serie_ec_50_line_x);
            chart.Series.Add(serie_ec_50_line_y);

            chart.Series.Add(serie_born_inf);
            chart.Series.Add(serie_born_sup);

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
            chart.PostPaint += new EventHandler<ChartPaintEventArgs>(this.chart1_PostPaint);
            chart.Paint += new PaintEventHandler(this.chart1_Paint);

            //Create a rectangle annotation

            //RectangleAnnotation annotationRectangle = new RectangleAnnotation();
            //annotation_ec50 = annotationRectangle;

            //chart.ChartAreas[0].AxisX.Minimum = -10;
            //chart.ChartAreas[0].AxisX.Maximum = -5;

            //chart.ChartAreas[0].AxisY.Minimum = -1;
            //chart.ChartAreas[0].AxisY.Maximum = +1;

            //draw_DRC(false, false);

            chart.ChartAreas[0].AxisX.MajorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dash;
            chart.ChartAreas[0].AxisY.MajorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dash;

            chart.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightGray;
            chart.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;

            general_params = false;

            //chart.ChartAreas[0].RecalculateAxesScale();

            //if (minY < -1e10 + 1)
            //{
            minY = chart.ChartAreas[0].AxisY.Minimum;
            //}

            //if (maxY < -1e10 + 1)
            //{
            maxY = chart.ChartAreas[0].AxisY.Maximum;
            //}

            fit_DRC();

            double min_curve = Math.Min(y_fit_log[0], y_fit_log[y_fit_log.Count - 1]);
            double max_curve = Math.Max(y_fit_log[0], y_fit_log[y_fit_log.Count - 1]);
            double amplitude = max_curve - min_curve;

            if (fixed_y_max == false)
            {
                minY = min_curve - amplitude * 0.5;
                maxY = max_curve + amplitude * 0.5;
            }

            //draw_DRC(false, false);
        }

        private void chart1_Paint(object sender, PaintEventArgs e)
        {
            if (confidence_interval && display_confidence_interval && !compound_id.Contains("DMSO"))
            {
                // we assume two series variables are set..:
                if (chart.Series["Born_Inf"] == null || chart.Series["Born_Sup"] == null) return;

                // short references:
                Axis ax = chart.ChartAreas[0].AxisX;
                Axis ay = chart.ChartAreas[0].AxisY;

                // now we convert all values to pixels
                List<PointF> points1 = chart.Series["Born_Inf"].Points.Select(x =>
                   new PointF((float)ax.ValueToPixelPosition(x.XValue),
                              (float)ay.ValueToPixelPosition(x.YValues[0]))).ToList();

                List<PointF> points2 = chart.Series["Born_Sup"].Points.Select(x =>
                   new PointF((float)ax.ValueToPixelPosition(x.XValue),
                              (float)ay.ValueToPixelPosition(x.YValues[0]))).ToList();

                // one list forward, the other backward:
                points2.Reverse();

                GraphicsPath gp = new GraphicsPath();
                gp.FillMode = FillMode.Winding;  // the right fillmode

                // it will work fine with either Splines or Lines:
                if (chart.Series["Born_Inf"].ChartType == SeriesChartType.Spline) gp.AddCurve(points1.ToArray());
                else gp.AddLines(points1.ToArray());
                if (chart.Series["Born_Sup"].ChartType == SeriesChartType.Spline) gp.AddCurve(points2.ToArray());
                else gp.AddLines(points2.ToArray());

                // pick your own color, maybe a mix of the Series colors..
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(25, chart_color)))
                    e.Graphics.FillPath(brush, gp);
                gp.Dispose();
            }
            // Don't uncomment this : infinite looping
            //else
            //{
            //    chart.Series["Born_Inf"].Points.Clear();
            //    chart.Series["Born_Sup"].Points.Clear();
            //}
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

        //public static void function1_grad(double[] c, double[] x, ref double func, double[] grad, object obj)
        //{
        //    func = c[0] + ((c[1] - c[0]) / (1 + Math.Pow(10, (c[2] - x[0]) * c[3])));

        //    grad[0] = 1.0 - 1.0 / (1 + Math.Pow(10, c[3] * (c[2] - x[0])));
        //    grad[1] = 1.0 / (1 + Math.Pow(10, c[3] * (c[2] - x[0])));
        //    grad[2] = -1.0 * (c[3] * Math.Log(10) * (c[1] - c[0]) * Math.Pow(10, c[3] * (c[2] - x[0]))) / ((1 + Math.Pow(10, c[3] * (c[2] - x[0]))) * (1 + Math.Pow(10, c[3] * (c[2] - x[0]))));
        //    grad[3] = -1.0 * (Math.Log(10) * (c[1] - c[0]) * (c[2] - x[0]) * Math.Pow(10, c[3] * (c[2] - x[0]))) / ((1 + Math.Pow(10, c[3] * (c[2] - x[0]))) * (1 + Math.Pow(10, c[3] * (c[2] - x[0]))));
        //}


        private static void function_SigmoidInhibition_3_params(double[] c, double[] x, ref double func, object obj)
        {
            double fixed_top = (double)obj;

            func = c[0] + ((fixed_top - c[0]) / (1 + Math.Pow(10, (c[1] - x[0]) * c[2])));
        }

        private double Sigmoid(double[] c, double x)
        {
            double y = c[0] + ((c[1] - c[0]) / (1 + Math.Pow(10, (c[2] - x) * c[3])));
            return y;
        }

        private double Sigmoid_3_params(double[] c, double x, double top)
        {
            double y = c[0] + ((top - c[0]) / (1 + Math.Pow(10, (c[1] - x) * c[2])));
            return y;
        }

        private double compute_jacobian_param_0(double a0, double a1, double a2, double a3, double x)
        {
            return 1.0 - 1.0 / (1 + Math.Pow(10, a3 * (a2 - x)));
        }

        private double compute_jacobian_param_1(double a0, double a1, double a2, double a3, double x)
        {
            return 1.0 / (1.0 + Math.Pow(10, a3 * (a2 - x)));
        }

        private double compute_jacobian_param_2(double a0, double a1, double a2, double a3, double x)
        {
            return -1.0 * (a3 * Math.Log(10) * (a1 - a0) * Math.Pow(10, a3 * (a2 - x))) / ((1 + Math.Pow(10, a3 * (a2 - x))) * (1.0 + Math.Pow(10, a3 * (a2 - x))));
        }

        private double compute_jacobian_param_3(double a0, double a1, double a2, double a3, double x)
        {
            return -1.0 * (Math.Log(10) * (a1 - a0) * (a2 - x) * Math.Pow(10, a3 * (a2 - x))) / ((1.0 + Math.Pow(10, a3 * (a2 - x))) * (1.0 + Math.Pow(10, a3 * (a2 - x))));
        }


        public static void compute_chi_square(double[] c, ref double func, double[] grad, object obj)
        {
            double[,] data = (double[,])obj;

            //grad = new double[4];

            func = 0.0;
            grad[0] = 0.0;
            grad[1] = 0.0;
            grad[2] = 0.0;
            grad[3] = 0.0;

            // Compute the cost function and the exact gradient

            double dof = 1.0 / (data.GetLength(0) - 4);

            for (int i = 0; i < data.GetLength(0); ++i)
            {

                double denom = 1 + Math.Pow(10, c[3] * (c[2] - data[i, 0]));

                double y_pred = c[0] + (c[1] - c[0]) / denom;

                //grad[0] += 1.0 - 1.0 / (1 + Math.Pow(10, c[3] * (c[2] - data[i, 0])));
                //grad[1] += 1.0 / (1 + Math.Pow(10, c[3] * (c[2] - data[i, 0])));
                //grad[2] += -1.0 * (c[3] * Math.Log(10) * (c[1] - c[0]) * Math.Pow(10, c[3] * (c[2] - data[i, 0]))) / ((1 + Math.Pow(10, c[3] * (c[2] - data[i, 0]))) * (1 + Math.Pow(10, c[3] * (c[2] - data[i, 0]))));
                //grad[3] += -1.0 * (Math.Log(10) * (c[1] - c[0]) * (c[2] - data[i, 0]) * Math.Pow(10, c[3] * (c[2] - data[i, 0]))) / ((1 + Math.Pow(10, c[3] * (c[2] - data[i, 0]))) * (1 + Math.Pow(10, c[3] * (c[2] - data[i, 0]))));

                //grad[0] += 2 * dof * (-1 - 1 / (denom)) * ((c[1] - c[0]) / denom + y_pred - c[0]);
                //grad[1] += 2 * dof * ((c[1] - c[0]) / denom - c[0] + y_pred) / denom;
                //grad[2] += (c[3] * dof * Math.Log(10) * (c[1] - c[0]) * Math.Pow(2, c[3] * (c[2] - data[i, 0]) + 1) * Math.Pow(5, c[3] * (c[2] - data[i, 0])) * (y_pred - c[0] + (c[1] - c[0]) / denom)) / (denom * denom);
                //grad[3] += -1.0 * dof * ((c[2]-data[i,0])*Math.Log(10)*(c[1]-c[0])*Math.Pow(2, 1+c[2]*c[3]-c[3]*data[i,0])*Math.Pow(5,(c[2]-data[i,0])*c[3])*(y_pred-c[0]+ (c[1]-c[0])/denom))/ (denom * denom);

                double b2 = c[0]; // 0.0; //c[0];
                double t2 = c[1]; // 1.0; // c[1];
                double e2 = c[2]; // -7.10568394; // c[2];
                double s2 = c[3];
                double y2 = data[i, 1];
                double w2 = data[i, 0];

                //double a = (2 - 2 / (Math.Pow(10, (s2 * (e2 - w2))) + 1)) * (b2 - y2 + (-b2 + t2) / (Math.Pow(10, (s2 * (e2 - w2))) + 1));

                grad[0] += (2 - 2 / (Math.Pow(10, (s2 * (e2 - w2))) + 1)) * (b2 - y2 + (-b2 + t2) / (Math.Pow(10, (s2 * (e2 - w2))) + 1));
                grad[1] += 2 * (b2 - y2 + (-b2 + t2) / (Math.Pow(10, (s2 * (e2 - w2))) + 1)) / (Math.Pow(10, (s2 * (e2 - w2))) + 1);
                grad[2] += -2 * Math.Pow(10, (s2 * (e2 - w2))) * s2 * (-b2 + t2) * (b2 - y2 + (-b2 + t2) / (Math.Pow(10, (s2 * (e2 - w2))) + 1)) * Math.Log(10) / Math.Pow((Math.Pow(10, (s2 * (e2 - w2))) + 1), 2);
                grad[3] += -2 * Math.Pow(10, (s2 * (e2 - w2))) * (-b2 + t2) * (e2 - w2) * (b2 - y2 + (-b2 + t2) / (Math.Pow(10, (s2 * (e2 - w2))) + 1)) * Math.Log(10) / Math.Pow((Math.Pow(10, (s2 * (e2 - w2))) + 1), 2);

                func += Math.Pow(y2 - y_pred, 2);
            }

            grad[0] *= dof;
            grad[1] *= dof;
            grad[2] *= dof;
            grad[3] *= dof;
            func *= dof;
        }

        private double[] compute_jacobian_chi_square(double[] c, double[,] data)
        {

            double[] grad = new double[4];

            grad[0] = 0.0;
            grad[1] = 0.0;
            grad[2] = 0.0;
            grad[3] = 0.0;

            // Compute the cost function and the exact gradient

            double dof = 1.0 / (data.GetLength(0) - 4);

            for (int i = 0; i < data.GetLength(0); ++i)
            {

                double denom = 1 + Math.Pow(10, c[3] * (c[2] - data[i, 0]));

                double y_pred = c[0] + (c[1] - c[0]) / denom;

                double b2 = c[0]; // 0.0; //c[0];
                double t2 = c[1]; // 1.0; // c[1];
                double e2 = c[2]; // -7.10568394; // c[2];
                double s2 = c[3];
                double y2 = data[i, 1];
                double w2 = data[i, 0];

                grad[0] += (2 - 2 / (Math.Pow(10, (s2 * (e2 - w2))) + 1)) * (b2 - y2 + (-b2 + t2) / (Math.Pow(10, (s2 * (e2 - w2))) + 1));
                grad[1] += 2 * (b2 - y2 + (-b2 + t2) / (Math.Pow(10, (s2 * (e2 - w2))) + 1)) / (Math.Pow(10, (s2 * (e2 - w2))) + 1);
                grad[2] += -2 * Math.Pow(10, (s2 * (e2 - w2))) * s2 * (-b2 + t2) * (b2 - y2 + (-b2 + t2) / (Math.Pow(10, (s2 * (e2 - w2))) + 1)) * Math.Log(10) / Math.Pow((Math.Pow(10, (s2 * (e2 - w2))) + 1), 2);
                grad[3] += -2 * Math.Pow(10, (s2 * (e2 - w2))) * (-b2 + t2) * (e2 - w2) * (b2 - y2 + (-b2 + t2) / (Math.Pow(10, (s2 * (e2 - w2))) + 1)) * Math.Log(10) / Math.Pow((Math.Pow(10, (s2 * (e2 - w2))) + 1), 2);

            }

            grad[0] *= dof;
            grad[1] *= dof;
            grad[2] *= dof;
            grad[3] *= dof;

            return grad;

        }

        public static void compute_chi_square2(double[] c, ref double func, object obj)
        {
            double[,] data = (double[,])obj;

            // Compute the cost function and the exact gradient

            double dof = 1.0 / (data.GetLength(0) - 4);

            //c[0] = 0.0;
            //c[1] = 1.0;
            //c[2] = -7.10568394;
            //c[3] = 1.0;

            for (int i = 0; i < data.GetLength(0); ++i)
            {
                double denom = 1 + Math.Pow(10, c[3] * (c[2] - data[i, 0]));
                double y_pred = c[0] + (c[1] - c[0]) / denom;

                func += Math.Pow(data[i, 1] - y_pred, 2);
            }

            func *= dof;
        }

        private double[,] compute_hessian(double[] c, double[,] data)
        {

            double[,] H = new double[4, 4];

            for (int i = 0; i < H.GetLength(0); i++)
            {
                for (int j = 0; j < H.GetLength(1); ++j)
                {
                    H[i, j] = 0.0;
                }
            }


            double dof = 1.0 / (data.GetLength(0) - 4.0);

            for (int i = 0; i < data.GetLength(0); ++i)
            {

                double w = data[i, 0];
                double y_obs = data[i, 1];

                double b = c[0];
                double t = c[1];
                double e = c[2];
                double s = c[3];

                H[0, 0] += (1 - 1 / (Math.Pow(10, (s * (e - w))) + 1)) * (2 - 2 / (Math.Pow(10, (s * (e - w))) + 1));

                H[0, 1] += (2 - 2 / (Math.Pow(10, (s * (e - w))) + 1)) / (Math.Pow(10, (s * (e - w))) + 1);

                H[0, 2] += -Math.Pow(10, (s * (e - w))) * s * (2 - 2 / (Math.Pow(10, (s * (e - w))) + 1)) * (-b + t) * Math.Log(10) / Math.Pow((
                    Math.Pow(10, (s * (e - w))) + 1), 2) + 2 * Math.Pow(10, (s * (e - w))) * s * (
                    b - y_obs + (-b + t) / (Math.Pow(10, (s * (e - w))) + 1)) * Math.Log(10) / Math.Pow((Math.Pow(10, (s * (e - w))) + 1), 2);

                H[0, 3] += -Math.Pow(10, (s * (e - w))) * (2 - 2 / (Math.Pow(10, (s * (e - w))) + 1)) * (-b + t) * (e - w) * Math.Log(10) / Math.Pow((
                    Math.Pow(10, (s * (e - w))) + 1), 2) + 2 * Math.Pow(10, (s * (e - w))) * (e - w) * (
                    b - y_obs + (-b + t) / (Math.Pow(10, (s * (e - w))) + 1)) * Math.Log(10) / Math.Pow((Math.Pow(10, (s * (e - w))) + 1), 2);

                H[1, 0] += (2 - 2 / (Math.Pow(10, (s * (e - w))) + 1)) / (Math.Pow(10, (s * (e - w))) + 1);

                H[1, 1] += 2 / Math.Pow((Math.Pow(10, (s * (e - w))) + 1), 2);

                H[1, 2] += -Math.Pow(10, (s * (e - w))) * s * (2 * b - 2 * y_obs + 2 * (-b + t) / (Math.Pow(10, (s * (e - w))) + 1)) * Math.Log(10) / Math.Pow((
                    Math.Pow(10, (s * (e - w))) + 1), 2) - Math.Pow(10, (s * (e - w))) * s * (-2 * b + 2 * t) * Math.Log(10) / Math.Pow((
                    Math.Pow(10, (s * (e - w))) + 1), 3);

                H[1, 3] += -Math.Pow(10, (s * (e - w))) * (e - w) * (2 * b - 2 * y_obs + 2 * (-b + t) / (Math.Pow(10, (s * (e - w))) + 1)) * Math.Log(10) / Math.Pow((
                    Math.Pow(10, (s * (e - w))) + 1), 2) - Math.Pow(10, (s * (e - w))) * (-2 * b + 2 * t) * (e - w) * Math.Log(10) / Math.Pow((
                    Math.Pow(10, (s * (e - w))) + 1), 3);

                H[2, 0] += -2 * Math.Log(10) * Math.Pow(10, (s * (e - w))) * s * (1 - 1 / (Math.Pow(10, (s * (e - w))) + 1)) * (-b + t) / Math.Pow((
                    Math.Pow(10, (s * (e - w))) + 1), 2) + 2 * Math.Log(10) * Math.Pow(10, (s * (e - w))) * s * (
                    b - y_obs + (-b + t) / (Math.Pow(10, (s * (e - w))) + 1)) / Math.Pow((Math.Pow(10, (s * (e - w))) + 1), 2);

                H[2, 1] += -2 * Math.Log(10) * Math.Pow(10, (s * (e - w))) * s * (b - y_obs + (-b + t) / (Math.Pow(10, (s * (e - w))) + 1)) / Math.Pow((
                    Math.Pow(10, (s * (e - w))) + 1), 2) - 2 * Math.Log(10) * Math.Pow(10, (s * (e - w))) * s * (-b + t) / Math.Pow((
                    Math.Pow(10, (s * (e - w))) + 1), 3);

                H[2, 2] += 4 * Math.Log(10) * Math.Pow(10, (2 * s * (e - w))) * Math.Pow(s, 2) * (-b + t) * (
                    b - y_obs + (-b + t) / (Math.Pow(10, (s * (e - w))) + 1)) * Math.Log(10) / Math.Pow((
                    Math.Pow(10, (s * (e - w))) + 1), 3) + 2 * Math.Log(10) * Math.Pow(10, (2 * s * (e - w))) * Math.Pow(s, 2) * Math.Pow((
                    -b + t), 2) * Math.Log(10) / Math.Pow((Math.Pow(10, (s * (e - w))) + 1), 4) - 2 * Math.Log(10) * Math.Pow(10, (
                    s * (e - w))) * Math.Pow(s, 2) * (-b + t) * (b - y_obs + (-b + t) / (Math.Pow(10, (s * (e - w))) + 1)) * Math.Log(
                    10) / Math.Pow((Math.Pow(10, (s * (e - w))) + 1), 2);

                H[2, 3] += 4 * Math.Log(10) * Math.Pow(10, (2 * s * (e - w))) * s * (-b + t) * (e - w) * (
                    b - y_obs + (-b + t) / (Math.Pow(10, (s * (e - w))) + 1)) * Math.Log(10) / Math.Pow((
                    Math.Pow(10, (s * (e - w))) + 1), 3) + 2 * Math.Log(10) * Math.Pow(10, (2 * s * (e - w))) * s * Math.Pow((-b + t), 2) * (
                    e - w) * Math.Log(10) / Math.Pow((Math.Pow(10, (s * (e - w))) + 1), 4) - 2 * Math.Log(10) * Math.Pow(10, (s * (e - w))) * s * (
                    -b + t) * (e - w) * (b - y_obs + (-b + t) / (Math.Pow(10, (s * (e - w))) + 1)) * Math.Log(10) / Math.Pow((
                    Math.Pow(10, (s * (e - w))) + 1), 2) - 2 * Math.Log(10) * Math.Pow(10, (s * (e - w))) * (-b + t) * (
                    b - y_obs + (-b + t) / (Math.Pow(10, (s * (e - w))) + 1)) / Math.Pow((Math.Pow(10, (s * (e - w))) + 1), 2);

                H[3, 0] += -2 * Math.Log(10) * Math.Pow(10, s * (e - w)) * (1 - 1 / (Math.Pow(10, s * (e - w)) + 1)) * (-b + t) * (e - w) / Math.Pow((
                           Math.Pow(10, s * (e - w)) + 1), 2) + 2 * Math.Log(10) * Math.Pow(10, s * (e - w)) * (e - w) * (
                            b - y_obs + (-b + t) / (Math.Pow(10, s * (e - w)) + 1)) / Math.Pow(Math.Pow(10, s * (e - w)) + 1, 2);

                H[3, 1] += -2 * Math.Log(10) * Math.Pow(10, s * (e - w)) * (e - w) * (
                            b - y_obs + (-b + t) / (Math.Pow(10, s * (e - w)) + 1)) / Math.Pow((
                           Math.Pow(10, s * (e - w)) + 1), 2) - 2 * Math.Log(10) * Math.Pow(10, s * (e - w)) * (-b + t) * (e - w) / Math.Pow((
                           Math.Pow(10, s * (e - w)) + 1), 3);

                H[3, 2] += 4 * Math.Log(10) * Math.Pow(10, (2 * s * (e - w))) * s * (-b + t) * (e - w) * (
                            b - y_obs + (-b + t) / (Math.Pow(10, s * (e - w)) + 1)) * Math.Log(10) / Math.Pow((
                           Math.Pow(10, s * (e - w)) + 1), 3) + 2 * Math.Log(10) * Math.Pow(10, (2 * s * (e - w))) * s * Math.Pow((-b + t), 2) * (
                            e - w) * Math.Log(10) / Math.Pow((Math.Pow(10, s * (e - w)) + 1), 4) - 2 * Math.Log(10) * Math.Pow(10, s * (e - w)) * s * (
                            -b + t) * (e - w) * (b - y_obs + (-b + t) / (Math.Pow(10, s * (e - w)) + 1)) * Math.Log(10) / Math.Pow((
                           Math.Pow(10, s * (e - w)) + 1), 2) - 2 * Math.Log(10) * Math.Pow(10, s * (e - w)) * (-b + t) * (
                            b - y_obs + (-b + t) / (Math.Pow(10, s * (e - w)) + 1)) / Math.Pow(Math.Pow(10, s * (e - w)) + 1, 2);

                H[3, 3] += 4 * Math.Log(10) * Math.Pow(10, (2 * s * (e - w))) * (-b + t) * Math.Pow((e - w), 2) * (
                            b - y_obs + (-b + t) / (Math.Pow(10, s * (e - w)) + 1)) * Math.Log(10) / Math.Pow((
                           Math.Pow(10, s * (e - w)) + 1), 3) + 2 * Math.Log(10) * Math.Pow(10, (2 * s * (e - w))) * Math.Pow((-b + t), 2) * Math.Pow((
                            e - w), 2) * Math.Log(10) / Math.Pow((Math.Pow(10, s * (e - w)) + 1), 4) - 2 * Math.Log(10) * Math.Pow(10, s * (e - w)) * (
                            -b + t) * Math.Pow((e - w), 2) * (b - y_obs + (-b + t) / (Math.Pow(10, s * (e - w)) + 1)) * Math.Log(10) / Math.Pow((
                           Math.Pow(10, s * (e - w)) + 1), 2);

            }

            H[0, 0] *= dof;
            H[0, 1] *= dof;
            H[0, 2] *= dof;
            H[0, 3] *= dof;

            H[1, 0] *= dof;
            H[1, 1] *= dof;
            H[1, 2] *= dof;
            H[1, 3] *= dof;

            H[2, 0] *= dof;
            H[2, 1] *= dof;
            H[2, 2] *= dof;
            H[2, 3] *= dof;

            H[3, 0] *= dof;
            H[3, 1] *= dof;
            H[3, 2] *= dof;
            H[3, 3] *= dof;

            return H;
        }

        private double compute_least_square_error(double[,] cov, double a0, double a1, double a2, double a3, double x)
        {

            double[,] jacobian = {
                {compute_jacobian_param_0(a0, a1, a2, a3, x)},
                {compute_jacobian_param_1(a0, a1, a2, a3, x)},
                {compute_jacobian_param_2(a0, a1, a2, a3, x)},
                {compute_jacobian_param_3(a0, a1, a2, a3, x)},
                                 };

            double[,] jacobianT = jacobian.Transpose();

            double[,] A = cov.Dot(jacobian);

            double[,] B = jacobianT.Dot(A);

            double c = B[0, 0];

            return c;
        }

        private double compute_least_square_error2(double[,] cov, double a0, double a1, double a2, double a3, double x)
        {

            double[,] jacobian = {
                {compute_jacobian_param_0(a0, a1, a2, a3, x)},
                {compute_jacobian_param_1(a0, a1, a2, a3, x)},
                {compute_jacobian_param_2(a0, a1, a2, a3, x)},
                {compute_jacobian_param_3(a0, a1, a2, a3, x)},
                                 };

            double[,] jacobianT = jacobian.Transpose();

            double[,] A = cov.Dot(jacobian);

            double[,] B = jacobianT.Dot(A);

            double c = B[0, 0];

            return c;
        }


        private double compute_least_square_error_chi_square(double[,] cov, double[] c, double[,] data)
        {

            double[] jac = compute_jacobian_chi_square(c, data);

            double[,] jacobian = {
                                   {jac[0]},
                                   {jac[1]},
                                   {jac[2]},
                                   {jac[3]}
                                  };

            double[,] jacobianT = jacobian.Transpose();

            double[,] A = cov.Dot(jacobian);

            double[,] B = jacobianT.Dot(A);

            double val = B[0, 0];

            return val;
        }

        private double compute_least_square_error_3_params(double[,] cov, double a0, double a1, double a2, double a3, double x)
        {

            double[,] jacobian = {
                {compute_jacobian_param_0(a0, a1, a2, a3, x)},
                {compute_jacobian_param_2(a0, a1, a2, a3, x)},
                {compute_jacobian_param_3(a0, a1, a2, a3, x)},
                                 };

            double[,] jacobianT = jacobian.Transpose();

            double[,] A = cov.Dot(jacobian);

            double[,] B = jacobianT.Dot(A);

            double c = B[0, 0];

            return c;
        }

        private double sum_sqaure_residuals(List<double> drc_points_x_enable, List<double> drc_points_y_enable, double[] c)
        {
            double sum_square_residuals = 0.0;

            for (int i = 0; i < drc_points_x_enable.Count; ++i)
            {
                double x = drc_points_x_enable[i];

                double y_fit_curve = Sigmoid(c, x);

                double residual_square = (drc_points_y_enable[i] - y_fit_curve) * (drc_points_y_enable[i] - y_fit_curve);

                sum_square_residuals += residual_square;
            }

            return sum_square_residuals;
        }

        private double sum_sqaure_residuals_3_params(List<double> drc_points_x_enable, List<double> drc_points_y_enable, double[] c, double top)
        {
            double sum_square_residuals = 0.0;

            for (int i = 0; i < drc_points_x_enable.Count; ++i)
            {
                double x = drc_points_x_enable[i];

                double y_fit_curve = Sigmoid_3_params(c, x, top);

                double residual_square = (drc_points_y_enable[i] - y_fit_curve) * (drc_points_y_enable[i] - y_fit_curve);

                sum_square_residuals += residual_square;
            }

            return sum_square_residuals;
        }

        public void fit_DRC()
        {
            double GlobalMax = double.MinValue;
            double MaxValues = MaxA(drc_points_y_enable.ToArray());

            GlobalMax = MaxValues + 0.05 * Math.Abs(MaxValues);

            double GlobalMin = double.MaxValue;
            double MinValues = MinA(drc_points_y_enable.ToArray());

            GlobalMin = MinValues - 0.05 * Math.Abs(MinValues);

            double epsf = 0;
            double epsx = 1e-6; // 0.000000001;
            double diffstep = 1e-8;

            //double epsx = 1e-6;
            int maxits = 0;
            int info;

            if (bound_auto)
            {
                min_bound_y = GlobalMin;
                max_bound_y = GlobalMax;

                min_bound_x = Math.Log10(MaxConcentrationLin) + 1.0;
                max_bound_x = Math.Log10(MinConcentrationLin) - 1.0;
            }

            if (fit_bounds.Count() > 0 && manual_bounds && !bound_auto)
            {
                min_bound_y = fit_bounds["min_y"];
                max_bound_y = fit_bounds["max_y"];

                min_bound_x = fit_bounds["min_x"];
                max_bound_x = fit_bounds["max_x"];
            }

            // Top Fixed = fit with 3 params
            if (is_top_fixed)
            {
                double BaseEC50 = Math.Log10(MaxConcentrationLin) - Math.Abs(Math.Log10(MaxConcentrationLin) - Math.Log10(MinConcentrationLin)) / 2.0;

                double first_slope = (GlobalMax - GlobalMin) / (Math.Log10(MaxConcentrationLin) - Math.Log10(MinConcentrationLin));
                if ((GlobalMax - GlobalMin) > 0)
                {
                    first_slope = -Math.Abs(first_slope);
                }
                else
                {
                    first_slope = +Math.Abs(first_slope);
                }

                double[] c = new double[] { min_bound_y, BaseEC50, 0.0 };

                double[] bndl = null;
                double[] bndu = null;

                // boundaries
                bndu = new double[] { max_bound_y, min_bound_x, +10.0 * Math.Abs(first_slope) };
                bndl = new double[] { min_bound_y, max_bound_x, -10.0 * Math.Abs(first_slope) };

                alglib.lsfitstate state;
                alglib.lsfitreport rep;

                // Fitting without weights
                //alglib.lsfitcreatefg(Concentrations, Values.ToArray(), c, false, out state);

                double[,] Concentration = new double[drc_points_x_enable.Count(), 1];
                for (var i = 0; i < drc_points_x_enable.Count(); ++i)
                {
                    Concentration[i, 0] = drc_points_x_enable[i];
                }

                alglib.lsfitcreatef(Concentration, drc_points_y_enable.ToArray(), c, diffstep, out state);
                alglib.lsfitsetcond(state, epsx, maxits);
                alglib.lsfitsetbc(state, bndl, bndu);
                // alglib.lsfitsetscale(state, s);

                alglib.lsfitfit(state, function_SigmoidInhibition_3_params, null, fixed_top);
                alglib.lsfitresults(state, out info, out c, out rep);

                Console.WriteLine(info);

                fit_parameters[0] = c[0];
                fit_parameters[1] = fixed_top;
                fit_parameters[2] = c[1];
                fit_parameters[3] = c[2];

                RelativeError = rep.avgrelerror;
                r2 = rep.r2;
                double mse = sum_sqaure_residuals_3_params(drc_points_x_enable, drc_points_y_enable, c, fixed_top);

                if (r2 >= 0.85 && patient == false) confidence_interval = true;
                else confidence_interval = false;

                err_bottom = rep.errpar[0];
                err_ec_50 = rep.errpar[1];
                err_slope = rep.errpar[2];

                if (confidence_interval && display_confidence_interval)
                {
                    double[,] covariance_matrix = rep.covpar;

                    int dof = drc_points_y_enable.Count - 3;

                    double t_test_val = chart.DataManipulator.Statistics.InverseTDistribution(.05, dof);

                    //double sum_square_residuals = sum_sqaure_residuals(drc_points_x_enable, drc_points_y_enable, c);

                    y_conf_int_born_sup.Clear();
                    y_conf_int_born_inf.Clear();
                    x_log_unique.Clear();

                    double amplitude = Math.Abs(Sigmoid_3_params(c, x_fit_log[0], fixed_top) - Sigmoid_3_params(c, x_fit_log[x_fit_log.Count - 1], fixed_top));

                    double min_curve = Math.Min(Sigmoid_3_params(c, x_fit_log[0], fixed_top), Sigmoid_3_params(c, x_fit_log[x_fit_log.Count - 1], fixed_top));
                    double max_curve = Math.Max(Sigmoid_3_params(c, x_fit_log[0], fixed_top), Sigmoid_3_params(c, x_fit_log[x_fit_log.Count - 1], fixed_top));

                    SortedDictionary<double, double> born_sup = new SortedDictionary<double, double>();
                    SortedDictionary<double, double> born_inf = new SortedDictionary<double, double>();

                    for (int i = 0; i < x_fit_log.Count; ++i)
                    {
                        double a = compute_least_square_error_3_params(covariance_matrix, c[0], fixed_top, c[1], c[2], x_fit_log[i]);
                        double sigma_confidence_interval = t_test_val * /*Math.Sqrt(mse/dof) */ Math.Sqrt(a); // * Math.Sqrt(sum_square_residuals / (double)dof);

                        double CI_max = Sigmoid_3_params(c, x_fit_log[i], fixed_top) + sigma_confidence_interval;
                        double CI_min = Sigmoid_3_params(c, x_fit_log[i], fixed_top) - sigma_confidence_interval;

                        if (CI_max > max_curve + 0.4 * amplitude) //|| sigma_confidence_interval > amplitude)
                        {
                            born_sup[x_fit_log[i]] = max_curve + 0.4 * amplitude;
                        }
                        else
                        {
                            born_sup[x_fit_log[i]] = CI_max;
                        }

                        if (CI_min < min_curve - 0.4 * amplitude) //|| sigma_confidence_interval > amplitude)
                        {
                            born_inf[x_fit_log[i]] = min_curve - 0.4 * amplitude;
                        }
                        else
                        {
                            born_inf[x_fit_log[i]] = CI_min;
                        }
                    }

                    foreach (KeyValuePair<double, double> elem in born_sup)
                    {
                        y_conf_int_born_sup.Add(elem.Value);
                    }

                    foreach (KeyValuePair<double, double> elem in born_inf)
                    {
                        y_conf_int_born_inf.Add(elem.Value);
                        x_log_unique.Add(Math.Pow(10, elem.Key));
                    }

                }

                y_fit_log.Clear();

                for (int IdxConc = 0; IdxConc < x_fit.Count; IdxConc++)
                {
                    y_fit_log.Add(Sigmoid_3_params(c, x_fit_log[IdxConc], fixed_top));
                }

            }
            else // top not fixed, fit with 4 params.
            {
                double BaseEC50 = Math.Log10(MaxConcentrationLin) - Math.Abs(Math.Log10(MaxConcentrationLin) - Math.Log10(MinConcentrationLin)) / 2.0;

                double first_slope = (GlobalMax - GlobalMin) / (Math.Log10(MaxConcentrationLin) - Math.Log10(MinConcentrationLin));
                if ((GlobalMax - GlobalMin) > 0)
                {
                    first_slope = -Math.Abs(first_slope);
                }
                else
                {
                    first_slope = +Math.Abs(first_slope);
                }

                double[] c = new double[] { min_bound_y, max_bound_y, BaseEC50, 0 };
                double[] c2 = new double[] { min_bound_y, max_bound_y, BaseEC50, 0 };
                double[] c3 = new double[] { min_bound_y, max_bound_y, BaseEC50, 0 };

                double[] bndl = null;
                double[] bndu = null;


                // boundaries
                bndu = new double[] { max_bound_y, max_bound_y, min_bound_x, +10 * Math.Abs(first_slope) };
                bndl = new double[] { min_bound_y, min_bound_y, max_bound_x, -10 * Math.Abs(first_slope) };

                alglib.lsfitstate state;
                alglib.lsfitreport rep;

                // Fitting without weights
                //alglib.lsfitcreatefg(Concentrations, Values.ToArray(), c, false, out state);

                double[,] Concentration = new double[drc_points_x_enable.Count(), 1];
                double[] conc_0 = new double[drc_points_x_enable.Count()];

                for (var i = 0; i < drc_points_x_enable.Count(); ++i)
                {
                    Concentration[i, 0] = drc_points_x_enable[i];
                    conc_0[i] = drc_points_x_enable[i];
                }

                alglib.lsfitcreatef(Concentration, drc_points_y_enable.ToArray(), c, diffstep, out state);
                alglib.lsfitsetcond(state, epsx, maxits);
                alglib.lsfitsetbc(state, bndl, bndu);
                // alglib.lsfitsetscale(state, s);

                alglib.lsfitfit(state, function_SigmoidInhibition, null, null);
                alglib.lsfitresults(state, out info, out c, out rep);

                fit_parameters = c;
                RelativeError = rep.avgrelerror;
                r2 = rep.r2;
                info = info;

                double[,] data = new double[drc_points_x_enable.Count(), 2];

                for (int i = 0; i < drc_points_x_enable.Count(); ++i)
                {
                    data[i, 0] = drc_points_x_enable[i];
                    data[i, 1] = drc_points_y_enable[i];
                }

                // Method minimization chi2
                /*
                double epsg2 = 1e-30;
                double epsf2 = 0;
                double epsx2 = 0;
                int maxits2 = 0; //10000;

                alglib.minlbfgsstate state2;
                alglib.minlbfgsreport rep2;

                //double diffstep2 = 1.0e-6;
                //double[] s2 = new double[] { 1 / c[0], 1 / c[1], 1 / c[2], 1 / c[3] };

                //alglib.minlbfgscreatef(4, c2, diffstep2, out state2);
                alglib.minlbfgscreate(4, c2, out state2);
                alglib.minlbfgssetcond(state2, epsg2, epsf2, epsx2, maxits2);
                //alglib.mincgsetscale(state2, s2);
                alglib.minlbfgsoptimize(state2, compute_chi_square, null, data);
                alglib.minlbfgsresults(state2, out c2, out rep2);

                int code = rep2.terminationtype;
                //rep2.varidx
                //int code3 = rep3.terminationtype;
                //c = c3;
                //c = c2;
                */
                fit_parameters = c;

                double mse = sum_sqaure_residuals(drc_points_x_enable, drc_points_y_enable, c);

                if (r2 >= 0.85 && patient == false) confidence_interval = true;
                else confidence_interval = false;

                err_bottom = rep.errpar[0];
                err_top = rep.errpar[1];
                err_ec_50 = rep.errpar[2];
                //err_ec_50 = 0.0;
                err_slope = rep.errpar[3];

                if (confidence_interval && display_confidence_interval)
                {
                    /*
                    double[,] hessian = compute_hessian(c, data);

                    alglib.matinvreport rep_mat;
                    int info_mat;

                    alglib.rmatrixinverse(ref hessian, out info_mat, out rep_mat);

                    for (int i = 0; i < 4; i++)
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            hessian[i, j] = 0.5 * hessian[i, j];
                        }
                    }

                    double[,] covariance_matrix2 = hessian;
                    */

                    double[,] covariance_matrix = rep.covpar;

                    //covariance_matrix[3,3] = 0.2;

                    int dof = drc_points_y_enable.Count - 4;

                    double t_test_val = chart.DataManipulator.Statistics.InverseTDistribution(.05, dof);

                    //double sum_square_residuals = sum_sqaure_residuals(drc_points_x_enable, drc_points_y_enable, fit_parameters);

                    y_conf_int_born_sup.Clear();
                    y_conf_int_born_inf.Clear();
                    x_log_unique.Clear();

                    //double amplitude = Math.Abs(Sigmoid(c, x_fit_log[0]) - Sigmoid(c, x_fit_log[x_fit_log.Count - 1]));

                    //double min_curve = Math.Min(Sigmoid(c, x_fit_log[0]), Sigmoid(c, x_fit_log[x_fit_log.Count - 1],));
                    //double max_curve = Math.Max(Sigmoid(c, x_fit_log[0]), Sigmoid(c, x_fit_log[x_fit_log.Count - 1]));

                    SortedDictionary<double, double> born_sup = new SortedDictionary<double, double>();
                    SortedDictionary<double, double> born_inf = new SortedDictionary<double, double>();

                    double min_curve = Math.Min(Sigmoid(c, x_fit_log[0]), Sigmoid(c, x_fit_log[x_fit_log.Count - 1]));
                    double max_curve = Math.Max(Sigmoid(c, x_fit_log[0]), Sigmoid(c, x_fit_log[x_fit_log.Count - 1]));
                    double amplitude = Math.Abs(Sigmoid(c, x_fit_log[0]) - Sigmoid(c, x_fit_log[x_fit_log.Count - 1]));

                    /*
                    for (int i = 0; i < covariance_matrix2.GetLength(0); i++)
                    {
                        for (int j = 0; j < covariance_matrix2.GetLength(1); ++j)
                        {
                            covariance_matrix2[i, j] = (double)(mse/dof) * covariance_matrix2[i, j];
                        }
                    }
                    */

                    double max_c = 0.0;

                    for (int i = 0; i < x_fit_log.Count; ++i)
                    {
                        double a = compute_least_square_error(covariance_matrix, fit_parameters[0], fit_parameters[1], fit_parameters[2], fit_parameters[3], x_fit_log[i]);

                        /*
                        double a3 = compute_least_square_error2(covariance_matrix2, fit_parameters[0], fit_parameters[1], fit_parameters[2], fit_parameters[3], x_fit_log[i]);

                        if (max_c < a3) max_c = a3;

                        double sigma_confidence_interval = t_test_val * Math.Sqrt(mse / dof) * Math.Sqrt(a3); // * Math.Sqrt(sum_square_residuals / (double)dof);
                        */

                        double sigma_confidence_interval = t_test_val * Math.Sqrt(a); // * Math.Sqrt(sum_square_residuals / (double)dof);

                        double CI_max = Sigmoid(c, x_fit_log[i]) + sigma_confidence_interval;
                        double CI_min = Sigmoid(c, x_fit_log[i]) - sigma_confidence_interval;

                        if (CI_max > max_curve + 0.4 * amplitude) //|| sigma_confidence_interval > amplitude)
                        {
                            born_sup[x_fit_log[i]] = max_curve + 0.4 * amplitude;
                        }
                        else
                        {
                            born_sup[x_fit_log[i]] = CI_max;
                        }

                        if (CI_min < min_curve - 0.4 * amplitude) //|| sigma_confidence_interval > amplitude)
                        {
                            born_inf[x_fit_log[i]] = min_curve - 0.4 * amplitude;
                        }
                        else
                        {
                            born_inf[x_fit_log[i]] = CI_min;
                        }
                    }

                    foreach (KeyValuePair<double, double> elem in born_sup)
                    {
                        y_conf_int_born_sup.Add(elem.Value);
                    }

                    foreach (KeyValuePair<double, double> elem in born_inf)
                    {
                        y_conf_int_born_inf.Add(elem.Value);
                        x_log_unique.Add(Math.Pow(10, elem.Key));
                    }

                }

                y_fit_log.Clear();

                for (int IdxConc = 0; IdxConc < x_fit.Count; IdxConc++)
                {
                    y_fit_log.Add(Sigmoid(c, x_fit_log[IdxConc]));
                }
            }
        }

        public void Is_Modified()
        {
            draw_DRC(false, false);
        }

        public void threshold_r2(double thr)
        {
            draw_DRC(false, false);

            //double r2_threshold = double.Parse(_form1.numericUpDown1.Value.ToString());

            not_fitted = not_fitted_init;

            if (r2 < thr)
            {
                not_fitted = true;
                inactive = false;

                ((RectangleAnnotation)chart.Annotations["menu_inactive"]).ForeColor = Color.LightGray;
                ((RectangleAnnotation)chart.Annotations["menu_not_fitted"]).ForeColor = Color.Red;

                //if (inactive_init == true)
                //{
                //    not_fitted = false;
                //    inactive = true;

                //    ((RectangleAnnotation)chart.Annotations["menu_inactive"]).ForeColor = Color.Orange;
                //    ((RectangleAnnotation)chart.Annotations["menu_not_fitted"]).ForeColor = Color.LightGray;
                //}
            }

            Is_Modified();
        }

        public void threshold_inactive(double thr)
        {
            draw_DRC(false, false);

            inactive = inactive_init;

            double min_max_activity = Math.Abs(fit_parameters[1] - fit_parameters[0]);

            if (min_max_activity < thr)
            {
                inactive = true;
                not_fitted = false;

                ((RectangleAnnotation)chart.Annotations["menu_inactive"]).ForeColor = Color.Orange;
                ((RectangleAnnotation)chart.Annotations["menu_not_fitted"]).ForeColor = Color.LightGray;

                //if (not_fitted_init == true)
                //{
                //    inactive = false;
                //    not_fitted = true;

                //    ((RectangleAnnotation)chart.Annotations["menu_inactive"]).ForeColor = Color.LightGray;
                //    ((RectangleAnnotation)chart.Annotations["menu_not_fitted"]).ForeColor = Color.Red;
                //}
            }

            Is_Modified();
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

            //string my_descritpor_name = descriptor;
            //string cpound_id = compound_id;

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

                if (diff_top_last_point <= thr_2_last_points * Math.Abs(top - bottom) && diff_top_last_point2 <= thr_2_last_points * Math.Abs(top - bottom))
                {
                    draw_DRC(false, false);

                    is_ec50_exact = true;
                    ((RectangleAnnotation)chart.Annotations["menu_ec_50_sup"]).Text = "=";

                    //annotation_ec50.Text = "EC_50 = " + Math.Pow(10, fit_parameters[2] - err_ec_50).ToString("E2") + " | " +
                    //                            Math.Pow(10, fit_parameters[2]).ToString("E2") + " | " +
                    //                            Math.Pow(10, fit_parameters[2] + err_ec_50).ToString("E2") + " | R2 = "
                    //                            + r2.ToString("N2");

                    annotation_ec50.Text = "EC_50 = " + Math.Pow(10, fit_parameters[2]).ToString("E2") + " | R2 = " + r2.ToString("N2");

                }
                else
                {
                    draw_DRC(false, false);

                    is_ec50_exact = false;
                    ((RectangleAnnotation)chart.Annotations["menu_ec_50_sup"]).Text = ">";

                    //annotation_ec50.Text = "EC_50 > " + Math.Pow(10, fit_parameters[2] - err_ec_50).ToString("E2") + " | " +
                    //                            Math.Pow(10, fit_parameters[2]).ToString("E2") + " | " +
                    //                            Math.Pow(10, fit_parameters[2] + err_ec_50).ToString("E2") + " | R2 = "
                    //                            + r2.ToString("N2");

                    annotation_ec50.Text = "EC_50 = " + Math.Pow(10, fit_parameters[2]).ToString("E2") + " | R2 = " + r2.ToString("N2");

                }


            }

        }

        public void check_toxicity(double thr_toxicity)
        {
            // Compute the top :
            double curve_fit_value = 0.0;

            //if (fit_parameters[0] < fit_parameters[1])
            //{

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

                curve_fit_value = Sigmoid(fit_parameters, dict_points.Keys.ElementAt(dict_points.Count() - 1));

                if (Math.Abs(response_last_point - curve_fit_value) >= thr_toxicity * min_max_activity)
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

                            int index = 0;

                            List<int> indices = new List<int>();
                            for (int i = 0; i < drc_points_y_enable.Count(); i++)
                                if (drc_points_y_enable[i] < y_val + 1e-6 && drc_points_y_enable[i] > y_val - 1e-6)
                                    indices.Add(i);

                            foreach (int idx in indices)
                            {
                                if (drc_points_x_enable[idx] < (point_x + 1e-12) && drc_points_x_enable[idx] > (point_x - 1e-12))
                                {
                                    index = idx;
                                    break;
                                }
                            }

                            //int index = drc_points_y_enable.FindIndex(a => a < y_val + .0000001 && a > y_val - .0000001);

                            drc_points_x_disable.Add(point_x);
                            drc_points_y_disable.Add(y_val);

                            drc_points_x_enable.RemoveAt(index);
                            drc_points_y_enable.RemoveAt(index);

                            int index_raw_data = 0;

                            List<int> indices_raw = new List<int>();

                            for (int i = 0; i < y_raw_data.Count(); i++)
                                if (y_raw_data[i] < y_val + 1e-6 && y_raw_data[i] > y_val - 1e-6)
                                    indices_raw.Add(i);

                            foreach (int idx in indices_raw)
                            {
                                if (Math.Log10(x_raw_data[idx]) < (point_x + 1e-12) && Math.Log10(x_raw_data[idx]) > (point_x - 1e-12))
                                {
                                    index_raw_data = idx;
                                    break;
                                }
                            }

                            is_raw_data_removed[index_raw_data] = true;

                            //int index_raw_data = y_raw_data.FindIndex(a => a < y_val + .0000001 && a > y_val - .0000001);
                            //is_raw_data_removed[index_raw_data] = true;
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

            chart.Titles["Title1"].Text = descriptor + " CPD = " + compound_id;

            //----------------------------- Axis Labels ---------------------------//

            fit_DRC();

            chart.ChartAreas[0].RecalculateAxesScale();

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

            if (general_params == false)
            {
                if (minX < -0.5)
                {
                    minX = Math.Pow(10, min_x); // initialized at -1. <-0.5 to test if it is the first filled values
                }
                else
                {
                    minX = chart.ChartAreas[0].AxisX.Minimum;
                }

                if (maxX < -0.5)
                {
                    maxX = Math.Pow(10, max_x);
                }
                else
                {
                    maxX = chart.ChartAreas[0].AxisX.Maximum;
                }

                double min_curve = Math.Min(y_fit_log[0], y_fit_log[y_fit_log.Count - 1]);
                double max_curve = Math.Max(y_fit_log[0], y_fit_log[y_fit_log.Count - 1]);
                double amplitude = max_curve - min_curve;

                if (fixed_y_max == false)
                {
                    minY = min_curve - amplitude * 0.5;
                    maxY = max_curve + amplitude * 0.5;
                }

                //maxY = Math.Ceiling((maxY / 10.0) * 10.0);
                //minY = Math.Floor((minY / 10.0) * 10.0);

            }
            else
            {
                //maxY = Math.Ceiling((maxY / 10.0) * 10.0);
                //minY = Math.Floor((minY / 10.0) * 10.0);

                chart.ChartAreas[0].AxisY.Minimum = minY;
                chart.ChartAreas[0].AxisY.Maximum = maxY;
            }

            chart.ChartAreas[0].AxisX.Minimum = minX;
            chart.ChartAreas[0].AxisX.Maximum = maxX;

            chart.ChartAreas[0].AxisX.IsLogarithmic = true;
            chart.ChartAreas[0].AxisX.LogarithmBase = 10;
            chart.ChartAreas[0].AxisX.LabelStyle.Format = "E2";

            //chart.ChartAreas[0].AxisX.ScaleView.Zoomable = false;
            //chart.ChartAreas[0].AxisY.ScaleView.Zoomable = false;

            // End Axis Labels.

            // Draw the first graph

            chart.Series["Series1"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
            chart.Series["Series1"].Points.DataBindXY(x_concentrations, y_response);
            chart.Series["Series1"].Color = chart_color;

            if (dmso)
            {
                for (int i = 0; i < list_wells.Count; ++i)
                {
                    chart.Series["Series1"].Points[i].AxisLabel = list_wells[i].Substring(1, list_wells[i].Length - 1);
                    chart.Series["Series1"].Points[i].Label = list_wells[i];
                }
            }

            if (display_fit)
            {
                chart.Series["Series2"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                chart.Series["Series2"].Points.DataBindXY(x_fit, y_fit_log);
                chart.Series["Series2"].Color = chart_color;

                if (confidence_interval && display_confidence_interval)
                {
                    chart.Series["Born_Inf"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                    chart.Series["Born_Inf"].Points.DataBindXY(x_log_unique, y_conf_int_born_inf);
                    chart.Series["Born_Inf"].Color = Color.FromArgb(50, chart_color);

                    chart.Series["Born_Sup"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                    chart.Series["Born_Sup"].Points.DataBindXY(x_log_unique, y_conf_int_born_sup);
                    chart.Series["Born_Sup"].Color = Color.FromArgb(50, chart_color);
                }
                else
                {
                    chart.Series["Born_Inf"].Points.Clear();
                    chart.Series["Born_Sup"].Points.Clear();
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

            //if (drc_points_x_disable.Count() == 0 && data_modified==false) data_modified = false;
            //else data_modified = true;
            if (drc_points_x_disable.Count() > 0) data_modified = true;

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
                if ((is_top_fixed || manual_bounds) && data_modified)
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
            }

            // Setup visual attributes
            string sign = "";

            if (is_ec50_exact == true) sign = "=";
            else sign = ">";

            //annotation_ec50.BackColor = Color.FromArgb(240, 240, 240);
            annotation_ec50.BackColor = Color.White;
            annotation_ec50.AnchorX = 50;

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

            if ((is_top_fixed || manual_bounds) && data_modified == true && inactive == false && not_fitted == false) // general_params || 
            {
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 1].Style.BackColor = Color.LightSeaGreen;
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 2].Style.BackColor = Color.LightSeaGreen;
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 3].Style.BackColor = Color.LightSeaGreen;
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 4].Style.BackColor = Color.LightSeaGreen;
                _form1.f2.dataGridView2.Rows[row_index].Cells[5 * descriptor_index + 5].Style.BackColor = Color.LightSeaGreen;
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

                RectangleAnnotation menu_CI = new RectangleAnnotation();
                menu_CI.Name = "menu_CI";
                menu_CI.Text = "CI";
                menu_CI.AnchorX = 11.5;
                menu_CI.AnchorY = 5;
                menu_CI.Height = 5;
                menu_CI.Width = 4;
                menu_CI.ForeColor = Color.Green;
                menu_CI.Font = new Font(menu_CI.Font.FontFamily, menu_CI.Font.Size, FontStyle.Bold);
                menu_CI.Visible = true;
                chart.Annotations.Add(menu_CI);
                annotation_ec50.Text = "EC_50 = " + Math.Pow(10, fit_parameters[2]).ToString("E2") + " | R2 = " + r2.ToString("N2");
                if (inactive)
                {
                    ((RectangleAnnotation)chart.Annotations["menu_inactive"]).ForeColor = Color.Orange;
                    ((RectangleAnnotation)chart.Annotations["menu_not_fitted"]).ForeColor = Color.LightGray;
                    annotation_ec50.Text = "Inactive";

                }

                if (not_fitted)
                {
                    ((RectangleAnnotation)chart.Annotations["menu_inactive"]).ForeColor = Color.LightGray;
                    ((RectangleAnnotation)chart.Annotations["menu_not_fitted"]).ForeColor = Color.Red;
                    annotation_ec50.Text = "Not fitted";
                }

                if (confidence_interval && display_confidence_interval)
                {
                    ((RectangleAnnotation)chart.Annotations["menu_CI"]).ForeColor = Color.Green;
                    //annotation_ec50.Text = "EC_50 = " + Math.Pow(10, fit_parameters[2]).ToString("E2") + " | R2 = " + r2.ToString("N2");

                }
                else
                {
                    ((RectangleAnnotation)chart.Annotations["menu_CI"]).ForeColor = Color.LightGray;
                    //annotation_ec50.Text = "EC_50 = " + Math.Pow(10, fit_parameters[2]).ToString("E2") + " | R2 = " + r2.ToString("N2");

                }
            }

            //annotation_ec50.Text = "EC_50 = " + Math.Pow(10, fit_parameters[2] - err_ec_50).ToString("E2") + " | " +
            //                                    Math.Pow(10, fit_parameters[2]).ToString("E2") + " | " +
            //                                    Math.Pow(10, fit_parameters[2] + err_ec_50).ToString("E2") + " | R2 = "
            //                                    + r2.ToString("N2");

            //annotation_ec50.Text = "EC_50 = " + Math.Pow(10, fit_parameters[2]).ToString("E2") + " | R2 = " + r2.ToString("N2");

            if (patient)
            {
                if (if_report) chart.Series["Series1"].Points.Clear(); // /!\ POINTS AUC REMOVED /!\
                draw_area_under_curve(drc_points_x_enable, drc_points_y_enable);
                annotation_ec50.Text = "AUC = " + auc.ToString("N2") + " +/- " + error_auc.ToString("N2");
            }

            if (display_fit == false)
            {
                annotation_ec50.Text = "Mean Value = " + drc_points_y_enable.Average().ToString("N2");
            }

        }

        private void draw_area_under_curve(List<double> drc_points_x_enable, List<double> drc_points_y_enable)
        {

            //Or if series is of Series type, you could:

            if (chart.Series.IndexOf("Serie_AUC") != -1)
            {
                chart.Series.RemoveAt(chart.Series.IndexOf("Serie_AUC"));

                Series serie_line_auc = new Series();
                serie_line_auc.ChartType = SeriesChartType.Line;
                serie_line_auc.Name = "Serie_AUC";
                chart.Series.Add(serie_line_auc);
            }
            else
            {
                Series serie_line_auc = new Series();
                serie_line_auc.ChartType = SeriesChartType.Line;
                serie_line_auc.Name = "Serie_AUC";
                chart.Series.Add(serie_line_auc);
            }

            if (chart.Series.IndexOf("Fill_AUC") != -1)
            {
                chart.Series.RemoveAt(chart.Series.IndexOf("Fill_AUC"));

                Series serie_fill_auc = new Series();
                serie_fill_auc.ChartType = SeriesChartType.Area;
                serie_fill_auc.Name = "Fill_AUC";
                chart.Series.Add(serie_fill_auc);
            }
            else
            {
                Series serie_fill_auc = new Series();
                serie_fill_auc.ChartType = SeriesChartType.Area;
                serie_fill_auc.Name = "Fill_AUC";
                chart.Series.Add(serie_fill_auc);
            }

            if (drc_points_x_enable.Count > 0)
            {
                SortedDictionary<double, List<double>> points_dict = new SortedDictionary<double, List<double>>();

                for (int i = 0; i < drc_points_x_enable.Count; ++i)
                {
                    double point_x = drc_points_x_enable[i];
                    double point_y = drc_points_y_enable[i];

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

                List<double> list_x = new List<double>();
                List<double> mean_y = new List<double>();

                foreach (KeyValuePair<double, List<double>> elem in points_dict)
                {
                    list_x.Add(Math.Pow(10, elem.Key));
                    mean_y.Add(elem.Value.Average());
                }

                chart.Series["Serie_Mean"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
                chart.Series["Serie_Mean"].MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Circle;
                chart.Series["Serie_Mean"].Points.DataBindXY(list_x, mean_y);
                chart.Series["Serie_Mean"].Color = Color.FromArgb(50, chart_color);

                chart.Series["Serie_AUC"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                chart.Series["Serie_AUC"].Points.DataBindXY(list_x, mean_y);
                chart.Series["Serie_AUC"].Color = Color.FromArgb(50, chart_color);

                chart.Series["Fill_AUC"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Area;
                chart.Series["Fill_AUC"].Points.DataBindXY(list_x, mean_y);
                chart.Series["Fill_AUC"].Color = Color.FromArgb(25, chart_color);

            }

            compute_AUC();
        }


        private void chart1_PostPaint(object sender, System.Windows.Forms.DataVisualization.Charting.ChartPaintEventArgs e)
        {
            if (display_post_paint == false) return;

            Chart my_chart = (Chart)sender;
            ChartArea area = my_chart.ChartAreas[0];
            if (area.Name == descriptor)
            {
                Axis ax = chart.ChartAreas[0].AxisX;
                Axis ay = chart.ChartAreas[0].AxisY;

                //double minimum_x = chart.ChartAreas[0].AxisX.Minimum;
                //double minimum_y = chart.ChartAreas[0].AxisY.Minimum;

                // Ec 50 Line :
                //List<double> line_ec_50_point_x = new List<double>();
                //line_ec_50_point_x.Add(Math.Pow(10, fit_parameters[2]));
                //line_ec_50_point_x.Add(Math.Pow(10, fit_parameters[2]));

                //List<double> line_ec_50_point_y = new List<double>();
                ////line_ec_50_point_y.Add(y_fit_log.Min());
                //line_ec_50_point_y.Add(chart.ChartAreas[0].AxisY.Minimum);
                //line_ec_50_point_y.Add(Sigmoid(fit_parameters, fit_parameters[2]));

                //chart.Series["line_ec_50_x"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                //chart.Series["line_ec_50_x"].Points.DataBindXY(line_ec_50_point_x, line_ec_50_point_y);
                //chart.Series["line_ec_50_x"].Color = Color.DimGray;
                //chart.Series["line_ec_50_x"].BorderDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dash;

                Graphics graph = e.ChartGraphics.Graphics;

                PointF point1 = PointF.Empty;
                PointF point2 = PointF.Empty;

                point1.X = (float)ax.ValueToPixelPosition(Math.Pow(10, fit_parameters[2]));
                point1.Y = (float)ay.ValueToPixelPosition(chart.ChartAreas[0].AxisY.Minimum);
                point2.X = (float)ax.ValueToPixelPosition(Math.Pow(10, fit_parameters[2]));
                point2.Y = (float)ay.ValueToPixelPosition(Sigmoid(fit_parameters, fit_parameters[2]));

                float[] dashValues = { 2, 2, 2, 2 };
                Pen blackPen = new Pen(Color.DimGray, 0.25f);
                blackPen.DashPattern = dashValues;

                graph.DrawLine(blackPen, point1, point2);

                //List<double> line_ec_50_point_y_bis = new List<double>();
                //line_ec_50_point_y_bis.Add(Sigmoid(fit_parameters, fit_parameters[2]));
                //line_ec_50_point_y_bis.Add(Sigmoid(fit_parameters, fit_parameters[2]));

                //List<double> line_ec_50_point_x_bis = new List<double>();
                ////line_ec_50_point_x_bis.Add(x_fit[0]);
                //line_ec_50_point_x_bis.Add(chart.ChartAreas[0].AxisX.Minimum);
                //line_ec_50_point_x_bis.Add(Math.Pow(10, fit_parameters[2]));

                //chart.Series["line_ec_50_y"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                //chart.Series["line_ec_50_y"].Points.DataBindXY(line_ec_50_point_x_bis, line_ec_50_point_y_bis);
                //chart.Series["line_ec_50_y"].Color = Color.DimGray;
                //chart.Series["line_ec_50_y"].BorderDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dash;

                PointF point3 = PointF.Empty;
                PointF point4 = PointF.Empty;

                point3.X = (float)ax.ValueToPixelPosition(chart.ChartAreas[0].AxisX.Minimum);
                point3.Y = (float)ay.ValueToPixelPosition(Sigmoid(fit_parameters, fit_parameters[2]));
                point4.X = (float)ax.ValueToPixelPosition(Math.Pow(10, fit_parameters[2]));
                point4.Y = (float)ay.ValueToPixelPosition(Sigmoid(fit_parameters, fit_parameters[2]));

                graph.DrawLine(blackPen, point3, point4);

                //chart.ChartAreas[0].AxisX.Minimum = minimum_x;
                //chart.ChartAreas[0].AxisY.Minimum = minimum_y;
            }

            //if (fixed_y_max)
            //{
            //    chart.ChartAreas[0].AxisY.Maximum = maxY;
            //}
        }

        Point mdown = Point.Empty;
        List<DataPoint> selectedPoints = null;

        private void chart1_MouseDown(object sender, MouseEventArgs e)
        {
            mdown = e.Location;
            selectedPoints = new List<DataPoint>();
        }


        Point? prevPosition = null;
        ToolTip tooltip = new ToolTip();

        private void chart1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                chart.Refresh();
                using (Graphics g = chart.CreateGraphics())
                    g.DrawRectangle(Pens.Red, GetRectangle(mdown, e.Location));
            }


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

                        //int pointXPixel = 0;
                        //if (index_lbls > -1) pointXPixel = (int)chart.ChartAreas[0].AxisX.ValueToPixelPosition(index_lbls + 1);
                        var pointXPixel = result.ChartArea.AxisX.ValueToPixelPosition(prop.XValue);
                        var pointYPixel = result.ChartArea.AxisY.ValueToPixelPosition(prop.YValues[0]);

                        // check if the cursor is really close to the point (10 pixels around the point)
                        if (Math.Abs(pos.X - pointXPixel) < 4 && Math.Abs(pos.Y - pointYPixel) < 4)
                        {
                            double point_x = prop.XValue;
                            double point_y = prop.YValues[0];

                            int index = 0;
                            //bool test = false;

                            List<int> indices = new List<int>();
                            for (int i = 0; i < y_raw_data.Count(); i++)
                                if (y_raw_data[i] < point_y + 1e-6 && y_raw_data[i] > point_y - 1e-6)
                                    indices.Add(i);

                            foreach (int idx in indices)
                            {
                                if (x_raw_data[idx] < (point_x + 1e-12) && x_raw_data[idx] > (point_x - 1e-12))
                                {
                                    index = idx;
                                    break;
                                }
                            }

                            string plate_name = plate_per_point[index];
                            string current_well = well_per_point[index];

                            tooltip.Show("Plate : " + plate_name + " | Well : " + current_well, this.chart, pos.X, pos.Y - 15);

                        }
                    }
                }
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
                                if (drc_points_y_disable[i] < point_y + 1e-6 && drc_points_y_disable[i] > point_y - 1e-6)
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

                            int index_raw_data = 0;

                            List<int> indices_raw = new List<int>();

                            for (int i = 0; i < y_raw_data.Count(); i++)
                                if (y_raw_data[i] < point_y + 1e-6 && y_raw_data[i] > point_y - 1e-6)
                                    indices_raw.Add(i);

                            foreach (int idx in indices_raw)
                            {
                                if (Math.Log10(x_raw_data[idx]) < (point_x + 1e-12) && Math.Log10(x_raw_data[idx]) > (point_x - 1e-12))
                                {
                                    index_raw_data = idx;
                                    break;
                                }
                            }

                            is_raw_data_removed[index_raw_data] = false;

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
                            //bool test = false;

                            List<int> indices = new List<int>();
                            for (int i = 0; i < drc_points_y_enable.Count(); i++)
                                if (drc_points_y_enable[i] < point_y + 1e-6 && drc_points_y_enable[i] > point_y - 1e-6)
                                    indices.Add(i);

                            foreach (int idx in indices)
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
                                if (y_raw_data[i] < point_y + 1e-6 && y_raw_data[i] > point_y - 1e-6)
                                    indices_raw.Add(i);

                            foreach (int idx in indices_raw)
                            {
                                if (Math.Log10(x_raw_data[idx]) < (point_x + 1e-12) && Math.Log10(x_raw_data[idx]) > (point_x - 1e-12))
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

                if (display_fit)
                {
                    chart.Series["Series2"].Points.DataBindXY(x_fit, y_fit_log);

                    if (confidence_interval && display_confidence_interval)
                    {
                        chart.Series["Born_Inf"].Points.DataBindXY(x_log_unique, y_conf_int_born_inf);
                        chart.Series["Born_Inf"].Color = Color.FromArgb(50, chart_color);

                        chart.Series["Born_Sup"].Points.DataBindXY(x_log_unique, y_conf_int_born_sup);
                        chart.Series["Born_Sup"].Color = Color.FromArgb(50, chart_color);

                        ((RectangleAnnotation)chart.Annotations["menu_CI"]).ForeColor = Color.Green;
                    }
                    else
                    {
                        ((RectangleAnnotation)chart.Annotations["menu_CI"]).ForeColor = Color.LightGray;

                        chart.Series["Born_Inf"].Points.Clear();
                        chart.Series["Born_Sup"].Points.Clear();
                    }
                }

                if (patient)
                {
                    draw_area_under_curve(drc_points_x_enable, drc_points_y_enable);
                }

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
                    if (is_top_fixed || manual_bounds)
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
                }

                ((RectangleAnnotation)chart.Annotations["menu_inactive"]).ForeColor = Color.LightGray;
                ((RectangleAnnotation)chart.Annotations["menu_not_fitted"]).ForeColor = Color.LightGray;

                //annotation_ec50.Text = "EC_50 = " + Math.Pow(10, fit_parameters[2] - err_ec_50).ToString("E2") + " | " +
                //                                Math.Pow(10, fit_parameters[2]).ToString("E2") + " | " +
                //                                Math.Pow(10, fit_parameters[2] + err_ec_50).ToString("E2") + " | R2 = "
                //                                + r2.ToString("N2");
                annotation_ec50.Text = "EC_50 = " + Math.Pow(10, fit_parameters[2]).ToString("E2") + " | R2 = " + r2.ToString("N2");

                if (patient) annotation_ec50.Text = "AUC = " + auc.ToString("N2") + " +/- " + error_auc.ToString("N2");
                if (display_fit == false) annotation_ec50.Text = "Mean Value = " + drc_points_y_enable.Average().ToString("N2");
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
            descriptor_name = descriptor_name.Replace(@"\", @"_");

            string compound_id1 = compound_id.Replace(@"/", @"_");
            string compound_id2 = compound_id1.Replace(@"\", @"_");

            string output_image = path + "/CPD_" + compound_id2 + "_" + descriptor_name + ".bmp";

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

                //not_fitted_init = true;
                //inactive_init = false;

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

                //inactive_init = true;
                //not_fitted_init = false;

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
                    minY = chart.ChartAreas[0].AxisY.Minimum;
                    maxX = chart.ChartAreas[0].AxisX.Maximum;
                    maxY = chart.ChartAreas[0].AxisY.Maximum;

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
                        //annotation_ec50.Text = "EC_50 > " + Math.Pow(10, fit_parameters[2] - err_ec_50).ToString("E2") + " | " +
                        //                        Math.Pow(10, fit_parameters[2]).ToString("E2") + " | " +
                        //                        Math.Pow(10, fit_parameters[2] + err_ec_50).ToString("E2") + " | R2 = "
                        //                        + r2.ToString("N2");
                        annotation_ec50.Text = "EC_50 = " + Math.Pow(10, fit_parameters[2]).ToString("E2") + " | R2 = " + r2.ToString("N2");


                    }
                    else
                    {
                        is_ec50_exact = true;
                        ((RectangleAnnotation)chart.Annotations["menu_ec_50_sup"]).Text = "=";

                        //annotation_ec50.Text = "EC_50 = " + Math.Pow(10, fit_parameters[2] - err_ec_50).ToString("E2") + " | " +
                        //                        Math.Pow(10, fit_parameters[2]).ToString("E2") + " | " +
                        //                        Math.Pow(10, fit_parameters[2] + err_ec_50).ToString("E2") + " | R2 = "
                        //                        + r2.ToString("N2");

                        annotation_ec50.Text = "EC_50 = " + Math.Pow(10, fit_parameters[2]).ToString("E2") + " | R2 = " + r2.ToString("N2");

                    }
                }

                if (pointer_x >= 48 && pointer_x < 73 && pointer_y <= 18)
                {

                    ((RectangleAnnotation)chart.Annotations["menu_CI"]).Text = "CI";

                    if (display_confidence_interval == true)
                    {
                        display_confidence_interval = false;
                        ((RectangleAnnotation)chart.Annotations["menu_CI"]).ForeColor = Color.LightGray;

                        //chart.Series["Born_Inf"].Points.Clear();
                        //chart.Series["Born_Sup"].Points.Clear();
                    }
                    else
                    {
                        display_confidence_interval = true;
                        ((RectangleAnnotation)chart.Annotations["menu_CI"]).ForeColor = Color.Green;

                        //chart.Series["Born_Inf"].Points.DataBindXY(x_log_unique, y_conf_int_born_inf);
                        //chart.Series["Born_Inf"].Color = Color.FromArgb(50, chart_color);

                        //chart.Series["Born_Sup"].Points.DataBindXY(x_log_unique, y_conf_int_born_sup);
                        //chart.Series["Born_Sup"].Color = Color.FromArgb(50, chart_color);

                    }

                    draw_DRC(false, false);

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

                        //not_fitted_init = true;
                        //inactive_init = false;

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
                            if (is_top_fixed || manual_bounds)
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
                        }

                        string sign = "";

                        if (is_ec50_exact == true) sign = "=";
                        else sign = ">";

                        //annotation_ec50.Text = "EC_50 " + sign + " " + Math.Pow(10, fit_parameters[2] - err_ec_50).ToString("E2") + " | " +
                        //                        Math.Pow(10, fit_parameters[2]).ToString("E2") + " | " +
                        //                        Math.Pow(10, fit_parameters[2] + err_ec_50).ToString("E2") + " | R2 = "
                        //                        + r2.ToString("N2");
                        annotation_ec50.Text = "EC_50 " + sign + " " + Math.Pow(10, fit_parameters[2]).ToString("E2") + " | R2 = " + r2.ToString("N2");

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

                        //inactive_init = true;
                        //not_fitted_init = false;

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
                            if (is_top_fixed || manual_bounds)
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
                        }

                        string sign = "";

                        if (is_ec50_exact == true) sign = "=";
                        else sign = ">";

                        //annotation_ec50.Text = "EC_50 " + sign + " " + Math.Pow(10, fit_parameters[2] - err_ec_50).ToString("E2") + " | " +
                        //                        Math.Pow(10, fit_parameters[2]).ToString("E2") + " | " +
                        //                        Math.Pow(10, fit_parameters[2] + err_ec_50).ToString("E2") + " | R2 = "
                        //                        + r2.ToString("N2");

                        annotation_ec50.Text = "EC_50 " + sign + " " + Math.Pow(10, fit_parameters[2]).ToString("E2") + " | R2 = " + r2.ToString("N2");

                    }
                }

            }

        }

        public void set_inactive()
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

                //inactive_init = true;
                //not_fitted_init = false;

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
                    if (is_top_fixed || manual_bounds)
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
                }

                string sign = "";

                if (is_ec50_exact == true) sign = "=";
                else sign = ">";

                //annotation_ec50.Text = "EC_50 " + sign + " " + Math.Pow(10, fit_parameters[2] - err_ec_50).ToString("E2") + " | " +
                //                        Math.Pow(10, fit_parameters[2]).ToString("E2") + " | " +
                //                        Math.Pow(10, fit_parameters[2] + err_ec_50).ToString("E2") + " | R2 = "
                //                        + r2.ToString("N2");

                annotation_ec50.Text = "EC_50 " + sign + " " + Math.Pow(10, fit_parameters[2]).ToString("E2") + " | R2 = " + r2.ToString("N2");

            }

        }

        public void change_params(double min_x, double max_x, double min_y, double max_y, Color my_color)
        {
            chart_color = my_color;

            chart.ChartAreas[0].AxisX.Minimum = min_x;
            chart.ChartAreas[0].AxisX.Maximum = max_x;
            chart.ChartAreas[0].AxisY.Minimum = min_y;
            chart.ChartAreas[0].AxisY.Maximum = max_y;

            minX = min_x;
            maxX = max_x;
            minY = min_y;
            maxY = max_y;
        }

        public void remove_outlier_median(double thresold_median, double thr_actvity)
        {

            Dictionary<double, List<double>> points_dict = new Dictionary<double, List<double>>();
            //Dictionary<double, List<double>> residual_dict = new Dictionary<double, List<double>>();

            foreach (DataPoint dp in chart.Series["Series1"].Points)
            {
                double point_x = Math.Log10(dp.XValue);
                double point_y = dp.YValues[0];

                double residual = Math.Abs(point_y - Sigmoid(fit_parameters, point_x));

                if (points_dict.ContainsKey(point_x))
                {
                    points_dict[point_x].Add(point_y);
                    //residual_dict[point_x].Add(residual);
                }
                else
                {
                    List<double> my_list = new List<double>();
                    my_list.Add(point_y);

                    points_dict[point_x] = my_list;

                    //List<double> my_list_residual = new List<double>();
                    //my_list_residual.Add(residual);

                    //residual_dict[point_x] = my_list_residual;
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

                List<double> mad_vector = new List<double>();

                for (int i = 0; i < y_points.Count(); ++i)
                {
                    mad_vector.Add(Math.Abs(y_points[i] - median_value));
                }

                mad_vector.Sort();

                double median_value_mad = 0;
                int count_mad = mad_vector.Count();

                if (count_mad % 2 == 0 && count_mad > 0)
                {
                    // count is even, need to get the middle two elements, add them together, then divide by 2
                    double middleElement1 = mad_vector[(count / 2) - 1];
                    double middleElement2 = mad_vector[(count / 2)];
                    median_value_mad = (middleElement1 + middleElement2) / 2;
                }
                else
                {
                    median_value_mad = mad_vector[(count / 2)];
                }

                for (int i = 0; i < mad_vector.Count(); ++i)
                {
                    mad_vector[i] = Math.Abs(y_points[i] - median_value) / median_value_mad;
                }

                counter++;

                // Enable/Disabel points :
                for (int i = 0; i < y_points.Count(); ++i)
                {

                    bool point_exclusion = false;

                    // test MAD + residual % of activity
                    if (mad_vector[i] > thresold_median && ((y_points[i] - Sigmoid(fit_parameters, x_points)) >= thr_actvity * Math.Abs(fit_parameters[0] - fit_parameters[1]))) point_exclusion = true;

                    double current_y = y_points[i];

                    // Remove Points enabled
                    if (!(drc_points_x_disable.Contains(x_points) && drc_points_y_disable.Contains(current_y)) && point_exclusion)
                    {
                        //int index = drc_points_y_enable.FindIndex(a => a < current_y + .0000001 && a > current_y - .0000001);

                        int index = 0;

                        List<int> indices = new List<int>();
                        for (int j = 0; j < drc_points_y_enable.Count(); j++)
                            if (drc_points_y_enable[j] < current_y + 1e-6 && drc_points_y_enable[j] > current_y - 1e-6)
                                indices.Add(j);

                        foreach (int idx in indices)
                        {
                            if (drc_points_x_enable[idx] < (x_points + 1e-12) && drc_points_x_enable[idx] > (x_points - 1e-12))
                            {
                                index = idx;
                                break;
                            }
                        }

                        drc_points_x_disable.Add(x_points);
                        drc_points_y_disable.Add(current_y);

                        drc_points_x_enable.RemoveAt(index); //Add(data_chart[i].XValue);
                        drc_points_y_enable.RemoveAt(index); //Add(data_chart[i].YValues[0]);

                        //chart.Series["Series1"].Points[point_index].Color = Color.LightGray;

                        int index_raw_data = 0;

                        List<int> indices_raw = new List<int>();

                        for (int j = 0; j < y_raw_data.Count(); j++)
                            if (y_raw_data[j] < current_y + 1e-6 && y_raw_data[j] > current_y - 1e-6)
                                indices_raw.Add(j);

                        foreach (int idx in indices_raw)
                        {
                            if (Math.Log10(x_raw_data[idx]) < (x_points + 1e-12) && Math.Log10(x_raw_data[idx]) > (x_points - 1e-12))
                            {
                                index_raw_data = idx;
                                break;
                            }
                        }

                        is_raw_data_removed[index_raw_data] = true;
                    }
                    else if ((drc_points_x_disable.Contains(x_points) && drc_points_y_disable.Contains(current_y)) && !point_exclusion)
                    {
                        //int index = drc_points_y_disable.FindIndex(a => a < current_y + .0000001 && a > current_y - .0000001);

                        int index = 0;

                        List<int> indices = new List<int>();
                        for (int j = 0; j < drc_points_y_disable.Count(); j++)
                            if (drc_points_y_disable[j] < current_y + 1e-6 && drc_points_y_disable[j] > current_y - 1e-6)
                                indices.Add(j);

                        foreach (int idx in indices)
                        {
                            if (drc_points_x_disable[idx] < (x_points + 1e-12) && drc_points_x_disable[idx] > (x_points - 1e-12))
                            {
                                index = idx;
                                break;
                            }
                        }

                        drc_points_x_enable.Add(x_points);
                        drc_points_y_enable.Add(current_y);

                        drc_points_x_disable.RemoveAt(index); //Add(data_chart[i].XValue);
                        drc_points_y_disable.RemoveAt(index); //Add(data_chart[i].YValues[0]);

                        //chart.Series["Series1"].Points[point_index].Color = Color.LightGray;

                        int index_raw_data = 0;

                        List<int> indices_raw = new List<int>();

                        for (int j = 0; j < y_raw_data.Count(); j++)
                            if (y_raw_data[j] < current_y + 1e-6 && y_raw_data[j] > current_y - 1e-6)
                                indices_raw.Add(j);

                        foreach (int idx in indices_raw)
                        {
                            if (Math.Log10(x_raw_data[idx]) < (x_points + 1e-12) && Math.Log10(x_raw_data[idx]) > (x_points - 1e-12))
                            {
                                index_raw_data = idx;
                                break;
                            }
                        }

                        is_raw_data_removed[index_raw_data] = false;

                        //int index_raw_data = y_raw_data.FindIndex(a => a < current_y + .0000001 && a > current_y - .0000001);
                        //is_raw_data_removed[index_raw_data] = false;

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

            draw_DRC(false, false);

        }

        private double evaluate_DRC_integral(double x)
        {
            double top = fit_parameters[1];
            double bottom = fit_parameters[0];
            double ec_50 = fit_parameters[2];
            double slope = fit_parameters[3];

            double numerator = Math.Sign(bottom * Math.Pow(10, slope * (ec_50 - x)) + top) * (top - bottom) * (Math.Pow(10, slope * (ec_50 - x)) + 1) + slope * top * x * Math.Log(10);
            double denominator = Math.Sign(Math.Pow(10, slope * (ec_50 - x)) + 1) * slope * Math.Log(10);
            double integral_value = numerator / denominator;

            return integral_value;
        }

        //private double eval_sigmoid(double x)
        //{
        //    double top = fit_parameters[1];
        //    double bottom = fit_parameters[0];
        //    double ec_50 = fit_parameters[2];
        //    double slope = fit_parameters[3];

        //    return bottom + ((top - bottom) / (1 + Math.Pow(10, (ec_50 - x) * slope)));
        //}
        //public static void int_function_1_func(double x, double xminusa, double bminusx, ref double y, object obj)
        //{



        //    //double top = c[1];
        //    //double bottom = c[0];
        //    //double ec_50 = c[2];
        //    //double slope = c[3];

        //    //y = bottom + ((top - bottom) / (1 + Math.Pow(10, (ec_50 - x) * slope)));
        //}
        public static List<double> Arange(double start, int count)
        {
            return (List<double>)Enumerable.Range((int)start, count).Select(v => (double)v).ToList();
        }

        public static List<double> Power(List<double> exponents, double baseValue = 10.0d)
        {
            return (List<double>)exponents.Select(v => Math.Pow(baseValue, v)).ToList();
        }

        public static List<double> LinSpace(double start, double stop, int num, bool endpoint = true)
        {
            var result = new List<double>();
            if (num <= 0)
            {
                return result;
            }

            if (endpoint)
            {
                if (num == 1)
                {
                    return new List<double>() { start };
                }

                var step = (stop - start) / ((double)num - 1.0d);
                result = Arange(0, num).Select(v => (v * step) + start).ToList();
            }
            else
            {
                var step = (stop - start) / (double)num;
                result = Arange(0, num).Select(v => (v * step) + start).ToList();
            }

            return result;
        }

        public static List<double> LogSpace(double start, double stop, int num, bool endpoint = true, double numericBase = 10.0d)
        {
            List<double> y = LinSpace(start, stop, num: num, endpoint: endpoint);
            return Power(y, numericBase);
        }

        public double compute_AUC()
        {
            // AUC with fitted curve :
            //double min_x_fit_auc = x_fit_log[0];
            //double max_x_fit_auc = x_fit_log[x_fit_log.Count - 1];

            //double top = fit_parameters[1];
            //double bottom = fit_parameters[0];
            //double ec_50 = fit_parameters[2];
            //double slope = fit_parameters[3];

            //double integral_val = 0.0;

            //Func<double, double> g = (x) => bottom + ((top - bottom) / (1 + Math.Pow(10, (ec_50 - x) * slope)));

            //integral_val = TrapezoidalRule.Integrate(g, min_x_fit_auc, max_x_fit_auc, steps: 1000);

            // AUC with points : (Prism method)
            SortedDictionary<double, List<double>> points_dict = new SortedDictionary<double, List<double>>();

            for (int i = 0; i < drc_points_x_enable.Count; ++i)
            {
                double point_x = drc_points_x_enable[i];
                double point_y = drc_points_y_enable[i];

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

            List<double> list_x = new List<double>();
            List<double> mean_y = new List<double>();

            List<double> variances = new List<double>();
            List<double> derivatives = new List<double>();

            foreach (KeyValuePair<double, List<double>> elem in points_dict)
            {
                list_x.Add(elem.Key);
                mean_y.Add(elem.Value.Average());
            }

            // Compute the variance of each concentration sample :
            foreach (KeyValuePair<double, List<double>> elem in points_dict)
            {
                List<double> y_values = elem.Value;
                double mean_y_value = y_values.Average();
                double current_variance = 0.0;

                for (int i = 0; i < y_values.Count(); ++i)
                {
                    double current_y = y_values[i];
                    current_variance += 100.0 * (current_y - mean_y_value) * 100.0 * (current_y - mean_y_value);
                }

                current_variance /= 2.0;

                variances.Add(current_variance);
            }


            // Compute the derivative of the auc :
            for (int i = 0; i < list_x.Count; ++i)
            {
                if (i == 0) derivatives.Add((list_x[1] - list_x[0]) / 2.0);
                else if (i == list_x.Count - 1) derivatives.Add((list_x[list_x.Count - 1] - list_x[list_x.Count - 2]) / 2.0);
                else
                {
                    derivatives.Add((list_x[i] - list_x[i - 1]) / 2.0 + (list_x[i + 1] - list_x[i]) / 2.0);
                }
            }

            double variance_auc = 0.0;

            for (int i = 0; i < list_x.Count; ++i)
            {
                variance_auc += derivatives[i] * derivatives[i] * variances[i];
            }

            error_auc = 1.96 * Math.Sqrt(variance_auc);

            //Console.WriteLine(compound_id.ToString());
            //for (int i = 0; i < variances.Count; ++i) Console.WriteLine(variances[i].ToString());
            //for (int i = 0; i < derivatives.Count; ++i) Console.WriteLine(derivatives[i].ToString());
            //Console.WriteLine(error_auc.ToString());

            double top = fit_parameters[1];
            double bottom = fit_parameters[0];
            double ec_50 = fit_parameters[2];
            double slope = fit_parameters[3];



            //Func<double, double> g = (x) => bottom + ((top - bottom) / (1 + Math.Pow(10, (ec_50 - x) * slope)));


            double area_geom = 0.0;
            List<double> totot = LinSpace(list_x[0], list_x[list_x.Count - 1], 50);


            for (int i = 0; i < totot.Count - 1; ++i)
            {
                double delta_x = Math.Abs(totot[i + 1] - totot[i]);
                double y0 = bottom + ((top - bottom) / (1 + Math.Pow(10, (ec_50 - totot[i + 1]) * slope)));
                double y1 = bottom + ((top - bottom) / (1 + Math.Pow(10, (ec_50 - totot[i]) * slope)));

                double y_mean = (y0 + y1) / 2.0;// (mean_y[i] + mean_y[i + 1]) / 2.0;

                double trapezoid_area = y_mean * delta_x;

                area_geom += trapezoid_area;
            }

            auc = 100.0 * area_geom;

            return 100.0 * area_geom;
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

        private Dictionary<string, List<double>> x_fit = new Dictionary<string, List<double>>();
        private Dictionary<string, List<double>> x_fit_log = new Dictionary<string, List<double>>();
        private Dictionary<string, List<double>> y_fit = new Dictionary<string, List<double>>();
        private Dictionary<string, List<double>> x_fit_points = new Dictionary<string, List<double>>();

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

        private int min_x = +20;
        private int max_x = -20;

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

        //private void chart1_PostPaint(object sender, System.Windows.Forms.DataVisualization.Charting.ChartPaintEventArgs e)
        //{

        //    Chart my_chart = (Chart)sender;
        //    ChartArea area = my_chart.ChartAreas[0];
        //    if (area.Name == descriptor)
        //    {
        //        Axis ax = chart.ChartAreas[0].AxisX;
        //        Axis ay = chart.ChartAreas[0].AxisY;

        //        Graphics graph = e.ChartGraphics.Graphics;

        //        PointF point1 = PointF.Empty;
        //        PointF point2 = PointF.Empty;

        //        point1.X = (float)ax.ValueToPixelPosition(Math.Pow(10, fit_parameters[2]));
        //        point1.Y = (float)ay.ValueToPixelPosition(chart.ChartAreas[0].AxisY.Minimum);
        //        point2.X = (float)ax.ValueToPixelPosition(Math.Pow(10, fit_parameters[2]));
        //        point2.Y = (float)ay.ValueToPixelPosition(Sigmoid(fit_parameters, fit_parameters[2]));

        //        float[] dashValues = { 2, 2, 2, 2 };
        //        Pen blackPen = new Pen(Color.DimGray, 0.25f);
        //        blackPen.DashPattern = dashValues;

        //        graph.DrawLine(blackPen, point1, point2);

        //        PointF point3 = PointF.Empty;
        //        PointF point4 = PointF.Empty;

        //        point3.X = (float)ax.ValueToPixelPosition(chart.ChartAreas[0].AxisX.Minimum);
        //        point3.Y = (float)ay.ValueToPixelPosition(Sigmoid(fit_parameters, fit_parameters[2]));
        //        point4.X = (float)ax.ValueToPixelPosition(Math.Pow(10, fit_parameters[2]));
        //        point4.Y = (float)ay.ValueToPixelPosition(Sigmoid(fit_parameters, fit_parameters[2]));

        //        graph.DrawLine(blackPen, point3, point4);

        //    }

        //}

        public Chart_DRC_Time_Line() { }

        public Chart_DRC_Time_Line(string cpd, string descript, int step, ref List<double> x, ref List<double> x_log, ref List<double> y, Color color, MainTab form, string filename)
        {

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

            double minx = MinA(x.ToArray());
            double maxx = MaxA(x.ToArray());

            min_y = MinA(y.ToArray());
            max_y = MaxA(y.ToArray());

            MinConcentrationLin = minx;
            MaxConcentrationLin = maxx;

            x_fit[file_name] = new List<double>();
            x_fit_log[file_name] = new List<double>();
            y_fit[file_name] = new List<double>();

            for (int j = 0; j < step_curve; j++)
            {
                x_fit[file_name].Add(MinConcentrationLin + j * (MaxConcentrationLin - MinConcentrationLin) / (double)step_curve);
                x_fit_log[file_name].Add(Math.Log10(MinConcentrationLin) + j * (Math.Log10(MaxConcentrationLin) - Math.Log10(MinConcentrationLin)) / (double)step_curve);
            }

            chart = new Chart();

            ChartArea chartArea = new ChartArea();
            chartArea.Name = "ChartArea";

            Series series1 = new Series();
            Series series2 = new Series();

            Axis yAxis = new Axis(chartArea, AxisName.Y);
            Axis xAxis = new Axis(chartArea, AxisName.X);

            chartArea.AxisX.LabelStyle.Format = "N2";
            chartArea.AxisX.Title = "Concentration";
            chartArea.AxisY.Title = "Response";

            chartArea.AxisX.MajorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dash;
            chartArea.AxisY.MajorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dash;

            chartArea.AxisX.MajorGrid.LineColor = Color.LightGray;
            chartArea.AxisY.MajorGrid.LineColor = Color.LightGray;

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

            chart.Legends.Add(new Legend("Legend"));

            chart.Legends["Legend"].Position.Auto = false;
            chart.Legends["Legend"].Position = new ElementPosition(18, 15, 25, 4);

            series2.Legend = "Legend";
            series2.LegendText = filename;
            series2.IsVisibleInLegend = true;
            series1.IsVisibleInLegend = false;

            //chart.PostPaint += new EventHandler<ChartPaintEventArgs>(this.chart1_PostPaint);

            if (drc_points_x_log[file_name].Count > 0)
            {
                int min = (int)Math.Floor(MinA<double>(drc_points_x_log[file_name].ToArray()));
                int max = (int)Math.Ceiling(MaxA<double>(drc_points_x_log[file_name].ToArray()));

                if (min < min_x) min_x = min;
                if (max > max_x) max_x = max;
            }
            else
            {
                max_x = -5;
                min_x = -0;
            }

            double Minx = Math.Pow(10, min_x);
            double Maxx = Math.Pow(10, max_x);

            chart.ChartAreas[0].AxisX.Minimum = Minx;
            chart.ChartAreas[0].AxisX.Maximum = Maxx;

            chart.ChartAreas[0].AxisX.IsLogarithmic = true;
            chart.ChartAreas[0].AxisX.LogarithmBase = 10;
            chart.ChartAreas[0].AxisX.LabelStyle.Format = "E2";

            fit_DRC(file_name);
        }

        public void add_serie_points(string file, ref List<double> x, ref List<double> x_log, ref List<double> y, Color color)
        {
            filenames.Add(file);

            series_number += 1;

            drc_points_x[file] = x;
            drc_points_x_log[file] = x_log;

            drc_points_y[file] = y;
            chart_colors[file] = color;

            if (drc_points_x_log[file_name].Count > 0)
            {
                int min = (int)Math.Floor(MinA<double>(drc_points_x_log[file].ToArray()));
                int max = (int)Math.Ceiling(MaxA<double>(drc_points_x_log[file].ToArray()));

                if (min < min_x) min_x = min;
                if (max > max_x) max_x = max;
            }
            else
            {
                max_x = -5;
                min_x = -8;
            }

            double min_x_pow10 = Math.Pow(10, min_x);
            double max_x_pow10 = Math.Pow(10, max_x);

            chart.ChartAreas[0].AxisX.Minimum = min_x_pow10;
            chart.ChartAreas[0].AxisX.Maximum = max_x_pow10;

            chart.ChartAreas[0].AxisX.IsLogarithmic = true;
            chart.ChartAreas[0].AxisX.LogarithmBase = 10;
            chart.ChartAreas[0].AxisX.LabelStyle.Format = "E2";

            double minx = MinA(drc_points_x[file].ToArray());
            double maxx = MaxA(drc_points_x[file].ToArray());

            x_fit[file] = new List<double>();
            x_fit_log[file] = new List<double>();

            for (int j = 0; j < step_curve; j++)
            {
                x_fit[file].Add(minx + j * (maxx - minx) / (double)step_curve);
                x_fit_log[file].Add(Math.Log10(minx) + j * (Math.Log10(maxx) - Math.Log10(minx)) / (double)step_curve);
            }

            Series series_new_points = new Series();

            series_new_points.ChartType = SeriesChartType.Point;
            series_new_points.MarkerStyle = MarkerStyle.Circle;
            series_new_points.Name = file;

            chart.Series.Add(series_new_points);

            Series series_new_curve = new Series();

            series_new_curve.ChartType = SeriesChartType.Line;
            series_new_curve.Name = file + "_curve";

            chart.Series.Add(series_new_curve);

            series_new_curve.Legend = "Legend";
            series_new_curve.LegendText = file;
            series_new_curve.IsVisibleInLegend = true;
            series_new_points.IsVisibleInLegend = false;


            fit_DRC(file);

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

                y_fit.Remove(file);
                x_fit.Remove(file);
                x_fit_log.Remove(file);

                chart.Series.Remove(chart.Series[file]);
                chart.Series.Remove(chart.Series[file + "_curve"]);
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

        private void fit_DRC(string filename)
        {
            double GlobalMax = double.MinValue;
            double MaxValues = MaxA(drc_points_y[filename].ToArray());

            GlobalMax = MaxValues + 0.05 * Math.Abs(MaxValues);

            double GlobalMin = double.MaxValue;
            double MinValues = MinA(drc_points_y[filename].ToArray());

            GlobalMin = MinValues - 0.05 * Math.Abs(MinValues);

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
            bndu = new double[] { GlobalMax, GlobalMax, Math.Log10(MaxConcentrationLin) + 1.0, +1000 };
            bndl = new double[] { GlobalMin, GlobalMin, Math.Log10(MinConcentrationLin) - 1.0, -1000 };

            alglib.lsfitstate state;
            alglib.lsfitreport rep;
            double diffstep = 1e-12;

            // Fitting without weights
            //alglib.lsfitcreatefg(Concentrations, Values.ToArray(), c, false, out state);

            double[,] Concentration = new double[drc_points_x_log[filename].Count(), 1];
            for (var i = 0; i < drc_points_x_log[filename].Count(); ++i)
            {
                Concentration[i, 0] = drc_points_x_log[filename][i];
            }

            int NumDimension = 1;
            alglib.lsfitcreatef(Concentration, drc_points_y[filename].ToArray(), c, diffstep, out state);
            alglib.lsfitsetcond(state, epsx, maxits);
            alglib.lsfitsetbc(state, bndl, bndu);
            // alglib.lsfitsetscale(state, s);

            alglib.lsfitfit(state, function_SigmoidInhibition, null, null);
            alglib.lsfitresults(state, out info, out c, out rep);

            fit_parameters = c;
            RelativeError = rep.avgrelerror;
            r2 = rep.r2;

            if (y_fit.ContainsKey(filename)) y_fit[filename].Clear();
            if (x_fit_points.ContainsKey(filename)) x_fit_points[filename].Clear();

            y_fit[filename] = new List<double>();

            for (int IdxConc = 0; IdxConc < x_fit_log[filename].Count; IdxConc++)
            {
                y_fit[filename].Add(Sigmoid(c, x_fit_log[filename][IdxConc]));

                if (x_fit_points.ContainsKey(filename))
                {
                    x_fit_points[filename].Add(Math.Pow(10, x_fit_log[filename][IdxConc]));
                }
                else
                {
                    List<double> temp = new List<double>();
                    temp.Add(Math.Pow(10, x_fit_log[filename][IdxConc]));
                    x_fit_points[filename] = temp;
                }
            }

        }

        public void draw_DRC()
        {
            string cpd = compound_id;

            fit_DRC(file_name);

            //chart.ChartAreas[0].RecalculateAxesScale();

            chart.Titles["Title1"].Text = descriptor + " CPD=" + compound_id;

            // Draw the first graph
            chart.Series["DRC_Points"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
            chart.Series["DRC_Points"].Points.DataBindXY(drc_points_x[file_name], drc_points_y[file_name]);
            chart.Series["DRC_Points"].Color = chart_colors[file_name];

            chart.Series["DRC_Fit"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chart.Series["DRC_Fit"].Points.DataBindXY(x_fit_points[file_name], y_fit[file_name]);
            chart.Series["DRC_Fit"].Color = chart_colors[file_name];

            // Draw the other graph
            //int counter_color = 0;

            foreach (KeyValuePair<string, List<double>> elem in drc_points_x)
            {
                if (elem.Key != file_name)
                {
                    fit_DRC(elem.Key);

                    chart.Series[elem.Key].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
                    chart.Series[elem.Key].Points.DataBindXY(drc_points_x[elem.Key], drc_points_y[elem.Key]);

                    chart.Series[elem.Key + "_curve"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                    chart.Series[elem.Key + "_curve"].Points.DataBindXY(x_fit_points[elem.Key], y_fit[elem.Key]);

                    //if (counter_color + 1 >= curve_color.Count())
                    //{
                    chart.Series[elem.Key].Color = chart_colors[elem.Key];
                    chart.Series[elem.Key + "_curve"].Color = chart_colors[elem.Key];

                    //}
                    //else
                    //{
                    //    chart.Series[elem.Key].Color = curve_color[counter_color + 1];
                    //    chart.Series[elem.Key + "_curve"].Color = curve_color[counter_color + 1];
                    //}

                    //counter_color++;
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
            descriptor_name = descriptor_name.Replace(@"\", @"_");
            string compound_id1 = compound_id.Replace(@"/", @"_");
            string compound_id2 = compound_id1.Replace(@"\", @"_");

            string output_image = path + "/CPD_" + compound_id2 + "_" + descriptor_name + ".bmp";

            System.Diagnostics.Debug.WriteLine("Write Image = " + output_image);
            chart.SaveImage(output_image, ChartImageFormat.Bmp);

            return output_image;
        }
    }
}