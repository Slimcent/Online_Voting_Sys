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

---

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

---

## Unit of Work Transaction Support

Transaction management was added to the Unit of Work so service methods can safely execute business operations that require multiple database saves.

### Changes

The `IUnitOfWork` interface now exposes methods for:

- Beginning a database transaction
- Committing the active transaction
- Rolling back the active transaction

The `UnitOfWork` implementation now stores the active `IDbContextTransaction` and disposes it after a successful commit or rollback.

### Why this was added

A single Entity Framework Core `SaveChanges` call is already transactional. However, some business operations may require 

multiple `SaveChangesAsync` calls.

For example, one entity may need to be saved first so that its generated identifier can be used when creating another related entity.

Without an explicit transaction, the first save could remain in the database if a later operation fails. Transaction support ensures 
that either the complete business operation succeeds or all its database changes are rolled back.

### Service usage

Services can now perform multi-step database operations using the following structure:

```csharp
await _unitOfWork.BeginTransactionAsync();

try
{
    repository.Add(firstEntity);
    await _unitOfWork.SaveChangesAsync();

    repository.Add(secondEntity);
    await _unitOfWork.SaveChangesAsync();

    await _unitOfWork.CommitTransactionAsync();
}
catch
{
    await _unitOfWork.RollbackTransactionAsync();
    throw;
}

---

## Milestone: Application Data Seeding

### Summary
Implemented a comprehensive application data seeding process to automatically provision the application with the required reference 

data and initial users during startup.

### Changes
- Added transactional application seeding using EF Core execution strategies.
- Seeded reference data:
  - User types
  - Genders
  - Roles
  - Administrator role claims
  - Student role claims
  - Faculties
  - Departments
- Added creation of the initial administrator account.
- Added creation of the initial student account.
- Seeded related Staff and Student records.
- Implemented dependency-aware seeding so entities are created in the correct order.
- Used database-generated identity values for Gender, Faculty and Department instead of hard-coded IDs.
- Prevented duplicate seed data by checking for existing records before insertion.
- Added validation to ensure required dependencies exist before creating related entities.
- Centralized identity operation validation through a reusable helper.


# Request Validation Modernization

## Foundation Modernization

Before beginning the validation refactoring, the project was first modernized to use the latest .NET platform.

### .NET Upgrade

- Upgraded the solution from the previous .NET version to **.NET 10**.
- Updated all projects to target **.NET 10**.
- Verified that the solution builds successfully after the upgrade.

### Package Updates

Updated project dependencies to their latest compatible versions.

This included updating NuGet packages across the solution to versions compatible with .NET 10, ensuring support for the 

new runtime and language features before beginning the validation refactoring.

---

## Overview

This milestone replaces the project's Data Annotation-based request validation with a centralized FluentValidation implementation. 

Validation is now executed automatically before controller actions, resulting in cleaner request models, reusable validation rules 

and a more maintainable validation architecture.

---

## Changes

### Added FluentValidation

- Integrated FluentValidation into the application.
- Configured automatic validator registration through assembly scanning.
- Added a global validation filter to validate all incoming request models before controller actions execute.

```csharp
// Uses LoginRequestValidator as an assembly marker and automatically
// registers all FluentValidation validators found in the same assembly.
services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
```

---

### Added Global Validation Filter

Created a reusable `ValidationFilter` that:

- Executes before controller actions.
- Locates the correct validator for each request model.
- Executes validation automatically.
- Returns a standardized `ValidationProblemDetails` response with HTTP 400 when validation fails.
- Prevents controller actions from executing when request validation fails.

This removes the need for manual validation inside controllers.

---

### Removed Data Annotation Validation

Removed validation attributes from request DTOs including:

- `Required`
- `EmailAddress`
- `MaxLength`
- `MinLength`
- `RegularExpression`

Validation responsibility has been moved entirely to FluentValidation.

---

## Shared Validators

Created reusable validators to eliminate duplicated validation logic across request models.

### EmailValidator

Provides reusable email validation.

Rules:

- Required
- Valid email format

### NameValidator

Provides reusable validation for name fields.

Rules:

- Required
- Between 2 and 50 characters

Used for:

- FirstName
- LastName
- Faculty names
- Department names
- Position names
- Role names
- Generic name-based requests

### PasswordValidator

Provides reusable password validation.

Rules:

- Required
- Minimum length of 8 characters
- At least one uppercase letter
- At least one lowercase letter
- At least one number
- At least one special character

### PhoneNumberValidator

Provides reusable phone number validation.

Rules:

- Required
- Must match:

```
^0\d{10}$
```

---

## Request Validators

Added dedicated FluentValidation validators for request models including:

- LoginRequest
- ChangePasswordRequest
- VerifyAccountRequest
- UpdateAddressRequest
- CreateUserRequest
- CreateStaffRequest
- CreateStudentRequest
- CreateDepartmentRequest
- CreateWithNameRequest
- AddUserToRoleRequest
- VoteRequest
- UpdateStaffRequest
- UploadStudentRequest
- EditUserClaimsRequest

Each validator now contains validation rules specific to its request model.

---

## Generic Request Models

### CreateWithNameRequest

Introduced a reusable request model for endpoints that only require a single name.

Used by:

- Faculty creation
- Position creation
- Role creation

This removed several duplicated request DTOs.

### CreateUserRequest

Refactored common user creation properties into a single reusable request model.

Common properties include:

- FirstName
- LastName
- Email
- PhoneNumber
- Gender
- UserType
- Role

Specialized requests now inherit from this base request where appropriate.

---

## Student Registration Number

Removed `RegNumber` from the student creation request.

The registration number is now generated internally by the application instead of being supplied by the client.

---

## Department Validation

Improved department creation validation.

Supports:

- Creating a single department.
- Creating multiple departments.

Validation ensures:

- FacultyId is valid.
- At least one department name is supplied.
- Every supplied department name passes validation.

---

## Password Validation Improvements

Added validation preventing users from reusing their current password during password changes.

---

## Validation Behavior

Configured validators to use `CascadeMode.Stop`.

This prevents multiple validation errors from being returned after the first failure on a property, resulting in cleaner and more meaningful validation responses.

---

## Request Model Cleanup

Renamed request DTOs to follow a consistent naming convention.

Examples:

- LoginDto → LoginRequest
- ChangePasswordRequestDto → ChangePasswordRequest
- VerifyAccountRequestDto → VerifyAccountRequest
- CreateStaffRequestDto → CreateStaffRequest
- CreateStudentRequestDto → CreateStudentRequest

The `Request` suffix is now used consistently throughout the project.

---

## AutoMapper Updates

Updated AutoMapper profiles to support the refactored request models and inheritance hierarchy.

This reduced manual object mapping and simplified the transition to the new shared request models.

---

## Controllers

Updated controllers to use the new request models.

Controllers no longer perform manual validation.

Validation is now handled automatically by the global validation filter before controller actions execute.

---

## Benefits

This modernization provides:

- Upgrade to .NET 10.
- Updated project dependencies.
- Centralized request validation.
- Automatic validation before controller execution.
- Reusable validation rules.
- Reduced duplicated validation logic.
- Cleaner request DTOs.
- Consistent validation messages.
- Improved separation of concerns.
- Easier maintenance and future extensibility.

---

## API Versioning and Version-Aware Swagger

API versioning was added using:

- `Asp.Versioning.Mvc`
- `Asp.Versioning.Mvc.ApiExplorer`

The API uses URL-segment versioning.

Example:

```text
/api/v1/auth/login

---

## Standardized API Error Responses with ProblemDetails

The API error-handling pipeline was modernized to use ASP.NET Core's built-in `ProblemDetails` and `ValidationProblemDetails` response formats.

This replaces the previous custom `ResponseError` model with the standardized error format recommended for ASP.NET Core APIs.

The goal of this modernization is to provide a consistent response structure for all API errors, making the API easier to consume and 

improving interoperability with client applications such as React, Angular, mobile applications and third-party integrations.

### Exception Middleware

The existing exception middleware was updated to return `ProblemDetails` for application exceptions instead of the custom `ResponseError` object.

The following exception mappings were implemented:

| Exception | HTTP Status |
|-----------|------------:|
| `InvalidDataException` | 400 Bad Request |
| `ArgumentException` | 400 Bad Request |
| `InvalidCredentialsException` | 401 Unauthorized |
| `NotFoundException` | 404 Not Found |
| `ConflictException` | 409 Conflict |
| Any unhandled exception | 500 Internal Server Error |

Each response now contains a standardized structure consisting of:

- `type`
- `title`
- `status`
- `detail`
- `instance`
- `traceId`

The middleware continues to log all exceptions using the existing logging infrastructure while returning consistent JSON responses to clients.

### Validation Responses

The custom `ValidationFilter` was updated to return `ValidationProblemDetails`.

Validation responses now include:

- Field-level validation errors
- RFC-compliant error metadata
- Request path
- Request trace identifier

This provides a consistent experience between validation errors and runtime exceptions.

### Status Code Pages

A new status-code response handler was added using:

```csharp
app.ConfigureExceptionHandler();
app.ConfigureStatusCodePages();
```

This ensures that framework-generated responses are also returned using the same standardized format.

The following responses are now standardized even when no exception is thrown:

- 401 Unauthorized
- 403 Forbidden
- 404 Not Found

### Response Examples

#### Validation Error (400)

```json
{
  "errors": {
    "email": [
      "Email cannot be empty."
    ]
  },
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Validation failed.",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "instance": "/api/v1/Auth/login",
  "traceId": "..."
}
```

#### Unauthorized (401)

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.2",
  "title": "Unauthorized",
  "status": 401,
  "detail": "Authentication is required to access this resource.",
  "instance": "/api/v1/Position/all-paged-positions",
  "traceId": "..."
}
```

#### Resource Not Found (404)

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Resource not found",
  "status": 404,
  "detail": "User not found",
  "instance": "/api/v1/Auth/login",
  "traceId": "..."
}
```

#### Conflict (409)

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.10",
  "title": "Conflict",
  "status": 409,
  "detail": "A resource with the same name already exists.",
  "instance": "/api/v1/Position/create-position",
  "traceId": "..."
}
```

#### Internal Server Error (500)

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.6.1",
  "title": "Internal server error",
  "status": 500,
  "detail": "An unexpected error occurred.",
  "instance": "/api/v1/Position/all-paged-positions",
  "traceId": "..."
}
```

### Benefits

This modernization provides several improvements:

- Replaced the custom `ResponseError` model with the ASP.NET Core standard.
- Standardized all error responses across the application.
- Improved consistency between validation errors and runtime exceptions.
- Added RFC-compliant `ProblemDetails` responses.
- Included request path (`instance`) for easier debugging.
- Included request trace identifiers (`traceId`) to simplify troubleshooting and log correlation.
- Preserved the existing logging behavior while improving the API response format.
- Improved compatibility with modern client applications and API tooling.

---

# API Documentation Modernization

## Overview

Modernized the API documentation by replacing scattered Swagger annotations with a centralized documentation system. The new approach 

improves maintainability, reduces duplication and provides a more consistent OpenAPI specification.

---

## Centralized Documentation Architecture

Introduced a reusable documentation framework consisting of:

- `ApiDocumentationAttribute`
- `ApiDocumentationRegistry`
- `ApiDocumentationOperationFilter`
- `ApiOperationDocumentation`
- `ApiResponseDocumentation`
- `CommonApiResponses`

Endpoint documentation is now maintained separately from controllers, keeping controllers clean and focused on request handling.

---

## Endpoint Documentation

Created dedicated documentation definitions and key classes for the following controllers:

- Auth
- Student
- Staff
- Role
- Position
- Faculty
- Department
- Claims

Each endpoint now includes:

- Summary
- Description
- Success response
- Standardized error responses

---

## Standardized API Responses

Standardized the documented HTTP responses across the API:

- `200 OK`
- `400 Bad Request`
- `401 Unauthorized`
- `403 Forbidden`
- `404 Not Found`
- `409 Conflict`
- `500 Internal Server Error`

All error responses are documented using the `ProblemDetails` format.

---

## XML Documentation

Enabled XML documentation generation for:

- `OnlineVoting.Api`
- `OnlineVoting.Models`

Configured Swagger to load XML comments from both assemblies.

Added XML documentation to request models, including:

- Class summaries
- Property descriptions
- Example values

---

## Swagger Improvements

Configured Swagger to:

- Support API versioning
- Display XML documentation
- Display request model descriptions and examples
- Display standardized response documentation
- Support nullable reference types
- Improve schema and model rendering

---

## Controller Improvements

Updated controllers to:

- Use `ApiDocumentationAttribute` for endpoint documentation
- Add explicit `[FromBody]` and `[FromQuery]` attributes where appropriate
- Remove duplicated endpoint-specific Swagger response annotations where centralized documentation is used
- Keep controllers focused on request handling

---

## Authentication Documentation

Completed documentation for the remaining authentication endpoints:

- Send password reset email
- Reset password
- Change password
- Update recovery email
- Send change email confirmation
- Change email

Documented successful authentication responses returning text with:

- `ResponseType = typeof(string)`

Endpoints returning no response body were documented without a response type.

---

## Verification

Verified that:

- All endpoints appear correctly in Swagger
- API versioning works correctly
- Endpoint summaries and descriptions are displayed
- XML documentation appears for request models
- Request examples are rendered correctly
- Response schemas are generated correctly
- `ProblemDetails` responses are documented consistently
- Authorization requirements are displayed correctly
- Anonymous authentication endpoints remain publicly accessible

---

## Result

The API now uses a centralized documentation system that:

- Eliminates duplicated Swagger annotations
- Improves maintainability
- Produces a cleaner OpenAPI specification
- Provides consistent request and response documentation
- Keeps controller implementations concise and focused

---

# Result Pattern Modernization

## Overview

The application has been updated to use a consistent Result pattern across the service and controller layers.

Before this change, many service methods returned plain values, returned error messages as successful responses, or threw exceptions for expected business 

conditions such as missing records, invalid input and duplicate data. Controllers also contained repeated logic for deciding which HTTP response to return.

The Result pattern now provides a standard way for services to communicate the outcome of an operation. Controllers receive the result and convert it into 

the appropriate HTTP response.

This change improves consistency, reduces duplicated response handling and keeps business logic inside the service layer.

## Objectives

The Result pattern was introduced to:

- standardize service return values
- reduce repeated controller response logic
- avoid using exceptions for expected business conditions
- return consistent HTTP status codes
- simplify error propagation between services
- improve maintainability and testability

Unexpected technical failures are still handled by the global exception middleware.

## Supported Result Outcomes

The Result pattern supports the following outcomes:

| Result | HTTP status |
|---|---:|
| Success | 200 OK |
| Created | 201 Created |
| No Content | 204 No Content |
| Validation Error | 400 Bad Request |
| Unauthorized | 401 Unauthorized |
| Forbidden | 403 Forbidden |
| Not Found | 404 Not Found |
| Conflict | 409 Conflict |

## Shared Infrastructure

The following shared components were added or updated:

- result status definition
- generic result model
- controller result mapping extension
- failure propagation support for dependent service calls

The generic result model carries the operation status, returned value, error message, and success state.

The controller result mapping extension converts service outcomes into the corresponding ASP.NET Core HTTP responses.

## Affected Application Areas

### Authentication and User Management

The authentication and user service operations were updated to return structured results.

Affected operations include:

- user login
- user creation
- password reset
- user verification
- password change
- recovery email update
- email change

Authentication failures now return an unauthorized result instead of exposing whether the username or password was incorrect.

Services that depend on user creation now propagate the original failure result instead of throwing a new exception.

Affected areas:

- User controller
- User service interface
- User service implementation
- services that call user creation or other converted user operations

### Faculty Management

The Faculty feature was updated to use the Result pattern.

Affected operations include:

- faculty creation
- faculty retrieval
- faculty update
- faculty deletion
- faculty list retrieval
- paginated faculty queries

The service now returns structured outcomes for invalid input, duplicate records, missing records, successful creation and successful updates.

Affected areas:

- Faculty controller
- Faculty service interface
- Faculty service implementation

### Department Management

The Department feature was updated to use the Result pattern.

Affected operations include:

- department creation
- department retrieval
- department update
- department deletion
- department list retrieval
- paginated department queries

The service now handles validation failures, duplicate departments, missing departments and successful operations through structured results.

Existing response messages that incorrectly referred to faculties were corrected.

Affected areas:

- Department controller
- Department service interface
- Department service implementation

### Position Management

The Position feature was updated to use the Result pattern.

Affected operations include:

- position creation
- position retrieval
- position update
- position deletion
- position status changes
- active and inactive position lists
- paginated position queries

Collection endpoints now return successful empty collections instead of treating the absence of records as an error.

Affected areas:

- Position controller
- Position service interface
- Position service implementation

### Role Management

The Role feature was updated to use the Result pattern.

Affected operations include:

- role creation
- role editing
- role deletion
- adding users to roles
- removing users from roles
- retrieving user roles
- role status changes
- active and inactive role lists
- paginated role queries

Identity operation failures are now converted into structured validation results.

Duplicate role membership is returned as a conflict instead of an exception.

Affected areas:

- Roles controller
- Roles service interface
- Roles service implementation

### Claims Management

The Claims feature was updated to use the Result pattern.

Affected operations include:

- adding a claim to a user
- removing a claim
- editing a claim
- retrieving user claims

The service now handles missing users, invalid claim input, duplicate claims, missing claims and Identity operation failures through structured results.

An existing user with no claims now receives a successful empty collection instead of a bad request.

The internal route discovery helper remains unchanged because it is not part of the normal controller response flow.

Affected areas:

- Claims controller
- Claims service interface
- Claims service implementation

### Staff Management

The Staff feature was updated to use the Result pattern.

Affected operations include:

- staff creation
- staff retrieval
- staff retrieval by email
- staff update
- staff address update
- staff deletion
- staff status changes
- total staff count
- active and deleted staff lists
- paginated staff queries

Additional corrections were made during the conversion:

- newly created staff records now use the user ID returned by the user creation service
- the total staff endpoint now returns a numeric count
- a count of zero is treated as a valid successful response
- active and deleted filters are preserved during paginated searches
- empty staff collections return successful empty results

Affected areas:

- Staff controller
- Staff service interface
- Staff service implementation

### Student Management

The Student feature was updated to use the Result pattern.

Affected operations include:

- student creation
- contestant creation
- bulk student upload

Student creation now propagates user creation failures through the Result pattern.

Contestant creation now returns structured outcomes for missing registration numbers, missing position information, existing contestants, missing students, and successful creation.

The student Excel template download endpoint remains unchanged because it returns a file stream rather than a standard JSON response.

Affected areas:

- Student controller
- Student service interface
- Student service implementation

## Controller Changes

Controllers were simplified across the affected features.

Previously, controllers often:

- checked whether returned collections contained items
- manually returned bad request responses
- manually created error response objects
- returned successful responses even when a service returned an error message
- repeated the same response-handling logic across multiple endpoints

Controllers now generally:

1. receive the request
2. call the corresponding service
3. convert the returned result into an HTTP response

This keeps controllers focused on HTTP handling and leaves business outcome decisions in the service layer.

## Service Changes

Service interfaces and implementations were updated to return structured results instead of plain values for expected business outcomes.

Services now return:

- validation results for invalid input
- not-found results for missing records
- conflict results for duplicate records
- unauthorized or forbidden results where applicable
- created results for successful creation operations
- success results for successful reads, updates, and deletions
- successful empty collections where no matching records exist

Expected business conditions are no longer represented by exceptions.

## Exception Handling

The Result pattern is used for expected business outcomes.

Examples include:

- invalid request data
- missing records
- duplicate records
- failed Identity operations
- unauthorized login attempts
- forbidden operations

Unexpected technical failures remain the responsibility of the global exception middleware.

Examples include:

- database connection failures
- infrastructure errors
- unhandled framework exceptions
- programming errors
- unexpected null references

The Result pattern complements the exception middleware rather than replacing it.

## Empty Collection Behaviour

Collection endpoints now return successful empty collections when no records are found.

A valid request with no matching results returns:

- `200 OK`
- an empty collection

The absence of matching records is not treated as a failed request.

## File Download Behaviour

File download endpoints were intentionally left outside the standard result conversion where necessary.

These endpoints continue to use the controller file response because they must return:

- a file stream
- a content type
- a filename

This applies to the student Excel template download.

## Benefits

The Result pattern modernization provides the following improvements:

- consistent HTTP responses
- thinner controllers
- clearer service contracts
- reduced exception usage
- less duplicated response logic
- easier error propagation
- improved testability
- clearer handling of empty collections
- better separation between business failures and technical failures
- easier future extension of service operations

## Affected Files

### Shared Infrastructure

- Result status definition
- Generic result model
- Controller result mapping extension

### Authentication and User Management

- User controller
- User service interface
- User service implementation
- dependent services using converted user operations

### Faculty

- Faculty controller
- Faculty service interface
- Faculty service implementation

### Department

- Department controller
- Department service interface
- Department service implementation

### Position

- Position controller
- Position service interface
- Position service implementation

### Roles

- Roles controller
- Roles service interface
- Roles service implementation

### Claims

- Claims controller
- Claims service interface
- Claims service implementation

### Staff

- Staff controller
- Staff service interface
- Staff service implementation

### Student

- Student controller
- Student service interface
- Student service implementation

---

# Health Checks

## Overview

Health check endpoints were added to allow monitoring systems and hosting platforms to determine the application's operational state.

Two endpoints were introduced:

- `/health`
- `/health/ready`

## Liveness Endpoint

The `/health` endpoint verifies that the application process is running.

This endpoint does not check external dependencies such as the database and is intended for liveness monitoring.

## Readiness Endpoint

The `/health/ready` endpoint verifies that the application is ready to serve requests.

In addition to confirming that the application is running, it checks database connectivity using `VotingDbContext`.

If the database is unavailable, the endpoint returns an unhealthy status.

## Service Registration

Health check registration was centralized in the service extension layer through `ConfigureHealthChecks()` to keep `Program.cs` clean 

and consistent with the existing application architecture.

## Affected Areas

- Service extension
- Program configuration
- Health check middleware
- Database readiness monitoring

## Verification

The implementation was verified by:

- accessing `/health`
- accessing `/health/ready`
- confirming successful database connectivity
- confirming anonymous access to both endpoints

---

# Rate Limiting

## Overview

Rate limiting was added to protect sensitive API operations from excessive requests, repeated authentication attempts, accidental request loops 

and basic abuse.

The implementation uses ASP.NET Core's built-in rate-limiting middleware and applies named policies only to selected controllers or endpoints. 

Normal read operations, health checks and development Swagger endpoints are not restricted by default.

## Objectives

The rate-limiting implementation was introduced to:

- protect authentication endpoints from repeated login attempts
- reduce abuse of voting and administrative operations
- provide consistent `429 Too Many Requests` responses
- centralize policy configuration
- allow different limits for different types of operations
- avoid applying one restrictive policy to the entire API

## Rate-Limit Policies

The following named policies were introduced:

| Policy | Purpose |
|---|---|
| Authentication | Login and other sensitive authentication operations |
| Voting | Vote submission operations |
| Administrative Write | Administrative creation, update and bulk-upload operations |

The initial limits are intended for development and testing. They should be reviewed and adjusted based on expected production traffic and load-test results.

## Policy Definitions

The rate-limit policy values are defined in an enum inside the Models project.

A separate policy-name class exposes the corresponding constant string values required by ASP.NET Core attributes and policy registration.

This avoids repeating policy-name strings throughout the application and keeps the names consistent between service registration and controller usage.

## Service Configuration

Rate-limiting registration was added to the service extension layer.

The configuration includes:

- named fixed-window policies
- request limits and time windows
- disabled request queuing
- automatic permit replenishment
- `429 Too Many Requests` as the rejection status
- a standardized `ProblemDetails` response
- a `Retry-After` response header when retry metadata is available

Keeping this configuration in the service extension layer maintains a clean `Program.cs` and follows the existing application configuration structure.

## Middleware Configuration

The rate-limiting middleware was added to the request pipeline.

It runs after authentication and before authorization. This ordering allows future policies to use authenticated user information when partitioning or evaluating requests.

## Controller and Endpoint Usage

Rate limiting is applied through named policies.

A policy can be applied to:

- an individual endpoint
- an entire controller

Endpoint-level configuration is used when only selected operations in a controller require protection.

Controller-level configuration is used when all actions in the controller share the same rate-limit requirements.

Individual endpoints can also disable rate limiting when they should remain unrestricted.

## Authentication Protection

The authentication policy was applied to the login endpoint.

After the permitted number of requests is exceeded, the endpoint returns:

- `429 Too Many Requests`
- a `ProblemDetails` response body
- a `Retry-After` header when available

This reduces the effectiveness of repeated login attempts and basic brute-force activity.

## Voting Protection

The voting policy is intended for vote-submission operations.

It applies a stricter request limit than normal administrative operations because repeated vote submissions are security-sensitive and should not be processed at a high frequency.

## Administrative Write Protection

The administrative-write policy is intended for operations such as:

- student creation
- staff creation
- contestant creation
- bulk student upload
- other administrative write operations

These operations receive a higher request allowance than authentication or voting while still being protected from excessive traffic.

## Swagger Documentation

The centralized API documentation for protected endpoints was updated to include the `429 Too Many Requests` response.

The response documentation uses `ProblemDetails` as the response type and explains that the request limit has been exceeded.

This prevents Swagger from displaying the `429` response as undocumented.

## Exception Handling

Rate-limit rejections are not handled by the global exception middleware.

A rate-limit rejection is an expected middleware response, not an application exception.

The responsibilities remain separated as follows:

- rate-limit rejections are handled by the rate-limiting middleware
- expected business outcomes are handled by the Result pattern
- unexpected failures are handled by the global exception middleware

## Affected Areas

### Models

- rate-limit policy enum
- rate-limit policy-name configuration

### Service Configuration

- rate-limiting service registration
- named policy definitions
- rejection response handling
- `Retry-After` header handling

### Application Pipeline

- rate-limiting middleware registration

### Controllers

- authentication endpoints
- voting endpoints
- selected administrative write endpoints
- controllers that use a shared policy where appropriate

### API Documentation

- `429 Too Many Requests` responses for protected operations

## Verification

The rate limiter was verified through the login endpoint.

The following behaviour was confirmed:

- requests within the configured limit were accepted
- requests exceeding the limit returned `429 Too Many Requests`
- the rejection response used `ProblemDetails`
- the endpoint became available again after the configured time window
- Swagger documented the `429` response
- no global exception-handler changes were required

## Result

Sensitive API operations are now protected by reusable named rate-limit policies.

The implementation remains flexible because limits can be changed centrally without modifying controller logic and 

policies can be applied selectively based on the risk and expected traffic of each endpoint.

---

# Correlation IDs and Request Tracing

## Overview

Correlation IDs were added to make API requests easier to trace between client responses and application logs.

Each request now has a correlation ID. If the client provides an `X-Correlation-ID` header, the existing value is used; otherwise, a new ID is 

generated. The correlation ID is returned in the response header and included in error responses where applicable.

ASP.NET Core's existing trace ID is kept separately.

## Implementation

A correlation ID middleware was added to the API request pipeline. It handles the correlation ID for the lifetime of the request and records 

request completion details such as:

- correlation ID
- trace ID
- HTTP method
- request path
- response status code
- execution time
- authenticated user, where available

The middleware is registered globally, so individual controllers do not need to implement request tracing themselves.

The existing exception handling was also updated so exception logs include the correlation ID and trace ID. Supported `ProblemDetails` responses 

now expose both identifiers, making it possible to match an API error with the corresponding server log.

## Result Handling

`ResultActionResultExtensions` was previously located under:

`OnlineVoting.Models/GlobalMessage`

During the correlation ID implementation, the extension needed access to the current `HttpContext` and correlation ID middleware. Keeping it in the 

Models project would introduce a dependency from Models to the API project.

It was therefore moved to:

`OnlineVoting.Api/Extensions/ResultActionResultExtensions.cs`

This keeps the responsibilities separated:

- `OnlineVoting.Models` contains `Result<T>` and `ResultStatus`
- `OnlineVoting.Api` handles the conversion of results into HTTP responses

Controllers using `ToActionResult` were updated to reference the new API extension namespace.

## Affected Areas

The changes affect the following parts of the application:

- API request middleware
- global exception and status code handling
- Result-to-HTTP response handling
- application logging
- application startup configuration
- controllers using `ToActionResult`

The existing `ILoggerMessage`, `LoggerMessage`, and NLog setup were retained.

## Verification

The implementation was tested using the login endpoint with an unsuccessful login request.

The test confirmed that:

- the API returned an `X-Correlation-ID` response header
- the error response contained the correlation ID and trace ID
- the correlation ID in the response body matched the response header
- the same correlation ID appeared in the NLog application log
- the request method, path, status code, execution time, and trace ID were recorded

This confirms that a request can be traced from the API response to its corresponding application log entry.