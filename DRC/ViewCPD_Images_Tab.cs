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
    public partial class ViewCPD_Images_Tab : Form
    {

        MainTab _main_tab;

        public ViewCPD_Images_Tab(MainTab main_tab)
        {
            InitializeComponent();
            _main_tab = main_tab;
        }

        public bool view_images_per_concentration;

        private void Form12_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Visible = false;
            e.Cancel = false;
        }

        private void dataGridView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                foreach (DataGridViewCell oneCell in dataGridView1.SelectedCells)
                {
                    if (oneCell.Selected)
                        dataGridView1.Rows.RemoveAt(oneCell.RowIndex);
                }
            }
        }

        private void saveToExcelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Excel Documents (*.xlsx)|*.xlsx";
            sfd.FileName = "Image_Hits_Report.xlsx";
            if (sfd.ShowDialog() == DialogResult.OK)
            {

                ExcelPackage pck = new ExcelPackage();

                ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Image_Hits_Report");
                //wsDt.Cells["A1"].LoadFromDataTable(dt, true);
                //wsDt.Cells["A2"].LoadFromDataTable((this.dataGridView1.DataSource as DataTable).DefaultView.ToTable(), true);

                int cellRowIndex = 1;
                int cellColumnIndex = 1;

                Graphics g = this.CreateGraphics();

                int image_width = dataGridView1.Rows[0].Cells[1].Size.Width;
                int image_height = dataGridView1.Rows[0].Cells[1].Size.Height;

                //double dpi_excel = 220.0;

                double height = (double)((image_height+10) / g.DpiY * 72.0f); //  g.DpiY
                double width = (double)(((image_width+10-7)/7.0+1.0)); // image_width; g.DpiX

                for (int i = 1; i <= dataGridView1.Rows.Count+1; i++)
                {
                    if (i == 1) ws.Row(i).Height = 20;
                    else ws.Row(i).Height = height;
                }
                
                for (int j = 1; j <= dataGridView1.Columns.Count; j++)
                {
                    if (j == 1) ws.Column(j).Width = 15;
                    else ws.Column(j).Width = width;
                          
                    if (j>2 && !view_images_per_concentration) ws.Column(j).Width = 15;
                    //if (j == 0) worksheet.Columns[j].ColumnWidth = 10;
                }

                toolStripProgressBar1.Visible = true;
                //Loop through each row and read value from each column. 
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    toolStripProgressBar1.Value = i * 100 / dataGridView1.Rows.Count;


                    for (int j = 0; j < dataGridView1.Columns.Count; j++)
                    {
                        // Excel index starts from 1,1. As first Row would have the Column headers, adding a condition check. 
                        if (cellRowIndex == 1)
                        {
                            ws.Cells[cellRowIndex, cellColumnIndex].Value = dataGridView1.Columns[j].HeaderText;

                            ws.Cells[cellRowIndex, cellColumnIndex].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            ws.Cells[cellRowIndex, cellColumnIndex].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                            ws.Cells[cellRowIndex, cellColumnIndex].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Dotted);
                            ws.Cells[cellRowIndex, cellColumnIndex].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                            ws.Cells[cellRowIndex, cellColumnIndex].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                            //ws.Cells[cellRowIndex, cellColumnIndex].Borders.Weight = 1d;
                        }

                        if (dataGridView1.Rows[i].Cells[j].Value.ToString() == "System.Drawing.Bitmap")
                        {

                            Bitmap img = (Bitmap)(dataGridView1.Rows[i].Cells[j].Value);

                            //double resX = img.HorizontalResolution;
                            //double resY = img.VerticalResolution;
                            //double size_X = img.Width;
                            //double size_Y = img.Height;

                            string name_idx = (cellRowIndex * dataGridView1.Columns.Count + cellColumnIndex).ToString();

                            ExcelPicture excelImage = null;

                            excelImage = ws.Drawings.AddPicture(name_idx, img);
                            excelImage.From.Column = cellColumnIndex - 1;
                            excelImage.From.Row = cellRowIndex;
                            excelImage.SetSize(img.Width, img.Height);
                            //excelImage.AdjustPositionAndSize.SetBounds

                        }
                        else
                        {

                            if (j == 0)
                            {
                                ws.Cells[cellRowIndex + 1, cellColumnIndex].Style.WrapText = true;
                                ws.Cells[cellRowIndex + 1, cellColumnIndex].Value = dataGridView1.Rows[i].Cells[j].Value;

                                ws.Cells[cellRowIndex + 1, cellColumnIndex].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                ws.Cells[cellRowIndex + 1, cellColumnIndex].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                                ws.Cells[cellRowIndex + 1, cellColumnIndex].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                ws.Cells[cellRowIndex + 1, cellColumnIndex].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                ws.Cells[cellRowIndex + 1, cellColumnIndex].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Dotted);

                            }
                            else if (j > 0)
                            {
                                ws.Cells[cellRowIndex + 1, cellColumnIndex].Value = dataGridView1.Rows[i].Cells[j].Value; //Convert.ToDouble(dataGridView1.Rows[i].Cells[j].Value);

                                ws.Cells[cellRowIndex + 1, cellColumnIndex].Style.Numberformat.Format = "0.00E+00";

                                //ws.Cells[cellRowIndex + 1, cellColumnIndex].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                //ws.Cells[cellRowIndex + 1, cellColumnIndex].Style.Fill.BackgroundColor.SetColor(dataGridView1.Rows[i].Cells[j].Style.BackColor);
                                ws.Cells[cellRowIndex + 1, cellColumnIndex].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                ws.Cells[cellRowIndex + 1, cellColumnIndex].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

                            }
                        }

                        cellColumnIndex++;
                    }

                    cellColumnIndex = 1;
                    cellRowIndex++;
                }

                ExcelWorksheet ws_params = pck.Workbook.Worksheets.Add("Parameters");

                ws_params.Cells[2, 2].Value = "Low Thr Ch1";
                ws_params.Cells[2, 3].Value = "Low Thr Ch2";
                ws_params.Cells[2, 4].Value = "Low Thr Ch3";
                ws_params.Cells[2, 5].Value = "Low Thr Ch4";

                ws_params.Cells[3, 2].Value = _main_tab.cpd_low_thr_ch1;
                ws_params.Cells[3, 3].Value = _main_tab.cpd_low_thr_ch2;
                ws_params.Cells[3, 4].Value = _main_tab.cpd_low_thr_ch3;
                ws_params.Cells[3, 5].Value = _main_tab.cpd_low_thr_ch4;

                ws_params.Cells[5, 2].Value = "High Thr Ch1";
                ws_params.Cells[5, 3].Value = "High Thr Ch2";
                ws_params.Cells[5, 4].Value = "High Thr Ch3";
                ws_params.Cells[5, 5].Value = "High Thr Ch4";

                ws_params.Cells[6, 2].Value = _main_tab.cpd_high_thr_ch1;
                ws_params.Cells[6, 3].Value = _main_tab.cpd_high_thr_ch2;
                ws_params.Cells[6, 4].Value = _main_tab.cpd_high_thr_ch3;
                ws_params.Cells[6, 5].Value = _main_tab.cpd_high_thr_ch4;

                toolStripProgressBar1.Visible = false;
                pck.SaveAs(new FileInfo(@"" + sfd.FileName));

                MessageBox.Show("Export Successful");

            }
        }
    }
}
