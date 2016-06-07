using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Splendid
{
    class FileAggregator
    {
        public Guardian MyGuardian { get; set; }

        public FileAggregator (Guardian InputGuardian)
        {
            MyGuardian = InputGuardian;
        }

        public void Aggregate (List<Tuple<string, string>> InputTickers)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(Path.Combine(MyGuardian.MyWorkingDirectory, "TradingData.txt")))
                {
                    writer.WriteLine("Ticker|Buy/Sell|Transaction Date|Acceptance DateTime|Issuer Name|Issuer Trading Symbol|Reporting Owner Name|Reporting Owner Relationship|Transaction Shares|Price per Share|Total Value|Shares Owned Following Transaction|Form");

                    foreach (Tuple<string, string> Ticker in InputTickers)
                    {
						if(!File.Exists(Path.Combine(MyGuardian.MyWorkingDirectory, Ticker.Item1 + "TradingData.txt")))
						{
							continue;
						}
						using (StreamReader reader = new StreamReader(Path.Combine(MyGuardian.MyWorkingDirectory, Ticker.Item1 + "TradingData.txt")))
                        {
                            string line;
                            reader.ReadLine();
                            while ((line = reader.ReadLine())!=null)
                            {
                                writer.WriteLine(Ticker.Item1 + "|" + line);
                            }
                        }

						File.Delete(Path.Combine(MyGuardian.MyWorkingDirectory, Ticker.Item1 + "TradingData.txt"));
                    }
                }

				using(StreamWriter writer=new StreamWriter(Path.Combine(MyGuardian.MyWorkingDirectory, "PriceData.txt")))
				{
					writer.WriteLine("Ticker|Date|Open|High|Low|Close|Volume|Adj Close");
					foreach(Tuple<string, string> Ticker in InputTickers)
					{
						if(!File.Exists(Path.Combine(MyGuardian.MyWorkingDirectory, Ticker.Item1 + "PriceData.txt")))
						{
							continue;
						}
						using(StreamReader reader=new StreamReader(Path.Combine(MyGuardian.MyWorkingDirectory, Ticker.Item1 + "PriceData.txt")))
						{
							reader.ReadLine();

							string line;

							while((line=reader.ReadLine())!=null)
							{
								writer.WriteLine(Ticker.Item1 + "|" + line);
							}
						}

						File.Delete(Path.Combine(MyGuardian.MyWorkingDirectory, Ticker.Item1 + "PriceData.txt"));
					}
				}
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally
            { }
        }
    }
}
