using ClassLibrary1;
using NATS.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NATSDemo1
{
    internal class Publisher
    {
        private static IConnection? _connection;

        internal static void Run()
        {
            using (_connection = ConnectToNats())
            {
                PubSub();
                _connection.Drain(5000);

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

            var company = new Company();
            var jsonString = JsonSerializer.Serialize(company);
            var bytes = Encoding.UTF8.GetBytes(jsonString);
            _connection?.Publish("nats.demo.pubsub", bytes);
        }
    }
}
