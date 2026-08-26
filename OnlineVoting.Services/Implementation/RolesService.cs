using AutoMapper;
using Microsoft.AspNetCore.Identity;
using OnlineVoting.Data.Interfaces;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Pagination;
using OnlineVoting.Models.Results;
using OnlineVoting.Services.Interfaces;
using VotingSystem.Logger;

namespace OnlineVoting.Services.Implementation
{
    public class RolesService : IRolesService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IServiceFactory _serviceFactory;
        private readonly IMapper _mapper;
        private readonly IRepository<Role> _roleRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggerMessage _loggerMessage;

        public RolesService(IServiceFactory serviceFactory)
        {
            _serviceFactory = serviceFactory;
            _unitOfWork = _serviceFactory.GetService<IUnitOfWork>();
            _userManager = _serviceFactory.GetService<UserManager<User>>();
            _roleManager = _serviceFactory.GetService<RoleManager<Role>>();
            _roleRepo = _unitOfWork.GetRepository<Role>();
            _mapper = _serviceFactory.GetService<IMapper>();
            _loggerMessage = _serviceFactory.GetService<ILoggerMessage>();
        }

        public async Task<Result<string>> CreateRole(CreateRoleRequest request)
        {
            _loggerMessage.LogInfo($"Role creation request received for role {request.Name}.");

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                _loggerMessage.LogWarn("Role creation failed because the role name was empty.");

                return Result<string>.ValidationError("Role name cannot be empty");
            }

            string roleName = request.Name.Trim();

            Role roleExists = await _roleManager.FindByNameAsync(roleName);

            if (roleExists != null)
            {
                _loggerMessage.LogWarn($"Role creation skipped because role {roleName} already exists.");

                return Result<string>.Conflict($"Role with name {request.Name} already exists");
            }

            Role roleToCreate = _mapper.Map<Role>(request);
            roleToCreate.Name = roleName;

            IdentityResult result = await _roleManager.CreateAsync(roleToCreate);

            if (!result.Succeeded)
            {
                string errorMessage = string.Join("\n", result.Errors.Select(x => x.Description));

                _loggerMessage.LogWarn($"Role creation failed for role {roleName}.");

                return Result<string>.ValidationError(errorMessage);
            }

            _loggerMessage.LogInfo($"Role {roleName} created successfully.");

            return Result<string>.Created($"Role with name {request.Name} created successfully");
        }

        public async Task<Result<string>> EditRole(string id, CreateRoleRequest request)
        {
            _loggerMessage.LogInfo($"Role update request received for role id {id}.");

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                _loggerMessage.LogWarn($"Role update failed for role id {id} because the role name was empty.");

                return Result<string>.ValidationError("Role name cannot be empty");
            }

            Role role = await _roleManager.FindByIdAsync(id);

            if (role == null)
            {
                _loggerMessage.LogWarn($"Role update failed because role with id {id} was not found.");

                return Result<string>.NotFound($"Role with id {id} was not found");
            }

            string roleName = request.Name.Trim();

            Role roleUpdate = _mapper.Map(request, role);
            roleUpdate.Name = roleName;

            IdentityResult result = await _roleManager.UpdateAsync(roleUpdate);

            if (!result.Succeeded)
            {
                string errorMessage = string.Join("\n", result.Errors.Select(x => x.Description));

                _loggerMessage.LogWarn($"Role update failed for role id {id}.");

                return Result<string>.ValidationError(errorMessage);
            }

            _loggerMessage.LogInfo($"Role with id {id} updated successfully.");

            return Result<string>.Success("Role updated successfully");
        }

        public async Task<Result<string>> AddUserToRole(AddUserToRoleRequest request)
        {
            _loggerMessage.LogInfo($"Add user to role request received for email {request.Email} and role {request.Name}.");

            string email = request.Email.Trim();
            string roleName = request.Name.Trim();

            User user = await _userManager.FindByNameAsync(email);

            if (user == null)
            {
                _loggerMessage.LogWarn($"Add user to role failed because user with email {email} was not found.");

                return Result<string>.NotFound($"User with email {request.Email} does not exist");
            }

            Role role = await _roleManager.FindByNameAsync(roleName);

            if (role == null)
            {
                _loggerMessage.LogWarn($"Add user to role failed because role {roleName} was not found.");

                return Result<string>.NotFound($"Role with name {request.Name} does not exist");
            }

            bool userIsInRole = await _userManager.IsInRoleAsync(user, role.Name);

            if (userIsInRole)
            {
                _loggerMessage.LogWarn($"User {user.Id} is already in role {role.Name}.");

                return Result<string>.Conflict($"{request.Email} is already in the role {request.Name}");
            }

            IdentityResult result = await _userManager.AddToRoleAsync(user, role.Name);

            if (!result.Succeeded)
            {
                string errorMessage = string.Join("\n", result.Errors.Select(x => x.Description));

                _loggerMessage.LogWarn($"Adding user {user.Id} to role {role.Name} failed.");

                return Result<string>.ValidationError(errorMessage);
            }

            _loggerMessage.LogInfo($"User {user.Id} added to role {role.Name} successfully.");

            return Result<string>.Success($"{request.Email} has been added to the role {request.Name} successfully");
        }

        public async Task<Result<IList<string>>> GetUserRoles(string userName)
        {
            _loggerMessage.LogInfo($"User roles request received for username {userName}.");

            string username = userName.Trim();

            User user = await _userManager.FindByNameAsync(username);

            if (user == null)
            {
                _loggerMessage.LogWarn($"User roles request failed because user {username} was not found.");

                return Result<IList<string>>.NotFound($"User with username {userName} was not found");
            }

            IList<string> userRoles = await _userManager.GetRolesAsync(user);

            _loggerMessage.LogInfo($"{userRoles.Count} roles found for user {user.Id}.");

            return Result<IList<string>>.Success(userRoles);
        }

        public async Task<Result<string>> RemoveUserFromRole(AddUserToRoleRequest request)
        {
            _loggerMessage.LogInfo($"Remove user from role request received for email {request.Email} and role {request.Name}.");

            string email = request.Email.Trim();

            User user = await _userManager.FindByNameAsync(email);

            if (user == null)
            {
                _loggerMessage.LogWarn($"Remove user from role failed because user with email {email} was not found.");

                return Result<string>.NotFound($"User with email {request.Email} does not exist");
            }

            IList<string> userRoles = await _userManager.GetRolesAsync(user);
            string roleToRemove = userRoles.FirstOrDefault(role => role.Equals(request.Name.Trim(), StringComparison.InvariantCultureIgnoreCase));

            if (roleToRemove == null)
            {
                _loggerMessage.LogWarn($"Remove user from role failed because user {user.Id} is not in role {request.Name}.");

                return Result<string>.NotFound($"User is not in the {request.Name} role");
            }

            IdentityResult result = await _userManager.RemoveFromRoleAsync(user, roleToRemove);

            if (!result.Succeeded)
            {
                string errorMessage = string.Join("\n", result.Errors.Select(x => x.Description));

                _loggerMessage.LogWarn($"Removing user {user.Id} from role {roleToRemove} failed.");

                return Result<string>.ValidationError(errorMessage);
            }

            _loggerMessage.LogInfo($"User {user.Id} removed from role {roleToRemove} successfully.");

            return Result<string>.Success($"{request.Email} removed from role {request.Name} successfully");
        }

        public async Task<Result<string>> DeleteRole(CreateRoleRequest request)
        {
            _loggerMessage.LogInfo($"Role deletion request received for role {request.Name}.");

            string roleName = request.Name.Trim();

            Role role = await _roleManager.FindByNameAsync(roleName);

            if (role == null)
            {
                _loggerMessage.LogWarn($"Role deletion failed because role {roleName} was not found.");

                return Result<string>.NotFound($"Role {request.Name} does not exist");
            }

            IdentityResult result = await _roleManager.DeleteAsync(role);

            if (!result.Succeeded)
            {
                string errorMessage = string.Join("\n", result.Errors.Select(x => x.Description));

                _loggerMessage.LogWarn($"Role deletion failed for role {role.Name}.");

                return Result<string>.ValidationError(errorMessage);
            }

            _loggerMessage.LogInfo($"Role {role.Name} deleted successfully.");

            return Result<string>.Success($"Role with name {role.Name} has been deleted successfully");
        }

        public async Task<Result<string>> DeleteUserRole(string id)
        {
            _loggerMessage.LogInfo($"Role deletion request received for role id {id}.");

            Role role = await _roleManager.FindByIdAsync(id);

            if (role == null)
            {
                _loggerMessage.LogWarn($"Role deletion failed because role with id {id} was not found.");

                return Result<string>.NotFound($"Role with id {id} does not exist");
            }

            IdentityResult result = await _roleManager.DeleteAsync(role);

            if (!result.Succeeded)
            {
                string errorMessage = string.Join("\n", result.Errors.Select(x => x.Description));

                _loggerMessage.LogWarn($"Role deletion failed for role id {id}.");

                return Result<string>.ValidationError(errorMessage);
            }

            _loggerMessage.LogInfo($"Role {role.Name} deleted successfully.");

            return Result<string>.Success($"Role with name {role.Name} deleted successfully");
        }

        public async Task<Result<string>> ToggleRoleStatus(string roleId)
        {
            _loggerMessage.LogInfo($"Role status toggle request received for role id {roleId}.");

            Role role = await _roleManager.FindByIdAsync(roleId);

            if (role == null)
            {
                _loggerMessage.LogWarn($"Role status toggle failed because role with id {roleId} was not found.");

                return Result<string>.NotFound("Role does not exist");
            }

            role.Active = !role.Active;

            IdentityResult result = await _roleManager.UpdateAsync(role);

            if (!result.Succeeded)
            {
                string errorMessage = string.Join("\n", result.Errors.Select(x => x.Description));

                _loggerMessage.LogWarn($"Role status toggle failed for role {role.Name}.");

                return Result<string>.ValidationError(errorMessage);
            }

            string status = role.Active ? "activated" : "deactivated";

            _loggerMessage.LogInfo($"Role {role.Name} {status} successfully.");

            return Result<string>.Success($"Role {role.Name} {status} successfully");
        }

        public async Task<Result<IEnumerable<RoleResponse>>> GetAllRoles()
        {
            _loggerMessage.LogInfo("All roles request received.");

            IEnumerable<Role> allRoles = await _roleRepo.GetAllAsync();

            if (!allRoles.Any())
            {
                _loggerMessage.LogInfo("No roles were found.");

                return Result<IEnumerable<RoleResponse>>.Success(new List<RoleResponse>());
            }

            IEnumerable<RoleResponse> roles = _mapper.Map<IEnumerable<RoleResponse>>(allRoles);

            _loggerMessage.LogInfo($"{roles.Count()} roles found.");

            return Result<IEnumerable<RoleResponse>>.Success(roles);
        }

        public async Task<Result<IEnumerable<RoleResponse>>> GetAllActiveRoles()
        {
            _loggerMessage.LogInfo("Active roles request received.");

            IEnumerable<Role> allRoles = await _roleRepo.GetByAsync(x => x.Active);

            if (!allRoles.Any())
            {
                _loggerMessage.LogInfo("No active roles were found.");

                return Result<IEnumerable<RoleResponse>>.Success(new List<RoleResponse>());
            }

            IEnumerable<RoleResponse> roles = _mapper.Map<IEnumerable<RoleResponse>>(allRoles);

            _loggerMessage.LogInfo($"{roles.Count()} active roles found.");

            return Result<IEnumerable<RoleResponse>>.Success(roles);
        }

        public async Task<Result<IEnumerable<RoleResponse>>> GetAllDeactivatedRoles()
        {
            _loggerMessage.LogInfo("Deactivated roles request received.");

            IEnumerable<Role> allRoles = await _roleRepo.GetByAsync(x => !x.Active);

            if (!allRoles.Any())
            {
                _loggerMessage.LogInfo("No deactivated roles were found.");

                return Result<IEnumerable<RoleResponse>>.Success(new List<RoleResponse>());
            }

            IEnumerable<RoleResponse> roles = _mapper.Map<IEnumerable<RoleResponse>>(allRoles);

            _loggerMessage.LogInfo($"{roles.Count()} deactivated roles found.");

            return Result<IEnumerable<RoleResponse>>.Success(roles);
        }

        public async Task<Result<PagedResponse<RoleResponse>>> AllRoles(RoleRequest request)
        {
            _loggerMessage.LogInfo($"Role list request received for page {request.PageNumber}.");

            PagedList<Role> roles = string.IsNullOrWhiteSpace(request.SearchTerm)
                ? await _roleRepo.GetPagedItems(request)
                : await _roleRepo.GetPagedItems(request, x => x.Name.Contains(request.SearchTerm.Trim()));

            PagedResponse<RoleResponse> response = _mapper.Map<PagedResponse<RoleResponse>>(roles);

            _loggerMessage.LogInfo($"{roles.MetaData.TotalCount} roles found.");

            return Result<PagedResponse<RoleResponse>>.Success(response);
        }

        public async Task<Result<PagedResponse<RoleResponse>>> AllActiveRoles(RoleRequest request)
        {
            _loggerMessage.LogInfo($"Active role list request received for page {request.PageNumber}.");

            PagedList<Role> roles = string.IsNullOrWhiteSpace(request.SearchTerm)
                ? await _roleRepo.GetPagedItems(request, x => x.Active)
                : await _roleRepo.GetPagedItems(request, x => x.Active && x.Name.Contains(request.SearchTerm.Trim()));

            PagedResponse<RoleResponse> response = _mapper.Map<PagedResponse<RoleResponse>>(roles);

            _loggerMessage.LogInfo($"{roles.MetaData.TotalCount} active roles found.");

            return Result<PagedResponse<RoleResponse>>.Success(response);
        }

        public async Task<Result<PagedResponse<RoleResponse>>> AllDeactivatedRoles(RoleRequest request)
        {
            _loggerMessage.LogInfo($"Deactivated role list request received for page {request.PageNumber}.");

            PagedList<Role> roles = string.IsNullOrWhiteSpace(request.SearchTerm)
                ? await _roleRepo.GetPagedItems(request, x => !x.Active)
                : await _roleRepo.GetPagedItems(request, x => !x.Active && x.Name.Contains(request.SearchTerm.Trim()));

            PagedResponse<RoleResponse> response = _mapper.Map<PagedResponse<RoleResponse>>(roles);

            _loggerMessage.LogInfo($"{roles.MetaData.TotalCount} deactivated roles found.");

            return Result<PagedResponse<RoleResponse>>.Success(response);
        }
    }
}