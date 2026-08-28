using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using OnlineVoting.Data.Interfaces;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Pagination;
using OnlineVoting.Services.Implementation;
using OnlineVoting.Services.Interfaces;
using System.Linq.Expressions;
using VotingSystem.Logger;

namespace OnlineVoting.Tests.TestData.Factories
{
    public sealed class AuditTrailServiceFactory : IDisposable
    {
        public AuditDbContextFactory DbContextFactory { get; }

        public Mock<IRepository<AuditTrail>> AuditTrailRepository { get; }

        public Mock<IUnitOfWork> UnitOfWork { get; }

        public Mock<IMapper> Mapper { get; }

        public Mock<ILoggerMessage> LoggerMessage { get; }

        public Mock<IServiceFactory> ServiceFactory { get; }

        public AuditTrailService Service { get; }

        public PagedList<AuditTrail>? MappedAuditTrails { get; private set; }

        public PagedResponse<AuditTrailResponse> MappedResponse { get; }

        public AuditTrailServiceFactory()
        {
            DbContextFactory = new AuditDbContextFactory();

            AuditTrailRepository = new Mock<IRepository<AuditTrail>>();
            UnitOfWork = new Mock<IUnitOfWork>();
            Mapper = new Mock<IMapper>();
            LoggerMessage = new Mock<ILoggerMessage>();
            ServiceFactory = new Mock<IServiceFactory>();

            MappedResponse = new PagedResponse<AuditTrailResponse>();

            AuditTrailRepository.Setup(repository => repository.GetQueryable(It.IsAny<Expression<Func<AuditTrail, bool>>>(),
                It.IsAny<Func<IQueryable<AuditTrail>, IOrderedQueryable<AuditTrail>>>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Func<IQueryable<AuditTrail>, IIncludableQueryable<AuditTrail, object>>>()))
            .Returns((Expression<Func<AuditTrail, bool>> predicate, Func<IQueryable<AuditTrail>, IOrderedQueryable<AuditTrail>> orderBy,
                int? skip, int? take,
                Func<IQueryable<AuditTrail>, IIncludableQueryable<AuditTrail, object>> include) =>
                {
                    IQueryable<AuditTrail> query = DbContextFactory.Context.AuditTrails;

                    if (include != null)
                        query = include(query);

                    return query;
                });

            UnitOfWork.Setup(unitOfWork => unitOfWork.GetRepository<AuditTrail>())
                .Returns(AuditTrailRepository.Object);

            ServiceFactory.Setup(serviceFactory => serviceFactory.GetService<IUnitOfWork>())
                .Returns(UnitOfWork.Object);

            ServiceFactory.Setup(serviceFactory => serviceFactory.GetService<IMapper>())
                .Returns(Mapper.Object);

            ServiceFactory.Setup(serviceFactory => serviceFactory.GetService<ILoggerMessage>())
                .Returns(LoggerMessage.Object);

            Mapper.Setup(mapper => mapper.Map<PagedResponse<AuditTrailResponse>>(It.IsAny<PagedList<AuditTrail>>()))
                .Callback<object>(source => MappedAuditTrails = (PagedList<AuditTrail>)source)
                .Returns(MappedResponse);

            Service = new AuditTrailService(ServiceFactory.Object);
        }

        public void Dispose()
        {
            DbContextFactory.Dispose();
        }
    }
}