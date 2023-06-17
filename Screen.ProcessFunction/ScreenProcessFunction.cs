using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Screen.Access;
using Screen.Entity;
using Screen.Indicator;
using Screen.Symbols;
using Screen.Ticks;
using Screen.Utils;

namespace Screen.Function
{
    public class ScreenProcessFunction
    {
        [FunctionName("status")]
        public static Task<IActionResult> Status(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]
            HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Status check called.");
            var testValue = Environment.GetEnvironmentVariable("STORAGE_CONNECTION_STRING");

            return Task.FromResult<IActionResult>(
                new OkObjectResult($"Status ok. {testValue}" + DateTime.Now));
        }

        [FunctionName("symbol")]
        public static async Task<IActionResult> Symbol(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]
            HttpRequest req,
            ILogger log)
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


        [FunctionName("ticker")]
        public static async Task<IActionResult> Ticker(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]
            HttpRequest req,
            ILogger log)
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
                    } else
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

                    } catch (Exception ex)
                    {
                        throw new ArgumentException($"period much be an integer");
                    }

                    YahooTickManager tickManager = new YahooTickManager(new Shared.SharedSettings
                    {
                        YahooUrlTemplate = yahooUrlTemplate
                    });


                    symbol = symbol.IndexOf(".AX") > 0 ? symbol.ToUpper() : symbol.ToUpper().Trim() + ".AX";
                    DateTime end = DateTime.Now.Date;
                    DateTime start = interval == "d" ? DateTime.Now.Date.AddDays(-1* period) : DateTime.Now.Date.AddDays(-7 * period);
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
            ILogger log)
        {
            try
            {
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
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]
            HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            if (req.Method == "POST")
            {
                // Read the request body
                string requestBody;
                using (StreamReader streamReader = new StreamReader(req.Body))
                {
                    requestBody = streamReader.ReadToEnd();
                }

                // Process the posted data
                // Example: assuming the posted data is a JSON object
                // You can deserialize it to a class or perform any other necessary operations
                // For demonstration purposes, we'll simply return the posted data as the response
                return new OkObjectResult(requestBody);
            }
            else if (req.Method == "GET")
            {
                // Process GET request
                // Example: Return a simple response for GET requests
                return new OkObjectResult("This is a GET request");
            }

            return new BadRequestResult();
        }
    }
}