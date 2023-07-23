using Microsoft.Extensions.Logging;
using Screen.Entity;
using System.Collections.Generic;
using System.IO;

namespace Screen.ProcessFunction.etoro
{
    public class HKMarketProcess : BaseMarketProcess
    {
        public HKMarketProcess(ILogger log, string yahooTemplate): base(log, yahooTemplate) { 
        }

        public override List<ETSymbolEntity> FilterSymbols(List<ETSymbolEntity> symbolList)
        {
            return this._symbolManager.GetEtHkSymbolList(symbolList);
        }

        public override string PrepareSymbol(string symbol)
        {
            var parts = symbol.Split('.');
            var numberPart = int.Parse(parts[0]).ToString("D4");  // The 'D4' specifies to use 4 digits, adding leading zeros if necessary
            var newSymbol = $"{numberPart}.{parts[1]}";

            return newSymbol;
        }
    }
}
