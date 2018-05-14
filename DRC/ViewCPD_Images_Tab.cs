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
        public ViewCPD_Images_Tab()
        {
            InitializeComponent();
        }

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

                int height = (int)((double)image_height / g.DpiY * 72.0f + 10 / g.DpiY * 72.0f); //  g.DpiY
                int width = (int)((double)image_width / g.DpiX * 72.0f / 5.6f + 10 / g.DpiY * 72.0f); // image_width; g.DpiX

                for (int i = 1; i <= dataGridView1.Rows.Count+1; i++)
                {
                    if (i == 1) ws.Row(i).Height = 20;
                    else ws.Row(i).Height = height;
                }

                for (int j = 1; j <= dataGridView1.Columns.Count; j++)
                {
                    if (j == 1) ws.Column(j).Width = 15;
                    else ws.Column(j).Width = width;
                    //if (j == 0) worksheet.Columns[j].ColumnWidth = 10;
                }

                toolStripProgressBar1.Visible = true;
                //Loop through each row and read value from each column. 
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    toolStripProgressBar1.Value = i * 100 / (dataGridView1.Rows.Count - 1);


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

                            string name_idx = (cellRowIndex * dataGridView1.Columns.Count + cellColumnIndex).ToString();

                            ExcelPicture excelImage = null;

                            excelImage = ws.Drawings.AddPicture(name_idx, img);
                            excelImage.From.Column = cellColumnIndex - 1;
                            excelImage.From.Row = cellRowIndex;
                            excelImage.SetSize(image_width, image_height);

                        }
                        else
                        {

                            if (j > 0)
                            {

                                ws.Cells[cellRowIndex + 1, cellColumnIndex].Value = dataGridView1.Rows[i].Cells[j].Value; //Convert.ToDouble(dataGridView1.Rows[i].Cells[j].Value);

                                ws.Cells[cellRowIndex + 1, cellColumnIndex].Style.Numberformat.Format = "0.00E+00";

                                ws.Cells[cellRowIndex + 1, cellColumnIndex].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                ws.Cells[cellRowIndex + 1, cellColumnIndex].Style.Fill.BackgroundColor.SetColor(dataGridView1.Rows[i].Cells[j].Style.BackColor);
                                ws.Cells[cellRowIndex + 1, cellColumnIndex].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                ws.Cells[cellRowIndex + 1, cellColumnIndex].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

                            }
                            if (j == 0)
                            {
                                ws.Cells[cellRowIndex + 1, cellColumnIndex].Value = dataGridView1.Rows[i].Cells[j].Value;

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

                MessageBox.Show("Export Successful");

            }
        }
    }
}
