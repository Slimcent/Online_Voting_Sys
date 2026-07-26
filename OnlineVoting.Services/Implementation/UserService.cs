using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Request.Email;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Dtos.Response.Jwt;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Results;
using OnlineVoting.Services.Exceptions;
using OnlineVoting.Services.Extension;
using OnlineVoting.Services.Infrastructures;
using OnlineVoting.Services.Interfaces;
using SchMgr_FUTO.Data.Interfaces;
using System.Security.Claims;
using VotingSystem.Data.Interfaces;
using VotingSystem.Logger;

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
        private readonly ILoggerMessage _loggerMessage;

        public UserService(IServiceFactory serviceFactory)
        {
            _serviceFactory = serviceFactory;
            _unitOfWork = serviceFactory.GetService<IUnitOfWork>();
            _userManager = serviceFactory.GetService<UserManager<User>>();
            _roleManager = serviceFactory.GetService<RoleManager<Role>>();
            _studentRepo = _unitOfWork.GetRepository<Student>();
            _staffRepo = _unitOfWork.GetRepository<Staff>();
            _userRepo = _unitOfWork.GetRepository<User>();
            _mapper = _serviceFactory.GetService<IMapper>();
            _loggerMessage = _serviceFactory.GetService<ILoggerMessage>();
        }

        public async Task<Result<string>> CreateUser(CreateUserRequest request)
        {
            if (request is null)
                return Result<string>.ValidationError("Invalid data sent.");

            User? existingUser = await _userManager.FindByEmailAsync(request.Email.Trim().ToLower());

            if (existingUser is not null)
                return Result<string>.Conflict($"A user with email {request.Email} already exists.");

            User user = _mapper.Map<User>(request);

            string password = AuthExtension.GenerateRandomPassword();

            IdentityResult result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                string errorMessage = string.Join("\n", result.Errors.Select(error => error.Description));

                return Result<string>.ValidationError(errorMessage);
            }

            AddUserToRoleRequest userRole = new()
            {
                Email = user.Email,
                Name = request.Role
            };

            await _serviceFactory.GetService<IRolesService>().AddUserToRole(userRole);

            UserMailDto userMailDto = new()
            {
                User = user,
                FirstName = request.FirstName
            };

            await _serviceFactory.GetService<IEmailService>().SendCreateUserEmail(userMailDto);

            return Result<string>.Created(user.Id);
        }

        public async Task<Result<LoggedInUserResponse>> UserLogin(LoginRequest request)
        {
            _loggerMessage.LogInfo($"Login attempt received for email {request.Email}.");

            User? user = await _userRepo.GetSingleByAsync(user => user.UserName == request.Email.ToLower().Trim(),
                include: user => user.Include(item => item.UserType));

            if (user is null)
            {
                _loggerMessage.LogWarn($"Login failed because no user exists for email {request.Email}.");

                return Result<LoggedInUserResponse>.Unauthorized("Invalid email or password.");
            }

            if (!user.Active)
                return Result<LoggedInUserResponse>.Forbidden("Account is not active. Contact the administrator.");

            bool passwordIsValid = await _userManager.CheckPasswordAsync(user, request.Password);

            if (!passwordIsValid)
            {
                _loggerMessage.LogWarn($"Login failed because invalid credentials were provided for user {user.Id}.");

                return Result<LoggedInUserResponse>.Unauthorized("Invalid email or password.");
            }

            List<string> allUserRoles = (await _userManager.GetRolesAsync(user)).ToList();

            string? userRole = allUserRoles.FirstOrDefault();

            JwtToken userToken = await GetTokenAsync(user, userRole);

            List<Claim> userClaims = (await _userManager.GetClaimsAsync(user)).ToList();

            List<string> userRoles = (await _userManager.GetRolesAsync(user)).ToList();

            foreach (string roleName in userRoles)
            {
                Role? role = await _roleManager.FindByNameAsync(roleName);

                if (role is not null)
                {
                    IList<Claim> roleClaims = await _roleManager.GetClaimsAsync(role);

                    foreach (Claim roleClaim in roleClaims)
                    {
                        userClaims.Add(roleClaim);
                    }
                }
            }

            List<string> claims = userClaims.Select(claim => claim.Value).ToList();

            int userType = user.UserTypeId;

            string fullName = $"{user.FirstName} {user.LastName}";

            //switch (userType)
            //{
            //    case "Official":
            //    {
            //        Staff staff = await _staffRepo.GetSingleByAsync(x => x.UserId == user.Id);

            //        fullName = $"{staff?.LastName} {staff?.FirstName}";
            //        break;
            //    }
            //    case "Student":
            //    {
            //        Student student = await _studentRepo.GetSingleByAsync(x => x.UserId == user.Id);

            //        fullName = $"{student.LastName} {student.FirstName}";
            //        break;
            //    }
            //}

            LoggedInUserResponse response = new()
            {
                JwtToken = userToken,
                UserType = user.UserType?.Name,
                FullName = fullName
            };

            return Result<LoggedInUserResponse>.Success(response);
        }

        public async Task<Result<string>> VerifyUser(VerifyAccountRequest request)
        {
            string username = MessageEncoder.DecodeString(request.Email);

            string emailConfirmationToken = MessageEncoder.DecodeString(request.EmailConfirmationToken);

            string resetPasswordToken = MessageEncoder.DecodeString(request.ResetPasswordToken);

            User? user = await _userManager.FindByNameAsync(username);

            if (user is null)
                return Result<string>.NotFound("User was not found.");

            bool emailTokenIsValid = await _userManager.VerifyUserTokenAsync(user, _userManager.Options.Tokens.EmailConfirmationTokenProvider,
                "EmailConfirmation", emailConfirmationToken);

            if (!emailTokenIsValid)
                return Result<string>.ValidationError("Invalid email confirmation token.");

            bool passwordTokenIsValid = await _userManager.VerifyUserTokenAsync(user, _userManager.Options.Tokens.PasswordResetTokenProvider,
                "ResetPassword", resetPasswordToken);

            if (!passwordTokenIsValid)
                return Result<string>.ValidationError("Invalid password reset token.");

            IdentityResult emailResult = await _userManager.ConfirmEmailAsync(user, emailConfirmationToken);

            IdentityResult passwordResult = await _userManager.ResetPasswordAsync(user, resetPasswordToken, request.NewPassword);

            if (emailResult.Succeeded && passwordResult.Succeeded)
            {
                user.Active = true;

                await _userManager.UpdateAsync(user);

                return Result<string>.Success("Password reset was successful.");
            }

            string errorMessage = string.Join("\n", emailResult.Errors.Select(error => error.Description))
                + string.Join("\n", passwordResult.Errors.Select(error => error.Description));

            return Result<string>.ValidationError(errorMessage);
        }

        public async Task<Result<string>> ResetPassword(ResetPasswordRequest request)
        {
            string decodedEmail = MessageEncoder.DecodeString(request.Email);
            string decodedToken = MessageEncoder.DecodeString(request.ResetPasswordToken);

            User user = await _userManager.FindByEmailAsync(decodedEmail);

            if (user == null)
                return Result<string>.ValidationError("Invalid email");

            if (!await _userManager.VerifyUserTokenAsync(user, _userManager.Options.Tokens.PasswordResetTokenProvider, "ResetPassword", decodedToken))
                return Result<string>.ValidationError("Invalid Authentication Token");

            IdentityResult result = await _userManager.ResetPasswordAsync(user, decodedToken, request.NewPassword);

            if (result.Succeeded)
                return Result<string>.Success("Password reset was successful");

            string errorMessage = string.Join("\n", result.Errors.Select(e => e.Description).ToList());

            return Result<string>.ValidationError(errorMessage);
        }

        public async Task<Result<string>> ChangePassword(string userId, ChangePasswordRequest request)
        {
            User user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return Result<string>.NotFound("User not found");

            IdentityResult result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

            if (result.Succeeded)
                return Result<string>.Success("Password changed successfully");

            string errorMessage = string.Join("\n", result.Errors.Select(e => e.Description).ToList());

            return Result<string>.ValidationError(errorMessage);
        }

        public async Task<Result<string>> UpdateRecoveryEmail(string userId, string email)
        {

            User user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return Result<string>.NotFound("User not found");

            if (email == user.Email)
                return Result<string>.ValidationError("Recovery email cannot be the same as your email");

            user.RecoveryEmail = email;
            await _userManager.UpdateAsync(user);

            return Result<string>.Success("Recovery email updated successfully");
        }

        public async Task<Result<string>> ChangeEmail(string userId, ChangeEmailRequestDto request)
        {
            string decodedNewEmail = MessageEncoder.DecodeString(request.NewEmail);
            string decodedToken = MessageEncoder.DecodeString(request.Token);

            User user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return Result<string>.NotFound("User not found");

            IdentityResult result = await SaveChangedEmail(user, decodedNewEmail, decodedToken);

            if (result.Succeeded)
            {
                return Result<string>.Success("Email changed successfully");
            }

            string errorMessage = string.Join("\n", result.Errors.Select(e => e.Description).ToList());
            return Result<string>.ValidationError(errorMessage);
        }

        private async Task<IdentityResult> SaveChangedEmail(User user, string decodedNewEmail, string decodedToken)
        {
            //var rse = await _userManager.ChangeEmailAsync(user, decodedNewEmail, decodedToken);

            IdentityResult result = await _userManager.ChangeEmailAsync(user, decodedNewEmail, decodedToken);

            if (!result.Succeeded)
            {
                return result;
            }

            await _userManager.UpdateNormalizedEmailAsync(user);

            user.UserName = decodedNewEmail;

            await _userManager.UpdateNormalizedUserNameAsync(user);

            await _unitOfWork.SaveChangesAsync();

            return result;
        }

        private async Task<JwtToken> GetTokenAsync(User user, string role)
        {
            var authenticator = _serviceFactory.GetService<IJwtAuthenticator>();
            JwtToken jwt = await authenticator.GenerateJwtToken(user, role);

            return jwt;
        }
    }
}