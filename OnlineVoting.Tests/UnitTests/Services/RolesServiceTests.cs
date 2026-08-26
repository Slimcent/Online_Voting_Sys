using Microsoft.AspNetCore.Identity;
using Moq;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Results;
using OnlineVoting.Tests.TestData.Data;
using OnlineVoting.Tests.TestData.Factories;

namespace OnlineVoting.Tests.UnitTests.Services
{
    public class RolesServiceTests
    {
        [Fact]
        public async Task CreateRole_WithEmptyName_ShouldReturnValidationError()
        {
            RolesServiceFactory factory = new();

            CreateRoleRequest request = RoleTestData.CreateRoleRequest(" ");

            Result<string> result = await factory.Service.CreateRole(request);

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Equal("Role name cannot be empty", result.Error);

            factory.RoleManager.Verify(roleManager => roleManager.FindByNameAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task CreateRole_WithExistingRole_ShouldReturnConflict()
        {
            RolesServiceFactory factory = new();

            CreateRoleRequest request = RoleTestData.CreateRoleRequest(" Admin ");
            Role role = RoleTestData.CreateRole("Admin");

            factory.RoleManager.Setup(roleManager => roleManager.FindByNameAsync("Admin")).ReturnsAsync(role);

            Result<string> result = await factory.Service.CreateRole(request);

            Assert.Equal(ResultStatus.Conflict, result.Status);
            Assert.Equal("Role with name  Admin  already exists", result.Error);

            factory.RoleManager.Verify(roleManager => roleManager.CreateAsync(It.IsAny<Role>()), Times.Never);
        }

        [Fact]
        public async Task CreateRole_WhenIdentityCreationFails_ShouldReturnValidationError()
        {
            RolesServiceFactory factory = new();

            CreateRoleRequest request = RoleTestData.CreateRoleRequest("Admin");
            Role role = RoleTestData.CreateRole("Admin");

            IdentityError error = new()
            {
                Description = "Unable to create role."
            };

            factory.RoleManager.Setup(roleManager => roleManager.FindByNameAsync("Admin")).ReturnsAsync((Role?)null);
            factory.Mapper.Setup(mapper => mapper.Map<Role>(request)).Returns(role);
            factory.RoleManager.Setup(roleManager => roleManager.CreateAsync(role)).ReturnsAsync(IdentityResult.Failed(error));

            Result<string> result = await factory.Service.CreateRole(request);

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Equal("Unable to create role.", result.Error);
        }

        [Fact]
        public async Task CreateRole_WithValidRequest_ShouldCreateRole()
        {
            RolesServiceFactory factory = new();

            CreateRoleRequest request = RoleTestData.CreateRoleRequest(" Admin ");
            Role role = RoleTestData.CreateRole();

            factory.RoleManager.Setup(roleManager => roleManager.FindByNameAsync("Admin")).ReturnsAsync((Role?)null);
            factory.Mapper.Setup(mapper => mapper.Map<Role>(request)).Returns(role);
            factory.RoleManager.Setup(roleManager => roleManager.CreateAsync(role)).ReturnsAsync(IdentityResult.Success);

            Result<string> result = await factory.Service.CreateRole(request);

            Assert.Equal(ResultStatus.Created, result.Status);
            Assert.Equal("Role with name  Admin  created successfully", result.Value);
            Assert.Equal("Admin", role.Name);

            factory.RoleManager.Verify(roleManager => roleManager.CreateAsync(role), Times.Once);
        }

        [Fact]
        public async Task EditRole_WithEmptyName_ShouldReturnValidationError()
        {
            RolesServiceFactory factory = new();

            CreateRoleRequest request = RoleTestData.CreateRoleRequest(" ");

            Result<string> result = await factory.Service.EditRole("role-id", request);

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Equal("Role name cannot be empty", result.Error);

            factory.RoleManager.Verify(roleManager => roleManager.FindByIdAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task EditRole_WithMissingRole_ShouldReturnNotFound()
        {
            RolesServiceFactory factory = new();

            CreateRoleRequest request = RoleTestData.CreateRoleRequest("Admin");

            factory.RoleManager.Setup(roleManager => roleManager.FindByIdAsync("role-id")).ReturnsAsync((Role?)null);

            Result<string> result = await factory.Service.EditRole("role-id", request);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Equal("Role with id role-id was not found", result.Error);
        }

        [Fact]
        public async Task EditRole_WhenIdentityUpdateFails_ShouldReturnValidationError()
        {
            RolesServiceFactory factory = new();

            CreateRoleRequest request = RoleTestData.CreateRoleRequest("Updated Admin");
            Role role = RoleTestData.CreateRole("Admin");

            IdentityError error = new()
            {
                Description = "Unable to update role."
            };

            factory.RoleManager.Setup(roleManager => roleManager.FindByIdAsync("role-id")).ReturnsAsync(role);
            factory.Mapper.Setup(mapper => mapper.Map(request, role)).Returns(role);
            factory.RoleManager.Setup(roleManager => roleManager.UpdateAsync(role)).ReturnsAsync(IdentityResult.Failed(error));

            Result<string> result = await factory.Service.EditRole("role-id", request);

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Equal("Unable to update role.", result.Error);
        }

        [Fact]
        public async Task EditRole_WithValidRequest_ShouldUpdateRole()
        {
            RolesServiceFactory factory = new();

            CreateRoleRequest request = RoleTestData.CreateRoleRequest(" Updated Admin ");
            Role role = RoleTestData.CreateRole("Admin");

            factory.RoleManager.Setup(roleManager => roleManager.FindByIdAsync("role-id")).ReturnsAsync(role);
            factory.Mapper.Setup(mapper => mapper.Map(request, role)).Returns(role);
            factory.RoleManager.Setup(roleManager => roleManager.UpdateAsync(role)).ReturnsAsync(IdentityResult.Success);

            Result<string> result = await factory.Service.EditRole("role-id", request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.Equal("Role updated successfully", result.Value);
            Assert.Equal("Updated Admin", role.Name);
        }

        [Fact]
        public async Task AddUserToRole_WithMissingUser_ShouldReturnNotFound()
        {
            RolesServiceFactory factory = new();

            AddUserToRoleRequest request = RoleTestData.CreateAddUserToRoleRequest(" user@example.com ", "Admin");

            factory.UserManager.Setup(userManager => userManager.FindByNameAsync("user@example.com")).ReturnsAsync((User?)null);

            Result<string> result = await factory.Service.AddUserToRole(request);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Equal("User with email  user@example.com  does not exist", result.Error);

            factory.RoleManager.Verify(roleManager => roleManager.FindByNameAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task AddUserToRole_WithMissingRole_ShouldReturnNotFound()
        {
            RolesServiceFactory factory = new();

            AddUserToRoleRequest request = RoleTestData.CreateAddUserToRoleRequest("user@example.com", " Admin ");
            User user = RoleTestData.CreateUser();

            factory.UserManager.Setup(userManager => userManager.FindByNameAsync("user@example.com")).ReturnsAsync(user);
            factory.RoleManager.Setup(roleManager => roleManager.FindByNameAsync("Admin")).ReturnsAsync((Role?)null);

            Result<string> result = await factory.Service.AddUserToRole(request);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Equal("Role with name  Admin  does not exist", result.Error);

            factory.UserManager.Verify(userManager => userManager.IsInRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task AddUserToRole_WhenUserAlreadyInRole_ShouldReturnConflict()
        {
            RolesServiceFactory factory = new();

            AddUserToRoleRequest request = RoleTestData.CreateAddUserToRoleRequest();
            User user = RoleTestData.CreateUser();
            Role role = RoleTestData.CreateRole();

            factory.UserManager.Setup(userManager => userManager.FindByNameAsync("user@example.com")).ReturnsAsync(user);
            factory.RoleManager.Setup(roleManager => roleManager.FindByNameAsync("Admin")).ReturnsAsync(role);
            factory.UserManager.Setup(userManager => userManager.IsInRoleAsync(user, "Admin")).ReturnsAsync(true);

            Result<string> result = await factory.Service.AddUserToRole(request);

            Assert.Equal(ResultStatus.Conflict, result.Status);
            Assert.Equal("user@example.com is already in the role Admin", result.Error);

            factory.UserManager.Verify(userManager => userManager.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task AddUserToRole_WhenIdentityAddFails_ShouldReturnValidationError()
        {
            RolesServiceFactory factory = new();

            AddUserToRoleRequest request = RoleTestData.CreateAddUserToRoleRequest();
            User user = RoleTestData.CreateUser();
            Role role = RoleTestData.CreateRole();

            IdentityError error = new()
            {
                Description = "Unable to add user to role."
            };

            factory.UserManager.Setup(userManager => userManager.FindByNameAsync("user@example.com")).ReturnsAsync(user);
            factory.RoleManager.Setup(roleManager => roleManager.FindByNameAsync("Admin")).ReturnsAsync(role);
            factory.UserManager.Setup(userManager => userManager.IsInRoleAsync(user, "Admin")).ReturnsAsync(false);
            factory.UserManager.Setup(userManager => userManager.AddToRoleAsync(user, "Admin")).ReturnsAsync(IdentityResult.Failed(error));

            Result<string> result = await factory.Service.AddUserToRole(request);

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Equal("Unable to add user to role.", result.Error);
        }

        [Fact]
        public async Task AddUserToRole_WithValidRequest_ShouldAddUserToRole()
        {
            RolesServiceFactory factory = new();

            AddUserToRoleRequest request = RoleTestData.CreateAddUserToRoleRequest();
            User user = RoleTestData.CreateUser();
            Role role = RoleTestData.CreateRole();

            factory.UserManager.Setup(userManager => userManager.FindByNameAsync("user@example.com")).ReturnsAsync(user);
            factory.RoleManager.Setup(roleManager => roleManager.FindByNameAsync("Admin")).ReturnsAsync(role);
            factory.UserManager.Setup(userManager => userManager.IsInRoleAsync(user, "Admin")).ReturnsAsync(false);
            factory.UserManager.Setup(userManager => userManager.AddToRoleAsync(user, "Admin")).ReturnsAsync(IdentityResult.Success);

            Result<string> result = await factory.Service.AddUserToRole(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.Equal("user@example.com has been added to the role Admin successfully", result.Value);

            factory.UserManager.Verify(userManager => userManager.AddToRoleAsync(user, "Admin"), Times.Once);
        }

        [Fact]
        public async Task GetUserRoles_WithMissingUser_ShouldReturnNotFound()
        {
            RolesServiceFactory factory = new();

            factory.UserManager.Setup(userManager => userManager.FindByNameAsync("user@example.com")).ReturnsAsync((User?)null);

            Result<IList<string>> result = await factory.Service.GetUserRoles(" user@example.com ");

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Equal("User with username  user@example.com  was not found", result.Error);

            factory.UserManager.Verify(userManager => userManager.GetRolesAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task GetUserRoles_WithExistingUser_ShouldReturnRoles()
        {
            RolesServiceFactory factory = new();

            User user = RoleTestData.CreateUser();
            IList<string> roles = new List<string> { "Admin", "ElectionManager" };

            factory.UserManager.Setup(userManager => userManager.FindByNameAsync("user@example.com")).ReturnsAsync(user);
            factory.UserManager.Setup(userManager => userManager.GetRolesAsync(user)).ReturnsAsync(roles);

            Result<IList<string>> result = await factory.Service.GetUserRoles(" user@example.com ");

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.Equal(2, result.Value.Count);
            Assert.Contains("Admin", result.Value);
            Assert.Contains("ElectionManager", result.Value);
        }

        [Fact]
        public async Task RemoveUserFromRole_WithMissingUser_ShouldReturnNotFound()
        {
            RolesServiceFactory factory = new();

            AddUserToRoleRequest request = RoleTestData.CreateAddUserToRoleRequest();

            factory.UserManager.Setup(userManager => userManager.FindByNameAsync("user@example.com")).ReturnsAsync((User?)null);

            Result<string> result = await factory.Service.RemoveUserFromRole(request);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Equal("User with email user@example.com does not exist", result.Error);

            factory.UserManager.Verify(userManager => userManager.GetRolesAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task RemoveUserFromRole_WhenUserIsNotInRole_ShouldReturnNotFound()
        {
            RolesServiceFactory factory = new();

            AddUserToRoleRequest request = RoleTestData.CreateAddUserToRoleRequest();
            User user = RoleTestData.CreateUser();

            factory.UserManager.Setup(userManager => userManager.FindByNameAsync("user@example.com")).ReturnsAsync(user);
            factory.UserManager.Setup(userManager => userManager.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Student" });

            Result<string> result = await factory.Service.RemoveUserFromRole(request);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Equal("User is not in the Admin role", result.Error);

            factory.UserManager.Verify(userManager => userManager.RemoveFromRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task RemoveUserFromRole_WhenIdentityRemoveFails_ShouldReturnValidationError()
        {
            RolesServiceFactory factory = new();

            AddUserToRoleRequest request = RoleTestData.CreateAddUserToRoleRequest();
            User user = RoleTestData.CreateUser();

            IdentityError error = new()
            {
                Description = "Unable to remove user from role."
            };

            factory.UserManager.Setup(userManager => userManager.FindByNameAsync("user@example.com")).ReturnsAsync(user);
            factory.UserManager.Setup(userManager => userManager.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Admin" });
            factory.UserManager.Setup(userManager => userManager.RemoveFromRoleAsync(user, "Admin")).ReturnsAsync(IdentityResult.Failed(error));

            Result<string> result = await factory.Service.RemoveUserFromRole(request);

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Equal("Unable to remove user from role.", result.Error);
        }

        [Fact]
        public async Task RemoveUserFromRole_WithValidRequest_ShouldRemoveUserFromRole()
        {
            RolesServiceFactory factory = new();

            AddUserToRoleRequest request = RoleTestData.CreateAddUserToRoleRequest("user@example.com", "admin");
            User user = RoleTestData.CreateUser();

            factory.UserManager.Setup(userManager => userManager.FindByNameAsync("user@example.com")).ReturnsAsync(user);
            factory.UserManager.Setup(userManager => userManager.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Admin" });
            factory.UserManager.Setup(userManager => userManager.RemoveFromRoleAsync(user, "Admin")).ReturnsAsync(IdentityResult.Success);

            Result<string> result = await factory.Service.RemoveUserFromRole(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.Equal("user@example.com removed from role admin successfully", result.Value);

            factory.UserManager.Verify(userManager => userManager.RemoveFromRoleAsync(user, "Admin"), Times.Once);
        }

        [Fact]
        public async Task DeleteRole_WithMissingRole_ShouldReturnNotFound()
        {
            RolesServiceFactory factory = new();

            CreateRoleRequest request = RoleTestData.CreateRoleRequest(" Admin ");

            factory.RoleManager.Setup(roleManager => roleManager.FindByNameAsync("Admin")).ReturnsAsync((Role?)null);

            Result<string> result = await factory.Service.DeleteRole(request);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Equal("Role  Admin  does not exist", result.Error);

            factory.RoleManager.Verify(roleManager => roleManager.DeleteAsync(It.IsAny<Role>()), Times.Never);
        }

        [Fact]
        public async Task DeleteRole_WhenIdentityDeleteFails_ShouldReturnValidationError()
        {
            RolesServiceFactory factory = new();

            CreateRoleRequest request = RoleTestData.CreateRoleRequest("Admin");
            Role role = RoleTestData.CreateRole("Admin");

            IdentityError error = new()
            {
                Description = "Unable to delete role."
            };

            factory.RoleManager.Setup(roleManager => roleManager.FindByNameAsync("Admin")).ReturnsAsync(role);
            factory.RoleManager.Setup(roleManager => roleManager.DeleteAsync(role)).ReturnsAsync(IdentityResult.Failed(error));

            Result<string> result = await factory.Service.DeleteRole(request);

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Equal("Unable to delete role.", result.Error);
        }

        [Fact]
        public async Task DeleteRole_WithExistingRole_ShouldDeleteRole()
        {
            RolesServiceFactory factory = new();

            CreateRoleRequest request = RoleTestData.CreateRoleRequest("Admin");
            Role role = RoleTestData.CreateRole("Admin");

            factory.RoleManager.Setup(roleManager => roleManager.FindByNameAsync("Admin")).ReturnsAsync(role);
            factory.RoleManager.Setup(roleManager => roleManager.DeleteAsync(role)).ReturnsAsync(IdentityResult.Success);

            Result<string> result = await factory.Service.DeleteRole(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.Equal("Role with name Admin has been deleted successfully", result.Value);

            factory.RoleManager.Verify(roleManager => roleManager.DeleteAsync(role), Times.Once);
        }

        [Fact]
        public async Task DeleteUserRole_WithMissingRole_ShouldReturnNotFound()
        {
            RolesServiceFactory factory = new();

            factory.RoleManager.Setup(roleManager => roleManager.FindByIdAsync("role-id")).ReturnsAsync((Role?)null);

            Result<string> result = await factory.Service.DeleteUserRole("role-id");

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Equal("Role with id role-id does not exist", result.Error);
        }

        [Fact]
        public async Task DeleteUserRole_WhenIdentityDeleteFails_ShouldReturnValidationError()
        {
            RolesServiceFactory factory = new();

            Role role = RoleTestData.CreateRole("Admin");

            IdentityError error = new()
            {
                Description = "Unable to delete role."
            };

            factory.RoleManager.Setup(roleManager => roleManager.FindByIdAsync("role-id")).ReturnsAsync(role);
            factory.RoleManager.Setup(roleManager => roleManager.DeleteAsync(role)).ReturnsAsync(IdentityResult.Failed(error));

            Result<string> result = await factory.Service.DeleteUserRole("role-id");

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Equal("Unable to delete role.", result.Error);
        }

        [Fact]
        public async Task DeleteUserRole_WithExistingRole_ShouldDeleteRole()
        {
            RolesServiceFactory factory = new();

            Role role = RoleTestData.CreateRole("Admin");

            factory.RoleManager.Setup(roleManager => roleManager.FindByIdAsync("role-id")).ReturnsAsync(role);
            factory.RoleManager.Setup(roleManager => roleManager.DeleteAsync(role)).ReturnsAsync(IdentityResult.Success);

            Result<string> result = await factory.Service.DeleteUserRole("role-id");

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.Equal("Role with name Admin deleted successfully", result.Value);
        }

        [Fact]
        public async Task ToggleRoleStatus_WithMissingRole_ShouldReturnNotFound()
        {
            RolesServiceFactory factory = new();

            factory.RoleManager.Setup(roleManager => roleManager.FindByIdAsync("role-id")).ReturnsAsync((Role?)null);

            Result<string> result = await factory.Service.ToggleRoleStatus("role-id");

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Equal("Role does not exist", result.Error);
        }

        [Fact]
        public async Task ToggleRoleStatus_WhenIdentityUpdateFails_ShouldReturnValidationError()
        {
            RolesServiceFactory factory = new();

            Role role = RoleTestData.CreateRole("Admin", true);

            IdentityError error = new()
            {
                Description = "Unable to update role."
            };

            factory.RoleManager.Setup(roleManager => roleManager.FindByIdAsync("role-id")).ReturnsAsync(role);
            factory.RoleManager.Setup(roleManager => roleManager.UpdateAsync(role)).ReturnsAsync(IdentityResult.Failed(error));

            Result<string> result = await factory.Service.ToggleRoleStatus("role-id");

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Equal("Unable to update role.", result.Error);
        }

        [Fact]
        public async Task ToggleRoleStatus_WithActiveRole_ShouldDeactivateRole()
        {
            RolesServiceFactory factory = new();

            Role role = RoleTestData.CreateRole("Admin", true);

            factory.RoleManager.Setup(roleManager => roleManager.FindByIdAsync("role-id")).ReturnsAsync(role);
            factory.RoleManager.Setup(roleManager => roleManager.UpdateAsync(role)).ReturnsAsync(IdentityResult.Success);

            Result<string> result = await factory.Service.ToggleRoleStatus("role-id");

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.Equal("Role Admin deactivated successfully", result.Value);
            Assert.False(role.Active);
        }

        [Fact]
        public async Task ToggleRoleStatus_WithInactiveRole_ShouldActivateRole()
        {
            RolesServiceFactory factory = new();

            Role role = RoleTestData.CreateRole("Admin", false);

            factory.RoleManager.Setup(roleManager => roleManager.FindByIdAsync("role-id")).ReturnsAsync(role);
            factory.RoleManager.Setup(roleManager => roleManager.UpdateAsync(role)).ReturnsAsync(IdentityResult.Success);

            Result<string> result = await factory.Service.ToggleRoleStatus("role-id");

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.Equal("Role Admin activated successfully", result.Value);
            Assert.True(role.Active);
        }
    }
}