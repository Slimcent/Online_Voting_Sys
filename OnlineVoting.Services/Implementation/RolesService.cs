using AutoMapper;
using Microsoft.AspNetCore.Identity;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Pagination;
using OnlineVoting.Models.Results;
using OnlineVoting.Services.Interfaces;
using SchMgr_FUTO.Data.Interfaces;
using VotingSystem.Data.Interfaces;

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

        public RolesService(IServiceFactory serviceFactory)
        {
            _serviceFactory = serviceFactory;
            _unitOfWork = _serviceFactory.GetService<IUnitOfWork>();
            _userManager = _serviceFactory.GetService<UserManager<User>>();
            _roleManager = _serviceFactory.GetService<RoleManager<Role>>();
            _roleRepo = _unitOfWork.GetRepository<Role>();
            _mapper = _serviceFactory.GetService<IMapper>();
        }

        public async Task<Result<string>> CreateRole(CreateRoleRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return Result<string>.ValidationError("Role name cannot be empty");

            Role roleExists = await _roleManager.FindByNameAsync(request.Name.Trim().ToLower());
            if (roleExists != null)
                return Result<string>.Conflict($"Role with name {request.Name} already exists");

            Role roleToCreate = _mapper.Map<Role>(request);

            IdentityResult result = await _roleManager.CreateAsync(roleToCreate);
            if (!result.Succeeded)
            {
                string errorMessage = string.Join("\n", result.Errors.Select(x => x.Description));

                return Result<string>.ValidationError(errorMessage);
            }

            return Result<string>.Created($"Role with name {request.Name} created successfully");
        }

        public async Task<Result<string>> EditRole(string id, CreateRoleRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return Result<string>.ValidationError("Role name cannot be empty");

            Role role = await _roleManager.FindByIdAsync(id);
            if (role == null)
                return Result<string>.NotFound($"Role with id {id} was not found");

            Role roleUpdate = _mapper.Map(request, role);

            IdentityResult result = await _roleManager.UpdateAsync(roleUpdate);
            if (!result.Succeeded)
            {
                string errorMessage = string.Join("\n", result.Errors.Select(x => x.Description));

                return Result<string>.ValidationError(errorMessage);
            }

            return Result<string>.Success("Role updated successfully");
        }

        public async Task<Result<string>> AddUserToRole(AddUserToRoleRequest request)
        {
            User user = await _userManager.FindByNameAsync(request.Email.Trim().ToLower());
            if (user == null)
                return Result<string>.NotFound($"User with email {request.Email} does not exist");

            Role role = await _roleManager.FindByNameAsync(request.Name.Trim());
            if (role == null)
                return Result<string>.NotFound($"Role with name {request.Name} does not exist");

            bool userIsInRole = await _userManager.IsInRoleAsync(user, role.Name);
            if (userIsInRole)
                return Result<string>.Conflict($"{request.Email} is already in the role {request.Name}");

            IdentityResult result = await _userManager.AddToRoleAsync(user, role.Name);

            if (!result.Succeeded)
            {
                string errorMessage = string.Join("\n", result.Errors.Select(x => x.Description));

                return Result<string>.ValidationError(errorMessage);
            }

            return Result<string>.Success($"{request.Email} has been added to the role {request.Name} successfully");
        }

        public async Task<Result<IList<string>>> GetUserRoles(string userName)
        {
            User user = await _userManager.FindByNameAsync(userName.Trim().ToLower());
            if (user == null)
                return Result<IList<string>>.NotFound($"User with username {userName} was not found");

            IList<string> userRoles = await _userManager.GetRolesAsync(user);

            return Result<IList<string>>.Success(userRoles);
        }

        public async Task<Result<string>> RemoveUserFromRole(AddUserToRoleRequest request)
        {
            User user = await _userManager.FindByNameAsync(request.Email.Trim().ToLower());
            if (user == null)
                return Result<string>.NotFound($"User with email {request.Email} does not exist");

            IList<string> userRoles = await _userManager.GetRolesAsync(user);
            string roleToRemove = userRoles.FirstOrDefault(role => role.Equals(request.Name, StringComparison.InvariantCultureIgnoreCase));

            if (roleToRemove == null)
                return Result<string>.NotFound($"User is not in the {request.Name} role");

            IdentityResult result = await _userManager.RemoveFromRoleAsync(user, roleToRemove);
            if (!result.Succeeded)
            {
                string errorMessage = string.Join("\n", result.Errors.Select(x => x.Description));

                return Result<string>.ValidationError(errorMessage);
            }

            return Result<string>.Success($"{request.Email} removed from role {request.Name} successfully");
        }

        public async Task<Result<string>> DeleteRole(CreateRoleRequest request)
        {
            Role role = await _roleManager.FindByNameAsync(request.Name.Trim().ToLower());

            if (role == null)
                return Result<string>.NotFound($"Role {request.Name} does not exist");

            IdentityResult result = await _roleManager.DeleteAsync(role);
            if (!result.Succeeded)
            {
                string errorMessage = string.Join("\n", result.Errors.Select(x => x.Description));

                return Result<string>.ValidationError(errorMessage);
            }

            return Result<string>.Success($"Role with name {role.Name} has been deleted successfully");
        }

        public async Task<Result<string>> DeleteUserRole(string id)
        {
            Role role = await _roleManager.FindByIdAsync(id);

            if (role == null)
                return Result<string>.NotFound($"Role with id {id} does not exist");

            IdentityResult result = await _roleManager.DeleteAsync(role);
            if (!result.Succeeded)
            {
                string errorMessage = string.Join("\n", result.Errors.Select(x => x.Description));

                return Result<string>.ValidationError(errorMessage);
            }

            return Result<string>.Success($"Role with name {role.Name} deleted successfully");
        }

        public async Task<Result<string>> ToggleRoleStatus(string roleId)
        {
            Role role = await _roleManager.FindByIdAsync(roleId);

            if (role == null)
                return Result<string>.NotFound("Role does not exist");

            role.Active = !role.Active;

            IdentityResult result = await _roleManager.UpdateAsync(role);
            if (!result.Succeeded)
            {
                string errorMessage = string.Join("\n", result.Errors.Select(x => x.Description));

                return Result<string>.ValidationError(errorMessage);
            }

            if (role.Active == true)
            {
                return Result<string>.Success($"Role {role.Name} activated successfully");
            }
            else
            {
                return Result<string>.Success($"Role {role.Name} deactivated successfully");
            }
        }

        public async Task<Result<IEnumerable<RoleResponse>>> GetAllRoles()
        {
            IEnumerable<Role> allRoles = await _roleRepo.GetAllAsync();

            if (!allRoles.Any())
            {
                return Result<IEnumerable<RoleResponse>>.Success(new List<RoleResponse>());
            }

            IEnumerable<RoleResponse> roles = _mapper.Map<IEnumerable<RoleResponse>>(allRoles);

            return Result<IEnumerable<RoleResponse>>.Success(roles);
        }

        public async Task<Result<IEnumerable<RoleResponse>>> GetAllActiveRoles()
        {
            IEnumerable<Role> allRoles = await _roleRepo.GetByAsync(x => x.Active == true);

            if (!allRoles.Any())
            {
                return Result<IEnumerable<RoleResponse>>.Success(new List<RoleResponse>());
            }

            IEnumerable<RoleResponse> roles = _mapper.Map<IEnumerable<RoleResponse>>(allRoles);

            return Result<IEnumerable<RoleResponse>>.Success(roles);
        }

        public async Task<Result<IEnumerable<RoleResponse>>> GetAllDeactivatedRoles()
        {
            IEnumerable<Role> allRoles = await _roleRepo.GetByAsync(x => x.Active == false);

            if (!allRoles.Any())
            {
                return Result<IEnumerable<RoleResponse>>.Success(new List<RoleResponse>());
            }

            IEnumerable<RoleResponse> roles = _mapper.Map<IEnumerable<RoleResponse>>(allRoles);

            return Result<IEnumerable<RoleResponse>>.Success(roles);
        }

        public async Task<Result<PagedResponse<RoleResponse>>> AllRoles(RoleRequest request)
        {
            PagedList<Role> roles = string.IsNullOrWhiteSpace(request.SearchTerm)
                ? await _roleRepo.GetPagedItems(request)
                : await _roleRepo.GetPagedItems(request, x => x.Name.Contains(request.SearchTerm.ToLower().Trim()));

            PagedResponse<RoleResponse> response = _mapper.Map<PagedResponse<RoleResponse>>(roles);

            return Result<PagedResponse<RoleResponse>>.Success(response);
        }

        public async Task<Result<PagedResponse<RoleResponse>>> AllActiveRoles(RoleRequest request)
        {
            PagedList<Role> roles = string.IsNullOrWhiteSpace(request.SearchTerm)
                ? await _roleRepo.GetPagedItems(request, x => x.Active == true)
                : await _roleRepo.GetPagedItems(request, x => x.Name.Contains(request.SearchTerm.ToLower().Trim()));

            PagedResponse<RoleResponse> response = _mapper.Map<PagedResponse<RoleResponse>>(roles);

            return Result<PagedResponse<RoleResponse>>.Success(response);
        }

        public async Task<Result<PagedResponse<RoleResponse>>> AllDeactivatedRoles(RoleRequest request)
        {
            PagedList<Role> roles = string.IsNullOrWhiteSpace(request.SearchTerm)
                ? await _roleRepo.GetPagedItems(request, x => x.Active == false)
                : await _roleRepo.GetPagedItems(request, x => x.Name.Contains(request.SearchTerm.ToLower().Trim()));

            PagedResponse<RoleResponse> response = _mapper.Map<PagedResponse<RoleResponse>>(roles);

            return Result<PagedResponse<RoleResponse>>.Success(response);
        }
    }
}