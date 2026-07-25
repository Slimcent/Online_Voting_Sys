namespace OnlineVoting.Api.Documentation.Models
{
    public sealed class ApiResponseDocumentation
    {
        public required string Description { get; init; }

        public Type? ResponseType { get; init; }
    }
}