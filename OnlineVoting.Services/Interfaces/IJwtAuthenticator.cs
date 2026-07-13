using OnlineVoting.Models.Dtos.Response.Jwt;
using OnlineVoting.Models.Entities;
using System.Security.Claims;

namespace OnlineVoting.Services.Interfaces
{
    public interface IJwtAuthenticator
    {
        Task<JwtToken> GenerateJwtToken(User user, string role, string expires = null, List<Claim> additionalClaims = null);
    }
}
