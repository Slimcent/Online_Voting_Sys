using Microsoft.EntityFrameworkCore;
using OnlineVoting.Models.Context;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Pagination;
using OnlineVoting.Tests.TestData.Data;
using OnLineVoting.Data.Implementation;

namespace OnlineVoting.Tests.UnitTests.Data.Implementation
{
    public class RepositoryTests
    {
        [Fact]
        public Task Constructor_WithNullContext_ShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new Repository<Faculty>(null!));

            return Task.CompletedTask;
        }

        [Fact]
        public async Task Add_ShouldAddEntityWithoutSaving()
        {
            string databaseName = Guid.NewGuid().ToString();

            await using VotingDbContext context = TestDbContextFactory.Create(databaseName);

            Repository<Faculty> repository = new(context);

            Faculty faculty = FacultyTestData.CreateFaculty("Engineering");

            Faculty result = repository.Add(faculty);

            Assert.Same(faculty, result);
            Assert.Equal(EntityState.Added, context.Entry(faculty).State);

            await using VotingDbContext verificationContext = TestDbContextFactory.Create(databaseName);

            List<Faculty> savedFaculties = await verificationContext.Faculties.ToListAsync();

            Assert.Empty(savedFaculties);
        }

        [Fact]
        public async Task AddAsync_WithDefaultTracking_ShouldSaveAndDetachEntity()
        {
            string databaseName = Guid.NewGuid().ToString();

            await using VotingDbContext context = TestDbContextFactory.Create(databaseName);

            Repository<Faculty> repository = new(context);

            Faculty faculty = FacultyTestData.CreateFaculty("Engineering");

            Faculty result = await repository.AddAsync(faculty);

            Assert.Same(faculty, result);
            Assert.Equal(EntityState.Detached, context.Entry(faculty).State);

            await using VotingDbContext verificationContext = TestDbContextFactory.Create(databaseName);

            Faculty? savedFaculty = await verificationContext.Faculties.SingleOrDefaultAsync(item => item.Name == "Engineering");

            Assert.NotNull(savedFaculty);
        }

        [Fact]
        public async Task AddAsync_WithTrackingEnabled_ShouldSaveAndKeepEntityTracked()
        {
            string databaseName = Guid.NewGuid().ToString();

            await using VotingDbContext context = TestDbContextFactory.Create(databaseName);

            Repository<Faculty> repository = new(context);

            Faculty faculty = FacultyTestData.CreateFaculty("Engineering");

            Faculty result = await repository.AddAsync(faculty, true);

            Assert.Same(faculty, result);
            Assert.Equal(EntityState.Unchanged, context.Entry(faculty).State);

            await using VotingDbContext verificationContext = TestDbContextFactory.Create(databaseName);

            Faculty? savedFaculty = await verificationContext.Faculties.SingleOrDefaultAsync(item => item.Name == "Engineering");

            Assert.NotNull(savedFaculty);
        }

        [Fact]
        public async Task AddRange_ShouldAddEntitiesWithoutSaving()
        {
            string databaseName = Guid.NewGuid().ToString();

            await using VotingDbContext context = TestDbContextFactory.Create(databaseName);

            Repository<Faculty> repository = new(context);

            List<Faculty> faculties = FacultyTestData.CreateFaculties();

            repository.AddRange(faculties);

            Assert.All(faculties, faculty => Assert.Equal(EntityState.Added, context.Entry(faculty).State));

            await using VotingDbContext verificationContext = TestDbContextFactory.Create(databaseName);

            List<Faculty> savedFaculties = await verificationContext.Faculties.ToListAsync();

            Assert.Empty(savedFaculties);
        }

        [Fact]
        public async Task AddRangeAsync_ShouldAddAndSaveEntities()
        {
            string databaseName = Guid.NewGuid().ToString();

            await using VotingDbContext context = TestDbContextFactory.Create(databaseName);

            Repository<Faculty> repository = new(context);

            List<Faculty> faculties = FacultyTestData.CreateFaculties();

            await repository.AddRangeAsync(faculties);

            await using VotingDbContext verificationContext = TestDbContextFactory.Create(databaseName);

            List<Faculty> savedFaculties = await verificationContext.Faculties
                .OrderBy(faculty => faculty.Name)
                .ToListAsync();

            Assert.Equal(3, savedFaculties.Count);
            Assert.Equal("Arts", savedFaculties[0].Name);
            Assert.Equal("Engineering", savedFaculties[1].Name);
            Assert.Equal("Science", savedFaculties[2].Name);
        }

        [Fact]
        public async Task Save_ShouldPersistPendingChanges()
        {
            string databaseName = Guid.NewGuid().ToString();

            await using VotingDbContext context = TestDbContextFactory.Create(databaseName);

            Repository<Faculty> repository = new(context);

            Faculty faculty = FacultyTestData.CreateFaculty("Engineering");
            repository.Add(faculty);

            int result = repository.Save();

            Assert.Equal(1, result);

            await using VotingDbContext verificationContext = TestDbContextFactory.Create(databaseName);

            Faculty? savedFaculty = await verificationContext.Faculties.SingleOrDefaultAsync(item => item.Name == "Engineering");

            Assert.NotNull(savedFaculty);
        }

        [Fact]
        public async Task SaveAsync_ShouldPersistPendingChanges()
        {
            string databaseName = Guid.NewGuid().ToString();

            await using VotingDbContext context = TestDbContextFactory.Create(databaseName);

            Repository<Faculty> repository = new(context);

            Faculty faculty = FacultyTestData.CreateFaculty("Engineering");
            repository.Add(faculty);

            int result = await repository.SaveAsync();

            Assert.Equal(1, result);

            await using VotingDbContext verificationContext = TestDbContextFactory.Create(databaseName);

            Faculty? savedFaculty = await verificationContext.Faculties.SingleOrDefaultAsync(item => item.Name == "Engineering");

            Assert.NotNull(savedFaculty);
        }

        [Fact]
        public async Task Any_WithExistingRecords_ShouldReturnTrue()
        {
            string databaseName = Guid.NewGuid().ToString();

            await using VotingDbContext context = TestDbContextFactory.Create(databaseName);

            Repository<Faculty> repository = new(context);

            await FacultyTestData.SeedFaculties(context);

            bool result = repository.Any();

            Assert.True(result);
        }

        [Fact]
        public async Task Any_WithMatchingPredicate_ShouldReturnTrue()
        {
            string databaseName = Guid.NewGuid().ToString();

            await using VotingDbContext context = TestDbContextFactory.Create(databaseName);

            Repository<Faculty> repository = new(context);

            await FacultyTestData.SeedFaculties(context);

            bool result = repository.Any(faculty => faculty.Name == "Engineering");

            Assert.True(result);
        }

        [Fact]
        public async Task Any_WithNonMatchingPredicate_ShouldReturnFalse()
        {
            string databaseName = Guid.NewGuid().ToString();

            await using VotingDbContext context = TestDbContextFactory.Create(databaseName);

            Repository<Faculty> repository = new(context);

            await FacultyTestData.SeedFaculties(context);

            bool result = repository.Any(faculty => faculty.Name == "Medicine");

            Assert.False(result);
        }

        [Fact]
        public async Task AnyAsync_WithMatchingPredicate_ShouldReturnTrue()
        {
            string databaseName = Guid.NewGuid().ToString();

            await using VotingDbContext context = TestDbContextFactory.Create(databaseName);

            Repository<Faculty> repository = new(context);

            await FacultyTestData.SeedFaculties(context);

            bool result = await repository.AnyAsync(faculty => faculty.Name == "Science");

            Assert.True(result);
        }

        [Fact]
        public async Task Count_ShouldReturnTotalNumberOfRecords()
        {
            string databaseName = Guid.NewGuid().ToString();

            await using VotingDbContext context = TestDbContextFactory.Create(databaseName);

            Repository<Faculty> repository = new(context);

            await FacultyTestData.SeedFaculties(context);

            long result = repository.Count();

            Assert.Equal(3, result);
        }

        [Fact]
        public async Task Count_WithPredicate_ShouldReturnMatchingRecordCount()
        {
            string databaseName = Guid.NewGuid().ToString();

            await using VotingDbContext context = TestDbContextFactory.Create(databaseName);

            Repository<Faculty> repository = new(context);

            await FacultyTestData.SeedFaculties(context);

            long result = repository.Count(faculty => faculty.Active);

            Assert.Equal(2, result);
        }

        [Fact]
        public async Task CountAsync_WithPredicate_ShouldReturnMatchingRecordCount()
        {
            string databaseName = Guid.NewGuid().ToString();

            await using VotingDbContext context = TestDbContextFactory.Create(databaseName);

            Repository<Faculty> repository = new(context);

            await FacultyTestData.SeedFaculties(context);

            long result = await repository.CountAsync(faculty => faculty.Active);

            Assert.Equal(2, result);
        }

        [Fact]
        public async Task GetAll_ShouldReturnAllRecords()
        {
            string databaseName = Guid.NewGuid().ToString();

            await using VotingDbContext context = TestDbContextFactory.Create(databaseName);

            Repository<Faculty> repository = new(context);

            await FacultyTestData.SeedFaculties(context);

            IEnumerable<Faculty> result = repository.GetAll();

            Assert.Equal(3, result.Count());
        }

        [Fact]
        public async Task GetAll_WithOrderBy_ShouldReturnOrderedRecords()
        {
            string databaseName = Guid.NewGuid().ToString();

            await using VotingDbContext context = TestDbContextFactory.Create(databaseName);

            Repository<Faculty> repository = new(context);

            await FacultyTestData.SeedFaculties(context);

            IEnumerable<Faculty> result = repository.GetAll(query => query.OrderBy(faculty => faculty.Name));

            List<Faculty> faculties = result.ToList();

            Assert.Equal("Arts", faculties[0].Name);
            Assert.Equal("Engineering", faculties[1].Name);
            Assert.Equal("Science", faculties[2].Name);
        }

        [Fact]
        public async Task GetAllAsync_WithOrderBy_ShouldReturnOrderedRecords()
        {
            string databaseName = Guid.NewGuid().ToString();

            await using VotingDbContext context = TestDbContextFactory.Create(databaseName);

            Repository<Faculty> repository = new(context);

            await FacultyTestData.SeedFaculties(context);

            IEnumerable<Faculty> result = await repository.GetAllAsync(query => query.OrderByDescending(faculty => faculty.Name));

            List<Faculty> faculties = result.ToList();

            Assert.Equal("Science", faculties[0].Name);
            Assert.Equal("Engineering", faculties[1].Name);
            Assert.Equal("Arts", faculties[2].Name);
        }

        [Fact]
        public async Task GetBy_WithPredicate_ShouldReturnMatchingRecords()
        {
            string databaseName = Guid.NewGuid().ToString();

            await using VotingDbContext context = TestDbContextFactory.Create(databaseName);

            Repository<Faculty> repository = new(context);

            await FacultyTestData.SeedFaculties(context);

            IEnumerable<Faculty> result = repository.GetBy(faculty => faculty.Active);

            Assert.Equal(2, result.Count());
            Assert.All(result, faculty => Assert.True(faculty.Active));
        }

        [Fact]
        public async Task GetBy_WithSkipAndTake_ShouldReturnRequestedRecords()
        {
            string databaseName = Guid.NewGuid().ToString();

            await using VotingDbContext context = TestDbContextFactory.Create(databaseName);

            Repository<Faculty> repository = new(context);

            await FacultyTestData.SeedFaculties(context);

            IEnumerable<Faculty> result = repository.GetBy(orderBy: query => query.OrderBy(faculty => faculty.Name),
                skip: 1,
                take: 1);

            List<Faculty> faculties = result.ToList();

            Assert.Single(faculties);
            Assert.Equal("Engineering", faculties[0].Name);
        }

        [Fact]
        public async Task GetByAsync_WithPredicate_ShouldReturnMatchingRecords()
        {
            string databaseName = Guid.NewGuid().ToString();

            await using VotingDbContext context = TestDbContextFactory.Create(databaseName);

            Repository<Faculty> repository = new(context);

            await FacultyTestData.SeedFaculties(context);

            IEnumerable<Faculty> result = await repository.GetByAsync(faculty => faculty.Active);

            Assert.Equal(2, result.Count());
            Assert.All(result, faculty => Assert.True(faculty.Active));
        }

        [Fact]
        public async Task GetById_WithExistingId_ShouldReturnRecord()
        {
            string databaseName = Guid.NewGuid().ToString();

            await using VotingDbContext context = TestDbContextFactory.Create(databaseName);

            Repository<Faculty> repository = new(context);

            Faculty faculty = FacultyTestData.CreateFaculty("Engineering");

            context.Faculties.Add(faculty);
            await context.SaveChangesAsync(true);

            Faculty? result = repository.GetById(faculty.Id);

            Assert.NotNull(result);
            Assert.Equal("Engineering", result.Name);
        }

        [Fact]
        public async Task GetById_WithMissingId_ShouldReturnNull()
        {
            string databaseName = Guid.NewGuid().ToString();

            await using VotingDbContext context = TestDbContextFactory.Create(databaseName);

            Repository<Faculty> repository = new(context);

            Faculty? result = repository.GetById(999L);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_WithExistingId_ShouldReturnRecord()
        {
            string databaseName = Guid.NewGuid().ToString();

            await using VotingDbContext context = TestDbContextFactory.Create(databaseName);

            Repository<Faculty> repository = new(context);

            Faculty faculty = FacultyTestData.CreateFaculty("Engineering");

            context.Faculties.Add(faculty);
            await context.SaveChangesAsync(true);

            Faculty? result = await repository.GetByIdAsync(faculty.Id);

            Assert.NotNull(result);
            Assert.Equal("Engineering", result.Name);
        }

        [Fact]
        public async Task GetSingleBy_WithMatchingPredicate_ShouldReturnRecord()
        {
            string databaseName = Guid.NewGuid().ToString();

            await using VotingDbContext context = TestDbContextFactory.Create(databaseName);

            Repository<Faculty> repository = new(context);

            await FacultyTestData.SeedFaculties(context);

            Faculty? result = repository.GetSingleBy(faculty => faculty.Name == "Science");

            Assert.NotNull(result);
            Assert.Equal("Science", result.Name);
        }

        [Fact]
        public async Task GetSingleByAsync_WithMissingPredicate_ShouldReturnNull()
        {
            string databaseName = Guid.NewGuid().ToString();

            await using VotingDbContext context = TestDbContextFactory.Create(databaseName);

            Repository<Faculty> repository = new(context);

            await FacultyTestData.SeedFaculties(context);

            Faculty? result = await repository.GetSingleByAsync(faculty => faculty.Name == "Medicine");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetQueryable_WithPredicateAndPaging_ShouldReturnExpectedRecords()
        {
            string databaseName = Guid.NewGuid().ToString();

            await using VotingDbContext context = TestDbContextFactory.Create(databaseName);

            Repository<Faculty> repository = new(context);

            await FacultyTestData.SeedFaculties(context);

            IQueryable<Faculty> query = repository.GetQueryable(faculty => faculty.Active,
                orderBy: items => items.OrderBy(faculty => faculty.Name),
                skip: 1,
                take: 1);

            List<Faculty> result = await query.ToListAsync();

            Assert.Single(result);
            Assert.Equal("Science", result[0].Name);
        }

        [Fact]
        public async Task GetByAsync_WithTrackingDisabled_ShouldReturnUntrackedEntities()
        {
            string databaseName = Guid.NewGuid().ToString();

            await using VotingDbContext context = TestDbContextFactory.Create(databaseName);

            Repository<Faculty> repository = new(context);

            await FacultyTestData.SeedFaculties(context);

            IEnumerable<Faculty> result = await repository.GetByAsync(faculty => faculty.Active, tracking: false);

            Faculty faculty = result.First();

            Assert.Equal(EntityState.Detached, context.Entry(faculty).State);
        }

        [Fact]
        public async Task GetByAsync_WithTrackingEnabled_ShouldReturnTrackedEntities()
        {
            string databaseName = Guid.NewGuid().ToString();

            await using VotingDbContext context = TestDbContextFactory.Create(databaseName);

            Repository<Faculty> repository = new(context);

            await FacultyTestData.SeedFaculties(context);

            IEnumerable<Faculty> result = await repository.GetByAsync(faculty => faculty.Active, tracking: true);

            Faculty faculty = result.First();

            Assert.Equal(EntityState.Unchanged, context.Entry(faculty).State);
        }

        [Fact]
        public async Task GetSingleByAsync_WithTrackingDisabled_ShouldReturnUntrackedEntity()
        {
            string databaseName = Guid.NewGuid().ToString();

            await using VotingDbContext context = TestDbContextFactory.Create(databaseName);

            Repository<Faculty> repository = new(context);

            await FacultyTestData.SeedFaculties(context);

            Faculty? result = await repository.GetSingleByAsync(faculty => faculty.Name == "Engineering", tracking: false);

            Assert.NotNull(result);
            Assert.Equal(EntityState.Detached, context.Entry(result).State);
        }

        [Fact]
        public async Task GetSingleByAsync_WithTrackingEnabled_ShouldReturnTrackedEntity()
        {
            string databaseName = Guid.NewGuid().ToString();

            await using VotingDbContext context = TestDbContextFactory.Create(databaseName);

            Repository<Faculty> repository = new(context);

            await FacultyTestData.SeedFaculties(context);

            Faculty? result = await repository.GetSingleByAsync(faculty => faculty.Name == "Engineering", tracking: true);

            Assert.NotNull(result);
            Assert.Equal(EntityState.Unchanged, context.Entry(result).State);
        }

        [Fact]
        public async Task Update_ShouldMarkEntityAsModifiedWithoutSaving()
        {
            string databaseName = Guid.NewGuid().ToString();

            await using VotingDbContext context = TestDbContextFactory.Create(databaseName);

            Faculty faculty = FacultyTestData.CreateFaculty("Engineering");
            context.Faculties.Add(faculty);
            await context.SaveChangesAsync(true);

            context.Entry(faculty).State = EntityState.Detached;

            Repository<Faculty> repository = new(context);

            faculty.Name = "Updated Engineering";

            Faculty result = repository.Update(faculty);

            Assert.Same(faculty, result);
            Assert.Equal(EntityState.Modified, context.Entry(faculty).State);

            await using VotingDbContext verificationContext = TestDbContextFactory.Create(databaseName);

            Faculty? savedFaculty = await verificationContext.Faculties.FindAsync(faculty.Id);

            Assert.NotNull(savedFaculty);
            Assert.Equal("Engineering", savedFaculty.Name);
        }

        [Fact]
        public async Task UpdateAsync_WithDefaultTracking_ShouldSaveAndDetachEntity()
        {
            string databaseName = Guid.NewGuid().ToString();

            await using VotingDbContext context = TestDbContextFactory.Create(databaseName);

            Faculty faculty = FacultyTestData.CreateFaculty("Engineering");
            context.Faculties.Add(faculty);
            await context.SaveChangesAsync(true);

            context.Entry(faculty).State = EntityState.Detached;

            Repository<Faculty> repository = new(context);

            faculty.Name = "Updated Engineering";

            Faculty result = await repository.UpdateAsync(faculty);

            Assert.Same(faculty, result);
            Assert.Equal(EntityState.Detached, context.Entry(faculty).State);

            await using VotingDbContext verificationContext = TestDbContextFactory.Create(databaseName);

            Faculty? savedFaculty = await verificationContext.Faculties.FindAsync(faculty.Id);

            Assert.NotNull(savedFaculty);
            Assert.Equal("Updated Engineering", savedFaculty.Name);
        }

        [Fact]
        public async Task UpdateAsync_WithTrackingEnabled_ShouldSaveAndKeepEntityTracked()
        {
            string databaseName = Guid.NewGuid().ToString();

            await using VotingDbContext context = TestDbContextFactory.Create(databaseName);

            Faculty faculty = FacultyTestData.CreateFaculty("Engineering");
            context.Faculties.Add(faculty);
            await context.SaveChangesAsync(true);

            context.Entry(faculty).State = EntityState.Detached;

            Repository<Faculty> repository = new(context);

            faculty.Name = "Updated Engineering";

            Faculty result = await repository.UpdateAsync(faculty, true);

            Assert.Same(faculty, result);
            Assert.Equal(EntityState.Unchanged, context.Entry(faculty).State);

            await using VotingDbContext verificationContext = TestDbContextFactory.Create(databaseName);

            Faculty? savedFaculty = await verificationContext.Faculties.FindAsync(faculty.Id);

            Assert.NotNull(savedFaculty);
            Assert.Equal("Updated Engineering", savedFaculty.Name);
        }

        [Fact]
        public async Task UpdateRange_ShouldMarkEntitiesAsModifiedWithoutSaving()
        {
            string databaseName = Guid.NewGuid().ToString();

            await using VotingDbContext context = TestDbContextFactory.Create(databaseName);

            List<Faculty> faculties = FacultyTestData.CreateFaculties();

            context.Faculties.AddRange(faculties);
            await context.SaveChangesAsync(true);

            foreach (Faculty faculty in faculties)
            {
                context.Entry(faculty).State = EntityState.Detached;
                faculty.Active = false;
            }

            Repository<Faculty> repository = new(context);

            repository.UpdateRange(faculties);

            Assert.All(faculties, faculty => Assert.Equal(EntityState.Modified, context.Entry(faculty).State));

            await using VotingDbContext verificationContext = TestDbContextFactory.Create(databaseName);

            List<Faculty> savedFaculties = await verificationContext.Faculties.ToListAsync();

            Assert.Contains(savedFaculties, faculty => faculty.Active);
        }

        [Fact]
        public async Task UpdateRangeAsync_ShouldUpdateAndSaveEntities()
        {
            string databaseName = Guid.NewGuid().ToString();

            await using VotingDbContext context = TestDbContextFactory.Create(databaseName);

            List<Faculty> faculties = FacultyTestData.CreateFaculties();

            context.Faculties.AddRange(faculties);
            await context.SaveChangesAsync(true);

            foreach (Faculty faculty in faculties)
            {
                context.Entry(faculty).State = EntityState.Detached;
                faculty.Active = false;
            }

            Repository<Faculty> repository = new(context);

            await repository.UpdateRangeAsync(faculties);

            await using VotingDbContext verificationContext = TestDbContextFactory.Create(databaseName);

            List<Faculty> savedFaculties = await verificationContext.Faculties.ToListAsync();

            Assert.Equal(3, savedFaculties.Count);
            Assert.All(savedFaculties, faculty => Assert.False(faculty.Active));
        }

        [Fact]
        public async Task Delete_ShouldMarkEntityAsDeletedWithoutSaving()
        {
            string databaseName = Guid.NewGuid().ToString();

            await using VotingDbContext context = TestDbContextFactory.Create(databaseName);

            Faculty faculty = FacultyTestData.CreateFaculty("Engineering");

            context.Faculties.Add(faculty);
            await context.SaveChangesAsync(true);

            Repository<Faculty> repository = new(context);

            bool result = repository.Delete(faculty);

            Assert.True(result);
            Assert.Equal(EntityState.Deleted, context.Entry(faculty).State);

            await using VotingDbContext verificationContext = TestDbContextFactory.Create(databaseName);

            Faculty? savedFaculty = await verificationContext.Faculties.FindAsync(faculty.Id);

            Assert.NotNull(savedFaculty);
        }

        [Fact]
        public async Task Delete_WithMatchingPredicate_ShouldMarkEntityAsDeletedWithoutSaving()
        {
            string databaseName = Guid.NewGuid().ToString();

            await using VotingDbContext context = TestDbContextFactory.Create(databaseName);

            Repository<Faculty> repository = new(context);

            await FacultyTestData.SeedFaculties(context);

            bool result = repository.Delete(faculty => faculty.Name == "Engineering");

            Assert.True(result);

            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Faculty>? deletedEntry = context.ChangeTracker
                .Entries<Faculty>()
                .FirstOrDefault(entry => entry.Entity.Name == "Engineering" 
                    && entry.State == EntityState.Deleted);

            Assert.NotNull(deletedEntry);

            await using VotingDbContext verificationContext = TestDbContextFactory.Create(databaseName);

            Faculty? savedFaculty = await verificationContext.Faculties.SingleOrDefaultAsync(item => item.Name == "Engineering");

            Assert.NotNull(savedFaculty);
        }

        [Fact]
        public async Task Delete_WithMissingPredicate_ShouldThrowException()
        {
            string databaseName = Guid.NewGuid().ToString();

            await using VotingDbContext context = TestDbContextFactory.Create(databaseName);

            Repository<Faculty> repository = new(context);

            await FacultyTestData.SeedFaculties(context);

            Exception exception = Assert.Throws<Exception>(() => repository.Delete(faculty => faculty.Name == "Medicine"));

            Assert.Equal("object does not exist", exception.Message);
        }

        [Fact]
        public async Task DeleteAsync_WithDefaultTracking_ShouldDeleteSaveAndDetachEntity()
        {
            string databaseName = Guid.NewGuid().ToString();

            await using VotingDbContext context = TestDbContextFactory.Create(databaseName);

            Faculty faculty = FacultyTestData.CreateFaculty("Engineering");

            context.Faculties.Add(faculty);
            await context.SaveChangesAsync(true);

            Repository<Faculty> repository = new(context);

            await repository.DeleteAsync(faculty, false);

            Assert.Equal(EntityState.Detached, context.Entry(faculty).State);

            await using VotingDbContext verificationContext = TestDbContextFactory.Create(databaseName);

            Faculty? savedFaculty = await verificationContext.Faculties.FindAsync(faculty.Id);

            Assert.Null(savedFaculty);
        }

        [Fact]
        public async Task DeleteAsync_WithTrackingEnabled_ShouldDeleteAndSaveEntity()
        {
            string databaseName = Guid.NewGuid().ToString();

            await using VotingDbContext context = TestDbContextFactory.Create(databaseName);

            Faculty faculty = FacultyTestData.CreateFaculty("Engineering");
            context.Faculties.Add(faculty);
            await context.SaveChangesAsync(true);

            Repository<Faculty> repository = new(context);

            await repository.DeleteAsync(faculty, true);

            Assert.Equal(EntityState.Detached, context.Entry(faculty).State);

            await using VotingDbContext verificationContext = TestDbContextFactory.Create(databaseName);

            Faculty? savedFaculty = await verificationContext.Faculties.FindAsync(faculty.Id);

            Assert.Null(savedFaculty);
        }

        [Fact]
        public async Task DeleteAsync_WithMatchingPredicate_ShouldDeleteAndSaveEntity()
        {
            string databaseName = Guid.NewGuid().ToString();

            await using VotingDbContext context = TestDbContextFactory.Create(databaseName);
            Repository<Faculty> repository = new(context);

            await FacultyTestData.SeedFaculties(context);

            await repository.DeleteAsync(faculty => faculty.Name == "Engineering");

            await using VotingDbContext verificationContext = TestDbContextFactory.Create(databaseName);

            Faculty? savedFaculty = await verificationContext.Faculties.SingleOrDefaultAsync(item => item.Name == "Engineering");

            Assert.Null(savedFaculty);
        }

        [Fact]
        public async Task DeleteById_WithExistingId_ShouldMarkEntityAsDeletedWithoutSaving()
        {
            string databaseName = Guid.NewGuid().ToString();

            await using VotingDbContext context = TestDbContextFactory.Create(databaseName);

            Faculty faculty = FacultyTestData.CreateFaculty("Engineering");

            context.Faculties.Add(faculty);
            await context.SaveChangesAsync(true);

            Repository<Faculty> repository = new(context);

            bool result = repository.DeleteById(faculty.Id);

            Assert.True(result);
            Assert.Equal(EntityState.Deleted, context.Entry(faculty).State);

            await using VotingDbContext verificationContext = TestDbContextFactory.Create(databaseName);

            Faculty? savedFaculty = await verificationContext.Faculties.FindAsync(faculty.Id);

            Assert.NotNull(savedFaculty);
        }

        [Fact]
        public async Task DeleteById_WithMissingId_ShouldThrowException()
        {
            string databaseName = Guid.NewGuid().ToString();

            await using VotingDbContext context = TestDbContextFactory.Create(databaseName);
            Repository<Faculty> repository = new(context);

            Exception exception = Assert.Throws<Exception>(() => repository.DeleteById(999L));

            Assert.Equal("object with id 999 does not exist", exception.Message);
        }

        [Fact]
        public async Task DeleteByIdAsync_WithExistingId_ShouldDeleteAndSaveEntity()
        {
            string databaseName = Guid.NewGuid().ToString();

            await using VotingDbContext context = TestDbContextFactory.Create(databaseName);

            Faculty faculty = FacultyTestData.CreateFaculty("Engineering");
            context.Faculties.Add(faculty);
            await context.SaveChangesAsync(true);

            Repository<Faculty> repository = new(context);

            await repository.DeleteByIdAsync(faculty.Id);

            await using VotingDbContext verificationContext = TestDbContextFactory.Create(databaseName);

            Faculty? savedFaculty = await verificationContext.Faculties.FindAsync(faculty.Id);

            Assert.Null(savedFaculty);
        }

        [Fact]
        public async Task DeleteRange_WithRecords_ShouldMarkEntitiesAsDeletedWithoutSaving()
        {
            string databaseName = Guid.NewGuid().ToString();

            await using VotingDbContext context = TestDbContextFactory.Create(databaseName);

            await FacultyTestData.SeedFaculties(context);

            List<Faculty> faculties = await context.Faculties.Where(faculty => faculty.Active).ToListAsync();

            Repository<Faculty> repository = new(context);

            bool result = repository.DeleteRange(faculties);

            Assert.True(result);
            Assert.All(faculties, faculty => Assert.Equal(EntityState.Deleted, context.Entry(faculty).State));

            await using VotingDbContext verificationContext = TestDbContextFactory.Create(databaseName);

            List<Faculty> savedFaculties = await verificationContext.Faculties.ToListAsync();

            Assert.Equal(3, savedFaculties.Count);
        }

        [Fact]
        public async Task DeleteRange_WithPredicate_ShouldMarkMatchingEntitiesAsDeletedWithoutSaving()
        {
            string databaseName = Guid.NewGuid().ToString();

            await using VotingDbContext context = TestDbContextFactory.Create(databaseName);

            Repository<Faculty> repository = new(context);

            await FacultyTestData.SeedFaculties(context);

            bool result = repository.DeleteRange(faculty => faculty.Active);

            Assert.True(result);

            List<Faculty> deletedFaculties = context.ChangeTracker.Entries<Faculty>()
                .Where(entry => entry.State == EntityState.Deleted)
                .Select(entry => entry.Entity)
                .ToList();

            Assert.Equal(2, deletedFaculties.Count);

            await using VotingDbContext verificationContext = TestDbContextFactory.Create(databaseName);

            List<Faculty> savedFaculties = await verificationContext.Faculties.ToListAsync();

            Assert.Equal(3, savedFaculties.Count);
        }

        [Fact]
        public async Task DeleteRangeAsync_WithRecords_ShouldDeleteAndSaveEntities()
        {
            string databaseName = Guid.NewGuid().ToString();

            await using VotingDbContext context = TestDbContextFactory.Create(databaseName);

            await FacultyTestData.SeedFaculties(context);

            List<Faculty> faculties = await context.Faculties.Where(faculty => faculty.Active).ToListAsync();

            Repository<Faculty> repository = new(context);

            await repository.DeleteRangeAsync(faculties);

            await using VotingDbContext verificationContext = TestDbContextFactory.Create(databaseName);

            List<Faculty> savedFaculties = await verificationContext.Faculties.ToListAsync();

            Assert.Single(savedFaculties);
            Assert.Equal("Arts", savedFaculties[0].Name);
        }

        [Fact]
        public async Task DeleteRangeAsync_WithPredicate_ShouldDeleteAndSaveMatchingEntities()
        {
            string databaseName = Guid.NewGuid().ToString();

            await using VotingDbContext context = TestDbContextFactory.Create(databaseName);

            Repository<Faculty> repository = new(context);

            await FacultyTestData.SeedFaculties(context);

            await repository.DeleteRangeAsync(faculty => faculty.Active);

            await using VotingDbContext verificationContext = TestDbContextFactory.Create(databaseName);

            List<Faculty> savedFaculties = await verificationContext.Faculties.ToListAsync();

            Assert.Single(savedFaculties);
            Assert.Equal("Arts", savedFaculties[0].Name);
        }

        [Fact]
        public async Task GetPagedItems_FirstPage_ShouldReturnRequestedItemsAndMetadata()
        {
            string databaseName = Guid.NewGuid().ToString();

            await using VotingDbContext context = TestDbContextFactory.Create(databaseName);

            Repository<Faculty> repository = new(context);

            await FacultyTestData.SeedFaculties(context);

            TestRequestParameters parameters = new()
            {
                PageNumber = 1,
                PageSize = 2,
                OrderBy = "Name"
            };

            PagedList<Faculty> result = await repository.GetPagedItems(parameters);

            Assert.Equal(2, result.Count);
            Assert.Equal("Arts", result[0].Name);
            Assert.Equal("Engineering", result[1].Name);
            Assert.Equal(3, result.MetaData.TotalCount);
            Assert.Equal(2, result.MetaData.TotalPages);
            Assert.Equal(1, result.MetaData.CurrentPage);
            Assert.Equal(2, result.MetaData.PageSize);
            Assert.False(result.MetaData.HasPrevious);
            Assert.True(result.MetaData.HasNext);
        }

        [Fact]
        public async Task GetPagedItems_SecondPage_ShouldReturnRemainingItems()
        {
            string databaseName = Guid.NewGuid().ToString();

            await using VotingDbContext context = TestDbContextFactory.Create(databaseName);

            Repository<Faculty> repository = new(context);

            await FacultyTestData.SeedFaculties(context);

            TestRequestParameters parameters = new()
            {
                PageNumber = 2,
                PageSize = 2,
                OrderBy = "Name"
            };

            PagedList<Faculty> result = await repository.GetPagedItems(parameters);

            Assert.Single(result);
            Assert.Equal("Science", result[0].Name);
            Assert.Equal(3, result.MetaData.TotalCount);
            Assert.False(result.MetaData.HasNext);
            Assert.True(result.MetaData.HasPrevious);
        }

        [Fact]
        public async Task GetPagedItems_WithPredicate_ShouldReturnFilteredItemsAndCorrectCount()
        {
            string databaseName = Guid.NewGuid().ToString();

            await using VotingDbContext context = TestDbContextFactory.Create(databaseName);

            Repository<Faculty> repository = new(context);

            await FacultyTestData.SeedFaculties(context);

            TestRequestParameters parameters = new()
            {
                PageNumber = 1,
                PageSize = 10,
                OrderBy = "Name"
            };

            PagedList<Faculty> result = await repository.GetPagedItems(parameters, faculty => faculty.Active);

            Assert.Equal(2, result.Count);
            Assert.Equal(2, result.MetaData.TotalCount);
            Assert.Equal("Engineering", result[0].Name);
            Assert.Equal("Science", result[1].Name);
        }

        [Fact]
        public async Task LastAsync_WithOrderBy_ShouldReturnLastOrderedRecord()
        {
            string databaseName = Guid.NewGuid().ToString();

            await using VotingDbContext context = TestDbContextFactory.Create(databaseName);

            Repository<Faculty> repository = new(context);

            await FacultyTestData.SeedFaculties(context);

            Faculty? result = await repository.LastAsync(orderBy: query => query.OrderBy(faculty => faculty.Name));

            Assert.NotNull(result);
            Assert.Equal("Science", result.Name);
        }

        [Fact]
        public async Task LastAsync_WithPredicate_ShouldReturnLastMatchingRecord()
        {
            string databaseName = Guid.NewGuid().ToString();

            await using VotingDbContext context = TestDbContextFactory.Create(databaseName);

            Repository<Faculty> repository = new(context);

            await FacultyTestData.SeedFaculties(context);

            Faculty? result = await repository.LastAsync(
                faculty => faculty.Active,
                query => query.OrderBy(faculty => faculty.Name));

            Assert.NotNull(result);
            Assert.Equal("Science", result.Name);
        }

        [Fact]
        public async Task LastAsync_WithNoMatchingRecord_ShouldReturnNull()
        {
            string databaseName = Guid.NewGuid().ToString();

            await using VotingDbContext context = TestDbContextFactory.Create(databaseName);

            Repository<Faculty> repository = new(context);

            await FacultyTestData.SeedFaculties(context);

            Faculty? result = await repository.LastAsync(
                faculty => faculty.Name == "Medicine");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetAll_WithInclude_ShouldReturnRelatedDepartments()
        {
            string databaseName = Guid.NewGuid().ToString();

            await using VotingDbContext context = TestDbContextFactory.Create(databaseName);

            Repository<Faculty> repository = new(context);

            await FacultyTestData.SeedFacultyWithDepartments(context);

            context.ChangeTracker.Clear();

            IEnumerable<Faculty> result = repository.GetAll(includeProperties: nameof(Faculty.Departments));

            Faculty faculty = Assert.Single(result);

            Assert.Equal(2, faculty.Departments.Count);
        }

        [Fact]
        public async Task GetAllAsync_WithInclude_ShouldReturnRelatedDepartments()
        {
            string databaseName = Guid.NewGuid().ToString();

            await using VotingDbContext context = TestDbContextFactory.Create(databaseName);

            Repository<Faculty> repository = new(context);

            await FacultyTestData.SeedFacultyWithDepartments(context);

            context.ChangeTracker.Clear();

            IEnumerable<Faculty> result = await repository.GetAllAsync(include: query => query.Include(faculty => faculty.Departments));

            Faculty faculty = Assert.Single(result);

            Assert.Equal(2, faculty.Departments.Count);
        }

        [Fact]
        public async Task GetByAsSplitQueryAsync_ShouldReturnMatchingRecords()
        {
            string databaseName = Guid.NewGuid().ToString();

            await using VotingDbContext context = TestDbContextFactory.Create(databaseName);

            Repository<Faculty> repository = new(context);

            await FacultyTestData.SeedFaculties(context);

            IEnumerable<Faculty> result = await repository.GetByAsSplitQueryAsync(faculty => faculty.Active);

            Assert.Equal(2, result.Count());
            Assert.All(result, faculty => Assert.True(faculty.Active));
        }

        [Fact]
        public async Task GetSingleByAsSplitQueryAsync_WithMatchingPredicate_ShouldReturnRecord()
        {
            string databaseName = Guid.NewGuid().ToString();

            await using VotingDbContext context = TestDbContextFactory.Create(databaseName);

            Repository<Faculty> repository = new(context);

            await FacultyTestData.SeedFaculties(context);

            Faculty? result = await repository.GetSingleByAsSplitQueryAsync(faculty => faculty.Name == "Engineering");

            Assert.NotNull(result);
            Assert.Equal("Engineering", result.Name);
        }

        [Fact]
        public async Task GetSingleByAsSplitQueryAsync_WithMissingPredicate_ShouldReturnNull()
        {
            string databaseName = Guid.NewGuid().ToString();

            await using VotingDbContext context = TestDbContextFactory.Create(databaseName);
            Repository<Faculty> repository = new(context);

            await FacultyTestData.SeedFaculties(context);

            Faculty? result = await repository.GetSingleByAsSplitQueryAsync(faculty => faculty.Name == "Medicine");

            Assert.Null(result);
        }

        [Fact]
        public async Task SumAsync_LongProperty_ShouldReturnTotal()
        {
            string databaseName = Guid.NewGuid().ToString();

            await using VotingDbContext context = TestDbContextFactory.Create(databaseName);

            Repository<Faculty> repository = new(context);

            await FacultyTestData.SeedFaculties(context);

            long expected = await context.Faculties.SumAsync(faculty => faculty.Id);

            long result = await repository.SumAsync(faculty => faculty.Id);

            Assert.Equal(expected, result);
        }

        private sealed class TestRequestParameters : RequestParameters
        {
        }
    }
}