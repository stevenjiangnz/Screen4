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

namespace Screen.ProcessFunction.ibkr
{
    public abstract class BaseMarketProcess
    {
        protected ILogger _logger;
        protected YahooTickManager _tickerManager;
        protected IndicatorManager _indicatorManager;
        protected IbkrSymbolManager _symbolManager;
        protected ScanManager _scanManager;
        protected DriveService _driveService;
        protected NotificationManager _notificationManager;
        protected string _googleRootId;
        protected string _etListFileName;
        protected string _emailSender;
        protected string _emailRecipients;
        protected string _individualProcessTemplate;
        protected int _processBatch;

        public BaseMarketProcess(ILogger log, string yahooTemplate, string individualProcessTemplate)
        {
            this._logger = log;

            this._tickerManager = new YahooTickManager(new Shared.SharedSettings
            {
                YahooUrlTemplate = yahooTemplate,
            }, log);

            this._indicatorManager = new IndicatorManager();
            this._symbolManager = new IbkrSymbolManager(log);
            this._scanManager = new ScanManager(log);
            this._individualProcessTemplate = individualProcessTemplate;

            init();
        }

        public void init()
        {
            this._googleRootId = Environment.GetEnvironmentVariable("GOOGLE_ROOT_ID");
            this._etListFileName = Environment.GetEnvironmentVariable("US_ETF_LIST_FILE_NAME");
            string serviceAccountKeyJson = Environment.GetEnvironmentVariable("GoogleServiceAccountKey");
            this._driveService = GoogleDriveManager.GetDriveServic(serviceAccountKeyJson);


            var emailApiKey = Environment.GetEnvironmentVariable("EMAIL_API_KEY");
            var emailApiSecret = Environment.GetEnvironmentVariable("EMAIL_API_SECRET");
            this._emailSender = Environment.GetEnvironmentVariable("EMAIL_SENDER");
            this._emailRecipients = Environment.GetEnvironmentVariable("EMAIL_RECIPIENTS");
            this._processBatch = int.Parse(Environment.GetEnvironmentVariable("PROCESS_BATCH"));
            this._notificationManager = new NotificationManager(emailApiKey, emailApiSecret, this._logger);
        }

        public async Task<ScanResultEntity> ProcessIndividualSymbol(IbkrEtfSymbolEntity symbol,
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

                this._logger.LogDebug($"After retrieving for {symbol}, return ticks {tickList.Count}");

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


                // add more details into the scanResult
                if (tickList.Count > 0 && scanResults.Count > 0)
                {
                    var lastTick = tickList[tickList.Count - 1];
                    if (scanResults[0].TradingDate == lastTick.P)
                    {
                        scanResults[0].Price = lastTick.C;
                        scanResults[0].Volume = lastTick.V;
                        scanResults[0].Exposure = symbol.Description;
                        scanResults[0].Benchmark = symbol.Currency;
                        scanResults[0].InvestmentStyle = symbol.Region;
                    }
                }
            }
            catch (Exception ex)
            {
                this._logger.LogError($"Error in ProcessIndividualStock symbol: {symbol}, interval: {interval}," +
                    $" periodInMonth: {periodInMonth} \n {ex.ToString()}");
            }
            if (scanResults != null && scanResults.Count > 0)
            { 
                return scanResults[0];
            } else
            {
                return null;
            }
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

        public async Task SendNotificationScanResult(string market, List<ScanResultEntity> bullResult, List<ScanResultEntity> bearResult, int batch)
        {
            string filename = string.Empty;

            if (bullResult != null && bullResult.Count > 0)
            {
                var subject = $"IBkrScan_{market}-{batch}_(BULL)_{bullResult[0].TradingDate}-({bullResult.Count})";
                var body = ScanManager.ConvertToCsv<ScanResultEntity>(bullResult);

                await this._notificationManager.SendNotificationEmail(this._emailSender, this._emailRecipients, subject, body);
            }

            if (bearResult != null && bearResult.Count > 0)
            {
                var subject = $"IBkrScan_{market}-{batch}_(BEAR)_{bearResult[0].TradingDate}-({bearResult.Count})";
                var body = ScanManager.ConvertToCsv<ScanResultEntity>(bearResult);

                await this._notificationManager.SendNotificationEmail(this._emailSender, this._emailRecipients, subject, body);
            }
        }

        public async Task SaveScanResult(string market, List<ScanResultEntity> bullResult, List<ScanResultEntity> bearResult, int batch)
        {
            string filename = string.Empty;

            if (bullResult != null && bullResult.Count > 0)
            {
                filename = $"IBkrScan_{market}-{batch}_{bullResult[0].TradingDate}_bull.csv";
                await this._scanManager.SaveETScanResult(this._driveService, bullResult,
                    _googleRootId, filename, "ibkr");
            }

            if (bearResult != null && bearResult.Count > 0)
            {
                filename = $"IBkrScan_{market}-{batch}_{bearResult[0].TradingDate}_bear.csv";
                await this._scanManager.SaveETScanResult(this._driveService, bearResult,
                    _googleRootId, filename, "ibkr");
            }
        }

        public virtual async Task<List<ScanResultEntity>> ProcessMarket(string market, string interval = "1d", bool verbose = false, int batch = 0)
        {
            List<ScanResultEntity> bullResultList = new List<ScanResultEntity>();
            List<ScanResultEntity> bearResultList = new List<ScanResultEntity>();
            try
            {
                List<IbkrEtfSymbolEntity> symbolList = this._symbolManager.GetSymbolList(_driveService, this._googleRootId, this._etListFileName);

                symbolList = FilterSymbols(symbolList, batch);

                this._logger.LogInformation($"After retrieve symbol for {market}, returned items {symbolList.Count}");

                const int batchSize = 10;
                int numberOfBatches = (symbolList.Count + batchSize - 1) / batchSize;

                int processedSymbolsCount = 0; // To keep track of the total number of processed symbols

                for (int i = 0; i < numberOfBatches; i++)
                {
                    // Get the current batch of symbols
                    var currentBatch = symbolList.Skip(i * batchSize).Take(batchSize).ToList();

                    // Prepare a list of tasks for parallel processing of the current batch of symbols
                    var tasks = currentBatch.Select(symbol => ProcessSymbolAsync(symbol, interval, verbose)).ToList();

                    // Await all tasks to complete
                    var results = await Task.WhenAll(tasks);

                    // Update processed symbols count and log it
                    processedSymbolsCount += currentBatch.Count;
                    this._logger.LogInformation($"Processed {processedSymbolsCount} of {symbolList.Count} symbols so far.");

                    // Process the results
                    foreach (var scanResult in results)
                    {
                        if (scanResult.bullResult != null)
                        {
                            bullResultList.Add(scanResult.bullResult);
                        }
                        if (scanResult.bearResult != null)
                        {
                            bearResultList.Add(scanResult.bearResult);
                        }
                    }
                }

                bullResultList = bullResultList.OrderBy(b => b.Symbol).ToList();
                bearResultList = bearResultList.OrderBy(b => b.Symbol).ToList();

                // Send notification scan result
                await this.SendNotificationScanResult(market, bullResultList, bearResultList, batch);

                // Save to google drive
                await this.SaveScanResult(market, bullResultList, bearResultList, batch);

                return bullResultList.Concat(bearResultList).ToList();
            }
            catch (Exception ex)
            {
                this._logger.LogError($"Process {market} Error.\n {ex.ToString()}");
                throw;
            }
        }

        protected virtual async Task<(ScanResultEntity bullResult, ScanResultEntity bearResult)> ProcessSymbolAsync(IbkrEtfSymbolEntity symbol, string interval, bool verbose)
        {
            symbol.Symbol = PrepareSymbol(symbol.Symbol);
            var scanResult = await this.ProcessIndividualSymbol(symbol, interval);
            ScanResultEntity bullResult = null;
            ScanResultEntity bearResult = null;
            if (scanResult != null)
            {
                bullResult = this.IsBullResult(scanResult, verbose);
                bearResult = this.IsBearResult(scanResult, verbose);
            }
            return (bullResult, bearResult);
        }


        // The methods below can be overridden by child classes
        public abstract List<IbkrEtfSymbolEntity> FilterSymbols(List<IbkrEtfSymbolEntity> symbolList, int batch);
        public abstract string PrepareSymbol(string symbol);
        public abstract string GetSymbolListFileName();
    }
}
