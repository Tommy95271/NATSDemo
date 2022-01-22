namespace JetStreamSubscriberHosted.Client.Services
{
    public interface IStreamService
    {
        public Task<IEnumerable<string>> GetStreamNames();
        public Task<IEnumerable<string>> GetSubjectNames(string streamName);
        public Task<IEnumerable<string>> GetConsumerNames(string streamName);
    }
}
