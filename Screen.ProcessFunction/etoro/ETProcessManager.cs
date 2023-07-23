using Google.Apis.Drive.v3;
using Microsoft.Extensions.Logging;
using Screen.Access;
using Screen.Entity;
using Screen.Indicator;
using Screen.Notification;
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
        private DriveService _driveService;
        private NotificationManager _notificationManager;
        private string _googleRootId;
        private string _etListFileName;
        private string _emailSender;
        private string _emailRecipients;

        public ETProcessManager(ILogger log, string yahooTemplate)
        {
            this._logger = log;

            this._tickerManager = new YahooTickManager(new Shared.SharedSettings
            {
                YahooUrlTemplate = yahooTemplate,
            }, log);

            this._indicatorManager = new IndicatorManager();
            this._symbolManager = new ETSymbolManager(log);
            this._scanManager = new ScanManager(log);
        }

        public void init()
        {
            this._googleRootId = Environment.GetEnvironmentVariable("GOOGLE_ROOT_ID");
            this._etListFileName = Environment.GetEnvironmentVariable("ET_MARKET_LIST_FILE_NAME");
            string serviceAccountKeyJson = Environment.GetEnvironmentVariable("GoogleServiceAccountKey");
            this._driveService = GoogleDriveManager.GetDriveServic(serviceAccountKeyJson);


            var emailApiKey = Environment.GetEnvironmentVariable("EMAIL_API_KEY");
            var emailApiSecret = Environment.GetEnvironmentVariable("EMAIL_API_SECRET");
            this._emailSender = Environment.GetEnvironmentVariable("EMAIL_SENDER");
            this._emailRecipients = Environment.GetEnvironmentVariable("EMAIL_RECIPIENTS");

            this._notificationManager = new NotificationManager(emailApiKey, emailApiSecret, this._logger);
        }

        public async Task<List<ScanResultEntity>> ProcessEtMarket(string market, bool verbose)
        {
            this._logger.LogInformation($"About to start process ETMarket...");
            if (string.IsNullOrEmpty(market))
            {
                throw new ArgumentNullException($"market can not be empty");
            }

            switch (market)
            {
                case "asx":
                    return await this.ProcessMarketAsx(verbose:verbose);
                    break;
                default:
                    throw new NotImplementedException($"market {market} is not implemented.");
            }
        }

        public async Task<List<ScanResultEntity>> ProcessMarketAsx(string interval = "1d", bool verbose = false)
        {
            List<ScanResultEntity> bullResultList = new List<ScanResultEntity>();
            List<ScanResultEntity> bearResultList = new List<ScanResultEntity>();
            try
            {
                List<ETSymbolEntity> symbolList = this._symbolManager.GetEtSymbolFullList(_driveService, this._googleRootId, this._etListFileName);
                symbolList = this._symbolManager.GetEtAsxSymbolList(symbolList);

                this._logger.LogInformation($"after retrieve symbol for asx, returned items {symbolList.Count}");

                // process each symbol
                foreach (ETSymbolEntity symbol in symbolList)
                {
                    symbol.Symbol = symbol.Symbol.Replace(".ASX", ".AX");
                    var scanResult = await this.ProcessIndividualSymbol(symbol, interval);

                    if (scanResult != null && scanResult.Count > 0)
                    {
                        var bullResult = this.IsBullResult(scanResult[0], verbose);
                        if (bullResult != null)
                        {
                            bullResultList.Add(bullResult);
                        }

                        var bearResult = this.IsBearResult(scanResult[0], verbose);
                        if (bearResult != null)
                        {
                            bearResultList.Add(bearResult);
                        }

                    }
                }

                // Send notification scan result
                await this.SendNotificationScanResult("asx", bullResultList, bearResultList);

                // Save to google drive
                await this.SaveScanResult("asx", bullResultList, bearResultList);

                return bullResultList.Concat(bearResultList).ToList();
            }
            catch (Exception ex)
            {
                this._logger.LogError($"Process Asx Error.\n {ex.ToString()}");
                throw;
            }
        }

        public async Task SaveScanResult(string market, List<ScanResultEntity> bullResult, List<ScanResultEntity> bearResult)
        {
            string filename = string.Empty;

            if (bullResult!= null && bullResult.Count > 0)
            {
                filename = $"ETScan_{market}_{bullResult[0].TradingDate}_bull.csv";
                await this._scanManager.SaveETScanResult(this._driveService, bullResult,
                    _googleRootId, filename);
            }

            if (bearResult != null && bearResult.Count > 0)
            {
                filename = $"ETScan_{market}_{bearResult[0].TradingDate}_bear.csv";
                await this._scanManager.SaveETScanResult(this._driveService, bearResult,
                    _googleRootId, filename);
            }
        }


        public async Task SendNotificationScanResult(string market, List<ScanResultEntity> bullResult, List<ScanResultEntity> bearResult)
        {
            string filename = string.Empty;

            if (bullResult != null && bullResult.Count > 0)
            {
                var subject = $"ETScan_{market}_{bullResult[0].TradingDate}_bull ({bullResult.Count})";
                var body = ScanManager.ConvertToCsv<ScanResultEntity>(bullResult);

                await this._notificationManager.SendNotificationEmail(this._emailSender, this._emailRecipients, subject, body);
            }

            if (bearResult != null && bearResult.Count > 0)
            {
                var subject = $"ETScan_{market}_{bearResult[0].TradingDate}_bear ({bearResult.Count})";
                var body = ScanManager.ConvertToCsv<ScanResultEntity>(bearResult);

                await this._notificationManager.SendNotificationEmail(this._emailSender, this._emailRecipients, subject, body);
            }
        }

        public async Task<List<ScanResultEntity>> ProcessIndividualSymbol(ETSymbolEntity symbol,
            string interval)
        {
            List<ScanResultEntity> scanResults = new List<ScanResultEntity>();
            // for daily get one year data, for weekly get 5 years
            int periodInMonth = interval.ToLower() == "1d" ? 12 : 60;

            try
            {
                var tickList = await this._tickerManager.GetEtTickerList(symbol.Symbol,
                    DateTime.Now.Date.AddMonths(-1 * periodInMonth),
                    DateTime.Now.Date.AddDays(1),
                    interval);

                this._logger.LogDebug($"After retrieving for {symbol.Symbol}, return ticks {tickList.Count}");

                var indList = this._indicatorManager.CalculateIndicators(symbol.Symbol, tickList);

                if (indList != null)
                {
                    this._logger.LogDebug($"After calculate indicators for symbol {symbol.Symbol}, count: {indList.Count}");
                }
                else
                {
                    this._logger.LogDebug($"After calculate indicators for symbol {symbol.Symbol}, count: 0, return null from ticker");
                }

                scanResults = (List<ScanResultEntity>)this._scanManager.ProcessScan(indList);
            }
            catch (Exception ex)
            {
                this._logger.LogError($"Error in ProcessIndividualStock symbol: {symbol}, interval: {interval}," +
                    $" periodInMonth: {periodInMonth} \n {ex.ToString()}");
            }

            return scanResults;
        }

        public ScanResultEntity? IsBullResult(ScanResultEntity s, bool verbose = true)
        {
            ScanResultEntity? checkedResult = null;

            if (s.MACD_CROSS_BULL.GetValueOrDefault() 
                || s.MACD_REVERSE_BULL.GetValueOrDefault() 
                || s.ADX_CROSS_BULL.GetValueOrDefault() 
                || s.ADX_INTO_BULL.GetValueOrDefault() 
                || (s.ADX_TREND_BULL.GetValueOrDefault() && verbose))
            {
                s.MACD_CROSS_BEAR = null;
                s.MACD_REVERSE_BEAR = null;
                s.ADX_CROSS_BEAR = null;
                s.ADX_INTO_BEAR = null;
                s.ADX_TREND_BEAR = null;
                checkedResult = s;
            }

            return checkedResult;
        }

        public ScanResultEntity? IsBearResult(ScanResultEntity s, bool verbose = true)
        {
            ScanResultEntity? checkedResult = null;

            if (s.MACD_CROSS_BEAR.GetValueOrDefault()
            || s.MACD_REVERSE_BEAR.GetValueOrDefault() 
            || s.ADX_CROSS_BEAR.GetValueOrDefault() 
            || s.ADX_INTO_BEAR.GetValueOrDefault()
            || (s.ADX_TREND_BEAR.GetValueOrDefault() && verbose) )
            {
                s.MACD_CROSS_BULL = null;
                s.MACD_REVERSE_BULL = null;
                s.ADX_CROSS_BULL = null;
                s.ADX_INTO_BULL = null;
                s.ADX_TREND_BULL = null;
                checkedResult = s;
            }

            return checkedResult;
        }
    }
}
