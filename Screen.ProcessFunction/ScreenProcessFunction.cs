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
using System.Runtime.CompilerServices;
using Serilog;

namespace Screen.Function
{
    public class ScreenProcessFunction
    {
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

        public static DriveService GetDriveServic()
        {
            string serviceAccountKeyJson = Environment.GetEnvironmentVariable("GoogleServiceAccountKey");

            GoogleCredential credential;

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(serviceAccountKeyJson)))
            {
                credential = GoogleCredential.FromStream(stream).CreateScoped(DriveService.Scope.Drive);
            }

            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential
            });

            return service;
        }



        [FunctionName("symbol")]
        public static async Task<IActionResult> GoogleSymbol(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]
                    HttpRequest req,
            Microsoft.Extensions.Logging.ILogger log)
        {
            try
            {
                log.LogInformation($"GoogleSymbol: {req}");
                string parentFolderId = Environment.GetEnvironmentVariable("GOOGLE_ROOT_ID");
                string asxFileName = Environment.GetEnvironmentVariable("ASX_COMPANY_LIST_FILE_NAME");

                var symbolManager = new SymbolManager(log);

                var service = GetDriveServic();

                // top -1 means return all, otherwise take the number defined in top
                int top = -1;
                string topString = string.Empty;

                string output = "json";

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

                        if (queryDict.ContainsKey("output"))
                        {
                            output = queryDict["output"];
                        }

                    }
                    catch (Exception ex)
                    {
                        throw new ArgumentException($"Error parse input parameter {topString}", ex);
                    }
                }

                List<SymbolEntity> resultList = new List<SymbolEntity>();

                if (top > 0)
                {
                    resultList = await symbolManager.GetSymbolsFromGoogleStorage(service, parentFolderId, asxFileName, top);
                }
                else
                {
                    resultList = await symbolManager.GetSymbolsFromGoogleStorage(service, parentFolderId, asxFileName);
                }

                if (output == "json")
                {
                    return new JsonResult(resultList);
                }
                else
                {
                    return new OkObjectResult(symbolManager.GetStringFromSymbolList(resultList));
                }

            }
            catch (ArgumentException ex)
            {
                log.LogError(ex, "Error arguments in Symbol" + ex.ToString());
                return new BadRequestObjectResult(ex.Message);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error in Symbol" + ex.ToString());
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }


        [FunctionName("ticker")]
        public static async Task<IActionResult> Ticker(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]
            HttpRequest req,
            Microsoft.Extensions.Logging.ILogger log)
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
                    });

                    DateTime end = DateTime.Now.Date;
                    DateTime start = interval == "d" ? DateTime.Now.Date.AddDays(-1 * period) : DateTime.Now.Date.AddDays(-7 * period);
                    string intervalString = interval == "d" ? "1d" : "1wk";

                    string returnTickString = await tickManager.DownloadYahooTicks(symbol, start, end, intervalString);

                    if (output == "json")
                    {
                        var tickList = tickManager.ConvertToEntities(symbol, returnTickString);
                        return new JsonResult(tickList);
                    }

                    return new OkObjectResult(returnTickString);
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


        [FunctionName("indicator")]
        public static async Task<IActionResult> Indicator(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
            HttpRequest req,
            Microsoft.Extensions.Logging.ILogger log)
        {
            try
            {
                log.LogInformation("In indicator function method.");

                IndicatorManager indicatorManager = new IndicatorManager(null);

                if (req.Method == "POST")
                {
                    // Read the request body
                    string requestBody = string.Empty;
                    using (StreamReader streamReader = new StreamReader(req.Body))
                    {
                        requestBody = streamReader.ReadToEnd();
                    }

                    if (!string.IsNullOrEmpty(requestBody))
                    {
                        List<TickerEntity> tickets = ObjectHelper.FromJsonString<List<TickerEntity>>(requestBody);


                        if (tickets != null && tickets.Count > 0)
                        {
                            IList<IndicatorEntity> indicators = indicatorManager.CalculateIndicators(tickets[0].T, tickets);
                            return new OkObjectResult(indicators);
                        }
                    }
                }
            }
            catch (ArgumentException ex)
            {
                log.LogError(ex, "Error arguments in Indictor");
                return new BadRequestObjectResult(ex.Message);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error in Indictor");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
            return new BadRequestObjectResult("Error in get Indictors. Should not see this...");
        }

        [FunctionName("scan")]
        public static async Task<IActionResult> Scan(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
            HttpRequest req,
            Microsoft.Extensions.Logging.ILogger log)
        {
            log.LogInformation("In scan function method.");

            try
            {
                ScanManager scanManager = new ScanManager(null);

                if (req.Method == "POST")
                {
                    // Read the request body
                    string requestBody;
                    using (StreamReader streamReader = new StreamReader(req.Body))
                    {
                        requestBody = streamReader.ReadToEnd();

                        if (!string.IsNullOrEmpty(requestBody))
                        {
                            List<IndicatorEntity> indicators = ObjectHelper.FromJsonString<List<IndicatorEntity>>(requestBody);

                            var scanResult = scanManager.ProcessScan(indicators);

                            return new OkObjectResult(scanResult);
                        }
                    }

                    return new BadRequestObjectResult("Input of scan is not valid.");
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error in scan.");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
            return new BadRequestResult();
        }


        [FunctionName("store")]
        public static async Task<IActionResult> Store(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
            HttpRequest req,
            Microsoft.Extensions.Logging.ILogger log)
        {
            log.LogInformation("In Store function method.");

            try
            {
                string parentFolderId = Environment.GetEnvironmentVariable("GOOGLE_ROOT_ID");

                ScanManager scanManager = new ScanManager(log);
                DriveService driveService = GetDriveServic();

                if (req.Method == "POST")
                {
                    // Read the request body
                    string requestBody;
                    using (StreamReader streamReader = new StreamReader(req.Body))
                    {
                        requestBody = streamReader.ReadToEnd();

                        if (!string.IsNullOrEmpty(requestBody))
                        {
                            List<ScanResultEntity> scanResultList = ObjectHelper.FromJsonString<List<ScanResultEntity>>(requestBody);

                            await scanManager.SaveScanResultWeekly(driveService, scanResultList, parentFolderId);

                            return new OkObjectResult("scan result stored in google.");
                        }
                    }

                    return new BadRequestObjectResult("Input of scan is not valid.");
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error in scan.");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
            return new BadRequestResult();
        }
        #region support functions

        public static void CreateJsonFile(DriveService service, string filename, string jsonContent, string parentFolderId)
        {
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = filename,
                MimeType = "application/json",
                Parents = new List<string> { parentFolderId }
            };

            byte[] byteArray = Encoding.UTF8.GetBytes(jsonContent);
            using (MemoryStream stream = new MemoryStream(byteArray))
            {
                FilesResource.CreateMediaUpload request = service.Files.Create(fileMetadata, stream, "application/json");
                request.Fields = "id";
                request.Upload();
            }

            Console.WriteLine("File created with ID: " + fileMetadata.Id);
        }


        public static void ListFilesInFolder(DriveService service, string folderId, string indent = "")
        {
            // Define parameters of request.
            FilesResource.ListRequest listRequest = service.Files.List();
            listRequest.Q = $"'{folderId}' in parents";
            listRequest.Fields = "nextPageToken, files(id, name, mimeType)";

            // List all files and folders.
            IList<Google.Apis.Drive.v3.Data.File> files = listRequest.Execute().Files;
            Console.WriteLine(indent + "Files and folders:");
            if (files != null && files.Count > 0)
            {
                foreach (var file in files)
                {
                    Console.WriteLine(indent + "{0} ({1})", file.Name, file.Id);

                    // If the file is a folder, recurse into it.
                    if (file.MimeType == "application/vnd.google-apps.folder")
                    {
                        ListFilesInFolder(service, file.Id, indent + "  ");
                    }
                }
            }
            else
            {
                Console.WriteLine(indent + "No files or folders found.");
            }
        }

        [FunctionName("process")]
        public static async Task<IActionResult> GoogleProcess(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]
                HttpRequest req,
                Microsoft.Extensions.Logging.ILogger log)
        {
            try
            {
                log.LogInformation("in Google Process");

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
                            return new OkObjectResult("Daily Process finished.");
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

        #endregion

        #region Azure Storage for Symbol List
        [FunctionName("azureprocess")]
        public static async Task<IActionResult> AzureProcess(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]
            HttpRequest req,
            Microsoft.Extensions.Logging.ILogger log)
        {
            try
            {
                var storageConnString = Environment.GetEnvironmentVariable("STORAGE_CONNECTION_STRING");
                var storageContainer = Environment.GetEnvironmentVariable("STORAGE_CONTAINER");
                var symbolListFileName = Environment.GetEnvironmentVariable("SYMBOL_LIST_FILE_NAME");

                var yahooUrlTemplate = Environment.GetEnvironmentVariable("YAHOO_URL_TEMPLATE");

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
                            await processManager.ProcessWeeklyBull(storageConnString, storageContainer, symbolListFileName, top, yahooUrlTemplate);
                            return new OkObjectResult("Weekly Process finished.");
                        }
                        else if (interval == "d")
                        {
                            return new OkObjectResult("Daily Process finished.");
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
                log.LogError(ex, "Error arguments in Symbol");
                return new BadRequestObjectResult(ex.Message);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error in Symbol");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        [FunctionName("azuresymbol")]
        public static async Task<IActionResult> AzureSymbol(
    [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]
            HttpRequest req,
    Microsoft.Extensions.Logging.ILogger log)
        {
            try
            {
                var storageConnString = Environment.GetEnvironmentVariable("STORAGE_CONNECTION_STRING");
                var storageContainer = Environment.GetEnvironmentVariable("STORAGE_CONTAINER");
                var symbolListFileName = Environment.GetEnvironmentVariable("SYMBOL_LIST_FILE_NAME");


                // top -1 means return all, otherwise take the number defined in top
                int top = -1;
                string topString = string.Empty;

                string output = "json";

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

                        if (queryDict.ContainsKey("output"))
                        {
                            output = queryDict["output"];
                        }

                    }
                    catch (Exception ex)
                    {
                        throw new ArgumentException($"Error parse input parameter {topString}", ex);
                    }
                }

                SymbolManager symbolManager = new SymbolManager(log);

                List<SymbolEntity> resultList = new List<SymbolEntity>();

                if (top > 0)
                {
                    resultList = await symbolManager.GetSymbolsFromAzureStorage(storageConnString, storageContainer, symbolListFileName, top);
                }
                else
                {
                    resultList = await symbolManager.GetSymbolsFromAzureStorage(storageConnString, storageContainer, symbolListFileName);
                }

                if (output == "json")
                {
                    return new JsonResult(resultList);
                }
                else
                {
                    return new OkObjectResult(symbolManager.GetStringFromSymbolList(resultList));
                }

            }
            catch (ArgumentException ex)
            {
                log.LogError(ex, "Error arguments in Symbol");
                return new BadRequestObjectResult(ex.Message);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error in Symbol");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        #endregion
    }
}