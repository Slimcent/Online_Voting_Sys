using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using OnlineVoting.Models.Context;
using OnlineVoting.Models.Entities;
using System.Security.Claims;

namespace VotingSystem.Data.SeedData
{
    public static class SeedApplicationData
    {
        public static async Task EnsurePopulated(IApplicationBuilder app)
        {
            using IServiceScope scope = app.ApplicationServices.CreateScope();

            IServiceProvider serviceProvider = scope.ServiceProvider;

            Seed seed = serviceProvider.GetRequiredService<Seed>();

            VotingDbContext context = serviceProvider.GetRequiredService<VotingDbContext>();

            UserManager<User> userManager = serviceProvider.GetRequiredService<UserManager<User>>();

            RoleManager<Role> roleManager =  serviceProvider.GetRequiredService<RoleManager<Role>>();

            IExecutionStrategy executionStrategy =  context.Database.CreateExecutionStrategy();

            await executionStrategy.ExecuteAsync(async () =>
            {
                await using var transaction = await context.Database.BeginTransactionAsync();

                try
                {
                    await SeedUserTypes(context, seed);
                    await SeedRoles(roleManager, seed);
                    await SeedRoleClaims(context, roleManager, seed);
                    await SeedAdminUser(context, userManager, seed);

                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        private static async Task SeedUserTypes(VotingDbContext context, Seed seed)
        {
            List<string> existingUserTypeNames = await context
                .Set<UserType>()
                .Select(userType => userType.Name)
                .ToListAsync();

            List<UserType> missingUserTypes = seed.UserTypes.Where(userType => !existingUserTypeNames
                .Contains(userType.Name, StringComparer.OrdinalIgnoreCase))
                .Select(userType => new UserType
                {
                    Name = userType.Name
                })
                .ToList();

            if (!missingUserTypes.Any())
                return;

            await context.Set<UserType>().AddRangeAsync(missingUserTypes);
        }

        private static async Task SeedRoles(RoleManager<Role> roleManager, Seed seed)
        {
            List<string> existingRoles = await roleManager.Roles.Select(role => role.Name!).ToListAsync();

            List<Role> missingRoles = seed.Roles.Except(existingRoles, StringComparer.OrdinalIgnoreCase)
                .Select(roleName => new Role
                {
                    Name = roleName
                })
                .ToList();

            foreach (Role role in missingRoles)
            {
                IdentityResult result = await roleManager.CreateAsync(role);

                EnsureIdentityOperationSucceeded(result, $"Unable to create role '{role.Name}'.");
            }
        }

        private static async Task SeedRoleClaims(VotingDbContext context, RoleManager<Role> roleManager, Seed seed)
        {
            Role? administratorRole = await roleManager.FindByNameAsync(seed.AdminUser.Role);

            if (administratorRole == null)
                throw new InvalidOperationException($"The administrator role $'{seed.AdminUser.Role}' was not found.");

            IList<Claim> existingClaims = await roleManager.GetClaimsAsync(administratorRole);

            HashSet<string> existingClaimValues = existingClaims.Where(claim => claim.Type == ClaimTypes.Name)
                .Select(claim => claim.Value)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            List<IdentityRoleClaim<string>> missingRoleClaims = seed.RoleClaims.Distinct(StringComparer.OrdinalIgnoreCase)
                .Where(claimValue => !existingClaimValues.Contains(claimValue))
                .Select(claimValue => new IdentityRoleClaim<string>
                    {
                        RoleId = administratorRole.Id,
                        ClaimType = ClaimTypes.Name,
                        ClaimValue = claimValue
                    })
                    .ToList();

            if (!missingRoleClaims.Any())
                return;

            await context.Set<IdentityRoleClaim<string>>().AddRangeAsync(missingRoleClaims);
        }

        private static async Task SeedAdminUser(VotingDbContext context, UserManager<User> userManager, Seed seed)
        {
            UserType? administratorUserType = await context.Set<UserType>().FirstOrDefaultAsync(userType => userType.Id == seed.AdminUser.UserTypeId);

            if (administratorUserType == null)
                throw new InvalidOperationException($"The administrator user type {seed.AdminUser.UserTypeId} was not found.");

            User? user = await userManager.FindByNameAsync(seed.AdminUser.UserName);

            if (user == null)
            {
                user = new User
                {
                    FirstName = seed.AdminUser.FirstName,
                    LastName = seed.AdminUser.LastName,
                    Email = seed.AdminUser.Email,
                    UserName = seed.AdminUser.UserName,
                    UserTypeId = seed.AdminUser.UserTypeId,
                    IsActive = true,
                    EmailConfirmed = true
                };

                IdentityResult createUserResult = await userManager.CreateAsync(user, seed.AdminUser.Password);

                EnsureIdentityOperationSucceeded(createUserResult, "Unable to create the seeded administrator.");
            }

            bool userHasRole = await userManager.IsInRoleAsync(user, seed.AdminUser.Role);

            if (!userHasRole)
            {
                IdentityResult addToRoleResult = await userManager.AddToRoleAsync(user, seed.AdminUser.Role);

                EnsureIdentityOperationSucceeded(addToRoleResult,  $"Unable to assign role " +
                    $"'{seed.AdminUser.Role}' to the seeded administrator.");
            }

            bool staffExists = await context.Set<Staff>().AnyAsync(staff => staff.UserId == user.Id);

            if (staffExists)
                return;

            DateTime currentDate = DateTime.UtcNow;

            Staff staff = new Staff
            {
                Id = Guid.NewGuid(),
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserId = user.Id,
                CreatedAt = currentDate,
                UpdatedAt = currentDate,
                IsDeleted = false
            };

            await context.Set<Staff>().AddAsync(staff);
        }

        private static void EnsureIdentityOperationSucceeded(IdentityResult result, string message)
        {
            if (result.Succeeded)
                return;

            string errors = string.Join(", ", result.Errors.Select(error => error.Description));

            throw new InvalidOperationException($"{message} {errors}");
        }
    }
}