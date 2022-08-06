﻿using OnlineVoting.Models.Enums;

namespace OnlineVoting.Api.SeedData.Model
{
    public class AdminUser
    {
        public string? LastName { get; set; }
        public string? FirstName { get; set; }
        public string? Email { get; set; }
        public string? Username { get; set; }
        public string? Role { get; set; }
        public string? Password { get; set; }
        public UserType UserType { get; set; }
    }
}