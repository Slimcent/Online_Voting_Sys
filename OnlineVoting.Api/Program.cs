using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NLog;
using OnlineVoting.Api.Mapper;
using OnlineVoting.Api.Middlewares;
using OnlineVoting.Models.Context;
using OnlineVoting.Models.Entities.Email;
using OnlineVoting.Services.Infrastructures;
using OnlineVoting.Services.Infrastructures.Authorization;
using System.Text.Json.Serialization;
using VotingSystem.Data.SeedData;


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

builder.Services.AddScoped<DbContext, VotingDbContext>();

builder.Services.AddAuthorization(cfg =>
{
    cfg.AddPolicy("Authorization", policy => policy.Requirements.Add(new AuthorizationRequirement()));
});


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
builder.Services.BindConfigurations(builder.Configuration);
builder.Services.ConfigureJWT();
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

await SeedApplicationData.EnsurePopulated(app);

//SeedStudent.EnsurePopulated(app);

app.Run();