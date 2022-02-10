using JetStreamShared;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NATS.Client;
using NATS.Client.JetStream;
using NATSExamples;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetStreamPublisher
{
    internal class JetStreamPublish : IHostedService
    {
        public JetStreamPublish(IConnection connection,
            StreamSpace streamSpace,
            Publish publish,
            Consumer consumer,
            ILogger<JetStreamPublish> logger,
            IHostApplicationLifetime appLifetime)
        {
            _connection = connection;
            _streamSpace = streamSpace;
            _publish = publish;
            _consumer = consumer;
            _logger = logger;
            _appLifetime = appLifetime;
        }

        private static IConnection? _connection;
        private readonly StreamSpace _streamSpace;
        private readonly Publish _publish;
        private readonly Consumer _consumer;
        private readonly ILogger<JetStreamPublish> _logger;
        private readonly IHostApplicationLifetime _appLifetime;
        private readonly string _allowedOptions = "123qQ";
        private static Dictionary<string, Action> _jetStreamAction;
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
                {"1", () => _publish.JetStreamPubAsync(_jsm, _js)},
                {"2", () => _publish.JetStreamPubSync(_jsm, _js) },
                {"q", () => _exit = true },
                {"Q", () => _exit = true },
            };
            var modes = new List<string>() {
                "JetStream Pub async",
                "JetStream Pub sync",
            };
            while (!_exit)
            {
                Console.Clear();

                Console.WriteLine("NATS JetStream demo producer");
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

        private static void Clear()
        {
            Console.Clear();
            _connection.Publish("nats.demo.clear", null);
        }
    }
}
