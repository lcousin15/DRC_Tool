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
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add("AUC_Report");

                List<string> list_cpds = new List<string>();

                Dictionary<string,double> cpd_auc = auc_by_descriptor[auc_by_descriptor.First().Key].get_auc_values();
                foreach(KeyValuePair<string, double> elem in cpd_auc)
                {
                    list_cpds.Add(elem.Key);
                }

                // DRC Graph : excel size :
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

                int max_col_nb = 1 + 3 * _main_tab.get_descriptors_chart()[list_cpds[index_cpd]].Count;

                for (int j = 1; j <= max_col_nb; j++)
                {
                    if (j == 1) ws.Column(j).Width = 35;
                    else
                    {
                        if (j % 3 == 2) ws.Column(j).Width = width_excel_drc;
                        if ((j-1) % 3 == 2) ws.Column(j).Width = 15;
                        if ((j-1) % 3 == 0) ws.Column(j).Width = 20;
                    }
                    //else ws.Column(j).Width = 15;
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

                int img_rows = row_nb_auc_graph+1;
                int img_cols = col_nb_auc_graph+1;

                int rowBeginIndex = img_paths.Count * (1 + img_rows) + 1;

                int max_row_nb = list_cpds.Count + rowBeginIndex;

                for (int i = rowBeginIndex; i < max_row_nb; i++)
                {
                    if (i == 0) ws.Row(i).Height = 15;
                    else ws.Row(i).Height = height_excel_drc;
                }

                // Insert images AUC :

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

                //int cellRowIndex = (counter + 1) * img_rows + 2;
                int cellRowIndex = rowBeginIndex-1;
                //int cellColumnIndex = 1;


                ws.Cells[cellRowIndex, 1].Value = "CPD_ID";

                ws.Cells[cellRowIndex, 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws.Cells[cellRowIndex, 1].Style.Fill.BackgroundColor.SetColor(Color.Gray);
                ws.Cells[cellRowIndex, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                ws.Cells[cellRowIndex, 1].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                ws.Cells[cellRowIndex, 1].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Medium);
                ws.Cells[cellRowIndex, 1].Style.Font.Color.SetColor(Color.White);
                ws.Cells[cellRowIndex, 1].Style.Font.Bold = true;

                for (int k = 0; k < img_paths.Count/2; ++k)
                {
                    ws.Cells[cellRowIndex, 3 * k + 2].Value = "DRC  Curve";
                    ws.Cells[cellRowIndex, 3 * k + 3].Value = "AUC";
                    ws.Cells[cellRowIndex, 3 * k + 4].Value = "AUC (Z-Score)";

                    ws.Cells[cellRowIndex, 3 * k + 2].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    ws.Cells[cellRowIndex, 3 * k + 2].Style.Fill.BackgroundColor.SetColor(Color.Gray);
                    ws.Cells[cellRowIndex, 3 * k + 2].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    ws.Cells[cellRowIndex, 3 * k + 2].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    ws.Cells[cellRowIndex, 3 * k + 2].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Medium);
                    ws.Cells[cellRowIndex, 3 * k + 2].Style.Font.Color.SetColor(Color.White);
                    ws.Cells[cellRowIndex, 3 * k + 2].Style.Font.Bold = true;

                    ws.Cells[cellRowIndex, 3 * k + 3].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    ws.Cells[cellRowIndex, 3 * k + 3].Style.Fill.BackgroundColor.SetColor(Color.Gray);
                    ws.Cells[cellRowIndex, 3 * k + 3].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    ws.Cells[cellRowIndex, 3 * k + 3].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    ws.Cells[cellRowIndex, 3 * k + 3].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Medium);
                    ws.Cells[cellRowIndex, 3 * k + 3].Style.Font.Color.SetColor(Color.White);
                    ws.Cells[cellRowIndex, 3 * k + 3].Style.Font.Bold = true;

                    ws.Cells[cellRowIndex, 3 * k + 4].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    ws.Cells[cellRowIndex, 3 * k + 4].Style.Fill.BackgroundColor.SetColor(Color.Gray);
                    ws.Cells[cellRowIndex, 3 * k + 4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    ws.Cells[cellRowIndex, 3 * k + 4].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    ws.Cells[cellRowIndex, 3 * k + 4].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Medium);
                    ws.Cells[cellRowIndex, 3 * k + 4].Style.Font.Color.SetColor(Color.White);
                    ws.Cells[cellRowIndex, 3 * k + 4].Style.Font.Bold = true;
                }

                cellRowIndex++;

                toolStripProgressBar1.Visible = true;

                for (int idx = 0; idx < list_cpds.Count; idx++)
                {
                    toolStripProgressBar1.Value = idx * 100 / (list_cpds.Count - 1);
                    //toolStripStatusLabel1.Text = toolStripProgressBar1.Value.ToString();
                    //toolStripStatusLabel1.Visible=true;
                    string cpd_id = list_cpds[idx].ToString();

                    if (cpd_id.Contains("DMSO") || cpd_id.Contains("Untreated"))
                        continue;

                    _main_tab.tableLayoutPanel1.Controls.Clear();

                    Dictionary<string, List<Chart_DRC>> cpd_charts = _main_tab.get_descriptors_chart();

                    List<Chart_DRC> list_chart = cpd_charts[cpd_id];

                    List<string> list_images = new List<string>();

                    foreach (Chart_DRC current_chart in list_chart)
                    {
                        string image_path = current_chart.save_image(path);
                        list_images.Add(image_path);
                    }

                    int i_img = 0;

                    foreach (Chart_DRC current_chart in list_chart)
                    {
                        ws.Cells[cellRowIndex, 1].Value = cpd_id;

                        ws.Cells[cellRowIndex, 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        ws.Cells[cellRowIndex, 1].Style.Fill.BackgroundColor.SetColor(Color.Gray);
                        ws.Cells[cellRowIndex, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        ws.Cells[cellRowIndex, 1].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                        ws.Cells[cellRowIndex, 1].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Medium);
                        ws.Cells[cellRowIndex, 1].Style.Font.Color.SetColor(Color.White);
                        ws.Cells[cellRowIndex, 1].Style.Font.Bold = true;


                        Bitmap img = (Bitmap)LoadImageNoLock(list_images[i_img]);
                        string name_idx = "DRC_Curve" + "_" + cpd_id + "_" + i_img.ToString();
                        ExcelPicture excelImage = null;

                        excelImage = ws.Drawings.AddPicture(name_idx, img);
                        excelImage.From.Column = 3 * i_img + 1;
                        excelImage.From.Row = cellRowIndex-1;
                        excelImage.SetSize(485, 350);

                        ws.Cells[cellRowIndex, 3 * i_img + 3].Value = auc_by_descriptor[current_chart.get_Descriptor_Name()].get_auc_values()[cpd_id];
                        ws.Cells[cellRowIndex, 3 * i_img + 4].Value = auc_z_score_by_descriptor[current_chart.get_Descriptor_Name()].get_auc_values()[cpd_id];

                        ws.Cells[cellRowIndex, 3 * i_img + 2].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        ws.Cells[cellRowIndex, 3 * i_img + 2].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                        ws.Cells[cellRowIndex, 3 * i_img + 2].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        ws.Cells[cellRowIndex, 3 * i_img + 2].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                        ws.Cells[cellRowIndex, 3 * i_img + 2].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Hair);
                        
                        ws.Cells[cellRowIndex, 3 * i_img + 3].Style.Numberformat.Format = "0.00";
                        ws.Cells[cellRowIndex, 3 * i_img + 3].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        ws.Cells[cellRowIndex, 3 * i_img + 3].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                        ws.Cells[cellRowIndex, 3 * i_img + 3].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        ws.Cells[cellRowIndex, 3 * i_img + 3].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                        ws.Cells[cellRowIndex, 3 * i_img + 3].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Hair);

                        ws.Cells[cellRowIndex, 3 * i_img + 4].Style.Numberformat.Format = "0.00";
                        ws.Cells[cellRowIndex, 3 * i_img + 4].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        ws.Cells[cellRowIndex, 3 * i_img + 4].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                        ws.Cells[cellRowIndex, 3 * i_img + 4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        ws.Cells[cellRowIndex, 3 * i_img + 4].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                        ws.Cells[cellRowIndex, 3 * i_img + 4].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Hair);


                        i_img++;
                    }

                    //cellColumnIndex = 1;
                    cellRowIndex++;

                    foreach (string current_img in list_images) File.Delete(current_img);
                }

                /*
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
                */

                toolStripProgressBar1.Visible = false;
                pck.SaveAs(new FileInfo(@"" + sfd.FileName));

                pck.Dispose();

                foreach (string current_path in img_paths) File.Delete(current_path);

                MessageBox.Show("Export Successful");


            }

        }

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
