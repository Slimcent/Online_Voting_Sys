namespace OnlineVoting.Models.Results
{
    public sealed class Result<T>
    {
        private Result(ResultStatus status, T? value, string? error)
        {
            Status = status;
            Value = value;
            Error = error;
        }

        public ResultStatus Status { get; }

        public T? Value { get; }

        public string? Error { get; }

        public bool IsSuccess => Status is ResultStatus.Success
            or ResultStatus.Created
            or ResultStatus.NoContent;

        public static Result<T> Success(T value)
        {
            return new Result<T>(ResultStatus.Success, value, null);
        }

        public static Result<T> Created(T value)
        {
            return new Result<T>(ResultStatus.Created, value, null);
        }

        public static Result<T> NoContent()
        {
            return new Result<T>(ResultStatus.NoContent, default, null);
        }

        public static Result<T> ValidationError(string error)
        {
            return new Result<T>(ResultStatus.ValidationError, default, error);
        }

        public static Result<T> NotFound(string error)
        {
            return new Result<T>(ResultStatus.NotFound, default, error);
        }

        public static Result<T> Conflict(string error)
        {
            return new Result<T>(ResultStatus.Conflict, default, error);
        }

        public static Result<T> Unauthorized(string error)
        {
            return new Result<T>(ResultStatus.Unauthorized, default, error);
        }

        public static Result<T> Forbidden(string error)
        {
            return new Result<T>(ResultStatus.Forbidden, default, error);
        }

        public static Result<T> FromFailure<TFailure>(Result<TFailure> result)
        {
            if (result.IsSuccess)
                throw new InvalidOperationException("A successful result cannot be converted to a failure result.");

            return new Result<T>(result.Status, default, result.Error);
        }
    }
}