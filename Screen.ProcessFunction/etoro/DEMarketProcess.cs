using Microsoft.Extensions.Logging;
using Screen.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Screen.ProcessFunction.etoro
{
    public class DEMarketProcess : BaseMarketProcess
    {
        public DEMarketProcess(ILogger log, string yahooTemplate): base(log, yahooTemplate) { 
        }

        public override List<ETSymbolEntity> FilterSymbols(List<ETSymbolEntity> symbolList)
        {
            return this._symbolManager.GetEtDeSymbolList(symbolList);
        }

        public override string PrepareSymbol(string symbol)
        {
            return symbol;
        }
    }
}
