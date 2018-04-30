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
    public partial class ViewImages_Options_Tab : Form
    {
        MainTab _form1 = new MainTab();
        private string selected_cpd;
        private List<string> list_cpd = new List<string>();
        private bool if_list = false;

        //// CancellationTokenSource will hold the CancellationToken struct
        //public readonly System.Threading.CancellationTokenSource _cts = new System.Threading.CancellationTokenSource();

        //// Task will hold the logic
        //private readonly Task _task;

        public ViewImages_Options_Tab(MainTab form, string cpd_id)
        {
            InitializeComponent();
            _form1 = form;
            selected_cpd = cpd_id;

            if (_form1.cpd_low_th != -1)
            {
                numericUpDown1.Value = _form1.cpd_low_th;
                numericUpDown2.Value = _form1.cpd_high_thr_ch1;
                numericUpDown3.Value = _form1.cpd_high_thr_ch2;
                numericUpDown4.Value = _form1.cpd_high_thr_ch3;
                numericUpDown5.Value = _form1.cpd_high_thr_ch4;

                numericUpDown6.Value = _form1.cpd_img_scale;
                numericUpDown7.Value = _form1.cpd_replicate;

                comboBox2.SelectedIndex = _form1.cpd_color_format;
                comboBox3.SelectedIndex = _form1.cpd_segm_method;
            }

            //// The task will be started on the ThreadPool off the Dispatcher thread
            //_task = Task.Factory.StartNew(() => EventLoop(_cts.Token), _cts.Token);
        }

        public ViewImages_Options_Tab(MainTab form, List<string> cpd_id)
        {
            InitializeComponent();
            _form1 = form;
            list_cpd = cpd_id;

            if (_form1.cpd_low_th != -1)
            {
                numericUpDown1.Value = _form1.cpd_low_th;
                numericUpDown2.Value = _form1.cpd_high_thr_ch1;
                numericUpDown3.Value = _form1.cpd_high_thr_ch2;
                numericUpDown4.Value = _form1.cpd_high_thr_ch3;
                numericUpDown5.Value = _form1.cpd_high_thr_ch4;

                numericUpDown6.Value = _form1.cpd_img_scale;
                numericUpDown7.Value = _form1.cpd_replicate;

                comboBox2.SelectedIndex = _form1.cpd_color_format;
                comboBox3.SelectedIndex = _form1.cpd_segm_method;
            }

            if_list = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //_f1.imgCpdsViewOption = true;

            _form1.cpd_low_th = (int)numericUpDown1.Value;

            _form1.cpd_high_thr_ch1 = (int)numericUpDown2.Value;
            _form1.cpd_high_thr_ch2 = (int)numericUpDown3.Value;
            _form1.cpd_high_thr_ch3 = (int)numericUpDown4.Value;
            _form1.cpd_high_thr_ch4 = (int)numericUpDown5.Value;

            _form1.cpd_img_scale = (int)numericUpDown6.Value;
            _form1.cpd_replicate = (int)numericUpDown7.Value;

            _form1.cpd_color_format = comboBox2.SelectedIndex;
            _form1.cpd_segm_method = comboBox3.SelectedIndex;

            if(if_list==false) _form1.draw_images(selected_cpd);
            else
            {
                foreach(string cpd in list_cpd) _form1.draw_images(cpd);
            }

            this.Visible = false;
            if_list = false;
            //_cts.Cancel();
        }


        //private void EventLoop(System.Threading.CancellationToken token)
        //{
        //    while (!token.IsCancellationRequested)
        //    {
        //        // Do work
        //    }

        //    // This exception will be handled by the Task
        //    // and will not cause the program to crash
        //    token.ThrowIfCancellationRequested();
        //}
    }
}

