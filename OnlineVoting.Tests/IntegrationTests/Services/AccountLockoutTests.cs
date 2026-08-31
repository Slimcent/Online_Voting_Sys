using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OnlineVoting.Models.Context;
using OnlineVoting.Models.Entities;
using OnlineVoting.Tests.TestData.Constants;
using OnlineVoting.Tests.TestData.Data;

namespace OnlineVoting.Tests.IntegrationTests.Services
{
    public class AccountLockoutTests
    {
        [Fact]
        public async Task FailedLoginAttempts_ShouldLockAccountAfterFifthFailure()
        {
            (Microsoft.Data.Sqlite.SqliteConnection connection, VotingDbContext context) = await SqliteTestDbContextFactory.Create();

            await using (connection)
            await using (context)
            {
                using ServiceProvider serviceProvider = CreateIdentityServiceProvider(context);

                UserManager<User> userManager = serviceProvider.GetRequiredService<UserManager<User>>();
                SignInManager<User> signInManager = serviceProvider.GetRequiredService<SignInManager<User>>();

                User user = await CreateUser(userManager, context);

                for (int attempt = 1; attempt <= 4; attempt++)
                {
                    SignInResult result = await signInManager.CheckPasswordSignInAsync(user, TestValues.DifferentValidPassword, lockoutOnFailure: true);

                    Assert.False(result.Succeeded);
                    Assert.False(result.IsLockedOut);
                    Assert.Equal(attempt, await userManager.GetAccessFailedCountAsync(user));
                }

                SignInResult fifthAttempt = await signInManager.CheckPasswordSignInAsync(user, TestValues.DifferentValidPassword, lockoutOnFailure: true);

                Assert.False(fifthAttempt.Succeeded);
                Assert.True(fifthAttempt.IsLockedOut);
                Assert.True(await userManager.IsLockedOutAsync(user));
                Assert.NotNull(await userManager.GetLockoutEndDateAsync(user));
            }
        }

        [Fact]
        public async Task LockedAccount_WithCorrectPassword_ShouldRemainRejected()
        {
            (Microsoft.Data.Sqlite.SqliteConnection connection, VotingDbContext context) = await SqliteTestDbContextFactory.Create();

            await using (connection)
            await using (context)
            {
                using ServiceProvider serviceProvider = CreateIdentityServiceProvider(context);

                UserManager<User> userManager = serviceProvider.GetRequiredService<UserManager<User>>();
                SignInManager<User> signInManager = serviceProvider.GetRequiredService<SignInManager<User>>();

                User user = await CreateUser(userManager, context);

                for (int attempt = 0; attempt < 5; attempt++)
                {
                    await signInManager.CheckPasswordSignInAsync(user, TestValues.DifferentValidPassword, lockoutOnFailure: true);
                }

                SignInResult result = await signInManager.CheckPasswordSignInAsync(user, TestValues.ValidPassword, lockoutOnFailure: true);

                Assert.False(result.Succeeded);
                Assert.True(result.IsLockedOut);
            }
        }

        [Fact]
        public async Task SuccessfulLoginBeforeThreshold_ShouldResetFailedAccessCount()
        {
            (Microsoft.Data.Sqlite.SqliteConnection connection, VotingDbContext context) = await SqliteTestDbContextFactory.Create();

            await using (connection)
            await using (context)
            {
                using ServiceProvider serviceProvider = CreateIdentityServiceProvider(context);

                UserManager<User> userManager = serviceProvider.GetRequiredService<UserManager<User>>();
                SignInManager<User> signInManager = serviceProvider.GetRequiredService<SignInManager<User>>();

                User user = await CreateUser(userManager, context);

                for (int attempt = 0; attempt < 3; attempt++)
                {
                    await signInManager.CheckPasswordSignInAsync(user, TestValues.DifferentValidPassword, lockoutOnFailure: true);
                }

                Assert.Equal(3, await userManager.GetAccessFailedCountAsync(user));

                SignInResult result = await signInManager.CheckPasswordSignInAsync(user, TestValues.ValidPassword, lockoutOnFailure: true);

                Assert.True(result.Succeeded);
                Assert.Equal(0, await userManager.GetAccessFailedCountAsync(user));
                Assert.False(await userManager.IsLockedOutAsync(user));
            }
        }

        [Fact]
        public async Task ExpiredLockout_WithCorrectPassword_ShouldAllowLogin()
        {
            (Microsoft.Data.Sqlite.SqliteConnection connection, VotingDbContext context) = await SqliteTestDbContextFactory.Create();

            await using (connection)
            await using (context)
            {
                using ServiceProvider serviceProvider = CreateIdentityServiceProvider(context);

                UserManager<User> userManager = serviceProvider.GetRequiredService<UserManager<User>>();
                SignInManager<User> signInManager = serviceProvider.GetRequiredService<SignInManager<User>>();

                User user = await CreateUser(userManager, context);

                await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddMinutes(-1));

                SignInResult result = await signInManager.CheckPasswordSignInAsync(user, TestValues.ValidPassword, lockoutOnFailure: true);

                Assert.True(result.Succeeded);
                Assert.False(await userManager.IsLockedOutAsync(user));
                Assert.Equal(0, await userManager.GetAccessFailedCountAsync(user));
            }
        }

        private static ServiceProvider CreateIdentityServiceProvider(VotingDbContext context)
        {
            ServiceCollection services = new();

            services.AddLogging();
            services.AddAuthentication();
            services.AddHttpContextAccessor();
            services.AddSingleton(context);

            services.AddIdentityCore<User>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;

                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddRoles<Role>()
            .AddEntityFrameworkStores<VotingDbContext>()
            .AddSignInManager();

            return services.BuildServiceProvider();
        }

        private static async Task<User> CreateUser(UserManager<User> userManager, VotingDbContext context)
        {
            UserType userType = new()
            {
                Name = "Student"
            };

            await context.UserTypes.AddAsync(userType);
            await context.SaveChangesAsync();

            User user = new()
            {
                UserName = TestValues.ValidEmail,
                Email = TestValues.ValidEmail,
                FirstName = TestValues.ValidName,
                LastName = TestValues.ValidName,
                Active = true,
                UserTypeId = userType.Id
            };

            IdentityResult result = await userManager.CreateAsync(user, TestValues.ValidPassword);

            Assert.True(result.Succeeded, string.Join(", ", result.Errors.Select(error => error.Description)));

            return user;
        }
    }
}