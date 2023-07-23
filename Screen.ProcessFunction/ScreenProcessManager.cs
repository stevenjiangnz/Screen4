using Google.Apis.Drive.v3;
using Microsoft.Extensions.Logging;
using Screen.Entity;
using Screen.Indicator;
using Screen.Notification;
using Screen.Scan;
using Screen.Symbols;
using Screen.Ticks;
using System;
using System.Collections.Generic;
using System.Linq;
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
            }, log);

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
                        || s.MACD_REVERSE_BULL.GetValueOrDefault() ||
                        s.ADX_CROSS_BEAR.GetValueOrDefault() || s.ADX_INTO_BEAR.GetValueOrDefault()
                        || s.ADX_TREND_BEAR.GetValueOrDefault() || s.MACD_CROSS_BEAR.GetValueOrDefault()
                        || s.MACD_REVERSE_BEAR.GetValueOrDefault())
                    {
                        scanResult.Add(s);
                    }
                }
            }

            this._log.LogInformation($"After scan indicator, returned {scanResult.Count}");

            if (scanResult != null && scanResult.Count > 0)
            {
                scanResult = scanResult.OrderBy(m => m.Symbol).ToList();

                await _scanManager.SaveScanResultWeekly(service, scanResult, rootId);

                var dateString = scanResult[0].TradingDate.ToString();

                var subject = "Weekly Scan Result - " + dateString + $" ({scanResult.Count})";
                var body = ScanManager.ConvertToCsv<ScanResultEntity>(scanResult);

                await this.SendNotificationEmail(subject, body);
            }

            return scanResult;
        }


        public async Task<List<ScanResultEntity>> ProcessDailyBull(DriveService service,
            string rootId,
            string symbolListFileName,
            int top,
            string yahooUrlTemplate)
        {
            this._log.LogInformation("in ProcessDailyBull");

            List<ScanResultEntity> scanResultbull = new List<ScanResultEntity>();
            List<ScanResultEntity> scanResultbear = new List<ScanResultEntity>();

            SymbolManager symbolManager = new SymbolManager(this._log);

            var symbolList = await symbolManager.GetSymbolsFromGoogleStorage(service, rootId, symbolListFileName, top);

            this._log.LogInformation($"After get Symbol, returned {symbolList.Count}");

            foreach (var symbol in symbolList)
            {
                var stockResult = await this.ProcessIndividualStock(yahooUrlTemplate, symbol.Code, "1d", 15);

                if (stockResult != null && stockResult.Count > 0)
                {
                    var s = stockResult[0];

                    if (s.ADX_CROSS_BULL.GetValueOrDefault() || s.ADX_INTO_BULL.GetValueOrDefault()
                        || s.ADX_TREND_BULL.GetValueOrDefault() || s.MACD_CROSS_BULL.GetValueOrDefault()
                        || s.MACD_REVERSE_BULL.GetValueOrDefault())
                    {
                        s.MACD_CROSS_BEAR = null;
                        s.MACD_REVERSE_BEAR = null;
                        s.ADX_CROSS_BEAR = null;
                        s.ADX_INTO_BEAR = null;
                        s.ADX_TREND_BEAR = null;
                        scanResultbull.Add(s);
                    }

 
                }
            }

            this._log.LogInformation($"After scan indicator, returned {scanResultbull.Count}");

            if (scanResultbull != null && scanResultbull.Count > 0)
            {
                scanResultbull = scanResultbull.OrderBy(m => m.Symbol).ToList();

                await _scanManager.SaveScanResultDaily(service, scanResultbull, rootId, "bull");

                var dateString = scanResultbull[0].TradingDate.ToString();

                var subject = "Daily Scan Result (Bull) - " + dateString + $" ({scanResultbull.Count})"; 
                var body = ScanManager.ConvertToCsv<ScanResultEntity>(scanResultbull);

                await this.SendNotificationEmail(subject, body);
            }


            if (scanResultbear != null && scanResultbear.Count > 0)
            {
                scanResultbear = scanResultbear.OrderBy(m => m.Symbol).ToList();

                await _scanManager.SaveScanResultDaily(service, scanResultbear, rootId, "bear");

                var dateString = scanResultbear[0].TradingDate.ToString();

                var subject = "Daily Scan Result (Bear) - " + dateString + $" ({scanResultbear.Count})";
                var body = ScanManager.ConvertToCsv<ScanResultEntity>(scanResultbear);

                await this.SendNotificationEmail(subject, body);
            }

            return scanResultbull;
        }


        public async Task SendNotificationEmail(string subject, string body)
        {
            var emailApiKey = Environment.GetEnvironmentVariable("EMAIL_API_KEY");
            var emailApiSecret = Environment.GetEnvironmentVariable("EMAIL_API_SECRET");
            var emailSender = Environment.GetEnvironmentVariable("EMAIL_SENDER");
            var emailRecipients = Environment.GetEnvironmentVariable("EMAIL_RECIPIENTS");

            NotificationManager notificationManager = new NotificationManager(emailApiKey, emailApiSecret, this._log);

            await notificationManager.SendNotificationEmail(emailSender, emailRecipients, subject, body);   
        }
        #endregion

        #region Azure unfinished
        public async Task<List<ScanResultEntity>> ProcessIndividualStock(string urlTemplate, string symbol,
            string interval, int periodInMonth)
        {
            List<ScanResultEntity> scanResults = new List<ScanResultEntity>();

            try
            {
                this._log.LogDebug($"In ProcessIndividualStock for {symbol}");

                var tickString = await this._tickerManager.DownloadYahooTicks(symbol,
                    DateTime.Now.Date.AddMonths(-1 * periodInMonth),
                    DateTime.Now.Date,
                    interval);

                var tickList = this._tickerManager.ConvertToEntities(symbol, tickString);

                this._log.LogDebug($"After retireve ticker for symbol {symbol}, count: {tickList.Count}");

                var indList = this._indicatorManager.CalculateIndicators(symbol, tickList);

                if (indList != null)
                {
                    this._log.LogDebug($"After calculate indicators for symbol {symbol}, count: {indList.Count}");
                } else
                {
                    this._log.LogDebug($"After calculate indicators for symbol {symbol}, count: 0, return null from ticker");
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
