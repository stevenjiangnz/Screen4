using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace Screen4.Function
{
    public class MyTimerTrigger
    {
        [FunctionName("MyTimerTrigger")]
        public void Run([TimerTrigger("*/5 * * * * *")]TimerInfo myTimer, ILogger log)
        {
            Console.WriteLine("Hello, World!  " + DateTime.Now.ToLongTimeString());
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        }
    }
}
