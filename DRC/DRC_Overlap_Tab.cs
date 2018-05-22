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
    public partial class DRC_Overlap_Tab : Form
    {

        MainTab _form1 = new MainTab();

        public DRC_Overlap_Tab(MainTab form)
        {
            InitializeComponent();
            _form1 = form;
        }

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            _form1.dataGridView3_CellDoubleClick(sender, e);
        }

        private void Form10_FormClosing(object sender, FormClosingEventArgs e)
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
