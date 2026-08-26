using AutoMapper;
using Moq;
using OnlineVoting.Data.Interfaces;
using OnlineVoting.Models.Entities;
using OnlineVoting.Services.Implementation;
using OnlineVoting.Services.Interfaces;
using VotingSystem.Logger;

namespace OnlineVoting.Tests.TestData.Factories
{
    public class DepartmentServiceFactory
    {
        public Mock<IRepository<Department>> DepartmentRepository { get; }
        public Mock<IRepository<Faculty>> FacultyRepository { get; }
        public Mock<IUnitOfWork> UnitOfWork { get; }
        public Mock<IMapper> Mapper { get; }
        public Mock<IServiceFactory> ServiceFactory { get; }
        public Mock<ILoggerMessage> LoggerMessage { get; }
        public DepartmentService Service { get; }

        public DepartmentServiceFactory()
        {
            DepartmentRepository = new Mock<IRepository<Department>>();
            FacultyRepository = new Mock<IRepository<Faculty>>();
            UnitOfWork = new Mock<IUnitOfWork>();
            Mapper = new Mock<IMapper>();
            ServiceFactory = new Mock<IServiceFactory>();
            LoggerMessage = new Mock<ILoggerMessage>();

            UnitOfWork.Setup(unitOfWork => unitOfWork.GetRepository<Department>()).Returns(DepartmentRepository.Object);
            UnitOfWork.Setup(unitOfWork => unitOfWork.GetRepository<Faculty>()).Returns(FacultyRepository.Object);

            ServiceFactory.Setup(serviceFactory => serviceFactory.GetService<IUnitOfWork>()).Returns(UnitOfWork.Object);
            ServiceFactory.Setup(serviceFactory => serviceFactory.GetService<IMapper>()).Returns(Mapper.Object);
            ServiceFactory.Setup(serviceFactory => serviceFactory.GetService<ILoggerMessage>()).Returns(LoggerMessage.Object);

            Service = new DepartmentService(ServiceFactory.Object);
        }
    }
}