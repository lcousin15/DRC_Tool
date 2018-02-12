using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;
using System.IO;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace DRC
{
    public partial class Form5 : Form
    {
        public Form5()
        {
            InitializeComponent();
        }

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
            imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            return ms.ToArray();
        }

        private void saveToExcelToolStripMenuItem_Click(object sender, EventArgs e)
        {

            // Creating a Excel object. 
            Microsoft.Office.Interop.Excel._Application excel = new Microsoft.Office.Interop.Excel.Application();
            Microsoft.Office.Interop.Excel._Workbook workbook = excel.Workbooks.Add(Type.Missing);
            Microsoft.Office.Interop.Excel._Worksheet worksheet = null;

            try
            {

                //Getting the location and file name of the excel to save from user. 
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "Excel Documents (*.xlsx)|*.xlsx";
                saveDialog.FileName = "DRC_Report.xlsx";
                saveDialog.FilterIndex = 2;

                if (saveDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    worksheet = workbook.ActiveSheet;

                    worksheet.Name = "ExportedFromDatGrid";

                    int cellRowIndex = 1;
                    int cellColumnIndex = 1;

                    int image_width = 0;
                    int image_height = 0;

                    //Loop through each row and read value from each column. 
                    for (int i = 0; i < dataGridViewExport.Rows.Count - 1; i++)
                    {
                        for (int j = 0; j < dataGridViewExport.Columns.Count; j++)
                        {
                            // Excel index starts from 1,1. As first Row would have the Column headers, adding a condition check. 
                            if (cellRowIndex == 1)
                            {
                                worksheet.Cells[cellRowIndex, cellColumnIndex] = dataGridViewExport.Columns[j].HeaderText;
                                worksheet.Cells[cellRowIndex, cellColumnIndex].Interior.Color = Color.LightGray;
                                worksheet.Cells[cellRowIndex, cellColumnIndex].Borders.Weight = 1d;
                            }

                            if (dataGridViewExport.Rows[i].Cells[j].Value.ToString() == "System.Drawing.Bitmap")
                            {
                                Excel.Range oRange = (Excel.Range)worksheet.Cells[cellRowIndex + 1, cellColumnIndex];
                                Image img = (Image)(dataGridViewExport.Rows[i].Cells[j].Value);
                                //Image resizeImage = new Bitmap(img, new Size(280, 180));
                                //Image resizeImage = ResizeImage(img, 280, 180);
                                image_width = img.Width;
                                image_height = img.Height;
                                //Image newImage = (Image)dataGridViewExport.Rows[i].Cells[j].Value;
                                //Clipboard.Clear();
                                Clipboard.SetImage(img);
                                worksheet.Paste(oRange, false);
                                //System.Windows.Forms.Clipboard.SetDataObject(img, true);
                            }
                            else
                            {

                                if (j > 0)
                                {
                                    worksheet.Cells[cellRowIndex + 1, cellColumnIndex] = dataGridViewExport.Rows[i].Cells[j].Value; //Convert.ToDouble(dataGridViewExport.Rows[i].Cells[j].Value);
                                    worksheet.Cells[cellRowIndex + 1, cellColumnIndex].NumberFormat = "0.00E+00";
                                    worksheet.Cells[cellRowIndex + 1, cellColumnIndex].Interior.Color = dataGridViewExport.Rows[i].Cells[j].Style.BackColor;
                                }
                                if (j == 0)
                                {
                                    worksheet.Cells[cellRowIndex + 1, cellColumnIndex] = dataGridViewExport.Rows[i].Cells[j].Value;
                                    worksheet.Cells[cellRowIndex + 1, cellColumnIndex].Interior.Color = Color.LightGray;
                                    worksheet.Cells[cellRowIndex + 1, cellColumnIndex].Borders.Weight = 1d;
                                }
                                worksheet.Cells[cellRowIndex + 1, cellColumnIndex].Style.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                                worksheet.Cells[cellRowIndex + 1, cellColumnIndex].Style.VerticalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                            }

                            cellColumnIndex++;
                        }
                        cellColumnIndex = 1;
                        cellRowIndex++;

                        //worksheet.UsedRange.Columns.AutoFit();
                        //worksheet.UsedRange.Rows.AutoFit();
                    }

                    int col_count = worksheet.Columns.Count;
                    int row_count = worksheet.Rows.Count;
                    int dg_rows = dataGridViewExport.Rows.Count;
                    int dg_cols = dataGridViewExport.Columns.Count;

                    Graphics g = this.CreateGraphics();

                    for (int i = 1; i <= dataGridViewExport.Rows.Count; i++)
                    {
                        if (i == 1) worksheet.Rows[i].RowHeight = 20;
                        else worksheet.Rows[i].RowHeight = (double)image_height / g.DpiY * 72.0f; //  g.DpiY
                    }

                    for (int j = 1; j <= dataGridViewExport.Columns.Count; j++)
                    {
                        if (j % 2 == 0) worksheet.Columns[j].ColumnWidth = (double)image_width / g.DpiX * 72.0f / 5.1f; // image_width; g.DpiX
                        else worksheet.Columns[j].ColumnWidth = 15;
                        //if (j == 0) worksheet.Columns[j].ColumnWidth = 10;
                    }

                    workbook.SaveAs(saveDialog.FileName);
                    MessageBox.Show("Export Successful");
                }
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
