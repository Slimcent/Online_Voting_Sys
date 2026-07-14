using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NLog;
using OnlineVoting.Api.Data;
using OnlineVoting.Api.Mapper;
using OnlineVoting.Api.Middlewares;
using OnlineVoting.Api.SeedData.Admin;
using OnlineVoting.Models.Context;
using OnlineVoting.Models.Entities.Email;
using OnlineVoting.Services.Helpers;
using OnlineVoting.Services.Infrastructures;
using OnlineVoting.Services.Infrastructures.Jwt;
using System.Text;
using System.Text.Json.Serialization;


string environmentFilePath = Path.Combine(Directory.GetCurrentDirectory(), "OnlineVoting.Api", ".env");

if (!File.Exists(environmentFilePath))
{
    environmentFilePath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
}

Env.Load(environmentFilePath);

var builder = WebApplication.CreateBuilder(args);

LogManager.LoadConfiguration(string.Concat(Directory.GetCurrentDirectory(),
                "/Nlog/nlog.config"));

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("SmtpSettings"));

//builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

builder.Services.AddScoped<DbContext, VotingDbContext>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(jwt =>
    {

            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            var key = Encoding.ASCII.GetBytes(jwtSettings.GetSection("Secret").Value);

            jwt.SaveToken = true;
            jwt.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                RequireExpirationTime = true,
                ValidIssuer = jwtSettings.GetSection("validIssuer").Value,
                ValidAudience = jwtSettings.GetSection("validAudience").Value,
                ClockSkew = TimeSpan.Zero
            };
    });

builder.Services.AddAuthorization(cfg =>
{
    cfg.AddPolicy("Authorization", policy => policy.Requirements.Add(new AuthorizationRequirment()));
});


// Configure SeedData
builder.Services.BindSeedConfig(builder.Configuration);
builder.Services.AddRepositories();
builder.Services.AddHttpContextAccessor();

builder.Services.AddControllers(setupAction =>
{
    setupAction.ReturnHttpNotAcceptable = true;
}).AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

}).AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

});

builder.Services.ConfigureCors();
builder.Services.ConfigureIISIntegration();
builder.Services.AddDBConnection(builder.Configuration);
builder.Services.ConfigureLoggerService();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.ConfigureSwagger();
builder.Services.AddAutoMapper(typeof(MappingProfile));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Online_Voting_Api v1");
        c.InjectStylesheet("/css/swagger-dark-theme.css");
    });
}

app.ConfigureExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseStaticFiles();

await SeedAppData.EnsurePopulated(app);

//SeedStudent.EnsurePopulated(app);

app.Run();