using OnlineVoting.Services.Infrastructures.Authorization.Jwt;
using VotingSystem.Data.SeedData;

namespace OnlineVoting.Services.Extension
{
    public static class ConfigurationValidationExtensions
    {
        public static JwtSettings Validate(this JwtSettings jwt)
        {
            if (string.IsNullOrWhiteSpace(jwt.Secret))
                throw new InvalidOperationException("The JWT secret was not found.");

            if (string.IsNullOrWhiteSpace(jwt.Issuer))
                throw new InvalidOperationException("The JWT issuer was not found.");

            if (string.IsNullOrWhiteSpace(jwt.Audience))
                throw new InvalidOperationException("The JWT audience was not found.");

            if (Convert.ToInt32(jwt.Expires) <= 0)
                throw new InvalidOperationException("The JWT expiration value must be greater than zero.");

            return jwt;
        }

        public static Seed Validate(this Seed seed)
        {
            if (!seed.Roles.Any())
                throw new InvalidOperationException("At least one seed role must be configured.");

            if (!seed.UserTypes.Any())
                throw new InvalidOperationException("At least one user type must be configured.");

            if (string.IsNullOrWhiteSpace(seed.AdminUser.UserName))
                throw new InvalidOperationException("The seeded administrator username was not found.");

            if (string.IsNullOrWhiteSpace(seed.AdminUser.Email))
                throw new InvalidOperationException("The seeded administrator email was not found.");

            if (string.IsNullOrWhiteSpace(seed.AdminUser.Password))
                throw new InvalidOperationException("The seeded administrator password was not found.");

            if (string.IsNullOrWhiteSpace(seed.AdminUser.Role))
                throw new InvalidOperationException("The seeded administrator role was not found.");

            bool administratorRoleExists = seed.Roles.Any(role => string.Equals(role, seed.AdminUser.Role, StringComparison.OrdinalIgnoreCase));

            if (!administratorRoleExists)
                throw new InvalidOperationException($"The administrator role '{seed.AdminUser.Role}' " +
                    "is not included in the configured seed roles.");

            if (string.IsNullOrWhiteSpace(seed.AdminUser.UserType))
                throw new InvalidOperationException("The seeded administrator user type was not found.");

            return seed;
        }
    }
}