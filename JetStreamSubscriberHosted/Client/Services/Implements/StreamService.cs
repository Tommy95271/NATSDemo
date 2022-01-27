using JetStreamSubscriberHosted.Client.ViewModels;
using JetStreamSubscriberHosted.Shared.Models;
using Mapster;
using MapsterMapper;
using NATS.Client.JetStream;
using System.Net;
using System.Text;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;

namespace JetStreamSubscriberHosted.Client.Services.Implements
{
    public class StreamService : IStreamService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMapper _mapper;

        public HttpClient? httpClient { get; set; }


        public StreamService(IHttpClientFactory httpClientFactory, IMapper mapper)
        {
            _httpClientFactory = httpClientFactory;
            _mapper = mapper;
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
        public async Task<StreamConfigViewModel> AddStream(StreamConfigViewModel sc)
        {
            httpClient = _httpClientFactory?.CreateClient("NATS");
            //TypeAdapterConfig<StreamConfigViewModel, StreamConfig>
            //    .NewConfig()
            //    .Map(dest => dest.Subjects, src => src.Subjects.Select(s => s.Value));
            var streamConfiguration = _mapper.Map<StreamConfig>(sc);
            streamConfiguration.Subjects = sc.Subjects.Select(s => s.Value).ToList();

            var streamConfigJson = new StringContent(JsonSerializer.Serialize(streamConfiguration), Encoding.UTF8, Application.Json);
            var httpResponseMessage = await httpClient.PostAsync("api/Index/AddStream", streamConfigJson);

            StreamConfigViewModel? streamConfig;
            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
            {
                var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();
                streamConfig = JsonSerializer.Deserialize<StreamConfigViewModel>(contentStream);
            }
            else
            {
                streamConfig = new StreamConfigViewModel();
            }
            return streamConfig;
        }
    }
}
