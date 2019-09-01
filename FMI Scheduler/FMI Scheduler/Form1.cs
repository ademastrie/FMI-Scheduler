using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;


namespace FMI_Scheduler
{
    public partial class Form1 : Form
    {
        String[] split;
        String[] fileSplit;
        StreamReader sr = File.OpenText(@"C:\FMI\F3589 Angle Line\Parts\8152\001-007707-1M1.prt");
        string filePath = null;
        string line;
        Dictionary<string, string> myVars;

        public Form1()
        {
            InitializeComponent();
            readFile();
        }


        private void readFile()
        {
            if(sr.BaseStream is FileStream)
            {
                filePath = (sr.BaseStream as FileStream).Name;
                fileSplit = filePath.Split(new char[] { '-' });
            }

            myVars = new Dictionary<string,string>();
            while ((line = sr.ReadLine()) != null && line.Contains('='))
            {
                split = line.Split(new char[] { '=' });
                myVars.Add(split[0], split[1]);
            }

            Console.WriteLine("Split 0 is: " + myVars["LEN "]);
            Console.WriteLine("Split 1 is: " + myVars["QTY "]);
            Console.WriteLine("Order Number is: " + fileSplit[1]);
        }

        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            
            
        }
    }
}
