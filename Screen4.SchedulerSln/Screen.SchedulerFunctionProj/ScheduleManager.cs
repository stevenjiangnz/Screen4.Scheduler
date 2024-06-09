using Microsoft.Extensions.Logging;
using Screen.SchedulerFunctionProj.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Screen.SchedulerFunctionProj
{
    public class ScheduleManager
    {
        private readonly ILogger _logger;
        private string _etUrlTemplate;
        private string _asxetfUrlTemplate;
        public ScheduleManager(ILogger log)
        {
            this._logger = log;

            this._etUrlTemplate = Environment.GetEnvironmentVariable("ET_AUS_URL_TEMPLATE");
            this._asxetfUrlTemplate = Environment.GetEnvironmentVariable("ASX_ETF_URL_TEMPLATE");
        }

        public async Task RunAsxEtfProcessJobs()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start(); // Start the stopwatch

            this._logger.LogInformation($"{nameof(ScheduleManager)}, About to start Aus jobs");
            string processUrlTemplate = Environment.GetEnvironmentVariable("ASX_ETF_URL_TEMPLATE");
            string asxEtfUrl = getAccessUrl(Environment.GetEnvironmentVariable("ASX_ETF_URL_SETTING"), processUrlTemplate);

            List<string> urls = new List<string> {
                asxEtfUrl
            };

            this._logger.LogInformation($"About to request {urls.Count} requests \n {urls.ToJsonString()}");

            List<Task> tasks = new List<Task>();

            foreach (string url in urls)
            {
                tasks.Add(GenericRequestClient(url));
            }

            await Task.WhenAll(tasks);

            stopwatch.Stop(); // Stop the stopwatch

            this._logger.LogInformation($"All jobs done in {stopwatch.Elapsed.TotalSeconds} seconds");
        }


        public async Task RunForexProcessJobs()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start(); // Start the stopwatch

            this._logger.LogInformation($"{nameof(ScheduleManager)}, About to start Forex jobs");
            string processUrlTemplate = Environment.GetEnvironmentVariable("FOREX_URL_TEMPLATE");
            string forexUrl = getAccessUrl(Environment.GetEnvironmentVariable("FOREX_URL_SETTING"), processUrlTemplate);

            List<string> urls = new List<string> {
                forexUrl
            };

            this._logger.LogInformation($"About to request {urls.Count} requests \n {urls.ToJsonString()}");

            List<Task> tasks = new List<Task>();

            foreach (string url in urls)
            {
                tasks.Add(GenericRequestClient(url));
            }

            await Task.WhenAll(tasks);

            stopwatch.Stop(); // Stop the stopwatch

            this._logger.LogInformation($"All jobs done in {stopwatch.Elapsed.TotalSeconds} seconds");
        }


        public async Task RunIbkrUsProcessJobs(int batch)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start(); // Start the stopwatch

            this._logger.LogInformation($"{nameof(ScheduleManager)}, About to RunIbkrUsProcess Jobs");
            string processUrlTemplate = Environment.GetEnvironmentVariable("IBKR_URL_TEMPLATE");
            string settingString = Environment.GetEnvironmentVariable("US_ETF_URL_SETTING");
 
            var settings = settingString.Split(';');

            string processUrl = string.Format(processUrlTemplate, settings[0], settings[1], batch);

            List<string> urls = new List<string> {
                processUrl
            };

            this._logger.LogInformation($"About to request {urls.Count} requests \n {urls.ToJsonString()}");

            List<Task> tasks = new List<Task>();

            foreach (string url in urls)
            {
                tasks.Add(GenericRequestClient(url));
            }

            await Task.WhenAll(tasks);

            stopwatch.Stop(); // Stop the stopwatch

            this._logger.LogInformation($"All jobs done in {stopwatch.Elapsed.TotalSeconds} seconds");
        }



        public async Task RunEtAusProcessJobs()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start(); // Start the stopwatch

            this._logger.LogInformation($"{nameof(ScheduleManager)}, About to start Aus jobs");
            string processUrl = Environment.GetEnvironmentVariable("PROCESS_URL");
            string etAsxUrl = getEtAccessUrl(Environment.GetEnvironmentVariable("ET_ASX_URL_SETTING"));

            List<string> urls = new List<string> {
                processUrl,
                etAsxUrl
            };

            this._logger.LogInformation($"About to request {urls.Count} requests \n {urls.ToJsonString()}");

            List<Task> tasks = new List<Task>();

            foreach (string url in urls)
            {
                tasks.Add(GenericRequestClient(url));
            }

            await Task.WhenAll(tasks);

            stopwatch.Stop(); // Stop the stopwatch

            this._logger.LogInformation($"All jobs done in {stopwatch.Elapsed.TotalSeconds} seconds");
        }

        public async Task RunJobs(List<string> environmentVariables)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start(); // Start the stopwatch

            this._logger.LogInformation($"{nameof(ScheduleManager)}, About to start jobs");

            List<string> urls = environmentVariables
                .Select(Environment.GetEnvironmentVariable)
                .Select(getEtAccessUrl)
                .ToList();

            this._logger.LogInformation($"About to request {urls.Count} requests \n {urls.ToJsonString()}");

            List<Task> tasks = urls.Select(GenericRequestClient).ToList();

            await Task.WhenAll(tasks);

            stopwatch.Stop(); // Stop the stopwatch

            this._logger.LogInformation($"All jobs done in {stopwatch.Elapsed.TotalSeconds} seconds");
        }

        public async Task RunEtEuProcessJobs()
        {
            await RunJobs(new List<string>
            {
                "ET_ETFUK_URL_SETTING"
                //,
                //"ET_UK_URL_SETTING",
                //"ET_DE_URL_SETTING",
                //"ET_PA_URL_SETTING",
                //"ET_MI_URL_SETTING",
                //"ET_EU_URL_SETTING"
            });
        }

        public async Task RunEtHkProcessJobs()
        {
            //await RunJobs(new List<string>
            //{
            //    "ET_HK_URL_SETTING"
            //});
        }

        public async Task RunEtUsProcessJobs()
        {
            await RunJobs(new List<string>
            {
                "ET_ETFUS_URL_SETTING"
                //,
                //"ET_NASDAQ_URL_SETTING",
                //"ET_NYSE_URL_SETTING"
            });
    }

        public string getEtAccessUrl(string settingString)
        {
            string url = string.Empty;

            var settings = settingString.Split(';');

            if (settings.Length != 2)
            {
                throw new ArgumentException($"Invalid setting string: {settingString}");
            }

            url = string.Format(this._etUrlTemplate, settings[0], settings[1]);
            return url;
        }

        public string getAccessUrl(string settingString, string template)
        {
            string url = string.Empty;

            var settings = settingString.Split(';');

            if (settings.Length != 2)
            {
                throw new ArgumentException($"Invalid setting string: {settingString}");
            }

            url = string.Format(template, settings[0], settings[1]);
            return url;
        }


        public async Task GenericRequestClient(string url)
        {
            Stopwatch stopwatch = new Stopwatch();
            try
            {
                this._logger.LogInformation($"About to request: {url}");

                stopwatch.Start(); // Start the stopwatch

                // Send a GET request to the specified URL using HttpClient
                using (HttpClient client = new HttpClient())
                {
                    // Set timeout to 1200 seconds
                    client.Timeout = TimeSpan.FromSeconds(1200);

                    HttpResponseMessage response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        // Read the response content and log it
                        string responseContent = await response.Content.ReadAsStringAsync();
                        this._logger.LogInformation("Response: " + responseContent);
                    }
                    else
                    {
                        // Log an error message if the request fails
                        this._logger.LogError("Request failed with status code: " + response.StatusCode);
                    }
                }

                stopwatch.Stop(); // Stop the stopwatch

                this._logger.LogInformation($"Finish request: {url} in {stopwatch.Elapsed.TotalSeconds} seconds.");
            }
            catch (TaskCanceledException ex)
            {
                stopwatch.Stop(); // Stop the stopwatch in case of an exception

                this._logger.LogError($"Request to {url} timed out after {stopwatch.Elapsed.TotalSeconds} seconds. \n {ex.ToString()}");
            }
            catch (Exception ex)
            {
                stopwatch.Stop(); // Stop the stopwatch in case of an exception

                this._logger.LogError($"Error send request {url} after {stopwatch.Elapsed.TotalSeconds} seconds. \n {ex.ToString()}");
            }
        }


    }
}
