using AutoMapper;
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

namespace Screen.ProcessFunction.forex
{
    public class ForexProcess
    {
        protected ILogger _logger;
        protected YahooTickManager _tickerManager;
        protected IndicatorManager _indicatorManager;
        protected AsxEtfSymbolManager _symbolManager;
        protected ScanManager _scanManager;
        protected DriveService _driveService;
        protected NotificationManager _notificationManager;
        protected string _googleRootId;
        protected string _asxEtfListFileName;
        protected string _emailSender;
        protected string _emailRecipients;
        private string EXTRA_EAMIL_RECEIVE = "steven.jiang@shell.com";
        IMapper _mapper;


        public ForexProcess(ILogger log, string yahooTemplate)
        {
            this._logger = log;

            this._tickerManager = new YahooTickManager(new Shared.SharedSettings
            {
                YahooUrlTemplate = yahooTemplate,
            }, log);

            this._indicatorManager = new IndicatorManager();
            this._symbolManager = new AsxEtfSymbolManager(log);
            this._scanManager = new ScanManager(log);

            init();
        }

        public void init()
        {
            this._googleRootId = Environment.GetEnvironmentVariable("GOOGLE_ROOT_ID");
            this._asxEtfListFileName = Environment.GetEnvironmentVariable("ASX_ETF_LIST_FILE_NAME");
            string serviceAccountKeyJson = Environment.GetEnvironmentVariable("GoogleServiceAccountKey");
            this._driveService = GoogleDriveManager.GetDriveServic(serviceAccountKeyJson);


            var emailApiKey = Environment.GetEnvironmentVariable("EMAIL_API_KEY");
            var emailApiSecret = Environment.GetEnvironmentVariable("EMAIL_API_SECRET");
            this._emailSender = Environment.GetEnvironmentVariable("EMAIL_SENDER");

            this._emailRecipients = Environment.GetEnvironmentVariable("EMAIL_RECIPIENTS");

            this._notificationManager = new NotificationManager(emailApiKey, emailApiSecret, this._logger);

            bool shouldNotifiyExtra = _notificationManager.ShouldNotifyExtraRecipient();

            if (shouldNotifiyExtra)
            {
                this._emailRecipients += ";" + EXTRA_EAMIL_RECEIVE;
            }


            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new MappingProfile());
            });

            this._mapper = config.CreateMapper();
        }

        public async Task<List<ScanResultEntity>> ProcessIndividualSymbol(AsxEtfSymbolEntity symbol,
             string interval)
        {
            List<ScanResultEntity> scanResults = new List<ScanResultEntity>();
            // for daily get one year data, for weekly get 5 years
            int periodInMonth = interval.ToLower() == "1d" ? 12 : 60;

            try
            {
                var tickList = await this._tickerManager.GetEtTickerList(symbol.AsxCode,
                    DateTime.Now.Date.AddMonths(-1 * periodInMonth),
                    DateTime.Now.Date.AddDays(1),
                    interval);

                this._logger.LogDebug($"After retrieving for {symbol.AsxCode}, return ticks {tickList.Count}");

                var indList = this._indicatorManager.CalculateIndicators(symbol.AsxCode, tickList);

                if (indList != null)
                {
                    this._logger.LogDebug($"After calculate indicators for symbol {symbol.AsxCode}, count: {indList.Count}");
                }
                else
                {
                    this._logger.LogDebug($"After calculate indicators for symbol {symbol.AsxCode}, count: 0, return null from ticker");
                }

                scanResults = (List<ScanResultEntity>)this._scanManager.ProcessScan(indList);

                // add more details into the scanResult
                if (tickList.Count > 0 && scanResults.Count > 0)
                {
                    var lastTick = tickList[tickList.Count - 1];
                    if (scanResults[0].TradingDate == lastTick.P)
                    {
                        scanResults[0].Price = lastTick.C;
                        scanResults[0].Volume = lastTick.V;
                        scanResults[0].Exposure = symbol.Exposure;
                        scanResults[0].Benchmark = symbol.Benchmark;
                        scanResults[0].InvestmentStyle = symbol.InvestmentStyle;
                    }
                }

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
            || (s.ADX_TREND_BEAR.GetValueOrDefault() && verbose))
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

        public async Task SendNotificationScanResult(string market, List<ScanResultBullEntity> bullResult, List<ScanResultBearEntity> bearResult)
        {
            string filename = string.Empty;

            if (bullResult != null && bullResult.Count > 0)
            {
                var subject = $"AsxEtfScan_{market}_(BULL)_{bullResult[0].TradingDate}-({bullResult.Count})";
                var body = ScanManager.ConvertToCsv<ScanResultBullEntity>(bullResult);

                await this._notificationManager.SendNotificationEmail(this._emailSender, this._emailRecipients, subject, body);
            }

            if (bearResult != null && bearResult.Count > 0)
            {
                var subject = $"AsxEtfScan_{market}_(BEAR)_{bearResult[0].TradingDate}-({bearResult.Count})";
                var body = ScanManager.ConvertToCsv<ScanResultBearEntity>(bearResult);

                await this._notificationManager.SendNotificationEmail(this._emailSender, this._emailRecipients, subject, body);
            }
        }

        public async Task SaveScanResult(string market, List<ScanResultEntity> bullResult, List<ScanResultEntity> bearResult)
        {
            string filename = string.Empty;

            if (bullResult != null && bullResult.Count > 0)
            {
                filename = $"AsxEtfScan_{market}_{bullResult[0].TradingDate}_bull.csv";
                await this._scanManager.SaveETScanResult(this._driveService, bullResult,
                    _googleRootId, filename, "asx-etf");
            }

            if (bearResult != null && bearResult.Count > 0)
            {
                filename = $"AsxEtfScan_{market}_{bearResult[0].TradingDate}_bear.csv";
                await this._scanManager.SaveETScanResult(this._driveService, bearResult,
                    _googleRootId, filename, "asx-etf");
            }
        }

        public virtual async Task<List<ScanResultEntity>> ProcessMarket(string market, string interval = "1d", bool verbose = false)
        {
            List<ScanResultEntity> bullResultList = new List<ScanResultEntity>();
            List<ScanResultEntity> bearResultList = new List<ScanResultEntity>();
            try
            {
                List<AsxEtfSymbolEntity> symbolList = this._symbolManager.GetAsxEtfSymbolFullList(_driveService, this._googleRootId, this._asxEtfListFileName);

                symbolList = FilterSymbols(symbolList);

                this._logger.LogInformation($"after retrieve symbol for {market}, returned items {symbolList.Count}");

                // process each symbol
                foreach (AsxEtfSymbolEntity symbol in symbolList)
                {
                    symbol.AsxCode = PrepareSymbol(symbol.AsxCode);

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

                bullResultList = bullResultList.OrderBy(b => b.Symbol).ToList();
                bearResultList = bearResultList.OrderBy(b => b.Symbol).ToList();

                var bullResultConverted = this._mapper.Map<List<ScanResultBullEntity>>(bullResultList);
                var bearResultConverted = this._mapper.Map<List<ScanResultBearEntity>>(bearResultList);

                // Send notification scan result
                await this.SendNotificationScanResult(market, bullResultConverted, bearResultConverted);

                // Save to google drive
                await this.SaveScanResult(market, bullResultList, bearResultList);

                return bullResultList.Concat(bearResultList).ToList();
            }
            catch (Exception ex)
            {
                this._logger.LogError($"Process {market} Error.\n {ex.ToString()}");
                throw;
            }
        }

        // The methods below can be overridden by child classes
        public List<AsxEtfSymbolEntity> FilterSymbols(List<AsxEtfSymbolEntity> symbolList)
        {
            return symbolList.Where(s => s.IsEnabled).ToList();
        }
        public string PrepareSymbol(string symbol)
        {
            return symbol + ".ax";
        }

    }
}
