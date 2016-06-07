using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Threading;

namespace Splendid
{
    public class Downloader
    {
        public Guardian MyGuardian { get; set; }
        public string MyTaskTicker { get; set; }
        public string MyTracebackDate { get; set; }
        public string CurrentDate { get; set; }

        public Random rand { get; set; }

        public Downloader(Guardian InputGuardian, string InputTaskTicker, string InputTracebackDate, string InputToday) 
        {
            MyGuardian = InputGuardian;
            MyTaskTicker = InputTaskTicker;
            MyTracebackDate = InputTracebackDate;
            CurrentDate = InputToday;

            rand = new Random();
        }

        public void ThreadPoolCallback(Object threadContext)
        {
            try
            {
                int threadIndex = (int)threadContext;

                DownloadData();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public void InitiateDataFile()
        {
            if (File.Exists(Path.Combine(MyGuardian.MyWorkingDirectory, MyTaskTicker + "TradingData.txt")))
            {
				File.Delete(Path.Combine(MyGuardian.MyWorkingDirectory, MyTaskTicker + "TradingData.txt"));
            }

			if (File.Exists(Path.Combine(MyGuardian.MyWorkingDirectory, MyTaskTicker + "PriceData.txt")))
			{
				File.Delete(Path.Combine(MyGuardian.MyWorkingDirectory, MyTaskTicker + "PriceData.txt"));
			}

			using (StreamWriter writer = new StreamWriter(Path.Combine(MyGuardian.MyWorkingDirectory, MyTaskTicker + "TradingData.txt")))
            {
                writer.WriteLine("Buy/Sell|Transaction Date|Acceptance DateTime|Issuer Name|Issuer Trading Symbol|Reporting Owner Name|Reporting Owner Relationship|Transaction Shares|Price per Share|Total Value|Shares Owned Following Transaction|Form");
            }
        }

        public void DownloadData()
        {
            try
            {
                InitiateDataFile();

                bool SUCCESS_DOWNLOAD = true;
                int PageNumber = 1;
                while (SUCCESS_DOWNLOAD)
                {
                    Thread.Sleep(Convert.ToInt32(rand.NextDouble() * 1000.0));

                    SUCCESS_DOWNLOAD = DownloadOnePageOfTradingData(PageNumber);

                    Thread.Sleep(Convert.ToInt32(rand.NextDouble() * 1000.0));

                    PageNumber++;
                }

				DownloadPriceData();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public bool DownloadOnePageOfTradingData(int InputPageNumber)
        {
            try
            {				
                string DownloadString = "http://insidertrading.org/index.php?sort_by=acceptance_datetime&asc=&symbol=" + MyTaskTicker + "&date_from=" + MyTracebackDate + "&date_to=" + CurrentDate + "&submit=+GO+&page=" + InputPageNumber;
                TimedWebClient wc = new TimedWebClient();
                string page = wc.DownloadString(DownloadString);
//				var httpClient = new HttpClient(new NativeMessageHandler());
//				string page = httpClient.DownloadString(DownloadString);

                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(page);

                List<List<string>> table = doc.DocumentNode.SelectSingleNode("//*[@id='tracker']/tbody")
                .Descendants("tr").Where(tr => tr.Elements("td").Count() > 1)
                .Select(tr => tr.Elements("td").Select(td => td.InnerText.Trim()).ToList())
                .ToList();

                //HashSet<Tuple<string, string, string, string>> InsiderTradingIdentifier = new HashSet<Tuple<string, string, string, string>>();

                //List<List<string>> EntriesToBeAdded = new List<List<string>>();

                //foreach (List<string> ListEntry in table)
                //{
                //    if (!InsiderTradingIdentifier.Contains(new Tuple<string, string, string, string>(ListEntry[0], ListEntry[1], ListEntry[2], ListEntry[3])))
                //    {
                //        EntriesToBeAdded.Add(ListEntry);
                //    }
                //}

                if (table.Count() == 0)
                {
                    return false;
                }
                else
                {
                    using (StreamWriter writer = new StreamWriter(Path.Combine(MyGuardian.MyWorkingDirectory, MyTaskTicker + "TradingData.txt"), true))
                    {
                        foreach (List<string> ListEntry in table)
                        {
                            writer.WriteLine(String.Join("|", ListEntry));
                        }
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

		public void DownloadPriceData()
		{
			try
			{
				string TracebackYear = MyTracebackDate.Split('-')[0];
				string TracebackMonth = Convert.ToString(Convert.ToInt32(MyTracebackDate.Split('-')[1]) - 1).PadLeft(2,'0');
				string TracebackDay = MyTracebackDate.Split('-')[2];

				string CurrentYear = CurrentDate.Split('-')[0];
				string CurrentMonth = Convert.ToString(Convert.ToInt32(CurrentDate.Split('-')[1]) - 1).PadLeft(2,'0');
				string CurrentDay = CurrentDate.Split('-')[2];

				string DownloadString = "http://real-chart.finance.yahoo.com/table.csv?s=" + MyTaskTicker 
					+ "&a=" + TracebackMonth + "&b=" + TracebackDay + "&c=" + TracebackYear + 
					"&d=" + CurrentMonth + "&e=" + CurrentDay + "&f=" + CurrentYear + "&g=d&ignore=.csv";
				TimedWebClient wc = new TimedWebClient();
				string csv = wc.DownloadString(DownloadString).Replace(',', '|');

				using (StreamWriter writer = new StreamWriter(Path.Combine(MyGuardian.MyWorkingDirectory, MyTaskTicker + "PriceData.txt"), true))
				{
					writer.Write(csv);
				}

//				I try to use stream instead of string buffer to read price data, not successful.
//				using (StreamWriter writer = new StreamWriter(Path.Combine(MyGuardian.MyWorkingDirectory, MyTaskTicker + "PriceData.txt"), true))
//				{
//					using(StreamReader reader = new StreamReader(wc.OpenRead(DownloadString)))
//					{
//						string line = reader.ReadLine();
//						writer.Write(line);
//					}
//				}

			}
			catch(Exception ex) 
			{
				throw new Exception (ex.ToString ());
			}
		}
    }
}
