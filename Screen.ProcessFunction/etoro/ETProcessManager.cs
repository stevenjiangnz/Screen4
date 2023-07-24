using Google.Apis.Drive.v3;
using Microsoft.Extensions.Logging;
using Screen.Access;
using Screen.Entity;
using Screen.Indicator;
using Screen.Notification;
using Screen.Scan;
using Screen.Symbols;
using Screen.Ticks;
using Screen.Utils;
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
        private string _yahooTemplate;

        public ETProcessManager(ILogger log, string yahooTemplate)
        {
            this._logger = log;
            this._yahooTemplate = yahooTemplate;
        }

        public async Task<List<ScanResultEntity>> ProcessEtMarket(string market, bool verbose)
        {
            this._logger.LogInformation($"About to start process ETMarket...");
            if (string.IsNullOrEmpty(market))
            {
                throw new ArgumentNullException($"market can not be empty");
            }
            List<ScanResultEntity> scanResultEntities = new List<ScanResultEntity>();
            BaseMarketProcess process = null;
            switch (market)
            {
                case "asx":
                    process = new ASXMarketProcess(_logger, _yahooTemplate);
                    scanResultEntities = await process.ProcessMarket("asx", "1d", verbose);
                    break;
                case "etf-us":
                    process = new ETFUSMarketProcess(_logger, _yahooTemplate);
                    scanResultEntities = await process.ProcessMarket("etf-us", "1d", verbose);
                    break;
                case "etf-uk":
                    process = new ETFUKMarketProcess(_logger, _yahooTemplate);
                    scanResultEntities = await process.ProcessMarket("etf-uk", "1d", verbose);
                    break;
                case "hk":
                    process = new HKMarketProcess(_logger, _yahooTemplate);
                    scanResultEntities = await process.ProcessMarket("hk", "1d", verbose);
                    break;
                case "nasdaq":
                    process = new NASDAQMarketProcess(_logger, _yahooTemplate);
                    scanResultEntities = await process.ProcessMarket("nasdaq", "1d", verbose);
                    break;
                case "uk":
                    process = new UKMarketProcess(_logger, _yahooTemplate);
                    scanResultEntities = await process.ProcessMarket("uk", "1d", verbose);
                    break;
                case "de":
                    process = new DEMarketProcess(_logger, _yahooTemplate);
                    scanResultEntities = await process.ProcessMarket("de", "1d", verbose);
                    break;
                case "pa":
                    process = new PAMarketProcess(_logger, _yahooTemplate);
                    scanResultEntities = await process.ProcessMarket("pa", "1d", verbose);
                    break;
                case "mi":
                    process = new MIMarketProcess(_logger, _yahooTemplate);
                    scanResultEntities = await process.ProcessMarket("mi", "1d", verbose);
                    break;
                case "nyse":
                    process = new NYSEMarketProcess(_logger, _yahooTemplate);
                    scanResultEntities = await process.ProcessMarket("nyse", "1d", verbose);
                    break;
                case "eu":
                    process = new EUMarketProcess(_logger, _yahooTemplate);
                    scanResultEntities = await process.ProcessMarket("eu", "1d", verbose);
                    break;

                default:
                    throw new NotImplementedException($"market {market} is not implemented.");
            }

            return scanResultEntities;
        }
    }
}
