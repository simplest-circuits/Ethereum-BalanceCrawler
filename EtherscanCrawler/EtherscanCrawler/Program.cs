using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;

namespace EtherscanCrawler
{
    class Crawler
    {
        private static readonly List<ContentObject> _addrObjList = new();
        private static int _rankCounter = 1;
        static void Main(string[] args)
        {
            int pageID = 1;
            for (; pageID <= 100; pageID++)
            {
                var content = GetContent(pageID);
                DecomposeContent(content);
                Console.WriteLine($"Page {pageID} done");
            }
            SaveToCSV();
            Console.WriteLine($"Finished");
            Console.ReadKey();
        }
        static void DecomposeContent(string content)
        {
            var splittedContent = content.Split("<a href='");
            var listOfContentStrings = new List<string>(splittedContent);
            var newList = new List<List<string>>();

            foreach (var line in listOfContentStrings.ToList())
            {
                if (line.StartsWith("/address/") == false)
                    listOfContentStrings.Remove(line);
                else
                {
                    var s = line.Split("</td>").ToList();
                    newList.Add(s);
                }
            }
            foreach (var item in newList)
            {
                ContentObject addrObj = new();
                for (int i = 0; i < item.Count; i++)
                {
                    item[0] = item[0].Replace("</a>", "");
                    item[0] = item[0].Replace("</span>", "");
                    addrObj.Address = item[0].Replace("</a></span>", "").Remove(0, 53);
                    addrObj.NameTag = item[1].Replace("<td>", "");
                    item[2] = item[2].Replace("<td>", "");
                    if (item[2].Contains("<b>"))
                    {
                        item[2] = item[2].Substring(0, item[2].IndexOf("<") + 1).Replace("<", "");
                        addrObj.Balance = item[2];
                    }
                    item[2] = item[2].Replace(" Ether", "");
                    addrObj.Balance = item[2].Replace(",", "");
                    addrObj.Percentage = item[3].Replace("%", "").Replace("<td>", "").Replace(".", ",");
                    addrObj.TxnCount = item[4].Replace("<td>", "").Replace(",", "");
                }
                addrObj.Rank = _rankCounter++;
                _addrObjList.Add(addrObj);
            }
        }
        static void SaveToCSV()
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\Top10000BalanceAdresses.csv";
            using var writer = new StreamWriter(path);
            var config = new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ";" };
            using var csv = new CsvWriter(writer, config);
            csv.WriteRecords(_addrObjList);
        }
        static string GetContent(int pageID)
        {
            var url = $"https://etherscan.io/accounts/{pageID}?ps=100";

            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(url);
            myRequest.Method = "GET";
            WebResponse myResponse = myRequest.GetResponse();
            StreamReader sr = new(myResponse.GetResponseStream(), System.Text.Encoding.UTF8);
            string result = sr.ReadToEnd();
            sr.Close();
            myResponse.Close();
            return result;
        }
    }
}

