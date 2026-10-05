using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using OnlineVoting.Data.Interfaces;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Pagination;
using OnlineVoting.Services.Implementation;
using OnlineVoting.Services.Interfaces;
using System.Linq.Expressions;
using VotingSystem.Logger;

namespace OnlineVoting.Tests.TestData.Factories
{
    public sealed class ElectionPositionServiceFactory : IDisposable
    {
        public AuditDbContextFactory DbContextFactory { get; }
        public Mock<IRepository<ElectionPosition>> ElectionPositionRepository { get; }
        public Mock<IRepository<PositionApplication>> PositionApplicationRepository { get; }
        public Mock<IRepository<Election>> ElectionRepository { get; }
        public Mock<IRepository<Position>> PositionRepository { get; }
        public Mock<IUnitOfWork> UnitOfWork { get; }
        public Mock<IMapper> Mapper { get; }
        public Mock<ILoggerMessage> LoggerMessage { get; }
        public Mock<IServiceFactory> ServiceFactory { get; }
        public ElectionPositionService Service { get; }
        public ElectionPositionServiceFactory()
        {
            DbContextFactory = new AuditDbContextFactory();

            DbContextFactory.Context.Database.ExecuteSqlRaw("PRAGMA foreign_keys = OFF;");

            ElectionPositionRepository = new Mock<IRepository<ElectionPosition>>();
            PositionApplicationRepository = new Mock<IRepository<PositionApplication>>();
            ElectionRepository = new Mock<IRepository<Election>>();
            PositionRepository = new Mock<IRepository<Position>>();
            UnitOfWork = new Mock<IUnitOfWork>();
            Mapper = new Mock<IMapper>();
            LoggerMessage = new Mock<ILoggerMessage>();
            ServiceFactory = new Mock<IServiceFactory>();

            ElectionPositionRepository.Setup(repository => repository.GetQueryable(It.IsAny<Expression<Func<ElectionPosition, bool>>>(),
                It.IsAny<Func<IQueryable<ElectionPosition>, IOrderedQueryable<ElectionPosition>>>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Func<IQueryable<ElectionPosition>, IIncludableQueryable<ElectionPosition, object>>>()))
            .Returns((Expression<Func<ElectionPosition, bool>> predicate,
                Func<IQueryable<ElectionPosition>, IOrderedQueryable<ElectionPosition>> orderBy,
                int? skip,
                int? take,
                Func<IQueryable<ElectionPosition>, IIncludableQueryable<ElectionPosition, object>> include) =>
                {
                    IQueryable<ElectionPosition> query = DbContextFactory.Context.Set<ElectionPosition>();

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

            PositionApplicationRepository.Setup(repository => repository.GetQueryable(
                It.IsAny<Expression<Func<PositionApplication, bool>>>(),
                It.IsAny<Func<IQueryable<PositionApplication>, IOrderedQueryable<PositionApplication>>>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Func<IQueryable<PositionApplication>, IIncludableQueryable<PositionApplication, object>>>()))
            .Returns((Expression<Func<PositionApplication, bool>> predicate,
                Func<IQueryable<PositionApplication>, IOrderedQueryable<PositionApplication>> orderBy,
                int? skip,
                int? take,
                Func<IQueryable<PositionApplication>, IIncludableQueryable<PositionApplication, object>> include) =>
                {
                    IQueryable<PositionApplication> query = DbContextFactory.Context.Set<PositionApplication>();

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

            PositionRepository.Setup(repository => repository.GetQueryable(
                It.IsAny<Expression<Func<Position, bool>>>(),
                It.IsAny<Func<IQueryable<Position>, IOrderedQueryable<Position>>>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Func<IQueryable<Position>, IIncludableQueryable<Position, object>>>()))
            .Returns((Expression<Func<Position, bool>> predicate,
                Func<IQueryable<Position>, IOrderedQueryable<Position>> orderBy,
                int? skip,
                int? take,
                Func<IQueryable<Position>, IIncludableQueryable<Position, object>> include) =>
            {
                IQueryable<Position> query = DbContextFactory.Context.Set<Position>();

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

            UnitOfWork.Setup(unitOfWork => unitOfWork.GetRepository<ElectionPosition>())
                .Returns(ElectionPositionRepository.Object);

            ServiceFactory.Setup(serviceFactory => serviceFactory.GetService<IUnitOfWork>())
                .Returns(UnitOfWork.Object);

            ServiceFactory.Setup(serviceFactory => serviceFactory.GetService<IMapper>())
                .Returns(Mapper.Object);

            ServiceFactory.Setup(serviceFactory => serviceFactory.GetService<ILoggerMessage>())
                .Returns(LoggerMessage.Object);

            UnitOfWork.Setup(unitOfWork => unitOfWork.GetRepository<PositionApplication>())
                .Returns(PositionApplicationRepository.Object);

            UnitOfWork.Setup(unitOfWork => unitOfWork.GetRepository<Election>())
                .Returns(ElectionRepository.Object);

            UnitOfWork.Setup(unitOfWork => unitOfWork.GetRepository<Position>())
                .Returns(PositionRepository.Object);

            Mapper.Setup(mapper => mapper.Map<PagedResponse<ElectionPositionResponse>>(It.IsAny<PagedList<ElectionPosition>>()))
                .Returns((PagedList<ElectionPosition> electionPositions) => new PagedResponse<ElectionPositionResponse>
                {
                    Items = electionPositions.Select(electionPosition => new ElectionPositionResponse
                    {
                        Id = electionPosition.Id,
                        ElectionId = electionPosition.ElectionId,
                        Election = electionPosition.Election.Name,
                        PositionId = electionPosition.PositionId,
                        Position = electionPosition.Position.Name,
                        ApplicationFee = electionPosition.ApplicationFee,
                        Currency = electionPosition.Currency,
                        Active = electionPosition.Active
                    }).ToList(),
                    MetaData = electionPositions.MetaData
                });

            Mapper.Setup(mapper => mapper.Map<PagedResponse<ElectionPositionWithApplicationsResponse>>(It.IsAny<PagedList<ElectionPosition>>()))
                .Returns((PagedList<ElectionPosition> electionPositions) => new PagedResponse<ElectionPositionWithApplicationsResponse>
                {
                    Items = electionPositions.Select(electionPosition => new ElectionPositionWithApplicationsResponse
                    {
                        Id = electionPosition.Id,
                        ElectionId = electionPosition.ElectionId,
                        Election = electionPosition.Election.Name,
                        PositionId = electionPosition.PositionId,
                        Position = electionPosition.Position.Name,
                        ApplicationFee = electionPosition.ApplicationFee,
                        Currency = electionPosition.Currency,
                        Active = electionPosition.Active
                    }).ToList(),
                    MetaData = electionPositions.MetaData
                });

            Mapper.Setup(mapper => mapper.Map<IEnumerable<ElectionPositionApplicationResponse>>(It.IsAny<IEnumerable<PositionApplication>>()))
                .Returns((IEnumerable<PositionApplication> applications) => applications.Select(application => new ElectionPositionApplicationResponse
                {
                    Id = application.Id,
                    StudentId = application.StudentId,
                    FirstName = application.Student.User?.FirstName,
                    LastName = application.Student.User?.LastName,
                    RegNumber = application.Student.RegNumber,
                    DepartmentId = application.Student.DepartmentId,
                    Department = application.Student.Department!.Name,
                    FacultyId = application.Student.Department.FacultyId,
                    Faculty = application.Student.Department.Faculty.Name,
                    PositionApplicationStatusId = application.PositionApplicationStatusId,
                    PositionApplicationStatus = application.PositionApplicationStatus.Name,
                    Active = application.Active
                }).ToList());

            Mapper.Setup(mapper => mapper.Map<ElectionPosition>(It.IsAny<CreateElectionPositionRequest>()))
                .Returns((CreateElectionPositionRequest request) => new ElectionPosition
                {
                    ElectionId = request.ElectionId.Trim(),
                    PositionId = request.PositionId.Trim(),
                    ApplicationFee = request.ApplicationFee,
                    Currency = request.Currency.Trim()
                });

            Mapper.Setup(mapper => mapper.Map<ElectionPosition>(It.IsAny<CreateElectionPositionItemRequest>()))
                .Returns((CreateElectionPositionItemRequest request) => new ElectionPosition
                {
                    PositionId = request.PositionId.Trim(),
                    ApplicationFee = request.ApplicationFee,
                    Currency = request.Currency.Trim()
                });

            Service = new ElectionPositionService(ServiceFactory.Object);
        }

        public void Dispose()
        {
            DbContextFactory.Dispose();
        }
    }
}