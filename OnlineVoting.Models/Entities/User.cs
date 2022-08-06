﻿using Microsoft.AspNetCore.Identity;
using OnlineVoting.Models.Enums;
using OnlineVoting.Models.Interfaces;

namespace OnlineVoting.Models.Entities
{
    public class User : IdentityUser, ITracker
    {
        public User()
        {
            IsActive = true;
        }

        public bool IsActive { get; set; }
        public UserType UserTypeId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public virtual Staff? Staff { get; set; }
    }
}
