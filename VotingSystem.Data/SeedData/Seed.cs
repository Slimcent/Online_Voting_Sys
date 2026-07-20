namespace VotingSystem.Data.SeedData
{
    public class AdminUser
    {
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public string UserType { get; set; }
        public string Gender { get; set; }
    }

    public class StudentUser
    {
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public string UserType { get; set; }
        public string Gender { get; set; }
        public string Department { get; set; }
        public string RegNumber { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class SeedGender
    {
        public string Name { get; set; }
    }

    public class SeedFaculty
    {
        public string Name { get; set; }
    }

    public class SeedDepartment
    {
        public string Name { get; set; }
        public string Faculty { get; set; }
    }

    public class SeedUserType
    {
        public string Name { get; set; }
    }

    public class Seed
    {
        public IEnumerable<string> Roles { get; set; }
        public IEnumerable<string> RoleClaims { get; set; }
        public IEnumerable<string> StudentRoleClaims { get; set; }
        public IEnumerable<SeedUserType> UserTypes { get; set; }
        public IEnumerable<SeedGender> Genders { get; set; }
        public AdminUser AdminUser { get; set; }
        public StudentUser StudentUser { get; set; }
        public SeedFaculty Faculty { get; set; }
        public SeedDepartment Department { get; set; }
    }
}