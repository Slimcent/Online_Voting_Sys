using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using OnlineVoting.Models.Dtos.Response.Jwt;
using OnlineVoting.Models.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OnlineVoting.Services.Infrastructures.Jwt
{
    public class JwtAuthentication
    {
        private readonly JwtSettings _jwtSettings;
        UserManager<User> UserManager { get; }
        RoleManager<Role> RoleManager { get; }

        public JwtAuthentication(JwtSettings jwtSettings, UserManager<User> userManager, RoleManager<Role> roleManager)
        {
            _jwtSettings = jwtSettings;
            UserManager = userManager;
            RoleManager = roleManager; 
        }

        public async Task<JwtToken> GenerateJwtToken(User user)
        {
            JwtSecurityTokenHandler jwtTokenHandler = new();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);
            IdentityOptions _options = new();

            List<Claim> userClaims = await GetRoleClaims(user);

            var claims = new List<Claim>
            {
                new Claim("Id", user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(_options.ClaimsIdentity.UserIdClaimType, user.Id.ToString()),
                new Claim(_options.ClaimsIdentity.UserNameClaimType, user.UserName),
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(double.Parse(_jwtSettings.Expires)),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = jwtTokenHandler.WriteToken(token);

            return new JwtToken
            {
                Token = jwtToken,
                Issued = DateTime.UtcNow,
                Expires = tokenDescriptor.Expires
            };
        }

        public async Task<List<Claim>> GetRoleClaims(User user)
        {
            List<Claim> claims = new();      // Declared an empty list of claims

            var userClaims = await UserManager.GetClaimsAsync(user);      // Get all the claims associated to a user

            claims.AddRange(userClaims);      // Add those claims to the empty list of claims declared above
                        
            var userRoles = await UserManager.GetRolesAsync(user);     // Get all the roles associated to a user
                        
            foreach (var userRole in userRoles)       // Looping through all the roles of the user
            {
                claims.Add(new Claim(ClaimTypes.Role, userRole));        // Adding userRole to the list of claims declared above
                                
                var role = await RoleManager.FindByNameAsync(userRole);      // Get the name of the role in the list of the userRoles above
                if (role != null)
                {                    
                    var roleClaims = await RoleManager.GetClaimsAsync(role);      // Get all the claims associated to a role
                                        
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