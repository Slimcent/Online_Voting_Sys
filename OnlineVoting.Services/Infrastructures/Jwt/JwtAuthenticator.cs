using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OnlineVoting.Models.Dtos.Response.Jwt;
using OnlineVoting.Models.Entities;
using OnlineVoting.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OnlineVoting.Services.Infrastructures.Jwt
{
    public class JwtAuthenticator: IJwtAuthenticator
    {
        private readonly IConfiguration _configuration;
        
        public JwtAuthenticator(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<JwtToken> GenerateJwtToken(User user, string role, string expires = null, List<Claim> additionalClaims = null)
        {
            JwtSecurityTokenHandler jwtTokenHandler = new();

            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = Encoding.UTF8.GetBytes(jwtSettings.GetSection("secret").Value);

            IdentityOptions _options = new();

            var claims = new List<Claim>
            {
                new Claim("Id", user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(_options.ClaimsIdentity.UserIdClaimType, user.Id),
                new Claim(_options.ClaimsIdentity.UserNameClaimType, user.UserName),
                new Claim(ClaimTypes.Role, role)
            };

            if (additionalClaims != null)
            {
                claims.AddRange(additionalClaims);
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = string.IsNullOrWhiteSpace(expires)
                    ? DateTime.Now.AddHours(double.Parse(jwtSettings.GetSection("expires").Value))
                    : DateTime.Now.AddMinutes(double.Parse(expires)),
                SigningCredentials =
                    new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = jwtSettings.GetSection("validIssuer").Value,
                Audience = jwtSettings.GetSection("validAudience").Value,
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = jwtTokenHandler.WriteToken(token);

            return new JwtToken()
            {
                Token = jwtToken,
                IssuedAt = DateTime.Now,
                Issuer = tokenDescriptor.Issuer,
                Expires = tokenDescriptor.Expires
            };
        }
    }
}
