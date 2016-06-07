using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Splendid
{
    public class Guardian
    {
        public string MyWorkingDirectory { get; set; }
        public string MyTickerFile { get; set; }
        public string Today = DateTime.Today.Year.ToString() + "-" + DateTime.Now.ToString("MM") + "-" + DateTime.Now.ToString("dd");

        public Guardian(string InputWorkingDirectory, string InputTickerFile)
        {
            MyWorkingDirectory = InputWorkingDirectory;
            MyTickerFile = InputTickerFile;
        }

        public void DownloadInsiderTradingData()
        {
            try
            {
                List<Tuple<string, string>> Tickers = new List<Tuple<string, string>>();

                using (StreamReader reader = new StreamReader(Path.Combine(MyWorkingDirectory, MyTickerFile)))
                {
                    reader.ReadLine();

                    string line = null;

                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] elem = line.Split('|');
                        Tickers.Add(new Tuple<string, string>(elem[0], elem[1]));
                    }
                }

                int n = Tickers.Count();

                // Construct started tasks
                Task[] tasks = new Task[n];
                for (int i = 0; i < n; i++)
                {
                    Downloader DownloaderArmy = new Downloader(this, Tickers[i].Item1, Tickers[i].Item2, Today);
                    tasks[i] = Task.Factory.StartNew(() =>
                    {
                        DownloaderArmy.DownloadData();
                    });
                }

                // Exceptions thrown by tasks will be propagated to the main thread 
                // while it waits for the tasks. The actual exceptions will be wrapped in AggregateException. 
                try
                {
                    // Wait for all the tasks to finish.
                    Task.WaitAll(tasks);

                    FileAggregator MyFileAggregator = new FileAggregator(this);

                    MyFileAggregator.Aggregate(Tickers);
                }
                catch (AggregateException e)
                {
                    string ErrorMessage = "";
                    for (int j = 0; j < e.InnerExceptions.Count; j++)
                    {
                        if (ErrorMessage.Contains(e.InnerExceptions[j].ToString()))
                        {
                            continue;
                        }
                        ErrorMessage += e.InnerExceptions[j].ToString();
                    }
                    throw new Exception(ErrorMessage);
                    //throw new Exception(e.ToString());
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
    }
}