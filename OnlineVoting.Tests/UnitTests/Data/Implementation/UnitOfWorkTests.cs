using Microsoft.EntityFrameworkCore;
using OnlineVoting.Data.Interfaces;
using OnlineVoting.Models.Context;
using OnlineVoting.Models.Entities;
using OnlineVoting.Tests.TestData.Data;
using VotingSystem.Data.Implementation;

namespace OnlineVoting.Tests.UnitTests.Data.Implementation
{
    public class UnitOfWorkTests
    {
        [Fact]
        public Task Constructor_WithNullContext_ShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new UnitOfWork<VotingDbContext>(null!));

            return Task.CompletedTask;
        }

        [Fact]
        public Task GetRepository_FirstCall_ShouldCreateRepository()
        {
            string databaseName = Guid.NewGuid().ToString();

            using VotingDbContext context = TestDbContextFactory.Create(databaseName);
            UnitOfWork<VotingDbContext> unitOfWork = new(context);

            IRepository<Faculty> repository = unitOfWork.GetRepository<Faculty>();

            Assert.NotNull(repository);

            return Task.CompletedTask;
        }

        [Fact]
        public Task GetRepository_RepeatedCalls_ShouldReturnSameRepositoryInstance()
        {
            string databaseName = Guid.NewGuid().ToString();

            using VotingDbContext context = TestDbContextFactory.Create(databaseName);
            UnitOfWork<VotingDbContext> unitOfWork = new(context);

            IRepository<Faculty> firstRepository = unitOfWork.GetRepository<Faculty>();
            IRepository<Faculty> secondRepository = unitOfWork.GetRepository<Faculty>();

            Assert.Same(firstRepository, secondRepository);

            return Task.CompletedTask;
        }

        [Fact]
        public Task GetRepository_DifferentEntityTypes_ShouldReturnDifferentRepositoryInstances()
        {
            string databaseName = Guid.NewGuid().ToString();

            using VotingDbContext context = TestDbContextFactory.Create(databaseName);
            UnitOfWork<VotingDbContext> unitOfWork = new(context);

            IRepository<Faculty> facultyRepository = unitOfWork.GetRepository<Faculty>();
            IRepository<Department> departmentRepository = unitOfWork.GetRepository<Department>();

            Assert.NotSame(facultyRepository, departmentRepository);

            return Task.CompletedTask;
        }

        [Fact]
        public async Task SaveChanges_ShouldPersistPendingChanges()
        {
            string databaseName = Guid.NewGuid().ToString();

            await using VotingDbContext context = TestDbContextFactory.Create(databaseName);
            UnitOfWork<VotingDbContext> unitOfWork = new(context);

            IRepository<Faculty> repository = unitOfWork.GetRepository<Faculty>();

            Faculty faculty = FacultyTestData.CreateFaculty("Engineering");

            repository.Add(faculty);

            int result = unitOfWork.SaveChanges();

            Assert.Equal(1, result);

            await using VotingDbContext verificationContext = TestDbContextFactory.Create(databaseName);

            Faculty? savedFaculty = await verificationContext.Faculties.SingleOrDefaultAsync(item => item.Name == "Engineering");

            Assert.NotNull(savedFaculty);
        }

        [Fact]
        public async Task SaveChangesAsync_ShouldPersistPendingChanges()
        {
            string databaseName = Guid.NewGuid().ToString();

            await using VotingDbContext context = TestDbContextFactory.Create(databaseName);
            UnitOfWork<VotingDbContext> unitOfWork = new(context);

            IRepository<Faculty> repository = unitOfWork.GetRepository<Faculty>();

            Faculty faculty = FacultyTestData.CreateFaculty("Engineering");

            repository.Add(faculty);

            int result = await unitOfWork.SaveChangesAsync();

            Assert.Equal(1, result);

            await using VotingDbContext verificationContext = TestDbContextFactory.Create(databaseName);

            Faculty? savedFaculty = await verificationContext.Faculties.SingleOrDefaultAsync(item => item.Name == "Engineering");

            Assert.NotNull(savedFaculty);
        }

        [Fact]
        public async Task CommitTransactionAsync_WithoutActiveTransaction_ShouldThrowInvalidOperationException()
        {
            string databaseName = Guid.NewGuid().ToString();

            await using VotingDbContext context = TestDbContextFactory.Create(databaseName);
            UnitOfWork<VotingDbContext> unitOfWork = new(context);

            InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                unitOfWork.CommitTransactionAsync());

            Assert.Equal("There is no active transaction to commit.", exception.Message);
        }

        [Fact]
        public async Task RollbackTransactionAsync_WithoutActiveTransaction_ShouldCompleteWithoutException()
        {
            string databaseName = Guid.NewGuid().ToString();

            await using VotingDbContext context = TestDbContextFactory.Create(databaseName);
            UnitOfWork<VotingDbContext> unitOfWork = new(context);

            await unitOfWork.RollbackTransactionAsync();
        }
    }
}