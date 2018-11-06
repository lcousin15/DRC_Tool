using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using OfficeOpenXml.Table;
using System.Diagnostics;

namespace DRC
{
    public partial class AUC_Report : Form
    {
        public AUC_Report(MainTab main_tab)
        {
            InitializeComponent();

            _main_tab = main_tab;
        }

        MainTab _main_tab;

        private static Image LoadImageNoLock(string path)
        {
            var stream = new MemoryStream(File.ReadAllBytes(path));
            return Image.FromStream(stream);
        }

        public void auc_report()
        {

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Excel Documents (*.xlsx)|*.xlsx";
            sfd.FileName = "AUC_Report.xlsx";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                string filename = sfd.FileName;

                string path = filename.Substring(0, filename.LastIndexOf('\\')).Substring(0, filename.LastIndexOf('\\'));

                List<string> img_paths = new List<string>();
                List<string> chart_title = new List<string>();

                Dictionary<string, Chart_Patient> auc_by_descriptor = _main_tab.get_charts_auc();
                Dictionary<string, Chart_Patient> auc_z_score_by_descriptor = _main_tab.get_charts_auc_z_score();


                if (auc_by_descriptor.Count > 0)
                {
                    foreach (KeyValuePair<string, Chart_Patient> elem in auc_by_descriptor)
                    {
                        Chart_Patient current_chart = elem.Value;
                        string img_path = current_chart.save_image(path);
                        img_paths.Add(img_path);

                        chart_title.Add(current_chart.get_descriptor() + "AUC ");
                    }
                }
                else
                {
                    _main_tab.compute_auc("auc");

                    foreach (KeyValuePair<string, Chart_Patient> elem in auc_by_descriptor)
                    {
                        Chart_Patient current_chart = elem.Value;
                        string img_path = current_chart.save_image(path);
                        img_paths.Add(img_path);

                        chart_title.Add(current_chart.get_descriptor() + "AUC ");
                    }
                }

                if (auc_z_score_by_descriptor.Count > 0)
                {
                    foreach (KeyValuePair<string, Chart_Patient> elem in auc_z_score_by_descriptor)
                    {
                        Chart_Patient current_chart = elem.Value;
                        string img_path = current_chart.save_image(path);
                        img_paths.Add(img_path);

                        chart_title.Add(current_chart.get_descriptor() + "AUC (Z-Score");
                    }
                }
                else
                {
                    _main_tab.compute_auc("z-score");

                    foreach (KeyValuePair<string, Chart_Patient> elem in auc_z_score_by_descriptor)
                    {
                        Chart_Patient current_chart = elem.Value;
                        string img_path = current_chart.save_image(path);
                        img_paths.Add(img_path);

                        chart_title.Add(current_chart.get_descriptor() + "AUC (Z-Score");
                    }
                }

                toolStripProgressBar1.Visible = true;

                ExcelPackage pck = new ExcelPackage();
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add("AUC_Graph");
                ExcelWorksheet ws_table = pck.Workbook.Worksheets.Add("AUC_Table");

                List<string> list_cpds = new List<string>();

                Dictionary<string,double> cpd_auc = auc_by_descriptor[auc_by_descriptor.First().Key].get_auc_values();
                foreach(KeyValuePair<string, double> elem in cpd_auc)
                {
                    list_cpds.Add(elem.Key);
                }

                int counter = 0;

                // Compute the number of rows/cols to merge to insert the AUC chart :

                int image_width = 1100;
                int image_height = 500;

                Graphics g = this.CreateGraphics();

                double height = (double)image_height / g.DpiY * 72.0f; //  g.DpiY
                double width = (double)image_width / g.DpiX * 72.0f / 5.1f; // image_width; g.DpiX

                double height_excel = 0.0;
                double width_excel = 0.0;

                int row_nb_auc_graph = 0;
                int col_nb_auc_graph = 0;

                for (int i = 1; i <= 1000; ++i)
                {
                    height_excel += ws.Row(i).Height;

                    if (height_excel >= height)
                    {
                        row_nb_auc_graph = i;
                        break;
                    }
                }

                for (int i = 1; i <= 1000; ++i)
                {
                    width_excel += ws.Column(i).Width;

                    if (width_excel >= width)
                    {
                        col_nb_auc_graph = i;
                        break;
                    }
                }

                int img_rows = row_nb_auc_graph;
                int img_cols = col_nb_auc_graph;

                // Insert images Graphs AUC :

                foreach (string current_path in img_paths)
                {
                    ws.Cells[counter * img_rows + 3, 1, (counter + 1) * img_rows + 1, img_cols].Merge = true;

                    Bitmap img = (Bitmap)LoadImageNoLock(current_path);

                    string name_idx = counter.ToString();

                    ExcelPicture excelImage = null;

                    excelImage = ws.Drawings.AddPicture(name_idx, img);
                    excelImage.From.Column = 1;
                    excelImage.From.Row = counter * img_rows + 2;
                    excelImage.SetSize(1100, 500);

                    counter++;
                }


                // Table DRC :

                Graphics g2 = this.CreateGraphics();

                int drc_width = 485;
                int drc_height = 350;

                double height_excel_drc = (double)drc_height / g2.DpiY * 72.0f; //  g.DpiY
                double width_excel_drc = (double)drc_width / g2.DpiX * 72.0f / 5.1f; // image_width; g.DpiX

                // Columns resize :
                int index_cpd = 0;
                for (int i = 0; i < list_cpds.Count; ++i)
                {
                    if (list_cpds[i] != "DMSO" && list_cpds[i] != "Untreated")
                    {
                        index_cpd = i;
                        break;
                    }
                }

                int max_col_nb = 1 + 5 * _main_tab.get_descriptors_chart()[list_cpds[index_cpd]].Count;

                for (int j = 1; j <= max_col_nb; j++)
                {
                    if (j == 1) ws_table.Column(j).Width = 35;
                    else
                    {
                        if (j % 5 == 2) ws_table.Column(j).Width = width_excel_drc;
                        if ((j - 1) % 5 == 2 || (j - 1) % 5 == 3 || (j - 1) % 5 == 4) ws_table.Column(j).Width = 18;
                        if ((j - 1) % 5 == 0) ws_table.Column(j).Width = 20;
                    }
                }

                int max_row_nb = list_cpds.Count;

                for (int i = 1; i <= max_row_nb+1; i++)
                {
                    if (i == 1) ws_table.Row(i).Height = 15;
                    else ws_table.Row(i).Height = height_excel_drc;
                }

                int cellRowIndex = 1;

                ws_table.Cells[cellRowIndex, 1].Value = "BATCH_ID";

                ws_table.Cells[cellRowIndex, 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws_table.Cells[cellRowIndex, 1].Style.Fill.BackgroundColor.SetColor(Color.Gray);
                ws_table.Cells[cellRowIndex, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                ws_table.Cells[cellRowIndex, 1].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                ws_table.Cells[cellRowIndex, 1].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Medium);
                ws_table.Cells[cellRowIndex, 1].Style.Font.Color.SetColor(Color.White);
                ws_table.Cells[cellRowIndex, 1].Style.Font.Bold = true;

                for (int k = 0; k < img_paths.Count/2; ++k)
                {
                    ws_table.Cells[cellRowIndex, 5 * k + 2].Value = "DRC  Curve";
                    ws_table.Cells[cellRowIndex, 5 * k + 3].Value = "AUC";
                    ws_table.Cells[cellRowIndex, 5 * k + 4].Value = "AUC Error";
                    ws_table.Cells[cellRowIndex, 5 * k + 5].Value = "AUC (Z-Score)";
                    ws_table.Cells[cellRowIndex, 5 * k + 6].Value = "AUC Error (Z-Score)";

                    ws_table.Cells[cellRowIndex, 5 * k + 2].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    ws_table.Cells[cellRowIndex, 5 * k + 2].Style.Fill.BackgroundColor.SetColor(Color.Gray);
                    ws_table.Cells[cellRowIndex, 5 * k + 2].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    ws_table.Cells[cellRowIndex, 5 * k + 2].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    ws_table.Cells[cellRowIndex, 5 * k + 2].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Medium);
                    ws_table.Cells[cellRowIndex, 5 * k + 2].Style.Font.Color.SetColor(Color.White);
                    ws_table.Cells[cellRowIndex, 5 * k + 2].Style.Font.Bold = true;

                    ws_table.Cells[cellRowIndex, 5 * k + 3].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    ws_table.Cells[cellRowIndex, 5 * k + 3].Style.Fill.BackgroundColor.SetColor(Color.Gray);
                    ws_table.Cells[cellRowIndex, 5 * k + 3].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    ws_table.Cells[cellRowIndex, 5 * k + 3].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    ws_table.Cells[cellRowIndex, 5 * k + 3].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Medium);
                    ws_table.Cells[cellRowIndex, 5 * k + 3].Style.Font.Color.SetColor(Color.White);
                    ws_table.Cells[cellRowIndex, 5 * k + 3].Style.Font.Bold = true;

                    ws_table.Cells[cellRowIndex, 5 * k + 4].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    ws_table.Cells[cellRowIndex, 5 * k + 4].Style.Fill.BackgroundColor.SetColor(Color.Gray);
                    ws_table.Cells[cellRowIndex, 5 * k + 4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    ws_table.Cells[cellRowIndex, 5 * k + 4].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    ws_table.Cells[cellRowIndex, 5 * k + 4].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Medium);
                    ws_table.Cells[cellRowIndex, 5 * k + 4].Style.Font.Color.SetColor(Color.White);
                    ws_table.Cells[cellRowIndex, 5 * k + 4].Style.Font.Bold = true;

                    ws_table.Cells[cellRowIndex, 5 * k + 5].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    ws_table.Cells[cellRowIndex, 5 * k + 5].Style.Fill.BackgroundColor.SetColor(Color.Gray);
                    ws_table.Cells[cellRowIndex, 5 * k + 5].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    ws_table.Cells[cellRowIndex, 5 * k + 5].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    ws_table.Cells[cellRowIndex, 5 * k + 5].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Medium);
                    ws_table.Cells[cellRowIndex, 5 * k + 5].Style.Font.Color.SetColor(Color.White);
                    ws_table.Cells[cellRowIndex, 5 * k + 5].Style.Font.Bold = true;

                    ws_table.Cells[cellRowIndex, 5 * k + 6].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    ws_table.Cells[cellRowIndex, 5 * k + 6].Style.Fill.BackgroundColor.SetColor(Color.Gray);
                    ws_table.Cells[cellRowIndex, 5 * k + 6].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    ws_table.Cells[cellRowIndex, 5 * k + 6].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    ws_table.Cells[cellRowIndex, 5 * k + 6].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Medium);
                    ws_table.Cells[cellRowIndex, 5 * k + 6].Style.Font.Color.SetColor(Color.White);
                    ws_table.Cells[cellRowIndex, 5 * k + 6].Style.Font.Bold = true;
                }

                cellRowIndex++;

                toolStripProgressBar1.Visible = true;

                for (int idx = 0; idx < list_cpds.Count; idx++)
                {
                    toolStripProgressBar1.Value = idx * 100 / (list_cpds.Count - 1);
                    //toolStripStatusLabel1.Text = toolStripProgressBar1.Value.ToString();
                    //toolStripStatusLabel1.Visible=true;
                    string BATCH_ID = list_cpds[idx].ToString();

                    if (BATCH_ID.Contains("DMSO") || BATCH_ID.Contains("Untreated"))
                        continue;

                    _main_tab.tableLayoutPanel1.Controls.Clear();

                    Dictionary<string, List<Chart_DRC>> cpd_charts = _main_tab.get_descriptors_chart();

                    List<Chart_DRC> list_chart = cpd_charts[BATCH_ID];

                    List<string> list_images = new List<string>();

                    foreach (Chart_DRC current_chart in list_chart)
                    {
                        current_chart.draw_DRC(false, false);

                        if (current_chart.get_window_y_max() < 1.5)
                        {
                            current_chart.set_general_params(true);
                            current_chart.set_data_modified(true);
                            current_chart.set_window_y_min(0.0);
                            current_chart.set_window_y_max(1.5);
                        }

                        string image_path = current_chart.save_image(path);
                        list_images.Add(image_path);
                    }

                    int i_img = 0;

                    foreach (Chart_DRC current_chart in list_chart)
                    {
                        ws_table.Cells[cellRowIndex, 1].Value = BATCH_ID;

                        ws_table.Cells[cellRowIndex, 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        ws_table.Cells[cellRowIndex, 1].Style.Fill.BackgroundColor.SetColor(Color.Gray);
                        ws_table.Cells[cellRowIndex, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        ws_table.Cells[cellRowIndex, 1].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                        ws_table.Cells[cellRowIndex, 1].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Medium);
                        ws_table.Cells[cellRowIndex, 1].Style.Font.Color.SetColor(Color.White);
                        ws_table.Cells[cellRowIndex, 1].Style.Font.Bold = true;


                        Bitmap img = (Bitmap)LoadImageNoLock(list_images[i_img]);
                        string name_idx = "DRC_Curve" + "_" + BATCH_ID + "_" + i_img.ToString();
                        ExcelPicture excelImage = null;

                        excelImage = ws_table.Drawings.AddPicture(name_idx, img);
                        excelImage.From.Column = 5 * i_img + 1;
                        excelImage.From.Row = cellRowIndex-1;
                        excelImage.SetSize(485, 350);

                        ws_table.Cells[cellRowIndex, 5 * i_img + 3].Value = auc_by_descriptor[current_chart.get_Descriptor_Name()].get_auc_values()[BATCH_ID];
                        ws_table.Cells[cellRowIndex, 5 * i_img + 4].Value = auc_by_descriptor[current_chart.get_Descriptor_Name()].get_auc_error_values()[BATCH_ID];
                        ws_table.Cells[cellRowIndex, 5 * i_img + 5].Value = auc_z_score_by_descriptor[current_chart.get_Descriptor_Name()].get_auc_values()[BATCH_ID];
                        ws_table.Cells[cellRowIndex, 5 * i_img + 6].Value = auc_z_score_by_descriptor[current_chart.get_Descriptor_Name()].get_auc_error_values()[BATCH_ID];

                        ws_table.Cells[cellRowIndex, 5 * i_img + 2].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        ws_table.Cells[cellRowIndex, 5 * i_img + 2].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                        ws_table.Cells[cellRowIndex, 5 * i_img + 2].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        ws_table.Cells[cellRowIndex, 5 * i_img + 2].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                        ws_table.Cells[cellRowIndex, 5 * i_img + 2].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Hair);

                        ws_table.Cells[cellRowIndex, 5 * i_img + 3].Style.Numberformat.Format = "0.00";
                        ws_table.Cells[cellRowIndex, 5 * i_img + 3].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        ws_table.Cells[cellRowIndex, 5 * i_img + 3].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                        ws_table.Cells[cellRowIndex, 5 * i_img + 3].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        ws_table.Cells[cellRowIndex, 5 * i_img + 3].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                        ws_table.Cells[cellRowIndex, 5 * i_img + 3].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Hair);

                        ws_table.Cells[cellRowIndex, 5 * i_img + 4].Style.Numberformat.Format = "0.00";
                        ws_table.Cells[cellRowIndex, 5 * i_img + 4].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        ws_table.Cells[cellRowIndex, 5 * i_img + 4].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                        ws_table.Cells[cellRowIndex, 5 * i_img + 4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        ws_table.Cells[cellRowIndex, 5 * i_img + 4].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                        ws_table.Cells[cellRowIndex, 5 * i_img + 4].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Hair);

                        ws_table.Cells[cellRowIndex, 5 * i_img + 5].Style.Numberformat.Format = "0.00";
                        ws_table.Cells[cellRowIndex, 5 * i_img + 5].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        ws_table.Cells[cellRowIndex, 5 * i_img + 5].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                        ws_table.Cells[cellRowIndex, 5 * i_img + 5].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        ws_table.Cells[cellRowIndex, 5 * i_img + 5].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                        ws_table.Cells[cellRowIndex, 5 * i_img + 5].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Hair);

                        ws_table.Cells[cellRowIndex, 5 * i_img + 6].Style.Numberformat.Format = "0.00";
                        ws_table.Cells[cellRowIndex, 5 * i_img + 6].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        ws_table.Cells[cellRowIndex, 5 * i_img + 6].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                        ws_table.Cells[cellRowIndex, 5 * i_img + 6].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        ws_table.Cells[cellRowIndex, 5 * i_img + 6].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                        ws_table.Cells[cellRowIndex, 5 * i_img + 6].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Hair);

                        i_img++;
                    }

                    //cellColumnIndex = 1;
                    cellRowIndex++;

                    foreach (string current_img in list_images) File.Delete(current_img);
                }

                toolStripProgressBar1.Visible = false;
                pck.SaveAs(new FileInfo(@"" + sfd.FileName));

                pck.Dispose();

                foreach (string current_path in img_paths) File.Delete(current_path);

                MessageBox.Show("Export Successful");
            }
        }
    }
}
