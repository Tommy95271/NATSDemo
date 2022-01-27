using JetStreamSubscriberHosted.Shared.Models;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NATS.Client;
using NATS.Client.JetStream;

namespace JetStreamSubscriberHosted.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IndexController : ControllerBase
    {

        private readonly ILogger<IndexController> _logger;
        private readonly IConnection _connection;
        private readonly IMapper _mapper;

        private IJetStreamManagement jsm { get; set; }
        public IndexController(ILogger<IndexController> logger, IConnection connection, IMapper mapper)
        {
            _logger = logger;
            _connection = connection;
            _mapper = mapper;
            jsm = _connection.CreateJetStreamManagementContext();
        }

        [HttpPost("AddStream")]
        public async Task<ActionResult> AddStreamAsync(StreamConfig streamConfig)
        {
            StreamConfiguration sc = StreamConfiguration.Builder()
                .WithName(streamConfig.StreamName)
                .WithStorageType((StorageType)streamConfig.StorageType)
                .WithSubjects(streamConfig.Subjects)
                .Build();
            var streamInfo = jsm.AddStream(sc);

            if (streamInfo is not null)
            {
                return Conflict();
            }
            return Ok(streamInfo);

        }

        [HttpGet("GetStreams")]
        public async Task<ActionResult> GetStreamsAsync()
        {
            var streamNames = jsm.GetStreamNames();

            if (streamNames.Count == 0)
            {
                return NoContent();
            }

            return Ok(streamNames);

        }

        [HttpGet("GetSubjects/{streamName}")]
        public async Task<ActionResult> GetSubjectsAsync(string streamName)
        {
            var subjectNames = jsm.GetStreamInfo(streamName).Config.Subjects;

            if (subjectNames.Count == 0)
            {
                return NoContent();
            }

            return Ok(subjectNames);
        }

        [HttpGet("GetConsumers/{streamName}")]
        public async Task<ActionResult> GetConsumersAsync(string streamName)
        {
            var consumerNames = jsm.GetConsumerNames(streamName);

            if (consumerNames.Count == 0)
            {
                return NoContent();
            }

            return Ok(consumerNames);
        }
    }
}
