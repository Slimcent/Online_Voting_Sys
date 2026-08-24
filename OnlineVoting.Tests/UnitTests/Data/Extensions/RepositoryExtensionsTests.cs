using Microsoft.EntityFrameworkCore;
using OnlineVoting.Models.Context;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Interfaces;
using OnlineVoting.Models.Pagination;
using VotingSystem.Data.Extensions;

namespace OnlineVoting.Tests.UnitTests.Data.Extensions
{
    public class RepositoryExtensionsTests
    {
        [Fact]
        public async Task Sort_Ascending_ShouldReturnItemsInAscendingOrder()
        {
            await using VotingDbContext context = CreateContext();

            await SeedFaculties(context);

            List<Faculty> faculties = await context.Faculties
                .Sort("Name")
                .ToListAsync();

            Assert.Equal("Arts", faculties[0].Name);
            Assert.Equal("Engineering", faculties[1].Name);
            Assert.Equal("Science", faculties[2].Name);
        }

        [Fact]
        public async Task Sort_Descending_ShouldReturnItemsInDescendingOrder()
        {
            await using VotingDbContext context = CreateContext();

            await SeedFaculties(context);

            List<Faculty> faculties = await context.Faculties
                .Sort("Name desc")
                .ToListAsync();

            Assert.Equal("Science", faculties[0].Name);
            Assert.Equal("Engineering", faculties[1].Name);
            Assert.Equal("Arts", faculties[2].Name);
        }

        [Fact]
        public async Task Sort_EmptyOrderBy_ShouldReturnOriginalOrder()
        {
            await using VotingDbContext context = CreateContext();

            await SeedFaculties(context);

            List<Faculty> faculties = await context.Faculties
                .Sort(string.Empty)
                .ToListAsync();

            Assert.Equal("Engineering", faculties[0].Name);
            Assert.Equal("Arts", faculties[1].Name);
            Assert.Equal("Science", faculties[2].Name);
        }

        [Fact]
        public async Task Sort_InvalidProperty_ShouldReturnOriginalOrder()
        {
            await using VotingDbContext context = CreateContext();

            await SeedFaculties(context);

            List<Faculty> faculties = await context.Faculties
                .Sort("InvalidProperty")
                .ToListAsync();

            Assert.Equal("Engineering", faculties[0].Name);
            Assert.Equal("Arts", faculties[1].Name);
            Assert.Equal("Science", faculties[2].Name);
        }

        [Fact]
        public async Task GetPagedItems_FirstPage_ShouldReturnRequestedItemsAndMetadata()
        {
            await using VotingDbContext context = CreateContext();

            await SeedFaculties(context);

            TestRequestParameters parameters = new()
            {
                PageNumber = 1,
                PageSize = 2,
                OrderBy = "Name"
            };

            PagedList<Faculty> result = await context.Faculties.GetPagedItems(parameters);

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
            await using VotingDbContext context = CreateContext();

            await SeedFaculties(context);

            TestRequestParameters parameters = new()
            {
                PageNumber = 2,
                PageSize = 2,
                OrderBy = "Name"
            };

            PagedList<Faculty> result = await context.Faculties.GetPagedItems(parameters);

            Assert.Single(result);
            Assert.Equal("Science", result[0].Name);
            Assert.False(result.MetaData.HasNext);
            Assert.True(result.MetaData.HasPrevious);
        }

        [Fact]
        public async Task GetPagedItems_WithSearchExpression_ShouldReturnFilteredItems()
        {
            await using VotingDbContext context = CreateContext();

            await SeedFaculties(context);

            TestRequestParameters parameters = new()
            {
                PageNumber = 1,
                PageSize = 10,
                OrderBy = "Name"
            };

            PagedList<Faculty> result = await context.Faculties.GetPagedItems(
                parameters,
                faculty => faculty.Activated);

            Assert.Equal(2, result.Count);
            Assert.Equal(2, result.MetaData.TotalCount);
            Assert.All(result, faculty => Assert.True(faculty.Activated));
        }

        [Fact]
        public async Task GetPagedItems_WithSearchAndSorting_ShouldApplyBoth()
        {
            await using VotingDbContext context = CreateContext();

            await SeedFaculties(context);

            TestRequestParameters parameters = new()
            {
                PageNumber = 1,
                PageSize = 10,
                OrderBy = "Name desc"
            };

            PagedList<Faculty> result = await context.Faculties.GetPagedItems(parameters, faculty => faculty.Activated);

            Assert.Equal(2, result.Count);
            Assert.Equal("Science", result[0].Name);
            Assert.Equal("Engineering", result[1].Name);
        }

        [Fact]
        public async Task GetPagedItems_NoMatchingItems_ShouldReturnEmptyPage()
        {
            await using VotingDbContext context = CreateContext();

            await SeedFaculties(context);

            TestRequestParameters parameters = new()
            {
                PageNumber = 1,
                PageSize = 10
            };

            PagedList<Faculty> result = await context.Faculties.GetPagedItems(parameters, faculty => faculty.Name == "Medicine");

            Assert.Empty(result);
            Assert.Equal(0, result.MetaData.TotalCount);
            Assert.Equal(0, result.MetaData.TotalPages);
        }

        private static VotingDbContext CreateContext()
        {
            DbContextOptions<VotingDbContext> options = new DbContextOptionsBuilder<VotingDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new VotingDbContext(options, new TestCurrentUserContext());
        }

        private static async Task SeedFaculties(VotingDbContext context)
        {
            List<Faculty> faculties = new()
            {
                new Faculty
                {
                    Name = "Engineering",
                    Activated = true
                },
                new Faculty
                {
                    Name = "Arts",
                    Activated = false
                },
                new Faculty
                {
                    Name = "Science",
                    Activated = true
                }
            };

            await context.Faculties.AddRangeAsync(faculties);
            await context.SaveChangesAsync(true);
        }

        private sealed class TestRequestParameters : RequestParameters
        {
        }

        private sealed class TestCurrentUserContext : ICurrentUserContext
        {
            public string? Username => "testuser";
        }
    }
}