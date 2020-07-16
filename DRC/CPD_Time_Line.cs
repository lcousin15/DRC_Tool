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
        private string BATCH_ID = "";
        public CPD_Time_Line(MainTab f)
        {
            InitializeComponent();
            _form1 = f;
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            BATCH_ID = dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString();
            //Console.WriteLine(BATCH_ID);
            _form1.get_compound_data(BATCH_ID);
        }

        public void checkedListBox1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            string file = checkedListBox1.Items[e.Index].ToString();
            bool checked_state = true;

            //checkedListBox1.Update();

            if (!checkedListBox1.GetItemChecked(e.Index)) checked_state = false;

            Console.WriteLine(file);
            Console.WriteLine(checked_state);
            _form1.draw_cpd_list(file, BATCH_ID, checked_state);
        }
    }
}
