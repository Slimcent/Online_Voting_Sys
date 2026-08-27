using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Entities.Email;
using OnlineVoting.Services.Interfaces;
using VotingSystem.Logger;

namespace OnlineVoting.Tests.TestData.Factories
{
    public class EmailServiceFactory
    {
        public Mock<IUserStore<User>> UserStore { get; }
        public Mock<UserManager<User>> UserManager { get; }
        public Mock<IServiceFactory> ServiceFactory { get; }
        public Mock<ILoggerMessage> LoggerMessage { get; }
        public TestEmailService Service { get; }

        public EmailServiceFactory()
        {
            UserStore = new Mock<IUserStore<User>>();

            UserManager = new Mock<UserManager<User>>(
                UserStore.Object,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!);

            ServiceFactory = new Mock<IServiceFactory>();
            LoggerMessage = new Mock<ILoggerMessage>();

            ServiceFactory.Setup(serviceFactory => serviceFactory.GetService<UserManager<User>>()).Returns(UserManager.Object);
            ServiceFactory.Setup(serviceFactory => serviceFactory.GetService<ILoggerMessage>()).Returns(LoggerMessage.Object);

            EmailSettings emailSettings = new()
            {
                SenderName = "Online Voting System",
                SenderEmail = "noreply@example.com",
                Password = "password",
                Server = "smtp.example.com",
                Port = 465,
                AppUrl = "https://example.com"
            };

            Service = new TestEmailService(Options.Create(emailSettings), ServiceFactory.Object);
        }
    }
}