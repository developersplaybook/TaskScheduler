using JobScheduler.Interfaces;
using JobScheduler.Models;

namespace JobScheduler.Services
{
    public class JobService : IJobService
    {
        private readonly IHttpClientFactoryService _httpClientFactory;
        private static readonly TimeSpan Timeout = TimeSpan.FromMilliseconds(10000);

        public JobService(IHttpClientFactoryService httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task TriggerJobToggle(JobNames jobName)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("http://localhost:63566/api/");
                client.Timeout = Timeout;

                var uriBuilder = new UriBuilder("http://localhost:63566/api/jobstatus/toggle")
                {
                    Query = $"jobName={Uri.EscapeDataString(jobName.ToString())}"
                };

                var response = await client.GetAsync(uriBuilder.Uri);

                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Trigger 'TriggerJobToggle' was not successful. {response.StatusCode} {content}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
