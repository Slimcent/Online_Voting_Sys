using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Moq;
using OnlineVoting.Data.Interfaces;
using OnlineVoting.Models.Entities;
using OnlineVoting.Services.Implementation;
using OnlineVoting.Services.Interfaces;
using VotingSystem.Logger;

namespace OnlineVoting.Tests.TestData.Factories
{
    public class RolesServiceFactory
    {
        public Mock<IUserStore<User>> UserStore { get; }
        public Mock<IRoleStore<Role>> RoleStore { get; }
        public Mock<UserManager<User>> UserManager { get; }
        public Mock<RoleManager<Role>> RoleManager { get; }
        public Mock<IRepository<Role>> RoleRepository { get; }
        public Mock<IUnitOfWork> UnitOfWork { get; }
        public Mock<IMapper> Mapper { get; }
        public Mock<IServiceFactory> ServiceFactory { get; }
        public Mock<ILoggerMessage> LoggerMessage { get; }
        public RolesService Service { get; }

        public RolesServiceFactory()
        {
            UserStore = new Mock<IUserStore<User>>();
            RoleStore = new Mock<IRoleStore<Role>>();

            UserManager = new Mock<UserManager<User>>(UserStore.Object, null!, null!, null!, null!, null!, null!, null!, null!);
            RoleManager = new Mock<RoleManager<Role>>(RoleStore.Object, null!, null!, null!, null!);

            RoleRepository = new Mock<IRepository<Role>>();
            UnitOfWork = new Mock<IUnitOfWork>();
            Mapper = new Mock<IMapper>();
            ServiceFactory = new Mock<IServiceFactory>();
            LoggerMessage = new Mock<ILoggerMessage>();

            UnitOfWork.Setup(unitOfWork => unitOfWork.GetRepository<Role>()).Returns(RoleRepository.Object);

            ServiceFactory.Setup(serviceFactory => serviceFactory.GetService<IUnitOfWork>()).Returns(UnitOfWork.Object);
            ServiceFactory.Setup(serviceFactory => serviceFactory.GetService<UserManager<User>>()).Returns(UserManager.Object);
            ServiceFactory.Setup(serviceFactory => serviceFactory.GetService<RoleManager<Role>>()).Returns(RoleManager.Object);
            ServiceFactory.Setup(serviceFactory => serviceFactory.GetService<IMapper>()).Returns(Mapper.Object);
            ServiceFactory.Setup(serviceFactory => serviceFactory.GetService<ILoggerMessage>()).Returns(LoggerMessage.Object);

            Service = new RolesService(ServiceFactory.Object);
        }
    }
}