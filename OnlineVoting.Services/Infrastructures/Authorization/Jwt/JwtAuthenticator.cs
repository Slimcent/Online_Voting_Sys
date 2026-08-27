using Microsoft.IdentityModel.Tokens;
using OnlineVoting.Models.Dtos.Response.Jwt;
using OnlineVoting.Models.Entities;
using OnlineVoting.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using VotingSystem.Logger;

namespace OnlineVoting.Services.Infrastructures.Authorization.Jwt
{
    public class JwtAuthenticator : IJwtAuthenticator
    {
        private readonly JwtSettings _jwtSettings;
        private readonly ILoggerMessage _loggerMessage;

        public JwtAuthenticator(JwtSettings jwtSettings, ILoggerMessage loggerMessage)
        {
            _jwtSettings = jwtSettings;
            _loggerMessage = loggerMessage;
        }

        public Task<JwtToken> GenerateJwtToken(User user, string role, string? expires = null, List<Claim>? additionalClaims = null)
        {
            JwtSecurityTokenHandler jwtTokenHandler = new();

            byte[] key = Encoding.UTF8.GetBytes(_jwtSettings.Secret!);

            List<Claim> claims = new()
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

            SecurityTokenDescriptor tokenDescriptor = new()
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

            SecurityToken token = jwtTokenHandler.CreateToken(tokenDescriptor);
            string jwtToken = jwtTokenHandler.WriteToken(token);

            JwtToken result = new()
            {
                Token = jwtToken,
                IssuedAt = DateTime.UtcNow,
                Issuer = tokenDescriptor.Issuer,
                Expires = tokenDescriptor.Expires
            };

            _loggerMessage.LogInfo($"JWT token generated for user {user.Id} with role {role}");

            return Task.FromResult(result);
        }
    }
}