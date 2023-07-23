using Screen.Indicator;
using Screen.Shared;
using Screen.Ticks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Screen.Test.Indicator
{
    public class TestIndicatorManager
    {
        private SharedSettings _settings = new SharedSettings()
        {
            BasePath = "c:\\data",
            TickerEmailAccount = Environment.GetEnvironmentVariable("Settings__TickerEmailAccount", EnvironmentVariableTarget.Machine),
            TickerEmailPWD = Environment.GetEnvironmentVariable("Settings__TickerEmailPWD", EnvironmentVariableTarget.Machine),
            TickerPath = "tickers",
            SymbolFullFileName = "Fulllist.csv",
            TickerProcessedPath = "tickers_processed",
            IndicatorPath = "indicators",
            YahooUrlTemplate = "https://query1.finance.yahoo.com/v7/finance/download/{0}?period1={1}&period2={2}&interval=1d&events=history&includeAdjustedClose=true",
            YahooFilePath = @"c:\data\yahootickers"
        };

        [Fact]
        public void TestProcessIndicators()
        {
            TickerManager manager = new TickerManager(_settings, null);
            string code = "CBA";

            var result = manager.GetTickerListByCode(code);

            IndicatorManager indManager = new IndicatorManager(_settings);

            indManager.ProcessIndicatorsForCode(code, result);
        }

        [Fact]
        public async void TestProcessTaooIndicators()
        {
            string symbol = "SUN.AX";
            var manager = new YahooTickManager(this._settings, null);
            DateTime start = DateTime.Today.AddMonths(-200);
            DateTime end = DateTime.Today;
            var tickString = await manager.DownloadYahooTicks("SUN.AX", start, end);

            var tickerEntityList = manager.ConvertToEntities(symbol, tickString);
            IndicatorManager indManager = new IndicatorManager(_settings);

            indManager.ProcessIndicatorsForCode(symbol, tickerEntityList);

        }
    }
}
