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

            RoleManager<Role> roleManager = serviceProvider.GetRequiredService<RoleManager<Role>>();

            IExecutionStrategy executionStrategy = context.Database.CreateExecutionStrategy();

            await executionStrategy.ExecuteAsync(async () =>
            {
                await using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync();

                try
                {
                    await SeedUserTypes(context, seed);
                    await SeedGenders(context, seed);

                    // These records must be saved before the admin
                    // and student seed methods can query them.
                    await context.SaveChangesAsync();

                    await SeedRoles(roleManager, seed);

                    await SeedRoleClaims(context, roleManager, seed);

                    await SeedStudentRoleClaims(context, roleManager, seed);

                    await context.SaveChangesAsync();

                    await SeedFaculty(context, seed);

                    // Faculty must exist before the department is created.
                    await context.SaveChangesAsync();

                    await SeedDepartment(context, seed);

                    // Department must exist before the student is created.
                    await context.SaveChangesAsync();

                    await SeedAdminUser(context, userManager, seed);

                    await SeedStudentUser(context, userManager, roleManager, seed);

                    // Persist staff and student records added through the context.
                    await context.SaveChangesAsync();

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
            List<string> existingUserTypeNames = await context.Set<UserType>()
                .Select(userType => userType.Name)
                .ToListAsync();

            List<UserType> missingUserTypes = seed.UserTypes
                .Where(userType => !existingUserTypeNames
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

        private static async Task SeedGenders(VotingDbContext context, Seed seed)
        {
            List<string> existingGenderNames = await context.Set<Gender>()
                .Select(gender => gender.Name)
                .ToListAsync();

            List<Gender> missingGenders = seed.Genders
                .Where(gender => !existingGenderNames.Contains(gender.Name.Trim(), StringComparer.OrdinalIgnoreCase))
                .Select(gender => new Gender
                {
                    Name = gender.Name.Trim()
                })
                .ToList();

            if (!missingGenders.Any())
                return;

            await context.Set<Gender>().AddRangeAsync(missingGenders);
        }

        private static async Task SeedRoles(RoleManager<Role> roleManager, Seed seed)
        {
            foreach (string roleName in seed.Roles)
            {
                bool roleExists = await roleManager.RoleExistsAsync(roleName);

                if (roleExists)
                    continue;

                Role role = new Role
                {
                    Name = roleName,
                    Active = true
                };

                IdentityResult createRoleResult = await roleManager.CreateAsync(role);

                EnsureIdentityOperationSucceeded(createRoleResult, $"Unable to create role '{roleName}'.");
            }
        }

        private static async Task SeedRoleClaims(VotingDbContext context, RoleManager<Role> roleManager, Seed seed)
        {
            Role? administratorRole = await roleManager.FindByNameAsync(seed.AdminUser.Role);

            if (administratorRole == null)
            {
                throw new InvalidOperationException($"The administrator role "
                    + $"'{seed.AdminUser.Role}' was not found.");
            }

            List<string> existingClaimValues = await context.Set<ApplicationRoleClaim>()
                .Where(roleClaim => roleClaim.RoleId == administratorRole.Id
                    && roleClaim.ClaimType == ClaimTypes.Name)
                .Select(roleClaim => roleClaim.ClaimValue!)
                .ToListAsync();

            HashSet<string> existingClaimValueSet = existingClaimValues.ToHashSet(StringComparer.OrdinalIgnoreCase);

            DateTime currentDate = DateTime.UtcNow;

            List<ApplicationRoleClaim> missingRoleClaims = seed.RoleClaims.Distinct(StringComparer.OrdinalIgnoreCase)
                .Where(claimValue => !existingClaimValueSet.Contains(claimValue))
                    .Select(claimValue =>
                        new ApplicationRoleClaim
                        {
                            RoleId = administratorRole.Id,
                            ClaimType = ClaimTypes.Name,
                            ClaimValue = claimValue,
                            Active = true,
                            CreatedAt = currentDate,
                            UpdatedAt = currentDate
                        })
                    .ToList();

            if (!missingRoleClaims.Any())
                return;

            await context.Set<ApplicationRoleClaim>().AddRangeAsync(missingRoleClaims);
        }

        private static async Task SeedStudentRoleClaims(VotingDbContext context, RoleManager<Role> roleManager, Seed seed)
        {
            Role? studentRole = await roleManager.FindByNameAsync(seed.StudentUser.Role);

            if (studentRole == null)
            {
                throw new InvalidOperationException($"The student role "
                    + $"'{seed.StudentUser.Role}' was not found.");
            }

            List<string> existingClaimValues = await context.Set<ApplicationRoleClaim>()
                .Where(roleClaim => roleClaim.RoleId == studentRole.Id && roleClaim.ClaimType == ClaimTypes.Name)
                .Select(roleClaim => roleClaim.ClaimValue!)
                .ToListAsync();

            HashSet<string> existingClaimValueSet = existingClaimValues.ToHashSet(StringComparer.OrdinalIgnoreCase);

            DateTime currentDate = DateTime.UtcNow;

            List<ApplicationRoleClaim> missingRoleClaims = seed.StudentRoleClaims.Distinct(StringComparer.OrdinalIgnoreCase)
                .Where(claimValue => !existingClaimValueSet.Contains(claimValue))
                    .Select(claimValue =>
                        new ApplicationRoleClaim
                        {
                            RoleId = studentRole.Id,
                            ClaimType = ClaimTypes.Name,
                            ClaimValue = claimValue,
                            Active = true,
                            CreatedAt = currentDate,
                            UpdatedAt = currentDate
                        })
                    .ToList();

            if (!missingRoleClaims.Any())
                return;

            await context.Set<ApplicationRoleClaim>().AddRangeAsync(missingRoleClaims);
        }

        private static async Task SeedFaculty(VotingDbContext context, Seed seed)
        {
            string facultyName = seed.Faculty.Name.Trim();

            bool facultyExists = await context.Set<Faculty>()
                .AnyAsync(faculty => faculty.Name.ToLower() == facultyName.ToLower());

            if (facultyExists)
                return;

            Faculty faculty = new Faculty
            {
                Name = facultyName,
                Activated = true
            };

            await context.Set<Faculty>().AddAsync(faculty);
        }

        private static async Task SeedDepartment(VotingDbContext context, Seed seed)
        {
            string facultyName = seed.Department.Faculty.Trim();
            string departmentName = seed.Department.Name.Trim();

            Faculty? faculty = await context.Set<Faculty>()
                .FirstOrDefaultAsync(existingFaculty => existingFaculty.Name.ToLower() == facultyName.ToLower());

            if (faculty == null)
            {
                throw new InvalidOperationException($"The faculty '{seed.Department.Faculty}' "
                    + "is required by the seeded department was not found.");
            }

            bool departmentExists = await context.Set<Department>()
                .AnyAsync(department => department.Name.ToLower() == departmentName.ToLower()
                    && department.FacultyId == faculty.Id);

            if (departmentExists)
                return;

            Department department = new Department
            {
                Name = departmentName,
                FacultyId = faculty.Id,
                Activated = true
            };

            await context.Set<Department>().AddAsync(department);
        }

        private static async Task SeedAdminUser(VotingDbContext context, UserManager<User> userManager, Seed seed)
        {
            (Gender? administratorGender, UserType? administratorUserType) = await GetGenderAndUserType(context, seed);

            string administratorGenderName = seed.AdminUser.Gender.Trim();

            User? user = await userManager.FindByNameAsync(seed.AdminUser.UserName);

            if (user == null)
            {
                User? userWithEmail = await userManager.FindByEmailAsync(seed.AdminUser.Email);

                if (userWithEmail != null)
                {
                    throw new InvalidOperationException($"A user with the email "
                        + $"'{seed.AdminUser.Email}' already exists, "
                        + $"but does not use the configured username "
                        + $"'{seed.AdminUser.UserName}'.");
                }

                user = new User
                {
                    FirstName = seed.AdminUser.FirstName,
                    LastName = seed.AdminUser.LastName,
                    Email = seed.AdminUser.Email,
                    UserName = seed.AdminUser.UserName,
                    UserTypeId = administratorUserType.Id,
                    Active = true,
                    EmailConfirmed = true
                };

                IdentityResult createUserResult = await userManager.CreateAsync(user, seed.AdminUser.Password);

                EnsureIdentityOperationSucceeded(createUserResult, "Unable to create the seeded administrator.");
            }

            bool userHasRole = await userManager.IsInRoleAsync(user, seed.AdminUser.Role);

            if (!userHasRole)
            {
                IdentityResult addToRoleResult = await userManager.AddToRoleAsync(user, seed.AdminUser.Role);

                EnsureIdentityOperationSucceeded(addToRoleResult, $"Unable to assign role "
                    + $"'{seed.AdminUser.Role}' to the seeded administrator.");
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
                GenderId = administratorGender.Id,
                CreatedAt = currentDate,
                UpdatedAt = currentDate,
                Active = true
            };

            await context.Set<Staff>().AddAsync(staff);
        }

        private static async Task SeedStudentUser(VotingDbContext context, UserManager<User> userManager,
            RoleManager<Role> roleManager, Seed seed)
        {
            StudentUser studentSeed = seed.StudentUser;

            (Gender? studentGender, UserType? administratorUserType) = await GetGenderAndUserType(context, seed);

            Department? studentDepartment = await context.Set<Department>()
                .FirstOrDefaultAsync(department => department.Name == studentSeed.Department);

            if (studentDepartment == null)
            {
                throw new InvalidOperationException($"The student department "
                    + $"{studentSeed.Department} was not found.");
            }

            Role? studentRole = await roleManager.FindByNameAsync(studentSeed.Role);

            if (studentRole == null)
            {
                throw new InvalidOperationException($"The student role "
                    + $"'{studentSeed.Role}' was not found.");
            }

            User? studentUser = await userManager.FindByNameAsync(studentSeed.UserName);

            if (studentUser == null)
            {
                User? userWithEmail = await userManager.FindByEmailAsync(studentSeed.Email);

                if (userWithEmail != null)
                {
                    throw new InvalidOperationException($"A user with the email "
                        + $"'{studentSeed.Email}' already exists, "
                        + $"but does not use the configured username "
                        + $"'{studentSeed.UserName}'.");
                }

                studentUser = new User
                {
                    LastName = studentSeed.LastName,
                    FirstName = studentSeed.FirstName,
                    Email = studentSeed.Email,
                    UserName = studentSeed.UserName,
                    PhoneNumber = studentSeed.PhoneNumber,
                    UserTypeId = administratorUserType.Id,
                    Active = true,
                    EmailConfirmed = true
                };

                IdentityResult createUserResult = await userManager.CreateAsync(studentUser, studentSeed.Password);

                EnsureIdentityOperationSucceeded(createUserResult, "Unable to create the seeded student user.");
            }

            bool studentHasRole = await userManager.IsInRoleAsync(studentUser, studentSeed.Role);

            if (!studentHasRole)
            {
                IdentityResult addRoleResult = await userManager.AddToRoleAsync(studentUser, studentSeed.Role);

                EnsureIdentityOperationSucceeded(addRoleResult, $"Unable to assign role " +
                    $"'{studentSeed.Role}' to the seeded student user.");
            }

            bool studentProfileExists = await context.Set<Student>().AnyAsync(student => student.UserId == studentUser.Id);

            if (studentProfileExists)
                return;

            bool registrationNumberExists = await context.Set<Student>().AnyAsync(student => student.RegNumber == studentSeed.RegNumber);

            if (registrationNumberExists)
            {
                throw new InvalidOperationException($"A student with registration number " +
                    $"'{studentSeed.RegNumber}' already exists.");
            }

            Student student = new Student
            {
                UserId = studentUser.Id,
                DepartmentId = studentDepartment.Id,
                GenderId = studentGender.Id,
                RegNumber = studentSeed.RegNumber,
                PhoneNumber = studentSeed.PhoneNumber,
                CreatedAt = DateTime.UtcNow,
                Active = true,
                CreatedBy = $"{seed.AdminUser.FirstName} {seed.AdminUser.LastName}",
            };

            await context.Set<Student>().AddAsync(student);
        }

        private static async Task<(Gender gender, UserType usertype)> GetGenderAndUserType(VotingDbContext context, Seed seed)
        {
            UserType? administratorUserType = await context.Set<UserType>()
                .FirstOrDefaultAsync(userType => userType.Name.ToLower() == seed.AdminUser.UserType.ToLower());

            if (administratorUserType == null)
            {
                throw new InvalidOperationException($"The administrator user type "
                    + $"{seed.AdminUser.UserType} was not found.");
            }

            string administratorGenderName = seed.AdminUser.Gender.Trim();

            Gender? administratorGender = await context.Set<Gender>()
                .FirstOrDefaultAsync(gender => gender.Name.ToLower() == administratorGenderName.ToLower());

            if (administratorGender == null)
            {
                throw new InvalidOperationException($"The administrator gender "
                    + $"'{seed.AdminUser.Gender}' was not found.");
            }

            return (administratorGender, administratorUserType);
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