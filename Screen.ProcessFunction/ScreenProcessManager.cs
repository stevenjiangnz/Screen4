using Microsoft.Extensions.Logging;
using Screen.Entity;
using Screen.Symbols;
using Screen.Ticks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Screen.ProcessFunction
{
    public class ScreenProcessManager
    {
        private ILogger _log;
        public ScreenProcessManager(ILogger log) { 
            this._log = log;
        }

        public async Task<List<ScanResultEntity>> ProcessWeeklyBull(string storageConnStr, 
            string storageContainer, string symbolListFileName, int top, string yahooUrlTemplate)
        {
            List<ScanResultEntity> scanResult = new List<ScanResultEntity>();

            try
            {
                SymbolManager symbolManager = new SymbolManager(this._log);

                this._log.LogInformation($"About to load symbols, top: {top}");
                var symbolList = await symbolManager.GetSymbolsFromAzureStorage(storageConnStr, storageContainer,
                    symbolListFileName, top);

                this._log.LogInformation($"Symbol retireved {symbolList.Count}");

                // TODO: change it to WaitAll
                foreach ( var symbol in symbolList )
                {
                    var stockResult = await ProcessIndividualStock(yahooUrlTemplate, symbol.Code, "1wk", 60);
                }
            }
            catch (Exception ex)
            {
                this._log.LogError(ex, "Error in ProcessWeeklyBull");
                throw;
            }

            this._log.LogInformation("in process weekly bull");

            return scanResult;
        }

        public async Task<List<ScanResultEntity>> ProcessIndividualStock(string urlTemplate, string symbol,
            string interval, int periodInMonth)
        {
            this._log.LogInformation($"In ProcessIndividualStock for {symbol}");

            List<ScanResultEntity> scanResults = new List<ScanResultEntity>();

            YahooTickManager tickerManager = new YahooTickManager(new Shared.SharedSettings
            {
                YahooUrlTemplate = urlTemplate,
            });

            var tickString = await tickerManager.DownloadYahooTicks(symbol,
                DateTime.Now.Date.AddMonths(-1 * periodInMonth),
                DateTime.Now.Date,
                interval);

            var tickList = tickerManager.ConvertToEntities(symbol, tickString);

            return scanResults;
        }

    }
}
