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

            if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string path = folderBrowserDialog1.SelectedPath;

                dataGridView1.Rows.Clear();

                dataGridView1.ColumnCount = 1;
                dataGridView1.Columns[0].HeaderText = "AUC Graphs";
                dataGridView1.Columns[0].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;


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

                toolStripProgressBar1.Visible = true;

                int counter = 0;
                foreach (string current_path in img_paths)
                {
                    toolStripProgressBar1.Value = counter * 100 / (img_paths.Count - 1);

                    int index = dataGridView1.Rows.Add();
                    dataGridView1.Rows[index].Cells[0].Value = chart_title[counter];
                    dataGridView1.Rows[index].Cells[0].Style.BackColor = System.Drawing.Color.LightGray;

                    Image image = LoadImageNoLock(current_path);

                    DataGridViewImageCell img = new DataGridViewImageCell();

                    index = dataGridView1.Rows.Add();
                    dataGridView1.Rows[index].Cells[0] = img;
                    dataGridView1.Rows[index].Cells[0].Value = image;

                    counter++;
                }

                //dataGridView1.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dataGridView1.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

                toolStripProgressBar1.Visible = false;
                this.Show();
                MessageBox.Show("Images generated.");

                //foreach (string current_path in img_paths) File.Delete(current_path);
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
