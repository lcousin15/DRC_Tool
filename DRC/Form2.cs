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
    public partial class Form2 : Form
    {

        Form1 _form1 = new Form1();

        public Form2(Form1 form)
        {
            InitializeComponent();
            _form1 = form;
        }

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            _form1.dataGridView2_CellDoubleClick(sender, e);
        }

        private void dataGridView2_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {

                string filename = "";

                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "CSV (*.csv)|*.csv";
                sfd.FileName = "Output.csv";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    if (File.Exists(filename))
                    {
                        try
                        {
                            File.Delete(filename);
                        }
                        catch (IOException ex)
                        {
                            MessageBox.Show("It wasn't possible to write the data to the disk." + ex.Message);
                        }
                    }
                    int columnCount = dataGridView2.ColumnCount;
                    string columnNames = "";
                    string[] output = new string[dataGridView2.RowCount + 1];
                    for (int i = 0; i < columnCount; i++)
                    {
                        columnNames += dataGridView2.Columns[i].Name.ToString() + ",";
                    }
                    output[0] += columnNames;
                    for (int i = 1; (i - 1) < dataGridView2.RowCount; i++)
                    {
                        if (dataGridView2.Rows[i-1].Cells[0].Value == null) continue;

                        for (int j = 0; j < columnCount-1; j++)
                        {
                            //System.Diagnostics.Debug.WriteLine("Index, Write = " + i.ToString() + "-->" + dataGridView2.Rows[i - 1].Cells[j].Value.ToString() + ",");
                            if (j < columnCount - 2) output[i] += dataGridView2.Rows[i - 1].Cells[j].Value.ToString() + ",";
                            else if (j == columnCount - 2) output[i] += dataGridView2.Rows[i - 1].Cells[j].Value.ToString();
                        }
                    }
                    System.IO.File.WriteAllLines(sfd.FileName, output, System.Text.Encoding.UTF8);
                    MessageBox.Show("File was generated.");
                }
            }
        }

        private void Form2_closing_event(object sender, FormClosingEventArgs e)
        {
            DialogResult dlgresult = MessageBox.Show("Don't close this window before leaving the software. Exit or no?",
                            "Warning",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Information);

            if (dlgresult == DialogResult.No)
            {
                e.Cancel = true;
            }
            else
            {
                e.Cancel = false;
            }
        }
    }
}
