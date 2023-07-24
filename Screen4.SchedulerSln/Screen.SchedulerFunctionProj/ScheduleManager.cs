using Microsoft.Extensions.Logging;
using Screen.SchedulerFunctionProj.Helpers;
using System;
using System.Collections.Generic;
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
        public ScheduleManager(ILogger log) {
            this._logger = log;

            this._etUrlTemplate = Environment.GetEnvironmentVariable("ET_AUS_URL_TEMPLATE");
        }
        public async Task RunEtAsxProcess(string endpoint)
        {
            this._logger.LogInformation("About to run ASX Etoro Process...");
            await GenericRequestClient(endpoint);
            this._logger.LogInformation("After ran ASX Etoro Process...");
        }

        public async Task RunEtAusProcessJobs()
        {
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
            this._logger.LogInformation($"All jobs done");

        }

        public string getEtAccessUrl(string settingString)
        {
            string url = string.Empty;

            var settings = settingString.Split(';');

            if(settings.Length != 2)
            {
                throw new ArgumentException($"Invalid setting string: {settingString}");
            }

            url = string.Format(this._etUrlTemplate, settings[0], settings[1]);
            return url;
        }

        public async Task GenericRequestClient(string url)
        {
            try
            {
                this._logger.LogInformation($"About to request: {url}");
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

                this._logger.LogInformation($"Finish request: {url}");
            }
            catch (TaskCanceledException ex)
            {
                this._logger.LogError($"Request to {url} timed out. \n {ex.ToString()}");
            }
            catch (Exception ex)
            {
                this._logger.LogError($"Error send request {url} \n {ex.ToString()}");
            }
        }


    }
}
