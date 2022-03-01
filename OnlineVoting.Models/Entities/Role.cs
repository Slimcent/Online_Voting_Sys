using Microsoft.AspNetCore.Identity;
using OnlineVoting.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineVoting.Models.Entities
{
    public class Role : IdentityRole, ITracker
    {
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
