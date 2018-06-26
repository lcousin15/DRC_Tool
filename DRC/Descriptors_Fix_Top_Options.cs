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
    public partial class Descriptors_Fix_Top_Options : Form
    {
        MainTab _main_tab;

        public Descriptors_Fix_Top_Options(MainTab main_tab)
        {
            InitializeComponent();

            _main_tab = main_tab;
        }

        private void btn_apply_Click(object sender, EventArgs e)
        {

        }

        private void btn_ok_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
