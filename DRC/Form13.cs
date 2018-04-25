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
        Form1 _f1 = new Form1();
        private string selected_cpd;

        //// CancellationTokenSource will hold the CancellationToken struct
        //public readonly System.Threading.CancellationTokenSource _cts = new System.Threading.CancellationTokenSource();

        //// Task will hold the logic
        //private readonly Task _task;

        public Form13(Form1 form, string cpd_id)
        {
            InitializeComponent();
            _f1 = form;
            selected_cpd = cpd_id;
            //// The task will be started on the ThreadPool off the Dispatcher thread
            //_task = Task.Factory.StartNew(() => EventLoop(_cts.Token), _cts.Token);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _f1.imgCpdsViewOption = true;
            _f1.draw_images(selected_cpd);
            this.Visible = false;
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

