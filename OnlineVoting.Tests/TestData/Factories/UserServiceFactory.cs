using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using OnlineVoting.BackgroundTasks.Interfaces;
using OnlineVoting.Data.Interfaces;
using OnlineVoting.Models.Entities;
using OnlineVoting.Services.Implementation;
using OnlineVoting.Services.Interfaces;
using System.Linq.Expressions;
using VotingSystem.Logger;

namespace OnlineVoting.Tests.TestData.Factories
{
    public sealed class UserServiceFactory : IDisposable
    {
        public AuditDbContextFactory DbContextFactory { get; }

        public Mock<IUserStore<User>> UserStore { get; }

        public Mock<IRoleStore<Role>> RoleStore { get; }

        public Mock<UserManager<User>> UserManager { get; }

        public Mock<SignInManager<User>> SignInManager { get; }

        public Mock<RoleManager<Role>> RoleManager { get; }

        public Mock<IRepository<User>> UserRepository { get; }

        public Mock<IRepository<Student>> StudentRepository { get; }

        public Mock<IRepository<Staff>> StaffRepository { get; }

        public Mock<IRepository<Address>> AddressRepository { get; }

        public Mock<IRepository<RegisteredVoter>> RegisteredVoterRepository { get; }
        public Mock<IBackgroundTaskQueue> BackgroundTaskQueue { get; }

        public Mock<IUnitOfWork> UnitOfWork { get; }

        public Mock<IMapper> Mapper { get; }

        public Mock<ILoggerMessage> LoggerMessage { get; }

        public Mock<IAuditTrailService> AuditTrailService { get; }

        public Mock<IRefreshTokenService> RefreshTokenService { get; }

        public Mock<IJwtAuthenticator> JwtAuthenticator { get; }

        public Mock<IServiceFactory> ServiceFactory { get; }

        public UserService Service { get; }

        public UserServiceFactory()
        {
            DbContextFactory = new AuditDbContextFactory();

            DbContextFactory.Context.Database.ExecuteSqlRaw("PRAGMA foreign_keys = OFF;");

            UserStore = new Mock<IUserStore<User>>();
            RoleStore = new Mock<IRoleStore<Role>>();

            UserManager = new Mock<UserManager<User>>(UserStore.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            Mock<IHttpContextAccessor> httpContextAccessor = new();
            Mock<IUserClaimsPrincipalFactory<User>> claimsPrincipalFactory = new();
            Mock<ILogger<SignInManager<User>>> signInManagerLogger = new();
            Mock<IAuthenticationSchemeProvider> authenticationSchemeProvider = new();
            Mock<IUserConfirmation<User>> userConfirmation = new();

            httpContextAccessor.Setup(accessor => accessor.HttpContext)
                .Returns(new DefaultHttpContext());

            SignInManager = new Mock<SignInManager<User>>(UserManager.Object, httpContextAccessor.Object, claimsPrincipalFactory.Object,
                Options.Create(new IdentityOptions()), signInManagerLogger.Object, authenticationSchemeProvider.Object, userConfirmation.Object);

            RoleManager = new Mock<RoleManager<Role>>(RoleStore.Object, null!, null!, null!, null!);

            UserRepository = new Mock<IRepository<User>>();
            StudentRepository = new Mock<IRepository<Student>>();
            StaffRepository = new Mock<IRepository<Staff>>();
            AddressRepository = new Mock<IRepository<Address>>();
            RegisteredVoterRepository = new Mock<IRepository<RegisteredVoter>>();
            UnitOfWork = new Mock<IUnitOfWork>();
            Mapper = new Mock<IMapper>();
            LoggerMessage = new Mock<ILoggerMessage>();
            AuditTrailService = new Mock<IAuditTrailService>();
            RefreshTokenService = new Mock<IRefreshTokenService>();
            JwtAuthenticator = new Mock<IJwtAuthenticator>();
            ServiceFactory = new Mock<IServiceFactory>();
            BackgroundTaskQueue = new Mock<IBackgroundTaskQueue>();

            UserRepository.Setup(repository => repository.GetQueryable(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<Func<IQueryable<User>, IOrderedQueryable<User>>>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Func<IQueryable<User>, IIncludableQueryable<User, object>>>()))
                .Returns((Expression<Func<User, bool>> predicate,
                    Func<IQueryable<User>, IOrderedQueryable<User>> orderBy,
                    int? skip,
                    int? take,
                    Func<IQueryable<User>, IIncludableQueryable<User, object>> include) =>
                {
                    IQueryable<User> query = DbContextFactory.Context.Users;

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

            StaffRepository.Setup(repository => repository.GetQueryable(
                It.IsAny<Expression<Func<Staff, bool>>>(),
                It.IsAny<Func<IQueryable<Staff>, IOrderedQueryable<Staff>>>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Func<IQueryable<Staff>, IIncludableQueryable<Staff, object>>>()))
                .Returns((Expression<Func<Staff, bool>> predicate,
                    Func<IQueryable<Staff>, IOrderedQueryable<Staff>> orderBy,
                    int? skip,
                    int? take,
                    Func<IQueryable<Staff>, IIncludableQueryable<Staff, object>> include) =>
                {
                    IQueryable<Staff> query = DbContextFactory.Context.StaffProfile;

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

            AddressRepository.Setup(repository => repository.GetQueryable(
                It.IsAny<Expression<Func<Address, bool>>>(),
                It.IsAny<Func<IQueryable<Address>, IOrderedQueryable<Address>>>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Func<IQueryable<Address>, IIncludableQueryable<Address, object>>>()))
                .Returns((Expression<Func<Address, bool>> predicate,
                    Func<IQueryable<Address>, IOrderedQueryable<Address>> orderBy,
                    int? skip,
                    int? take,
                    Func<IQueryable<Address>, IIncludableQueryable<Address, object>> include) =>
                {
                    IQueryable<Address> query = DbContextFactory.Context.Addresses;

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

            RegisteredVoterRepository.Setup(repository => repository.GetQueryable(
                It.IsAny<Expression<Func<RegisteredVoter, bool>>>(),
                It.IsAny<Func<IQueryable<RegisteredVoter>, IOrderedQueryable<RegisteredVoter>>>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Func<IQueryable<RegisteredVoter>, IIncludableQueryable<RegisteredVoter, object>>>()))
                .Returns((Expression<Func<RegisteredVoter, bool>> predicate,
                    Func<IQueryable<RegisteredVoter>, IOrderedQueryable<RegisteredVoter>> orderBy,
                    int? skip,
                    int? take,
                    Func<IQueryable<RegisteredVoter>, IIncludableQueryable<RegisteredVoter, object>> include) =>
                {
                    IQueryable<RegisteredVoter> query = DbContextFactory.Context.Set<RegisteredVoter>();

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

            UnitOfWork.Setup(unitOfWork => unitOfWork.GetRepository<User>())
                .Returns(UserRepository.Object);

            UnitOfWork.Setup(unitOfWork => unitOfWork.GetRepository<Student>())
                .Returns(StudentRepository.Object);

            UnitOfWork.Setup(unitOfWork => unitOfWork.GetRepository<Staff>())
                .Returns(StaffRepository.Object);

            UnitOfWork.Setup(unitOfWork => unitOfWork.GetRepository<Address>())
                .Returns(AddressRepository.Object);

            UnitOfWork.Setup(unitOfWork => unitOfWork.GetRepository<RegisteredVoter>())
                .Returns(RegisteredVoterRepository.Object);

            UnitOfWork.Setup(unitOfWork => unitOfWork.SaveChangesAsync())
                .ReturnsAsync(1);

            ServiceFactory.Setup(serviceFactory => serviceFactory.GetService<IUnitOfWork>())
                .Returns(UnitOfWork.Object);

            ServiceFactory.Setup(serviceFactory => serviceFactory.GetService<UserManager<User>>())
                .Returns(UserManager.Object);

            ServiceFactory.Setup(serviceFactory => serviceFactory.GetService<SignInManager<User>>())
                .Returns(SignInManager.Object);

            ServiceFactory.Setup(serviceFactory => serviceFactory.GetService<RoleManager<Role>>())
                .Returns(RoleManager.Object);

            ServiceFactory.Setup(serviceFactory => serviceFactory.GetService<IMapper>())
                .Returns(Mapper.Object);

            ServiceFactory.Setup(serviceFactory => serviceFactory.GetService<ILoggerMessage>())
                .Returns(LoggerMessage.Object);

            ServiceFactory.Setup(serviceFactory => serviceFactory.GetService<IAuditTrailService>())
                .Returns(AuditTrailService.Object);

            ServiceFactory.Setup(serviceFactory => serviceFactory.GetService<IRefreshTokenService>())
                .Returns(RefreshTokenService.Object);

            ServiceFactory.Setup(serviceFactory => serviceFactory.GetService<IJwtAuthenticator>())
                .Returns(JwtAuthenticator.Object);

            ServiceFactory.Setup(serviceFactory => serviceFactory.GetService<IBackgroundTaskQueue>())
                .Returns(BackgroundTaskQueue.Object);

            Service = new UserService(ServiceFactory.Object);
        }

        public void SetLoginUser(User? user)
        {
            UserRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<Func<IQueryable<User>, IOrderedQueryable<User>>>(), It.IsAny<int?>(), It.IsAny<int?>(),
                It.IsAny<Func<IQueryable<User>, IIncludableQueryable<User, object>>>(), It.IsAny<bool>()))
                .ReturnsAsync(user);
        }

        public async Task AddCleanupData(IEnumerable<User> users, IEnumerable<Student>? students = null,
            IEnumerable<Staff>? staff = null, IEnumerable<Address>? addresses = null,
            IEnumerable<RegisteredVoter>? registeredVoters = null)
        {
            List<User> userList = users.ToList();
            List<Student> studentList = students?.ToList() ?? new List<Student>();
            List<Staff> staffList = staff?.ToList() ?? new List<Staff>();
            List<Address> addressList = addresses?.ToList() ?? new List<Address>();
            List<RegisteredVoter> registeredVoterList = registeredVoters?.ToList() ?? new List<RegisteredVoter>();

            Dictionary<string, DateTime> createdAtValues = userList.ToDictionary(x => x.Id, x => x.CreatedAt);

            if (userList.Count > 0)
                await DbContextFactory.Context.Users.AddRangeAsync(userList);

            if (studentList.Count > 0)
                await DbContextFactory.Context.Students.AddRangeAsync(studentList);

            if (staffList.Count > 0)
                await DbContextFactory.Context.StaffProfile.AddRangeAsync(staffList);

            if (addressList.Count > 0)
                await DbContextFactory.Context.Addresses.AddRangeAsync(addressList);

            if (registeredVoterList.Count > 0)
                await DbContextFactory.Context.Set<RegisteredVoter>().AddRangeAsync(registeredVoterList);

            await DbContextFactory.Context.SaveChangesAsync();

            foreach (User user in userList)
                user.CreatedAt = createdAtValues[user.Id];

            if (userList.Count > 0)
                await DbContextFactory.Context.SaveChangesAsync();
        }

        public void Dispose()
        {
            DbContextFactory.Dispose();
        }
    }
}