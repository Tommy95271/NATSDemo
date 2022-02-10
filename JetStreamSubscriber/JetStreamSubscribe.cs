using JetStreamShared;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NATS.Client;
using NATS.Client.JetStream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JetStreamSubscriber
{
    internal class JetStreamSubscribe : IHostedService
    {
        public JetStreamSubscribe(IConnection connection,
            StreamSpace streamSpace,
            Subscribe subscribe,
            Consumer consumer,
            ILogger<JetStreamSubscribe> logger,
            IHostApplicationLifetime appLifetime)
        {
            _connection = connection;
            _streamSpace = streamSpace;
            _subscribe = subscribe;
            _consumer = consumer;
            _logger = logger;
            _appLifetime = appLifetime;
        }

        private IConnection? _connection;
        private readonly StreamSpace _streamSpace;
        private readonly Subscribe _subscribe;
        private readonly Consumer _consumer;
        private readonly ILogger<JetStreamSubscribe> _logger;
        private readonly IHostApplicationLifetime _appLifetime;
        private readonly string _allowedOptions = "12345678qQ";
        private Dictionary<string, Action> _jetStreamAction;
        private bool _exit = false;
        private IJetStreamManagement _jsm { get; set; }
        private IJetStream _js { get; set; }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _appLifetime.ApplicationStarted.Register(OnStarted);
            _appLifetime.StopApplication();
        }

        private void OnStarted()
        {
            _js = _connection.CreateJetStreamContext();
            _jsm = _connection.CreateJetStreamManagementContext();
            _jetStreamAction = new Dictionary<string, Action>()
                {
                    {"1", () => _streamSpace.JetStreamListStreams(_jsm) },
                    {"2", () => _consumer.JetStreamListConsumers(_jsm) },
                    {"3", () => _streamSpace.JetStreamCreateStream(_jsm) },
                    {"4", () => _consumer.JetStreamCreateConsumer(_jsm) },
                    {"5", () => _subscribe.JetStreamSubPullBased(_jsm,_js) },
                    {"6", () => _subscribe.JetStreamSubPushBased(_jsm,_js) },
                    {"7", () => _streamSpace.JetStreamDeleteStream(_jsm) },
                    {"8", () => _consumer.JetStreamDeleteConsumer(_jsm) },
                    {"q", () => _exit = true },
                    {"Q", () => _exit = true },
                };
            var modes = new List<string>() {
                    "JetStream list all streams",
                    "JetStream list all consumers in a stream",
                    "JetStream create stream",
                    "JetStream create consumer",
                    "JetStream subscribe a subject from stream (pull-based)",
                    "JetStream subscribe a subject from stream (push-based)",
                    "JetStream delete stream",
                    "JetStream delete consumer" };

            while (!_exit)
            {
                Console.Clear();

                Console.WriteLine("NATS JetStream demo consumer");
                Console.WriteLine("==================");
                Console.WriteLine("Select mode:");

                int count = 1;
                foreach (var mode in modes)
                {
                    Console.WriteLine($"{count}) {mode}");
                    count++;
                }
                Console.WriteLine("q) Quit");

                string input;
                do
                {
                    input = Console.ReadLine();
                } while (!_allowedOptions.Contains(input));

                if (_jetStreamAction.ContainsKey(input))
                {
                    _jetStreamAction[input].Invoke();
                    if (input == "q" || input == "Q")
                    {
                        continue;
                    }
                }

                Console.WriteLine();
                Console.WriteLine("Done. Press any key to continue...");
                Console.ReadKey(true);
                Clear();
                //BenchmarkRunner.Run(typeof(Publisher).Assembly);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private void Clear()
        {
            Console.Clear();
            _connection.Publish("nats.demo.clear", null);
        }
    }
}
