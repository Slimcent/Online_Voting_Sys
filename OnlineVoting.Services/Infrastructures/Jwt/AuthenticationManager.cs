using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OnlineVoting.Models.Dtos.Response.Jwt;
using OnlineVoting.Models.Entities;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace OnlineVoting.Services.Infrastructures.Jwt
{
    public class AuthenticationManager : IAuthenticationManager
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<User> _roleManager;
        private readonly IConfiguration _configuration;
        private User _user;
        public AuthenticationManager(UserManager<User> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<string> CreateToken()
        {
            var signingCredentials = GetSigningCredentials();
            var claims = await GetRoleClaims();

            var tokenOptions = GenerateTokenOptions(signingCredentials, claims);

            return new JwtSecurityTokenHandler().WriteToken(tokenOptions);
        }

        private JwtSecurityToken GenerateTokenOptions(SigningCredentials signingCredentials, List<Claim> claims)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            IdentityOptions _options = new();
                        
            var tokenOptions = new JwtSecurityToken
            (
                issuer: jwtSettings.GetSection("validIssuer").Value,
                audience: jwtSettings.GetSection("validAudience").Value,
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(jwtSettings.GetSection("Expires").Value)),
                
                signingCredentials: signingCredentials
            );

            return tokenOptions;
        }

        private SigningCredentials GetSigningCredentials()
        {
            //var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);

            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = Encoding.UTF8.GetBytes(jwtSettings.GetSection("Secret").Value);
            var secret = new SymmetricSecurityKey(key);

            return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
        }

        public async Task<List<Claim>> GetRoleClaims()
        {
            List<Claim> claims = new();      // Declared an empty list of claims

            var userClaims = await _userManager.GetClaimsAsync(_user);      // Get all the claims associated to a user

            claims.AddRange(userClaims);      // Add those claims to the empty list of claims declared above

            var userRoles = await _userManager.GetRolesAsync(_user);     // Get all the roles associated to a user

            foreach (var userRole in userRoles)       // Looping through all the roles of the user
            {
                claims.Add(new Claim(ClaimTypes.Role, userRole));        // Adding userRole to the list of claims declared above

                var role = await _roleManager.FindByNameAsync(userRole);      // Get the name of the role in the list of the userRoles above
                if (role != null)
                {
                    var roleClaims = await _roleManager.GetClaimsAsync(role);      // Get all the claims associated to a role

                    foreach (Claim roleClaim in roleClaims)           // Looping through those claims from a particalar role
                    {
                        claims.Add(roleClaim);           // Adding those claims from a role to a user
                    }
                }
            }
            return claims;
        }
    }
}
