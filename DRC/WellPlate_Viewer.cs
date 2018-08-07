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
            //Console.WriteLine(val.ToString());
            //Console.WriteLine(red((val-0.5)*2.0).ToString());
            //Console.WriteLine(green((val-0.5)*2.0).ToString());
            //Console.WriteLine(blue((val-0.5)*2.0).ToString());

            val = (val - 0.5) * 2.0;

            Color color = Color.FromArgb((int)(255.0 * red(val)), (int)(255.0 * green(val)), (int)(255.0 * blue(val)));
            return color;
        }

        private List<string> letter = new List<string>(new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P" });
        private List<int> number = new List<int>(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24 });

        private int shift = 40;
        private int offset = 5;

        private Dictionary<string, Dictionary<int, Color>> well_plate_colors = new Dictionary<string, Dictionary<int, Color>>();

        private MainTab _main_tab;

        private Dictionary<string, Dictionary<string, Dictionary<string, double>>> cpd_values_by_plate_well 
            = new Dictionary<string, Dictionary<string, Dictionary<string, double>>>();

        private Dictionary<string, Dictionary<string, double>> value_max_per_plate_descriptor = new Dictionary<string, Dictionary<string, double>>();
        private Dictionary<string, Dictionary<string, double>> value_min_per_plate_descriptor = new Dictionary<string, Dictionary<string, double>>();


        public WellPlate_Viewer(MainTab main_tab)
        {
            InitializeComponent();

            _main_tab = main_tab;

            initialize_well_plate_colors(Color.DarkBlue);
            draw_well_plate();

            get_cpd_values_colormap();
            draw_well_plate();
        }


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
                    cpd_colors[cpd] = get_color_colormap((double)counter_color / (double)compound_number); // between 0 and 1.
                    counter_color++;
                }
            }

        }

        private void draw_well_plate_colors(Dictionary<string, Color> cpd_colors, Dictionary<string, string> cpd_position)
        {
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

        private void draw_well_plate_colors(Dictionary<string, Color> cpd_colors) // well, color
        {
            for (int row = 0; row < 16; ++row)
            {
                for (int col = 0; col < 24; ++col)
                {
                    string well = "";
                    if (number[col].ToString().Length == 2) well = letter[row] + number[col].ToString();
                    else well = letter[row] + "0" + number[col].ToString();

                    if (cpd_colors.ContainsKey(well))
                    {
                        Color my_color = cpd_colors[well];
                        fill_well_colors(row, col, my_color);
                    }
                }
            }
        }

        private void get_cpd_values_colormap()
        {
            cpd_values_by_plate_well = new Dictionary<string, Dictionary<string, Dictionary<string, double>>>();

            // plate, cpd_id, descriptor, well, value

            Dictionary<string, List<Chart_DRC>> list_cpd_chart = _main_tab.get_descriptors_chart();
            value_max_per_plate_descriptor = new Dictionary<string, Dictionary<string, double>>();
            value_min_per_plate_descriptor = new Dictionary<string, Dictionary<string, double>>();

            //Dictionary<string, Dictionary<string, double>> cpd_values_by_well = new Dictionary<string, Dictionary<string, double>>();

            foreach (KeyValuePair<string, List<Chart_DRC>> elem in list_cpd_chart)
            {
                string cpd_id = elem.Key;
                List<Chart_DRC> charts = elem.Value;

                string plate = "";

                foreach (Chart_DRC current_chart in charts)
                {
                    string descriptor_name = current_chart.get_Descriptor_Name();
                    List<DataGridViewRow> raw_data_descriptor = current_chart.get_Raw_Data();

                    //Dictionary<string, double> descritptor_wells_values = new Dictionary<string, double>();

                    foreach (DataGridViewRow row in raw_data_descriptor)
                    {
                        string well = row.Cells["Well"].Value.ToString();
                        double value = Double.Parse(row.Cells[descriptor_name].Value.ToString());
                        plate = row.Cells["Plate"].Value.ToString();

                        if (value_max_per_plate_descriptor.ContainsKey(plate))
                        {
                            if (value_max_per_plate_descriptor[plate].ContainsKey(descriptor_name))
                            {
                                if (value > value_max_per_plate_descriptor[plate][descriptor_name]) value_max_per_plate_descriptor[plate][descriptor_name] = value;
                            }
                            else
                            {
                                value_max_per_plate_descriptor[plate][descriptor_name] = value;
                            }

                        }
                        else
                        {
                            Dictionary<string, double> temp = new Dictionary<string, double>();
                            temp[descriptor_name] = value;
                            value_max_per_plate_descriptor[plate] = temp;
                        }

                        if (value_min_per_plate_descriptor.ContainsKey(plate))
                        {
                            if (value_min_per_plate_descriptor[plate].ContainsKey(descriptor_name))
                            {
                                if (value < value_min_per_plate_descriptor[plate][descriptor_name]) value_min_per_plate_descriptor[plate][descriptor_name] = value;
                            }
                            else
                            {
                                value_min_per_plate_descriptor[plate][descriptor_name] = value;
                            }

                        }
                        else
                        {
                            Dictionary<string, double> temp = new Dictionary<string, double>();
                            temp[descriptor_name] = value;
                            value_min_per_plate_descriptor[plate] = temp;
                        }

                        //descritptor_wells_values[well] = value;

                        // <well, value>
                        // decriptor_name
                        // plate

                        if (cpd_values_by_plate_well.ContainsKey(plate))
                        {
                            if (cpd_values_by_plate_well[plate].ContainsKey(descriptor_name))
                            {
                                cpd_values_by_plate_well[plate][descriptor_name][well] = value;
                            }
                            else
                            {
                                Dictionary<string, double> temp_dict = new Dictionary<string, double>();
                                temp_dict[well] = value;
                                cpd_values_by_plate_well[plate][descriptor_name] = temp_dict;
                            }
                        }
                        else
                        {
                            Dictionary<string, double> temp_dict = new Dictionary<string, double>();
                            temp_dict[well] = value;

                            Dictionary<string, Dictionary<string, double>> descriptor_values_by_well = new Dictionary<string, Dictionary<string, double>>();

                            descriptor_values_by_well[descriptor_name] = temp_dict;
                            cpd_values_by_plate_well[plate] = descriptor_values_by_well;
                        }
                    }
                }
            }

            
            foreach(KeyValuePair<string, Dictionary<string, Dictionary<string, double>>> elem in cpd_values_by_plate_well)
            {
                string plate_name = elem.Key;

                if (!comboBox2.Items.Contains(plate_name)) comboBox2.Items.Add(plate_name);
                comboBox2.SelectedItem = plate_name;


                Dictionary<string, Dictionary<string, double>> plate = elem.Value;

                foreach(KeyValuePair<string, Dictionary<string, double>> descriptor_well_value in plate)
                {
                    string descriptor = descriptor_well_value.Key;

                    if(!comboBox1.Items.Contains(descriptor)) comboBox1.Items.Add(descriptor);
                    comboBox1.SelectedItem = descriptor;

                    double max_value_current_plate_descriptor = value_max_per_plate_descriptor[plate_name][descriptor];
                    double min_value_current_plate_descriptor = value_min_per_plate_descriptor[plate_name][descriptor];

                    Dictionary<string, double> well_value = descriptor_well_value.Value;

                    Dictionary<string, double> well_normalized_value = new Dictionary<string, double>();

                    foreach(KeyValuePair<string, double> value_by_well in well_value)
                    {
                        string well = value_by_well.Key;

                        double normalized_value = (value_by_well.Value - min_value_current_plate_descriptor) / (max_value_current_plate_descriptor - min_value_current_plate_descriptor);
                        well_normalized_value[well] = normalized_value;

                        Color my_color = get_color_colormap(normalized_value);

                        byte[] asciiBytes = Encoding.ASCII.GetBytes(well.Substring(0,1));
                        int row = asciiBytes[0]-65;
                        int col = int.Parse(well.Substring(1, 2))-1;

                        fill_well_colors(row, col, my_color);
                    }
                }
            }
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            initialize_well_plate_colors(Color.DarkBlue);

            foreach (KeyValuePair<string, Dictionary<string, Dictionary<string, double>>> elem in cpd_values_by_plate_well)
            {
                string plate_name = elem.Key;

                if (comboBox2.SelectedItem.ToString() == plate_name)
                {
                    Dictionary<string, Dictionary<string, double>> plate = elem.Value;

                    foreach (KeyValuePair<string, Dictionary<string, double>> descriptor_well_value in plate)
                    {
                        string descriptor = descriptor_well_value.Key;

                        if (comboBox1.SelectedItem.ToString() == descriptor)
                        {
                            double max_value_current_plate_descriptor = value_max_per_plate_descriptor[plate_name][descriptor];
                            double min_value_current_plate_descriptor = value_min_per_plate_descriptor[plate_name][descriptor];

                            Dictionary<string, double> well_value = descriptor_well_value.Value;

                            Dictionary<string, double> well_normalized_value = new Dictionary<string, double>();

                            foreach (KeyValuePair<string, double> value_by_well in well_value)
                            {
                                string well = value_by_well.Key;

                                double normalized_value = (value_by_well.Value - min_value_current_plate_descriptor) / (max_value_current_plate_descriptor - min_value_current_plate_descriptor);
                                well_normalized_value[well] = normalized_value;

                                Color my_color = get_color_colormap(normalized_value);

                                byte[] asciiBytes = Encoding.ASCII.GetBytes(well.Substring(0, 1));
                                int row = asciiBytes[0] - 65;
                                int col = int.Parse(well.Substring(1, 2)) - 1;

                                fill_well_colors(row, col, my_color);
                            }
                        }
                    }
                }
            }

            draw_well_plate();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
