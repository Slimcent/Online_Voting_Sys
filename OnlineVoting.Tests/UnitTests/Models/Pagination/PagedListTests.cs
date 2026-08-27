using OnlineVoting.Models.Pagination;

namespace OnlineVoting.Tests.UnitTests.Models.Pagination
{
    public class PagedListTests
    {
        [Fact]
        public void Constructor_ValidArguments_ShouldSetItemsAndMetaData()
        {
            List<int> items = new() { 1, 2, 3 };

            PagedList<int> result = new(items, 12, 2, 5);

            Assert.Equal(items, result);
            Assert.Equal(12, result.MetaData.TotalCount);
            Assert.Equal(5, result.MetaData.PageSize);
            Assert.Equal(2, result.MetaData.CurrentPage);
            Assert.Equal(3, result.MetaData.TotalPages);
        }

        [Fact]
        public void ToPagedList_FirstPage_ShouldReturnFirstPageItems()
        {
            IEnumerable<int> source = Enumerable.Range(1, 10);

            PagedList<int> result = PagedList<int>.ToPagedList(source, 1, 3);

            Assert.Equal(new[] { 1, 2, 3 }, result);
            Assert.Equal(10, result.MetaData.TotalCount);
            Assert.Equal(4, result.MetaData.TotalPages);
            Assert.Equal(1, result.MetaData.CurrentPage);
            Assert.Equal(3, result.MetaData.PageSize);
        }

        [Fact]
        public void ToPagedList_MiddlePage_ShouldReturnRequestedPageItems()
        {
            IEnumerable<int> source = Enumerable.Range(1, 10);

            PagedList<int> result = PagedList<int>.ToPagedList(source, 2, 3);

            Assert.Equal(new[] { 4, 5, 6 }, result);
        }

        [Fact]
        public void ToPagedList_LastPage_ShouldReturnRemainingItems()
        {
            IEnumerable<int> source = Enumerable.Range(1, 10);

            PagedList<int> result = PagedList<int>.ToPagedList(source, 4, 3);

            Assert.Single(result);
            Assert.Equal(10, result[0]);
            Assert.False(result.MetaData.HasNext);
            Assert.True(result.MetaData.HasPrevious);
        }

        [Fact]
        public void ToPagedList_EmptySource_ShouldReturnEmptyPage()
        {
            IEnumerable<int> source = Enumerable.Empty<int>();

            PagedList<int> result = PagedList<int>.ToPagedList(source, 1, 10);

            Assert.Empty(result);
            Assert.Equal(0, result.MetaData.TotalCount);
            Assert.Equal(0, result.MetaData.TotalPages);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ToPagedList_InvalidPageNumber_ShouldThrowArgumentOutOfRangeException(int pageNumber)
        {
            IEnumerable<int> source = Enumerable.Range(1, 10);

            ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
                PagedList<int>.ToPagedList(source, pageNumber, 10));

            Assert.Equal("pageNumber", exception.ParamName);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ToPagedList_InvalidPageSize_ShouldThrowArgumentOutOfRangeException(int pageSize)
        {
            IEnumerable<int> source = Enumerable.Range(1, 10);

            ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
                PagedList<int>.ToPagedList(source, 1, pageSize));

            Assert.Equal("pageSize", exception.ParamName);
        }

        [Fact]
        public void ToPagedList_NullSource_ShouldThrowArgumentNullException()
        {
            IEnumerable<int>? source = null;

            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() =>
                PagedList<int>.ToPagedList(source!, 1, 10));

            Assert.Equal("source", exception.ParamName);
        }
    }
}