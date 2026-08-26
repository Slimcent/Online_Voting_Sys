using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using OnlineVoting.Data.Interfaces;
using OnlineVoting.Models.Context;
using OnlineVoting.Models.Entities;
using OnlineVoting.Tests.TestData.Data;
using VotingSystem.Data.Implementation;

namespace OnlineVoting.Tests.IntegrationTests.Data.Implementation
{
    public class UnitOfWorkTransactionTests
    {
        [Fact]
        public async Task BeginTransaction_ShouldCreateActiveTransaction()
        {
            (SqliteConnection connection, VotingDbContext context) = await SqliteTestDbContextFactory.Create();

            await using (connection)
            await using (context)
            {
                UnitOfWork<VotingDbContext> unitOfWork = new(context);

                IDbContextTransaction transaction = await unitOfWork.BeginTransactionAsync();

                Assert.NotNull(transaction);
            }
        }

        [Fact]
        public async Task CommitTransaction_ShouldPersistChanges()
        {
            (SqliteConnection connection, VotingDbContext context) = await SqliteTestDbContextFactory.Create();

            await using (connection)
            await using (context)
            {
                UnitOfWork<VotingDbContext> unitOfWork = new(context);
                IRepository<Faculty> repository = unitOfWork.GetRepository<Faculty>();

                await unitOfWork.BeginTransactionAsync();

                Faculty faculty = FacultyTestData.CreateFaculty("Engineering");

                repository.Add(faculty);
                await unitOfWork.SaveChangesAsync();

                await unitOfWork.CommitTransactionAsync();

                await using VotingDbContext verificationContext = SqliteTestDbContextFactory.Create(connection);

                Faculty? savedFaculty = await verificationContext.Faculties.SingleOrDefaultAsync(item => item.Name == "Engineering");

                Assert.NotNull(savedFaculty);
            }
        }

        [Fact]
        public async Task RollbackTransaction_ShouldDiscardChanges()
        {
            (SqliteConnection connection, VotingDbContext context) = await SqliteTestDbContextFactory.Create();

            await using (connection)
            await using (context)
            {
                UnitOfWork<VotingDbContext> unitOfWork = new(context);
                IRepository<Faculty> repository = unitOfWork.GetRepository<Faculty>();

                await unitOfWork.BeginTransactionAsync();

                Faculty faculty = FacultyTestData.CreateFaculty("Engineering");

                repository.Add(faculty);
                await unitOfWork.SaveChangesAsync();

                await unitOfWork.RollbackTransactionAsync();

                context.ChangeTracker.Clear();

                await using VotingDbContext verificationContext = SqliteTestDbContextFactory.Create(connection);

                Faculty? savedFaculty = await verificationContext.Faculties.SingleOrDefaultAsync(item => item.Name == "Engineering");

                Assert.Null(savedFaculty);
            }
        }

        [Fact]
        public async Task BeginTransaction_WithActiveTransaction_ShouldThrowInvalidOperationException()
        {
            (SqliteConnection connection, VotingDbContext context) = await SqliteTestDbContextFactory.Create();

            await using (connection)
            await using (context)
            {
                UnitOfWork<VotingDbContext> unitOfWork = new(context);

                await unitOfWork.BeginTransactionAsync();

                InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    unitOfWork.BeginTransactionAsync());

                Assert.Equal("A transaction is already active.", exception.Message);
            }
        }
    }
}