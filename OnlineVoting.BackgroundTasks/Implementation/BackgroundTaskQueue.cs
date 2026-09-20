using Hangfire;
using OnlineVoting.BackgroundTasks.Interfaces;

namespace OnlineVoting.BackgroundTasks.Implementation
{
    public class BackgroundTaskQueue : IBackgroundTaskQueue
    {
        private readonly IBackgroundJobClient _backgroundJobClient;

        public BackgroundTaskQueue(IBackgroundJobClient backgroundJobClient)
        {
            _backgroundJobClient = backgroundJobClient;
        }

        public string Enqueue<TTask, TRequest>(TRequest request) where TTask : class, IBackgroundTask<TRequest> // Whatever type is supplied as TTask must be a class and must implement IBackgroundTask<TRequest>.
        {
            return _backgroundJobClient.Enqueue<TTask>(x => x.ExecuteAsync(request));
        }

        public IReadOnlyCollection<string> EnqueueRange<TTask, TRequest>(IEnumerable<TRequest> requests)
            where TTask : class, IBackgroundTask<TRequest>
        {
            List<string> jobIds = new();

            foreach (TRequest request in requests)
            {
                string jobId = Enqueue<TTask, TRequest>(request);

                jobIds.Add(jobId);
            }

            return jobIds;
        }
    }
}