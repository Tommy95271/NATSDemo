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
        private IJetStreamManagement jsm { get; set; }
        public IndexController(ILogger<IndexController> logger, IConnection connection)
        {
            _logger = logger;
            _connection = connection;
            jsm = _connection.CreateJetStreamManagementContext();
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
