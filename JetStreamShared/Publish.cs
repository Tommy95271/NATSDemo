using NATS.Client;
using NATS.Client.JetStream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetStreamShared
{
    public class Publish
    {
        private readonly Helper _helper;

        public Publish(Helper helper)
        {
            _helper = helper;
        }
        public void JetStreamPubSync(IJetStreamManagement jsm, IJetStream js)
        {
            _helper.banner("JetStream publish sync demo");
            var streamResult = _helper.streamExists(jsm);
            var subjectResult = _helper.subjectExists(jsm, streamResult.streamName, null);
            if (streamResult.result && subjectResult.result)
            {
                Console.WriteLine("Please type in any text to publish in JetStream.");
                var text = Console.ReadLine();
                byte[] data = Encoding.UTF8.GetBytes(text);
                Msg msg = new Msg(subjectResult.subjectName, null, null, data);
                PublishAck pa = js.Publish(msg);
                Console.WriteLine("Published message '{0}' on subject '{1}', stream '{2}', seqno '{3}'.",
                    Encoding.UTF8.GetString(data), subjectResult.subjectName, pa.Stream, pa.Seq);
            }
        }

        public void JetStreamPubAsync(IJetStreamManagement jsm, IJetStream js)
        {
            _helper.banner("JetStream publish async demo");
            var streamResult = _helper.streamExists(jsm);
            var subjectResult = _helper.subjectExists(jsm, streamResult.streamName, null);
            if (streamResult.result && subjectResult.result)
            {
                Console.WriteLine("Please type in any text to publish in JetStream.");
                var text = Console.ReadLine();

                byte[] data = Encoding.UTF8.GetBytes(text);
                Msg msg = new Msg(subjectResult.subjectName, null, null, data);

                Task<PublishAck> pa = js.PublishAsync(msg);
                Console.WriteLine("Published message '{0}' on subject '{1}', stream '{2}', seqno '{3}'.",
                    Encoding.UTF8.GetString(data), subjectResult.subjectName, pa.Result.Stream, pa.Result.Seq);
            }
        }
    }
}
