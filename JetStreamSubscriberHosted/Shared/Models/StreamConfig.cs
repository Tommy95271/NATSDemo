using JetStreamSubscriberHosted.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetStreamSubscriberHosted.Shared.Models
{
    public class StreamConfig
    {
        public string StreamName { get; set; }
        public StorageType StorageType { get; set; }
        public List<string> Subjects { get; set; }
    }
}
