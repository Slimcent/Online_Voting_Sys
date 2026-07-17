using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using OnlineVoting.Models.Dtos.Response.Jwt;
using OnlineVoting.Models.Entities;
using OnlineVoting.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OnlineVoting.Services.Infrastructures.Authorization.Jwt
{
    public class JwtAuthenticator : IJwtAuthenticator
    {
        private readonly JwtSettings _jwtSettings;

        public JwtAuthenticator(JwtSettings jwtSettings)
        {
            _jwtSettings = jwtSettings;
        }

        public async Task<JwtToken> GenerateJwtToken(User user, string role, string expires = null,
            List<Claim> additionalClaims = null)
        {
            JwtSecurityTokenHandler jwtTokenHandler = new();

            var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);

            IdentityOptions _options = new();

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
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
                    ? DateTime.UtcNow.AddHours(double.Parse(_jwtSettings.Expires))
                    : DateTime.UtcNow.AddMinutes(double.Parse(expires)),
                SigningCredentials =
                    new SigningCredentials(
                        new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = jwtTokenHandler.WriteToken(token);

            return new JwtToken()
            {
                Token = jwtToken,
                IssuedAt = DateTime.UtcNow,
                Issuer = tokenDescriptor.Issuer,
                Expires = tokenDescriptor.Expires
            };
        }
    }
}