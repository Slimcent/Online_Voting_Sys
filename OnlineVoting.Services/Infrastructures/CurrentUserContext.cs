using Microsoft.AspNetCore.Http;
using OnlineVoting.Models.Extensions;
using OnlineVoting.Models.Interfaces;

namespace OnlineVoting.Services.Infrastructures
{
    public class CurrentUserContext : ICurrentUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? UserId => _httpContextAccessor.HttpContext?.User?.GetUserId();
        public string? Username => _httpContextAccessor.HttpContext?.User?.GetUsername();
    }
}