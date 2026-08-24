using VotingSystem.Data.Extensions;

namespace OnlineVoting.Tests.UnitTests.Data.Extensions
{
    public class OrderQueryBuilderTests
    {
        [Fact]
        public Task CreateOrderQuery_WithProperty_ShouldReturnAscendingOrder()
        {
            string result = OrderQueryBuilder.CreateOrderQuery<TestModel>("Name");

            Assert.Equal("Name ascending", result);

            return Task.CompletedTask;
        }

        [Fact]
        public Task CreateOrderQuery_WithDescendingProperty_ShouldReturnDescendingOrder()
        {
            string result = OrderQueryBuilder.CreateOrderQuery<TestModel>("Name desc");

            Assert.Equal("Name descending", result);

            return Task.CompletedTask;
        }

        [Theory]
        [InlineData("Name DESC")]
        [InlineData("Name Desc")]
        [InlineData("name desc")]
        public Task CreateOrderQuery_WithDifferentCasing_ShouldReturnDescendingOrder(string orderBy)
        {
            string result = OrderQueryBuilder.CreateOrderQuery<TestModel>(orderBy);

            Assert.Equal("Name descending", result);

            return Task.CompletedTask;
        }

        [Fact]
        public Task CreateOrderQuery_WithMultipleProperties_ShouldReturnMultipleOrders()
        {
            string result = OrderQueryBuilder.CreateOrderQuery<TestModel>("Name, Age desc");

            Assert.Equal("Name ascending, Age descending", result);

            return Task.CompletedTask;
        }

        [Fact]
        public Task CreateOrderQuery_WithInvalidProperty_ShouldIgnoreProperty()
        {
            string result = OrderQueryBuilder.CreateOrderQuery<TestModel>("InvalidProperty");

            Assert.Empty(result);

            return Task.CompletedTask;
        }

        [Fact]
        public Task CreateOrderQuery_WithValidAndInvalidProperties_ShouldIgnoreInvalidProperty()
        {
            string result = OrderQueryBuilder.CreateOrderQuery<TestModel>("InvalidProperty, Name desc");

            Assert.Equal("Name descending", result);

            return Task.CompletedTask;
        }

        [Fact]
        public Task CreateOrderQuery_WithEmptyParameters_ShouldIgnoreEmptyParameters()
        {
            string result = OrderQueryBuilder.CreateOrderQuery<TestModel>("Name, , Age desc");

            Assert.Equal("Name ascending, Age descending", result);

            return Task.CompletedTask;
        }

        [Fact]
        public Task CreateOrderQuery_WithExtraSpaces_ShouldReturnCorrectOrder()
        {
            string result = OrderQueryBuilder.CreateOrderQuery<TestModel>("  Name desc  ,  Age  ");

            Assert.Equal("Name descending, Age ascending", result);

            return Task.CompletedTask;
        }

        private class TestModel
        {
            public string Name { get; set; } = string.Empty;

            public int Age { get; set; }
        }
    }
}