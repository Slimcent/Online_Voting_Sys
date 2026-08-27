using Microsoft.Extensions.Options;
using OnlineVoting.Models.Dtos.Request.Email;
using OnlineVoting.Models.Entities.Email;
using OnlineVoting.Services.Implementation;
using OnlineVoting.Services.Interfaces;

namespace OnlineVoting.Tests.TestData.Factories
{
    public class TestEmailService : EmailService
    {
        public EmailDataDto? EmailData { get; private set; }
        public bool SendResult { get; set; } = true;
        public int SendCount { get; private set; }

        public TestEmailService(IOptions<EmailSettings> emailSettings, IServiceFactory serviceFactory)
            : base(emailSettings, serviceFactory)
        {
        }

        protected override Task<bool> SendEmail(EmailDataDto request)
        {
            EmailData = request;
            SendCount++;

            return Task.FromResult(SendResult);
        }
    }
}