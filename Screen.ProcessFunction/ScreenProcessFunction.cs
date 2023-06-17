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
using Screen.Symbols;

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
                log.LogError(ex, "Error in SearchBatteryState");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        [FunctionName("ticker")]
        public static async Task<IActionResult> Ticker(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
            HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

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
    }
}