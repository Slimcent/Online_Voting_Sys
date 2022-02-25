using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineVoting.Models.Dtos.Request
{
    public class EditUserClaimsDto
    {
        public string? Email { get; set; }
        public string? ClaimType { get; set; }
        public string? ClaimValue { get; set; }
        public string? OldValue { get; set; }
    }
}
