using OnlineVoting.Models.Results;

namespace OnlineVoting.Tests.UnitTests.Models.GlobalMessage
{
    public class ResultTests
    {
        [Fact]
        public void Success_ShouldCreateSuccessfulResult()
        {
            Result<string> result = Result<string>.Success("value");

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.Equal("value", result.Value);
            Assert.Null(result.Error);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void Created_ShouldCreateSuccessfulResult()
        {
            Result<string> result = Result<string>.Created("value");

            Assert.Equal(ResultStatus.Created, result.Status);
            Assert.Equal("value", result.Value);
            Assert.Null(result.Error);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void NoContent_ShouldCreateSuccessfulResultWithoutValue()
        {
            Result<string> result = Result<string>.NoContent();

            Assert.Equal(ResultStatus.NoContent, result.Status);
            Assert.Null(result.Value);
            Assert.Null(result.Error);
            Assert.True(result.IsSuccess);
        }

        [Theory]
        [InlineData(ResultStatus.ValidationError)]
        [InlineData(ResultStatus.NotFound)]
        [InlineData(ResultStatus.Conflict)]
        [InlineData(ResultStatus.Unauthorized)]
        [InlineData(ResultStatus.Forbidden)]
        public void FailureResult_ShouldNotBeSuccessful(ResultStatus status)
        {
            Result<string> result = status switch
            {
                ResultStatus.ValidationError => Result<string>.ValidationError("error"),
                ResultStatus.NotFound => Result<string>.NotFound("error"),
                ResultStatus.Conflict => Result<string>.Conflict("error"),
                ResultStatus.Unauthorized => Result<string>.Unauthorized("error"),
                ResultStatus.Forbidden => Result<string>.Forbidden("error"),
                _ => throw new ArgumentOutOfRangeException(nameof(status))
            };

            Assert.Equal(status, result.Status);
            Assert.Null(result.Value);
            Assert.Equal("error", result.Error);
            Assert.False(result.IsSuccess);
        }

        [Fact]
        public void FromFailure_ShouldConvertFailureResult()
        {
            Result<int> source = Result<int>.NotFound("Item not found.");

            Result<string> result = Result<string>.FromFailure(source);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Null(result.Value);
            Assert.Equal("Item not found.", result.Error);
            Assert.False(result.IsSuccess);
        }

        [Fact]
        public void FromFailure_WithSuccessfulResult_ShouldThrowInvalidOperationException()
        {
            Result<int> source = Result<int>.Success(1);

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
                Result<string>.FromFailure(source));

            Assert.Equal("A successful result cannot be converted to a failure result.", exception.Message);
        }
    }
}