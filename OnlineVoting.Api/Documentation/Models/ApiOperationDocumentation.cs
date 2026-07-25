namespace OnlineVoting.Api.Documentation.Models
{
    public sealed class ApiOperationDocumentation
    {
        public required string Summary { get; init; }

        public required string Description { get; init; }

        public IReadOnlyDictionary<string, ApiResponseDocumentation> Responses { get; init; }
            = new Dictionary<string, ApiResponseDocumentation>();
    }
}