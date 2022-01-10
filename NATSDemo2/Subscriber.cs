using NATS.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NATSDemo2
{
    internal class Subscriber
    {
        private static bool _exit = false;
        private static IConnection _connection;
        internal static void Run()
        {
            using (_connection = ConnectToNats())
            {
                SubscribePubSub();
                _exit = true;
                _connection.Drain();

            }
        }
        private static IConnection ConnectToNats()
        {
            ConnectionFactory factory = new ConnectionFactory();

            var options = ConnectionFactory.GetDefaultOptions();
            options.Url = "nats://localhost:4222";

            return factory.CreateConnection(options);
        }

        private static void SubscribePubSub()
        {
            ISyncSubscription sub = _connection.SubscribeSync("nats.demo.pubsub");
            //while (!_exit)
            {
                var message = sub.NextMessage();
                if (message != null)
                {
                    string data = Encoding.UTF8.GetString(message.Data);
                    LogMessage(data);
                }
            }
        }

        private static void LogMessage(string message)
        {
            Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.fffffff")} - {message}");
        }
    }
}
