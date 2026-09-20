namespace OnlineVoting.BackgroundTasks.Configuration
{
    public class BackgroundTaskOptions
    {
        public const string SectionName = "BackgroundTasks";
        public const string ConnectionStringName = "VotingConnection";

        public string SchemaName { get; set; } = "HangFire";

        public int WorkerCount { get; set; } = 4;

        public int QueuePollIntervalSeconds { get; set; } = 5;

        public int RetryAttempts { get; set; } = 3;

        public int[] RetryDelaysSeconds { get; set; } = [10, 30, 60];
    }
}