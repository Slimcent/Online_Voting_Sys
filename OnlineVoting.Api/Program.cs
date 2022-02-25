using Microsoft.Extensions.Logging;
using NLog;
using OnlineVoting.Api.Mapper;
using OnlineVoting.Api.Middlewares;

var builder = WebApplication.CreateBuilder(args);

LogManager.LoadConfiguration(string.Concat(Directory.GetCurrentDirectory(),
                "/Nlog/nlog.config"));

// Add services to the container.

builder.Services.AddControllers();
builder.Services.ConfigureCors();
builder.Services.ConfigureIISIntegration();
builder.Services.AddDBConnection(builder.Configuration);
builder.Services.ConfigureLoggerService();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddRepositories();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.ConfigureExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
