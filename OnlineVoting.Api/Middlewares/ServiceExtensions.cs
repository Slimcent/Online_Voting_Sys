using Asp.Versioning;
using DinkToPdf;
using DinkToPdf.Contracts;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using OnlineVoting.Api.Configurations;
using OnlineVoting.Api.Documentation.Filters;
using OnlineVoting.Api.Filters;
using OnlineVoting.Data.Interfaces;
using OnlineVoting.Models.Configurations;
using OnlineVoting.Models.Context;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Interfaces;
using OnlineVoting.Models.Validators.Request;
using OnlineVoting.Services.Implementation;
using OnlineVoting.Services.Infrastructures;
using OnlineVoting.Services.Infrastructures.Auditing;
using OnlineVoting.Services.Infrastructures.Authorization;
using OnlineVoting.Services.Infrastructures.Authorization.Jwt;
using OnlineVoting.Services.Interfaces;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using VotingSystem.Data.Implementation;
using VotingSystem.Logger;
using Microsoft.AspNetCore.HttpOverrides;
using System.Net;


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
            services.AddSingleton<ILoggerMessage, VotingSystem.Logger.LoggerMessage>();

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
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserContext, CurrentUserContext>();
            services.AddScoped<IRefreshTokenService, RefreshTokenService>();
            services.AddScoped<IAuditMetadataProvider, AuditMetadataProvider>();
            services.AddScoped<IAuditTrailService, AuditTrailService>();
            services.AddMemoryCache();

            services.AddHttpClient<IIpGeolocationService, IpGeolocationService>(client =>
            {
                client.BaseAddress = new Uri("https://ipwho.is/");
                client.Timeout = TimeSpan.FromSeconds(2);
            });

            return services;
        }

        public static void ConfigureValidators(this IServiceCollection services)
        {
            services.AddScoped<ValidationFilter>();

            // Uses LoginRequestValidator as an assembly marker and automatically
            // registers all FluentValidation validators found in the same assembly.
            services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
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
            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

            services.AddSwaggerGen(options =>
            {
                // Api documentation, for reading the XML comments from the code.
                Assembly[] assemblies =
                {
                    Assembly.GetExecutingAssembly(), typeof(ModelsAssemblyMarker).Assembly
                };

                foreach (Assembly assembly in assemblies)
                {
                    string xmlFile = $"{assembly.GetName().Name}.xml";

                    string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

                    options.IncludeXmlComments(xmlPath);
                }

                options.EnableAnnotations();

                options.OperationFilter<ApiDocumentationOperationFilter>();
                options.SupportNonNullableReferenceTypes();

                // Custom operation filter to add the "X-Device-Location" header parameter to all API endpoints.
                // This middleware was only added just to test X-Device-Location on swagger.
                //options.OperationFilter<DeviceLocationHeaderOperationFilter>(); 

                options.AddSecurityDefinition(
                    "Bearer",
                    new OpenApiSecurityScheme
                    {
                        Name = "Authorization",
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        BearerFormat = "JWT",
                        In = ParameterLocation.Header,
                        Description = "Enter the JWT token only. Swagger will add the 'Bearer' prefix automatically."
                    });

                options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecuritySchemeReference(
                            "Bearer",
                            document),
                        new List<string>()
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

        public static void ConfigureApiVersioning(this IServiceCollection services)
        {
            services.AddApiVersioning(options =>
            {
                // Uses version 1.0 when the client does not explicitly provide
                // an API version, preserving the existing API routes.
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;

                // Adds the supported and deprecated API versions to the
                // response headers.
                options.ReportApiVersions = true;

                // Reads the API version from the URL route.
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddMvc()
            .AddApiExplorer(options =>
            {
                // Produces version names such as "v1" and "v2".
                options.GroupNameFormat = "'v'V";

                // Replaces {version:apiVersion} in the route displayed
                // by Swagger with the real API version number.
                options.SubstituteApiVersionInUrl = true;
            });
        }

        public static IServiceCollection ConfigureHealthChecks(this IServiceCollection services)
        {
            services.AddHealthChecks().AddDbContextCheck<VotingDbContext>(name: "database", failureStatus: HealthStatus.Unhealthy,
                tags: new[] { "ready" });

            return services;
        }

        public static IServiceCollection ConfigureRateLimiting(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.OnRejected = async (context, cancellationToken) =>
                {
                    if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter))
                        context.HttpContext.Response.Headers.RetryAfter = ((int)Math.Ceiling(retryAfter.TotalSeconds)).ToString();

                    string correlationId = context.HttpContext.Items[CorrelationIdMiddleware.CorrelationIdItemName]?.ToString() ?? "Unavailable";

                    ProblemDetails problemDetails = new()
                    {
                        Status = StatusCodes.Status429TooManyRequests,
                        Title = "Too many requests",
                        Detail = "The request limit has been exceeded. Try again later.",
                        Instance = context.HttpContext.Request.Path
                    };

                    problemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
                    problemDetails.Extensions["correlationId"] = correlationId;

                    await context.HttpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
                };

                options.AddFixedWindowLimiter(RateLimitPolicyNames.Authentication,
                    limiterOptions =>
                    {
                        limiterOptions.PermitLimit = 5;
                        limiterOptions.Window = TimeSpan.FromMinutes(1);
                        limiterOptions.QueueLimit = 0;
                        limiterOptions.AutoReplenishment = true;
                    });

                options.AddFixedWindowLimiter(RateLimitPolicyNames.Voting,
                    limiterOptions =>
                    {
                        limiterOptions.PermitLimit = 3;
                        limiterOptions.Window = TimeSpan.FromMinutes(1);
                        limiterOptions.QueueLimit = 0;
                        limiterOptions.AutoReplenishment = true;
                    });

                options.AddFixedWindowLimiter(RateLimitPolicyNames.AdministrativeWrite,
                    limiterOptions =>
                    {
                        limiterOptions.PermitLimit = 20;
                        limiterOptions.Window = TimeSpan.FromMinutes(1);
                        limiterOptions.QueueLimit = 0;
                        limiterOptions.AutoReplenishment = true;
                    });
            });

            return services;
        }

        public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
        {
            return app.UseMiddleware<CorrelationIdMiddleware>();
        }

        public static async Task ApplyDatabaseMigrations(this IApplicationBuilder app)
        {
            using IServiceScope scope = app.ApplicationServices.CreateScope();

            VotingDbContext context = scope.ServiceProvider.GetRequiredService<VotingDbContext>();

            IEnumerable<string> migrations = context.Database.GetMigrations();

            if (!migrations.Any())
                throw new InvalidOperationException("No database migrations were found.");

            await context.Database.MigrateAsync();
        }

        public static void ConfigureForwardedHeaders(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor
                    | ForwardedHeaders.XForwardedProto;

                IEnumerable<IConfigurationSection> knownProxySections =
                    configuration.GetSection("ReverseProxy:KnownProxies").GetChildren();

                foreach (IConfigurationSection knownProxySection in knownProxySections)
                {
                    string? knownProxy = knownProxySection.Value;

                    if (IPAddress.TryParse(knownProxy, out IPAddress? ipAddress))
                        options.KnownProxies.Add(ipAddress);
                }
            });
        }
    }
}