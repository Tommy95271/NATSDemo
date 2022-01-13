using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using ClassLibrary1;
using NATS.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Publisher
{
    public class Publish
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

                    Console.WriteLine("NATS demo producer");
                    Console.WriteLine("==================");
                    Console.WriteLine("Select mode:");
                    Console.WriteLine("1) Pub / Sub");
                    Console.WriteLine("2) Request / Response");
                    Console.WriteLine("3) Load-balancing (queue groups)");
                    Console.WriteLine("q) Quit");

                    ConsoleKeyInfo input;
                    do
                    {
                        input = Console.ReadKey(true);
                    } while (!ALLOWED_OPTIONS.Contains(input.KeyChar));

                    switch (input.KeyChar)
                    {
                        case '1':
                            PubSub();
                            break;
                        case '2':
                            RequestResponse();
                            break;
                        case '3':
                            QueueGroups();
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
        private static void PubSub()
        {
            Console.Clear();
            Console.WriteLine("Pub/Sub demo");
            Console.WriteLine("============");

            var text = Console.ReadLine();
            ConsolePublish(text, "nats.demo.pubsub");
        }

        private static void QueueGroups()
        {
            Console.Clear();
            Console.WriteLine("Load-balancing demo");
            Console.WriteLine("===================");
            var text = Console.ReadLine();
            while (text.ToLower() != "q")
            {
                ConsolePublish(text, "nats.demo.queuegroups");
                text = Console.ReadLine();
                if (text.ToLower() == "q") break;
            }
        }

        /// <summary>
        /// Console WriteLine and Publish
        /// </summary>
        /// <param name="text">The content to publish</param>
        /// <param name="subject">The subject to publish to</param>
        private static void ConsolePublish(string text, string subject)
        {
            Console.WriteLine($"Sending: {text}");
            byte[] data = Encoding.UTF8.GetBytes(text);
            _connection?.Publish(subject, data);
        }

        private static void RequestResponse()
        {
            Console.Clear();
            Console.WriteLine("Request/Response demo");
            Console.WriteLine("================================");

            var text = Console.ReadLine();
            Console.WriteLine($"Sending: {text}");
            byte[] data = Encoding.UTF8.GetBytes(text);
            var response = _connection.Request("nats.demo.requestresponse", data, 5000);
            var responseMsg = Encoding.UTF8.GetString(response.Data);
            Console.WriteLine($"Response: {responseMsg}");
        }


        private static void Clear()
        {
            Console.Clear();
            _connection.Publish("nats.demo.clear", null);
        }


        [Benchmark]
        public void BenchmarkPubSub()
        {
            var million = 1000000;

            for (int i = 0; i < million; i++)
            {
                var bytes = Encoding.UTF8.GetBytes($"Publish{i}");
                _connection?.Publish("nats.demo.pubsub", bytes);
            }
        }
    }
}
