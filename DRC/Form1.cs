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
using Accord.IO;
//using Accord.MachineLearning.DecisionTrees;
//using Accord.MachineLearning.DecisionTrees.Learning;
using Accord.MachineLearning.Clustering;
using Accord.Math;
using Accord.Statistics.Analysis;
using Accord.Statistics.Models.Regression.Linear;
using System.Reflection;

namespace DRC
{

    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
        }

        public Form2 f2;
        public Form6 f6;
        public Form10 f10;
        public void SetForm()
        {
            f2 = new Form2(this);
            f6 = new Form6(this);
            f10 = new DRC.Form10(this);
        }

        public Form3 f3 = new Form3();
        public Form4 f4 = new Form4();
        public Form5 f5 = new Form5();
        public Form7 f7 = new Form7();

        private string current_cpd_id;
        private Dictionary<string, int> cpd_row_index = new Dictionary<string, int>();
        private List<string> list_cpd;
        private int output_parameter_number;
        private int descritpor_number;
        private List<string> descriptor_list;

        private List<string> deslected_data_descriptor;

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
        int aplkwz = 0;
        private bool is_with_plate;
        //private bool is_with_exp;

        private Random rnd = new Random();

        List<List<string>> CPD_ID_List = new List<List<string>>();
        //List<List<int>> Exp_ID_List = new List<List<int>>();

        public double get_descriptors_number()
        {
            return (double)descriptor_list.Count;
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

            if (!f3.dataGridView1.Columns.Contains("CPD_ID"))
            {
                System.Windows.Forms.MessageBox.Show("CPD_ID column doesn't exist.");
                return;
            }

            foreach (DataGridViewRow row in f3.dataGridView1.Rows)
            {
                if (f3.dataGridView1.Columns.Contains("Plate") && is_with_plate == true)
                {
                    CPD_ID.Add(row.Cells["CPD_ID"].Value.ToString() + "__" + row.Cells["Plate"].Value.ToString());
                }
                else CPD_ID.Add(row.Cells["CPD_ID"].Value.ToString());
            }

            var unique_items = new HashSet<string>(CPD_ID);
            comboBox1.DataSource = unique_items.ToList<string>();

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

            deslected_data_descriptor = new List<string>();
            deslected_data_descriptor.Clear();

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

            if (CPD == "DMSO")
                return;

            tableLayoutPanel1.Controls.Clear();

            //int test_modified = 0;
           
            List<Chart_DRC> list_chart = descriptors_chart[current_cpd_id];
            foreach (Chart_DRC current_chart in list_chart)
            {
                current_chart.draw_DRC();
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
            string CPD = f2.dataGridView2.Rows[e.RowIndex].Cells[0].Value.ToString();
            comboBox1.Text = CPD;

            if (CPD == "DMSO")
                return;

            tableLayoutPanel1.Controls.Clear();

            List<Chart_DRC> list_chart = descriptors_chart[CPD];

            //tableLayoutPanel1.Controls.Clear();

            //int test_modified = 0;

            foreach (Chart_DRC current_chart in list_chart)
            {
                current_chart.draw_DRC();
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

            exportDataToolStripMenuItem_Click(sender, e);

            if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string path = folderBrowserDialog1.SelectedPath;

                //DataGridView dataGridViewExport = new DataGridView();
                //this.Controls.Add(dataGridViewExport);

                f5.Text = this.Text;

                f5.dataGridViewExport.Rows.Clear();

                f5.dataGridViewExport.ColumnCount = 1 + descriptor_list.Count;

                f5.dataGridViewExport.Columns[0].Name = "CPD_ID";

                int i = 0;
                foreach (string elem in descriptor_list)
                {

                    DataGridViewImageColumn img = new DataGridViewImageColumn();
                    f5.dataGridViewExport.Columns.Insert(2 * i + 1, img);

                    i++;
                }

                i = 0;
                foreach (string elem in descriptor_list)
                {
                    f5.dataGridViewExport.Columns[2 * i + 1].Name = elem;
                    f5.dataGridViewExport.Columns[2 * i + 2].Name = "EC_50 " + elem;
                    i++;
                }

                for (var idx = 0; idx < list_cpd.Count; idx++)
                {
                    string cpd_id = list_cpd[idx].ToString();

                    if (cpd_id == "DMSO")
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

                        double current_ec_50 = fit_params[2];

                        Image image = Image.FromFile(list_images[i_img]);

                        //f5.dataGridViewExport.Rows[index].Height = 
                        f5.dataGridViewExport.Rows[index].Cells[i_img * 2 + 1].Value = image;
                        if (!not_fitted)
                        {
                            f5.dataGridViewExport.Rows[index].Cells[i_img * 2 + 2].Value = Math.Pow(10, current_ec_50);
                            f5.dataGridViewExport.Rows[index].Cells[i_img * 2 + 2].Style.BackColor = Color.LightGreen;
                        }
                        else if (not_fitted)
                        {
                            f5.dataGridViewExport.Rows[index].Cells[i_img * 2 + 2].Value = "Not Fitted";
                            f5.dataGridViewExport.Rows[index].Cells[i_img * 2 + 2].Style.BackColor = Color.Tomato;
                        }
                        else if (inactive)
                        {
                            f5.dataGridViewExport.Rows[index].Cells[i_img * 2 + 2].Value = "Inactive";
                            f5.dataGridViewExport.Rows[index].Cells[i_img * 2 + 2].Style.BackColor = Color.Orange;
                        }

                        i_img++;

                    }

                }

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
                string cpd_id = list_cpd[idx].ToString();

                if (cpd_id.Contains("DMSO"))
                    continue;

                // Add chart

                List<double> concentrations = new List<double>();
                List<double> concentrations_log = new List<double>();

                List<DataGridViewRow> raw_data_rows = new List<DataGridViewRow>();

                data_descriptor.Clear();
                deselected_data_descriptor.Clear();

                foreach (DataGridViewRow row in f3.dataGridView1.Rows)
                {
                    string cpd_string = "";

                    if (is_with_plate == true) cpd_string = row.Cells["CPD_ID"].Value.ToString() + "__" + row.Cells["Plate"].Value.ToString();
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

                        concentrations.Add(double.Parse(row.Cells["Concentration"].Value.ToString()));
                        concentrations_log.Add(Math.Log10(double.Parse(row.Cells["Concentration"].Value.ToString())));
                    }
                }

                // Loop descriptors :

                List<Chart_DRC> list_DRC_cpd = new List<Chart_DRC>();
                List<double> row_params = new List<double>();

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
                    Color color = Color.Blue;

                    if (descriptor_name == "Nuclei") color = Color.Blue;
                    if (descriptor_name == "R/N" || descriptor_name == "R") color = Color.Red;
                    if (descriptor_name == "G/N" || descriptor_name == "G") color = Color.Green;
                    if (descriptor_name == "LDA_1") color = Color.Black;

                    List<string> deselected = new List<string>();
                    if (deselected_data_descriptor.ContainsKey(descriptor_name)) deselected = deselected_data_descriptor[descriptor_name];

                    Chart_DRC chart_drc = new Chart_DRC(cpd_id, descriptor_name, 100, ref concentrations, ref concentrations_log, ref data, color, descriptor_index, deselected, this);
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

                int col_already_present = 0;
                int new_columns = 0;

                for (int descriptor_index = 0; descriptor_index < descritpor_number; descriptor_index++)
                {
                    string column_name = "Deselected_" + descriptor_list[descriptor_index];

                    if (f3.dataGridView1.Columns.Contains(column_name))
                    {
                        foreach (DataGridViewRow myRow in f3.dataGridView1.Rows)
                        {
                            myRow.Cells[column_name].Value = null;
                        }

                        col_already_present++;
                        f3.dataGridView1.Columns.Remove(f3.dataGridView1.Columns[column_name]);
                        //dataGridView4.ColumnCount -= 1;
                    }
                    else
                    {
                        dataGridView4.ColumnCount += 1;
                        dataGridView4.Columns[col_index].Name = column_name;
                        new_columns++;
                        col_index++;
                    }

                    int content = dataGridView4.ColumnCount;
                }

                for (var idx = 0; idx < list_cpd.Count; idx++)
                {
                    string cpd_id = list_cpd[idx].ToString();

                    if (cpd_id == "DMSO")
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
                            if(not_fitted) newCell.Value = "Not Fitted";
                            if(inactive) newCell.Value = "Inactive";

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
                        System.Diagnostics.Debug.WriteLine("Index, Write = " + i.ToString() + "-->" + dataGridView4.Rows[i - 1].Cells[j].Value.ToString() + ",");
                        if (j < columnCount - 1) output[i] += dataGridView4.Rows[i - 1].Cells[j].Value.ToString() + ",";
                        if (j == columnCount - 1) output[i] += dataGridView4.Rows[i - 1].Cells[j].Value.ToString();
                    }
                }
                System.IO.File.WriteAllLines(saveFileDialog1.FileName, output, System.Text.Encoding.UTF8);
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

            if (CPD == "DMSO")
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

                if (!f3.dataGridView1.Columns.Contains("CPD_ID"))
                {
                    System.Windows.Forms.MessageBox.Show("CPD_ID column doesn't exist.");
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

                if (cpd_id.Contains("DMSO"))
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
                        Color color = Color.Blue;

                        if (descriptor_name == "Nuclei") color = Color.Blue;
                        if (descriptor_name == "R/N" || descriptor_name == "R") color = Color.Red;
                        if (descriptor_name == "G/N" || descriptor_name == "G") color = Color.Green;
                        if (descriptor_name == "LDA_1") color = Color.Black;

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

                if (cpd_id == "DMSO")
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

                if (cpd_id == "DMSO")
                    continue;

                List<Chart_DRC> list_chart = descriptors_chart[cpd_id];

                foreach (Chart_DRC current_chart in list_chart)
                {
                    current_chart.threshold_inactive(inactive_threshold);
                    current_chart.Is_Modified();
                }
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            draw_drc();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }

    public class Chart_DRC_Overlap
    {
        Form1 _form1 = new Form1();

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

            foreach (DataGridViewRow item in raw_data)
            {
                y_raw_data.Add(double.Parse(item.Cells[descriptor].Value.ToString()));
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

        public Chart_DRC_Overlap(string cpd, string descript, int step, ref List<double> x_1, ref List<double> x_log_1, ref List<double> x_2, ref List<double> x_log_2, ref List<double> y_1, ref List<double> y_2, Color color, int index, Form1 form)
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

            if (max_y < 1.0) chartArea.AxisY.Maximum = 1.0;

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
            double MaxValues = max_y_1;
            GlobalMax = MaxValues;

            double GlobalMin = double.MaxValue;
            double MinValues = min_y_1;
            GlobalMin = MinValues;

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
            double MaxValues = max_y_2;
            GlobalMax = MaxValues;

            double GlobalMin = double.MaxValue;
            double MinValues = min_y_2;
            GlobalMin = MinValues;

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

            double ratio = 100.0 / (Math.Ceiling(_form1.get_descriptors_number() / 2.0));
            _form1.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, (float)ratio));

            _form1.tableLayoutPanel1.Controls.Add(chart);
            chart.Anchor = (AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top);
            //chart_already_loaded = true;
        }

        public string save_image(string path)
        {
            draw_DRC();
            string descriptor_name = descriptor.Replace(@"/", @"_");
            string output_image = path + "/CPD_" + compound_id + "_" + descriptor_name + ".png";

            System.Diagnostics.Debug.WriteLine("Write Image = " + output_image);
            chart.SaveImage(output_image, ChartImageFormat.Png);

            return output_image;
        }
    }


    public class Chart_DRC
    {
        Form1 _form1 = new Form1();

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

        private bool not_fitted_init;
        private bool inactive_init;

        List<DataGridViewRow> raw_data;
        List<double> y_raw_data;

        List<bool> is_raw_data_removed;

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

            foreach (DataGridViewRow item in raw_data)
            {
                y_raw_data.Add(double.Parse(item.Cells[descriptor].Value.ToString()));
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

        public Chart_DRC(string cpd, string descript, int step, ref List<double> x, ref List<double> x_log, ref List<double> resp, Color color, int index, List<string> deselected, Form1 form)
        {
            _form1 = form;

            descriptor_index = index;

            compound_id = cpd;
            descriptor = descript;
            step_curve = step;
            chart_color = color;

            not_fitted = false;
            data_modified = false;

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
                if (deselected[index_deselect] == "True")
                {

                    drc_points_x_disable.Add(x_concentrations_log[index_deselect]);
                    drc_points_y_disable.Add(y_response[index_deselect]);

                    is_raw_data_removed[index_deselect] = true;

                    double point_y = y_response[index_deselect];

                    int remove_index = drc_points_y_enable.FindIndex(a => a < point_y + .00001 && a > point_y - .00001);


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

            double max_y = MaxA(y_response.ToArray());

            if (max_y < 1.0) chartArea.AxisY.Maximum = 1.0;

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

            //Create a rectangle annotation

            RectangleAnnotation annotationRectangle = new RectangleAnnotation();
            annotation_ec50 = annotationRectangle;

            //chart.ChartAreas[0].AxisX.Minimum = -10;
            //chart.ChartAreas[0].AxisX.Maximum = -5;

            //chart.ChartAreas[0].AxisY.Minimum = -1;
            //chart.ChartAreas[0].AxisY.Maximum = +1;

            //draw_DRC();

            fit_DRC();

            //chart_already_loaded = true;
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
            double MaxValues = MaxA(drc_points_y_enable.ToArray());
            GlobalMax = MaxValues;

            double GlobalMin = double.MaxValue;
            double MinValues = MinA(drc_points_y_enable.ToArray());
            GlobalMin = MinValues;

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
            int k = 0;
            foreach (DataGridViewRow row2 in _form1.f2.dataGridView2.Rows)
            {
                string compound = row2.Cells[0].Value.ToString();
                if (compound_id == compound) break;
                k++;
            }
            int row_index = k;

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

        }

        public void threshold_r2(double thr)
        {
            //double r2_threshold = double.Parse(_form1.numericUpDown1.Value.ToString());

            not_fitted = not_fitted_init;

            if (r2 < thr)
            {
                not_fitted = true;
                if (inactive_init == true) not_fitted = false;
            }


            //Is_Modified();
        }

        public void threshold_inactive(double thr)
        {
            inactive = inactive_init;

            double GlobalMax = double.MinValue;
            double MaxValues = MaxA(drc_points_y_enable.ToArray());
            GlobalMax = MaxValues;

            double GlobalMin = double.MaxValue;
            double MinValues = MinA(drc_points_y_enable.ToArray());
            GlobalMin = MinValues;

            double min_max_activity = Math.Abs(GlobalMax-GlobalMin);

            if (min_max_activity < thr)
            {
                inactive = true;
                if (not_fitted_init == true) inactive = false;
            }

            //Is_Modified();
        }

        public void draw_DRC()
        {
            string cpd = compound_id;

            fit_DRC();

            chart.Titles["Title1"].Text = descriptor + " CPD=" + compound_id;

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

            chart.ChartAreas[0].AxisX.Minimum = Math.Pow(10, min_x);
            chart.ChartAreas[0].AxisX.Maximum = Math.Pow(10, max_x);

            chart.ChartAreas[0].AxisX.IsLogarithmic = true;
            chart.ChartAreas[0].AxisX.LogarithmBase = 10;
            chart.ChartAreas[0].AxisX.LabelStyle.Format = "E2";

            // End Axis Labels.

            foreach (DataPoint dp in chart.Series["Series1"].Points)
            {
                double point_x = dp.XValue;
                double point_y = dp.YValues[0];

                if (drc_points_x_disable.Contains(point_x) && drc_points_y_disable.Contains(point_y))
                {
                    dp.Color = Color.LightGray;
                    //continue;
                }
                // Remove Points enabled
                if (!(drc_points_x_disable.Contains(point_x) && drc_points_y_disable.Contains(point_y)))
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
            annotation_ec50.AnchorY = 25;
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

            double ratio = 100.0 / (Math.Ceiling(_form1.get_descriptors_number() / 2.0));
            _form1.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, (float)ratio));

            _form1.tableLayoutPanel1.Controls.Add(chart);
            chart.Anchor = (AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top);
            //chart_already_loaded = true;
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
            if (e.Button == MouseButtons.Left)
            {

                Axis ax = chart.ChartAreas[0].AxisX;
                Axis ay = chart.ChartAreas[0].AxisY;
                Rectangle rect = GetRectangle(mdown, e.Location);

                foreach (DataPoint dp in chart.Series["Series1"].Points)
                {
                    int x = (int)ax.ValueToPixelPosition(dp.XValue);
                    int y = (int)ay.ValueToPixelPosition(dp.YValues[0]);

                    double point_x = dp.XValue;
                    double point_y = dp.YValues[0];

                    if (rect.Contains(new Point(x, y)))
                    {

                        if (drc_points_x_disable.Contains(point_x) && drc_points_y_disable.Contains(point_y))
                        {
                            // Add points enabled

                            drc_points_x_enable.Add(point_x);
                            drc_points_y_enable.Add(point_y);

                            int index = drc_points_y_disable.FindIndex(a => a < point_y + .0000001 && a > point_y - .0000001);

                            drc_points_x_disable.RemoveAt(index);
                            drc_points_y_disable.RemoveAt(index);

                            dp.Color = chart_color;
                            continue;
                        }
                        // Remove Points enabled
                        if (!(drc_points_x_disable.Contains(point_x) && drc_points_y_disable.Contains(point_y)))
                        {
                            drc_points_x_disable.Add(point_x);
                            drc_points_y_disable.Add(point_y);

                            int index = drc_points_y_enable.FindIndex(a => a < point_y + .0000001 && a > point_y - .0000001);

                            drc_points_x_enable.RemoveAt(index); //Add(data_chart[i].XValue);
                            drc_points_y_enable.RemoveAt(index); //Add(data_chart[i].YValues[0]);
                            dp.Color = Color.LightGray;

                            int index_raw_data = y_raw_data.FindIndex(a => a < point_y + .0000001 && a > point_y - .0000001);
                            is_raw_data_removed[index_raw_data] = true;
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
            draw_DRC();
            string descriptor_name = descriptor.Replace(@"/", @"_");
            string output_image = path + "/CPD_" + compound_id + "_" + descriptor_name + ".png";

            System.Diagnostics.Debug.WriteLine("Write Image = " + output_image);
            chart.SaveImage(output_image, ChartImageFormat.Png);

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

            }

            if (e.Button == MouseButtons.Middle)
            {
                ColorDialog dlg = new ColorDialog();
                dlg.ShowDialog();

                Color new_color = dlg.Color;

                chart_color = new_color;

                foreach (DataPoint dp in chart.Series["Series1"].Points)
                {
                    dp.Color = new_color;
                }

                chart.Series["Series2"].Color = new_color;

            }

        }

    }

}
