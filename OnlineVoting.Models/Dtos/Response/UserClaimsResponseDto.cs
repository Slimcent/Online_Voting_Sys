using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineVoting.Models.Dtos.Response
{
    public class UserClaimsResponseDto
    {
        public string? ClaimType { get; set; }
        public string? ClaimValue { get; set; }
    }
}
