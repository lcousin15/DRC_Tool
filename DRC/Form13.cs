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
    public partial class Form13 : Form
    {
        private bool ButtonOkWasClicked;

        public Form13()
        {
            InitializeComponent();
            ButtonOkWasClicked = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ButtonOkWasClicked = true;
        }

        public bool isButtonClicked()
        {
            return ButtonOkWasClicked;
        }
    
    }
}
