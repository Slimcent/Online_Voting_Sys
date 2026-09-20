using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using OnlineVoting.Data.Interfaces;
using OnlineVoting.Models.Entities;
using OnlineVoting.Services.Implementation;
using OnlineVoting.Services.Interfaces;
using System.Linq.Expressions;
using VotingSystem.Logger;

namespace OnlineVoting.Tests.TestData.Factories
{
    public sealed class StudentServiceFactory : IDisposable
    {
        public AuditDbContextFactory DbContextFactory { get; }

        public Mock<IRepository<Student>> StudentRepository { get; }

        public Mock<IRepository<Contestant>> ContestantRepository { get; }

        public Mock<IUnitOfWork> UnitOfWork { get; }

        public Mock<IMapper> Mapper { get; }

        public Mock<ILoggerMessage> LoggerMessage { get; }

        public Mock<IServiceFactory> ServiceFactory { get; }

        public Mock<UserManager<User>> UserManager { get; }

        public Mock<RoleManager<Role>> RoleManager { get; }

        public StudentService Service { get; }

        public StudentServiceFactory()
        {
            DbContextFactory = new AuditDbContextFactory();

            DbContextFactory.Context.Database.ExecuteSqlRaw("PRAGMA foreign_keys = OFF;");

            StudentRepository = new Mock<IRepository<Student>>();
            ContestantRepository = new Mock<IRepository<Contestant>>();
            UnitOfWork = new Mock<IUnitOfWork>();
            Mapper = new Mock<IMapper>();
            LoggerMessage = new Mock<ILoggerMessage>();
            ServiceFactory = new Mock<IServiceFactory>();

            UserManager = new Mock<UserManager<User>>(
                Mock.Of<IUserStore<User>>(),
                null!,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!);

            RoleManager = new Mock<RoleManager<Role>>(
                Mock.Of<IRoleStore<Role>>(),
                null!,
                null!,
                null!,
                null!);

            StudentRepository.Setup(repository => repository.GetQueryable(
                It.IsAny<Expression<Func<Student, bool>>>(),
                It.IsAny<Func<IQueryable<Student>, IOrderedQueryable<Student>>>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Func<IQueryable<Student>, IIncludableQueryable<Student, object>>>()))
                .Returns((Expression<Func<Student, bool>> predicate,
                    Func<IQueryable<Student>, IOrderedQueryable<Student>> orderBy,
                    int? skip,
                    int? take,
                    Func<IQueryable<Student>, IIncludableQueryable<Student, object>> include) =>
                {
                    IQueryable<Student> query = DbContextFactory.Context.Students;

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

            UnitOfWork.Setup(unitOfWork => unitOfWork.GetRepository<Student>()).Returns(StudentRepository.Object);
            UnitOfWork.Setup(unitOfWork => unitOfWork.GetRepository<Contestant>()).Returns(ContestantRepository.Object);
            UnitOfWork.Setup(unitOfWork => unitOfWork.SaveChangesAsync()).ReturnsAsync(1);

            ServiceFactory.Setup(serviceFactory => serviceFactory.GetService<IUnitOfWork>()).Returns(UnitOfWork.Object);
            ServiceFactory.Setup(serviceFactory => serviceFactory.GetService<UserManager<User>>()).Returns(UserManager.Object);
            ServiceFactory.Setup(serviceFactory => serviceFactory.GetService<RoleManager<Role>>()).Returns(RoleManager.Object);
            ServiceFactory.Setup(serviceFactory => serviceFactory.GetService<IMapper>()).Returns(Mapper.Object);
            ServiceFactory.Setup(serviceFactory => serviceFactory.GetService<ILoggerMessage>()).Returns(LoggerMessage.Object);

            Service = new StudentService(ServiceFactory.Object);
        }

        public async Task AddStudents(params Student[] students)
        {
            List<User> users = students
                .Where(student => student.User != null)
                .Select(student => student.User!)
                .GroupBy(user => user.Id)
                .Select(group => group.First())
                .ToList();

            if (users.Count > 0)
                await DbContextFactory.Context.Users.AddRangeAsync(users);

            await DbContextFactory.Context.Students.AddRangeAsync(students);

            await DbContextFactory.Context.SaveChangesAsync();
        }

        public void Dispose()
        {
            DbContextFactory.Dispose();
        }
    }
}