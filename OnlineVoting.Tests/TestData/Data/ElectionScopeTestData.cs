using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Entities.OnlineVoting.Models.Entities;

namespace OnlineVoting.Tests.TestData.Data
{
    public static class ElectionScopeTestData
    {
        public static ElectionScope CreateElectionScope(int id = 2, string code = "FACULTY",
            string name = "Faculty", string? description = "Applies to elections conducted within a faculty.",
            bool active = true)
        {
            return new ElectionScope
            {
                Id = id,
                Code = code,
                Name = name,
                Description = description,
                Active = active
            };
        }

        public static UpdateElectionScopeRequest CreateUpdateElectionScopeRequest(int id = 2,
            string name = "Faculty Level",
            string? description = "Applies to elections conducted within an academic faculty.")
        {
            return new UpdateElectionScopeRequest
            {
                Id = id,
                Name = name,
                Description = description
            };
        }
    }
}