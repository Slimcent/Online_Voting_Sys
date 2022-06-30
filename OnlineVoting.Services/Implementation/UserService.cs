using AutoMapper;
using Microsoft.AspNetCore.Identity;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Dtos.Response.Jwt;
using OnlineVoting.Models.Entities;
using OnlineVoting.Services.Exceptions;
using OnlineVoting.Services.Infrastructures.Jwt;
using OnlineVoting.Services.Interfaces;
using System.Security.Claims;
using VotingSystem.Data.Interfaces;

namespace OnlineVoting.Services.Implementation
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IRepository<User> _userRepo;
        private readonly IRepository<Role> _roleRepo;
        private readonly IRepository<Student> _studentRepo;
        private readonly IRepository<Staff> _staffRepo;
        private readonly IMapper _mapper;
        private readonly IServiceFactory _serviceFactory;
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IServiceFactory serviceFactory)
        {
            _serviceFactory = serviceFactory;
            _unitOfWork = serviceFactory.GetService<IUnitOfWork>();
            _userManager = serviceFactory.GetService<UserManager<User>>();
            _roleManager = serviceFactory.GetService<RoleManager<Role>>();
            _studentRepo = _unitOfWork.GetRepository<Student>();
            _staffRepo = _unitOfWork.GetRepository<Staff>();
            _mapper = _serviceFactory.GetService<IMapper>();
        }

        public async Task<string> CreateUser(UserCreateRequestDto model)
        {
            if (model == null)
                throw new InvalidOperationException("Invalid data sent");

            var existingUser = await _userManager.FindByEmailAsync(model.Email.Trim().ToLower());
            if (existingUser != null)
                throw new UserExistException(model.Email);

            var user = _mapper.Map<User>(model);
            user.EmailConfirmed = true;

            var password = "123456";
            var res = await _userManager.CreateAsync(user, password);

            if (!res.Succeeded)
                throw new InvalidOperationException($"User creation failed");

            AddUserToRoleDto userRole = new() { UserName = user.Email, Name = model.Role };

            await _serviceFactory.GetService<IRolesService>().AddUserToRole(userRole);

            return user.Id;
        }

        public async Task<LoggedInUserDto> UserLogin(LoginDto request)
        {
            User user = await _userManager.FindByNameAsync(request.Email.ToLower().Trim());

            if (user == null)
                throw new InvalidOperationException("Invalid email or password");

            if (user.IsActive == false)
                throw new InvalidOperationException("Account is not active, contact the admin");

            bool result = await _userManager.CheckPasswordAsync(user, request.Password);

            if (!result)
                throw new InvalidOperationException("Invalid email or password");

            JwtToken userToken = await GetTokenAsync(user);

            List<Claim> userClaims = (await _userManager.GetClaimsAsync(user)).ToList();
            List<string> userRoles = (await _userManager.GetRolesAsync(user)).ToList();

            foreach (string userRole in userRoles)
            {
                Role role = await _roleManager.FindByNameAsync(userRole);
                if (role != null)
                {
                    IList<Claim> roleClaims = await _roleManager.GetClaimsAsync(role);
                    foreach (Claim roleClaim in roleClaims)
                    {
                        userClaims.Add(roleClaim);
                    }
                }
            }

            List<string> claims = userClaims.Select(x => x.Value).ToList();
            string? userType = userRoles.Contains("Student") ? "Student" : userRoles.Contains("Staff") ? "Staff" : userRoles.FirstOrDefault();
            string fullName = string.Empty;
                        
            switch (userType)
            {
                case "Staff":
                    {
                        Staff staff = await _staffRepo.GetSingleByAsync(x => x.UserId == user.Id);
                        fullName = $"{staff.LastName} {staff.FirstName}";
                        break;
                    }
                case "Student":
                    {
                        Student student = await _studentRepo.GetSingleByAsync(x => x.UserId == user.Id);
                                                
                        fullName = $"{student.LastName} {student.FirstName}";
                        break;
                    }
            }
            return new LoggedInUserDto { JwtToken = userToken, UserType = userType, FullName = fullName };
        }

        public async Task<UserClaimsResponseDto> CreateUserClaims(string email, string claimType, string claimValue)
        {
            var user = await _userManager.FindByEmailAsync(email.ToString().ToLower());
            if (user == null)
                throw new UserNotFoundException(email);

            Claim claim = new Claim(claimType, claimValue, ClaimValueTypes.String);

            IdentityResult result = await _userManager.AddClaimAsync(user, claim);

            if (result.Succeeded)
                return new UserClaimsResponseDto { ClaimType = claimType, ClaimValue = claimValue };

            var errorMessage = string.Empty;

            if (result.Errors.Any())
            {
                errorMessage = string.Join('\n', result.Errors);
            }

            throw new InvalidOperationException(errorMessage);
        }

        public async Task<string> DeleteClaims(UserClaimsRequestDto request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                throw new UserNotFoundException(request.Email);

            var claim = new Claim(request.ClaimType, request.ClaimValue);

            IdentityResult result = await _userManager.RemoveClaimAsync(user, claim);

            if (result.Succeeded)
                return "User removed from claim successfully";

            var errorMessage = string.Empty;

            if (result.Errors.Any())
            {
                errorMessage = string.Join('\n', result.Errors);
            }

            throw new InvalidOperationException(errorMessage);
        }

        public async Task<EditUserClaimsDto> EditUserClaims(EditUserClaimsDto userClaimsDto)
        {
            var user = await _userManager.FindByEmailAsync(userClaimsDto.Email.ToString().Trim());
            if (user == null)
                throw new UserNotFoundException(userClaimsDto.Email);

            Claim newClaim = new(userClaimsDto.ClaimType.Trim().ToLower(), userClaimsDto.ClaimValue.Trim().ToLower());

            var oldClaim = new Claim(userClaimsDto.ClaimType.Trim().ToLower(), userClaimsDto.OldValue.Trim().ToLower());

            var result = await _userManager.ReplaceClaimAsync(user, oldClaim, newClaim);

            if (result.Succeeded)
                return new EditUserClaimsDto { Email = userClaimsDto.Email, ClaimType = userClaimsDto.ClaimType, ClaimValue = userClaimsDto.ClaimValue, OldValue = userClaimsDto.OldValue };


            var errorMessage = string.Empty;

            if (result.Errors.Any())
                errorMessage = string.Join('\n', result.Errors);
            
            throw new InvalidOperationException(errorMessage);
        }

        public async Task<IEnumerable<UserClaimsResponseDto>> GetUserClaims(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
                throw new UserNotFoundException(email);

            var claim = await _userManager.GetClaimsAsync(user);

            var dto = claim.Select(x => new UserClaimsResponseDto
            {
                ClaimType = x.Type,
                ClaimValue = x.Value
            });

            return dto;
        }

        private async Task<JwtToken> GetTokenAsync(User user)
        {
            var authenticator = _serviceFactory.GetService<IJwtAuthenticator>();
            JwtToken jwt = await authenticator.GenerateJwtToken(user);

            return jwt;
        }
    }
}
