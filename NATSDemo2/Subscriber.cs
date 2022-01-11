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
        private static IConnection _connection;
        private static bool _exit = false;

        internal static void Run()
        {
            using (_connection = ConnectToNats())
            {
                SubscribePubSub();
                SubscribeRequestResponse();
                SubscribeQueueGroups();
                SubscribeClear();

                Console.Clear();
                Console.WriteLine($"Connected to {_connection.ConnectedUrl}.");
                Console.WriteLine("Consumers started");
                Console.ReadKey(true);
                _exit = true;

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

        private static void SubscribePubSub()
        {
            EventHandler<MsgHandlerEventArgs> handler = (sender, args) =>
            {
                string data = Encoding.UTF8.GetString(args.Message.Data);
                LogMessage(data);
            };

            IAsyncSubscription sub = _connection.SubscribeAsync("nats.demo.pubsub", handler);
        }

        private static void SubscribeRequestResponse()
        {
            EventHandler<MsgHandlerEventArgs> handler = (sender, args) =>
            {
                string data = Encoding.UTF8.GetString(args.Message.Data);
                LogMessage(data);

                string replySubject = args.Message.Reply;
                if (replySubject != null)
                {
                    byte[] responseData = Encoding.UTF8.GetBytes($"ACK for {data}");
                    _connection.Publish(replySubject, responseData);
                }
            };

            IAsyncSubscription s = _connection.SubscribeAsync(
                "nats.demo.requestresponse", "request-response-queue", handler);
        }
        private static void SubscribeQueueGroups()
        {
            EventHandler<MsgHandlerEventArgs> handler = (sender, args) =>
            {
                string data = Encoding.UTF8.GetString(args.Message.Data);
                LogMessage(data);
            };

            IAsyncSubscription s = _connection.SubscribeAsync(
                "nats.demo.queuegroups", "load-balancing-queue", handler);
        }

        private static void SubscribeClear()
        {
            EventHandler<MsgHandlerEventArgs> handler = (sender, args) =>
            {
                Console.Clear();
            };

            IAsyncSubscription s = _connection.SubscribeAsync(
                "nats.demo.clear", handler);
        }

        private static void LogMessage(string message)
        {
            Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.fffffff")} - {message}");
        }
    }
}
