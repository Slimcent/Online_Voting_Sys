using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Moq;
using OnlineVoting.BackgroundTasks.Implementation;
using OnlineVoting.BackgroundTasks.Interfaces;

namespace OnlineVoting.Tests.UnitTests.BackgroundTasks
{
    public class HangfireBackgroundTaskQueueTests
    {
        [Fact]
        public void Enqueue_ShouldCreateHangfireJobAndReturnJobId()
        {
            Mock<IBackgroundJobClient> backgroundJobClient = new();

            backgroundJobClient.Setup(client => client.Create(It.IsAny<Job>(), It.IsAny<IState>()))
                .Returns("background-job-id");

            BackgroundTaskQueue queue = new(backgroundJobClient.Object);

            TestBackgroundTaskRequest request = new()
            {
                Value = "test-value"
            };

            string jobId = queue.Enqueue<TestBackgroundTask, TestBackgroundTaskRequest>(request);

            Assert.Equal("background-job-id", jobId);

            backgroundJobClient.Verify(client => client.Create(It.Is<Job>(job => job.Type == typeof(TestBackgroundTask)
                && job.Method.Name == nameof(TestBackgroundTask.ExecuteAsync)
                && job.Args.Count == 1
                && job.Args[0] == request),
            It.Is<IState>(state => state is EnqueuedState)), Times.Once);
        }

        public class TestBackgroundTask : IBackgroundTask<TestBackgroundTaskRequest>
        {
            public Task ExecuteAsync(TestBackgroundTaskRequest request)
            {
                return Task.CompletedTask;
            }
        }

        public class TestBackgroundTaskRequest
        {
            public string Value { get; set; } = string.Empty;
        }
    }
}