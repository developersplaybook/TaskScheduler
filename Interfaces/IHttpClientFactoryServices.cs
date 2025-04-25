namespace JobScheduler.Interfaces
{
    public interface IHttpClientFactoryService
    {
        HttpClient CreateClient(string baseUrl);
    }
}

