using AutoMapper;
using Moq;
using OnlineVoting.Data.Interfaces;
using OnlineVoting.Models.Entities;
using OnlineVoting.Services.Implementation;
using OnlineVoting.Services.Interfaces;
using VotingSystem.Logger;
using OnlineVoting.Caching.Configuration;
using OnlineVoting.Caching.Interfaces;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Pagination;

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
        public Mock<ICacheService> CacheService { get; }

        public DepartmentServiceFactory()
        {
            DepartmentRepository = new Mock<IRepository<Department>>();
            FacultyRepository = new Mock<IRepository<Faculty>>();
            UnitOfWork = new Mock<IUnitOfWork>();
            Mapper = new Mock<IMapper>();
            ServiceFactory = new Mock<IServiceFactory>();
            LoggerMessage = new Mock<ILoggerMessage>();
            CacheService = new Mock<ICacheService>();

            UnitOfWork.Setup(unitOfWork => unitOfWork.GetRepository<Department>()).Returns(DepartmentRepository.Object);
            UnitOfWork.Setup(unitOfWork => unitOfWork.GetRepository<Faculty>()).Returns(FacultyRepository.Object);

            ServiceFactory.Setup(serviceFactory => serviceFactory.GetService<IUnitOfWork>()).Returns(UnitOfWork.Object);
            ServiceFactory.Setup(serviceFactory => serviceFactory.GetService<IMapper>()).Returns(Mapper.Object);
            ServiceFactory.Setup(serviceFactory => serviceFactory.GetService<ILoggerMessage>()).Returns(LoggerMessage.Object);
            ServiceFactory.Setup(serviceFactory => serviceFactory.GetService<ICacheService>()).Returns(CacheService.Object);

            CacheService.Setup(cacheService => cacheService.GetOrCreate<DepartmentResponse?>(It.IsAny<string>(),
                It.IsAny<Func<CancellationToken, ValueTask<DepartmentResponse?>>>(),
                It.IsAny<CacheEntryOptions?>(),
                It.IsAny<CancellationToken>()))
            .Returns((string key, Func<CancellationToken, ValueTask<DepartmentResponse?>> factory, CacheEntryOptions? options,
                CancellationToken cancellationToken) => factory(cancellationToken));

            CacheService.Setup(cacheService => cacheService.GetOrCreate<PagedResponse<DepartmentResponse>>(
                It.IsAny<string>(),
                It.IsAny<Func<CancellationToken, ValueTask<PagedResponse<DepartmentResponse>>>>(),
                It.IsAny<CacheEntryOptions?>(),
                It.IsAny<CancellationToken>()))
            .Returns((string key, Func<CancellationToken, ValueTask<PagedResponse<DepartmentResponse>>> factory,
                CacheEntryOptions? options, CancellationToken cancellationToken) => factory(cancellationToken));

            CacheService.Setup(cacheService => cacheService.GetOrCreate<IEnumerable<DepartmentResponse>>(
                It.IsAny<string>(),
                It.IsAny<Func<CancellationToken, ValueTask<IEnumerable<DepartmentResponse>>>>(),
                It.IsAny<CacheEntryOptions?>(),
                It.IsAny<CancellationToken>()))
            .Returns((string key, Func<CancellationToken, ValueTask<IEnumerable<DepartmentResponse>>> factory,
                CacheEntryOptions? options, CancellationToken cancellationToken) => factory(cancellationToken));

            CacheService.Setup(cacheService => cacheService.RemoveByTag(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

            Service = new DepartmentService(ServiceFactory.Object);
        }
    }
}