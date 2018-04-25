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
    public partial class ViewList_CPD_Tab : Form
    {

        MainTab _form1 = new MainTab();

        public ViewList_CPD_Tab(MainTab form)
        {
            InitializeComponent();
            _form1 = form;
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            _form1.load_cpd_images(sender, e);
        }
    }
}
