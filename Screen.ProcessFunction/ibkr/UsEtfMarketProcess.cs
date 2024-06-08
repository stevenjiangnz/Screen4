using Microsoft.Extensions.Logging;
using Screen.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Screen.ProcessFunction.ibkr
{
    public class UsEtfMarketProcess : BaseMarketProcess
    {
        public UsEtfMarketProcess(ILogger log, string yahooTemplate) : base(log, yahooTemplate) { 
        }

        public override List<IbkrEtfSymbolEntity> FilterSymbols(List<IbkrEtfSymbolEntity> symbolList, int batch)
        {
            return this._symbolManager.FilterIbkrUsEtfSymbolList(symbolList, batch, this._processBatch);
        }

        public override string PrepareSymbol(string symbol)
        {
            return symbol;
        }

        public override string GetSymbolListFileName()
        {
            return Environment.GetEnvironmentVariable("US_ETF_LIST_FILE_NAME");
        }
    }
}
