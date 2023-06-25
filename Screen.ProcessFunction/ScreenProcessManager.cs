﻿using Google.Apis.Drive.v3;
using Microsoft.Extensions.Logging;
using Screen.Entity;
using Screen.Indicator;
using Screen.Scan;
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
        private YahooTickManager _tickerManager;
        private IndicatorManager _indicatorManager;
        private ScanManager _scanManager;
        public ScreenProcessManager(ILogger log, string yahooTemplate)
        {
            this._log = log;

            this._tickerManager = new YahooTickManager(new Shared.SharedSettings
            {
                YahooUrlTemplate = yahooTemplate,
            });

            this._indicatorManager = new IndicatorManager();

            this._scanManager = new ScanManager(this._log);
        }

        #region Google based
        public async Task<List<ScanResultEntity>> ProcessWeeklyBull(DriveService service,
            string rootId,
            string symbolListFileName, 
            int top, 
            string yahooUrlTemplate)
        {
            this._log.LogInformation("in ProcessWeeklyBull");

            List<ScanResultEntity> scanResult = new List<ScanResultEntity>();

            SymbolManager symbolManager = new SymbolManager(this._log);

            var symbolList = await symbolManager.GetSymbolsFromGoogleStorage(service, rootId, symbolListFileName, top);

            this._log.LogInformation($"After get Symbol, returned {symbolList.Count}");

            foreach ( var symbol in symbolList )
            {
                var stockResult = await this.ProcessIndividualStock(yahooUrlTemplate, symbol.Code, "1wk", 60);
                
                if(stockResult != null && stockResult.Count > 0)
                {
                    var s = stockResult[0];

                    if (s.ADX_CROSS_BULL.GetValueOrDefault() || s.ADX_INTO_BULL.GetValueOrDefault()
                        || s.ADX_TREND_BULL.GetValueOrDefault() || s.MACD_CROSS_BULL.GetValueOrDefault()
                        || s.MACD_REVERSE_BULL.GetValueOrDefault())
                    {
                        scanResult.Add(s);
                    }
                }
            }
            
            return scanResult;
        }
        #endregion

        #region Azure unfinished
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
                foreach (var symbol in symbolList)
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
            List<ScanResultEntity> scanResults = new List<ScanResultEntity>();

            try
            {
                this._log.LogInformation($"In ProcessIndividualStock for {symbol}");

                var tickString = await this._tickerManager.DownloadYahooTicks(symbol,
                    DateTime.Now.Date.AddMonths(-1 * periodInMonth),
                    DateTime.Now.Date,
                    interval);

                var tickList = this._tickerManager.ConvertToEntities(symbol, tickString);

                this._log.LogInformation($"After retireve ticker for symbol {symbol}, count: {tickList.Count}");

                var indList = this._indicatorManager.CalculateIndicators(symbol, tickList);

                if (indList != null)
                {
                    this._log.LogInformation($"After calculate indicators for symbol {symbol}, count: {indList.Count}");
                } else
                {
                    this._log.LogInformation($"After calculate indicators for symbol {symbol}, count: 0, return null from ticker");
                }

                scanResults = (List<ScanResultEntity>)this._scanManager.ProcessScan(indList);


            } catch (Exception ex)
            {
                this._log.LogError($"Error in ProcessIndividualStock symbol: {symbol}, interval: {interval}," +
                    $" periodInMonth: {periodInMonth} ");
            }

            return scanResults;

        }
        #endregion
    }
}