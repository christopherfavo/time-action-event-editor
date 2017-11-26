using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DS_TAE_Editor
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Shown(object sender, EventArgs e)
        {
            for (int i = 0; i < Helper.helpers.Count; i++)
            {
                comboBox1.Items.Add(Helper.helpers[i].id + ": " + Helper.helpers[i].description.Split(';')[0]);
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            richTextBox1.Lines = Helper.helpers[comboBox1.SelectedIndex].description.Split(';');
            
        }
    }
}
