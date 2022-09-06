using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Entities;
using OnlineVoting.Services.Exceptions;
using OnlineVoting.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using VotingSystem.Data.Interfaces;

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

        public async Task<UserClaimsResponseDto> CreateUserClaims(string email, string claimType, string claimValue)
        {
            var user = await _userManager.FindByEmailAsync(email.ToString().ToLower());
            if (user == null)
                throw new UserNotFoundException(email);

            System.Security.Claims.Claim claim = new System.Security.Claims.Claim(claimType, claimValue, ClaimValueTypes.String);

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

            var claim = new System.Security.Claims.Claim(request.ClaimType, request.ClaimValue);

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

            System.Security.Claims.Claim newClaim = new(userClaimsDto.ClaimType.Trim().ToLower(), userClaimsDto.ClaimValue.Trim().ToLower());

            var oldClaim = new System.Security.Claims.Claim(userClaimsDto.ClaimType.Trim().ToLower(), userClaimsDto.OldValue.Trim().ToLower());

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
    }
}
