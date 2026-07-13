# M001 — Environment-Based Configuration

## Why was this necessary?

The original implementation stored sensitive configuration values directly inside `appsettings.json`. These included the database connection string, 

SMTP credentials, JWT signing secret and administrator seed credentials.

While this approach is common during the early stages of development, it is not suitable for applications that may be shared publicly or deployed 

across multiple environments. Keeping secrets inside source-controlled configuration files increases the risk of accidental exposure and makes 

environment-specific deployments more difficult.

Modern ASP.NET Core applications separate application configuration from sensitive configuration by supplying secrets through environment variables 

or secret management services. This allows the same application to run in different environments without modifying the codebase or committing 

credentials to source control.

For these reasons, the first modernization task focused on introducing environment-based configuration.

## Existing Implementation

Before this modernization:

- Sensitive configuration was stored directly in `appsettings.json`.
- The project did not load a local `.env` file.
- Configuration values were read only from the default ASP.NET Core configuration providers.

Although the application functioned correctly, its configuration approach was not suitable for long-term maintenance or deployment.

## Implementation

### Added DotNetEnv

The `DotNetEnv` package was added to the API project to enable loading configuration values from a local `.env` file during development.

### Updated application startup

`Program.cs` was modified to load the `.env` file before creating the application builder.

Loading the `.env` file before calling `WebApplication.CreateBuilder(args)` ensures that the environment variables become part of ASP.NET Core's 

configuration pipeline and are available throughout the application.

### Created the local `.env` file

A local `.env` file was introduced to store development-only secrets.

The file now contains:

- Database connection string
- SMTP configuration
- JWT configuration
- Seed administrator credentials

The `.env` file is excluded from source control and is intended to exist only on the developer's machine.

### Cleaned appsettings.json

Sensitive values were removed from `appsettings.json`.

Only non-sensitive application configuration remains in the file, including logging configuration, SMTP host information, JWT metadata, 

application settings and seed metadata.

## Challenges Encountered

During implementation, several issues were identified and resolved.

### Environment variables were not being loaded

Initially, the application continued to read values from `appsettings.json`.

The issue was traced to the fact that ASP.NET Core does not automatically load values from a `.env` file. Adding `DotNetEnv` and loading the file 

before creating the application builder resolved the issue.

### Incorrect LocalDB connection string

The first version of the `.env` file contained an incorrectly escaped LocalDB instance name, which prevented SQL Server from locating the LocalDB 

instance. After correcting the connection string, the application successfully connected to the database.

### Database verification

After updating the connection string, the application was able to start successfully and complete the database initialization process.

## Files Modified

- `OnlineVoting.Api/Program.cs`
- `OnlineVoting.Api/OnlineVoting.Api.csproj`
- `OnlineVoting.Api/appsettings.json`
- `OnlineVoting.Api/.env`

## Verification

The following commands were executed:

```powershell
dotnet restore
dotnet build
dotnet run --project OnlineVoting.Api
```

The backend started successfully.

```
Now listening on: https://localhost:7229
Now listening on: http://localhost:5229

Application started.
Hosting environment: Development
```

This confirmed that:

- the `.env` file was successfully loaded;
- sensitive configuration was being supplied through environment variables;
- the application could connect to the database; and
- the backend remained fully functional after the configuration changes.

## Lessons Learned

Although ASP.NET Core supports environment variables by default, it does not automatically load values from a local `.env` file. 

When using a `.env` file for local development, it must be loaded before the application builder is created.

Separating sensitive configuration from application configuration also simplifies deployment because the same build can be used across multiple 

environments without modifying configuration files.

## Outcome

The backend now follows a more secure and maintainable configuration approach. Sensitive information is no longer stored in source-controlled 

configuration files, providing a solid foundation for the remaining modernization tasks.

---