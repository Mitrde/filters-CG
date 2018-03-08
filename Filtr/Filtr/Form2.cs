using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Filtr
{
    public partial class Form2 : Form
    {
        private MyDelegate d;
        public Form2(MyDelegate sender)
        {
            InitializeComponent();
            d = sender;
        }
        protected bool[,] elem = new bool[3, 3];
        public Form2()
        {
            InitializeComponent();  
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (checkBox9.CheckState==CheckState.Checked)
            { elem[2, 2] = true; }
            if (checkBox6.CheckState == CheckState.Checked)
            { elem[2, 1] = true; }
            if (checkBox3.CheckState == CheckState.Checked)
            { elem[2, 0] = true; }
            if (checkBox8.CheckState == CheckState.Checked)
            { elem[1, 2] = true; }
            if (checkBox5.CheckState == CheckState.Checked)
            { elem[1, 1] = true; }
            if (checkBox2.CheckState == CheckState.Checked)
            { elem[1, 0] = true; }
            if (checkBox7.CheckState == CheckState.Checked)
            { elem[0, 2] = true; }
            if (checkBox4.CheckState == CheckState.Checked)
            { elem[0, 1] = true; }
            if (checkBox1.CheckState == CheckState.Checked)
            { elem[0, 0] = true; }
            d(elem);
            this.Close();
        }

        
       

        
    }
}
