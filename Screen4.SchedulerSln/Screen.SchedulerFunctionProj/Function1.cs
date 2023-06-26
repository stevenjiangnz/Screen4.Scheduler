using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace Screen.SchedulerFunctionProj
{
    public class SchedulerFunction
    {
        [FunctionName("scheduler")] 
        public void Run([TimerTrigger("0 */1 * * * *  ")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        }
    }
}
