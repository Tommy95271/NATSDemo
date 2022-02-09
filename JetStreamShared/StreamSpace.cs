using NATS.Client.JetStream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetStreamShared
{
    public class StreamSpace
    {
        private readonly Helper _helper;

        public StreamSpace(Helper helper)
        {
            _helper = helper;
        }
        public void JetStreamListStreams(IJetStreamManagement jsm)
        {
            _helper.banner("JetStream list all stream demo");
            var streamNames = jsm.GetStreamNames();
            var count = 1;
            foreach (var streamName in streamNames)
            {
                Console.WriteLine($"{count}) {streamName}");
                count++;
            }
        }

        public void JetStreamCreateStream(IJetStreamManagement jsm)
        {
            _helper.banner("JetStream create stream demo");

            var streamResult = _helper.streamExistsBeforeCreating(jsm);
            if (streamResult.result)
            {
                var storageTypeResult = _helper.chooseStorageType();
                if (storageTypeResult.result)
                {
                    var subjectsResult = _helper.setSubjects();
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

        public void JetStreamDeleteStream(IJetStreamManagement jsm)
        {
            _helper.banner("JetStream delete stream demo");
            var streamResult = _helper.streamExists(jsm);
            if (streamResult.result)
            {
                jsm.DeleteStream(streamResult.streamName);
            }
        }

    }
}
