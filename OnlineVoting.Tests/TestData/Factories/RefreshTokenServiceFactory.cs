using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moq;
using OnlineVoting.Data.Interfaces;
using OnlineVoting.Models.Entities;
using OnlineVoting.Services.Implementation;
using OnlineVoting.Services.Interfaces;
using VotingSystem.Logger;

namespace OnlineVoting.Tests.TestData.Factories
{
    public class RefreshTokenServiceFactory
    {
        public Mock<IUserStore<User>> UserStore { get; }
        public Mock<UserManager<User>> UserManager { get; }
        public Mock<IRepository<RefreshToken>> RefreshTokenRepository { get; }
        public Mock<IUnitOfWork> UnitOfWork { get; }
        public Mock<IMapper> Mapper { get; }
        public Mock<IHttpContextAccessor> HttpContextAccessor { get; }
        public DefaultHttpContext HttpContext { get; }
        public Mock<IJwtAuthenticator> JwtAuthenticator { get; }
        public Mock<IServiceFactory> ServiceFactory { get; }
        public Mock<ILoggerMessage> LoggerMessage { get; }
        public RefreshTokenService Service { get; }

        public RefreshTokenServiceFactory()
        {
            UserStore = new Mock<IUserStore<User>>();

            UserManager = new Mock<UserManager<User>>(UserStore.Object, null!, null!, null!, null!, null!,  null!, null!, null!);

            RefreshTokenRepository = new Mock<IRepository<RefreshToken>>();
            UnitOfWork = new Mock<IUnitOfWork>();
            Mapper = new Mock<IMapper>();
            HttpContextAccessor = new Mock<IHttpContextAccessor>();
            HttpContext = new DefaultHttpContext();
            JwtAuthenticator = new Mock<IJwtAuthenticator>();
            ServiceFactory = new Mock<IServiceFactory>();
            LoggerMessage = new Mock<ILoggerMessage>();

            UnitOfWork.Setup(unitOfWork => unitOfWork.GetRepository<RefreshToken>())
                .Returns(RefreshTokenRepository.Object);

            ServiceFactory.Setup(serviceFactory => serviceFactory.GetService<IUnitOfWork>())
                .Returns(UnitOfWork.Object);

            ServiceFactory.Setup(serviceFactory => serviceFactory.GetService<IMapper>())
                .Returns(Mapper.Object);

            ServiceFactory.Setup(serviceFactory => serviceFactory.GetService<ILoggerMessage>())
                .Returns(LoggerMessage.Object);

            ServiceFactory.Setup(serviceFactory => serviceFactory.GetService<IHttpContextAccessor>())
                .Returns(HttpContextAccessor.Object);

            ServiceFactory.Setup(serviceFactory => serviceFactory.GetService<UserManager<User>>())
                .Returns(UserManager.Object);

            ServiceFactory.Setup(serviceFactory => serviceFactory.GetService<IJwtAuthenticator>())
                .Returns(JwtAuthenticator.Object);

            HttpContextAccessor.Setup(accessor => accessor.HttpContext)
                .Returns(HttpContext);

            Service = new RefreshTokenService(ServiceFactory.Object);
        }
    }
}