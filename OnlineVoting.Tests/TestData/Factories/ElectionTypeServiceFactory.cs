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
    public sealed class ElectionTypeServiceFactory : IDisposable
    {
        public AuditDbContextFactory DbContextFactory { get; }
        public Mock<IRepository<ElectionType>> ElectionTypeRepository { get; }
        public Mock<IRepository<Election>> ElectionRepository { get; }
        public Mock<IRepository<ElectionScope>> ElectionScopeRepository { get; }
        public Mock<IUnitOfWork> UnitOfWork { get; }
        public Mock<IMapper> Mapper { get; }
        public Mock<IServiceFactory> ServiceFactory { get; }
        public Mock<ILoggerMessage> LoggerMessage { get; }
        public ElectionTypeService Service { get; }

        public ElectionTypeServiceFactory()
        {
            DbContextFactory = new AuditDbContextFactory();

            List<ElectionScope> electionScopes = new()
            {
                new ElectionScope
                {
                    Id = 1,
                    Code = "UNIVERSITY",
                    Name = "University",
                    Description = "Applies across the university.",
                    Active = true
                },
                new ElectionScope
                {
                    Id = 2,
                    Code = "FACULTY",
                    Name = "Faculty",
                    Description = "Applies within a faculty.",
                    Active = true
                },
                new ElectionScope
                {
                    Id = 3,
                    Code = "DEPARTMENT",
                    Name = "Department",
                    Description = "Applies within a department.",
                    Active = true
                }
            };

            if (!DbContextFactory.Context.Set<ElectionScope>().Any())
            {
                DbContextFactory.Context.Set<ElectionScope>().AddRange(electionScopes);
                DbContextFactory.Context.SaveChanges();
            }

            ElectionTypeRepository = new Mock<IRepository<ElectionType>>();
            ElectionRepository = new Mock<IRepository<Election>>();
            ElectionScopeRepository = new Mock<IRepository<ElectionScope>>();
            UnitOfWork = new Mock<IUnitOfWork>();
            Mapper = new Mock<IMapper>();
            ServiceFactory = new Mock<IServiceFactory>();
            LoggerMessage = new Mock<ILoggerMessage>();

            ElectionTypeRepository.Setup(repository => repository.GetQueryable(
                It.IsAny<Expression<Func<ElectionType, bool>>>(),
                It.IsAny<Func<IQueryable<ElectionType>, IOrderedQueryable<ElectionType>>>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Func<IQueryable<ElectionType>, IIncludableQueryable<ElectionType, object>>>()))
                .Returns((Expression<Func<ElectionType, bool>> predicate,
                    Func<IQueryable<ElectionType>, IOrderedQueryable<ElectionType>> orderBy,
                    int? skip,
                    int? take,
                    Func<IQueryable<ElectionType>, IIncludableQueryable<ElectionType, object>> include) =>
                {
                    IQueryable<ElectionType> query = DbContextFactory.Context.Set<ElectionType>();

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

            ElectionScopeRepository.Setup(repository => repository.GetByIdAsync(It.IsAny<object>()))
                .ReturnsAsync((object id) =>
                    electionScopes.SingleOrDefault(electionScope => electionScope.Id == Convert.ToInt32(id)));

            UnitOfWork.Setup(unitOfWork => unitOfWork.GetRepository<ElectionType>())
                .Returns(ElectionTypeRepository.Object);

            UnitOfWork.Setup(unitOfWork => unitOfWork.GetRepository<Election>())
                .Returns(ElectionRepository.Object);

            UnitOfWork.Setup(unitOfWork => unitOfWork.GetRepository<ElectionScope>())
                .Returns(ElectionScopeRepository.Object);

            ServiceFactory.Setup(serviceFactory => serviceFactory.GetService<IUnitOfWork>())
                .Returns(UnitOfWork.Object);

            ServiceFactory.Setup(serviceFactory => serviceFactory.GetService<IMapper>())
                .Returns(Mapper.Object);

            ServiceFactory.Setup(serviceFactory => serviceFactory.GetService<ILoggerMessage>())
                .Returns(LoggerMessage.Object);

            Service = new ElectionTypeService(ServiceFactory.Object);
        }

        public void Dispose()
        {
            DbContextFactory.Dispose();
        }
    }
}