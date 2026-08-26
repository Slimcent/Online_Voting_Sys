using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Results;
using OnlineVoting.Services.Interfaces;
using System.Security.Claims;
using VotingSystem.Logger;

namespace OnlineVoting.Services.Implementation
{
    public class ClaimsService : IClaimsService
    {
        private readonly UserManager<User> _userManager;
        private readonly ILoggerMessage _loggerMessage;

        public ClaimsService(IServiceFactory serviceFactory)
        {
            _userManager = serviceFactory.GetService<UserManager<User>>();
            _loggerMessage = serviceFactory.GetService<ILoggerMessage>();
        }

        public async Task<List<string>> GetRouteNames(string baseUrl)
        {
            _loggerMessage.LogInfo("Route names request received.");

            List<string> operationIds = new();

            using (HttpClient client = new())
            {
                client.BaseAddress = new Uri($"{baseUrl}");
                client.DefaultRequestHeaders.Accept.Clear();

                HttpResponseMessage response = await client.GetAsync("/swagger/v1/swagger.json");

                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    dynamic routePaths = JsonConvert.DeserializeObject<dynamic>(content).paths;

                    foreach (dynamic route in routePaths)
                    {
                        dynamic operationGet = route.First?.get?.operationId?.ToString() ?? string.Empty;
                        dynamic operationPost = route.First?.post?.operationId?.ToString() ?? string.Empty;
                        dynamic operationPut = route.First?.put?.operationId?.ToString() ?? string.Empty;
                        dynamic operationDelete = route.First?.delete?.operationId?.ToString() ?? string.Empty;
                        dynamic operationPatch = route.First?.patch?.operationId?.ToString() ?? string.Empty;

                        if (!string.IsNullOrEmpty(operationGet))
                        {
                            operationIds.Add(operationGet);
                        }
                        if (!string.IsNullOrEmpty(operationPost))
                        {
                            operationIds.Add(operationPost);
                        }
                        if (!string.IsNullOrEmpty(operationPut))
                        {
                            operationIds.Add(operationPut);
                        }
                        if (!string.IsNullOrEmpty(operationDelete))
                        {
                            operationIds.Add(operationDelete);
                        }
                        if (!string.IsNullOrEmpty(operationPatch))
                        {
                            operationIds.Add(operationPatch);
                        }
                    }

                    _loggerMessage.LogInfo($"{operationIds.Count} route names found.");
                }
                else
                {
                    _loggerMessage.LogWarn("Route names request failed because the Swagger document could not be retrieved.");
                }
            }

            return operationIds;
        }

        public async Task<Result<UserClaimsResponse>> CreateUserClaims(UserClaimsRequest request)
        {
            _loggerMessage.LogInfo($"User claim creation request received for email {request.Email}.");

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                _loggerMessage.LogWarn("User claim creation failed because the email was empty.");

                return Result<UserClaimsResponse>.ValidationError("Email cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(request.ClaimType))
            {
                _loggerMessage.LogWarn("User claim creation failed because the claim type was empty.");

                return Result<UserClaimsResponse>.ValidationError("Claim type cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(request.ClaimValue))
            {
                _loggerMessage.LogWarn("User claim creation failed because the claim value was empty.");

                return Result<UserClaimsResponse>.ValidationError("Claim value cannot be empty");
            }

            string email = request.Email.Trim();
            string claimType = request.ClaimType.Trim();
            string claimValue = request.ClaimValue.Trim();

            User user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                _loggerMessage.LogWarn($"User claim creation failed because user with email {email} was not found.");

                return Result<UserClaimsResponse>.NotFound($"User with email {request.Email} was not found");
            }

            Claim claim = new Claim(claimType, claimValue, ClaimValueTypes.String);

            IList<Claim> existingClaims = await _userManager.GetClaimsAsync(user);
            bool claimExists = existingClaims.Any(x => x.Type == claimType && x.Value == claimValue);

            if (claimExists)
            {
                _loggerMessage.LogWarn($"User claim creation skipped because user {user.Id} already has the claim.");

                return Result<UserClaimsResponse>.Conflict("The user already has this claim");
            }

            IdentityResult result = await _userManager.AddClaimAsync(user, claim);

            if (!result.Succeeded)
            {
                string errorMessage = string.Join("\n", result.Errors.Select(x => x.Description));

                _loggerMessage.LogWarn($"User claim creation failed for user {user.Id}.");

                return Result<UserClaimsResponse>.ValidationError(errorMessage);
            }

            UserClaimsResponse response = new UserClaimsResponse
            {
                ClaimType = claimType,
                ClaimValue = claimValue
            };

            _loggerMessage.LogInfo($"Claim created successfully for user {user.Id}.");

            return Result<UserClaimsResponse>.Created(response);
        }

        public async Task<Result<string>> DeleteClaims(UserClaimsRequest request)
        {
            _loggerMessage.LogInfo($"User claim deletion request received for email {request.Email}.");

            string email = request.Email.Trim();
            string claimType = request.ClaimType.Trim();
            string claimValue = request.ClaimValue.Trim();

            User user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                _loggerMessage.LogWarn($"User claim deletion failed because user with email {email} was not found.");

                return Result<string>.NotFound($"User with email {request.Email} was not found");
            }

            Claim claim = new Claim(claimType, claimValue);

            IList<Claim> existingClaims = await _userManager.GetClaimsAsync(user);
            bool claimExists = existingClaims.Any(x => x.Type == claimType && x.Value == claimValue);

            if (!claimExists)
            {
                _loggerMessage.LogWarn($"User claim deletion failed because the claim was not found for user {user.Id}.");

                return Result<string>.NotFound("The claim was not found for this user");
            }

            IdentityResult result = await _userManager.RemoveClaimAsync(user, claim);

            if (!result.Succeeded)
            {
                string errorMessage = string.Join("\n", result.Errors.Select(x => x.Description));

                _loggerMessage.LogWarn($"User claim deletion failed for user {user.Id}.");

                return Result<string>.ValidationError(errorMessage);
            }

            _loggerMessage.LogInfo($"Claim removed successfully from user {user.Id}.");

            return Result<string>.Success("User removed from claim successfully");
        }

        public async Task<Result<UserClaimsResponse>> EditUserClaims(UserClaimsRequest request)
        {
            _loggerMessage.LogInfo($"User claim update request received for email {request.Email}.");

            string email = request.Email.Trim();

            User user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                _loggerMessage.LogWarn($"User claim update failed because user with email {email} was not found.");

                return Result<UserClaimsResponse>.NotFound($"User with email {request.Email} was not found");
            }

            if (string.IsNullOrWhiteSpace(request.OldValue))
            {
                _loggerMessage.LogWarn($"User claim update failed because the old claim value was empty for user {user.Id}.");

                return Result<UserClaimsResponse>.ValidationError("Old claim value cannot be empty.");
            }

            string claimType = request.ClaimType.Trim();
            string claimValue = request.ClaimValue.Trim();
            string oldValue = request.OldValue.Trim();

            Claim newClaim = new Claim(claimType, claimValue);

            Claim oldClaim = new Claim(claimType, oldValue);

            IList<Claim> existingClaims = await _userManager.GetClaimsAsync(user);
            bool oldClaimExists = existingClaims.Any(x => x.Type == oldClaim.Type && x.Value == oldClaim.Value);

            if (!oldClaimExists)
            {
                _loggerMessage.LogWarn($"User claim update failed because the existing claim was not found for user {user.Id}.");

                return Result<UserClaimsResponse>.NotFound("The claim to edit was not found for this user");
            }

            bool newClaimExists = existingClaims.Any(x => x.Type == newClaim.Type && x.Value == newClaim.Value);

            if (newClaimExists)
            {
                _loggerMessage.LogWarn($"User claim update failed because user {user.Id} already has the new claim.");

                return Result<UserClaimsResponse>.Conflict("The user already has the new claim");
            }

            IdentityResult result = await _userManager.ReplaceClaimAsync(user, oldClaim, newClaim);

            if (!result.Succeeded)
            {
                string errorMessage = string.Join("\n", result.Errors.Select(x => x.Description));

                _loggerMessage.LogWarn($"User claim update failed for user {user.Id}.");

                return Result<UserClaimsResponse>.ValidationError(errorMessage);
            }

            UserClaimsResponse response = new UserClaimsResponse
            {
                Email = email,
                ClaimType = claimType,
                ClaimValue = claimValue,
                OldValue = oldValue
            };

            _loggerMessage.LogInfo($"Claim updated successfully for user {user.Id}.");

            return Result<UserClaimsResponse>.Success(response);
        }

        public async Task<Result<IEnumerable<UserClaimsResponse>>> GetUserClaims(string email)
        {
            _loggerMessage.LogInfo($"User claims request received for email {email}.");

            string userEmail = email.Trim();

            User user = await _userManager.FindByEmailAsync(userEmail);

            if (user == null)
            {
                _loggerMessage.LogWarn($"User claims request failed because user with email {userEmail} was not found.");

                return Result<IEnumerable<UserClaimsResponse>>.NotFound($"User with email {email} was not found");
            }

            IList<Claim> claims = await _userManager.GetClaimsAsync(user);

            IEnumerable<UserClaimsResponse> response = claims.Select(x => new UserClaimsResponse
            {
                ClaimType = x.Type,
                ClaimValue = x.Value
            });

            _loggerMessage.LogInfo($"{claims.Count} claims found for user {user.Id}.");

            return Result<IEnumerable<UserClaimsResponse>>.Success(response);
        }
    }
}