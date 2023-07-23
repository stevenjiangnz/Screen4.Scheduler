using System;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

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

            log.LogInformation("in RunDailyProcess " + currentTime.ToString() + "hour: " + currentTime.Hour);

            // Check if the current hour is 18 (6 PM)
            if (currentTime.Hour == 18)
            {
                // call Etoro scan
                ScheduleManager scheduleManager = new ScheduleManager(log);
                string asxEndpoint = Environment.GetEnvironmentVariable("ET_ASX_PROCESS_URL");
                await scheduleManager.RunEtAsxProcess(asxEndpoint);

                // Log a message indicating that the scheduler was triggered
                log.LogInformation("Hourly scan scheduler triggered");

                // Get the process URL from the environment variables
                string processUrl = Environment.GetEnvironmentVariable("PROCESS_URL");
                await scheduleManager.GenericRequestClient(processUrl);
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
    }
}
