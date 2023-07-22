using Microsoft.Extensions.Logging;
using Screen.Indicator;
using Screen.Scan;
using Screen.Symbols;
using Screen.Ticks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Screen.ProcessFunction.etoro
{
    public class ETProcessManager
    {
        private ILogger _logger;
        private YahooTickManager _tickerManager;
        private IndicatorManager _indicatorManager;
        private ETSymbolManager _symbolManager;
        private ScanManager _scanManager;
        public ETProcessManager(ILogger log, string yahooTemplate)
        {
            this._logger = log;

            this._tickerManager = new YahooTickManager(new Shared.SharedSettings
            {
                YahooUrlTemplate = yahooTemplate,
            });

            this._indicatorManager = new IndicatorManager();
            this._symbolManager = new ETSymbolManager(log);
            this._scanManager = new ScanManager(log);
        }

        public async Task ProcessEtMarket (string market)
        {
            if (string.IsNullOrEmpty(market))
            {
                throw new ArgumentNullException($"market can not be empty");
            }

            switch (market)
            {
                case "asx":
                    break;
                default:
                    throw new NotImplementedException($"market {market} is not implemented.");
            }
        }

        public void ProcessMarketAsx () {
                    
        }
    }
}
