# Backend Modernization Log

## Introduction

The Online Voting System was originally developed as my undergraduate final-year project. The first version of the system was built 

using PHP, HTML, CSS, JavaScript and jQuery. As I continued learning ASP.NET Core and the .NET ecosystem, I decided to rebuild the application 

using C# and ASP.NET Core. That rewrite became the current version of the project.

Since completing the project, I have gained several years of professional experience developing backend applications using ASP.NET Core, 

Entity Framework Core, REST APIs, authentication and authorization, software architecture, cloud technologies, automated testing, 

and DevOps practices.

Revisiting this project provides an opportunity to apply that experience to an existing codebase. Rather than rewriting the application again, 

the goal is to modernize it incrementally while preserving its existing functionality. This approach reflects how many production systems 

evolve in practice, where applications are continuously improved instead of being replaced.

This document records that modernization process. Each modernization task explains:

- why the change was necessary;
- how it was implemented;
- how it was verified; and
- any follow-up work identified during the process.

The Angular frontend is intentionally excluded from this modernization because future frontend development for this project will use React. 

The focus of this document is therefore the ASP.NET Core backend.

---

## Modernization Objectives

The primary objective of this modernization is to evolve the project into a backend that aligns more closely with current software engineering practices. 

While the project will continue to serve as a learning resource, it should also demonstrate the design, implementation and operational standards 

expected of modern ASP.NET Core applications.

The modernization focuses on the following objectives:

- Adopt secure configuration management by removing sensitive information from source control and using environment-based configuration.
- Strengthen authentication and authorization to follow current security best practices.
- Improve the overall architecture through better separation of concerns, dependency injection and cleaner project organization.
- Review and improve the voting workflow to make the business logic easier to understand, maintain and extend.
- Improve input validation, exception handling and API responses to provide a more robust and predictable backend.
- Introduce automated testing to improve confidence in future changes and reduce regressions.
- Upgrade the application to a supported version of .NET and update outdated or vulnerable dependencies.
- Containerize the application using Docker to simplify development and deployment.
- Implement continuous integration using GitHub Actions to automate builds and testing.
- Improve API documentation and project documentation to make the system easier to understand and contribute to.
- Adopt modern development practices such as environment-based configuration, centralized logging, configuration validation, and consistent coding standards.
- Produce a backend project that reflects the quality and practices expected of a professional ASP.NET Core application while preserving the original functionality of the system.

### Existing Swagger change pending review

Before beginning the authentication review, an `ApiKey` reference was added to the Swagger security requirements alongside the existing bearer-token requirement.

This change has not yet been treated as complete because the corresponding API-key security definition and backend validation still need to be reviewed.

## Verification

The strongly typed JWT configuration was tested through Swagger.

A valid token was generated during login and supplied through Swagger's bearer authentication option. Swagger added the token to the request using the `Authorization` header and sent the request to a protected roles endpoint.

The request confirmed that:

- `JwtSettings` was successfully bound from configuration;
- the JWT generation service used the bound settings;
- the authentication middleware accepted the generated token; and
- Swagger correctly supplied the bearer token to protected endpoints.


# Custom Authorization Infrastructure

## Objective

The original project relied primarily on ASP.NET Core Identity for authentication and authorization. As the application evolved to support 

a custom role and permission model, the authorization process needed to be redesigned to work with the application's own database entities 

instead of the default Identity implementation.

The objective of this update was to implement a centralized authorization infrastructure that validates a user's permissions using the 

custom `ApplicationUserRole`, `Role`, `ApplicationRoleClaim`, and `ApplicationUserClaim` entities.

---

## Changes Made

To improve maintainability and project organization, all authentication and authorization components were reorganized into a 

dedicated **Authorization** module under the `Infrastructures` folder.

The following components were introduced or refactored:

- **JwtAuthenticator** – Generates JWT access tokens for authenticated users.
- **JwtSettings** – Provides strongly typed configuration for JWT authentication.
- **AuthorizationRequirement** – Represents the application's custom authorization requirement.
- **CustomAuthorizationHandler** – Performs permission validation during request authorization.
- **ClaimsPrincipalExtension** – Provides helper methods for retrieving the authenticated user's information, including:
  - User ID
  - Username
  - Roles
  - Claims

The authorization infrastructure is now organized as follows:

```text
Infrastructures
└── Authorization
    ├── Jwt
    │   ├── JwtAuthenticator.cs
    │   └── JwtSettings.cs
    │
    ├── Extensions
    │   └── ClaimsPrincipalExtension.cs
    │
    ├── AuthorizationRequirement.cs
    └── CustomAuthorizationHandler.cs
```

---

## Authorization Flow

The application now uses a permission-based authorization model.

Each protected endpoint exposes an endpoint name using the ASP.NET Core `Name` property. The endpoint name represents the permission required 

to access that resource. For example:

```csharp
[HttpGet("all-roles", Name = "all-roles")]
```

During authorization, the following steps are performed:

1. The JWT access token is validated by the ASP.NET Core authentication middleware.
2. The authenticated user's ID is retrieved from the `ClaimsPrincipal`.
3. The authorization handler loads all active roles assigned to the user.
4. The active permission claims associated with those roles are loaded.
5. The endpoint name is treated as the required permission.
6. The authorization handler checks whether any active role contains an active claim whose `ClaimValue` matches the endpoint name.
7. If no matching role claim exists, any direct permission claims assigned to the user are also evaluated.
8. Authorization succeeds if either a role claim or a direct user claim matches the endpoint name.
9. Otherwise, access is denied with **403 Forbidden**.

The authorization flow can be summarized as follows:

```text
Incoming Request
        │
        ▼
JWT Authentication
        │
        ▼
ClaimsPrincipal
        │
        ▼
Retrieve User ID
        │
        ▼
Load User Roles
        │
        ▼
Load Role Claims
        │
        ▼
Endpoint Name == ClaimValue ?
        │
   Yes ─┴─ No
    │       │
    ▼       ▼
200 OK   Check Direct User Claims
                │
          Match Found?
            │      │
           Yes     No
            │      │
            ▼      ▼
         200 OK   403 Forbidden
```

---

## Benefits

This redesign introduces several improvements over the previous implementation:

- Centralizes all authorization logic within a single authorization handler.
- Uses the application's custom role and permission model instead of relying solely on ASP.NET Core Identity.
- Allows permissions to be managed entirely through the database.
- Simplifies the addition of new protected endpoints, since authorization is driven by endpoint names and permission claims rather than hardcoded permission checks.
- Improves project organization by grouping all authentication and authorization infrastructure into a dedicated module.
- Clearly separates authentication (JWT validation) from authorization (permission evaluation), making the security architecture easier to understand, maintain and extend.

---

## Validation

| Scenario | Expected Result | Observed Result |
|----------|-----------------|-----------------|
| Request without JWT | 401 Unauthorized | 401 Unauthorized |
| Authenticated user without required permission | 403 Forbidden | 403 Forbidden |
| Authenticated user with matching role permission | 200 OK | 200 OK |
| Inactive role | 403 Forbidden | 403 Forbidden |
| Inactive role claim | 403 Forbidden | 403 Forbidden |
| Matching direct user permission | 200 OK | 200 OK |

The successful completion of these tests confirms that the custom authorization infrastructure correctly authenticates users, 

evaluates role and user permissions, and grants or denies access based on the configured permission model.


### Dependency Registration Cleanup

The dependency-injection configuration was reviewed after the authorization changes to remove duplicate registrations 

and align service lifetimes with the database context.

The following changes were made:

- Removed the duplicate `CustomAuthorizationHandler` registration from `AddRepositories()`.
- Kept the authorization handler registration inside `ConfigureAuthorization()`.
- Removed the duplicate `DbContext` registration from `Program.cs`.
- Kept the `DbContext` abstraction registration in `AddRepositories()`.
- Changed services that depend on `VotingDbContext` from transient to scoped.
- Kept `SynchronizedConverter` registered as a singleton.
- Removed unused namespace imports from `Program.cs`.

These changes ensure that each dependency is registered in one appropriate location and that database-dependent services share the same scoped `VotingDbContext` instance throughout an HTTP request.

The application was rebuilt and the authentication and authorization flows were retested successfully after the cleanup.


## Exception Handling Cleanup

The application's custom exception structure was simplified to make HTTP error responses more consistent and easier to maintain.

Previously, the service layer contained several entity-specific exceptions, including:

- `RegNoExistException`
- `RegNoNotFoundException`
- `StudentNotFoundException`
- `UserExistException`
- `UserNotFoundException`

These exceptions duplicated behaviour and in some cases, represented the wrong HTTP meaning. For example, exceptions for records 

that already existed inherited from `NotFoundException`, which caused conflict situations to be treated as `404 Not Found`.

The exception structure was reduced to:

```text
Exceptions
├── ConflictException.cs
├── InvalidCredentialsException.cs
└── NotFoundException.cs

