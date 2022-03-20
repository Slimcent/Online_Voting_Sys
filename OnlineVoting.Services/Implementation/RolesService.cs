using AutoMapper;
using Microsoft.AspNetCore.Identity;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Entities;
using OnlineVoting.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            _unitOfWork = _serviceFactory.GetService<IUnitOfWork>();
            _userManager = _serviceFactory.GetService<UserManager<User>>();
            _roleManager = _serviceFactory.GetService<RoleManager<Role>>();
            _roleRepo = _unitOfWork.GetRepository<Role>();
            _mapper = _serviceFactory.GetService<IMapper>();
        }

        public async Task<string> CreateRole(RoleDto request)
        {
            var roleExists = await _roleManager.FindByNameAsync(request.Name.Trim().ToLower());
            if (roleExists != null)
                throw new InvalidOperationException($"Role with name {request.Name} already exist");

            var roleToCreate = _mapper.Map<Role>(request);

            var result = await _roleManager.CreateAsync(roleToCreate);
            if (!result.Succeeded)
                throw new InvalidOperationException("Role creation failed");

            return $"Role with name {request.Name} created successfully";
        }

        public Task<IEnumerable<RoleResponseDto>> GetAllRoles()
        {
            throw new NotImplementedException();
        }

        public async Task EditRole(string id, RoleDto request)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
                throw new InvalidOperationException($"Role with {id} not found");

            var roleUpdate = _mapper.Map(request, role);

            await _roleManager.UpdateAsync(roleUpdate);
        }

        public async Task<string> AddUserToRole(AddUserToRoleDto request)
        {
            var user = await _userManager.FindByNameAsync(request.UserName.Trim().ToLower());
            if (user == null)
                return $"User with email {request.UserName} does not Exist";

            var result = await _userManager.AddToRoleAsync(user, request.Name);

            if (!result.Succeeded)
                return $"Adding {request.UserName} to the Role {request.Name} failed!";

            return $"{request.UserName} has been added to the Role, {request.Name} Successful!";
        }

        public async Task<IList<string>> GetUserRoles(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName.Trim().ToLower());
            if (user == null)
                throw new InvalidOperationException($"User with userName {userName} not found");

            List<string> userRoles = (List<string>)await _userManager.GetRolesAsync(user);
            if (!userRoles.Any())
                throw new InvalidOperationException($"user {userName} has no role");

            return userRoles;
        }

        public async Task<string> RemoveUserFromRole(AddUserToRoleDto request)
        {
            var user = await _userManager.FindByNameAsync(request.UserName.Trim().ToLower());
            if (user == null)
                return $"User with email {request.UserName} does not Exist";

            List<string> userRoles = (List<string>)await _userManager.GetRolesAsync(user);
            var roleToRemove = userRoles.FirstOrDefault(role => role.Equals(request.Name, StringComparison.InvariantCultureIgnoreCase));

            if (roleToRemove == null)
                return $"User not in {request.Name} Role";

            var result = await _userManager.RemoveFromRoleAsync(user, roleToRemove);
            if (!result.Succeeded)
                return $"failed to remove {request.UserName} from Role {request.Name}";

            return $"{request.UserName} removed from Role {request.Name} successfully";
        }

        public async Task<string> DeleteRole(RoleDto request)
        {
            var role = await _roleManager.FindByNameAsync(request.Name.Trim().ToLower());

            if (role is null)
                return $"Role {request.Name} does not Exist";

            await _roleManager.DeleteAsync(role);

            return $"Role with Name {role.Name} has been deleted Successfully";
        }
    }
}
