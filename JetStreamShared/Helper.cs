using NATS.Client.JetStream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JetStreamShared
{
    public class Helper
    {
        public string[] _invalidChar = new string[] { " ", ".", ">", "*" };

        public void banner(string title)
        {
            Console.Clear();
            Console.WriteLine(title);
            Console.WriteLine("============");
        }

        /// <summary>
        /// Examine if the stream exists
        /// </summary>
        /// <returns></returns>
        public (bool result, string? streamName) streamExists(IJetStreamManagement jsm)
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
        public (bool result, string? subjectName) subjectExists(IJetStreamManagement jsm, string streamName, string? consumer)
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
                    if (!string.IsNullOrWhiteSpace(consumer))
                    {
                        createConsumer(jsm, consumer, chosenSubject, streamName);
                    }
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
        /// Create a consumer
        /// </summary>
        /// <param name="consumer"></param>
        /// <param name="chosenSubject"></param>
        /// <param name="streamName"></param>
        public void createConsumer(IJetStreamManagement jsm, string consumer, string chosenSubject, string streamName)
        {
            ConsumerConfiguration cc = ConsumerConfiguration.Builder()
                .WithAckWait(2500)
                .WithDurable(consumer)
                .WithFilterSubject(chosenSubject)
                .Build();
            jsm.AddOrUpdateConsumer(streamName, cc);
        }

        /// <summary>
        /// Examine if the consumer exists
        /// </summary>
        /// <returns></returns>
        public (bool result, string? consumerName) consumerExists(IJetStreamManagement jsm, string streamName)
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

        public (bool result, StorageType? storageType) chooseStorageType()
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

        public (bool result, string[]? subjects) setSubjects()
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
        public (bool result, string? streamName) streamExistsBeforeCreating(IJetStreamManagement jsm)
        {
            Console.WriteLine("Please type in a stream name.");
            var streamName = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(streamName) || _invalidChar.Any(i => streamName.Contains(i)))
            {
                Console.WriteLine("Space, period, > or * is not allowed, please type in a valid stream name.");
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
    }
}
