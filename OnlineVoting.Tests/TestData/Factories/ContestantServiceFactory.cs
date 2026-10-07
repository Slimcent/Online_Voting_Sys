using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using OnlineVoting.Caching.Configuration;
using OnlineVoting.Caching.Interfaces;
using OnlineVoting.Data.Interfaces;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Pagination;
using OnlineVoting.Services.Implementation;
using OnlineVoting.Services.Interfaces;
using OnlineVoting.Tests.TestData.Factories;
using System.Linq.Expressions;
using VotingSystem.Logger;

namespace OnlineVoting.Tests.Factories
{
    public sealed class ContestantServiceFactory : IDisposable
    {
        public AuditDbContextFactory DbContextFactory { get; }
        public Mock<IRepository<Contestant>> ContestantRepository { get; }
        public Mock<IUnitOfWork> UnitOfWork { get; }
        public Mock<IMapper> Mapper { get; }
        public Mock<ILoggerMessage> LoggerMessage { get; }
        public Mock<IServiceFactory> ServiceFactory { get; }
        public Mock<ICacheService> CacheService { get; }
        public ContestantService Service { get; }

        public ContestantServiceFactory()
        {
            DbContextFactory = new AuditDbContextFactory();

            DbContextFactory.Context.Database.ExecuteSqlRaw("PRAGMA foreign_keys = OFF;");

            ContestantRepository = new Mock<IRepository<Contestant>>();

            UnitOfWork = new Mock<IUnitOfWork>();
            Mapper = new Mock<IMapper>();
            LoggerMessage = new Mock<ILoggerMessage>();
            ServiceFactory = new Mock<IServiceFactory>();
            CacheService = new Mock<ICacheService>();

            SetupRepositories();
            SetupUnitOfWork();
            SetupServiceFactory();
            SetupCacheService();

            Service = new ContestantService(ServiceFactory.Object);
        }

        private void SetupRepositories()
        {
            SetupRepository(ContestantRepository);

            UnitOfWork.Setup(x => x.GetRepository<Contestant>())
                .Returns(ContestantRepository.Object);
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
                .Setup(x => x.GetService<ICacheService>())
                .Returns(CacheService.Object);
        }

        private void SetupCacheService()
        {
            CacheService
                .Setup(x => x.GetOrCreate(It.IsAny<string>(),
                    It.IsAny<Func<CancellationToken, ValueTask<PagedResponse<ContestantResponse>>>>(),
                    It.IsAny<CacheEntryOptions?>(),
                    It.IsAny<CancellationToken>()))
                .Returns((string cacheKey, Func<CancellationToken, ValueTask<PagedResponse<ContestantResponse>>> cacheFactory,
                    CacheEntryOptions? cacheEntryOptions, CancellationToken cancellationToken) => cacheFactory(cancellationToken));

            CacheService
                .Setup(x => x.GetOrCreate(It.IsAny<string>(),
                    It.IsAny<Func<CancellationToken, ValueTask<ContestantResponse?>>>(),
                    It.IsAny<CacheEntryOptions?>(),
                    It.IsAny<CancellationToken>()))
                .Returns((string cacheKey, Func<CancellationToken, ValueTask<ContestantResponse?>> cacheFactory,
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
            DbContextFactory.Dispose();
        }
    }
}