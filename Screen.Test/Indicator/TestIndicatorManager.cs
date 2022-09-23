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
            IndicatorPath = "indicators"
        };

        [Fact]
        public void TestProcessIndicators()
        {
            TickerManager manager = new TickerManager(_settings);

            var result = manager.GetTickerListByCode("CBA");

            IndicatorManager indManager = new IndicatorManager(_settings);

            indManager.ProcessIndicatorsForCode(result);
        }
    }
}
