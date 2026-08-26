using OnlineVoting.Models.Pagination;

namespace OnlineVoting.Tests.UnitTests.Models.Pagination
{
    public class MetaDataTests
    {
        [Theory]
        [InlineData(1, false)]
        [InlineData(2, true)]
        [InlineData(5, true)]
        public void HasPrevious_ShouldReturnExpectedResult(int currentPage, bool expected)
        {
            MetaData metaData = new()
            {
                CurrentPage = currentPage
            };

            Assert.Equal(expected, metaData.HasPrevious);
        }

        [Theory]
        [InlineData(1, 3, true)]
        [InlineData(2, 3, true)]
        [InlineData(3, 3, false)]
        public void HasNext_ShouldReturnExpectedResult(int currentPage, int totalPages, bool expected)
        {
            MetaData metaData = new()
            {
                CurrentPage = currentPage,
                TotalPages = totalPages
            };

            Assert.Equal(expected, metaData.HasNext);
        }
    }
}