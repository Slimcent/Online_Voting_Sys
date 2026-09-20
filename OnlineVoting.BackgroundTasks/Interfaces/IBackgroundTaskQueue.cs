namespace OnlineVoting.BackgroundTasks.Interfaces
{
    public interface IBackgroundTaskQueue
    {
        string Enqueue<TTask, TRequest>(TRequest request) where TTask : class, IBackgroundTask<TRequest>;

        IReadOnlyCollection<string> EnqueueRange<TTask, TRequest>(IEnumerable<TRequest> requests)
            where TTask : class, IBackgroundTask<TRequest>;
    }
}