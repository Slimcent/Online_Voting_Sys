using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlineVoting.Models.Context;
using OnlineVoting.Models.Entities;

namespace OnlineVoting.Api.Data
{
    public static class SeedStudent
    {
        private const string password = "123456";

        public static async void EnsurePopulated(IApplicationBuilder app)
        {
            VotingDbContext  context = app.ApplicationServices
                .CreateScope().ServiceProvider
                .GetRequiredService<VotingDbContext>();
            if ((await context.Database.GetPendingMigrationsAsync()).Any()) 
                await context.Database.MigrateAsync();

            UserManager<User> userManager = app.ApplicationServices
               .CreateScope().ServiceProvider
               .GetRequiredService<UserManager<User>>();

            RoleManager<Role> roleManager = app.ApplicationServices
               .CreateScope().ServiceProvider
               .GetRequiredService<RoleManager<Role>>();

            Student student = app.ApplicationServices
               .CreateScope().ServiceProvider
               .GetRequiredService<Student>();

            User user = new() { Email = "weidai@mac.com", UserName = "weidai@mac.com", EmailConfirmed = true };
            var createUser = await userManager.CreateAsync(user, password);

            if (createUser.Succeeded)
            {
                if (!await roleManager.RoleExistsAsync("Student"))
                {
                    var role = new Role { Name = "Student" };
                    await roleManager.CreateAsync(role);
                }
                await userManager.AddToRoleAsync(user, "Student");
            }
            context.SaveChanges();

            // context.Users.AddRange(
            //  new User
            //  {
            //    Email = "weidai@mac.com"
            //  },
            //  new User
            //  {
            //      Email = "wea@mac.com"
            //  },
            //  new User
            //  {
            //      Email = "afifi@verizon.net"
            //  }
            //  );
            //context.SaveChanges();
        }
    }
}
