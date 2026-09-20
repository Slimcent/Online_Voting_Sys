using OnlineVoting.BackgroundTasks.Interfaces;
using OnlineVoting.Models.Dtos.Request.Email;
using OnlineVoting.Services.Interfaces;

namespace OnlineVoting.Services.BackgroundTasks
{
    public class SendCreateUserEmailTask : IBackgroundTask<CreateUserEmailRequest>
    {
        private readonly IEmailService _emailService;

        public SendCreateUserEmailTask(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public async Task ExecuteAsync(CreateUserEmailRequest request)
        {
            await _emailService.SendCreateUserEmail(request);
        }
    }
}