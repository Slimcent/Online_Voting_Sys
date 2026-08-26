using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OnlineVoting.Models.Dtos.Request.Email;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Entities.Email;
using OnlineVoting.Models.Results;
using OnlineVoting.Services.Extension;
using OnlineVoting.Services.Interfaces;
using System.Net;
using VotingSystem.Logger;

namespace OnlineVoting.Services.Implementation
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly UserManager<User> _userManager;
        private readonly IServiceFactory _serviceFactory;
        private readonly ILoggerMessage _loggerMessage;

        public EmailService(IOptions<EmailSettings> emailSettings, IServiceFactory serviceFactory)
        {
            _emailSettings = emailSettings.Value;
            _serviceFactory = serviceFactory;
            _userManager = _serviceFactory.GetService<UserManager<User>>();
            _loggerMessage = _serviceFactory.GetService<ILoggerMessage>();
        }

        public async Task SendVoterEmail(VoterEmailDto request)
        {
            _loggerMessage.LogInfo($"Voter email request received for {request.Email}.");

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

            _loggerMessage.LogInfo($"Voter email processing completed for {request.Email}.");
        }

        public async Task SendCreateUserEmail(UserMailDto request)
        {
            _loggerMessage.LogInfo($"Create user email request received for user {request.User.Id}.");

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

            _loggerMessage.LogInfo($"Create user email processing completed for user {request.User.Id}.");
        }

        public async Task<Result<string>> SendResetPasswordEmail(string email)
        {
            _loggerMessage.LogInfo($"Reset password email request received for {email}.");

            if (string.IsNullOrWhiteSpace(email))
            {
                _loggerMessage.LogWarn("Reset password email request failed because the email was empty.");

                return Result<string>.ValidationError("Enter an email");
            }

            string userEmail = email.Trim();

            User user = await _userManager.FindByEmailAsync(userEmail);

            if (user == null)
            {
                _loggerMessage.LogWarn($"Reset password email request could not find a user for {userEmail}.");

                return Result<string>.NotFound("A link to reset your password will be sent to you if an account with this email exist");
            }

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

            _loggerMessage.LogInfo($"Reset password email processing completed for user {user.Id}.");

            return Result<string>.Success("A link to reset your password will be sent to you if an account with this email exist");
        }

        public async Task<Result<string>> SendChangeEmail(ChangeEmailRequest request)
        {
            _loggerMessage.LogInfo($"Change email request received for {request.Email}.");

            if (string.IsNullOrWhiteSpace(request.NewEmail) || string.IsNullOrWhiteSpace(request.RecoveryEmail))
            {
                _loggerMessage.LogWarn("Change email request failed because invalid data was provided.");

                return Result<string>.ValidationError("Invalid data sent");
            }

            string email = request.Email.Trim();
            string newEmail = request.NewEmail.Trim();

            User user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                _loggerMessage.LogWarn($"Change email request failed because user with email {email} was not found.");

                return Result<string>.NotFound("User not found");
            }

            string changeEmailToken = await _userManager.GenerateChangeEmailTokenAsync(user, newEmail);

            EmailRequestDto emailRequest = new()
            {
                FromName = _emailSettings.SenderName,
                FromEmail = _emailSettings.SenderEmail,
                ToName = user.FirstName,
                ToEmail = user.RecoveryEmail,
                AppUrl = _emailSettings?.AppUrl,
                RecoveryEmail = user.RecoveryEmail,
                NewEmail = newEmail,
                ChangeEmailToken = changeEmailToken
            };

            EmailDataDto emailData = EmailExtension.ChangeEmailData(emailRequest);

            await SendEmail(emailData);

            _loggerMessage.LogInfo($"Change email processing completed for user {user.Id}.");

            return Result<string>.Success("A link to change your email will be sent to you if an account with this email exist");
        }

        protected virtual async Task<bool> SendEmail(EmailDataDto request)
        {
            SmtpClient client = new();

            try
            {
                await client.ConnectAsync(_emailSettings.Server, _emailSettings.Port, true);
                await client.AuthenticateAsync(new NetworkCredential(_emailSettings.SenderEmail, _emailSettings.Password));
                await client.SendAsync(request.MessageBody);
                await client.DisconnectAsync(true);

                _loggerMessage.LogInfo("Email sent successfully.");

                return true;
            }
            catch (Exception exception)
            {
                _loggerMessage.LogError($"Email sending failed. {exception.Message}");

                return false;
            }
            finally
            {
                client.Dispose();
            }
        }
    }
}