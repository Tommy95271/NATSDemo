using NATS.Client;
using NATS.Client.JetStream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JetStreamShared
{
    public class Subscribe
    {
        private readonly Helper _helper;

        public Subscribe(Helper helper)
        {
            _helper = helper;
        }

        public void JetStreamSubPullBased(IJetStreamManagement jsm, IJetStream js)
        {
            _helper.banner("JetStream subscribe in pull-based demo");
            var streamResult = _helper.streamExists(jsm);
            var consumerResult = _helper.consumerExists(jsm, streamResult.streamName);
            if (streamResult.result && consumerResult.result)
            {
                ConsumerConfiguration cc = ConsumerConfiguration.Builder()
                    .WithAckWait(2500)
                    .Build();
                PullSubscribeOptions pullOptions = PullSubscribeOptions.Builder()
                    .WithDurable(consumerResult.consumerName) // required
                    .WithConfiguration(cc)
                    .Build();
                var subjectResult = _helper.subjectExists(jsm, streamResult.streamName, consumerResult.consumerName);
                // subscribe
                IJetStreamPullSubscription sub = js.PullSubscribe(subjectResult.subjectName, pullOptions);

                Console.WriteLine("How many messages do you want to consume?");
                var count = Console.ReadLine();
                Regex regex = new Regex(@"^[0-9]+$");
                if (regex.IsMatch(count))
                {
                    var countInt = int.Parse(count);
                    if (countInt == 0)
                    {
                        Console.WriteLine("0 message is not allowed!");
                    }
                    else
                    {
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
                }
                else
                {
                    Console.WriteLine("Please type in digitals from 0 to 9.");
                }
            }
        }

        public void JetStreamSubPushBased(IJetStreamManagement jsm, IJetStream js)
        {
            _helper.banner("JetStream subscribe in push-based demo");
            var streamResult = _helper.streamExists(jsm);
            if (streamResult.result)
            {
                PushSubscribeOptions pushOptions = PushSubscribeOptions.Builder()
                    .WithStream(streamResult.streamName)
                    .WithDurable(null) // not required in push-based
                    .Build();
                var subjectResult = _helper.subjectExists(jsm, streamResult.streamName, null);
                // subscribe
                IJetStreamPushSyncSubscription sub = js.PushSubscribeSync(subjectResult.subjectName, pushOptions);

                Console.WriteLine("How many messages do you want to consume?");
                var count = Console.ReadLine();
                Regex regex = new Regex(@"^[0-9]+$");
                if (regex.IsMatch(count))
                {
                    var countInt = int.Parse(count);
                    if (countInt == 0)
                    {
                        Console.WriteLine("0 message is not allowed!");
                    }
                    else
                    {
                        for (int i = 0; i < countInt; i++)
                        {
                            Msg msg = sub.NextMessage(1000);
                            Console.WriteLine("\nMessage Received:");
                            if (msg.HasHeaders)
                            {
                                Console.WriteLine("  Headers:");
                                foreach (string key in msg.Header.Keys)
                                {
                                    foreach (string value in msg.Header.GetValues(key))
                                    {
                                        Console.WriteLine($"    {key}: {value}");
                                    }
                                }
                            }

                            Console.WriteLine("  Subject: {0}\n  Data: {1}\n", msg.Subject, Encoding.UTF8.GetString(msg.Data));
                            Console.WriteLine("  " + msg.MetaData);

                            // Because this is a synchronous subscriber, there's no auto-ack.
                            // The default Consumer Configuration AckPolicy is Explicit
                            // so we need to ack the message or it'll be redelivered.
                            msg.Ack();
                        }

                        sub.Unsubscribe();
                        //_connection.Flush(5000);
                    }
                }
                else
                {
                    Console.WriteLine("Please type in digitals from 0 to 9.");
                }
            }
        }
    }
}
