using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ReadDataFromArduino
{
    public partial class closeSave : Form
    {
        Form1 form1;
        public closeSave()
        {
            InitializeComponent();
            
            this.csbtn1.Click += new System.EventHandler(this.csbtn1_Click);
            this.csbtn2.Click += new System.EventHandler(this.csbtn2_Click);
            form1 = new Form1();
            Console.WriteLine("guanbil ");

        }

        private void csbtn1_Click(object sender, EventArgs e)
        {
            form1.SaveNowDataFuc();
            Environment.Exit(0);
        }

        private void csbtn2_Click(object sender, EventArgs e)
        {
            Console.WriteLine("退出程序了哈");
            Environment.Exit(0);

            
        }
    }
}
