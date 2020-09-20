using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AsteroidLab2
{
    public partial class Size : Form
    {
        //variables to hold x and y values 
        int sizeX= 1000;
        int sizeY=1000;
        public int SizeXP
        {
            //use get to send the state of selected back to the main function 
            get
            {
                return sizeX;
            }
            set//selected stores the value from the data selected
            {              
               sizeX = value;
            }
        }
        public int SizeYP
        {
            //use get to send the state of selected back to the main function 
            get
            {
                return sizeY;
            }
            set
            {
                //selected stores the value from the data selected
               sizeY = value;
            }
        }
        public Size()
        {
            InitializeComponent();
        }

        //if the numeric up and down changes assign the value to x
        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            sizeX=(int) numericUpDown1.Value;
        }

        //if the numeric up and down changes assign the value to y
      private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            sizeY = (int)numericUpDown2.Value;
        }

        //choose the value by saying ok
        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        //cancel the dialog
        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void Size_Load(object sender, EventArgs e)
        {

        }
    }
}
