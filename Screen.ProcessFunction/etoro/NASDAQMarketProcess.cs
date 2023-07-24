using Microsoft.Extensions.Logging;
using Screen.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Screen.ProcessFunction.etoro
{
    public class NASDAQMarketProcess : BaseMarketProcess
    {
        public NASDAQMarketProcess(ILogger log, string yahooTemplate) : base(log, yahooTemplate)
        {
        }

        public override List<ETSymbolEntity> FilterSymbols(List<ETSymbolEntity> symbolList)
        {
            return this._symbolManager.GetEtNasdaqSymbolList(symbolList);
        }

        public override string PrepareSymbol(string symbol)
        {
            int dotIndex = symbol.IndexOf('.');

            if (dotIndex != -1)
            {
                symbol = symbol.Substring(0, dotIndex);
            }

            return symbol;
        }
    }
}
