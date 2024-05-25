using Microsoft.Extensions.Logging;
using Screen.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Screen.ProcessFunction.etoro
{
    public class UKMarketProcess : BaseMarketProcess
    {
        public UKMarketProcess(ILogger log, string yahooTemplate): base(log, yahooTemplate) { 
        }

        public override List<ETSymbolEntity> FilterSymbols(List<ETSymbolEntity> symbolList)
        {
            return this._symbolManager.GetEtUkSymbolList(symbolList);
        }

        public override string PrepareSymbol(string symbol)
        {
            // If symbol already ends with ".L", return as it is
            if (symbol.EndsWith(".L"))
            {
                return symbol;
            }

            // If the string contains ".L." somewhere in the middle, 
            // take the part before this occurrence and append ".L"
            if (symbol.Contains(".L."))
            {
                int index = symbol.IndexOf(".L.");
                return symbol.Substring(0, index + 2);
            }

            // If there is no ".L" in the string, simply append ".L" at the end
            return symbol + ".L";
        }
    }
}
