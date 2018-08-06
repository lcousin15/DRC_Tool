using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DRC
{

    public partial class WellPlate_Viewer : Form
    {

        private double interpolate(double val, double y0, double x0, double y1, double x1)
        {
            return (val - x0) * (y1 - y0) / (x1 - x0) + y0;
        }

        private double bases(double val)
        {
            if (val <= -0.75) return 0;
            else if (val <= -0.25) return interpolate(val, 0.0, -0.75, 1.0, -0.25);
            else if (val <= 0.25) return 1.0;
            else if (val <= 0.75) return interpolate(val, 1.0, 0.25, 0.0, 0.75);
            else return 0.0;
        }

        private double red(double gray)
        {
            return bases(gray - 0.5);
        }
        private double green(double gray)
        {
            return bases(gray);
        }
        private double blue(double gray)
        {
            return bases(gray + 0.5);
        }

        private Color get_color_colormap(double val) // beetween 0 and 1
        {
            Console.WriteLine(val.ToString());
            //Console.WriteLine(red((val-0.5)*2.0).ToString());
            //Console.WriteLine(green((val-0.5)*2.0).ToString());
            //Console.WriteLine(blue((val-0.5)*2.0).ToString());

            val = (val-0.5)*2.0;

            Color color = Color.FromArgb((int)(255.0*red(val)), (int)(255.0*green(val)), (int)(255.0*blue(val)));
            return color;
        }

        private List<string> letter = new List<string>(new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P" });
        private List<int> number = new List<int>(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24 });

        private int shift = 40;
        private int offset = 5;

        private Dictionary<string, Dictionary<int, Color>> well_plate_colors = new Dictionary<string, Dictionary<int, Color>>();

        private MainTab _main_tab;

        //private List<Color> colors = new List<Color>();

        public WellPlate_Viewer(MainTab main_tab)
        {
            InitializeComponent();

            _main_tab = main_tab;
            //init_colors();

            initialize_well_plate_colors(Color.DarkBlue);
            draw_well_plate();
        }

        //private void init_colors()
        //{
        //    Type colorType = typeof(System.Drawing.Color);
        //    // We take only static property to avoid properties like Name, IsSystemColor ...
        //    PropertyInfo[] propInfos = colorType.GetProperties(BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.Public);
        //    foreach (PropertyInfo propInfo in propInfos)
        //    {
        //        Color color = (Color)propInfo.GetValue(null, null);
        //        colors.Add(color);

        //    }
        //}

        public void draw_well_plate()
        {
            InitOutput(wellplate_panel);
        }

        private void InitOutput(object output)
        {
            if (output is Control)
            {
                Control c = (Control)output;
                c.Paint += new System.Windows.Forms.PaintEventHandler(wellplate_Paint);
                // Invalidate needed to rise paint event
                c.Invalidate();

            }
        }

        //private Random rnd = new Random();

        private void wellplate_Paint(object sender, PaintEventArgs e)
        {

            for (int row = 0; row < 16; ++row)
            {
                for (int col = 0; col < 24; ++col)
                {
                    //Color randomColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));

                    SolidBrush b = new SolidBrush(well_plate_colors[letter[row]][number[col]]);
                    e.Graphics.FillRectangle(b, shift + col * (30 + offset), shift + row * (20 + offset), 30, 20);
                }
            }

            FontFamily fontFamily = new FontFamily("Arial");
            Font font = new Font(
            fontFamily,
            16,
            FontStyle.Regular,
            GraphicsUnit.Pixel);

            for (int row = 0; row < 16; ++row)
            {
                SolidBrush b = new SolidBrush(Color.White);
                e.Graphics.DrawString(letter[row], font, b, new PointF(15, shift + row * (20 + offset)));
            }

            for (int col = 0; col < 24; ++col)
            {
                SolidBrush b = new SolidBrush(Color.White);
                e.Graphics.DrawString(number[col].ToString(), font, b, new PointF(shift + col * (30 + offset), 15));
            }

        }

        private void initialize_well_plate_colors(Color color)
        {
            for (int row = 0; row < 16; ++row)
            {
                for (int col = 0; col < 24; ++col)
                {
                    if (well_plate_colors.ContainsKey(letter[row]))
                    {
                        well_plate_colors[letter[row]][number[col]] = color;
                    }
                    else
                    {
                        Dictionary<int, Color> temp_dict = new Dictionary<int, Color>();
                        temp_dict[number[col]] = color;
                        well_plate_colors[letter[row]] = temp_dict;
                    }

                }
            }
        }

        private void fill_well_colors(int row, int col, Color color)
        {
            well_plate_colors[letter[row]][number[col]] = color;
        }

        private void get_wells_color()
        {
            //LUT

            Dictionary<string, string> cpd_position = _main_tab.get_ps_template_plate_2();
            Dictionary<string, Color> cpd_colors = new Dictionary<string, Color>();

            HashSet<string> cpd_unique = new HashSet<string>();

            foreach (KeyValuePair<string, string> well_id in cpd_position)
            {
                //string well = well_id.Key;
                string cpd = well_id.Value;
                cpd_unique.Add(cpd);
            }

            int compound_number = cpd_unique.Count();
            int counter_color = 0;

            foreach (KeyValuePair<string, string> well_id in cpd_position)
            {
                string well = well_id.Key;
                string cpd = well_id.Value;

                if (cpd_colors.ContainsKey(cpd)) continue;
                else
                {
                    cpd_colors[cpd] = get_color_colormap((double)counter_color / (double)compound_number);
                    counter_color++;
                }
            }


            for (int row = 0; row < 16; ++row)
            {
                for (int col = 0; col < 24; ++col)
                {
                    string well = "";
                    if (number[col].ToString().Length == 2) well = letter[row] + number[col].ToString();
                    else well = letter[row] + "0" + number[col].ToString();

                    if (cpd_position.ContainsKey(well))
                    {
                        string cpd = cpd_position[well];
                        Color my_color = cpd_colors[cpd];

                        fill_well_colors(row, col, my_color);
                    }
                }
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            get_wells_color();
            draw_well_plate();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
