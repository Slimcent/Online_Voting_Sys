using Hangfire;
using OnlineVoting.Services.BackgroundTasks;

namespace OnlineVoting.Api.Extensions
{
    public static class RecurringJobExtensions
    {
        public static void RegisterRecurringJobs(this WebApplication app)
        {
            // update inactive students job
            string updateInactiveStudentsCron = app.Configuration["BackgroundJobs:UpdateInactiveStudents"]
                ?? "0 0 * * *";

            TimeZoneInfo nigeriaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Africa/Lagos");

            RecurringJob.AddOrUpdate<UpdateInactiveStudentsTask>("update-inactive-students", x => x.Execute(), updateInactiveStudentsCron,
            new RecurringJobOptions
            {
                TimeZone = nigeriaTimeZone
            });

            // delete unconfirmed users job
            string deleteUnconfirmedUsersCron = app.Configuration["BackgroundJobs:DeleteUnconfirmedUsers"] ?? "0 0 * * *";

            RecurringJob.AddOrUpdate<DeleteUnconfirmedUsersTask>("delete-unconfirmed-users", x => x.Execute(), deleteUnconfirmedUsersCron,
            new RecurringJobOptions
            {
                TimeZone = nigeriaTimeZone
            });
        }
    }
}