using OnlineVoting.Models.Pagination;

namespace OnlineVoting.Tests.UnitTests.Models.Pagination
{
    public class RequestParametersTests
    {
        [Fact]
        public void Constructor_ShouldUseDefaultPaginationValues()
        {
            TestRequestParameters parameters = new();

            Assert.Equal(1, parameters.PageNumber);
            Assert.Equal(10, parameters.PageSize);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void PageNumber_LessThanOne_ShouldDefaultToOne(int pageNumber)
        {
            TestRequestParameters parameters = new()
            {
                PageNumber = pageNumber
            };

            Assert.Equal(1, parameters.PageNumber);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        public void PageSize_WithinAllowedRange_ShouldKeepRequestedValue(int pageSize)
        {
            TestRequestParameters parameters = new()
            {
                PageSize = pageSize
            };

            Assert.Equal(pageSize, parameters.PageSize);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void PageSize_LessThanOne_ShouldDefaultToOne(int pageSize)
        {
            TestRequestParameters parameters = new()
            {
                PageSize = pageSize
            };

            Assert.Equal(1, parameters.PageSize);
        }

        [Theory]
        [InlineData(101)]
        [InlineData(100)]
        [InlineData(1000)]
        public void PageSize_AboveMaximum_ShouldBeLimitedToHundred(int pageSize)
        {
            TestRequestParameters parameters = new()
            {
                PageSize = pageSize
            };

            Assert.Equal(100, parameters.PageSize);
        }

        private sealed class TestRequestParameters : RequestParameters
        {
        }
    }
}