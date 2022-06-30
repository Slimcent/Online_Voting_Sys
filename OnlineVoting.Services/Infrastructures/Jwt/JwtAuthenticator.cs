using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OnlineVoting.Models.Dtos.Response.Jwt;
using OnlineVoting.Models.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OnlineVoting.Services.Infrastructures.Jwt
{
    public class JwtAuthenticator: IJwtAuthenticator
    {
        private readonly IConfiguration _configuration;
        UserManager<User> _userManager { get; }
        RoleManager<Role> _roleManager { get; }


        public JwtAuthenticator(UserManager<User> userManager, RoleManager<Role> roleManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }


        public async Task<JwtToken> GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            JwtSecurityTokenHandler jwtTokenHandler = new();
            //var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);
            var key = Encoding.UTF8.GetBytes(jwtSettings.GetSection("Secret").Value);
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
                //Expires = DateTime.UtcNow.AddHours(double.Parse(_jwtSettings.Expires)),
                Expires = DateTime.Now.AddMinutes(Convert.ToDouble(jwtSettings.GetSection("Expires").Value)),
                Issuer = jwtSettings.GetSection("validIssuer").Value,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = jwtTokenHandler.WriteToken(token);

            return new JwtToken
            {
                Token = jwtToken,
                IssuedAt = DateTime.UtcNow,
                Expires = tokenDescriptor.Expires,
                Issuer = tokenDescriptor.Issuer,
            };
        }

        public async Task<List<Claim>> GetRoleClaims(User user)
        {
            List<Claim> claims = new();      // Declared an empty list of claims

            var userClaims = await _userManager.GetClaimsAsync(user);      // Get all the claims associated to a user

            claims.AddRange(userClaims);      // Add those claims to the empty list of claims declared above

            var userRoles = await _userManager.GetRolesAsync(user);     // Get all the roles associated to a user

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
