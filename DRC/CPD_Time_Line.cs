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
    public partial class CPD_Time_Line : Form
    {
        MainTab _form1 = new MainTab();

        public CPD_Time_Line(MainTab f)
        {
            InitializeComponent();
            _form1 = f;
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            string cpd_id = dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString();
            //Console.WriteLine(cpd_id);
            _form1.draw_compound_data(cpd_id);
        }
    }
}
