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

        private string cpd_id = "";

        public CPD_Time_Line(MainTab f)
        {
            InitializeComponent();
            _form1 = f;
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            cpd_id = dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString();
            //Console.WriteLine(cpd_id);
            _form1.get_compound_data(cpd_id);
        }

        private void checkedListBox1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            string file = checkedListBox1.Items[e.Index].ToString();
            Console.WriteLine(file);
            _form1.draw_cpd_list(file, cpd_id);
        }
    }
}
