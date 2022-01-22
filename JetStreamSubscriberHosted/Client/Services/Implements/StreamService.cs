using System.Net;
using System.Text.Json;

namespace JetStreamSubscriberHosted.Client.Services.Implements
{
    public class StreamService : IStreamService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HttpClient? httpClient { get; set; }


        public StreamService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        public async Task<IEnumerable<string>> GetStreamNames()
        {
            httpClient = _httpClientFactory?.CreateClient("NATS");
            var httpResponseMessage = await httpClient.GetAsync("api/Index/GetStreams");
            IEnumerable<string>? streamNames;
            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
            {
                var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();
                streamNames = JsonSerializer.Deserialize<IEnumerable<string>>(contentStream);
            }
            else
            {
                streamNames = new List<string>();
            }
            return streamNames;
        }
        public async Task<IEnumerable<string>> GetSubjectNames(string streamName)
        {
            httpClient = _httpClientFactory?.CreateClient("NATS");
            var httpResponseMessage = await httpClient.GetAsync($"api/Index/GetSubjects/{streamName}");
            IEnumerable<string>? subjectNames;
            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
            {
                var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();
                subjectNames = JsonSerializer.Deserialize<IEnumerable<string>>(contentStream);
            }
            else
            {
                subjectNames = new List<string>();
            }
            return subjectNames;
        }
        public async Task<IEnumerable<string>> GetConsumerNames(string streamName)
        {
            httpClient = _httpClientFactory?.CreateClient("NATS");
            var httpResponseMessage = await httpClient.GetAsync($"api/Index/GetConsumers/{streamName}");
            IEnumerable<string>? consumerNames;
            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
            {
                var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();
                consumerNames = JsonSerializer.Deserialize<IEnumerable<string>>(contentStream);
            }
            else
            {
                consumerNames = new List<string>();
            }
            return consumerNames;
        }
    }
}
