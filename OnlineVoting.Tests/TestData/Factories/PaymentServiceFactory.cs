using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using OnlineVoting.Data.Interfaces;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Interfaces;
using OnlineVoting.Services.Implementation;
using OnlineVoting.Services.Interfaces;
using OnlineVoting.Services.Interfaces.Payments;
using System.Linq.Expressions;
using VotingSystem.Logger;
using OnlineVoting.Caching.Configuration;
using OnlineVoting.Caching.Interfaces;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Pagination;

namespace OnlineVoting.Tests.TestData.Factories
{
    public sealed class PaymentServiceFactory : IDisposable
    {
        public AuditDbContextFactory DbContextFactory { get; }

        public Mock<IRepository<Invoice>> InvoiceRepository { get; }
        public Mock<IRepository<InvoiceStatus>> InvoiceStatusRepository { get; }
        public Mock<IRepository<PaymentTransaction>> PaymentTransactionRepository { get; }
        public Mock<IRepository<PaymentGateway>> PaymentGatewayRepository { get; }
        public Mock<IRepository<PaymentStatus>> PaymentStatusRepository { get; }
        public Mock<IRepository<IdempotencyRecord>> IdempotencyRecordRepository { get; }
        public Mock<IRepository<PositionApplication>> PositionApplicationRepository { get; }
        public Mock<IRepository<PositionApplicationStatus>> PositionApplicationStatusRepository { get; }
        public Mock<ICacheService> CacheService { get; }

        public Mock<IUnitOfWork> UnitOfWork { get; }
        public Mock<IServiceFactory> ServiceFactory { get; }
        public Mock<ICurrentUserContext> CurrentUserContext { get; }
        public Mock<IPaymentGatewayResolver> PaymentGatewayResolver { get; }
        public Mock<IMapper> Mapper { get; }
        public Mock<ILoggerMessage> LoggerMessage { get; }

        public PaymentService Service { get; }

        public PaymentServiceFactory()
        {
            DbContextFactory = new AuditDbContextFactory();

            DbContextFactory.Context.Database.ExecuteSqlRaw("PRAGMA foreign_keys = OFF;");

            InvoiceRepository = new Mock<IRepository<Invoice>>();
            InvoiceStatusRepository = new Mock<IRepository<InvoiceStatus>>();
            PaymentTransactionRepository = new Mock<IRepository<PaymentTransaction>>();
            PaymentGatewayRepository = new Mock<IRepository<PaymentGateway>>();
            PaymentStatusRepository = new Mock<IRepository<PaymentStatus>>();
            IdempotencyRecordRepository = new Mock<IRepository<IdempotencyRecord>>();
            PositionApplicationRepository = new Mock<IRepository<PositionApplication>>();
            PositionApplicationStatusRepository = new Mock<IRepository<PositionApplicationStatus>>();
            CacheService = new Mock<ICacheService>();

            UnitOfWork = new Mock<IUnitOfWork>();
            ServiceFactory = new Mock<IServiceFactory>();
            CurrentUserContext = new Mock<ICurrentUserContext>();
            PaymentGatewayResolver = new Mock<IPaymentGatewayResolver>();
            Mapper = new Mock<IMapper>();
            LoggerMessage = new Mock<ILoggerMessage>();

            SetupRepositories();
            SetupServiceFactory();
            SetupUnitOfWork();
            SetupCacheService();

            Service = new PaymentService(ServiceFactory.Object);
        }

        private void SetupRepositories()
        {
            SetupRepository(InvoiceRepository);
            SetupRepository(InvoiceStatusRepository);
            SetupRepository(PaymentTransactionRepository);
            SetupRepository(PaymentGatewayRepository);
            SetupRepository(PaymentStatusRepository);
            SetupRepository(IdempotencyRecordRepository);
            SetupRepository(PositionApplicationRepository);
            SetupRepository(PositionApplicationStatusRepository);

            UnitOfWork.Setup(x => x.GetRepository<Invoice>()).Returns(InvoiceRepository.Object);
            UnitOfWork.Setup(x => x.GetRepository<InvoiceStatus>()).Returns(InvoiceStatusRepository.Object);
            UnitOfWork.Setup(x => x.GetRepository<PaymentTransaction>()).Returns(PaymentTransactionRepository.Object);
            UnitOfWork.Setup(x => x.GetRepository<PaymentGateway>()).Returns(PaymentGatewayRepository.Object);
            UnitOfWork.Setup(x => x.GetRepository<PaymentStatus>()).Returns(PaymentStatusRepository.Object);
            UnitOfWork.Setup(x => x.GetRepository<IdempotencyRecord>()).Returns(IdempotencyRecordRepository.Object);
            UnitOfWork.Setup(x => x.GetRepository<PositionApplication>()).Returns(PositionApplicationRepository.Object);
            UnitOfWork.Setup(x => x.GetRepository<PositionApplicationStatus>()).Returns(PositionApplicationStatusRepository.Object);
        }

        private void SetupServiceFactory()
        {
            ServiceFactory.Setup(x => x.GetService<IUnitOfWork>()).Returns(UnitOfWork.Object);
            ServiceFactory.Setup(x => x.GetService<ICurrentUserContext>()).Returns(CurrentUserContext.Object);
            ServiceFactory.Setup(x => x.GetService<IPaymentGatewayResolver>()).Returns(PaymentGatewayResolver.Object);
            ServiceFactory.Setup(x => x.GetService<IMapper>()).Returns(Mapper.Object);
            ServiceFactory.Setup(x => x.GetService<ILoggerMessage>()).Returns(LoggerMessage.Object);
            ServiceFactory.Setup(x => x.GetService<ICacheService>()).Returns(CacheService.Object);
        }

        private void SetupUnitOfWork()
        {
            UnitOfWork.Setup(x => x.SaveChangesAsync())
                .Returns(() => DbContextFactory.Context.SaveChangesAsync());
        }

        private void SetupCacheService()
        {
            CacheService.Setup(x => x.GetOrCreate(It.IsAny<string>(),
                    It.IsAny<Func<CancellationToken, ValueTask<PagedResponse<InvoiceResponse>>>>(),
                    It.IsAny<CacheEntryOptions?>(),
                    It.IsAny<CancellationToken>()))
                .Returns((string cacheKey, Func<CancellationToken, ValueTask<PagedResponse<InvoiceResponse>>> cacheFactory,
                    CacheEntryOptions? cacheEntryOptions, CancellationToken cancellationToken) => cacheFactory(cancellationToken));

            CacheService.Setup(x => x.GetOrCreate(It.IsAny<string>(),
                    It.IsAny<Func<CancellationToken, ValueTask<InvoiceResponse?>>>(),
                    It.IsAny<CacheEntryOptions?>(),
                    It.IsAny<CancellationToken>()))
                .Returns((string cacheKey, Func<CancellationToken, ValueTask<InvoiceResponse?>> cacheFactory,
                    CacheEntryOptions? cacheEntryOptions, CancellationToken cancellationToken) => cacheFactory(cancellationToken));

            CacheService.Setup(x => x.GetOrCreate(It.IsAny<string>(),
                    It.IsAny<Func<CancellationToken, ValueTask<PagedResponse<PaymentTransactionResponse>>>>(),
                    It.IsAny<CacheEntryOptions?>(),
                    It.IsAny<CancellationToken>()))
                .Returns((string cacheKey, Func<CancellationToken, ValueTask<PagedResponse<PaymentTransactionResponse>>> cacheFactory,
                    CacheEntryOptions? cacheEntryOptions, CancellationToken cancellationToken) => cacheFactory(cancellationToken));

            CacheService.Setup(x => x.GetOrCreate(It.IsAny<string>(),
                    It.IsAny<Func<CancellationToken, ValueTask<PaymentTransactionResponse?>>>(),
                    It.IsAny<CacheEntryOptions?>(),
                    It.IsAny<CancellationToken>()))
                .Returns((string cacheKey, Func<CancellationToken, ValueTask<PaymentTransactionResponse?>> cacheFactory,
                    CacheEntryOptions? cacheEntryOptions, CancellationToken cancellationToken) => cacheFactory(cancellationToken));
        }

        private void SetupRepository<TEntity>(Mock<IRepository<TEntity>> repository) where TEntity : class
        {
            repository.Setup(x => x.GetQueryable(
                It.IsAny<Expression<Func<TEntity, bool>>?>(),
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

            repository.Setup(x => x.GetSingleByAsync(
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

            repository.Setup(x => x.GetSingleByAsync(It.IsAny<Expression<Func<TEntity, bool>>>()))
                .Returns(async (Expression<Func<TEntity, bool>> predicate) =>
                {
                    IQueryable<TEntity> query = DbContextFactory.Context.Set<TEntity>();

                    return await query.FirstOrDefaultAsync(predicate);
                });

            repository.Setup(x => x.Add(It.IsAny<TEntity>()))
                .Callback<TEntity>(entity => DbContextFactory.Context.Set<TEntity>().Add(entity));

            repository.Setup(x => x.Update(It.IsAny<TEntity>()))
                .Callback<TEntity>(entity => DbContextFactory.Context.Set<TEntity>().Update(entity));
        }

        public void Dispose()
        {
            DbContextFactory.Dispose();
        }
    }
}