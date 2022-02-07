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
        private const string ALLOWED_OPTIONS = "1234qQ";
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

                    Console.WriteLine("NATS JetStream demo consumer");
                    Console.WriteLine("==================");
                    Console.WriteLine("Select mode:");
                    Console.WriteLine("1) JetStream create stream");
                    Console.WriteLine("2) JetStream create consumer");
                    Console.WriteLine("3) JetStream subscribe a subject from stream");
                    Console.WriteLine("4) JetStream delete consumer");
                    Console.WriteLine("q) Quit");

                    string input;
                    do
                    {
                        input = Console.ReadLine();
                    } while (!ALLOWED_OPTIONS.Contains(input));

                    switch (input)
                    {
                        case "1":
                            JetStreamCreateStream();
                            break;
                        case "2":
                            JetStreamCreateConsumer();
                            break;
                        case "3":
                            JetStreamSub();
                            break;
                        case "4":
                            JetStreamDeleteConsumer();
                            break;
                        case "q":
                        case "Q":
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

        private static void JetStreamCreateStream()
        {
            banner("JetStream Create Stream demo");

            var streamResult = streamExistsBeforeCreating();
            if (streamResult.result)
            {
                var storageTypeResult = chooseStorageType();
                if (storageTypeResult.result)
                {
                    var subjectsResult = setSubjects();
                    if (subjectsResult.result)
                    {
                        StreamConfiguration sc = StreamConfiguration.Builder()
                            .WithName(streamResult.streamName)
                            .WithStorageType(storageTypeResult.storageType)
                            .WithSubjects(subjectsResult.subjects)
                            .Build();
                        var streamInfo = jsm.AddStream(sc);
                    }
                }
            }
        }

        private static void JetStreamCreateConsumer()
        {
            banner("JetStream Create Consumer demo");

            var streamResult = streamExists();

            if (streamResult.result)
            {
                Console.WriteLine("Please type in a consumer name.");
                var consumer = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(consumer))
                {
                    Console.WriteLine("Please type in a valid consumer name.");
                }
                else
                {
                    subjectExists(streamResult.streamName, consumer);
                }
            }
        }

        private static void JetStreamSub()
        {
            banner("JetStream Subscribe demo");
            var streamResult = streamExists();
            var consumerResult = consumerExists(streamResult.streamName);
            if (streamResult.result && consumerResult.result)
            {
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

                Console.WriteLine("How many messages do you want to consume?");
                var count = Console.ReadLine();
                Regex regex = new Regex(@"^[0-9]+$");
                if (regex.IsMatch(count))
                {
                    var countInt = int.Parse(count);
                    IList<Msg> list = sub.Fetch(countInt, 1000);
                    if (countInt > list.Count)
                    {
                        Console.WriteLine($"The count in subject: {subjectResult.subjectName} is {list.Count}, please type in smaller number.");
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

        private static void JetStreamDeleteConsumer()
        {
            banner("JetStream Delete Consumer demo");
            var streamResult = streamExists();
            var consumerResult = consumerExists(streamResult.streamName);
            if (streamResult.result && consumerResult.result)
            {
                jsm.DeleteConsumer(streamResult.streamName, consumerResult.consumerName);
            }
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
            Console.WriteLine("Which subject do you want to choose? Please type in 1 to 9.");
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
            var consumers = jsm.GetConsumerNames(streamName);
            if (consumers.Count == 0)
            {
                Console.WriteLine("There is no consumer.");
                return (false, null);
            }
            else
            {
                Console.WriteLine("Which consumer do you want to choose?");
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
        }

        private static (bool result, StorageType? storageType) chooseStorageType()
        {
            Console.WriteLine("Which Storage Type do you want to choose?");
            var count = 0;
            foreach (StorageType storageType in Enum.GetValues(typeof(StorageType)))
            {
                count += 1;
                Console.WriteLine($"{count}) Storage Type: {storageType}");
            }

            var st = Console.ReadLine();
            Regex regex = new Regex(@"^[1-2]+$");
            var stLength = Enum.GetValues(typeof(StorageType)).Length;
            if (regex.IsMatch(st))
            {
                var stInt = int.Parse(st);
                if (stInt > stLength)
                {
                    Console.WriteLine("Please type in smaller number.");
                    return (false, null);
                }
                else
                {
                    var chosenStorageType = stInt == 1 ? StorageType.Memory : StorageType.File;
                    return (true, chosenStorageType);
                }
            }
            else
            {
                Console.WriteLine("Please type in digitals from 1 to 2.");
                return (false, null);
            }
        }

        private static (bool result, string[]? subjects) setSubjects()
        {
            Console.WriteLine("Please type in subjects separated by comma or space.");
            var subjects = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(subjects))
            {
                Console.WriteLine("Please enter valid subjects separated by comma or space.");
                return (false, null);
            }
            else
            {
                var subjectsArray = subjects.Split(new char[] { ',', ' ' });
                return (true, subjectsArray);
            }
        }


        /// <summary>
        /// Examine if the stream exists before creating
        /// </summary>
        /// <returns></returns>
        private static (bool result, string? streamName) streamExistsBeforeCreating()
        {
            Console.WriteLine("Please type in a stream name.");
            var streamName = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(streamName))
            {
                Console.WriteLine("Please type in a valid stream name.");
                return (false, null);
            }
            var streamNames = jsm.GetStreamNames();
            if (streamNames.Contains(streamName))
            {
                Console.WriteLine("The stream already exists.");
                return (false, null);
            }
            else
            {
                return (true, streamName);
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
