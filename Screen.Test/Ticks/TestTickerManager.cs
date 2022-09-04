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

            SharedSettings settings = AppGlobal.LoadConfig();


            manager.LoadTickerFromEmail(settings);
        }
    }
}
