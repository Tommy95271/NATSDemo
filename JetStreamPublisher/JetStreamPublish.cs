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
        private const string ALLOWED_OPTIONS = "123qQ";
        private static bool exit = false;

        internal static void Run()
        {
            using (_connection = ConnectToNats())
            {
                while (!exit)
                {
                    Console.Clear();

                    Console.WriteLine("NATS JetStream demo producer");
                    Console.WriteLine("==================");
                    Console.WriteLine("Select mode:");
                    Console.WriteLine("1) JetStream Pub / Sub");
                    Console.WriteLine("q) Quit");

                    ConsoleKeyInfo input;
                    do
                    {
                        input = Console.ReadKey(true);
                    } while (!ALLOWED_OPTIONS.Contains(input.KeyChar));

                    switch (input.KeyChar)
                    {
                        case '1':
                            JetStreamPub(_connection);
                            break;
                        case '2':
                            break;
                        case '3':
                            break;
                        case 'q':
                        case 'Q':
                            exit = true;
                            continue;
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
        private static void JetStreamPub(IConnection _connection)
        {
            var subject = "nats.demo.subject";
            var stream = "nats-demo-stream";
            Console.Clear();
            Console.WriteLine("JetStreamPub demo");
            Console.WriteLine("============");

            JsUtils.CreateStreamOrUpdateSubjects(_connection, stream, subject);
            IJetStream js = _connection.CreateJetStreamContext();
            Console.WriteLine("Please type any text to publish in JetStream.");
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
