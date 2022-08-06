using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlineVoting.Api.SeedData.Model;
using OnlineVoting.Models.Context;
using OnlineVoting.Models.Entities;

namespace OnlineVoting.Api.SeedData.Admin
{
    public static class SeedAppData
    {
        public static IServiceCollection BindSeedConfig(this IServiceCollection services, IConfiguration configuration)
        {
            Seed seed = new();

            configuration.GetSection("Seed").Bind(seed);

            services.AddSingleton(seed);

            return services;
        }


        public static async Task EnsurePopulated(IApplicationBuilder app)
        {
            Seed seed = app.ApplicationServices.CreateScope().ServiceProvider.GetRequiredService<Seed>();

            VotingDbContext context = app.ApplicationServices
                .CreateScope().ServiceProvider.GetRequiredService<VotingDbContext>();

            UserManager<User> userManager = app.ApplicationServices
                .CreateScope().ServiceProvider.GetRequiredService<UserManager<User>>();

            RoleManager<Role> roleManager = app.ApplicationServices
                .CreateScope().ServiceProvider.GetRequiredService<RoleManager<Role>>();

            if (!await roleManager.Roles.AnyAsync())
            {
                foreach (string role in seed.Roles)
                {
                    await roleManager.CreateAsync(new Role { Name = role });
                }
            }

            if (!await userManager.Users.AnyAsync())
            {
                User user = new()
                {
                    FirstName = seed.AdminUser.FirstName,
                    LastName = seed.AdminUser.LastName,
                    Email = seed.AdminUser.Email,
                    UserName = seed.AdminUser.Username,
                    UserTypeId = seed.AdminUser.UserType,
                    IsActive = true,
                    EmailConfirmed = true,
                };

                IdentityResult createUser = await userManager.CreateAsync(user, seed.AdminUser.Password);

                if (createUser.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, seed.AdminUser.Role);
                }
            }
        }
    }
}
