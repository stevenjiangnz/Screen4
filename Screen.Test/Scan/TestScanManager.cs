using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Screen.Entity;
using Screen.Indicator;
using Screen.Scan;
using Screen.Shared;
using Screen.Symbols;
using Screen.Ticks;
using Screen.Utils;

namespace Screen.Test.Scan
{
    public class TestScanManager
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
            YahooFilePath = @"c:\data\yahootickers",
            IndicatorPath = "indicators"
        };

        [Fact]
        public void TestProcessScan()
        {
            string IndPath = @"C:\data\indicators\SUN.AX_indicator.json";

            string indicatorJsonString = File.ReadAllText(IndPath);

            IList<IndicatorEntity> IndList = ObjectHelper.FromJsonString<IList<IndicatorEntity>>(indicatorJsonString);

            ScanManager manager = new ScanManager(null);

            var result = manager.ProcessScan(IndList).Where(m=>m.ADX_INTO_BULL == true).ToList();

            var resultString = result.ToJsonString(true);
        }


        [Fact]
        public async Task TestProcessAllScan()
        {
            SymbolManager scanManager = new SymbolManager(_settings);

            var symbolList = scanManager.LoadFullSymbolList(300);
            var tickerManager = new YahooTickManager(this._settings, null);
            IndicatorManager indManager = new IndicatorManager(_settings);

            foreach (var symbol in symbolList)
            {
                try
                {

                        string symbolString = symbol.Code + ".AX";
                        DateTime start = DateTime.Today.AddMonths(-12);
                        DateTime end = DateTime.Today;
                        var tickerContent = await tickerManager.DownloadYahooTicks(symbolString, start, end);
                        var tickerEntityList = tickerManager.ConvertToEntities(symbolString, tickerContent);
                        indManager.ProcessIndicatorsForCode(symbolString, tickerEntityList);



                }
                catch (Exception ex)
                {
                    Console.WriteLine(symbol.Code + "   " + ex.ToString());
                }
            }

            Console.WriteLine("process done");
        }
    }
}
