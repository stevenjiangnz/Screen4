using Microsoft.Extensions.Logging;
using Screen.Entity;
using System.Collections.Generic;
using System.Linq;

namespace Screen.ProcessFunction.etoro
{
    public class ETFUKMarketProcess : BaseMarketProcess
    {
        public ETFUKMarketProcess(ILogger log, string yahooTemplate) : base(log, yahooTemplate)
        {
        }

        public override List<ETSymbolEntity> FilterSymbols(List<ETSymbolEntity> symbolList)
        {
            return this._symbolManager.GetEtEtfSymbolList(symbolList).Where(s => s.Exchange.ToLower() == "london").ToList();
        }

        public override string PrepareSymbol(string symbol)
        {
            if (!symbol.EndsWith(".L"))
            {
                symbol += ".L";
            }
            return symbol;
        }
    }
}
