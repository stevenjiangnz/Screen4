using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Screen.Entity;
using Screen.Shared;
using Screen.Ticks;

namespace Screen.Test.Ticks
{
    public class TestYahooTickManager
    {
        private SharedSettings _settings = new SharedSettings()
        {
            BasePath = "c:\\data",
            TickerEmailAccount = Environment.GetEnvironmentVariable("Settings__TickerEmailAccount", EnvironmentVariableTarget.Machine),
            TickerEmailPWD = Environment.GetEnvironmentVariable("Settings__TickerEmailPWD", EnvironmentVariableTarget.Machine),
            TickerPath = "tickers",
            SymbolFullFileName = "Fulllist.csv",
            TickerProcessedPath = "tickers_processed",
            YahooUrlTemplate = "https://query1.finance.yahoo.com/v7/finance/download/{0}?period1={1}&period2={2}&interval=1d&events=history&includeAdjustedClose=true",
            YahooFilePath = @"c:\data\yahootickers"
        };

        [Fact]
        public void TestDownloadTicks()
        {
            var manager = new YahooTcikManager(this._settings);
            DateTime start = DateTime.Today.AddMonths(-12);
            DateTime end = DateTime.Today;
            manager.DownloadTicks("SUN.AX", start, end).Wait();
        }

        [Fact]
        public void TestLoadTicksIntoEntities()
        {
            string tickPath = @"C:\data\yahootickers\sun.ax";

            string content = File.ReadAllText(tickPath);
            string symbol = "sun.ax";
            string[] lines = content.Split(new[] { '\n' }, StringSplitOptions.None);

            foreach (string line in lines)
            {
                if (!line.Contains("Date,"))
                {
                    // Process each line here
                    var entity = new TickerEntity(symbol, line);
                    Console.WriteLine(entity.ToString());
                }
            }

        }
    }
}
