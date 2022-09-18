using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Screen.Shared;
using Screen.Ticks;

namespace Screen.Test.Ticks
{
    public class TestTickerManager
    {
        [Fact]
        public void TestLoadTickerFromEmail()
        {
            TickerManager manager = new TickerManager();


            SharedSettings settings = new SharedSettings()
            {
                BasePath = "c:\\data",
                TickerEmailAccount = Environment.GetEnvironmentVariable("Settings__TickerEmailAccount", EnvironmentVariableTarget.Machine),
                TickerEmailPWD = Environment.GetEnvironmentVariable("Settings__TickerEmailPWD", EnvironmentVariableTarget.Machine),
                TickerPath = "tickers",
                SymbolFullFileName = "Fulllist.csv"
            };


            manager.LoadTickerFromEmail(settings);
        }
    }
}
