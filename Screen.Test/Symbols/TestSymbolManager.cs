using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Screen.Shared;
using Screen.Symbols;
using Xunit;
namespace Screen.Test.Symbols
{
    public class TestSymbolManager
    {
        [Fact]
        public void TestLoadFullSymbolList()
        {
            SymbolManager manager = new SymbolManager();

            SharedSettings settings = new SharedSettings()
            {
                BasePath = "c:\\data",
                SymbolFullFileName = "Fulllist.csv"
            };

            var result = manager.LoadFullSymbolList(settings);
            
            Assert.NotNull(result);

        }
    }
}
