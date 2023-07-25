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
        public static async Task RunDailyProcess([TimerTrigger("0 0 * * * *")] TimerInfo myTimer, ILogger log)
        {
            // Define the time zone and get the current time in that time zone
            TimeZoneInfo brisbaneTimeZone = TimeZoneInfo.FindSystemTimeZoneById("E. Australia Standard Time");
            DateTimeOffset currentTime = TimeZoneInfo.ConvertTime(DateTimeOffset.Now, brisbaneTimeZone);

            log.LogInformation("In RunDailyProcess " + currentTime.ToString() + "hour: " + currentTime.Hour);

            // Check if the current hour is 18 (6 PM) - asx job
            if (currentTime.Hour == 18)
            {
                // call Etoro scan
                ScheduleManager scheduleManager = new ScheduleManager(log);

                await scheduleManager.RunEtAusProcessJobs();
            }

            // Check if the current hour is 21 (9 PM) - Hongkong job
            if (currentTime.Hour == 21)
            {
                // call Etoro scan
                ScheduleManager scheduleManager = new ScheduleManager(log);

                await scheduleManager.RunEtHkProcessJobs();
            }

            // Check if the current hour is 5 (5 AM) - Eu job
            if (currentTime.Hour == 5)
            {
                // call Etoro scan
                ScheduleManager scheduleManager = new ScheduleManager(log);

                await scheduleManager.RunEtEuProcessJobs();
            }

            // Check if the current hour is 10 (10 AM) - Us job
            if (currentTime.Hour == 10)
            {
                // call Etoro scan
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

            await scheduleManager.RunEtUsProcessJobs();

            return new OkObjectResult("done test");

        }
    }
}
