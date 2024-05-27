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
        public static bool IsWithinTimeWindow(DateTimeOffset currentTime, int targetHour, int targetMinute, int toleranceInSeconds)
        {
            // Create a target time based on the current date and specified hour and minute, with seconds set to zero
            DateTimeOffset targetTime = new DateTimeOffset(currentTime.Year, currentTime.Month, currentTime.Day, targetHour, targetMinute, 0, currentTime.Offset);

            // Calculate the difference in seconds between the current time and the target time
            double secondsDifference = Math.Abs((currentTime - targetTime).TotalSeconds);

            // Check if the difference is within the tolerance
            return secondsDifference <= toleranceInSeconds;
        }

        // Usage in your Azure Function
        [FunctionName("schedulerdaily")]
        public static async Task RunDailyProcess([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer, ILogger log)
        {
            const int TimeToleranceInSeconds = 5;  // Define a local constant for the time tolerance

            TimeZoneInfo brisbaneTimeZone = TimeZoneInfo.FindSystemTimeZoneById("E. Australia Standard Time");
            DateTimeOffset currentTime = TimeZoneInfo.ConvertTime(DateTimeOffset.Now, brisbaneTimeZone);

            log.LogInformation("In RunDailyProcess at " + currentTime.ToString("yyyy-MM-dd HH:mm:ss"));

            // Check if today is a weekend day
            if (currentTime.DayOfWeek == DayOfWeek.Saturday || currentTime.DayOfWeek == DayOfWeek.Sunday)
            {
                log.LogInformation("Today is a weekend. No processing will occur.");
                return; // Exit the function if it's a weekend
            }

            // Perform checks at specific times using the local constant time tolerance
            if (IsWithinTimeWindow(currentTime, 18, 0, TimeToleranceInSeconds))
            {
                ScheduleManager scheduleManager = new ScheduleManager(log);
                await scheduleManager.RunAsxEtfProcessJobs();
            }

            if (IsWithinTimeWindow(currentTime, 18, 5, TimeToleranceInSeconds))
            {
                ScheduleManager scheduleManager = new ScheduleManager(log);
                await scheduleManager.RunEtAusProcessJobs();
            }

            else if (IsWithinTimeWindow(currentTime, 21, 15, TimeToleranceInSeconds))
            {
                ScheduleManager scheduleManager = new ScheduleManager(log);
                await scheduleManager.RunEtHkProcessJobs();
            }
            else if (IsWithinTimeWindow(currentTime, 5, 30, TimeToleranceInSeconds))
            {
                ScheduleManager scheduleManager = new ScheduleManager(log);
                await scheduleManager.RunEtEuProcessJobs();
            }
            else if (IsWithinTimeWindow(currentTime, 10, 45, TimeToleranceInSeconds))
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
