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
            TickerManager manager = new TickerManager();

            manager.LoadTickerFromEmail(_settings);
        }

        [Fact]
        public void TestProcessTickersFromDownload()
        {
            SymbolManager symbolManager = new SymbolManager();

            var result = symbolManager.LoadFullSymbolList(_settings, null);

            TickerManager manager = new TickerManager();

            manager.ProcessTickersFromDownload(_settings, result, 5);
        }

        [Fact]
        public void TestGetTickerFileList()
        {
            TickerManager manager = new TickerManager();

            var result = manager.GetTickerFileList(_settings, 7000);

        }
    }
}
