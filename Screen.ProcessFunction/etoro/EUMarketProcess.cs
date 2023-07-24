using Microsoft.Extensions.Logging;
using Screen.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Screen.ProcessFunction.etoro
{
    public class EUMarketProcess : BaseMarketProcess
    {
        public EUMarketProcess(ILogger log, string yahooTemplate): base(log, yahooTemplate) { 
        }

        public override List<ETSymbolEntity> FilterSymbols(List<ETSymbolEntity> symbolList)
        {
            return this._symbolManager.GetEtEuSymbolList(symbolList);
        }

        public override string PrepareSymbol(string symbol)
        {
            symbol = symbol.Replace(".ZU", ".SW");
            symbol = symbol.Replace(".NV", ".AS");
            return symbol;
        }
    }
}
