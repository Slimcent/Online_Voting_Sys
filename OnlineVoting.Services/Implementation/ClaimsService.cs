using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Results;
using OnlineVoting.Services.Interfaces;
using System.Security.Claims;
using OnlineVoting.Data.Interfaces;

namespace OnlineVoting.Services.Implementation
{
    public class ClaimsService : IClaimsService
    {
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        private readonly IServiceFactory _serviceFactory;
        private readonly IUnitOfWork _unitOfWork;

        public ClaimsService(IServiceFactory serviceFactory)
        {
            _userManager = serviceFactory.GetService<UserManager<User>>();
            _serviceFactory = serviceFactory;
            _unitOfWork = _serviceFactory.GetService<IUnitOfWork>();
            _mapper = _serviceFactory.GetService<IMapper>();
        }

        public async Task<List<string>> GetRouteNames(string baseUrl)
        {
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
                }
                else
                {
                    Console.WriteLine("Internal server Error");
                }
            }

            return operationIds;
        }

        public async Task<Result<UserClaimsResponse>> CreateUserClaims(string email, string claimType, string claimValue)
        {
            if (string.IsNullOrWhiteSpace(email))
                return Result<UserClaimsResponse>.ValidationError("Email cannot be empty");

            if (string.IsNullOrWhiteSpace(claimType))
                return Result<UserClaimsResponse>.ValidationError("Claim type cannot be empty");

            if (string.IsNullOrWhiteSpace(claimValue))
                return Result<UserClaimsResponse>.ValidationError("Claim value cannot be empty");

            User user = await _userManager.FindByEmailAsync(email.Trim().ToLower());
            if (user == null)
                return Result<UserClaimsResponse>.NotFound($"User with email {email} was not found");

            Claim claim = new Claim(claimType, claimValue, ClaimValueTypes.String);

            IList<Claim> existingClaims = await _userManager.GetClaimsAsync(user);
            bool claimExists = existingClaims.Any(x => x.Type == claimType && x.Value == claimValue);

            if (claimExists)
                return Result<UserClaimsResponse>.Conflict("The user already has this claim");

            IdentityResult result = await _userManager.AddClaimAsync(user, claim);

            if (!result.Succeeded)
            {
                string errorMessage = string.Join("\n", result.Errors.Select(x => x.Description));

                return Result<UserClaimsResponse>.ValidationError(errorMessage);
            }

            UserClaimsResponse response = new UserClaimsResponse
            {
                ClaimType = claimType,
                ClaimValue = claimValue
            };

            return Result<UserClaimsResponse>.Created(response);
        }

        public async Task<Result<string>> DeleteClaims(UserClaimsRequest request)
        {
            User user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return Result<string>.NotFound($"User with email {request.Email} was not found");

            Claim claim = new Claim(request.ClaimType, request.ClaimValue);

            IList<Claim> existingClaims = await _userManager.GetClaimsAsync(user);
            bool claimExists = existingClaims.Any(x => x.Type == request.ClaimType && x.Value == request.ClaimValue);

            if (!claimExists)
                return Result<string>.NotFound("The claim was not found for this user");

            IdentityResult result = await _userManager.RemoveClaimAsync(user, claim);

            if (!result.Succeeded)
            {
                string errorMessage = string.Join("\n", result.Errors.Select(x => x.Description));

                return Result<string>.ValidationError(errorMessage);
            }

            return Result<string>.Success("User removed from claim successfully");
        }

        public async Task<Result<EditUserClaimsRequest>> EditUserClaims(EditUserClaimsRequest userClaimsDto)
        {
            User user = await _userManager.FindByEmailAsync(userClaimsDto.Email.Trim());
            if (user == null)
                return Result<EditUserClaimsRequest>.NotFound($"User with email {userClaimsDto.Email} was not found");

            Claim newClaim = new Claim(userClaimsDto.ClaimType.Trim().ToLower(), userClaimsDto.ClaimValue.Trim().ToLower());

            Claim oldClaim = new Claim(userClaimsDto.ClaimType.Trim().ToLower(), userClaimsDto.OldValue.Trim().ToLower());

            IList<Claim> existingClaims = await _userManager.GetClaimsAsync(user);
            bool oldClaimExists = existingClaims.Any(x => x.Type == oldClaim.Type && x.Value == oldClaim.Value);

            if (!oldClaimExists)
                return Result<EditUserClaimsRequest>.NotFound("The claim to edit was not found for this user");

            bool newClaimExists = existingClaims.Any(x => x.Type == newClaim.Type && x.Value == newClaim.Value);

            if (newClaimExists)
                return Result<EditUserClaimsRequest>.Conflict("The user already has the new claim");

            IdentityResult result = await _userManager.ReplaceClaimAsync(user, oldClaim, newClaim);

            if (!result.Succeeded)
            {
                string errorMessage = string.Join("\n", result.Errors.Select(x => x.Description));

                return Result<EditUserClaimsRequest>.ValidationError(errorMessage);
            }

            EditUserClaimsRequest response = new EditUserClaimsRequest
            {
                Email = userClaimsDto.Email,
                ClaimType = userClaimsDto.ClaimType,
                ClaimValue = userClaimsDto.ClaimValue,
                OldValue = userClaimsDto.OldValue
            };

            return Result<EditUserClaimsRequest>.Success(response);
        }

        public async Task<Result<IEnumerable<UserClaimsResponse>>> GetUserClaims(string email)
        {
            User user = await _userManager.FindByEmailAsync(email);

            if (user == null)
                return Result<IEnumerable<UserClaimsResponse>>.NotFound($"User with email {email} was not found");

            IList<Claim> claims = await _userManager.GetClaimsAsync(user);

            IEnumerable<UserClaimsResponse> response = claims.Select(x => new UserClaimsResponse
            {
                ClaimType = x.Type,
                ClaimValue = x.Value
            });

            return Result<IEnumerable<UserClaimsResponse>>.Success(response);
        }
    }
}