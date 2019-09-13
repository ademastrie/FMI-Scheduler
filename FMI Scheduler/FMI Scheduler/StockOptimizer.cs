using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMI_Scheduler
{
    class StockOptimizer
    {
        //for every order that was selected loop through each list to see if a stock list has already been made for that material and punch layout
        //if not create one and add the item to it. 
        public void BuildStock(List<MainMenu.SalesOrder> rawOrderList)
        {
            List<List<MainMenu.SalesOrder>> stockList = new List<List<MainMenu.SalesOrder>>();
            
            foreach (MainMenu.SalesOrder salesOrder in rawOrderList)
            {
                bool isCategoryfound = false;
                
                if (stockList.Count == 0)
                {
                    List<MainMenu.SalesOrder> sortedOrderList = new List<MainMenu.SalesOrder>();
                    sortedOrderList.Add(salesOrder);
                    stockList.Add(sortedOrderList);
                    continue;
                }

                foreach (List<MainMenu.SalesOrder> orderList in stockList)
                {
                    List<MainMenu.SalesOrder> sortedOrderList = new List<MainMenu.SalesOrder>();
                    foreach (MainMenu.SalesOrder order in orderList)
                    {
                        if (order.Material == salesOrder.Material
                             && (order.TY1 == salesOrder.TY1 || order.TY1 == null)
                             && (order.TY2 == salesOrder.TY2 || order.TY2 == null)
                             && (order.TY3 == salesOrder.TY3 || order.TY3 == null)
                             && (order.TZ1 == salesOrder.TZ1 || order.TZ1 == null)
                             && (order.TZ2 == salesOrder.TZ2 || order.TZ2 == null)
                             && (order.TZ3 == salesOrder.TZ3 || order.TZ3 == null))
                        {
                            isCategoryfound = true;
                            orderList.Add(salesOrder);
                            break;
                        }
                    }
                    if (isCategoryfound == true)
                    {
                        break;
                    }

                }
                if (isCategoryfound == false)
                {
                    List<MainMenu.SalesOrder> sortedOrderList = new List<MainMenu.SalesOrder>();
                    sortedOrderList.Add(salesOrder);
                    stockList.Add(sortedOrderList);
                }

            }
            WriteToFile(stockList);
        }


        public void WriteToFile(List<List<MainMenu.SalesOrder>> stockList)
        {
            float totalLength = 0f;
            int stockLength = 36000;
            float cropCut = 0.5f;
            int orderQTY = 0;
            string bin = "0";//TODO give user the option to set this

            string[] stockArray = new string[15];

            foreach (List<MainMenu.SalesOrder> orderList in stockList)
            {
                List <List<String>> orderInfoList = new List<List<String>>();//list used to store all of the orders in the stock to print at the end of function
                totalLength = 0f;
                orderQTY = 0;
                foreach (MainMenu.SalesOrder salesOrder in orderList)//get information from all of the orders stored in the order list
                {
                    string prtFileName = Path.GetFileName(salesOrder.FilePath);
                    List<String> orderInfo = new List<string> { prtFileName, salesOrder.Length.ToString(), salesOrder.QTY.ToString(), bin , salesOrder.Order};
                    orderInfoList.Add(orderInfo);
                    totalLength += salesOrder.Length;
                    orderQTY += 1;


                }
                //this group of functions will store information that is common between all of the sales orders. useful in case a Y punch is used on one angle and the Z is used on the mirror.
                var material = orderList.Select(o => o.Material).FirstOrDefault().ToString().Trim();
                String[] stockSplit = material.Split(new char[] { 'A', 'X' });



                var TY1 = orderList.Select(o => o.TY1).DefaultIfEmpty("").FirstOrDefault();
                var TY2 = orderList.Select(o => o.TY2).DefaultIfEmpty("").FirstOrDefault();
                var TY3 = orderList.Select(o => o.TY3).DefaultIfEmpty("").FirstOrDefault();
                var TZ1 = orderList.Select(o => o.TZ1).DefaultIfEmpty("").FirstOrDefault();
                var TZ2 = orderList.Select(o => o.TZ2).DefaultIfEmpty("").FirstOrDefault();
                var TZ3 = orderList.Select(o => o.TZ3).DefaultIfEmpty("").FirstOrDefault();
                

                //populate the array used to print lines to the stock files
                stockArray[0] = material;
                stockArray[1] = stockSplit[1];
                stockArray[2] = stockSplit[2];
                stockArray[3] = stockSplit[3];
                stockArray[4] = (totalLength + cropCut).ToString();
                stockArray[5] = stockLength.ToString();
                stockArray[6] = cropCut.ToString();
                stockArray[7] = "0";//not entirely sure if this has a real purpose. Always shows as zero in all the stock ive seen.
                stockArray[8] = orderQTY.ToString();
                stockArray[9] = (TY1 == null) ? "" : "TY1 = " + TY1.Trim();
                stockArray[10] = (TY2 == null) ? "" : "TY2 = " + TY2.Trim();
                stockArray[11] = (TY3 == null) ? "" : "TY3 = " + TY3.Trim();
                stockArray[12] = (TZ1 == null) ? "" : "TZ1 = " + TZ1.Trim();
                stockArray[13] = (TZ2 == null) ? "" : "TZ2 = " + TZ2.Trim();
                stockArray[14] = (TZ3 == null) ? "" : "TZ3 = " + TZ3.Trim();

                //TODO may need to reconsider how we name the file. If two files are made inthe same second with the same material they may override each other.
                using (StreamWriter outputFile = new StreamWriter("../../../Stocks/"+ material + DateTime.Now.ToString("mms") + ".stk"))
                {
                    foreach (string line in stockArray)//print the stock info
                    {
                        outputFile.WriteLine(line);
                    }
                    foreach(List<string> list in orderInfoList)//print order information after all of the general stock info
                    {
                        foreach(string line in list)
                        {
                            outputFile.WriteLine(line);
                        }
                    }
                    outputFile.Close();
                }
            }

        }


    }
}
