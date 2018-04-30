using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
    }
}
