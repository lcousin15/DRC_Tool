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
    public partial class Load_PS_Options : Form
    {
        MainTab _main_tab;

        public Load_PS_Options(MainTab main_tab)
        {
            InitializeComponent();
            _main_tab = main_tab;
        }

        private string path_template;

        private string path_plate_1_1;
        private string path_plate_1_2;
        private string path_plate_2_1;
        private string path_plate_2_2;


        public string get_template_path()
        {
            return path_template;
        }

        public string get_plate_1_1_path()
        {
            return path_plate_1_1;
        }

        public string get_plate_1_2_path()
        {
            return path_plate_1_2;
        }

        public string get_plate_2_1_path()
        {
            return path_plate_2_1;
        }

        public string get_plate_2_2_path()
        {
            return path_plate_2_2;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            List<string> list_paths = new List<string>();
            List<string> plate_names = new List<string>();

            if (checkBox1.Checked && path_plate_1_1.Length > 1)
            {
                list_paths.Add(path_plate_1_1);
                plate_names.Add("Plate 1-1");
            }
            if (checkBox2.Checked && path_plate_1_2.Length > 1)
            {
                list_paths.Add(path_plate_1_2);
                plate_names.Add("Plate 1-2");
            }
            if (checkBox3.Checked && path_plate_2_1.Length > 1)
            {
                list_paths.Add(path_plate_2_1);
                plate_names.Add("Plate 2-1");
            }
            if (checkBox4.Checked && path_plate_2_2.Length > 1)
            {
                list_paths.Add(path_plate_2_2);
                plate_names.Add("Plate 2-2");
            }

            if (path_template.Length > 1) _main_tab.process_template(path_template);
            if (list_paths.Count > 0) _main_tab.process_data_PS(list_paths, plate_names);
            this.Close();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if(checkBox1.Checked)
            {
                textBox1.Enabled = true;
                button1.Enabled = true;
            }
            else
            {
                textBox1.Enabled = false;
                button1.Enabled = false;
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                textBox2.Enabled = true;
                button2.Enabled = true;
            }
            else
            {
                textBox2.Enabled = false;
                button2.Enabled = false;
            }
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox3.Checked)
            {
                textBox3.Enabled = true;
                button3.Enabled = true;
            }
            else
            {
                textBox3.Enabled = false;
                button3.Enabled = false;
            }
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox4.Checked)
            {
                textBox4.Enabled = true;
                button4.Enabled = true;
            }
            else
            {
                textBox4.Enabled = false;
                button4.Enabled = false;
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "CSV Files (*.csv)|*.csv";
            openFileDialog1.Title = "Plate 1-1 File";

            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                path_template = openFileDialog1.FileName;
                textBox5.Text = path_template;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "CSV Files (*.csv)|*.csv";
            openFileDialog1.Title = "Plate 1-1 File";

            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                path_plate_1_1 = openFileDialog1.FileName;
                textBox1.Text = path_plate_1_1;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "CSV Files (*.csv)|*.csv";
            openFileDialog1.Title = "Plate 1-2 File";

            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                path_plate_1_2 = openFileDialog1.FileName;
                textBox2.Text = path_plate_1_2;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "CSV Files (*.csv)|*.csv";
            openFileDialog1.Title = "Plate 2-1 File";

            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                path_plate_2_1 = openFileDialog1.FileName;
                textBox3.Text = path_plate_2_1;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "CSV Files (*.csv)|*.csv";
            openFileDialog1.Title = "Plate 2-2 File";

            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                path_plate_2_2 = openFileDialog1.FileName;
                textBox4.Text = path_plate_2_2;
            }
        }
    }
}
