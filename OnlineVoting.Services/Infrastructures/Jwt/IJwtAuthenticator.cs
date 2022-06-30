using OnlineVoting.Models.Dtos.Response.Jwt;
using OnlineVoting.Models.Entities;

namespace OnlineVoting.Services.Infrastructures.Jwt
{
    public interface IJwtAuthenticator
    {
        Task<JwtToken> GenerateJwtToken(User user);
    }
}
