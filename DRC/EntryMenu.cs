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
    public partial class EntryMenu : Form
    {
        public EntryMenu()
        {
            InitializeComponent();
        }
        public MainTab main_tab; 

        private void button1_Click(object sender, EventArgs e) // Standard loading
        {
            main_tab = new MainTab();
            main_tab.Visible = true;
            //main_tab.fileToolStripMenuItem.Visible = true;
            main_tab.loadToolStripMenuItem_Click(sender, e);
        }

        private void button3_Click(object sender, EventArgs e) // Loading by Plate
        {
            main_tab = new MainTab();
            main_tab.Visible = true;
            //main_tab.fileToolStripMenuItem.Visible = true;
            main_tab.loadWithPlateToolStripMenuItem_Click(sender, e);
        }

        private void button2_Click(object sender, EventArgs e)  // Draw Correlations
        {
            main_tab = new MainTab();
            main_tab.Visible = true;
            main_tab.correlationsToolStripMenuItem2_Click(sender, e);

        }

        private void button9_Click(object sender, EventArgs e)  // View Hits
        {
            main_tab = new MainTab();
            main_tab.Visible = true;
            main_tab.loadHitsToolStripMenuItem_Click(sender, e);
        }

        private void button5_Click(object sender, EventArgs e)  // View Images --> First Load Data
        {
            main_tab = new MainTab();
            main_tab.Visible = true;
            main_tab.loadWithPlateToolStripMenuItem_Click(sender, e);
            //main_tab.checkImagesToolStripMenuItem_Click(sender, e);
        }

        private void button6_Click(object sender, EventArgs e)  // Patient
        {
            main_tab = new MainTab();
            main_tab.Visible = true;
            main_tab.loadPSToolStripMenuItem_Click(sender, e);
        }

        private void button7_Click(object sender, EventArgs e)  // Overlap DRC 1 file (by plate)
        {
            main_tab = new MainTab();
            main_tab.Visible = true;
            main_tab.drawOverlap1FileToolStripMenuItem_Click(sender, e);
        }

        private void button8_Click(object sender, EventArgs e) // Overlap DRC N files
        {
            main_tab = new MainTab();
            main_tab.Visible = true;
            main_tab.dRCTimeLineToolStripMenuItem_Click(sender, e);
        }
    }
}
