using Screen.Shared;
using Screen.Symbols;

namespace Screen.Test.Symbols
{
    public class TestSymbolManager
    {
        private SharedSettings _settings = new SharedSettings()
        {
            BasePath = "c:\\data",
            SymbolFullFileName = "Fulllist.csv",
            TickerProcessedPath = "tickers_processed"

        };

        [Fact]
        public void TestLoadFullSymbolList_Full()
        {
            SymbolManager manager = new SymbolManager(_settings);

            var result = manager.LoadFullSymbolList(null);
            
            Assert.NotNull(result);
            Assert.True(result.Count > 1000);
        }

        [Fact]
        public void TestLoadFullSymbolList_Partial()
        {
            SymbolManager manager = new SymbolManager(_settings);

            var result = manager.LoadFullSymbolList(15);

            Assert.NotNull(result);
            Assert.True(result.Count == 15);
        }

    }
}
