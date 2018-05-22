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
    public partial class Correlations_Exp_Name : Form
    {

        Correlations_Tab _form_correl = new Correlations_Tab();

        public Correlations_Exp_Name(Correlations_Tab form, string fileName_1, string fileName_2)
        {
            InitializeComponent();
            _form_correl = form;

            label1.Text = "File : " + fileName_1;
            label2.Text = "File : " + fileName_2;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string label_exp_1 = textBox1.Text;
            string label_exp_2 = textBox2.Text;

            _form_correl.set_Labels(label_exp_1, label_exp_2);

            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
