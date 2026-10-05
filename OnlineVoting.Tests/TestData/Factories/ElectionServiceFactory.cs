using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using OnlineVoting.Data.Interfaces;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Entities.OnlineVoting.Models.Entities;
using OnlineVoting.Services.Implementation;
using OnlineVoting.Services.Interfaces;
using System.Linq.Expressions;
using VotingSystem.Logger;

namespace OnlineVoting.Tests.TestData.Factories
{
    public sealed class ElectionServiceFactory : IDisposable
    {
        public AuditDbContextFactory DbContextFactory { get; }
        public Mock<IRepository<ElectionScope>> ElectionScopeRepository { get; }
        public Mock<IRepository<Election>> ElectionRepository { get; }
        public Mock<IRepository<ElectionStatus>> ElectionStatusRepository { get; }
        public Mock<IRepository<Year>> YearRepository { get; }
        public Mock<IRepository<ElectionType>> ElectionTypeRepository { get; }
        public Mock<IRepository<Faculty>> FacultyRepository { get; }
        public Mock<IRepository<Department>> DepartmentRepository { get; }
        public Mock<IUnitOfWork> UnitOfWork { get; }
        public Mock<IMapper> Mapper { get; }
        public Mock<IServiceFactory> ServiceFactory { get; }
        public Mock<ILoggerMessage> LoggerMessage { get; }
        public ElectionService Service { get; }

        public ElectionServiceFactory()
        {
            DbContextFactory = new AuditDbContextFactory();

            ElectionRepository = new Mock<IRepository<Election>>();
            ElectionStatusRepository = new Mock<IRepository<ElectionStatus>>();
            ElectionScopeRepository = new Mock<IRepository<ElectionScope>>();
            YearRepository = new Mock<IRepository<Year>>();
            ElectionTypeRepository = new Mock<IRepository<ElectionType>>();
            FacultyRepository = new Mock<IRepository<Faculty>>();
            DepartmentRepository = new Mock<IRepository<Department>>();
            UnitOfWork = new Mock<IUnitOfWork>();
            Mapper = new Mock<IMapper>();
            ServiceFactory = new Mock<IServiceFactory>();
            LoggerMessage = new Mock<ILoggerMessage>();

            ElectionRepository.Setup(repository => repository.GetQueryable(It.IsAny<Expression<Func<Election, bool>>>(),
                It.IsAny<Func<IQueryable<Election>, IOrderedQueryable<Election>>>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Func<IQueryable<Election>, IIncludableQueryable<Election, object>>>()))
                .Returns((Expression<Func<Election, bool>> predicate,
                    Func<IQueryable<Election>, IOrderedQueryable<Election>> orderBy,
                    int? skip,
                    int? take,
                    Func<IQueryable<Election>, IIncludableQueryable<Election, object>> include) =>
                {
                    IQueryable<Election> query = DbContextFactory.Context.Set<Election>();

                    if (predicate != null)
                        query = query.Where(predicate);

                    if (orderBy != null)
                        query = orderBy(query);

                    if (include != null)
                        query = include(query);

                    if (skip != null)
                        query = query.Skip(skip.Value);

                    if (take != null)
                        query = query.Take(take.Value);

                    return query;
                });

            ElectionStatusRepository.Setup(repository => repository.GetQueryable(
                It.IsAny<Expression<Func<ElectionStatus, bool>>>(),
                It.IsAny<Func<IQueryable<ElectionStatus>, IOrderedQueryable<ElectionStatus>>>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Func<IQueryable<ElectionStatus>, IIncludableQueryable<ElectionStatus, object>>>()))
                .Returns((Expression<Func<ElectionStatus, bool>> predicate,
                    Func<IQueryable<ElectionStatus>, IOrderedQueryable<ElectionStatus>> orderBy,
                    int? skip,
                    int? take,
                    Func<IQueryable<ElectionStatus>, IIncludableQueryable<ElectionStatus, object>> include) =>
                {
                    IQueryable<ElectionStatus> query = DbContextFactory.Context.Set<ElectionStatus>();

                    if (predicate != null)
                        query = query.Where(predicate);

                    if (orderBy != null)
                        query = orderBy(query);

                    if (include != null)
                        query = include(query);

                    if (skip != null)
                        query = query.Skip(skip.Value);

                    if (take != null)
                        query = query.Take(take.Value);

                    return query;
                });

            ElectionScopeRepository.Setup(repository => repository.GetQueryable(It.IsAny<Expression<Func<ElectionScope, bool>>>(),
                It.IsAny<Func<IQueryable<ElectionScope>, IOrderedQueryable<ElectionScope>>>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Func<IQueryable<ElectionScope>, IIncludableQueryable<ElectionScope, object>>>()))
                .Returns((Expression<Func<ElectionScope, bool>> predicate,
                    Func<IQueryable<ElectionScope>, IOrderedQueryable<ElectionScope>> orderBy,
                    int? skip,
                    int? take,
                    Func<IQueryable<ElectionScope>, IIncludableQueryable<ElectionScope, object>> include) =>
                {
                    IQueryable<ElectionScope> query = DbContextFactory.Context.Set<ElectionScope>();

                    if (predicate != null)
                        query = query.Where(predicate);

                    if (orderBy != null)
                        query = orderBy(query);

                    if (include != null)
                        query = include(query);

                    if (skip != null)
                        query = query.Skip(skip.Value);

                    if (take != null)
                        query = query.Take(take.Value);

                    return query;
                });

            UnitOfWork.Setup(unitOfWork => unitOfWork.GetRepository<Election>())
                .Returns(ElectionRepository.Object);

            UnitOfWork.Setup(unitOfWork => unitOfWork.GetRepository<ElectionStatus>())
                .Returns(ElectionStatusRepository.Object);

            UnitOfWork.Setup(unitOfWork => unitOfWork.GetRepository<ElectionScope>())
                .Returns(ElectionScopeRepository.Object);

            UnitOfWork.Setup(unitOfWork => unitOfWork.GetRepository<Year>())
                .Returns(YearRepository.Object);

            UnitOfWork.Setup(unitOfWork => unitOfWork.GetRepository<ElectionType>())
                .Returns(ElectionTypeRepository.Object);

            UnitOfWork.Setup(unitOfWork => unitOfWork.GetRepository<Faculty>())
                .Returns(FacultyRepository.Object);

            UnitOfWork.Setup(unitOfWork => unitOfWork.GetRepository<Department>())
                .Returns(DepartmentRepository.Object);

            ServiceFactory.Setup(serviceFactory => serviceFactory.GetService<IUnitOfWork>())
                .Returns(UnitOfWork.Object);

            ServiceFactory.Setup(serviceFactory => serviceFactory.GetService<IMapper>())
                .Returns(Mapper.Object);

            ServiceFactory.Setup(serviceFactory => serviceFactory.GetService<ILoggerMessage>())
                .Returns(LoggerMessage.Object);

            Service = new ElectionService(ServiceFactory.Object);
        }

        public void Dispose()
        {
            DbContextFactory.Dispose();
        }
    }
}