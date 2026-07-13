using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace OnlineVoting.Services.Extension
{
    public static class ClaimsPrincipalExtension
    {
        public static string? GetUsername(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Name)?.Value;
        }

        public static string? GetUserId(this ClaimsPrincipal user)
        {
            return user.FindFirst("Id")?.Value;
        }
    }
}
