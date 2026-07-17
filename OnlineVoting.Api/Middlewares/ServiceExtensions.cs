using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OnlineVoting.Models.Context;
using OnlineVoting.Models.Entities;
using OnlineVoting.Services.Implementation;
using OnlineVoting.Services.Infrastructures.Authorization;
using OnlineVoting.Services.Infrastructures.Authorization.Jwt;
using OnlineVoting.Services.Interfaces;
using System.Security.Claims;
using System.Text;
using VotingSystem.Data.Implementation;
using VotingSystem.Data.Interfaces;
using VotingSystem.Logger;

namespace OnlineVoting.Api.Middlewares
{
    public static class ServiceExtensions
    {
        public static void ConfigureCors(this IServiceCollection services) => services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy", builder => builder.AllowAnyOrigin()
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
            services.AddScoped<IUnitOfWork, UnitOfWork<VotingDbContext>>();

            services.AddScoped<IJwtAuthenticator, JwtAuthenticator>();

            services.AddScoped<IStudentService, StudentService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRolesService, RolesService>();
            services.AddScoped<IPositionService, PositionService>();
            services.AddScoped<IFacultyService, FacultyService>();
            services.AddScoped<IDepartmentService, DepartmentService>();
            services.AddScoped<IVoterService, VoterService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IStaffService, StaffService>();
            services.AddScoped<IFileDataExtractorService, FileDataExtractorService>();

            services.AddScoped<DbContext, VotingDbContext>();
            services.AddScoped<IServiceFactory, ServiceFactory>();

            services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));

            return services;
        }

        public static IServiceCollection AddDBConnection(this IServiceCollection services, IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString("VotingConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("The connection string 'VotingConnection' was not found.");

            services.AddDbContext<VotingDbContext>(options => options.UseSqlServer(connectionString, b => b.MigrationsAssembly("OnlineVoting.Api")
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

        public static void ConfigureJWT(this IServiceCollection services)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer();

            services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
                .Configure<JwtSettings>((options, jwtSettings) =>
                {
                    byte[] key = Encoding.UTF8.GetBytes(jwtSettings.Secret!);

                    options.SaveToken = true;
                    options.TokenValidationParameters =
                        new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            RequireExpirationTime = true,
                            ValidIssuer = jwtSettings.Issuer,
                            ValidAudience = jwtSettings.Audience,
                            IssuerSigningKey = new SymmetricSecurityKey(key),
                            ClockSkew = TimeSpan.Zero,
                            NameClaimType = ClaimTypes.Name,
                            RoleClaimType = ClaimTypes.Role
                        };
                });
        }

        public static void ConfigureSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.EnableAnnotations();
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Online-Voting", Version = "v1" });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 1safsfsdfdfd\""
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });
        }

       
        public static IServiceCollection ConfigureAuthorization(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("Authorization",
                    policy =>
                    {
                        policy.Requirements.Add(new AuthorizationRequirement());
                    });
            });

            services.AddScoped<IAuthorizationHandler, CustomAuthorizationHandler>();

            return services;
        }
    }
}