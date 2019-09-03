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
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;


namespace FMI_Scheduler
{
    public partial class Form1 : Form
    {
        String[] split; //TODO make a more sensible naming convention -- This stores the splits for the part files line by line
        String[] fileSplit; //TODO make a more sensible naming convention -- This stores the file name splits for order # and left/right
        StreamReader sr; //TODO Programatically aquire all part files in a directory
        string filePath = null; // saves the name of the part file to later split to get order and left/right info
        string filePathName = null;
        string line; //stores the lines from part files
        Dictionary<string, string> myVars; // stores all of the splits from a part file
        

        //TODO Will want to create these directories programatically or even allow for them to be set in options
        string partsDirectory = @"C:\Users\sekon\Documents\FMI-Scheduler\Parts";
        string processedDirectory = @"C:\Users\sekon\Documents\FMI-Scheduler\Parts\Processed\";
        string errorDirectory = @"C:\Users\sekon\Documents\FMI-Scheduler\Parts\Error\";
        string completedDirectory = @"C:\Users\sekon\Documents\FMI-Scheduler\Parts\Completed\";

        public Form1()
        {
            InitializeComponent();
            ReadFile();
            CreateActiveList();
        }


        [Serializable()]
        public class SalesOrder
                {
            public string FilePath { get; set; }
            public string Order { get; set; }
            public string WallMidOutside { get; set; }                
            public string Left_Right { get; set; }
            public string Length { get; set; }
            public string QTY { get; set; }
            public string Material { get; set; }
            public string TZ1 { get; set; }
            public string TZ2 { get; set; }
            public string TZ3 { get; set; }
            public string TY1 { get; set; }
            public string TY2 { get; set; }
            public string TY3 { get; set; }
            public string UploadedTime { get; set; }
            public bool Completed { get; set; }
                    
                }
     
        /*ReadFile will iterate through all part files splitting the lines at the = and storing the two halfs of the line is the dictionary myVars. 
         * For every file an object will be created to store the information for later use */
        private void ReadFile()
        {
            var prtFiles = Directory.EnumerateFiles(partsDirectory, "*.prt");
            if (prtFiles != null)
            {
                foreach (string currentFile in prtFiles)
                {
                    sr = new StreamReader(currentFile);
                    if (sr.BaseStream is FileStream)
                    {
                        filePath = (sr.BaseStream as FileStream).Name;
                        filePathName = Path.GetFileName(filePath);
                        fileSplit = filePathName.Split(new char[] { '-', '.' });
                    }

                    myVars = new Dictionary<string, string>();
                    while ((line = sr.ReadLine()) != null && line.Contains('='))
                    {
                        split = line.Split(new char[] { '=' });
                        myVars.Add(split[0], split[1]);
                    }

                    sr.Close();

                    PopulateSalesOrder();
                }
            }
            
        }
        //TODO need error checking to make sure the .prt file is formatted correctly.
        //create our sale order objects and serialize them so that we dont have to do it everytime the program runs
        private void PopulateSalesOrder()
        {
            string serObjectPath = @"../../../" + fileSplit[1] + fileSplit[2] + ".bin"; //path to save a binary file containing object details

            if (!File.Exists(serObjectPath))//if we already created this object dont do it again
            {
                SalesOrder salesOrder = new SalesOrder
                {
                    FilePath = filePath,
                    Order = fileSplit[1] + "-" + fileSplit[2],
                    WallMidOutside = null,
                    Left_Right = null,
                    Length = myVars["LEN "],
                    QTY = myVars["QTY "],
                    Material = myVars["SEC "],
                    UploadedTime = DateTime.Now.ToString("MM/dd/yyyy hh:mm tt"),
                    Completed = false
                };

                //if we have a tool declared in the part file assign it to the saleOrder variable if not make it null.
                string result = null;
                if (myVars.TryGetValue("TZ1 ", out result)) { salesOrder.TZ1 = result; } else salesOrder.TZ1 = null;
                if (myVars.TryGetValue("TZ2 ", out result)) { salesOrder.TZ2 = result; } else salesOrder.TZ2 = null;
                if (myVars.TryGetValue("TZ3 ", out result)) { salesOrder.TZ3 = result; } else salesOrder.TZ3 = null;
                if (myVars.TryGetValue("TY1 ", out result)) { salesOrder.TY1 = result; } else salesOrder.TY1 = null;
                if (myVars.TryGetValue("TY2 ", out result)) { salesOrder.TY2 = result; } else salesOrder.TY2 = null;
                if (myVars.TryGetValue("TY3 ", out result)) { salesOrder.TY3 = result; } else salesOrder.TY3 = null;

                System.IO.File.Move(salesOrder.FilePath , processedDirectory + filePathName);
                salesOrder.FilePath = processedDirectory;

                Stream saveFileStream = File.Create(serObjectPath);
                BinaryFormatter serializer = new BinaryFormatter();
                serializer.Serialize(saveFileStream, salesOrder);
                saveFileStream.Close();
     
            }          
        }

        private void CreateActiveList()
        {
            ActiveList.AutoResizeColumns = false;

            var binFiles = Directory.EnumerateFiles(@"../../../ ", "*.bin");
            if (binFiles != null)
            {
                foreach (string currentFile in binFiles)
                {
                    Console.WriteLine("Reading Saved File");
                    Stream openFileStream = File.OpenRead(currentFile);
                    BinaryFormatter deserializer = new BinaryFormatter();
                    SalesOrder salesOrder = (SalesOrder)deserializer.Deserialize(openFileStream);
                    
                    
                    if (ActiveList.FindItemWithText(salesOrder.Order) == null)
                    {
                        ListViewItem WO = new ListViewItem(salesOrder.Order);
                        WO.SubItems.Add(salesOrder.WallMidOutside);
                        WO.SubItems.Add(salesOrder.Left_Right);
                        WO.SubItems.Add(salesOrder.Length);
                        WO.SubItems.Add(salesOrder.QTY);
                        WO.SubItems.Add(salesOrder.Material);
                        WO.SubItems.Add(salesOrder.TY1); WO.SubItems.Add(salesOrder.TY2); WO.SubItems.Add(salesOrder.TY3);
                        WO.SubItems.Add(salesOrder.TZ1); WO.SubItems.Add(salesOrder.TZ2); WO.SubItems.Add(salesOrder.TZ3);
                        WO.SubItems.Add(salesOrder.UploadedTime);

                        ActiveList.Items.Add(WO);
                    }
                    openFileStream.Close();
                }
            }
        }


        private void Timer1_Tick(object sender, EventArgs e)
        {
            ReadFile();
        }

        private void PartListTimer_Tick(object sender, EventArgs e)
        {
            CreateActiveList();
        }

        private void ActiveList_ColumnClick(object sender, ColumnClickEventArgs e)
        {

        }
    }
}
