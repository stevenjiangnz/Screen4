using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Screen.Entity;
using Screen.Indicator;
using Screen.ProcessFunction;
using Screen.Scan;
using Screen.Symbols;
using Screen.Ticks;
using Screen.Utils;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using System.Text;
using Screen.Notification;
using Screen.Access;
using Screen.ProcessFunction.etoro;
using Screen.ProcessFunction.asxetf;

namespace Screen.Function
{
    public class ScreenProcessFunction
    {

        #region Process Functions
        [FunctionName("status")]
        public static Task<IActionResult> Status(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]
            HttpRequest req,
            Microsoft.Extensions.Logging.ILogger log)
        {
            log.LogInformation("Status check called.");
            var testValue = Environment.GetEnvironmentVariable("STORAGE_CONNECTION_STRING");

            return Task.FromResult<IActionResult>(
                new OkObjectResult($"Status ok. {testValue}" + DateTime.Now));
        }

        [FunctionName("asxetfprocess")]
        public static async Task<IActionResult> AsxEtfProcess([HttpTrigger(AuthorizationLevel.Function, 
            "get", Route = null)] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("In ForexProcess");
                var yahooUrlTemplate = Environment.GetEnvironmentVariable("YAHOO_URL_TEMPLATE");
                string rootId = Environment.GetEnvironmentVariable("GOOGLE_ROOT_ID");
                var service = GetDriveServic();
                string etListFileName = Environment.GetEnvironmentVariable("ASX_ETF_LIST_FILE_NAME");

                string market = string.Empty;
                bool verbose = false;

                var queryDict = req.GetQueryParameterDictionary();

                if (queryDict != null)
                {
                    if (queryDict.ContainsKey("market"))
                    {
                        market = queryDict["market"];
                    }
                    else
                    {
                        return new BadRequestObjectResult("querystring market is required. e.g. etf, asx, nyse");
                    }

                    if (queryDict.ContainsKey("verbose"))
                    {
                        if (queryDict["verbose"].ToLower() == "true")
                        {
                            verbose = true;
                        }
                    }
                }

                AsxEtfProcess asxEtfProcessManager = new AsxEtfProcess(log, yahooUrlTemplate);

                var scanResultList = await asxEtfProcessManager.ProcessMarket(market, "1d", verbose);

                return new OkObjectResult(scanResultList);
            }
            catch (ArgumentException ex)
            {
                log.LogError("Error arguments in Process. " + ex.ToString());
                return new BadRequestObjectResult(ex.Message);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error in Process. " + ex.ToString());
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }


        [FunctionName("etprocess")]
        public static async Task<IActionResult> ETProcess([HttpTrigger(AuthorizationLevel.Function, 
            "get", Route = null)] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("In ETProcess");
                var yahooUrlTemplate = Environment.GetEnvironmentVariable("YAHOO_URL_TEMPLATE");
                string rootId = Environment.GetEnvironmentVariable("GOOGLE_ROOT_ID");
                var service = GetDriveServic();
                string etListFileName = Environment.GetEnvironmentVariable("ET_MARKET_LIST_FILE_NAME");

                string market = string.Empty;
                bool verbose = false;

                var queryDict = req.GetQueryParameterDictionary();

                if (queryDict != null)
                {
                    if (queryDict.ContainsKey("market"))
                    {
                        market = queryDict["market"];
                    }
                    else
                    {
                        return new BadRequestObjectResult("querystring market is required. e.g. etf, asx, nyse");
                    }

                    if (queryDict.ContainsKey("verbose"))
                    {
                        if (queryDict["verbose"].ToLower() == "true")
                        {
                            verbose = true;
                        }
                    }
                }

                ETProcessManager eTProcessManager = new ETProcessManager(log, yahooUrlTemplate);

                var scanResultList = await eTProcessManager.ProcessEtMarket(market, verbose);

                return new OkObjectResult(scanResultList);
            }
            catch (ArgumentException ex)
            {
                log.LogError("Error arguments in Process. " + ex.ToString());
                return new BadRequestObjectResult(ex.Message);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error in Process. " + ex.ToString());
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }


        #endregion

        #region Testing functions
        [FunctionName("test_forex_symbollist")]
        public static async Task<IActionResult> TestForexSymbolList([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]
                HttpRequest req, Microsoft.Extensions.Logging.ILogger log)
        {
            try
            {
                log.LogInformation("in TestForexSymbolList");

                string rootId = Environment.GetEnvironmentVariable("GOOGLE_ROOT_ID");
                var service = GetDriveServic();
                string forextFileName = Environment.GetEnvironmentVariable("FOREX_LIST_FILE_NAME");

                CurrencyPairSymbolManager asxetfManager = new CurrencyPairSymbolManager(log);

                string market = "forex";

                var queryDict = req.GetQueryParameterDictionary();

                if (queryDict != null)
                {
                    if (queryDict.ContainsKey("market"))
                    {
                        market = queryDict["market"];
                    }
                }

                var symbolList = asxetfManager.GetCurrencyPairsFullList(service, rootId, forextFileName);

                return new OkObjectResult(symbolList);
            }
            catch (ArgumentException ex)
            {
                log.LogError("Error arguments in Process. " + ex.ToString());
                return new BadRequestObjectResult(ex.Message);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error in Process. " + ex.ToString());
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        [FunctionName("forexprocess")]
        public static async Task<IActionResult> ForexProcess([HttpTrigger(AuthorizationLevel.Function,
            "get", Route = null)] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("In ForexProcess");
                var yahooUrlTemplate = Environment.GetEnvironmentVariable("YAHOO_URL_TEMPLATE");
                string rootId = Environment.GetEnvironmentVariable("GOOGLE_ROOT_ID");
                var service = GetDriveServic();
                string etListFileName = Environment.GetEnvironmentVariable("ASX_ETF_LIST_FILE_NAME");

                string market = string.Empty;
                bool verbose = false;

                var queryDict = req.GetQueryParameterDictionary();

                if (queryDict != null)
                {
                    if (queryDict.ContainsKey("market"))
                    {
                        market = queryDict["market"];
                    }
                    else
                    {
                        return new BadRequestObjectResult("querystring market is required. e.g. etf, asx, nyse");
                    }

                    if (queryDict.ContainsKey("verbose"))
                    {
                        if (queryDict["verbose"].ToLower() == "true")
                        {
                            verbose = true;
                        }
                    }
                }

                AsxEtfProcess asxEtfProcessManager = new AsxEtfProcess(log, yahooUrlTemplate);

                var scanResultList = await asxEtfProcessManager.ProcessMarket(market, "1d", verbose);

                return new OkObjectResult(scanResultList);
            }
            catch (ArgumentException ex)
            {
                log.LogError("Error arguments in Process. " + ex.ToString());
                return new BadRequestObjectResult(ex.Message);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error in Process. " + ex.ToString());
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }


        [FunctionName("test_forex_ticker")]
        public static async Task<IActionResult> TestForexTicker(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]
            HttpRequest req, ILogger log)
        {
            string symbol = string.Empty;
            string interval = "d"; // d for daily or w for weekly
            int period = 360; // default for 360
            try
            {
                var yahooUrlTemplate = Environment.GetEnvironmentVariable("YAHOO_URL_TEMPLATE");

                var queryDict = req.GetQueryParameterDictionary();

                string output = "json";


                if (queryDict != null)
                {
                    if (queryDict.ContainsKey("output"))
                    {
                        output = queryDict["output"];
                    }

                    if (queryDict.ContainsKey("symbol"))
                    {
                        symbol = queryDict["symbol"];
                    }
                    else
                    {
                        throw new ArgumentException("symbol is required");
                    }

                    if (queryDict.ContainsKey("interval"))
                    {
                        interval = queryDict["interval"].ToString().ToLower();

                        if (interval != "d" && interval != "w")
                        {
                            throw new ArgumentException("interval can be either 'd' or 'w'");
                        }
                    }

                    string periodString = string.Empty;
                    try
                    {
                        if (queryDict.ContainsKey("period"))
                        {
                            periodString = queryDict["period"];

                            if (!string.IsNullOrEmpty(periodString))
                            {
                                period = int.Parse(periodString);
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        throw new ArgumentException($"period much be an integer");
                    }

                    YahooTickManager tickManager = new YahooTickManager(new Shared.SharedSettings
                    {
                        YahooUrlTemplate = yahooUrlTemplate
                    }, log);

                    DateTime end = DateTime.Now.Date;
                    DateTime start = interval == "d" ? DateTime.Now.Date.AddDays(-1 * period) : DateTime.Now.Date.AddDays(-7 * period);
                    string intervalString = interval == "d" ? "1d" : "1wk";

                    var tickList = await tickManager.GetEtTickerList(symbol, start, end, intervalString);

                    return new JsonResult(tickList);
                }
            }
            catch (ArgumentException ex)
            {
                log.LogError(ex, "Error arguments in Ticker");
                return new BadRequestObjectResult(ex.Message);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error in Ticker");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
            return new BadRequestObjectResult("Error in get Ticker");
        }

        //[FunctionName("test_notification")]
        //public async static Task<IActionResult> TestNotification(
        //    [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]
        //    HttpRequest req,
        //    Microsoft.Extensions.Logging.ILogger log)
        //{
        //    log.LogInformation("Noticiation check called.");
        //    var emailApiKey = Environment.GetEnvironmentVariable("EMAIL_API_KEY");
        //    var emailApiSecret = Environment.GetEnvironmentVariable("EMAIL_API_SECRET");
        //    var emailSender = Environment.GetEnvironmentVariable("EMAIL_SENDER");
        //    var emailRecipients = Environment.GetEnvironmentVariable("EMAIL_RECIPIENTS");

        //    NotificationManager notificationManager = new NotificationManager(emailApiKey, emailApiSecret, log);

        //    await notificationManager.SendNotificationEmail(emailSender, emailRecipients, "test subjects", "my content  \n content line 2");

        //    return new OkObjectResult($"Status ok. {emailApiKey}" + DateTime.Now);
        //}


        //[FunctionName("test_symbol")]
        //public static async Task<IActionResult> TestGoogleSymbol(
        //    [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]
        //            HttpRequest req,
        //    Microsoft.Extensions.Logging.ILogger log)
        //{
        //    try
        //    {
        //        log.LogInformation($"GoogleSymbol: {req}");
        //        string parentFolderId = Environment.GetEnvironmentVariable("GOOGLE_ROOT_ID");
        //        string asxFileName = Environment.GetEnvironmentVariable("ASX_COMPANY_LIST_FILE_NAME");

        //        var symbolManager = new SymbolManager(log);

        //        var service = GetDriveServic();

        //        // top -1 means return all, otherwise take the number defined in top
        //        int top = -1;
        //        string topString = string.Empty;

        //        string output = "json";

        //        var queryDict = req.GetQueryParameterDictionary();

        //        if (queryDict != null)
        //        {
        //            try
        //            {
        //                if (queryDict.ContainsKey("top"))
        //                {
        //                    topString = queryDict["top"];

        //                    if (!string.IsNullOrEmpty(topString))
        //                    {
        //                        top = int.Parse(topString);
        //                    }
        //                }

        //                if (queryDict.ContainsKey("output"))
        //                {
        //                    output = queryDict["output"];
        //                }

        //            }
        //            catch (Exception ex)
        //            {
        //                throw new ArgumentException($"Error parse input parameter {topString}", ex);
        //            }
        //        }

        //        List<SymbolEntity> resultList = new List<SymbolEntity>();

        //        if (top > 0)
        //        {
        //            resultList = await symbolManager.GetSymbolsFromGoogleStorage(service, parentFolderId, asxFileName, top);
        //        }
        //        else
        //        {
        //            resultList = await symbolManager.GetSymbolsFromGoogleStorage(service, parentFolderId, asxFileName);
        //        }

        //        if (output == "json")
        //        {
        //            return new JsonResult(resultList);
        //        }
        //        else
        //        {
        //            return new OkObjectResult(symbolManager.GetStringFromSymbolList(resultList));
        //        }

        //    }
        //    catch (ArgumentException ex)
        //    {
        //        log.LogError(ex, "Error arguments in Symbol" + ex.ToString());
        //        return new BadRequestObjectResult(ex.Message);
        //    }
        //    catch (Exception ex)
        //    {
        //        log.LogError(ex, "Error in Symbol" + ex.ToString());
        //        return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        //    }
        //}


        //[FunctionName("test_ticker")]
        //public static async Task<IActionResult> TestTicker(
        //    [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]
        //    HttpRequest req,
        //    Microsoft.Extensions.Logging.ILogger log)
        //{
        //    string symbol = string.Empty;
        //    string interval = "d"; // d for daily or w for weekly
        //    int period = 360; // default for 360
        //    try
        //    {
        //        var yahooUrlTemplate = Environment.GetEnvironmentVariable("YAHOO_URL_TEMPLATE");

        //        var queryDict = req.GetQueryParameterDictionary();

        //        string output = "json";


        //        if (queryDict != null)
        //        {
        //            if (queryDict.ContainsKey("output"))
        //            {
        //                output = queryDict["output"];
        //            }

        //            if (queryDict.ContainsKey("symbol"))
        //            {
        //                symbol = queryDict["symbol"];
        //            }
        //            else
        //            {
        //                throw new ArgumentException("symbol is required");
        //            }

        //            if (queryDict.ContainsKey("interval"))
        //            {
        //                interval = queryDict["interval"].ToString().ToLower();

        //                if (interval != "d" && interval != "w")
        //                {
        //                    throw new ArgumentException("interval can be either 'd' or 'w'");
        //                }
        //            }

        //            string periodString = string.Empty;
        //            try
        //            {
        //                if (queryDict.ContainsKey("period"))
        //                {
        //                    periodString = queryDict["period"];

        //                    if (!string.IsNullOrEmpty(periodString))
        //                    {
        //                        period = int.Parse(periodString);
        //                    }
        //                }

        //            }
        //            catch (Exception ex)
        //            {
        //                throw new ArgumentException($"period much be an integer");
        //            }

        //            YahooTickManager tickManager = new YahooTickManager(new Shared.SharedSettings
        //            {
        //                YahooUrlTemplate = yahooUrlTemplate
        //            }, log);

        //            DateTime end = DateTime.Now.Date;
        //            DateTime start = interval == "d" ? DateTime.Now.Date.AddDays(-1 * period) : DateTime.Now.Date.AddDays(-7 * period);
        //            string intervalString = interval == "d" ? "1d" : "1wk";

        //            string returnTickString = await tickManager.DownloadYahooTicks(symbol, start, end, intervalString);

        //            if (output == "json")
        //            {
        //                var tickList = tickManager.ConvertToEntities(symbol, returnTickString);
        //                return new JsonResult(tickList);
        //            }

        //            return new OkObjectResult(returnTickString);
        //        }
        //    }
        //    catch (ArgumentException ex)
        //    {
        //        log.LogError(ex, "Error arguments in Ticker");
        //        return new BadRequestObjectResult(ex.Message);
        //    }
        //    catch (Exception ex)
        //    {
        //        log.LogError(ex, "Error in Ticker");
        //        return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        //    }
        //    return new BadRequestObjectResult("Error in get Ticker");
        //}


        //[FunctionName("test_indicator")]
        //public static async Task<IActionResult> TestIndicator(
        //    [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
        //    HttpRequest req,
        //    Microsoft.Extensions.Logging.ILogger log)
        //{
        //    try
        //    {
        //        log.LogInformation("In indicator function method.");

        //        IndicatorManager indicatorManager = new IndicatorManager(null);

        //        if (req.Method == "POST")
        //        {
        //            // Read the request body
        //            string requestBody = string.Empty;
        //            using (StreamReader streamReader = new StreamReader(req.Body))
        //            {
        //                requestBody = streamReader.ReadToEnd();
        //            }

        //            if (!string.IsNullOrEmpty(requestBody))
        //            {
        //                List<TickerEntity> tickets = ObjectHelper.FromJsonString<List<TickerEntity>>(requestBody);


        //                if (tickets != null && tickets.Count > 0)
        //                {
        //                    IList<IndicatorEntity> indicators = indicatorManager.CalculateIndicators(tickets[0].T, tickets);
        //                    return new OkObjectResult(indicators);
        //                }
        //            }
        //        }
        //    }
        //    catch (ArgumentException ex)
        //    {
        //        log.LogError(ex, "Error arguments in Indictor");
        //        return new BadRequestObjectResult(ex.Message);
        //    }
        //    catch (Exception ex)
        //    {
        //        log.LogError(ex, "Error in Indictor");
        //        return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        //    }
        //    return new BadRequestObjectResult("Error in get Indictors. Should not see this...");
        //}

        //[FunctionName("test_scan")]
        //public static async Task<IActionResult> TestScan(
        //    [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
        //    HttpRequest req,
        //    Microsoft.Extensions.Logging.ILogger log)
        //{
        //    log.LogInformation("In scan function method.");

        //    try
        //    {
        //        ScanManager scanManager = new ScanManager(null);

        //        if (req.Method == "POST")
        //        {
        //            // Read the request body
        //            string requestBody;
        //            using (StreamReader streamReader = new StreamReader(req.Body))
        //            {
        //                requestBody = streamReader.ReadToEnd();

        //                if (!string.IsNullOrEmpty(requestBody))
        //                {
        //                    List<IndicatorEntity> indicators = ObjectHelper.FromJsonString<List<IndicatorEntity>>(requestBody);

        //                    var scanResult = scanManager.ProcessScan(indicators);

        //                    return new OkObjectResult(scanResult);
        //                }
        //            }

        //            return new BadRequestObjectResult("Input of scan is not valid.");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        log.LogError(ex, "Error in scan.");
        //        return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        //    }
        //    return new BadRequestResult();
        //}

        //[FunctionName("test_store")]
        //public static async Task<IActionResult> TestStore(
        //    [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
        //    HttpRequest req,
        //    Microsoft.Extensions.Logging.ILogger log)
        //{
        //    log.LogInformation("In Store function method.");

        //    try
        //    {
        //        string parentFolderId = Environment.GetEnvironmentVariable("GOOGLE_ROOT_ID");

        //        ScanManager scanManager = new ScanManager(log);
        //        DriveService driveService = GetDriveServic();

        //        if (req.Method == "POST")
        //        {
        //            // Read the request body
        //            string requestBody;
        //            using (StreamReader streamReader = new StreamReader(req.Body))
        //            {
        //                requestBody = streamReader.ReadToEnd();

        //                if (!string.IsNullOrEmpty(requestBody))
        //                {
        //                    List<ScanResultEntity> scanResultList = ObjectHelper.FromJsonString<List<ScanResultEntity>>(requestBody);

        //                    await scanManager.SaveScanResultWeekly(driveService, scanResultList, parentFolderId);

        //                    return new OkObjectResult("scan result stored in google.");
        //                }
        //            }

        //            return new BadRequestObjectResult("Input of scan is not valid.");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        log.LogError(ex, "Error in scan.");
        //        return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        //    }
        //    return new BadRequestResult();
        //}

        //        [FunctionName("test_asx_etf_symbollist")]
        //        public static async Task<IActionResult> TestAsxEtfSymbolList(
        //[HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]
        //                HttpRequest req,
        //Microsoft.Extensions.Logging.ILogger log)
        //        {
        //            try
        //            {
        //                log.LogInformation("in AsxEtfSymbolRefresh");

        //                string rootId = Environment.GetEnvironmentVariable("GOOGLE_ROOT_ID");
        //                var service = GetDriveServic();
        //                string etListFileName = Environment.GetEnvironmentVariable("ASX_ETF_LIST_FILE_NAME");

        //                AsxEtfSymbolManager asxetfManager = new AsxEtfSymbolManager(log);

        //                string market = "forex";

        //                var queryDict = req.GetQueryParameterDictionary();

        //                if (queryDict != null)
        //                {
        //                    if (queryDict.ContainsKey("market"))
        //                    {
        //                        market = queryDict["market"];
        //                    }
        //                }

        //                var symbolList = asxetfManager.GetAsxEtfSymbolFullList(service, rootId, etListFileName);

        //                return new OkObjectResult(symbolList);
        //            }
        //            catch (ArgumentException ex)
        //            {
        //                log.LogError("Error arguments in Process. " + ex.ToString());
        //                return new BadRequestObjectResult(ex.Message);
        //            }
        //            catch (Exception ex)
        //            {
        //                log.LogError(ex, "Error in Process. " + ex.ToString());
        //                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        //            }
        //        }


        //        [FunctionName("test_etsymbollist")]
        //        public static async Task<IActionResult> TestETSymbolList(
        //[HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]
        //                HttpRequest req,
        //        Microsoft.Extensions.Logging.ILogger log)
        //        {
        //            try
        //            {
        //                log.LogInformation("in ETSymbolRefresh");

        //                string rootId = Environment.GetEnvironmentVariable("GOOGLE_ROOT_ID");
        //                var service = GetDriveServic();
        //                string etListFileName = Environment.GetEnvironmentVariable("ET_MARKET_LIST_FILE_NAME");

        //                ETSymbolManager etManager = new ETSymbolManager(log);

        //                string market = "etf";

        //                var queryDict = req.GetQueryParameterDictionary();

        //                if (queryDict != null)
        //                {
        //                    if (queryDict.ContainsKey("market"))
        //                    {
        //                        market = queryDict["market"];
        //                    }
        //                }

        //                var symbolList = etManager.GetEtSymbolFullList(service, rootId, etListFileName);

        //                return new OkObjectResult(symbolList);
        //            }
        //            catch (ArgumentException ex)
        //            {
        //                log.LogError("Error arguments in Process. " + ex.ToString());
        //                return new BadRequestObjectResult(ex.Message);
        //            }
        //            catch (Exception ex)
        //            {
        //                log.LogError(ex, "Error in Process. " + ex.ToString());
        //                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        //            }
        //        }

        #endregion

        #region support functions
        [FunctionName("process")]
        public static async Task<IActionResult> GoogleProcess(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]
                HttpRequest req,
                Microsoft.Extensions.Logging.ILogger log)
        {
            try
            {
                log.LogInformation("In AsxProcess");

                string parentFolderId = Environment.GetEnvironmentVariable("GOOGLE_ROOT_ID");
                string asxFileName = Environment.GetEnvironmentVariable("ASX_COMPANY_LIST_FILE_NAME");
                var yahooUrlTemplate = Environment.GetEnvironmentVariable("YAHOO_URL_TEMPLATE");

                ScreenProcessManager screenProcessManager = new ScreenProcessManager(log, yahooUrlTemplate);
                var service = GetDriveServic();

                string interval = "d"; // d for daily or w for weekly

                // top -1 means return all, otherwise take the number defined in top
                int top = 300;
                string topString = string.Empty;

                var queryDict = req.GetQueryParameterDictionary();

                if (queryDict != null)
                {
                    try
                    {
                        if (queryDict.ContainsKey("top"))
                        {
                            topString = queryDict["top"];

                            if (!string.IsNullOrEmpty(topString))
                            {
                                top = int.Parse(topString);
                            }
                        }

                        if (queryDict.ContainsKey("interval"))
                        {
                            interval = queryDict["interval"].ToString().ToLower();

                            if (interval != "d" && interval != "w")
                            {
                                throw new ArgumentException("interval can be either 'd' or 'w'");
                            }
                        }

                        ScreenProcessManager processManager = new ScreenProcessManager(log, yahooUrlTemplate);

                        if (interval == "w")
                        {
                            var scanResult = await processManager.ProcessWeeklyBull(service, parentFolderId, asxFileName, top, yahooUrlTemplate);
                            return new OkObjectResult(scanResult);
                        }
                        else if (interval == "d")
                        {
                            var scanResult = await processManager.ProcessDailyBull(service, parentFolderId, asxFileName, top, yahooUrlTemplate);

                            return new OkObjectResult(scanResult);
                        }

                    }
                    catch (Exception ex)
                    {
                        throw new ArgumentException($"Error parse input parameter {topString}", ex);
                    }
                }

                return new OkObjectResult("Process error, unknown inputs.");
            }
            catch (ArgumentException ex)
            {
                log.LogError("Error arguments in Process. " + ex.ToString());
                return new BadRequestObjectResult(ex.Message);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error in Process. " + ex.ToString());
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        private static DriveService GetDriveServic()
        {
            string serviceAccountKeyJson = Environment.GetEnvironmentVariable("GoogleServiceAccountKey");

            return GoogleDriveManager.GetDriveServic(serviceAccountKeyJson);
        }

        #endregion
    }
}