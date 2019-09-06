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
using BrightIdeasSoftware;

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
        string dashNum;
        Dictionary<string, string> myVars; // stores all of the splits from a part file
        

        //TODO Will want to create these directories programatically or even allow for them to be set in options
        string partsDirectory = @"../../../Parts";
        string processedDirectory = @"../../../Parts\Processed\";
        string errorDirectory = @"../../../Parts\Error\";
        string completedDirectory = @"../../../Parts\Completed\";

        public Form1()
        {
            InitializeComponent();
            ReadFile();
            ActiveObjList();
            //CreateActiveList();
        }


        [Serializable()]
        public class SalesOrder
                {
            public string FilePath { get; set; }
            public string Order { get; set; }
            public string WallMidOutside { get; set; }                
            public string LeftRight { get; set; }
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
            Directory.CreateDirectory(@"../../../obj\");
            string serObjectPath = @"../../../obj\" + fileSplit[1] + fileSplit[2] + ".bin"; //path to save a binary file containing object details

            //Split the last part of the part file to determine the dash number of the sales order.
            string[] lastSplit = fileSplit[2].Split(new char[] { 'W', 'M', 'O' });
            dashNum = lastSplit[1][0].ToString();
            
            

            if (!File.Exists(serObjectPath))//if we already created this object dont do it again
            {
                SalesOrder salesOrder = new SalesOrder
                {
                    FilePath = filePath,
                    Order = fileSplit[1] + "-" + dashNum,
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

               
                if (fileSplit[2].Contains("W")) { salesOrder.WallMidOutside = "Wall"; }
                if (fileSplit[2].Contains("M")) { salesOrder.WallMidOutside = "Middle"; }
                if (fileSplit[2].Contains("O")) { salesOrder.WallMidOutside = "Outside"; }

                //if there is more than one character in last split 1 we can determine that it is a right hand file.
                salesOrder.LeftRight = (lastSplit[1].Length > 1) ? "Right" : "Left";



                //move the file from the root directory to a folder so we dont have to keep looping through the parts(this may have undesired effets)
                File.Move(salesOrder.FilePath , processedDirectory + filePathName);
                salesOrder.FilePath = processedDirectory + filePathName;//set the filepath to the new location

                Stream saveFileStream = File.Create(serObjectPath);
                BinaryFormatter serializer = new BinaryFormatter();
                serializer.Serialize(saveFileStream, salesOrder);
                saveFileStream.Close();
     
            }          
        }


        private void ActiveObjList()
        {
            var binFiles = Directory.EnumerateFiles(@"../../../obj ", "*.bin");
            if (binFiles != null)
            {
                foreach (string currentFile in binFiles)
                {
                    Stream openFileStream = File.OpenRead(currentFile);
                    BinaryFormatter deserializer = new BinaryFormatter();
                    SalesOrder salesOrder = (SalesOrder)deserializer.Deserialize(openFileStream);

                    if (ActiveList.FindItemWithText(salesOrder.FilePath) == null)
                    {
                        ActiveList.AddObject(salesOrder);
                    }
                    openFileStream.Close();
                }
            }
        }
    
        private void CompletedObjList()
        {
            var binFiles = Directory.EnumerateFiles(@"../../../obj ", "*.bin");
            if (binFiles != null)
            {
                foreach (string currentFile in binFiles)
                {
                    Stream openFileStream = File.OpenRead(currentFile);
                    BinaryFormatter deserializer = new BinaryFormatter();
                    SalesOrder salesOrder = (SalesOrder)deserializer.Deserialize(openFileStream);


                    if (CompletedList.FindItemWithText(salesOrder.FilePath) == null && salesOrder.Completed == true)
                    {
                        CompletedList.AddObject(salesOrder);
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
            ActiveObjList();
           // CreateActiveList();
        }


    }
}
