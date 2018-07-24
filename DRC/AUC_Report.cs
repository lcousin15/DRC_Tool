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
    public partial class AUC_Report : Form
    {
        public AUC_Report(MainTab main_tab)
        {
            InitializeComponent();

            _main_tab = main_tab;
        }

        MainTab _main_tab;
    }
}
