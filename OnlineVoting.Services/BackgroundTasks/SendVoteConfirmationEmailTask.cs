using OnlineVoting.BackgroundTasks.Interfaces;
using OnlineVoting.Models.Dtos.Request.Email;
using OnlineVoting.Services.Interfaces;

namespace OnlineVoting.Services.BackgroundTasks
{
    public class SendVoteConfirmationEmailTask : IBackgroundTask<VoteConfirmationEmailRequest>
    {
        private readonly IEmailService _emailService;

        public SendVoteConfirmationEmailTask(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public async Task ExecuteAsync(VoteConfirmationEmailRequest request)
        {
            await _emailService.SendVoteConfirmationEmail(request);
        }
    }
}