using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OnlineVoting.Models.Dtos.Request.Email;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Entities.Email;
using OnlineVoting.Services.Extension;
using OnlineVoting.Services.Interfaces;
using System.Net;
using OnlineVoting.Models.Results;
using OnlineVoting.Data.Interfaces;

namespace OnlineVoting.Services.Implementation
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly UserManager<User> _userManager;
        private readonly IServiceFactory _serviceFactory;

        public EmailService(IOptions<EmailSettings> emailSettings, IServiceFactory serviceFactory)
        {
            _emailSettings = emailSettings.Value;
            _serviceFactory = serviceFactory;
            _userManager = _serviceFactory.GetService<UserManager<User>>();
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

        public async Task SendCreateUserEmail(UserMailDto request)
        {
            string emailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(request.User);
            string resetPasswordToken = await _userManager.GeneratePasswordResetTokenAsync(request.User);

            EmailRequestDto emailRequest = new()
            {
                FromName = _emailSettings.SenderName,
                FromEmail = _emailSettings.SenderEmail,
                ToName = request.FirstName,
                ToEmail = request.User.Email,
                AppUrl = _emailSettings?.AppUrl,
                EmailConfirmationToken = emailConfirmationToken,
                ResetPasswordToken = resetPasswordToken
            };

            EmailDataDto emailData = EmailExtension.CreateUserEmailData(emailRequest);

            await SendEmail(emailData);
        }

        public async Task<Result<string>> SendResetPasswordEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return Result<string>.ValidationError("Enter an email");

            User user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return Result<string>.NotFound("A link to reset your password will be sent to you if an account with this email exist");

            string resetPasswordToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            EmailRequestDto emailRequest = new()
            {
                FromName = _emailSettings.SenderName,
                FromEmail = _emailSettings.SenderEmail,
                ToName = user.FirstName,
                ToEmail = user.Email,
                AppUrl = _emailSettings?.AppUrl,
                ResetPasswordToken = resetPasswordToken
            };

            EmailDataDto emailData = EmailExtension.ResetPasswordEmailData(emailRequest);

            await SendEmail(emailData);

            return Result<string>.Success("A link to reset your password will be sent to you if an account with this email exist");
        }

        public async Task<Result<string>> SendChangeEmail(ChangeEmailRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.NewEmail) || string.IsNullOrWhiteSpace(request.RecoveryEmail))
                return Result<string>.ValidationError("Invalid data sent");

            User user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return Result<string>.NotFound("User not found");

            string changeEmailToken = await _userManager.GenerateChangeEmailTokenAsync(user, request.NewEmail);

            EmailRequestDto emailRequest = new()
            {
                FromName = _emailSettings.SenderName,
                FromEmail = _emailSettings.SenderEmail,
                ToName = user.FirstName,
                ToEmail = user.RecoveryEmail,
                AppUrl = _emailSettings?.AppUrl,
                RecoveryEmail = user.RecoveryEmail,
                NewEmail = request.NewEmail,
                ChangeEmailToken = changeEmailToken
            };

            EmailDataDto emailData = EmailExtension.ChangeEmailData(emailRequest);

            await SendEmail(emailData);

            return Result<string>.Success("A link to change your email will be sent to you if an account with this email exist");
        }

        private async Task<bool> SendEmail(EmailDataDto request)
        {
            SmtpClient client = new();

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
