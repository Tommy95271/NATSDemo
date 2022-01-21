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
    internal class JetStreamSubscribe
    {
        private static IConnection? _connection;
        private const string ALLOWED_OPTIONS = "123qQ";
        private static bool exit = false;
        private static IJetStreamManagement jsm { get; set; }

        internal static void Run()
        {
            using (_connection = ConnectToNats())
            {
                jsm = _connection.CreateJetStreamManagementContext();
                while (!exit)
                {
                    Console.Clear();

                    Console.WriteLine("NATS JetStream demo producer");
                    Console.WriteLine("==================");
                    Console.WriteLine("Select mode:");
                    Console.WriteLine("1) JetStream create consumer");
                    Console.WriteLine("2) JetStream subscribe a subject from stream");
                    Console.WriteLine("3) JetStream delete consumer");
                    Console.WriteLine("q) Quit");

                    ConsoleKeyInfo input;
                    do
                    {
                        input = Console.ReadKey(true);
                    } while (!ALLOWED_OPTIONS.Contains(input.KeyChar));

                    switch (input.KeyChar)
                    {
                        case '1':
                            JetStreamConsumer();
                            break;
                        case '2':
                            JetStreamSub();
                            break;
                        case '3':
                            JetStreamDel();
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

        private static void JetStreamConsumer()
        {
            banner("JetStream Consumer demo");

            var streamResult = streamExists();

            if (streamResult.result)
            {
                Console.WriteLine("Please type in a consumer name.");
                var consumer = Console.ReadLine();
                subjectExists(streamResult.streamName, consumer);
            }
        }

        private static void JetStreamSub()
        {
            banner("JetStream Subscribe demo");
            var streamResult = streamExists();
            if (streamResult.result)
            {
                Console.WriteLine("How many messages do you want to consume?");
                var count = Console.ReadLine();
                Regex regex = new Regex(@"^[0-9]+$");
                if (regex.IsMatch(count))
                {
                    var consumerResult = consumerExists(streamResult.streamName);
                    ConsumerConfiguration cc = ConsumerConfiguration.Builder()
                        .WithAckWait(2500)
                        .Build();
                    PullSubscribeOptions pullOptions = PullSubscribeOptions.Builder()
                        .WithDurable(consumerResult.consumerName) // required
                        .WithConfiguration(cc)
                        .Build();
                    var subjectResult = subjectExists(streamResult.streamName, consumerResult.consumerName);
                    // subscribe
                    IJetStreamPullSubscription sub = _connection.CreateJetStreamContext().PullSubscribe(subjectResult.subjectName, pullOptions);
                    var countInt = int.Parse(count);
                    IList<Msg> list = sub.Fetch(countInt, 1000);
                    if (countInt > list.Count)
                    {
                        Console.WriteLine($"The count in subject: {sub.Subject} is {list.Count}, please type in smaller number.");
                    }
                    else
                    {
                        for (int i = 0; i < list.Count; i++)
                        {
                            Console.WriteLine($"{i}. Message: {list[i]}");
                            list[i].Ack();
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Please type in digitals from 0 to 9.");
                }
            }
        }

        private static void JetStreamDel()
        {
            banner("JetStream Delete Consumer demo");
            var streamResult = streamExists();
            var consumerResult = consumerExists(streamResult.streamName);
            jsm.DeleteConsumer(streamResult.streamName, consumerResult.consumerName);
        }

        #region Helpers

        private static void banner(string title)
        {
            Console.Clear();
            Console.WriteLine(title);
            Console.WriteLine("============");
        }

        /// <summary>
        /// Examine if the stream exists
        /// </summary>
        /// <returns></returns>
        private static (bool result, string? streamName) streamExists()
        {
            Console.WriteLine("Which stream do you want to choose?");
            var streamNames = jsm.GetStreamNames();
            for (int i = 0; i < streamNames.Count; i++)
            {
                Console.WriteLine($"{i + 1}) {streamNames[i]}");
            }

            var stream = Console.ReadLine();
            Regex regex = new Regex(@"^[1-9]+$");

            if (regex.IsMatch(stream))
            {
                var streamInt = int.Parse(stream);
                if (streamInt > streamNames.Count)
                {
                    Console.WriteLine("Please type in smaller number.");
                    return (false, null);
                }
                else
                {
                    if (streamNames.IndexOf(streamNames[streamInt - 1]) >= 0)
                    {
                        var streamName = streamNames[streamInt - 1];
                        return (true, streamName);
                    }
                    return (false, null);
                }
            }
            else
            {
                Console.WriteLine("Please type in digitals from 1 to 9.");
                return (false, null);
            }
        }

        /// <summary>
        /// Examine if the subject exists
        /// </summary>
        /// <returns></returns>
        private static (bool result, string? subjectName) subjectExists(string streamName, string consumer)
        {
            Console.WriteLine("Which subject do you want to choose?");
            var subjects = jsm.GetStreamInfo(streamName).Config.Subjects;
            for (int i = 0; i < subjects.Count; i++)
            {
                Console.WriteLine($"{i + 1}) {subjects[i]}");
            }

            var subject = Console.ReadLine();
            Regex regex = new Regex(@"^[1-9]+$");
            if (regex.IsMatch(subject))
            {
                var subjectInt = int.Parse(subject);
                if (subjectInt > subjects.Count)
                {
                    Console.WriteLine("Please type in smaller number.");
                    return (false, null);
                }
                else
                {
                    var chosenSubject = subjects[subjectInt - 1];
                    ConsumerConfiguration cc = ConsumerConfiguration.Builder()
                        .WithAckWait(2500)
                        .WithDurable(consumer)
                        .WithFilterSubject(chosenSubject)
                        .Build();
                    jsm.AddOrUpdateConsumer(streamName, cc);
                    return (true, chosenSubject);
                }
            }
            else
            {
                Console.WriteLine("Please type in digitals from 1 to 9.");
                return (false, null);
            }
        }

        /// <summary>
        /// Examine if the consumer exists
        /// </summary>
        /// <returns></returns>
        private static (bool result, string? consumerName) consumerExists(string streamName)
        {
            Console.WriteLine("Which consumer do you want to choose?");
            var consumers = jsm.GetConsumerNames(streamName);
            for (int i = 0; i < consumers.Count; i++)
            {
                Console.WriteLine($"{i + 1}) {consumers[i]}");
            }

            var consumer = Console.ReadLine();
            Regex regex = new Regex(@"^[1-9]+$");
            if (regex.IsMatch(consumer))
            {
                var consumerInt = int.Parse(consumer);
                if (consumerInt > consumers.Count)
                {
                    Console.WriteLine("Please type in smaller number.");
                    return (false, null);
                }
                else
                {
                    return (true, consumers[consumerInt - 1]);
                }
            }
            else
            {
                Console.WriteLine("Please type in digitals from 1 to 9.");
                return (false, null);
            }
        }

        #endregion

        private static void Clear()
        {
            Console.Clear();
            _connection.Publish("nats.demo.clear", null);
        }
    }
}
