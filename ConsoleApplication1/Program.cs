using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;



namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            //greeding
            //Console.Write("Input your string:");
            ////local variable get user's input
            //string input = Console.ReadLine();
            //////validate the user's input make sure it is not empty
            ////if (!string.IsNullOrEmpty(input))
            ////{
            ////    //using Reverse() method to get reversed string
            ////    Console.WriteLine(input.Reverse().ToArray());
            ////}

            //Console.WriteLine(ReversedString(input));
            while (IsTabelExists())
            {
                string output = ConvertDataTabletoString();
                //Console.WriteLine("OUT: \n"+output);
                string currentLocation = System.AppDomain.CurrentDomain.BaseDirectory;
                //write string to file
                System.IO.File.WriteAllText(currentLocation + @"\repair.json", output);
                Console.WriteLine("Success!");
                Delay();

            }
            
            
        }
        public static void Delay()
        {
            Thread.Sleep(5*60*1000);
        }

        //method for verify the table exists
        private static bool IsTabelExists()
        {
            bool exists;

            try
            {
                int count;
                using (SqlConnection thisConnection = new SqlConnection("Data Source=ReportDB;Initial Catalog=EasyWIPDB;Integrated Security=True"))
                {
                    using (SqlCommand cmdCount = new SqlCommand("select case when exists((select * from EasyWIPDB.information_schema.tables where table_name = 'tblRepair')) then 1 else 0 end", thisConnection))
                    {
                        thisConnection.Open();
                        cmdCount.CommandTimeout = 0;
                        count = (int)cmdCount.ExecuteScalar();
                    }
                }

                if (count == 1)
                {
                    exists = true;
                }
                else
                {
                    exists = false;
                }
            }
            catch
            {
                exists = false;
            }

            return exists;
        }
        //public static string ReversedString(string input)
        //{
        //    //validate the string make sure its not noll or empty
        //    if (!string.IsNullOrEmpty(input))
        //    {
        //        //local variable taht covert the string to a char array
        //        char[] reversed = input.ToCharArray();
        //        //use the Array.Reverse method to reverse the string
        //        Array.Reverse(reversed);
        //        //return the result
        //        return new string(reversed);
        //    }
        //    else
        //    {
        //        return "Northing typed.";
        //    }
        //}
        // This method is used to convert datatable to json string
        public static string ConvertDataTabletoString()
        {
            string query = @"select RefNumber,DealerID,Manufacturer,FuturetelLocation,Warranty,[Status],SVP,
                    LastTechnician,FORMAT(DateIn,'yyyy-MM-dd HH:mm:ss') as DateIn,FORMAT(DateFinish,'yyyy-MM-dd HH:mm:ss') as DateFinish,DATEDIFF(day, DateIn, convert(date, GETDATE())) as AGING from tblRepair
                    where DateIn > convert(date,DATEADD(day,-180,GETDATE()))  
                    and (DealerID != '7398' and DealerID != '7430'  and DealerID != '7432' and DealerID != '7481' 
                                and DealerID != '7482' and DealerID != '7498' and DealerID != '7550' and DealerID != '7552' 
                                and DealerID != '7551' and DealerID != '7595') 
                    and (SVP != 'KCC' AND SVP != 'TCC' AND SVP != 'KXREPAIR' AND SVP != 'TXREPAIR')";

            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection("Data Source=ESDB-TST;Initial Catalog=EasyReportDB;Integrated Security=True"))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                    //JsonSerializer serializer = new JsonSerializer();
                    List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
                    Dictionary<string, object> row;
                    Console.WriteLine("Rows: " + dt.Rows.Count);
                    foreach (DataRow dr in dt.Rows)
                    {
                        row = new Dictionary<string, object>();
                        foreach (DataColumn col in dt.Columns)
                        {
                            row.Add(col.ColumnName, dr[col]);
                        }
                        rows.Add(row);
                    }
                    return JsonConvert.SerializeObject(rows);
                }
            }
        }

    }
}
