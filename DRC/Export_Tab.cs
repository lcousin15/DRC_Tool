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
using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using OfficeOpenXml.Table;
using System.Diagnostics;

namespace DRC
{
    public partial class Export_Tab : Form
    {
        MainTab _main_tab;

        public Export_Tab(MainTab main_tab)
        {
            InitializeComponent();
            _main_tab = main_tab;
        }
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
        /*
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

                            // Get an Excel Range of the same dimensions
                            Excel.Range range = (Excel.Range)worksheet.Cells[1, 1];
                            range = range.get_Resize(dataGridViewExport.Rows.Count, dataGridViewExport.Columns.Count);
                            // Assign the 2-d array to the Excel Range

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
                                        Excel.Range cell = range.Cells[cellRowIndex, cellColumnIndex];
                                        cell.Value = dataGridViewExport.Columns[j].HeaderText;
                                        cell.Interior.Color = Color.LightGray;
                                        cell.Borders.Weight = 1d;
                                    }

                                    if (dataGridViewExport.Rows[i].Cells[j].Value.ToString() == "System.Drawing.Bitmap")
                                    {
                                        Excel.Range oRange = range.Cells[cellRowIndex + 1, cellColumnIndex];
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

                                            Excel.Range cell = range.Cells[cellRowIndex + 1, cellColumnIndex];

                                            cell.Value = dataGridViewExport.Rows[i].Cells[j].Value; //Convert.ToDouble(dataGridViewExport.Rows[i].Cells[j].Value);
                                            cell.NumberFormat = "0.00E+00";
                                            cell.Interior.Color = dataGridViewExport.Rows[i].Cells[j].Style.BackColor;
                                            cell.Style.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                                            cell.Style.VerticalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                                        }
                                        if (j == 0)
                                        {
                                            Excel.Range cell = range.Cells[cellRowIndex + 1, cellColumnIndex];

                                            cell.Value = dataGridViewExport.Rows[i].Cells[j].Value;
                                            cell.Interior.Color = Color.LightGray;
                                            cell.Borders.Weight = 1d;
                                            cell.Style.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                                            cell.Style.VerticalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                                        }


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
        */
        /*
                private void saveToExcelToolStripMenuItem_Click(object sender, EventArgs e)
                {
                    SaveFileDialog sfd = new SaveFileDialog();
                    sfd.Filter = "Excel Documents (*.xls)|*.xls";
                    sfd.FileName = "DRC_Report.xls";
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        // Copy DataGridView results to clipboard
                        copyAlltoClipboard();

                        object misValue = System.Reflection.Missing.Value;
                        Excel.Application xlexcel = new Excel.Application();

                        xlexcel.DisplayAlerts = false; // Without this you will get two confirm overwrite prompts
                        Excel.Workbook xlWorkBook = xlexcel.Workbooks.Add(misValue);
                        xlexcel.ActiveWorkbook.Sheets[1].Activate();
                        Excel.Worksheet xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);

                        // Format column D as text before pasting results, this was required for my data
                        //Excel.Range rng = xlWorkSheet.get_Range("D:D").Cells;
                        //rng.NumberFormat = "@";

                        // Paste clipboard results to worksheet range
                        Excel.Range CR = (Excel.Range)xlWorkSheet.Cells[1, 1];
                        CR.Select();
                        xlWorkSheet.PasteSpecial(CR, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);

                        // For some reason column A is always blank in the worksheet. ¯\_(ツ)_/¯
                        // Delete blank column A and select cell A1
                        //Excel.Range delRng = xlWorkSheet.get_Range("A:A").Cells;
                        //delRng.Delete(Type.Missing);
                        //xlWorkSheet.get_Range("A1").Select();

                        // Save the excel file under the captured location from the SaveFileDialog
                        xlWorkBook.SaveAs(sfd.FileName, Excel.XlFileFormat.xlWorkbookNormal, misValue, misValue, misValue, misValue, Excel.XlSaveAsAccessMode.xlExclusive, misValue, misValue, misValue, misValue, misValue);
                        xlexcel.DisplayAlerts = true;
                        xlWorkBook.Close(true, misValue, misValue);
                        xlexcel.Quit();

                        releaseObject(xlWorkSheet);
                        releaseObject(xlWorkBook);
                        releaseObject(xlexcel);

                        // Clear Clipboard and DataGridView selection
                        Clipboard.Clear();
                        dataGridViewExport.ClearSelection();

                        // Open the newly saved excel file
                        //if (File.Exists(sfd.FileName))
                        //    System.Diagnostics.Process.Start(sfd.FileName);
                    }
                }
        */

        //public int Pixel2MTU(int pixels)
        //{
        //    int mtus = pixels * 9525;
        //    return mtus;
        //}

        private void AddImage(ExcelWorksheet oSheet, int rowIndex, int colIndex, Bitmap img, string name)
        {
            //Bitmap image = new Bitmap(img);
            ExcelPicture excelImage = null;

            excelImage = oSheet.Drawings.AddPicture(name, img);
            excelImage.From.Column = colIndex;
            excelImage.From.Row = rowIndex;
            excelImage.SetSize(485, 350);
            // 2x2 px space for better alignment
            //excelImage.From.ColumnOff = Pixel2MTU(2);
            //excelImage.From.RowOff = Pixel2MTU(2);
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
                //wsDt.Cells["A1"].LoadFromDataTable(dt, true);
                //wsDt.Cells["A2"].LoadFromDataTable((this.dataGridViewExport.DataSource as DataTable).DefaultView.ToTable(), true);

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
                    if ((j-1) % 4 == 1) ws.Column(j).Width = width;
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

                            //ws.Row(1).Height = 20;
                            //if (cellColumnIndex % 2 == 0) ws.Column(cellColumnIndex).Width = 485 / g.DpiX * 72.0f / 5.1f;
                            //else ws.Column(cellColumnIndex).Width = 15;

                            ws.Cells[cellRowIndex, cellColumnIndex].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            ws.Cells[cellRowIndex, cellColumnIndex].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                            ws.Cells[cellRowIndex, cellColumnIndex].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Dotted);
                            ws.Cells[cellRowIndex, cellColumnIndex].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                            ws.Cells[cellRowIndex, cellColumnIndex].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                            //ws.Cells[cellRowIndex, cellColumnIndex].Borders.Weight = 1d;
                        }

                        if (dataGridViewExport.Rows[i].Cells[j].Value.ToString() == "System.Drawing.Bitmap")
                        {

                            //var watch = new System.Diagnostics.Stopwatch();

                            //watch.Start();

                            Bitmap img = (Bitmap)(dataGridViewExport.Rows[i].Cells[j].Value);

                            //ws.Row(i + 1).Height = 350.0 / 96.0 * 72.0f;
                           
                           
                            //if (j % 2 == 0) ws.Column(1).Width = 485;
                            //else ws.Column(1).Width = 15;

                            //image_width = img.Width;
                            //image_height = img.Height; 


                            string name_idx = (cellRowIndex * dataGridViewExport.Columns.Count + cellColumnIndex).ToString();

                            ExcelPicture excelImage = null;

                            excelImage = ws.Drawings.AddPicture(name_idx, img);
                            excelImage.From.Column = cellColumnIndex - 1;
                            excelImage.From.Row = cellRowIndex;
                            excelImage.SetSize(485, 350);

                            //watch.Stop();
                            //Console.WriteLine($"Execution Time: {watch.ElapsedMilliseconds} ms");


                            //AddImage(ws, cellRowIndex, cellColumnIndex-1, img, name_idx);
                        }
                        else
                        {

                            if (j > 0)
                            {

                                ws.Cells[cellRowIndex + 1, cellColumnIndex].Value = dataGridViewExport.Rows[i].Cells[j].Value; //Convert.ToDouble(dataGridViewExport.Rows[i].Cells[j].Value);

                                //if (cellRowIndex + 1 == 1) ws.Row(cellRowIndex + 1).Height = 20;
                                //else ws.Row(cellRowIndex + 1).Height = 350.0 / g.DpiY * 72.0f; ;
                                //if (cellColumnIndex % 2 == 0) ws.Column(1).Width = 485;
                                //else ws.Column(cellColumnIndex).Width = 15;

                                //ws.Cells[cellRowIndex + 1, cellColumnIndex].NumberFormat = "0.00E+00";
                                ws.Cells[cellRowIndex + 1, cellColumnIndex].Style.Numberformat.Format = "0.00E+00";
                                //ws.Cells[cellRowIndex + 1, cellColumnIndex].Interior.Color = dataGridViewExport.Rows[i].Cells[j].Style.BackColor;

                                ws.Cells[cellRowIndex + 1, cellColumnIndex].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                ws.Cells[cellRowIndex + 1, cellColumnIndex].Style.Fill.BackgroundColor.SetColor(dataGridViewExport.Rows[i].Cells[j].Style.BackColor);
                                ws.Cells[cellRowIndex + 1, cellColumnIndex].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                ws.Cells[cellRowIndex + 1, cellColumnIndex].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                ws.Cells[cellRowIndex + 1, cellColumnIndex].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Dotted);

                                //ws.Cells[cellRowIndex + 1, cellColumnIndex].Style.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                                //ws.Cells[cellRowIndex + 1, cellColumnIndex].Style.VerticalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                            }
                            if (j == 0)
                            {
                                ws.Cells[cellRowIndex + 1, cellColumnIndex].Value = dataGridViewExport.Rows[i].Cells[j].Value;

                                //if (cellRowIndex + 1 == 1) ws.Row(cellRowIndex + 1).Height = 20;
                                //else ws.Row(cellRowIndex + 1).Height = 350.0 / g.DpiY * 72.0f; ;
                                //if (j % 2 == 0) ws.Column(1).Width = 485;
                                //else ws.Column(1).Width = 15;

                                ws.Cells[cellRowIndex + 1, cellColumnIndex].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                ws.Cells[cellRowIndex + 1, cellColumnIndex].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                                ws.Cells[cellRowIndex + 1, cellColumnIndex].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                ws.Cells[cellRowIndex + 1, cellColumnIndex].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                ws.Cells[cellRowIndex + 1, cellColumnIndex].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Dotted);

                                //ws.Cells[cellRowIndex + 1, cellColumnIndex].Interior.Color = Color.LightGray;
                                //ws.Cells[cellRowIndex + 1, cellColumnIndex].Borders.Weight = 1d;
                                //ws.Cells[cellRowIndex + 1, cellColumnIndex].Style.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                                //ws.Cells[cellRowIndex + 1, cellColumnIndex].Style.VerticalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                            }


                        }

                        cellColumnIndex++;
                    }
                    cellColumnIndex = 1;
                    cellRowIndex++;

                    //worksheet.UsedRange.Columns.AutoFit();
                    //worksheet.UsedRange.Rows.AutoFit();
                }

                //ws.Cells[ws.Dimension.Address].AutoFitColumns();
                /*
                                var watch2 = new System.Diagnostics.Stopwatch();

                                watch2.Start();

                                Graphics g = this.CreateGraphics();

                                double height = (double)image_height / g.DpiY * 72.0f; //  g.DpiY
                                double width = (double)image_width / g.DpiX * 72.0f / 5.1f; // image_width; g.DpiX

                                for (int i = 1; i <= dataGridViewExport.Rows.Count; i++)
                                {
                                    if (i == 1) ws.Row(i).Height = 20;
                                    else ws.Row(i).Height = height;
                                }

                                for (int j = 1; j <= dataGridViewExport.Columns.Count; j++)
                                {
                                    if (j % 2 == 0) ws.Column(j).Width = width;
                                    else ws.Column(j).Width = 15;
                                    //if (j == 0) worksheet.Columns[j].ColumnWidth = 10;
                                }

                                watch2.Stop();

                                Console.WriteLine($"Execution Time2: {watch2.ElapsedMilliseconds} ms");
                */
                toolStripProgressBar1.Visible = false;
                pck.SaveAs(new FileInfo(@"" + sfd.FileName));

                pck.Dispose();

                MessageBox.Show("Export Successful");

            }
        }
        /*
        public void ExportExcel(DataTable ds)
        {

            using (ExcelPackage pck = new ExcelPackage())
            {
                //Create the worksheet
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add("SearchReport");

                //Load the datatable into the sheet, starting from cell A1. Print the column names on row 1
                ws.Cells["A1"].LoadFromDataTable(ds, true);

                //prepare the range for the column headers
                string cellRange = "A1:" + Convert.ToChar('A' + ds.Columns.Count - 1) + 1;

                //Format the header for columns
                using (ExcelRange rng = ws.Cells[cellRange])
                {
                    rng.Style.WrapText = false;
                    rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    rng.Style.Font.Bold = true;
                    rng.Style.Fill.PatternType = ExcelFillStyle.Solid; //Set Pattern for the background to Solid
                    rng.Style.Fill.BackgroundColor.SetColor(Color.Gray);
                    rng.Style.Font.Color.SetColor(Color.White);
                }

                //prepare the range for the rows
                string rowsCellRange = "A2:" + Convert.ToChar('A' + ds.Columns.Count - 1) + ds.Rows.Count * ds.Columns.Count;

                //Format the rows
                using (ExcelRange rng = ws.Cells[rowsCellRange])
                {
                    rng.Style.WrapText = false;
                    rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                }

                //Read the Excel file in a byte array
                Byte[] fileBytes = pck.GetAsByteArray();

                //Clear the response
                Response.Clear();
                Response.ClearContent();
                Response.ClearHeaders();
                Response.Cookies.Clear();

                //Add the header & other information
                Response.Cache.SetCacheability(HttpCacheability.Private);
                Response.CacheControl = "private";
                Response.Charset = System.Text.UTF8Encoding.UTF8.WebName;
                Response.ContentEncoding = System.Text.UTF8Encoding.UTF8;
                Response.AppendHeader("Content-Length", fileBytes.Length.ToString());
                Response.AppendHeader("Pragma", "cache");
                Response.AppendHeader("Expires", "60");
                Response.AppendHeader("Content-Disposition",
                "attachment; " +
                "filename=\"ExcelReport.xlsx\"; " +
                "size=" + fileBytes.Length.ToString() + "; " +
                "creation-date=" + DateTime.Now.ToString("R") + "; " +
                "modification-date=" + DateTime.Now.ToString("R") + "; " +
                "read-date=" + DateTime.Now.ToString("R"));
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                //Write it back to the client
                Response.BinaryWrite(fileBytes);
                Response.End();
            }
        }
        */

        private void copyAlltoClipboard()
        {
            this.dataGridViewExport.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            dataGridViewExport.SelectAll();
            DataObject dataObj = dataGridViewExport.GetClipboardContent();
            if (dataObj != null)
                Clipboard.SetDataObject(dataObj);
        }

        private void releaseObject(object obj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch (Exception ex)
            {
                obj = null;
                MessageBox.Show("Exception Occurred while releasing object " + ex.ToString());
            }
            finally
            {
                GC.Collect();
            }
        }

        private void Export_Tab_FormClosed(object sender, FormClosedEventArgs e)
        {
            /*
            foreach (KeyValuePair<string, List<string>> item in _main_tab.list_img_path_by_cpd)
            {
                dataGridViewExport.Rows.Clear();
                dataGridViewExport.Refresh();
                dataGridViewExport.Dispose();

                List<string> list_path = item.Value;

                foreach (string current_path in list_path)
                {
                    File.Delete(current_path);
                }

            }
            */ 
        }
    }
}
