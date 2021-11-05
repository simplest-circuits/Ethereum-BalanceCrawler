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
            var splitted = content.Split("<tbody>" + 1).ToList();
            var newcontent = splitted.ElementAt(0);
            var contentElements = new List<string>();
            var contentElementsCleaned = new List<string>();
            var newList = new List<string>();

            contentElements = newcontent.Split("<td>").ToList();

            foreach (var line in contentElements.ToList())
            {
                if (line.StartsWith("\n<!doctype html>"))
                    contentElements.Remove(line);
                else
                    contentElementsCleaned.Add(line.Replace("</td>", ""));

                line.Replace("<td>", "");
                line.Replace("</tr><tr>", "");
            }

            var rowsList = new List<List<string>>();
            while (contentElementsCleaned.Count > 0)
            {
                newList = new List<string>();
                for (int i = 0; i <= 5; i++)
                {
                    newList.Add(contentElementsCleaned[i]);
                }
                contentElementsCleaned.RemoveRange(0, 6);
                rowsList.Add(newList);
            }

            foreach (var line in rowsList)
            {
                ContentObject addrObj = new();

                addrObj.Rank = Convert.ToInt32(line[0]);

                if (line[1].Contains("far fa-file-alt"))
                {
                    addrObj.IsContract = true;
                    line[1] = line[1].Remove(0, 186);
                    line[1] = line[1].Substring(0, line[1].IndexOf("<") + 1);
                    line[1] = line[1].Replace("<", "");
                    addrObj.Address = line[1];
                }
                else
                {
                    line[1] = line[1].Remove(0, 62);
                    line[1] = line[1].Replace("</a>", "");
                    addrObj.Address = line[1];
                }


                addrObj.NameTag = line[2];

                if (line[3].Contains("<"))
                {
                    line[3] = line[3].Substring(0, line[3].IndexOf("<") + 1);
                    line[3] = line[3].Replace("<", "");
                }
                else
                    line[3] = line[3].Replace(" Ether", "");
                line[3] = line[3].Replace(",", "");
                addrObj.Balance = line[3];

                line[4] = line[4].Replace("%", "");
                line[4] = line[4].Replace(".", ",");
                addrObj.Percentage = line[4];

                line[5] = line[5].Substring(0, line[5].IndexOf("<") + 1);
                line[5] = line[5].Replace("<", "");
                addrObj.TxnCount = line[5].Replace(",", "");

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

