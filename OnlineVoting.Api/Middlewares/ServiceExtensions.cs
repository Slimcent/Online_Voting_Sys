using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlineVoting.Models.Context;
using OnlineVoting.Models.Entities;
using OnlineVoting.Services.Implementation;
using OnlineVoting.Services.Interfaces;
using VotingSystem.Data.Implementation;
using VotingSystem.Data.Interfaces;
using VotingSystem.Logger;

namespace OnlineVoting.Api.Middlewares
{
    public static class ServiceExtensions
    {
        public static void ConfigureCors(this IServiceCollection services) => services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy", builder =>
                builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());
        });

        public static void ConfigureIISIntegration(this IServiceCollection services) => services.Configure<IISOptions>(options =>
        {
        });

        public static void ConfigureLoggerService(this IServiceCollection services) =>
            services.AddScoped<ILoggerMessage, VotingSystem.Logger.LoggerMessage>();

        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddTransient<IUnitOfWork, UnitOfWork<VotingDbContext>>();
            services.AddTransient<IStudentService, StudentService>();
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IRolesService, RolesService>();
            services.AddTransient<IPositionService, PositionService>();
            services.AddTransient<IFacultyService, FacultyService>();
            services.AddTransient<IDepartmentService, DepartmentService>();
            services.AddTransient<IFileDataExtractorService, FileDataExtractorService>();
            services.AddScoped<DbContext, VotingDbContext>();
            services.AddTransient<IServiceFactory, ServiceFactory>();
            services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));

            return services;
        }

        public static IServiceCollection AddDBConnection(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<VotingDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("VotingConnection"),
                     b => b.MigrationsAssembly("OnlineVoting.Api")
                ));


            services.AddIdentity<User, Role>(o =>
            {
                o.Password.RequireDigit = false;
                o.Password.RequireLowercase = false;
                o.Password.RequireUppercase = false;
                o.Password.RequireNonAlphanumeric = false;
                o.Password.RequiredLength = 6;
                o.User.RequireUniqueEmail = false;
                o.SignIn.RequireConfirmedEmail = false;
            })
                .AddEntityFrameworkStores<VotingDbContext>()
                .AddDefaultTokenProviders();

            return services;
        }
    }
}
