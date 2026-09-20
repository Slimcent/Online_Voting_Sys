using Microsoft.Extensions.Configuration;
using OnlineVoting.Services.Interfaces;

namespace OnlineVoting.Services.BackgroundTasks
{
    public class DeleteUnconfirmedUsersTask
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;

        public DeleteUnconfirmedUsersTask(IUserService userService, IConfiguration configuration)
        {
            _userService = userService;
            _configuration = configuration;
        }

        public async Task Execute()
        {
            int retentionDays = _configuration.GetValue<int?>("BackgroundJobs:UnconfirmedUserRetentionDays") ?? 5;

            await _userService.DeleteUnconfirmedUsers(retentionDays);
        }
    }
}