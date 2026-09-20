using OnlineVoting.BackgroundTasks.Interfaces;
using OnlineVoting.Models.Dtos.Request.Email;
using OnlineVoting.Services.Interfaces;

namespace OnlineVoting.Services.BackgroundTasks
{
    public class SendVoterEmailTask : IBackgroundTask<VoterEmailDto>
    {
        private readonly IEmailService _emailService;

        public SendVoterEmailTask(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public async Task ExecuteAsync(VoterEmailDto request)
        {
            await _emailService.SendVoterEmail(request);
        }
    }
}