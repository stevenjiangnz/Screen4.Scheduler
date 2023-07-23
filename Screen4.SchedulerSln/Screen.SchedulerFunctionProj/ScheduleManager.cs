using Microsoft.Extensions.Logging;
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
        public ScheduleManager(ILogger log) {
            this._logger = log;
        }
        public async Task RunEtAsxProcess(string endpoint)
        {
            this._logger.LogInformation("About to run ASX Etoro Process...");
            await GenericRequestClient(endpoint);
            this._logger.LogInformation("After ran ASX Etoro Process...");
        }

        public async Task GenericRequestClient(string url)
        {
            try
            {
                // Send a GET request to the specified URL using HttpClient
                using (HttpClient client = new HttpClient())
                {
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
            } catch (Exception ex) {
                this._logger.LogError($"Error send request {url} \n {ex.ToString()}");
            }

        }

    }
}
