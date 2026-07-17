namespace VotingSystem.Data.SeedData
{
    public class AdminUser
    {
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string Role { get; set; }
        public string Password { get; set; }
        public int UserTypeId { get; set; }
    }

    public class SeedUserType
    {
        public string Name { get; set; }
    }

    public class Seed
    {
        public AdminUser AdminUser { get; set; }
        public IEnumerable<string> Roles { get; set; }
        public IEnumerable<string> RoleClaims { get; set; }
        public IEnumerable<SeedUserType> UserTypes { get; set; }
    }
}