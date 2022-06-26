using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using OnlineVoting.Models.Dtos.Request.Email;
using OnlineVoting.Models.Entities.Email;
using OnlineVoting.Services.Extension;
using OnlineVoting.Services.Interfaces;
using System.Net;

namespace OnlineVoting.Services.Implementation
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task SendVoterEmail(VoterEmailDto request)
        {
            EmailRequestDto emailRequest = new()
            {
                FromName = _emailSettings.SenderName,
                FromEmail = _emailSettings.SenderEmail,
                ToName = request.FirstName,
                ToEmail = request.Email,
                VotingCode = request.VotingCode,
                AppUrl = _emailSettings?.AppUrl,
            };

            EmailDataDto emailData = EmailExtension.SendVoterEmailData(emailRequest);

            await SendEmail(emailData);
        }

        private async Task<bool> SendEmail(EmailDataDto request)
        {
            var client = new SmtpClient();

            try
            {
                await client.ConnectAsync(_emailSettings.Server, _emailSettings.Port, true);
                await client.AuthenticateAsync(new NetworkCredential(_emailSettings.SenderEmail, _emailSettings.Password));
                await client.SendAsync(request.MessageBody);
                await client.DisconnectAsync(true);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                client.Dispose();
            }
        }
    }
}
