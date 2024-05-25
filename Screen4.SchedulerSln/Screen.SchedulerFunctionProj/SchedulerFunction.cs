using System;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace Screen.SchedulerFunctionProj
{
    public class SchedulerFunction
    {
        [FunctionName("schedulerdaily")]
        public static async Task RunDailyProcess([TimerTrigger("0 * * * * *")] TimerInfo myTimer, ILogger log)
        {
            TimeZoneInfo brisbaneTimeZone = TimeZoneInfo.FindSystemTimeZoneById("E. Australia Standard Time");
            DateTimeOffset currentTime = TimeZoneInfo.ConvertTime(DateTimeOffset.Now, brisbaneTimeZone);

            log.LogInformation("In RunDailyProcess at " + currentTime.ToString("yyyy-MM-dd HH:mm:ss"));

            // Perform checks at specific minute past specific hours
            if (currentTime.Hour == 18 && currentTime.Minute == 8)
            {
                ScheduleManager scheduleManager = new ScheduleManager(log);
                await scheduleManager.RunEtAusProcessJobs();
                await scheduleManager.RunAsxEtfProcessJobs();
            }
            else if (currentTime.Hour == 21 && currentTime.Minute == 15) // Example: 21:15
            {
                ScheduleManager scheduleManager = new ScheduleManager(log);
                await scheduleManager.RunEtHkProcessJobs();
            }
            else if (currentTime.Hour == 5 && currentTime.Minute == 30) // Example: 05:30
            {
                ScheduleManager scheduleManager = new ScheduleManager(log);
                await scheduleManager.RunEtEuProcessJobs();
            }
            else if (currentTime.Hour == 10 && currentTime.Minute == 45) // Example: 10:45
            {
                ScheduleManager scheduleManager = new ScheduleManager(log);
                await scheduleManager.RunEtUsProcessJobs();
            }
        }

        [FunctionName("schedulerweekly")]
        public static async Task RunWeeklyProcess([TimerTrigger("0 0 * * * *")] TimerInfo myTimer, ILogger log)
        {
            TimeZoneInfo brisbaneTimeZone = TimeZoneInfo.FindSystemTimeZoneById("E. Australia Standard Time");
            DateTimeOffset currentTime = TimeZoneInfo.ConvertTime(DateTimeOffset.Now, brisbaneTimeZone);

            log.LogInformation("in RunWeeklyProcess " + currentTime.ToString() + "hour: " + currentTime.Hour);

            if (currentTime.DayOfWeek == DayOfWeek.Saturday && currentTime.Hour == 8)
            {
                log.LogInformation("Saturday weekly scheduler triggered");

                string processUrl = Environment.GetEnvironmentVariable("PROCESS_URL") + "&interval=w";

                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(processUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        log.LogInformation("Response: " + responseContent);
                    }
                    else
                    {
                        log.LogError("Request failed with status code: " + response.StatusCode);
                    }
                }

            }
        }

        [FunctionName("test")]
        public static async Task<IActionResult> Test(
    [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]
                    HttpRequest req,
    Microsoft.Extensions.Logging.ILogger log)
        {
            ScheduleManager scheduleManager = new ScheduleManager(log);

            await scheduleManager.RunAsxEtfProcessJobs();

            return new OkObjectResult("done test");

        }
    }
}
