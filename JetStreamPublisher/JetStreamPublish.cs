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
    internal class JetStreamPublish
    {
        private static IConnection? _connection;
        private const string _allowedOptions = "123qQ";
        private static Dictionary<string, Action> _jetStreamAction;
        private static bool _exit = false;

        internal static void Run()
        {
            using (_connection = ConnectToNats())
            {
                _jetStreamAction = new Dictionary<string, Action>()
                {
                    {"1", JetStreamPubAsync},
                    //{"2", JetStreamListConsumers },
                    //{"3", JetStreamCreateStream },
                    //{"4", JetStreamCreateConsumer },
                    //{"5", JetStreamSubPullBased },
                    //{"6", JetStreamSubPushBased },
                    //{"7", JetStreamDeleteStream },
                    //{"8", JetStreamDeleteConsumer },
                    {"q", () => _exit = true },
                    {"Q", () => _exit = true },
                };
                var modes = new List<string>() {
                    "JetStream Pub async",
                    "JetStream Pub sync", };
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
        }

        private static IConnection ConnectToNats()
        {
            ConnectionFactory factory = new ConnectionFactory();

            var options = ConnectionFactory.GetDefaultOptions();
            options.Url = "nats://localhost:4222";

            return factory.CreateConnection(options);
        }

        private static void JetStreamPubSync()
        {

        }

        private static void JetStreamPubAsync()
        {
            var subject = "nats.demo.subject";
            var stream = "nats-demo-stream";
            Console.Clear();
            Console.WriteLine("JetStreamPub demo");
            Console.WriteLine("============");

            JsUtils.CreateStreamOrUpdateSubjects(_connection, stream, subject);
            IJetStream js = _connection.CreateJetStreamContext();
            Console.WriteLine("Please type in any text to publish in JetStream.");
            var text = Console.ReadLine();

            byte[] data = Encoding.UTF8.GetBytes(text);
            Msg msg = new Msg(subject, null, null, data);

            Task<PublishAck> pa = js.PublishAsync(msg);
            Console.WriteLine("Published message '{0}' on subject '{1}', stream '{2}', seqno '{3}'.",
                Encoding.UTF8.GetString(data), subject, pa.Result.Stream, pa.Result.Seq);

        }

        private static void Clear()
        {
            Console.Clear();
            _connection.Publish("nats.demo.clear", null);
        }
    }
}
