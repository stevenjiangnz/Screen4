using Screen.Shared;
using Screen.Symbols;

namespace Screen.Test.Symbols
{
    public class TestSymbolManager
    {
        [Fact]
        public void TestLoadFullSymbolList_Full()
        {
            SymbolManager manager = new SymbolManager();

            SharedSettings settings = new SharedSettings()
            {
                BasePath = "c:\\data",
                SymbolFullFileName = "Fulllist.csv"
            };

            var result = manager.LoadFullSymbolList(settings, null);
            
            Assert.NotNull(result);
            Assert.True(result.Count > 1000);
        }

        [Fact]
        public void TestLoadFullSymbolList_Partial()
        {
            SymbolManager manager = new SymbolManager();

            SharedSettings settings = new SharedSettings()
            {
                BasePath = "c:\\data",
                SymbolFullFileName = "Fulllist.csv"
            };

            var result = manager.LoadFullSymbolList(settings, 15);

            Assert.NotNull(result);
            Assert.True(result.Count == 15);
        }

    }
}
