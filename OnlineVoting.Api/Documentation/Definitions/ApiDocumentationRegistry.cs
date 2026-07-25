using OnlineVoting.Api.Documentation.Definitions.EndpointDefinitions;
using OnlineVoting.Api.Documentation.Models;

namespace OnlineVoting.Api.Documentation.Definitions
{
    public static class ApiDocumentationRegistry
    {
        public static readonly IReadOnlyDictionary<string, ApiOperationDocumentation> Operations = BuildOperations();

        private static IReadOnlyDictionary<string, ApiOperationDocumentation> BuildOperations()
        {
            Dictionary<string, ApiOperationDocumentation> operations = new();

            AddOperations(operations, AuthDocumentation.Operations);

            AddOperations(operations, StudentDocumentation.Operations);

            AddOperations(operations, StaffDocumentation.Operations);

            AddOperations(operations, RoleDocumentation.Operations);

            AddOperations(operations, PositionDocumentation.Operations);

            AddOperations(operations, FacultyDocumentation.Operations);

            AddOperations(operations, DepartmentDocumentation.Operations);

            AddOperations(operations, ClaimsDocumentation.Operations);

            return operations;
        }

        private static void AddOperations(IDictionary<string, ApiOperationDocumentation> target, IReadOnlyDictionary<string, ApiOperationDocumentation> source)
        {
            foreach (KeyValuePair<string, ApiOperationDocumentation> operation in source)
            {
                target.Add(operation.Key, operation.Value);
            }
        }
    }
}