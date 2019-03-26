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
    public partial class CPD_Tab : Form
    {

        MainTab _form1 = new MainTab();

        public CPD_Tab(MainTab form)
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
                    for (int i = 0; i < columnCount-1; i++)
                    {
                        if (i < columnCount - 2) columnNames += dataGridView2.Columns[i].Name.ToString() + ",";
                        else columnNames += dataGridView2.Columns[i].Name.ToString();
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
                    System.IO.File.WriteAllLines(sfd.FileName, output);
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

       

        private void dataGridView2_Row_enter(object sender, DataGridViewCellEventArgs e)
        {
            _form1.dataGridView2_CellDoubleClick(sender, e);
        }

        private void dataGridView2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                //List<string> cpds_id = new List<string>();
                _form1.view_images_per_concentration = true;
                _form1.check_images();
                foreach (DataGridViewRow item in dataGridView2.SelectedRows)
                {
                    string BATCH_ID = dataGridView2.Rows[item.Index].Cells[0].Value.ToString();
                    //_form1.view_images_per_concentration = true;
                    _form1.load_cpd_images(BATCH_ID, true);
                }
                //string BATCH_ID = dataGridView2.Rows[dataGridView2.CurrentCell.OwningRow.Index].Cells[0].Value.ToString();
            }
            if(e.KeyCode == Keys.I)
            {

                string BATCH_ID = "";

                foreach (DataGridViewCell item in dataGridView2.SelectedCells)
                {
                    if (item.ColumnIndex != 0) return;
         
                    BATCH_ID = item.Value.ToString();
                }

                _form1.inactive_cpd(BATCH_ID);

            }
        }
    }
}
