using Hangfire;
using OnlineVoting.Services.BackgroundTasks;

namespace OnlineVoting.Api.Extensions
{
    public static class RecurringJobExtensions
    {
        public static void RegisterRecurringJobs(this WebApplication app)
        {
            IRecurringJobManager recurringJobManager = app.Services.GetRequiredService<IRecurringJobManager>();

            string updateInactiveStudentsCron = app.Configuration["BackgroundJobs:UpdateInactiveStudents"] ?? "0 0 * * *";
            string deleteUnconfirmedUsersCron = app.Configuration["BackgroundJobs:DeleteUnconfirmedUsers"] ?? "0 0 * * *";

            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById("Africa/Lagos");

            // update inactive students job
            recurringJobManager.AddOrUpdate<UpdateInactiveStudentsTask>("update-inactive-students", x => x.Execute(), updateInactiveStudentsCron,
            new RecurringJobOptions
            {
                TimeZone = timeZone
            });

            recurringJobManager.AddOrUpdate<DeleteUnconfirmedUsersTask>("delete-unconfirmed-users", x => x.Execute(), deleteUnconfirmedUsersCron,
            new RecurringJobOptions
            {
                TimeZone = timeZone
            });
        }
    }
}