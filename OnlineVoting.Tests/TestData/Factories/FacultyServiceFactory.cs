using AutoMapper;
using Moq;
using OnlineVoting.Caching.Configuration;
using OnlineVoting.Caching.Interfaces;
using OnlineVoting.Data.Interfaces;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Pagination;
using OnlineVoting.Services.Implementation;
using OnlineVoting.Services.Interfaces;
using VotingSystem.Logger;

namespace OnlineVoting.Tests.TestData.Factories
{
    public class FacultyServiceFactory
    {
        public Mock<IRepository<Faculty>> FacultyRepository { get; }
        public Mock<IUnitOfWork> UnitOfWork { get; }
        public Mock<IMapper> Mapper { get; }
        public Mock<IServiceFactory> ServiceFactory { get; }
        public Mock<ILoggerMessage> LoggerMessage { get; }
        public Mock<ICacheService> CacheService { get; }
        public FacultyService Service { get; }

        public FacultyServiceFactory()
        {
            FacultyRepository = new Mock<IRepository<Faculty>>();
            UnitOfWork = new Mock<IUnitOfWork>();
            Mapper = new Mock<IMapper>();
            ServiceFactory = new Mock<IServiceFactory>();
            LoggerMessage = new Mock<ILoggerMessage>();
            CacheService = new Mock<ICacheService>();

            UnitOfWork.Setup(unitOfWork => unitOfWork.GetRepository<Faculty>()).Returns(FacultyRepository.Object);

            ServiceFactory.Setup(serviceFactory => serviceFactory.GetService<IUnitOfWork>()).Returns(UnitOfWork.Object);
            ServiceFactory.Setup(serviceFactory => serviceFactory.GetService<IMapper>()).Returns(Mapper.Object);
            ServiceFactory.Setup(serviceFactory => serviceFactory.GetService<ILoggerMessage>()).Returns(LoggerMessage.Object);
            ServiceFactory.Setup(serviceFactory => serviceFactory.GetService<ICacheService>()).Returns(CacheService.Object);

            CacheService.Setup(cacheService => cacheService.GetOrCreate<FacultyResponse?>(It.IsAny<string>(),
                It.IsAny<Func<CancellationToken, ValueTask<FacultyResponse?>>>(),
                It.IsAny<CacheEntryOptions?>(),
                It.IsAny<CancellationToken>()))
            .Returns((string key, Func<CancellationToken, ValueTask<FacultyResponse?>> factory, CacheEntryOptions? options,
                CancellationToken cancellationToken) => factory(cancellationToken));

            CacheService.Setup(cacheService => cacheService.GetOrCreate<PagedResponse<FacultyResponse>>(It.IsAny<string>(),
                It.IsAny<Func<CancellationToken, ValueTask<PagedResponse<FacultyResponse>>>>(),
                It.IsAny<CacheEntryOptions?>(),
                It.IsAny<CancellationToken>()))
            .Returns((string key, Func<CancellationToken, ValueTask<PagedResponse<FacultyResponse>>> factory,
                CacheEntryOptions? options, CancellationToken cancellationToken) => factory(cancellationToken));

            CacheService.Setup(cacheService => cacheService.Remove(It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

            CacheService.Setup(cacheService => cacheService.RemoveByTag(It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

            Service = new FacultyService(ServiceFactory.Object);
        }
    }
}