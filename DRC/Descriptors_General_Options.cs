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
    public partial class Descriptors_General_Options : Form
    {
        MainTab _main_tab;

        public Descriptors_General_Options(MainTab main_tab)
        {
            InitializeComponent();
            _main_tab = main_tab;
        }

        private void btn_ok_Click(object sender, EventArgs e)
        {
            _main_tab.apply_descritpor_general_options();

            this.Close();
        }
    }
}
