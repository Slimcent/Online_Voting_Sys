using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using OnlineVoting.Data.Interfaces;
using OnlineVoting.Models.Entities;
using OnlineVoting.Services.Implementation;
using OnlineVoting.Services.Interfaces;
using System.Linq.Expressions;
using VotingSystem.Logger;

namespace OnlineVoting.Tests.TestData.Factories
{
    public sealed class UserServiceFactory
    {
        public Mock<IUserStore<User>> UserStore { get; }

        public Mock<IRoleStore<Role>> RoleStore { get; }

        public Mock<UserManager<User>> UserManager { get; }

        public Mock<SignInManager<User>> SignInManager { get; }

        public Mock<RoleManager<Role>> RoleManager { get; }

        public Mock<IRepository<User>> UserRepository { get; }

        public Mock<IRepository<Student>> StudentRepository { get; }

        public Mock<IRepository<Staff>> StaffRepository { get; }

        public Mock<IUnitOfWork> UnitOfWork { get; }

        public Mock<IMapper> Mapper { get; }

        public Mock<ILoggerMessage> LoggerMessage { get; }

        public Mock<IAuditTrailService> AuditTrailService { get; }

        public Mock<IRefreshTokenService> RefreshTokenService { get; }

        public Mock<IJwtAuthenticator> JwtAuthenticator { get; }

        public Mock<IServiceFactory> ServiceFactory { get; }

        public UserService Service { get; }

        public UserServiceFactory()
        {
            UserStore = new Mock<IUserStore<User>>();
            RoleStore = new Mock<IRoleStore<Role>>();

            UserManager = new Mock<UserManager<User>>(UserStore.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            Mock<IHttpContextAccessor> httpContextAccessor = new();
            Mock<IUserClaimsPrincipalFactory<User>> claimsPrincipalFactory = new();
            Mock<ILogger<SignInManager<User>>> signInManagerLogger = new();
            Mock<IAuthenticationSchemeProvider> authenticationSchemeProvider = new();
            Mock<IUserConfirmation<User>> userConfirmation = new();

            httpContextAccessor.Setup(accessor => accessor.HttpContext)
                .Returns(new DefaultHttpContext());

            SignInManager = new Mock<SignInManager<User>>(UserManager.Object, httpContextAccessor.Object, claimsPrincipalFactory.Object,
                Options.Create(new IdentityOptions()), signInManagerLogger.Object, authenticationSchemeProvider.Object, userConfirmation.Object);

            RoleManager = new Mock<RoleManager<Role>>(RoleStore.Object, null!, null!, null!, null!);

            UserRepository = new Mock<IRepository<User>>();
            StudentRepository = new Mock<IRepository<Student>>();
            StaffRepository = new Mock<IRepository<Staff>>();
            UnitOfWork = new Mock<IUnitOfWork>();
            Mapper = new Mock<IMapper>();
            LoggerMessage = new Mock<ILoggerMessage>();
            AuditTrailService = new Mock<IAuditTrailService>();
            RefreshTokenService = new Mock<IRefreshTokenService>();
            JwtAuthenticator = new Mock<IJwtAuthenticator>();
            ServiceFactory = new Mock<IServiceFactory>();

            UnitOfWork.Setup(unitOfWork => unitOfWork.GetRepository<User>())
                .Returns(UserRepository.Object);

            UnitOfWork.Setup(unitOfWork => unitOfWork.GetRepository<Student>())
                .Returns(StudentRepository.Object);

            UnitOfWork.Setup(unitOfWork => unitOfWork.GetRepository<Staff>())
                .Returns(StaffRepository.Object);

            ServiceFactory.Setup(serviceFactory => serviceFactory.GetService<IUnitOfWork>())
                .Returns(UnitOfWork.Object);

            ServiceFactory.Setup(serviceFactory => serviceFactory.GetService<UserManager<User>>())
                .Returns(UserManager.Object);

            ServiceFactory.Setup(serviceFactory => serviceFactory.GetService<SignInManager<User>>())
                .Returns(SignInManager.Object);

            ServiceFactory.Setup(serviceFactory => serviceFactory.GetService<RoleManager<Role>>())
                .Returns(RoleManager.Object);

            ServiceFactory.Setup(serviceFactory => serviceFactory.GetService<IMapper>())
                .Returns(Mapper.Object);

            ServiceFactory.Setup(serviceFactory => serviceFactory.GetService<ILoggerMessage>())
                .Returns(LoggerMessage.Object);

            ServiceFactory.Setup(serviceFactory => serviceFactory.GetService<IAuditTrailService>())
                .Returns(AuditTrailService.Object);

            ServiceFactory.Setup(serviceFactory => serviceFactory.GetService<IRefreshTokenService>())
                .Returns(RefreshTokenService.Object);

            ServiceFactory.Setup(serviceFactory => serviceFactory.GetService<IJwtAuthenticator>())
                .Returns(JwtAuthenticator.Object);

            Service = new UserService(ServiceFactory.Object);
        }

        public void SetLoginUser(User? user)
        {
            UserRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<Func<IQueryable<User>, IOrderedQueryable<User>>>(), It.IsAny<int?>(), It.IsAny<int?>(),
                It.IsAny<Func<IQueryable<User>, IIncludableQueryable<User, object>>>(), It.IsAny<bool>()))
                .ReturnsAsync(user);
        }
    }
}