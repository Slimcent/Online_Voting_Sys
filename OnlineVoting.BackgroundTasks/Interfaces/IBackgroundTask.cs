namespace OnlineVoting.BackgroundTasks.Interfaces
{
    public interface IBackgroundTask<TRequest>
    {
        Task ExecuteAsync(TRequest request);
    }

    public interface IBackgroundTask
    {
        Task ExecuteAsync();
    }
}