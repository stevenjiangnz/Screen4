using System;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

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
            return Task.FromResult<IActionResult>(
                new OkObjectResult("Status ok. " + DateTime.Now));
        }

        [FunctionName("symbol")]
        public static Task<IActionResult> Symbol(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]
            HttpRequest req,
            ILogger log)
        {
            try
            {
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

                string storageConnString = Environment.GetEnvironmentVariable("STORAGE_CONNECTION_STRING");
                string storageContainer = Environment.GetEnvironmentVariable("STORAGE_CONTAINER");

                log.LogInformation("Status check called." + storageConnString + storageContainer);
                return Task.FromResult<IActionResult>(
                    new OkObjectResult("Status ok. " + DateTime.Now));
            }
            catch (ArgumentException ex)
            {
                log.LogError(ex, "Error arguments in Symbol");
                return Task.FromResult<IActionResult>(new BadRequestObjectResult(ex.Message));
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error in SearchBatteryState");
                return Task.FromResult<IActionResult>(new InternalServerErrorResult());
            }
        }
    }
}
