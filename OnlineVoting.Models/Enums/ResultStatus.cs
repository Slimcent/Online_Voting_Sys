namespace OnlineVoting.Models.Results
{
    public enum ResultStatus
    {
        Success,
        Created,
        NoContent,
        ValidationError,
        NotFound,
        Conflict,
        Unauthorized,
        Forbidden
    }
}