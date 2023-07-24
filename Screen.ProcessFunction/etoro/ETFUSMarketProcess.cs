using Microsoft.Extensions.Logging;
using Screen.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Screen.ProcessFunction.etoro
{
    public class ETFUSMarketProcess : BaseMarketProcess
    {
        public ETFUSMarketProcess(ILogger log, string yahooTemplate) : base(log, yahooTemplate)
        {
        }

        public override List<ETSymbolEntity> FilterSymbols(List<ETSymbolEntity> symbolList)
        {
            return this._symbolManager.GetEtEtfSymbolList(symbolList).Where(s => s.Exchange.ToLower() != "london").ToList();
        }

        public override string PrepareSymbol(string symbol)
        {
            return symbol.Replace(".US", "");
        }
    }
}
