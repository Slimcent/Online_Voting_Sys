using Microsoft.EntityFrameworkCore;
using OnlineVoting.Models.Context;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Pagination;
using OnlineVoting.Tests.TestData.Data;
using OnlineVoting.Tests.TestData.Factories;
using VotingSystem.Data.Extensions;

namespace OnlineVoting.Tests.UnitTests.Data.Extensions
{
    public class RepositoryExtensionsTests
    {
        [Fact]
        public async Task Sort_Ascending_ShouldReturnItemsInAscendingOrder()
        {
            using AuditDbContextFactory factory = new();

            VotingDbContext context = factory.Context;

            await FacultyTestData.SeedFaculties(context);

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
            using AuditDbContextFactory factory = new();

            VotingDbContext context = factory.Context;

            await FacultyTestData.SeedFaculties(context);

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
            using AuditDbContextFactory factory = new();

            VotingDbContext context = factory.Context;

            await FacultyTestData.SeedFaculties(context);

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
            using AuditDbContextFactory factory = new();

            VotingDbContext context = factory.Context;

            await FacultyTestData.SeedFaculties(context);

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
            using AuditDbContextFactory factory = new();

            VotingDbContext context = factory.Context;

            await FacultyTestData.SeedFaculties(context);

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
            using AuditDbContextFactory factory = new();

            VotingDbContext context = factory.Context;

            await FacultyTestData.SeedFaculties(context);

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
            using AuditDbContextFactory factory = new();

            VotingDbContext context = factory.Context;

            await FacultyTestData.SeedFaculties(context);

            TestRequestParameters parameters = new()
            {
                PageNumber = 1,
                PageSize = 10,
                OrderBy = "Name"
            };

            PagedList<Faculty> result = await context.Faculties.GetPagedItems(
                parameters,
                faculty => faculty.Active);

            Assert.Equal(2, result.Count);
            Assert.Equal(2, result.MetaData.TotalCount);
            Assert.All(result, faculty => Assert.True(faculty.Active));
        }

        [Fact]
        public async Task GetPagedItems_WithSearchAndSorting_ShouldApplyBoth()
        {
            using AuditDbContextFactory factory = new();

            VotingDbContext context = factory.Context;

            await FacultyTestData.SeedFaculties(context);

            TestRequestParameters parameters = new()
            {
                PageNumber = 1,
                PageSize = 10,
                OrderBy = "Name desc"
            };

            PagedList<Faculty> result = await context.Faculties.GetPagedItems(parameters, faculty => faculty.Active);

            Assert.Equal(2, result.Count);
            Assert.Equal("Science", result[0].Name);
            Assert.Equal("Engineering", result[1].Name);
        }

        [Fact]
        public async Task GetPagedItems_NoMatchingItems_ShouldReturnEmptyPage()
        {
            using AuditDbContextFactory factory = new();

            VotingDbContext context = factory.Context;

            await FacultyTestData.SeedFaculties(context);

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

        private sealed class TestRequestParameters : RequestParameters
        {
        }
    }
}