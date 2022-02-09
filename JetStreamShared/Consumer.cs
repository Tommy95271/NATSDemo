using NATS.Client.JetStream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetStreamShared
{
    public class Consumer
    {
        private readonly Helper _helper;

        public Consumer(Helper helper)
        {
            _helper = helper;
        }
        public void JetStreamListConsumers(IJetStreamManagement jsm)
        {
            _helper.banner("JetStream list all consumers demo");
            var streamResult = _helper.streamExists(jsm);
            if (streamResult.result)
            {
                var consumerNames = jsm.GetConsumerNames(streamResult.streamName);
                if (consumerNames.Count == 0)
                {
                    Console.WriteLine("There is no consumer in the stream.");
                }
                else
                {
                    Console.WriteLine("Below are consumers in the stream.");
                    var count = 1;
                    foreach (var consumerName in consumerNames)
                    {
                        Console.WriteLine($"{count}) {consumerName}");
                        count++;
                    }
                }
            }
        }

        public void JetStreamCreateConsumer(IJetStreamManagement jsm)
        {
            _helper.banner("JetStream create consumer demo");

            var streamResult = _helper.streamExists(jsm);

            if (streamResult.result)
            {
                Console.WriteLine("Please type in a consumer name.");
                var consumer = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(consumer) || _helper._invalidChar.Any(i => consumer.Contains(i)))
                {
                    Console.WriteLine("Space, period, > or * is not allowed, please type in a valid consumer name.");
                }
                else
                {
                    _helper.subjectExists(jsm, streamResult.streamName, consumer);
                }
            }
        }

        public void JetStreamDeleteConsumer(IJetStreamManagement jsm)
        {
            _helper.banner("JetStream Delete Consumer demo");
            var streamResult = _helper.streamExists(jsm);
            var consumerResult = _helper.consumerExists(jsm, streamResult.streamName);
            if (streamResult.result && consumerResult.result)
            {
                jsm.DeleteConsumer(streamResult.streamName, consumerResult.consumerName);
            }
        }
    }
}
