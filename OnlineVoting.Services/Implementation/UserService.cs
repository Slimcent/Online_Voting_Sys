using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlineVoting.BackgroundTasks.Interfaces;
using OnlineVoting.Data.Interfaces;
using OnlineVoting.Models.Constants;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Request.Email;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Dtos.Response.Jwt;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Results;
using OnlineVoting.Services.BackgroundTasks;
using OnlineVoting.Services.Extension;
using OnlineVoting.Services.Infrastructures;
using OnlineVoting.Services.Interfaces;
using System.Security.Claims;
using VotingSystem.Logger;

namespace OnlineVoting.Services.Implementation
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IRepository<User> _userRepo;
        private readonly IRepository<Role> _roleRepo;
        private readonly IRepository<Student> _studentRepo;
        private readonly IRepository<Staff> _staffRepo;
        private readonly IRepository<Address> _addressRepo;
        private readonly IRepository<RegisteredVoter> _registeredVoterRepo;
        private readonly IMapper _mapper;
        private readonly IServiceFactory _serviceFactory;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggerMessage _loggerMessage;

        public UserService(IServiceFactory serviceFactory)
        {
            _serviceFactory = serviceFactory;
            _unitOfWork = serviceFactory.GetService<IUnitOfWork>();
            _userManager = serviceFactory.GetService<UserManager<User>>();
            _signInManager = serviceFactory.GetService<SignInManager<User>>();
            _roleManager = serviceFactory.GetService<RoleManager<Role>>();
            _studentRepo = _unitOfWork.GetRepository<Student>();
            _staffRepo = _unitOfWork.GetRepository<Staff>();
            _userRepo = _unitOfWork.GetRepository<User>();
            _addressRepo = _unitOfWork.GetRepository<Address>();
            _registeredVoterRepo = _unitOfWork.GetRepository<RegisteredVoter>();
            _mapper = _serviceFactory.GetService<IMapper>();
            _loggerMessage = _serviceFactory.GetService<ILoggerMessage>();
        }

        public async Task<Result<string>> CreateUser(CreateUserRequest request, Action<User>? configureUser = null)
        {
            if (request is null)
            {
                _loggerMessage.LogWarn("User creation rejected because the request was null.");

                return Result<string>.ValidationError("Invalid data sent.");
            }

            _loggerMessage.LogInfo($"Starting user creation for user type {request.UserTypeId}.");

            User? existingUser = await _userManager.FindByEmailAsync(request.Email.Trim().ToLower());

            if (existingUser is not null)
            {
                _loggerMessage.LogWarn("User creation rejected because a user with the supplied email already exists.");

                return Result<string>.Conflict($"A user with email {request.Email} already exists.");
            }

            await _serviceFactory.GetService<IRolesService>().ValidateRoleById(request.RoleId);

            User user = _mapper.Map<User>(request);
                        
            configureUser?.Invoke(user);

            user.UserRoles = new List<ApplicationUserRole>
            {
                new ApplicationUserRole
                {
                    RoleId = request.RoleId,
                }
            };

            PasswordOptions passwordOptions = _userManager.Options.Password;
            string password = AuthExtension.GenerateRandomPassword(passwordOptions);

            IdentityResult result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                string errorMessage = string.Join("\n", result.Errors.Select(error => error.Description));

                _loggerMessage.LogError($"Identity user creation failed. Errors: {errorMessage}");

                return Result<string>.ValidationError(errorMessage);
            }

             _loggerMessage.LogInfo($"User {user.Email} created successfully.");

            try
            {
                CreateUserEmailRequest emailRequest = _mapper.Map<CreateUserEmailRequest>(user);

                emailRequest.EmailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                emailRequest.ResetPasswordToken = await _userManager.GeneratePasswordResetTokenAsync(user);

                _serviceFactory.GetService<IBackgroundTaskQueue>().Enqueue<SendCreateUserEmailTask, CreateUserEmailRequest>(emailRequest);
            }
            catch (Exception exception)
            {
                _loggerMessage.LogError($"Create user email could not be queued for user {user.Id}. {exception.Message}");
            }

            _loggerMessage.LogInfo($"User creation completed successfully for user {user.Email}.");

            return Result<string>.Created(user.Email);
        }

        public async Task<Result<string>> CreateUsers<TRequest>(IEnumerable<TRequest> requests, Action<TRequest, User>? configureUser = null) where TRequest : CreateUserRequest
        {
            if (requests == null)
            {
                _loggerMessage.LogWarn("Bulk user creation rejected because the request was null.");

                return Result<string>.ValidationError("Invalid data sent.");
            }

            List<TRequest> userRequests = requests.ToList();

            if (!userRequests.Any())
            {
                _loggerMessage.LogWarn("Bulk user creation rejected because no users were supplied.");

                return Result<string>.ValidationError("No users were supplied.");
            }

            int userCount = userRequests.Count;

            _loggerMessage.LogInfo($"Starting bulk creation of {userCount} users.");

            List<string> normalizedEmails = userRequests
                .Select(request => _userManager.NormalizeEmail(request.Email.Trim()))
                .ToList();

            string? duplicateEmail = normalizedEmails
                .GroupBy(email => email, StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .FirstOrDefault();

            if (duplicateEmail != null)
            {
                _loggerMessage.LogWarn($"Bulk user creation rejected because email {duplicateEmail} occurs more than once.");

                return Result<string>.Conflict($"Email {duplicateEmail} occurs more than once in the upload.");
            }

            List<string> existingEmails = await _userManager.Users
                .AsNoTracking()
                .Where(user => user.NormalizedEmail != null && normalizedEmails.Contains(user.NormalizedEmail))
                .Select(user => user.NormalizedEmail!)
                .ToListAsync();

            if (existingEmails.Any())
            {
                _loggerMessage.LogWarn($"Bulk user creation rejected because {existingEmails.Count} users already exist.");

                return Result<string>.Conflict("One or more users in the upload already exist.");
            }

            List<string> roleIds = userRequests.Select(request => request.RoleId)
                .Distinct()
                .ToList();

            List<string> existingRoleIds = await _roleManager.Roles.AsNoTracking()
                .Where(role => roleIds.Contains(role.Id))
                .Select(role => role.Id)
                .ToListAsync();

            if (existingRoleIds.Count != roleIds.Count)
            {
                _loggerMessage.LogWarn("Bulk user creation rejected because one or more roles were not found.");

                return Result<string>.ValidationError("One or more specified roles do not exist.");
            }

            PasswordOptions passwordOptions = _userManager.Options.Password;

            List<User> users = new(userCount);

            foreach (TRequest request in userRequests)
            {
                User user = _mapper.Map<User>(request);

                string password = AuthExtension.GenerateRandomPassword(passwordOptions);

                foreach (IPasswordValidator<User> passwordValidator in _userManager.PasswordValidators)
                {
                    IdentityResult passwordValidationResult = await passwordValidator.ValidateAsync(_userManager, user, password);

                    if (!passwordValidationResult.Succeeded)
                    {
                        string errorMessage = string.Join("\n", passwordValidationResult.Errors.Select(error => error.Description));

                        _loggerMessage.LogWarn($"Bulk user creation failed password validation. {errorMessage}");

                        return Result<string>.ValidationError(errorMessage);
                    }
                }

                user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, password);
                user.NormalizedEmail = _userManager.NormalizeEmail(user.Email);
                user.NormalizedUserName = _userManager.NormalizeName(user.UserName);

                configureUser?.Invoke(request, user);

                user.UserRoles = new List<ApplicationUserRole>
                {
                    new ApplicationUserRole
                    {
                        RoleId = request.RoleId
                    }
                };

                users.Add(user);
            }

            await _userRepo.AddRangeAsync(users);

            List<CreateUserEmailRequest> emailRequests = _mapper.Map<List<CreateUserEmailRequest>>(users);

            int usersCount = users.Count;

            try
            {
                _serviceFactory.GetService<IBackgroundTaskQueue>().EnqueueRange<SendCreateUserEmailTask, CreateUserEmailRequest>(emailRequests);
            }
            catch (Exception exception)
            {
                _loggerMessage.LogError($"Bulk create user emails could not be queued. {exception.Message}");
            }

            _loggerMessage.LogInfo($"Bulk user creation completed successfully. Created {usersCount} users.");

            return Result<string>.Created($"{usersCount} users created successfully.");
        }

        public async Task<Result<LoggedInUserResponse>> UserLogin(LoginRequest request)
        {
            string normalizedEmail = request.Email.Trim().ToLowerInvariant();

            IAuditTrailService auditTrailService = _serviceFactory.GetService<IAuditTrailService>();

            _loggerMessage.LogInfo($"Login attempt received for email {request.Email}.");

            User? user = await _userRepo.GetSingleByAsync(user => user.Email == normalizedEmail ,
                include: user => user.Include(item => item.UserType));

            if (user is null)
            {
                _loggerMessage.LogWarn($"Login failed because no user exists for email {request.Email}.");

                await auditTrailService.RecordAuthenticationEvent(ApplicationConstants.Audit.Events.LoginFailed, ApplicationConstants.Audit.Outcomes.Failure,
                    ApplicationConstants.Audit.Descriptions.InvalidCredentials, attemptedUsername: normalizedEmail);

                return Result<LoggedInUserResponse>.Unauthorized(ApplicationConstants.Authentication.Messages.InvalidCredentials);
            }

            if (!user.Active)
            {
                await auditTrailService.RecordAuthenticationEvent(ApplicationConstants.Audit.Events.LoginFailed, ApplicationConstants.Audit.Outcomes.Denied,
                    ApplicationConstants.Audit.Descriptions.InactiveAccount, user);

                return Result<LoggedInUserResponse>.Forbidden(ApplicationConstants.Authentication.Messages.InactiveAccount);
            }

            bool wasLockedOut = await _userManager.IsLockedOutAsync(user);
                        
            if (wasLockedOut)
            {
                _loggerMessage.LogWarn($"Login rejected because user {user.Id} is temporarily locked out.");

                await auditTrailService.RecordAuthenticationEvent(ApplicationConstants.Audit.Events.LoginRejectedLocked, ApplicationConstants.Audit.Outcomes.Denied,
                    ApplicationConstants.Audit.Descriptions.LoginRejectedLocked, user);

                return Result<LoggedInUserResponse>.Unauthorized(ApplicationConstants.Authentication.Messages.InvalidCredentials);
            }

            SignInResult signInResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

            if (signInResult.IsLockedOut)
            {
                _loggerMessage.LogWarn($"Account lockout triggered for user {user.Id}.");

                await auditTrailService.RecordAuthenticationEvent(ApplicationConstants.Audit.Events.AccountLocked, ApplicationConstants.Audit.Outcomes.Denied,
                    ApplicationConstants.Audit.Descriptions.AccountLocked, user);

                return Result<LoggedInUserResponse>.Unauthorized(ApplicationConstants.Authentication.Messages.InvalidCredentials);
            }

            if (!signInResult.Succeeded)
            {
                _loggerMessage.LogWarn($"Login failed because invalid credentials were provided for user {user.Id}.");

                await auditTrailService.RecordAuthenticationEvent(ApplicationConstants.Audit.Events.LoginFailed, ApplicationConstants.Audit.Outcomes.Failure, 
                    ApplicationConstants.Audit.Descriptions.InvalidCredentials, user);

                return Result<LoggedInUserResponse>.Unauthorized(ApplicationConstants.Authentication.Messages.InvalidCredentials);
            }

            List<string> allUserRoles = (await _userManager.GetRolesAsync(user)).ToList();

            string? userRole = allUserRoles.FirstOrDefault();

            JwtToken userToken = await GetTokenAsync(user, userRole);

            RefreshTokenContext refreshTokenContext = new()
            {
                UserId = user.Id
            };

            Result<RefreshTokenResponse> refreshTokenResult = await _serviceFactory.GetService<IRefreshTokenService>()
                .CreateRefreshToken(refreshTokenContext);

            if (!refreshTokenResult.IsSuccess)
            {
                _loggerMessage.LogWarn($"Login failed because refresh token could not be created for user {user.Id}.");

                return Result<LoggedInUserResponse>.FromFailure(refreshTokenResult);
            }

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

            LoggedInUserResponse response = new()
            {
                JwtToken = userToken,
                UserType = user.UserType?.Name,
                FullName = fullName
            };

            await auditTrailService.RecordAuthenticationEvent(ApplicationConstants.Audit.Events.LoginSucceeded, ApplicationConstants.Audit.Outcomes.Success,
                ApplicationConstants.Audit.Descriptions.LoginSucceeded, user);

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

        public async Task<int> DeleteUnconfirmedUsers(int retentionDays)
        {
            _loggerMessage.LogInfo($"Starting cleanup of unconfirmed users older than {retentionDays} day(s).");

            if (retentionDays <= 0)
            {
                _loggerMessage.LogWarn("Unconfirmed user cleanup rejected because the retention period is invalid.");

                throw new ArgumentOutOfRangeException(nameof(retentionDays));
            }

            DateTime cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);

            List<User> users = await _userRepo.GetQueryable(x => !x.EmailConfirmed && x.CreatedAt <= cutoffDate).ToListAsync();

            if (users.Count == 0)
            {
                _loggerMessage.LogInfo("No unconfirmed user accounts were found for deletion.");

                return 0;
            }

            List<string> userIds = users.Select(x => x.Id).ToList();

            List<Student> students = await _studentRepo.GetQueryable(x => x.UserId != null && userIds.Contains(x.UserId)).ToListAsync();

            List<Staff> staff = await _staffRepo.GetQueryable(x => x.UserId != null && userIds.Contains(x.UserId)).ToListAsync();

            List<Guid> studentIds = students.Select(x => x.Id).ToList();

            List<Guid> staffIds = staff.Select(x => x.Id).ToList();

            List<Address> addresses = await _addressRepo.GetQueryable(x => (x.StudentId.HasValue && studentIds.Contains(x.StudentId.Value))
                || (x.StaffId.HasValue && staffIds.Contains(x.StaffId.Value))).ToListAsync();

            List<RegisteredVoter> registeredVoters = await _registeredVoterRepo.GetQueryable(x => x.StudentId.HasValue && studentIds.Contains(x.StudentId.Value))
                .ToListAsync();

            _addressRepo.DeleteRange(addresses);
            _registeredVoterRepo.DeleteRange(registeredVoters);
            _studentRepo.DeleteRange(students);
            _staffRepo.DeleteRange(staff);
            _userRepo.DeleteRange(users);

            await _unitOfWork.SaveChangesAsync();

            _loggerMessage.LogInfo($"{users.Count} unconfirmed user account(s) deleted successfully.");

            return users.Count;
        }

        private async Task<JwtToken> GetTokenAsync(User user, string role)
        {
            var authenticator = _serviceFactory.GetService<IJwtAuthenticator>();
            JwtToken jwt = await authenticator.GenerateJwtToken(user, role);

            return jwt;
        }
    }
}