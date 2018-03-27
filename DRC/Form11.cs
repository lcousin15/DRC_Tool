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
    public partial class Form11 : Form
    {

        Form1 _form1 = new Form1();

        public Form11(Form1 form)
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
