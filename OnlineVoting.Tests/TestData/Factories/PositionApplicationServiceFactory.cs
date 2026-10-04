using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using OnlineVoting.Caching.Configuration;
using OnlineVoting.Caching.Interfaces;
using OnlineVoting.Data.Interfaces;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Interfaces;
using OnlineVoting.Models.Pagination;
using OnlineVoting.Services.Implementation;
using OnlineVoting.Services.Interfaces;
using OnlineVoting.Tests.TestData.Factories;
using System.Linq.Expressions;
using VotingSystem.Logger;

namespace OnlineVoting.Tests.Factories
{
    public sealed class PositionApplicationServiceFactory : IDisposable
    {
        private IDbContextTransaction? _transaction;

        public AuditDbContextFactory DbContextFactory { get; }
        public Mock<IRepository<PositionApplication>> PositionApplicationRepository { get; }
        public Mock<IRepository<PositionApplicationStatus>> PositionApplicationStatusRepository { get; }
        public Mock<IRepository<ElectionPosition>> ElectionPositionRepository { get; }
        public Mock<IRepository<Student>> StudentRepository { get; }
        public Mock<IRepository<IdempotencyRecord>> IdempotencyRecordRepository { get; }
        public Mock<IRepository<Invoice>> InvoiceRepository { get; }
        public Mock<IRepository<InvoiceStatus>> InvoiceStatusRepository { get; }
        public Mock<IRepository<ApplicationUserRole>> ApplicationUserRoleRepository { get; }
        public Mock<IRepository<Contestant>> ContestantRepository { get; }
        public Mock<IUnitOfWork> UnitOfWork { get; }
        public Mock<IMapper> Mapper { get; }
        public Mock<ILoggerMessage> LoggerMessage { get; }
        public Mock<IServiceFactory> ServiceFactory { get; }
        public Mock<ICurrentUserContext> CurrentUserContext { get; }
        public Mock<IPaymentService> PaymentService { get; }
        public Mock<ICacheService> CacheService { get; }
        public PositionApplicationService Service { get; }

        public PositionApplicationServiceFactory()
        {
            DbContextFactory = new AuditDbContextFactory();

            DbContextFactory.Context.Database.ExecuteSqlRaw("PRAGMA foreign_keys = OFF;");

            PositionApplicationRepository = new Mock<IRepository<PositionApplication>>();
            PositionApplicationStatusRepository = new Mock<IRepository<PositionApplicationStatus>>();
            ElectionPositionRepository = new Mock<IRepository<ElectionPosition>>();
            StudentRepository = new Mock<IRepository<Student>>();
            IdempotencyRecordRepository = new Mock<IRepository<IdempotencyRecord>>();
            InvoiceRepository = new Mock<IRepository<Invoice>>();
            InvoiceStatusRepository = new Mock<IRepository<InvoiceStatus>>();
            ApplicationUserRoleRepository = new Mock<IRepository<ApplicationUserRole>>();
            ContestantRepository = new Mock<IRepository<Contestant>>();

            UnitOfWork = new Mock<IUnitOfWork>();
            Mapper = new Mock<IMapper>();
            LoggerMessage = new Mock<ILoggerMessage>();
            ServiceFactory = new Mock<IServiceFactory>();
            CurrentUserContext = new Mock<ICurrentUserContext>();
            PaymentService = new Mock<IPaymentService>();
            CacheService = new Mock<ICacheService>();

            SetupRepositories();
            SetupTransactions();
            SetupUnitOfWork();
            SetupServiceFactory();
            SetupCacheService();

            Service = new PositionApplicationService(ServiceFactory.Object);
        }

        private void SetupRepositories()
        {
            SetupRepository(PositionApplicationRepository);
            SetupRepository(PositionApplicationStatusRepository);
            SetupRepository(ElectionPositionRepository);
            SetupRepository(StudentRepository);
            SetupRepository(IdempotencyRecordRepository);
            SetupRepository(InvoiceRepository);
            SetupRepository(InvoiceStatusRepository);
            SetupRepository(ApplicationUserRoleRepository);
            SetupRepository(ContestantRepository);

            UnitOfWork.Setup(x => x.GetRepository<PositionApplication>())
                .Returns(PositionApplicationRepository.Object);

            UnitOfWork.Setup(x => x.GetRepository<PositionApplicationStatus>())
                .Returns(PositionApplicationStatusRepository.Object);

            UnitOfWork.Setup(x => x.GetRepository<ElectionPosition>())
                .Returns(ElectionPositionRepository.Object);

            UnitOfWork.Setup(x => x.GetRepository<Student>())
                .Returns(StudentRepository.Object);

            UnitOfWork.Setup(x => x.GetRepository<IdempotencyRecord>())
                .Returns(IdempotencyRecordRepository.Object);

            UnitOfWork.Setup(x => x.GetRepository<Invoice>())
                .Returns(InvoiceRepository.Object);

            UnitOfWork.Setup(x => x.GetRepository<InvoiceStatus>())
                .Returns(InvoiceStatusRepository.Object);

            UnitOfWork.Setup(x => x.GetRepository<ApplicationUserRole>())
                .Returns(ApplicationUserRoleRepository.Object);

            UnitOfWork.Setup(x => x.GetRepository<Contestant>())
                .Returns(ContestantRepository.Object);
        }

        private void SetupTransactions()
        {
            UnitOfWork
                .Setup(x => x.BeginTransactionAsync())
                .Returns(async () =>
                {
                    if (_transaction is not null)
                    {
                        throw new InvalidOperationException("A transaction is already active.");
                    }

                    _transaction = await DbContextFactory.Context.Database.BeginTransactionAsync();

                    return _transaction;
                });

            UnitOfWork
                .Setup(x => x.CommitTransactionAsync())
                .Returns(async () =>
                {
                    if (_transaction is null)
                    {
                        throw new InvalidOperationException("There is no active transaction to commit.");
                    }

                    try
                    {
                        await _transaction.CommitAsync();
                    }
                    finally
                    {
                        await _transaction.DisposeAsync();
                        _transaction = null;
                    }
                });

            UnitOfWork
                .Setup(x => x.RollbackTransactionAsync())
                .Returns(async () =>
                {
                    if (_transaction is null)
                    {
                        return;
                    }

                    try
                    {
                        await _transaction.RollbackAsync();
                    }
                    finally
                    {
                        await _transaction.DisposeAsync();
                        _transaction = null;
                    }
                });
        }

        private void SetupUnitOfWork()
        {
            UnitOfWork.Setup(x => x.SaveChangesAsync())
                .Returns(() => DbContextFactory.Context.SaveChangesAsync());
        }

        private void SetupServiceFactory()
        {
            ServiceFactory
                .Setup(x => x.GetService<IUnitOfWork>())
                .Returns(UnitOfWork.Object);

            ServiceFactory
                .Setup(x => x.GetService<IMapper>())
                .Returns(Mapper.Object);

            ServiceFactory
                .Setup(x => x.GetService<ILoggerMessage>())
                .Returns(LoggerMessage.Object);

            ServiceFactory
                .Setup(x => x.GetService<ICurrentUserContext>())
                .Returns(CurrentUserContext.Object);

            ServiceFactory
                .Setup(x => x.GetService<IPaymentService>())
                .Returns(PaymentService.Object);

            ServiceFactory
                .Setup(x => x.GetService<ICacheService>())
                .Returns(CacheService.Object);
        }

        private void SetupCacheService()
        {
            CacheService
                .Setup(x => x.GetOrCreate(It.IsAny<string>(),
                    It.IsAny<Func<CancellationToken, ValueTask<PagedResponse<PositionApplicationResponse>>>>(),
                    It.IsAny<CacheEntryOptions?>(),
                    It.IsAny<CancellationToken>()))
                .Returns((string cacheKey, Func<CancellationToken, ValueTask<PagedResponse<PositionApplicationResponse>>> cacheFactory,
                    CacheEntryOptions? cacheEntryOptions, CancellationToken cancellationToken) => cacheFactory(cancellationToken));

            CacheService
                .Setup(x => x.GetOrCreate(It.IsAny<string>(),
                    It.IsAny<Func<CancellationToken, ValueTask<PositionApplicationResponse?>>>(),
                    It.IsAny<CacheEntryOptions?>(),
                    It.IsAny<CancellationToken>()))
                .Returns((string cacheKey, Func<CancellationToken, ValueTask<PositionApplicationResponse?>> cacheFactory,
                    CacheEntryOptions? cacheEntryOptions, CancellationToken cancellationToken) => cacheFactory(cancellationToken));
        }

        private void SetupRepository<TEntity>(Mock<IRepository<TEntity>> repository) where TEntity : class
        {
            repository
                .Setup(x => x.GetQueryable(It.IsAny<Expression<Func<TEntity, bool>>?>(),
                    It.IsAny<Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>?>(),
                    It.IsAny<int?>(),
                    It.IsAny<int?>(),
                    It.IsAny<Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>?>()))
                .Returns((
                    Expression<Func<TEntity, bool>>? predicate,
                    Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy,
                    int? skip,
                    int? take,
                    Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include) =>
                {
                    IQueryable<TEntity> query = DbContextFactory.Context.Set<TEntity>();

                    if (predicate is not null)
                    {
                        query = query.Where(predicate);
                    }

                    if (orderBy is not null)
                    {
                        query = orderBy(query);
                    }

                    if (include is not null)
                    {
                        query = include(query);
                    }

                    if (skip is not null)
                    {
                        query = query.Skip(skip.Value);
                    }

                    if (take is not null)
                    {
                        query = query.Take(take.Value);
                    }

                    return query;
                });

            repository
                .Setup(x => x.GetSingleByAsync(
                    It.IsAny<Expression<Func<TEntity, bool>>?>(),
                    It.IsAny<Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>?>(),
                    It.IsAny<int?>(),
                    It.IsAny<int?>(),
                    It.IsAny<Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>?>(),
                    It.IsAny<bool>()))
                .Returns(async (
                    Expression<Func<TEntity, bool>>? predicate,
                    Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy,
                    int? skip,
                    int? take,
                    Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include,
                    bool tracking) =>
                {
                    IQueryable<TEntity> query = DbContextFactory.Context.Set<TEntity>();

                    if (predicate is not null)
                    {
                        query = query.Where(predicate);
                    }

                    if (orderBy is not null)
                    {
                        query = orderBy(query);
                    }

                    if (include is not null)
                    {
                        query = include(query);
                    }

                    if (skip is not null)
                    {
                        query = query.Skip(skip.Value);
                    }

                    if (take is not null)
                    {
                        query = query.Take(take.Value);
                    }

                    if (!tracking)
                    {
                        query = query.AsNoTracking();
                    }

                    return await query.FirstOrDefaultAsync();
                });

            repository
                .Setup(x => x.GetSingleByAsync(It.IsAny<Expression<Func<TEntity, bool>>>()))
                .Returns(async (Expression<Func<TEntity, bool>> predicate) =>
                {
                    IQueryable<TEntity> query = DbContextFactory.Context.Set<TEntity>();

                    return await query.FirstOrDefaultAsync(predicate);
                });

            repository
                .Setup(x => x.AddAsync(It.IsAny<TEntity>(), It.IsAny<bool>()))
                .Returns(async (TEntity entity, bool tracking) =>
                {
                    DbContextFactory.Context.Set<TEntity>().Add(entity);

                    await DbContextFactory.Context.SaveChangesAsync();

                    if (!tracking)
                    {
                        DbContextFactory.Context.Entry(entity).State = EntityState.Detached;
                    }

                    return entity;
                });

            repository
                .Setup(x => x.UpdateAsync(It.IsAny<TEntity>(), It.IsAny<bool>()))
                .Returns(async (TEntity entity, bool tracking) =>
                {
                    DbContextFactory.Context.Set<TEntity>().Attach(entity);
                    DbContextFactory.Context.Entry(entity).State = EntityState.Modified;

                    try
                    {
                        await DbContextFactory.Context.SaveChangesAsync();
                    }
                    finally
                    {
                        if (!tracking)
                        {
                            DbContextFactory.Context.Entry(entity).State = EntityState.Detached;
                        }
                    }

                    return entity;
                });

            repository
                .Setup(x => x.Add(It.IsAny<TEntity>()))
                .Callback<TEntity>(entity => DbContextFactory.Context.Set<TEntity>().Add(entity));

            repository
                .Setup(x => x.Update(It.IsAny<TEntity>()))
                .Callback<TEntity>(entity => DbContextFactory.Context.Set<TEntity>().Update(entity));
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            DbContextFactory.Dispose();
        }
    }
}