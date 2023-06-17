using System;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Screen.Access;

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
                StorageManager storageManager = new StorageManager(log);

                var storageConnString = Environment.GetEnvironmentVariable("STORAGE_CONNECTION_STRING");
                var storageContainer = Environment.GetEnvironmentVariable("STORAGE_CONTAINER");
                var symbolListFileName = Environment.GetEnvironmentVariable("SYMBOL_LIST_FILE_NAME");
                

                var result = await storageManager.AzureAccess(storageConnString, storageContainer, symbolListFileName);

                // top -1 means return all, otherwise take the number defined in top
                int top = -1;
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
                    }
                    catch (Exception ex)
                    {
                        throw new ArgumentException($"Error parse input parameter {topString}", ex);
                    }
                }

                return new OkObjectResult(result);
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
    }
}
