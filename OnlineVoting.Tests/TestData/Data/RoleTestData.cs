using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Entities;

namespace OnlineVoting.Tests.TestData.Data
{
    public static class RoleTestData
    {
        public static Role CreateRole(string name = "Admin", bool active = true)
        {
            return new Role
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                Active = active
            };
        }

        public static User CreateUser(string email = "user@example.com")
        {
            return new User
            {
                Id = Guid.NewGuid().ToString(),
                Email = email,
                UserName = email
            };
        }

        public static CreateRoleRequest CreateRoleRequest(string name = "Admin")
        {
            return new CreateRoleRequest
            {
                Name = name
            };
        }

        public static AddUserToRoleRequest CreateAddUserToRoleRequest(string email = "user@example.com", string name = "Admin")
        {
            return new AddUserToRoleRequest
            {
                Email = email,
                Name = name
            };
        }

        public static RoleResponse CreateRoleResponse(string? id = null, string name = "Admin", bool active = true)
        {
            return new RoleResponse
            {
                Id = id ?? Guid.NewGuid().ToString(),
                Name = name,
                IsActive = active
            };
        }
    }
}