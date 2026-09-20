using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using OnlineVoting.Api.Mapper;
using OnlineVoting.Data.Interfaces;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Interfaces;
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

        public Mock<IRepository<AuditOutcome>> AuditOutcomeRepository { get; }

        public Mock<IUnitOfWork> UnitOfWork { get; }

        public Mock<IMapper> Mapper { get; }

        public Mock<ILoggerMessage> LoggerMessage { get; }

        public Mock<IServiceFactory> ServiceFactory { get; }

        public AuditTrailService Service { get; }

        public PagedList<AuditTrail>? MappedAuditTrails { get; private set; }

        public PagedResponse<AuditTrailResponse> MappedResponse { get; }

        public AuditTrailServiceFactory(string? username = "super.admin", string? userId = "user-id")
        {
            DbContextFactory = new AuditDbContextFactory(username, userId);

            AuditTrailRepository = new Mock<IRepository<AuditTrail>>();
            AuditOutcomeRepository = new Mock<IRepository<AuditOutcome>>();
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

            AuditOutcomeRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<Expression<Func<AuditOutcome, bool>>>()))
                .ReturnsAsync((Expression<Func<AuditOutcome, bool>> predicate) =>
                    DbContextFactory.Context.AuditOutcomes.FirstOrDefault(predicate));

            AuditTrailRepository.Setup(repository => repository.AddAsync(It.IsAny<AuditTrail>(), It.IsAny<bool>()))
                 .Returns(async (AuditTrail auditTrail, bool tracking) =>
                 {
                     DbContextFactory.Context.AuditTrails.Add(auditTrail);

                     await DbContextFactory.Context.SaveChangesAsync();

                     if (!tracking)
                         DbContextFactory.Context.Entry(auditTrail).State = EntityState.Detached;

                     return auditTrail;
                 });

            UnitOfWork.Setup(unitOfWork => unitOfWork.GetRepository<AuditTrail>())
                .Returns(AuditTrailRepository.Object);

            UnitOfWork.Setup(unitOfWork => unitOfWork.GetRepository<AuditOutcome>())
                .Returns(AuditOutcomeRepository.Object);
                        
            ServiceFactory.Setup(serviceFactory => serviceFactory.GetService<IUnitOfWork>())
                .Returns(UnitOfWork.Object);

            ServiceFactory.Setup(serviceFactory => serviceFactory.GetService<IMapper>())
                .Returns(Mapper.Object);

            ServiceFactory.Setup(serviceFactory => serviceFactory.GetService<ILoggerMessage>())
                .Returns(LoggerMessage.Object);

            ServiceFactory.Setup(serviceFactory => serviceFactory.GetService<IAuditMetadataProvider>())
                .Returns(DbContextFactory.AuditMetadataProvider);

            MapperConfiguration mapperConfiguration = new(configuration => configuration.AddProfile<AuditMappingProfile>(),
                NullLoggerFactory.Instance);

            IMapper realMapper = mapperConfiguration.CreateMapper();

            Mapper.Setup(mapper => mapper.Map<AuditTrail>(It.IsAny<AuditEventRequest>()))
                .Returns((AuditEventRequest request) => realMapper.Map<AuditTrail>(request));

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