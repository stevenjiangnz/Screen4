using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Screen.Shared;
using Screen.Symbols;
using Screen.Ticks;

namespace Screen.Test.Ticks
{
    public class TestTickerManager
    {
        private SharedSettings _settings = new SharedSettings()
        {
            BasePath = "c:\\data",
            TickerEmailAccount = Environment.GetEnvironmentVariable("Settings__TickerEmailAccount", EnvironmentVariableTarget.Machine),
            TickerEmailPWD = Environment.GetEnvironmentVariable("Settings__TickerEmailPWD", EnvironmentVariableTarget.Machine),
            TickerPath = "tickers",
            SymbolFullFileName = "Fulllist.csv",
            TickerProcessedPath = "tickers_processed"
        };

        [Fact]
        public void TestLoadTickerFromEmail()
        {
            TickerManager manager = new TickerManager(_settings);

            manager.LoadTickerFromEmail();
        }

        [Fact]
        public void TestProcessTickersFromDownload()
        {
            SymbolManager symbolManager = new SymbolManager(_settings);

            var result = symbolManager.LoadFullSymbolList(null);

            TickerManager manager = new TickerManager(_settings);

            manager.ProcessTickersFromDownload(result, 5);
        }

        [Fact]
        public void TestGetTickerFileList()
        {
            TickerManager manager = new TickerManager(_settings);

            var result = manager.GetTickerFileList(7000);

        }

        [Fact]
        public void TestGetTickerListByCode()
        {
            TickerManager manager = new TickerManager(_settings);

            var result = manager.GetTickerListByCode("CCL");

        }
    }
}
