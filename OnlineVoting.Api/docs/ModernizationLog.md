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
```

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

---

# Improve NLog Portability and Message Validation

## Overview

Updated the NLog configuration to remove local machine-specific paths and improved the handling of invalid encoded messages.

## Changes

### NLog Configuration

The NLog configuration previously used absolute paths pointing to the local development environment. This meant the same configuration would not 

work when the application is moved to another machine or deployed.

The log paths now use the application's current directory, so logs can be created regardless of where the application is running.

Exception details were also added to the NLog layout so unexpected errors include the exception and stack trace in the application log.

### Message Validation

While testing the logging changes with the `verify-user` endpoint, an invalid encoded token caused a `FormatException` and returned a `500 Internal Server Error`.

The message decoder was updated to validate the encoded value before returning the decoded message. Invalid or empty values now throw an 

`ArgumentException`, which is already handled as a `400 Bad Request` by the global exception handler.

## Affected Areas

- NLog configuration
- application and internal log paths
- exception logging
- message encoding and decoding

## Verification

The API was restarted and tested after changing the NLog paths. Application logs continued to be written correctly, including correlation IDs, 

trace IDs and request information.

The `verify-user` endpoint was also tested with an invalid encoded token. The test confirmed that the exception and stack trace were written to 

the application log, and the decoder was updated to handle invalid encoded values as bad input rather than an internal server error.

---

## Dockerization and Production Deployment

### Overview

The application was extended to support three separate environments:

- normal local development using SQL Server LocalDB;
- Docker-based development using SQL Server in a container;
- production hosting using MonsterASP.NET and a hosted Microsoft SQL Server database.

The goal was to make the application easier to run on a new machine, keep database migrations consistent across all environments and prepare 

the project for automated deployment.

The same application source code and Entity Framework Core migration files are used in all three environments. The databases and 

environment-specific configuration remain separate.

---

## Dockerizing the Application

The application was originally designed to run directly from Visual Studio against SQL Server LocalDB. Docker support was added without removing 

this existing development workflow.

The Dockerization was done incrementally so that the application could still be run normally while the containerized environment was being introduced.

### Step 1: Create the API Dockerfile

A Dockerfile was added at:

    OnlineVoting.Api/Dockerfile

The Dockerfile uses a multi-stage build.

The build stage restores the project dependencies, builds the application and publishes the ASP.NET Core API.

The runtime stage contains only the files required to run the published application.

This keeps the final API image smaller than an image containing the complete .NET SDK.

The API container is built from the same `OnlineVoting.Api` project that is used when running the application normally from Visual Studio.

### Step 2: Add `.dockerignore`

A `.dockerignore` file was added to prevent unnecessary local files from being copied into the Docker build context.

Files such as build output, IDE files, Git metadata, local environment files and other development artifacts do not need to be included when 

building the Docker image.

This reduces the Docker build context and prevents local configuration or secrets from accidentally becoming part of the image.

### Step 3: Add SQL Server to Docker Compose

A Docker Compose configuration was created at:

    OnlineVoting.Api/docker-compose.yml

Instead of requiring SQL Server to be installed or configured manually for the Docker environment, SQL Server runs as a container.

The Docker SQL Server service:

- uses SQL Server 2022;
- has its own database credentials;
- stores its database files in a persistent Docker volume;
- exposes SQL Server to the other containers through the Docker network;
- uses a health check to determine when SQL Server is ready.

The persistent volume allows the Docker database to survive normal container restarts.

### Step 4: Add the API Service

The ASP.NET Core API was then added to the same Docker Compose configuration.

Docker Compose places the API and SQL Server containers on the same Docker network.

The API therefore connects to SQL Server using the SQL Server service name rather than `localhost`.

This is important because `localhost` inside the API container refers to the API container itself, not the SQL Server container.

### Step 5: Separate Local and Docker Configuration

Normal development and Docker require different database connection strings.

Two local environment files are therefore used:

    OnlineVoting.Api/.env
    OnlineVoting.Api/.env.docker

`.env` contains configuration for normal local development.

`.env.docker` contains configuration used by Docker Compose.

Normal local development can connect to LocalDB while the Docker environment connects to the SQL Server service running inside Docker.

Both files contain environment-specific or sensitive values and are excluded from Git.

The relevant entries were added to `.gitignore`:

    /OnlineVoting.Api/.env
    /OnlineVoting.Api/.env.docker

The application was also changed so that `.env` is loaded only when the file exists. This allows production to use server environment variables 

without requiring a development `.env` file to be deployed.

### Step 6: Add SQL Server Readiness Checking

Starting the SQL Server container does not mean SQL Server is immediately ready to accept connections.

A SQL Server health check was therefore added to Docker Compose.

Services that depend on the database wait for SQL Server to become healthy before continuing.

This prevents the API or migration process from attempting to connect while SQL Server is still starting.

### Step 7: Add Database Migration Handling

Entity Framework Core migrations were integrated into the Docker workflow.

The Docker setup includes a migration generator service that checks whether migrations exist.

If the project has no migrations yet, it can generate the initial migration:

    InitialCreate

The generated migration is written back to the source project rather than remaining only inside a temporary container.

This is important because EF Core migrations are part of the application's source code and must be committed to Git.

If migrations already exist, migration generation is skipped.

The application itself uses the existing migration files to bring its development database to the expected schema.

### Step 8: Add the Migration/Publish Service

The Docker Compose setup also includes a migration/publish service.

This service prepares the application before the runtime API container starts.

The resulting published application is made available to the runtime container through the Docker setup.

This separates preparation of the application from the container responsible for serving API requests.

### Step 9: Add Persistent Docker Storage

A named Docker volume was configured for SQL Server.

This means:

    docker compose down

stops and removes the containers but preserves the database.

When a completely fresh database is required, the volume can intentionally be removed with:

    docker compose down -v

The `-v` option should therefore not be used as the normal way of stopping the application because it deletes the Docker database volume.

### Step 10: Integrate Docker Compose with Visual Studio

Docker Compose was connected to the Visual Studio solution through:

    docker-compose.dcproj

The solution now provides two clear ways of starting the application:

    Normal Api
    Docker Compose

`Normal Api` runs the ASP.NET Core application normally using the local development configuration.

`Docker Compose` starts the containerized environment.

The existing:

    OnlineVoting.Api/docker-compose.yml

remains the Compose configuration used by both the command line and Visual Studio.

This means Docker support was added without replacing the existing local development workflow.

---

## Running the Application

### Normal Local Development

For normal development, select:

    Normal Api

in Visual Studio and start the application.

This runs the API directly on the development machine and uses the configuration from:

    OnlineVoting.Api/.env

The local database is SQL Server LocalDB.

In the Development environment, available migrations are applied and development seed data is created automatically.

### Docker Compose from Visual Studio

Select:

    Docker Compose

as the startup profile and run the solution.

Visual Studio uses the existing Docker Compose configuration to start the required containers.

### Docker Compose from PowerShell

The same environment can be started without Visual Studio.

From the solution root:

    docker compose -f .\OnlineVoting.Api\docker-compose.yml up -d

Check all containers with:

    docker compose -f .\OnlineVoting.Api\docker-compose.yml ps -a

View API logs with:

    docker compose -f .\OnlineVoting.Api\docker-compose.yml logs online-voting-api --tail 100

View migration generator logs with:

    docker compose -f .\OnlineVoting.Api\docker-compose.yml logs migration-generator --tail 100

Stop the environment while preserving the database:

    docker compose -f .\OnlineVoting.Api\docker-compose.yml down

Completely reset the Docker environment and database:

    docker compose -f .\OnlineVoting.Api\docker-compose.yml down -v

After `down -v`, the next startup creates a fresh SQL Server database.

---

## Database Migration Strategy

The project uses one set of Entity Framework Core migration files for all environments.

The environments currently use:

    Local development
        -> SQL Server LocalDB

    Docker
        -> SQL Server 2022 container

    Production
        -> MonsterASP SQL Server

The databases are independent, but the migration source files are shared.

Each database records the migrations it has applied in:

    __EFMigrationsHistory

Creating a migration therefore does not automatically change every database. The migration is first created as source code and must then be 

applied to the required database.

### Testing the Migration Workflow

The migration workflow was tested by adding:

    public bool Active { get; set; }

to the `Contestant` entity.

A migration named:

    Added_Active_To_Contestant

was created.

During this change, the existing incorrectly named table:

    Contestans

was also corrected to:

    Contestants

through the migration.

The migration was applied and verified against LocalDB, Docker SQL Server and the hosted production SQL Server.

The migration history was checked to confirm that the databases contained:

    InitialCreate
    Added_Active_To_Contestant

This demonstrated that the same migration history can be used consistently across the three database environments.

### Workflow for Future Schema Changes

Future database changes should follow this process:

1. Modify the entity or database model.
2. Create a new EF Core migration.
3. Review the generated migration before applying it.
4. Apply and test it against LocalDB.
5. Test the application using Docker SQL Server.
6. Commit the model change and migration together.
7. Apply the migration deliberately to production.

Production migrations are not automatically executed when the production application starts.

---

## Development and Production Database Initialization

Development is allowed to initialize itself automatically.

The application currently uses:

    if (app.Environment.IsDevelopment())
    {
        await app.ApplyDatabaseMigrations();
        await SeedApplicationData.EnsurePopulated(app);
    }

This provides a convenient development experience while preventing the production application from unexpectedly changing its database every time 

it starts.

Migration handling was also moved out of the main startup code into an application extension method to keep `Program.cs` focused on application configuration.

Production migrations are applied deliberately rather than as part of normal application startup.

---

## Production Database Setup

A Microsoft SQL Server database was created on MonsterASP.NET.

The production application uses the database connection information provided by MonsterASP through its runtime environment configuration.

Remote database access was also enabled so that the production database can be inspected using SQL Server Management Studio.

The production database was initially empty. The existing EF Core migrations were applied to it and the migration history was checked afterward.

SSMS was then connected successfully to the hosted database, allowing the production tables and data to be inspected directly.

---

## Initial Production Data

Automatic development seeding was intentionally not enabled permanently in Production.

However, a completely new production database still required its initial users, roles, claims, faculty, department, gender records, and other required reference data.

A controlled production bootstrap setting was therefore introduced:

    Seed__RunOnce

For the initial setup it was temporarily configured as:

    Seed__RunOnce=true

The existing seed logic then initialized the required production data.

After confirming that the expected records had been created, the setting was changed back to:

    Seed__RunOnce=false

This keeps normal production startup free from automatic seeding while still providing an explicit mechanism for initializing a new environment.

---

## Production Configuration

Production configuration and secrets are not committed to Git.

MonsterASP environment variables are used for values such as:

    ConnectionStrings__VotingConnection
    JwtSettings__Secret
    JwtSettings__Issuer
    JwtSettings__Audience
    Seed__AdminUser__Email
    Seed__AdminUser__Password
    Seed__AdminUser__Username
    Seed__StudentUser__Email
    Seed__StudentUser__Password
    Seed__StudentUser__Username
    Seed__RunOnce

ASP.NET Core converts double underscores in environment-variable names to configuration sections.

For example:

    JwtSettings__Secret

maps to:

    JwtSettings:Secret

Development secrets remain in the ignored local environment files while production secrets remain in the hosting environment.

---

## JWT Configuration

Authentication testing exposed an issue with the JWT signing key.

The application uses HS256, but the original secret:

    MySecretKey1234567890

was only 168 bits.

The JWT library rejected it because the signing key must be at least 256 bits for HS256.

The local configuration was corrected first using a sufficiently long random secret. The production environment was then configured with its own strong secret.

Development and production do not need to use the same JWT secret.

Secrets themselves are not stored in this documentation or committed to the repository.

---

## First Production Deployment

Visual Studio Web Deploy was used for the initial production deployment to MonsterASP.

This was useful for establishing the hosting configuration and debugging the application before automating deployment.

Several hosting problems were discovered during this process.

### Incorrect `web.config`

The original deployment contained an incorrect `web.config` with:

- duplicate `aspNetCore` elements;
- a reference to `MyApplication.exe`;
- InProcess hosting;
- stale generated configuration under `bin` and `obj`.

The source file:

    OnlineVoting.Api/web.config

was corrected rather than manually fixing only the copy on the server.

Generated `bin` and `obj` directories were removed and rebuilt.

The repository was checked for stale configuration using:

    Get-ChildItem -Recurse -File |
        Select-String 'MyApplication.exe|hostingModel="InProcess"'

After the cleanup, the search returned no matches.

This ensured that subsequent deployments were generated from the corrected source configuration.

---

## Out-of-Process Hosting

The application initially failed under IIS with errors including:

    failed to load coreclr
    CLR worker thread exited prematurely

The API was changed from InProcess to Out-of-Process ASP.NET Core hosting.

The project configuration now contains:

    <AspNetCoreHostingModel>OutOfProcess</AspNetCoreHostingModel>

and `web.config` launches:

    dotnet
        -> OnlineVoting.Api.dll

using:

    hostingModel="outofprocess"

With this setup, IIS receives the public request and forwards it to the ASP.NET Core application running as a separate `dotnet` process.

This resolved the CoreCLR startup problem encountered on MonsterASP.

---

## HTTPS

Once HTTP hosting was working, the application was tested over HTTPS.

HTTP `/health` worked correctly while HTTPS initially produced connection resets.

A Let's Encrypt certificate was enabled for:

    online-voting-api.runasp.net

through the MonsterASP control panel.

The HTTPS binding was verified and HTTPS handling was enabled at the hosting level.

Because MonsterASP terminates the public HTTPS connection before forwarding the request to the Out-of-Process ASP.NET Core application, application-level HTTPS redirection was limited to Development:

    if (app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
    }

After the certificate and hosting configuration were corrected, the production API worked successfully over HTTPS.

---

## Production Verification

The production deployment was verified in stages rather than relying only on the browser opening the application.

The following endpoints were tested:

    /health
    /health/ready

Both returned:

    Healthy

The basic health endpoint confirmed that the deployed ASP.NET Core application was running.

The readiness endpoint verified the application's configured readiness checks.

Swagger was also enabled during production verification and loaded successfully after the hosting and HTTPS issues were resolved.

Authentication was then tested against the hosted database using the initial production user.

These checks confirmed that the request path was functioning through:

    HTTPS
        -> MonsterASP / IIS
        -> ASP.NET Core
        -> application services
        -> production SQL Server

---

## Current Environment Architecture

The project now supports three execution paths.

### Normal Development

    Visual Studio
        |
        v
    OnlineVoting.Api
        |
        v
    .env
        |
        v
    SQL Server LocalDB

### Docker Development

    Visual Studio / Docker Compose
        |
        v
    OnlineVoting.Api container
        |
        v
    Docker network
        |
        v
    SQL Server container
        |
        v
    Persistent Docker volume

### Production

    Internet / HTTPS
        |
        v
    MonsterASP IIS
        |
        v
    ASP.NET Core Out-of-Process
        |
        v
    MonsterASP environment variables
        |
        v
    MonsterASP SQL Server

The environments have separate databases and secrets but share the application source code and EF Core migrations.

---

## Deployment Source of Truth

Visual Studio Web Deploy was useful for proving that the application could run successfully on MonsterASP, but it should not remain the normal 

production deployment method.

Manual publishing from a developer machine makes it easier for deployed files to differ from the repository.

The GitHub repository will therefore become the single source of truth for deployment.

The target workflow is:

    Local development
        |
        v
    Local tests
        |
        v
    Docker verification
        |
        v
    Git commit
        |
        v
    GitHub
        |
        v
    GitHub Actions
        |
        +--> Restore
        +--> Build
        +--> Test
        +--> Docker build verification
        |
        v
    Production deployment
        |
        v
    MonsterASP

Production runtime secrets will remain in MonsterASP rather than being committed to Git.

The existing GitHub Actions workflow already restores and builds the solution and verifies that the Docker image can be built.

---

### Git-based production deployment

Previously, the API was published to MonsterASP directly from Visual Studio. Although this worked, it meant that the version running in 

production could depend on what was available on a developer's local machine.

To keep the repository as the single source of truth, production deployment was changed to use MonsterASP Git Deploy.

MonsterASP is now connected to the GitHub repository and configured to build:

- Repository: `Slimcent/Online_Voting_Sys`
- Branch: `main`
- Project: `OnlineVoting.Api/OnlineVoting.Api.csproj`
- Deployment type: `Build (.NET)`

The normal workflow is now:

`Feature branch -> development -> main -> production`

Changes are developed on a feature branch and merged into `development` through a pull request. Once the changes have been tested and are ready 

for production, `development` is merged into `main`.

MonsterASP then pulls the application from `main`, builds the API and publishes the result to the website.

During the first Git deployment, the application returned HTTP 500 errors because `main` was still behind the feature and development branches. 

The latest Dockerization and deployment changes had not yet reached `main`.

The feature branch was therefore merged into `development`, followed by `development` into `main`. After deploying again from the updated `main` branch, 

the application started successfully.

The following production endpoints were checked after deployment:

- `/health`
- `/health/ready`
- `/swagger/index.html`

All endpoints worked correctly over HTTPS.

Going forward, Visual Studio publishing will not be the normal way of deploying the application. Production should come from the `main` branch 

so that the code in GitHub and the code running in production remain consistent.

---

### Publishing Docker Images to Docker Hub

The CI workflow was extended to publish the Docker image to Docker Hub after a successful build.

The image is available as:

```text
slimcent/online-voting
```

Images are published when changes are pushed or merged into `development` or `main`. Pull requests still build the Docker image as part of CI, but they do not publish it.

The branch determines the image tag:

```text
development → slimcent/online-voting:development
main        → slimcent/online-voting:latest
```

Each build is also tagged with its Git commit SHA. This gives us a specific image for every published version instead of relying only on `development` 

or `latest`.

For example:

```text
slimcent/online-voting:development
slimcent/online-voting:949add315724c11b6...

slimcent/online-voting:latest
slimcent/online-voting:b9b27431558bed7c7...
```

The Docker Hub credentials used by the workflow are stored as GitHub repository secrets and are not committed to the repository.

The workflow was tested with both branches. A merge into `development` successfully published the `development` image and a later merge 

into `main` published the `latest` image. The corresponding commit SHA images were also created.

MonsterASP continues to use the existing .NET/Web Deploy process for production. The Docker image is published separately so that the application 

already has a container image available if we later move to a hosting environment that supports Docker deployment.

---

# Automated Testing

## Overview

Automated tests were added to the project to make it easier to verify changes during the modernization.

Testing was introduced gradually, starting with the models, followed by the services and controllers. Once the tests were stable locally, they were 

added to the CI pipeline so that they run automatically when code is pushed.

The testing work followed this order:

```text
Models
  |
  v
Services
  |
  v
Controllers
  |
  v
CI
```

---

## Test Setup

The tests are contained in the `OnlineVoting.Tests` project and use xUnit as the test framework.

Moq is used for mocking dependencies, while Entity Framework Core InMemory and SQLite are available for tests that require database behaviour.

The main test packages are:

```text
Microsoft.NET.Test.Sdk
xunit
xunit.runner.visualstudio
Moq
coverlet.collector
Microsoft.EntityFrameworkCore.InMemory
Microsoft.EntityFrameworkCore.Sqlite
SQLitePCLRaw.lib.e_sqlite3
```

The test project references:

```text
OnlineVoting.Api
OnlineVoting.Models
OnlineVoting.Services
VotingSystem.Data
```

The current test structure is:

```text
OnlineVoting.Tests
|
+-- UnitTests
|
+-- IntegrationTests
|   |
|   +-- Api
|   |
|   +-- Data
|   |
|   +-- Database
|
+-- TestData
    |
    +-- Constants
    |
    +-- Data
    |
    +-- Factories
    |
    +-- Fixtures
```

Shared test data and setup are kept under `TestData` instead of being repeated across test classes.

---

## Model Tests

Testing started with the models.

These tests cover the behaviour of the request, response, entity, pagination and other shared models used by the application.

Starting here made it possible to verify the objects used by the rest of the application before testing the business and API layers.

---

## Service Tests

After the model tests, tests were added and expanded for the service layer.

The service tests cover the application's business logic without going through the controllers.

The tested areas include:

- roles;
- claims;
- faculties;
- departments;
- email operations;
- user-related operations.

Both successful operations and relevant failure cases are tested.

The service tests also cover the `Result<T>` pattern introduced during the modernization. Services can return statuses such as:

```text
Success
Created
NoContent
ValidationError
NotFound
Conflict
Unauthorized
Forbidden
```

This allows service behaviour to be tested directly without depending on ASP.NET Core HTTP responses.

---

## Email Service Tests

Tests were added for the main email operations, including:

- account creation;
- password reset;
- voter registration.

These tests verify the email data and token generation without sending real emails.

The email service uses:

```text
OnlineVoting.Api/Template/EmailTemplate.html
```

The test project copies this template to its output directory so that it is available when the email tests run.

The following configuration was added to `OnlineVoting.Tests.csproj`:

```xml
<ItemGroup>
    <None Include="..\OnlineVoting.Api\Template\EmailTemplate.html">
        <Link>Template\EmailTemplate.html</Link>
        <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
        <TargetPath>Template\EmailTemplate.html</TargetPath>
    </None>
</ItemGroup>
```

---

## Controller Tests

Controller tests were added after the corresponding services had been tested.

The services are mocked in these tests because the business logic is already covered by the service tests. The controller tests instead check 

that request values are passed correctly to the service and that the returned `Result<T>` is converted into the expected HTTP response.

Controller tests were added for:

```text
ClaimsController
DepartmentController
FacultyController
RolesController
```

### ClaimsController

The claims controller tests cover its calls to `IClaimsService` and the handling of the returned results.

The tests were also updated when the controller was changed to inherit from `BaseController`.

### DepartmentController

The department controller tests cover:

```text
CreateDepartment
GetDepartments
GetDepartment
GetDepartmentsByFacultyId
GetDepartmentsByFacultyId with pagination
UpdateDepartment
ToggleDepartmentActivation
DeleteDepartment
```

They verify request bodies, department and faculty IDs, pagination parameters, and the calls made to `IDepartmentService`.

The tests also cover the endpoint names added during the controller cleanup:

```text
Create-Department
Get-Departments
Get-Department
Get-Departments-By-Faculty
Get-Paged-Departments-By-Faculty
Update-Department
Department-Activation
Delete-Department
```

### FacultyController

The faculty controller tests cover:

```text
CreateFaculty
GetFaculties
GetFaculty
UpdateFaculty
ToggleFacultyActivation
DeleteFaculty
GetFacultiesWithDepartments
GetFacultyWithDepartments
```

They verify the values passed to `IFacultyService` and the HTTP results returned by the controller.

The faculty endpoints were also tested after their URL templates and endpoint names were standardised.

### RolesController

The role service was already covered by service tests, so the `RolesController` tests focus on the controller itself.

They verify that the correct `IRolesService` methods are called and that the service results are converted correctly into HTTP responses.

---

## Result Response Handling

The modernized controllers use:

```csharp
return result.ToActionResult(this);
```

`ResultActionResultExtensions` handles the conversion from service results to HTTP responses:

```text
Success         -> 200 OK
Created         -> 201 Created
NoContent       -> 204 No Content
ValidationError -> 400 Bad Request
Unauthorized    -> 401 Unauthorized
Forbidden       -> 403 Forbidden
NotFound        -> 404 Not Found
Conflict        -> 409 Conflict
```

This response handling is covered by the controller tests and avoids repeating the same response logic in every controller action.

---

## Local Verification

Tests were run throughout the modernization rather than waiting until all changes were complete.

Relevant test groups were run after each change, followed by the complete test suite once the model, service, and controller tests were in place.

After the tests were passing locally, the next step was to run them automatically through GitHub Actions.

---

# Automated Testing in CI

## CI Test Job

The GitHub Actions workflow was updated to run the test suite automatically.

Build and Test are separate jobs:

```text
Build ----+
          |
          +----> Docker
          |
Test -----+
```

The Build job restores and builds the solution in Release configuration.

The Test job performs its own setup and runs:

```text
dotnet test Online_Voting_Sys.sln --configuration Release --no-restore
```

Keeping them separate makes it clear in GitHub Actions whether a failure comes from the build or from the tests.

The Docker job depends on both jobs, so it only continues when Build and Test have succeeded.

---

## CI Triggers

The workflow runs on every push.

Pull requests targeting `development` or `main` also run the workflow.

For feature branch pushes, Build and Test run automatically.

For pull requests to `development` or `main`, the Docker image is also built for verification without being published.

For pushes or merges to `development` and `main`, Docker publishing only continues after Build and Test have passed.

Production deployment remains limited to `main`.

---

## Cross-Platform Email Template Fix

The first CI test run exposed an issue with the email template path.

The email template was previously loaded using:

```
Directory.GetCurrentDirectory() + "\\Template\\EmailTemplate.html"
```

This worked on Windows but failed on the Ubuntu runner used by GitHub Actions.

Three email tests failed because the template could not be found:

```
SendResetPasswordEmail_WithExistingUser_ShouldGenerateTokenAndSendEmail
SendVoterEmail_ShouldSendEmail
SendCreateUserEmail_ShouldGenerateTokensAndSendEmail
```

The template path was changed to:

```
Path.Combine("Template", "EmailTemplate.html")
```

and the file is resolved relative to:

```
AppContext.BaseDirectory
```

This removed the Windows-specific path and made the template loading work on both Windows and Linux.

---

## CI Verification

After fixing the template path, the complete test suite passed on the Ubuntu GitHub Actions runner:

```
Failed:   0
Passed: 419
Skipped: 0
Total:  419
```

The current CI flow is:

```
Push / Pull Request
        |
   +----+----+
   |         |
 Build      Test
   |         |
   +----+----+
        |
      Docker
        |
 development/main
        |
    Docker Hub
        |
    main only
        |
      Deploy
```

Tests are now part of the normal development workflow. A failed build or failed test stops the Docker job and prevents the change from continuing to the later deployment stages.

---

# JWT and Authorization Improvements

The JWT and authorization code was reviewed and cleaned up without changing the existing authentication or permission behaviour.

## Authorization Logging

Logging was added to `CustomAuthorizationHandler` to make authorization decisions easier to trace.

Successful authorization is logged as information, while denied authorization is logged as a warning.

```
_loggerMessage.LogInfo($"Authorization succeeded for user {userId}. Required claim: {routeClaim}");

_loggerMessage.LogWarn($"Authorization denied for user {userId}. Required claim: {routeClaim}");
```

The existing lowercase claim comparison was kept because claim values are not always stored with the same casing.

## JWT Cleanup

`JwtAuthenticator` was cleaned up by:

- removing the unused `IdentityOptions`;
- removing unnecessary `async` usage;
- replacing `var` with explicit types;
- marking the optional `expires` and `additionalClaims` parameters as nullable;
- using UTF-8 for the signing key to match JWT validation.

JWT generation is also logged without writing the token or secret to the logs.

```
_loggerMessage.LogInfo($"JWT token generated for user {user.Id} with role {role}");
```

The existing token claims, expiry behaviour, issuer, audience and signing algorithm were not changed.

## Verification

The project was built and the existing tests were run after the changes.

The JWT and authorization behaviour remained unchanged while the implementation became cleaner and easier to trace.

---

# Controller and Endpoint Improvements

The API controllers were reviewed to make their routes, endpoint names, response handling and documentation more consistent.

## Controller Consistency

Controllers being modernized were updated to inherit from `BaseController` so that common API behaviour is handled consistently.

The controllers also use the shared `Result<T>` response handling where applicable:

```
return result.ToActionResult(this);
```

This keeps HTTP status handling out of the individual controller actions.

## Route and Endpoint Names

The Faculty and Department endpoints were updated to use clear URL templates instead of relying only on route parameters.

```
[HttpGet("faculty/{id:long}", Name = "Get-Faculty")]

[HttpGet("department/{id:long}", Name = "Get-Department")]

[HttpGet("departments-by-faculty/{facultyId:long}", Name = "Get-Departments-By-Faculty")]

[HttpGet("paged-departments-by-faculty/{facultyId:long}", Name = "Get-Paged-Departments-By-Faculty")]
```

Each endpoint also has an explicit `Name`. These names are important because the custom authorization handler uses them when checking the user's claims.

## API Documentation

The Faculty and Department endpoints were connected to the custom Swagger documentation system using `ApiDocumentation`.

Documentation keys were added for the individual operations and mapped to their definitions in:

```
FacultyDocumentation.cs
DepartmentDocumentation.cs
```

Example Usage:

```
[ApiDocumentation(FacultyDocumentationKeys.GetFaculty)]

[ApiDocumentation(DepartmentDocumentationKeys.GetDepartment)]
```

The documentation includes the endpoint summary, description, response type and expected error responses.

## Verification

The controller changes were covered by the controller tests added earlier.

The tests passed after the route, endpoint name, `BaseController`, and documentation changes, confirming that the cleanup did not break the existing controller behaviour.

---

## Refresh Token Rotation and Session Revocation

### Overview

Refresh-token support was added so users can renew access tokens without signing in again while still keeping sessions revocable and auditable.

The implementation includes:

- cryptographically secure refresh tokens;
- SHA-256 hashing before database storage;
- `HttpOnly` and `Secure` cookies;
- refresh-token rotation;
- token-family tracking;
- absolute family expiry;
- logout and logout-all support;
- token reuse detection;
- optimistic concurrency using `RowVersion`;
- IP address and user-agent tracking.

The raw refresh token is never stored in the database.

### Token Rotation

Each refresh token is single-use.

When a refresh succeeds:

1. the current refresh token is revoked;
2. a replacement token is created in the same family;
3. the old token stores the hash of the replacement;
4. the database changes are committed inside one transaction;
5. the new cookie is written only after the transaction commits.

If the refresh operation fails, the transaction is rolled back and the failed request does not create its own replacement token.

### Token Reuse Detection

During testing, a rotated token was initially rejected as a normal revoked token before the reuse-detection logic could run.

Validation was updated so that a revoked token with a `ReplacedByTokenHash` is treated as a reused rotated token.

When reuse is detected, all active tokens in that family are revoked.

Tokens revoked through normal logout are still rejected, but they are not treated as rotation reuse.

### Concurrent Refresh Requests

`RowVersion` is used to protect against two requests trying to rotate the same refresh token at the same time.

The losing request receives a concurrency failure, rolls back its transaction and revokes the active token created by the successful request.

The final concurrent test produced:

```
Request 1 -> HTTP 200
Request 2 -> HTTP 401
```

---

---

# Audit Trail

## Overview

The audit trail was added as a new backend feature to record important operations performed on application data.

The audit trail should answer:

- Who performed the operation?
- What endpoint/business operation triggered it?
- What entity was affected?
- Was the entity created, updated, or deleted?
- What values changed?
- Did the operation succeed?
- What request metadata was associated with the operation?
- When did the operation occur?

The implementation is centralized so that individual controllers and services do not need to manually create audit records.

---

## Objectives

The audit trail was introduced to:

- provide traceability for important data changes
- identify the user responsible for an operation
- record the affected entity and operation type
- retain previous and new values for relevant changes
- associate operations with their originating HTTP requests
- provide a human-readable description of each audited operation
- support approximate IP-based location information
- optionally store client-provided device location
- prevent historical audit records from being modified or deleted
- provide a protected API for reviewing audit records

---

## Audit Entities

The audit implementation introduced the following entities:

- `AuditTrail`
- `AuditOutcome`
- `AuditLocation`

### AuditTrail

`AuditTrail` contains the main information about an audited operation.

Stored information includes:

- audit identifier
- actor user ID
- actor username
- endpoint name
- event name
- HTTP method
- entity type
- entity ID
- outcome
- description
- old values
- new values
- IP address
- user agent
- correlation ID
- creation timestamp
- optional location information

### AuditOutcome

`AuditOutcome` represents the result of an audited operation.

The following outcomes were seeded:

- `Success`
- `Failure`
- `Denied`

The values are resolved by name rather than by hardcoded database IDs.

Successful automatic auditing resolves:

```
Success
```

### AuditLocation

`AuditLocation` stores optional location information associated with an audit record.

The relationship is:

```
AuditTrail 1 ───── 0..1 AuditLocation
```

`AuditLocation` has its own primary key.

`AuditTrailId` is a unique foreign key referencing the corresponding audit trail.

The relationship therefore allows:

- an audit trail without location information
- one location record for an audit trail
- no more than one location record for the same audit trail

---

## Auditable Entities

Automatic persistence auditing is enabled using the `IAuditable` marker interface.

Entities implementing `IAuditable` are inspected by `VotingDbContext` when changes are saved.

This avoids placing audit logic inside individual controllers or services.

The audit infrastructure automatically handles:

- entity creation
- entity updates
- entity deletion

Some entities are intentionally excluded from the general automatic audit mechanism.

In particular:

- `Vote`
- `RefreshToken`

do not implement `IAuditable`.

This prevents sensitive or inappropriate information from being captured through the general entity-change audit mechanism.

---

## Automatic Audit Creation

`VotingDbContext` was extended so that auditable changes are detected automatically during `SaveChanges` and `SaveChangesAsync`.

The general save flow is:

```text
Business entity changed
        ↓
VotingDbContext detects auditable changes
        ↓
Prepare pending audit information
        ↓
Save business entity
        ↓
Resolve generated entity IDs
        ↓
Create AuditTrail records
        ↓
Save audit records
        ↓
Commit transaction
```

Generated entity IDs are resolved after the initial business save.

This is necessary for entities whose IDs are generated by the database.


## Transaction Handling

Automatic auditing is performed within the same transaction as the business change.

If the context does not already have an active transaction, `VotingDbContext` creates one.

The transaction contains:

1. the business entity change
2. the generated audit record

If either operation fails, the transaction is rolled back.

This prevents a situation where the application successfully changes business data but fails to create the corresponding audit record.

If an existing transaction is already active, the audit implementation uses that transaction instead of creating another one.

The value returned by `SaveChanges` continues to represent the number of affected business records rather than the additional internal audit save.

---

## Audit Events

The automatic audit trail currently records three main entity events:

- `Created`
- `Updated`
- `Deleted`

The event name is derived from the entity state.

---

## Tracking-Only Changes

Framework-maintained tracking properties should not create unnecessary audit events.

Properties such as:

- `CreatedAt`
- `UpdatedAt`
- `CreatedBy`
- `UpdatedBy`

are excluded from the entity change snapshots.

If only tracker properties change and no meaningful business property changes, an `Updated` audit record is not created.

This prevents the audit trail from being filled with changes that only represent internal tracking maintenance.

---

## Sensitive Data Exclusion

Sensitive properties are excluded from `OldValues` and `NewValues`.

This prevents the audit trail from becoming another storage location for authentication or security secrets.

Sensitive values such as the following are not intended to be recorded:

- password hashes
- passwords
- security stamps
- authentication tokens
- refresh-token values
- voting codes
- other security-related values

The audit trail should provide traceability without exposing secret information.

---

## Human-Readable Description

Each automatically generated audit record receives a short human-readable description.

Examples:

```
Faculty 1 was created by super.admin.
Faculty 1 was updated by super.admin.
Faculty 2 was deleted by super.admin.
Department 3 was created by super.admin.
```

If an authenticated username is unavailable, the operation is described as being performed by the system.

```
Faculty 1 was created by the system.
```

The description provides a quick summary of the operation.

The detailed change information remains available through `OldValues` and `NewValues`.

---

## Request Metadata

Audit records also contain metadata from the HTTP request that triggered the operation.

The request metadata includes:

- actor user ID
- actor username
- endpoint name
- HTTP method
- IP address
- user agent
- correlation ID

Request metadata is provided through `IAuditMetadataProvider`.

The implementation uses the current `HttpContext` to retrieve request information without making controllers responsible for audit metadata.

---

## Correlation IDs

The existing correlation ID infrastructure is integrated into the audit trail.

Each relevant request receives a correlation ID.

That correlation ID is also stored with the corresponding audit record.

This allows a request to be traced across:

```text
HTTP request
        ↓
Application logs
        ↓
AuditTrail
```

An administrator or developer can therefore use the same correlation ID to connect an audit entry with the logs produced during the request.

---

# Audit Location

## Overview

Location support was added as optional enrichment for audit records.

Location information is stored in a separate `AuditLocation` entity rather than directly adding all location fields to `AuditTrail`.

An audit record can contain:

- approximate IP-based location
- optional device/browser location
- both
- neither

Location information must never be required for the business operation to succeed.

---

## IP-Based Location

The backend can determine an approximate geographic location from the public client IP address.

The following values may be stored:

- `IpCountry`
- `IpRegion`
- `IpCity`
- `IpLatitude`
- `IpLongitude`

Example:

```
{
  "ipCountry": "Germany",
  "ipRegion": "North Rhine-Westphalia",
  "ipCity": "Paderborn",
  "ipLatitude": 51.7189,
  "ipLongitude": 8.7575
}
```

---

## IP Geolocation Service

IP geolocation is handled through:

```
IIpGeolocationService
```

with the current implementation:

```
IpGeolocationService
```

The external provider currently used is:

```
ipwho.is
```

The frontend does not call the geolocation provider.

The lookup is performed directly by the backend.

The request flow is:

```
Incoming API request
        ↓
IpGeolocationMiddleware
        ↓
context.Connection.RemoteIpAddress
        ↓
IpGeolocationService
        ↓
ipwho.is
        ↓
Country / Region / City / Coordinates
        ↓
HttpContext.Items
        ↓
AuditMetadataProvider
        ↓
AuditLocation
```

---

## IP Geolocation Middleware

`IpGeolocationMiddleware` performs the IP lookup before the request reaches the controller.

Location lookup is limited to mutating API requests.

The middleware currently considers:

- `POST`
- `PUT`
- `PATCH`
- `DELETE`

`GET` requests do not trigger IP geolocation.

This avoids unnecessary external lookups for read-only requests.

The middleware also checks that the request is an API request before attempting the lookup.

---

## Private and Reserved IP Addresses

Private, loopback and reserved addresses are not sent to the external geolocation provider.

Examples include:

```text
127.0.0.1
::1
10.x.x.x
172.16.x.x - 172.31.x.x
192.168.x.x
```

These addresses do not represent a meaningful public geographic location.

This also means that normal local development requests will generally not contain IP geolocation information.

A request from:

```
127.0.0.1
```

will still create an audit record, but no IP location lookup will be performed.

---

## IP Geolocation Failure Handling

Location enrichment does not affect the underlying business operation.

If the geolocation provider:

- is unavailable
- times out
- returns an unsuccessful response
- returns invalid JSON
- cannot resolve the IP address

the service returns no location information and the original request continues.

The audit trail is still created.

This keeps geolocation as optional audit enrichment rather than a dependency for application functionality.

---

## IP Geolocation Timeout

The IP geolocation HTTP client uses a short timeout.

The current timeout is:

```
2 seconds
```

This prevents an unavailable external geolocation service from delaying API requests for an extended period.

---

## IP Geolocation Caching

Successful IP geolocation results are cached using the ASP.NET Core in-memory cache.

The cache key is based on the client IP address.

Successful results are currently cached for:

```
6 hours
```

This reduces:

- repeated external HTTP calls
- request latency
- load on the external provider
- unnecessary use of provider request limits

---

# Device Location

## Overview

The backend also supports optional device/browser location.

Unlike IP location, the backend cannot independently access a browser or phone GPS sensor.

The browser or client application must first obtain permission from the user and then send the location information with the API request.

Device location is therefore optional client-provided metadata.

---

## Device Location Headers

The backend currently accepts device location through the following HTTP headers:

```text
X-Device-Latitude
X-Device-Longitude
X-Device-Accuracy
X-Device-Location-Captured-At
```

When valid coordinates are provided, the following values may be stored:

- `DeviceLatitude`
- `DeviceLongitude`
- `DeviceAccuracyMeters`
- `DeviceLocationCapturedAt`

---

## Device Location Validation

Device coordinates are validated before being accepted.

Latitude must be between:

```
-90 and 90
```

Longitude must be between:

```
-180 and 180
```

Accuracy must not be negative.

Invalid or missing location values are ignored rather than causing the request to fail.

Date/time values are normalized to UTC where appropriate.

---

## Device Location Request Flow

The client-side flow is:

```
Browser requests geolocation permission
        ↓
navigator.geolocation
        ↓
Latitude / Longitude / Accuracy
        ↓
Frontend adds location headers
        ↓
Existing API request
        ↓
AuditMetadataProvider
        ↓
AuditLocation
```

No dedicated device-location endpoint is required.

The device location can be attached to the same request that performs the business operation.

For example:

```
POST /api/v1/faculties
```

may contain the device location headers.

The controller does not need to read them.

The audit infrastructure receives the values automatically through `AuditMetadataProvider`.

---

# Reverse Proxy Support

## Forwarded Headers

The application may run behind a reverse proxy.

Without forwarded-header handling, ASP.NET Core may see the reverse proxy's IP address instead of the original client's IP address.

Forwarded-header support was therefore added.

The application processes:

- `X-Forwarded-For`
- `X-Forwarded-Proto`

before the IP geolocation middleware runs.

The request flow becomes:

```
Client
    ↓
Reverse proxy
    ↓
X-Forwarded-For
    ↓
UseForwardedHeaders()
    ↓
RemoteIpAddress corrected
    ↓
IpGeolocationMiddleware
```

---

## Trusted Proxies

Forwarded headers are accepted only from trusted proxies.

Known proxies can be configured using application configuration.

For example:

```
ReverseProxy__KnownProxies__0=10.0.0.10
ReverseProxy__KnownProxies__1=10.0.0.11
```

These are configuration examples only.

Actual proxy addresses depend on the production deployment environment.

The application does not clear the framework's trusted proxy restrictions to blindly trust every forwarded header.

This prevents clients from spoofing `X-Forwarded-For` and controlling the IP address recorded by the audit system.

---

# Audit Record Protection

## Append-Only Audit History

Audit records are treated as append-only historical information.

After an `AuditTrail` has been created, attempts to:

- modify it
- delete it

through `VotingDbContext` are rejected.

The same protection applies to `AuditLocation`.

This prevents normal application operations from rewriting historical audit information.

An attempt to modify or delete an audit record results in an `InvalidOperationException`.

```
Audit trail records cannot be modified or deleted.
```

This behavior is enforced centrally by `VotingDbContext`.

---

# Audit Trail API

## Endpoint

A protected endpoint was added for retrieving audit records:

```
GET /api/v1/audit-trails
```

The controller uses the existing service and Result-pattern architecture.

The controller remains thin:

```
Request
    ↓
AuditTrailService
    ↓
Result<PagedResponse<AuditTrailResponse>>
    ↓
HTTP response
```

---

## Audit Filtering

The audit endpoint supports optional filtering by:

- actor user ID
- actor username
- endpoint name
- event name
- entity type
- entity ID
- outcome
- correlation ID
- IP address
- start date
- end date

Pagination is also supported.

This allows administrators to investigate specific users, operations or entities without retrieving the complete audit history.

---

## Audit Pagination

Audit records are returned through the existing pagination infrastructure.

The response uses:

```
PagedResponse<AuditTrailResponse>
```

---

# Audit Response

## Improved Response Mapping

The audit response includes:

- audit ID
- actor information
- endpoint information
- event
- HTTP method
- entity information
- outcome
- description
- old values
- new values
- IP address
- user agent
- correlation ID
- location
- creation timestamp

---

## Outcome Mapping

`AuditTrail` stores the relationship to `AuditOutcome`.

The API response exposes only the outcome name.

Example:

```
{
  "outcome": "Success"
}
```

rather than exposing the complete `AuditOutcome` database entity.

---

## Location Mapping

`AuditLocation` is mapped into a dedicated:

```
AuditLocationResponse
```

The audit service explicitly loads:

```
Outcome
Location
```

before the response is mapped.

This ensures location metadata is included when it exists.

---

## Example Audit Response

A typical audit record can be returned as:

```
{
  "id": "23e6f088-c24c-4488-8332-8bea2a923d46",
  "actorUserId": "475d1d1c-4659-4140-b1f0-105e56b27b5d",
  "actorUsername": "super.admin",
  "endpointName": "Update-Faculty",
  "eventName": "Updated",
  "httpMethod": "PUT",
  "entityType": "Faculty",
  "entityId": "1",
  "outcome": "Success",
  "description": "Faculty 1 was updated by super.admin.",
  "oldValues": {
    "Name": "Media Sciences"
  },
  "newValues": {
    "Name": "Media Technology"
  },
  "ipAddress": "93.x.x.x",
  "userAgent": "Mozilla/5.0",
  "correlationId": "9be8769f-53de-45f7-923a-7df0e17ae6a2",
  "location": {
    "ipCountry": "Germany",
    "ipRegion": "North Rhine-Westphalia",
    "ipCity": "Paderborn",
    "ipLatitude": 51.7189,
    "ipLongitude": 8.7575,
    "deviceLatitude": null,
    "deviceLongitude": null,
    "deviceAccuracyMeters": null,
    "deviceLocationCapturedAt": null
  },
  "createdAt": "2026-08-28T16:20:00Z"
}
```

---

# Database Changes

The audit implementation introduced the following database tables:

```text
AuditOutcomes
AuditTrails
AuditLocations
```

The `AuditLocations` table contains:

- `Id`
- `AuditTrailId`
- `IpCountry`
- `IpRegion`
- `IpCity`
- `IpLatitude`
- `IpLongitude`
- `DeviceLatitude`
- `DeviceLongitude`
- `DeviceAccuracyMeters`
- `DeviceLocationCapturedAt`

`AuditTrailId` has a unique index so that a single audit trail cannot have multiple location records.

The relationship uses restricted delete behavior.

---

# Seed Data

Audit outcome seed data was added.

The seeded outcomes are:

```
Success
Failure
Denied
```

# Testing

Audit-related tests were added across the context, service, mapper, middleware and geolocation layers.

## VotingDbContext Audit Tests

Tests cover:

- creation of an audit trail when an auditable entity is created
- correct actor metadata
- endpoint metadata
- event name
- HTTP method
- entity type
- generated entity ID
- success outcome
- IP address
- user agent
- correlation ID
- created entity values
- detached update old-value retrieval
- updated values
- deleted entity old values
- exclusion of tracker-only changes
- append-only audit protection
- IP location persistence
- device location persistence
- human-readable descriptions

---

## Audit Architecture Tests

Architecture tests verify that entities that should not participate in automatic entity auditing remain excluded.

In particular:

```
Vote
RefreshToken
```

are verified not to implement `IAuditable`.

---

## AuditTrailService Tests

Integration tests cover:

- retrieving audit records without filters
- actor filtering
- entity filtering
- event filtering
- outcome filtering
- correlation ID filtering
- IP address filtering
- endpoint filtering
- date filtering
- pagination
- loading associated location information

---

## Audit Mapping Tests

Dedicated mapping tests verify:

- `AuditTrail` to `AuditTrailResponse`
- outcome name mapping
- `AuditLocation` to `AuditLocationResponse`
- IP location mapping
- device location mapping
- deserialization of `OldValues`
- deserialization of `NewValues`
- created entity null old-values behavior
- updated entity old/new behavior
- deleted entity null new-values behavior

---

## IP Geolocation Service Tests

Tests cover:

- successful provider responses
- unsuccessful provider responses
- null handling
- HTTP failure handling
- caching behavior

---

## IP Geolocation Middleware Tests

Middleware tests cover:

- public IP lookup for mutating requests
- loopback address skipping
- private address skipping
- GET request skipping
- unsuccessful geolocation lookup
- continuing the request when no location is available
- populating request context when location is available

---

## Final Verification

After the audit trail, location support, response improvements and associated tests were completed, the full test suite was executed.

All tests passed successfully.

---

## Application Caching

### Goal
Add a reusable caching layer to reduce repeated database reads without tying the service layer to a specific cache implementation.

### What Changed
- Added a new `OnlineVoting.Caching` class library to keep caching concerns separate from the rest of the application.
- Added `ICacheService` as the abstraction used by the service layer.
- Implemented the cache service with ASP.NET Core `HybridCache`.
- Added support for:
  - Hybrid caching
  - Local-only caching
  - Distributed-only caching
  - Cache expiration
  - Local cache expiration
  - Cache tags
  - Cache removal by key
  - Cache removal by tag
- Added a `Caching` configuration section in `appsettings.json`.
- Added Redis-related configuration so distributed caching can be enabled later without changing the service layer.
- Added shared cache keys, tags, and policies under the Services project.
- Kept the actual cache implementation out of the business services. The services only depend on `ICacheService`.

### Faculty as the First Implementation
Faculty operations were used as the first place to apply and test the caching approach.

This allowed the basic flow to be verified before applying the same pattern elsewhere:
- Cache key generation.
- Cache hits and cache misses.
- Caching DTO responses instead of EF Core entities.
- Caching paginated responses.
- Caching Faculty responses with and without Departments.
- Invalidating the cache after successful create, update, activation, and delete operations.
- Avoiding cache invalidation when a write fails or does not change anything.
- Making sure repository and mapping calls are skipped when the requested data is already cached.

Once the Faculty implementation and tests were working correctly, the same approach was applied to Department operations.

### Department Caching
Caching was added to the main Department read operations:
- Get Department by ID.
- Get paginated Departments.
- Get Departments by Faculty.
- Get paginated Departments by Faculty.

Cache-hit tests were also added to confirm that repository and mapping calls are not made when the response is already available in the cache.

### Cache Invalidation
Faculty and Department data are related, so updating one can make cached data for the other stale.

- Faculty responses can include Departments.
- Department responses include Faculty information.

Because of this:
- Successful Faculty changes invalidate both Faculty and Department caches.
- Successful Department changes invalidate both Department and Faculty caches.

Invalidation only happens after the database operation succeeds. Validation failures, conflicts, not-found results, and other no-op cases do not clear the cache.

### Cache Failure Handling
Cache failure handling was added inside `OnlineVoting.Caching` instead of adding `try/catch` blocks throughout the services.

Failures from:
- `Set`
- `Remove`
- `RemoveByTag`

are logged and do not cause an already successful business operation to fail.

If a Department update succeeds in SQL Server but cache invalidation fails, the update still returns successfully and the cache error is logged.

Cancellation is handled differently. If the request is cancelled, `OperationCanceledException` is allowed to propagate instead of being treated as a normal cache failure.

`GetOrCreate` was left without a broad fallback catch because the factory can contain repository or database calls. Catching everything there and 

running the factory again could repeat a database operation or hide the real exception.

### Logging
The existing `VotingSystem.Logger` project is reused for cache logging.

- `HybridCacheService` now uses `ILoggerMessage`.
- `ILoggerMessage` was changed from scoped to singleton so it can safely be injected into the singleton cache service.
- `AddApplicationCaching` uses `TryAddSingleton` so the caching setup can register the logger when needed without replacing an existing registration.

### Current Configuration
Caching is currently running with HybridCache using the local in-memory cache.

Current setup:
- Caching enabled.
- Default cache expiration configured.
- Default local cache expiration configured.
- Distributed caching disabled.
- Redis connection string name reserved for later use.

SQL Server remains the source of truth.

The current flow is:

```
Application Services
        ↓
ICacheService
        ↓
HybridCacheService
        ↓
HybridCache
        ├── L1 Memory Cache
        └── L2 Redis - planned
```

### Tests

Tests were added or updated for:

- Cache hits.
- Cache misses.
- Cache set operations.
- Key removal.
- Tag removal.
- Disabled caching.
- Invalid distributed cache configuration.
- Configuration binding and validation.
- Faculty caching.
- Department caching.
- Paginated caching.
- Repository and mapper bypass on cache hits.
- Cache invalidation after successful writes.
- No invalidation after failed or no-op writes.
- Faculty and Department cross-cache invalidation.
- Cache failure handling.
- Cache failure logging.
- Cancellation handling.
- Dependency injection registration.

---

## Redis L2 Caching

### Goal
Added Redis as the distributed L2 cache for HybridCache and verify that cached data can be shared across application instances.

### What Changed
- Added Redis to Docker Compose using `redis:7.4-alpine`.
- Added a Redis health check with `redis-cli ping`.
- Added Redis connection settings for Docker and local development.
- Enabled distributed caching through `Caching__DistributedEnabled`.
- Kept Redis disabled by default in `appsettings.json`.
- Docker API connects with `online-voting-redis:6379`.
- Local Visual Studio runs connect with `localhost:6379`.

### Verification
Faculty caching was used to verify the Redis setup.

The Redis cache was cleared, a Faculty endpoint was called, and the expected cache key was created in Redis.

After restarting the API, the Redis key was still available. `redis-cli MONITOR` confirmed that HybridCache read the cached Faculty response from 

Redis after the local L1 cache had been cleared.

The same behavior was also verified with the API running locally from Visual Studio.

### Tests
Added Redis integration tests using Testcontainers.

The tests verified that:
- A value cached by one HybridCache instance can be read by a new instance from Redis.
- The cache factory is not called when the value already exists in Redis.
- Tag invalidation marks the Redis value as stale.
- A new cache instance runs the factory again after the related tag is invalidated.

### Current Setup

```
Application Services
        ↓
ICacheService
        ↓
HybridCache
     ├── L1 Memory
     └── L2 Redis
```

---

## Response Compression

### Goal
Add configurable response compression for API responses while keeping compression concerns outside controllers and services.

### Architecture

```
Client
  ↓
Reverse Proxy / Hosting Layer
  ↓
Forwarded Headers
  ↓
Response Compression Middleware
  ↓
Controllers
  ↓
Services
  ↓
Cache / Database
  ↓
Response Compression Middleware
  ↓
Reverse Proxy / Hosting Layer
  ↓
Client
```

Compression is handled at the API infrastructure layer.

### Compression Flow

| Protocol | `EnableForHttps` | Application Compression | Expected Compression Owner |
|---|---:|---|---|
| HTTP | `false` | Enabled | ASP.NET Core |
| HTTP | `true` | Enabled | ASP.NET Core |
| HTTPS | `false` | Disabled | Reverse proxy / hosting layer |
| HTTPS | `true` | Enabled | ASP.NET Core |

Production deployments can therefore choose where HTTPS compression is handled without changing application code.

### Changed
- Added ASP.NET Core response compression.
- Added Brotli and Gzip providers.
- Configured both providers with `CompressionLevel.Fastest`.
- Added `application/problem+json` to supported MIME types.
- Added `UseResponseCompression()` to the middleware pipeline.
- Added configurable HTTPS compression through `ResponseCompression:EnableForHttps`.
- Kept HTTPS application compression disabled by default.
- Added response compression integration tests.

### Configuration

Default configuration:

```json
"ResponseCompression": {
  "EnableForHttps": false
}
```

For environments where the reverse proxy or hosting platform handles HTTPS compression:

```env
ResponseCompression__EnableForHttps=false
```

For environments where ASP.NET Core must handle HTTPS compression:

```env
ResponseCompression__EnableForHttps=true
```

### Middleware Order

```
app.UseForwardedHeaders();
app.UseResponseCompression();
```

Forwarded headers are processed before compression so the original request scheme is available when the application is hosted behind a reverse proxy.

### Compression Providers

Supported providers:

```
Brotli
Gzip
```

The provider is selected from the client's `Accept-Encoding` header. A response is compressed using one provider only.

### Tests

Added integration coverage for:

- Brotli compression.
- Gzip compression.
- Requests without compression support.
- HTTPS compression disabled.
- HTTPS compression enabled.

The Brotli and Gzip tests also verify that the compressed response can be successfully decompressed.

---

## Security Headers and Transport Security

### Goal
Harden the HTTP boundary of the API by adding centralized security headers and configurable transport-security behaviour without introducing 

security logic into controllers or application services.

### Why
Authentication and authorization protect access to API operations, but they do not control how browsers handle HTTP responses or how HTTPS enforcement is managed.

This change adds protection at the HTTP infrastructure layer for:

- MIME-type sniffing.
- Clickjacking and framing.
- Referrer information leakage.
- Unused browser capabilities.
- HTTPS enforcement.
- HSTS.
- Reverse-proxy deployments where TLS is terminated outside the application.

The implementation is centralized so every applicable response receives the same security behaviour.

### Architecture

```
Client
  ↓
Reverse Proxy / Hosting Layer
  ↓
Forwarded Headers
  ↓
Security / Transport Layer
  ├── HSTS
  ├── Security Headers
  └── HTTPS Redirection
  ↓
Response Compression
  ↓
Authentication / Authorization
  ↓
Controllers
  ↓
Services
  ↓
Cache / Database
```

### Changes
- Added centralized `SecurityHeadersMiddleware`.
- Added configurable HSTS support.
- Added configurable HTTPS redirection.
- Added startup validation for the HSTS max-age.
- Added:
  - `X-Content-Type-Options: nosniff`
  - `Referrer-Policy: no-referrer`
  - `X-Frame-Options: DENY`
  - `Content-Security-Policy`
  - `Permissions-Policy`
- Moved HTTPS-redirection ownership out of the previous Development-only condition.
- Kept `UseForwardedHeaders()` before transport-security middleware.
- Added integration tests for security headers, HSTS and HTTPS-redirection behavior.

### Design Decisions

#### Centralized Middleware
Security headers are applied through a dedicated middleware instead of being added in controllers.

This keeps HTTP security consistent across API responses and prevents transport concerns from leaking into business logic.

#### Built-in HSTS Middleware
ASP.NET Core's built-in `UseHsts()` middleware is used instead of implementing HSTS manually.

The custom middleware is responsible only for the additional response headers, while the framework remains responsible for HSTS behaviour.

#### Deployment-Configurable HTTPS Ownership
HTTPS redirection and HSTS are configuration-driven instead of being hardcoded.

A production deployment may terminate TLS at:

```
Nginx
IIS
Load Balancer
Cloud Gateway
```

or directly in:

```
ASP.NET Core / Kestrel
```

The application therefore does not assume which layer owns transport security.

When the hosting layer owns HTTPS enforcement:

```env
SecurityHeaders__HttpsRedirectionEnabled=false
SecurityHeaders__Hsts__Enabled=false
```

When ASP.NET Core owns HTTPS enforcement:

```env
SecurityHeaders__HttpsRedirectionEnabled=true
SecurityHeaders__Hsts__Enabled=true
```

This avoids duplicating redirects or HSTS configuration between the application and reverse proxy.

#### Forwarded Headers First
The middleware order begins with:

```
app.UseForwardedHeaders();
app.UseSecurityHeaders();
```

This is required for reverse-proxy deployments where the external request may use HTTPS while the proxy communicates with ASP.NET Core over HTTP.

Processing forwarded headers first allows the application to work with the original request scheme before making HTTPS-related decisions.

#### Conservative HSTS Defaults
The default HSTS configuration is:

```
"Hsts": {
  "Enabled": false,
  "MaxAgeDays": 30,
  "IncludeSubDomains": false,
  "Preload": false
}
```

A 30-day max age was chosen as a conservative starting point instead of immediately committing production clients to a long-lived HSTS policy.

`IncludeSubDomains` remains disabled because enabling it affects every subdomain.

`Preload` remains disabled because HSTS preload should only be enabled after the complete production domain and HTTPS strategy has been verified.

#### Security Header Selection

`X-Content-Type-Options: nosniff`

Prevents browsers from attempting to reinterpret responses as a different MIME type.

`X-Frame-Options: DENY`

Prevents the application from being embedded in frames and reduces clickjacking exposure.

`Content-Security-Policy`

Uses:

```
frame-ancestors 'none'; object-src 'none'; base-uri 'none'
```

The policy is intentionally limited rather than applying an aggressive global CSP that could break Swagger UI.

`Referrer-Policy: no-referrer`

Prevents browser referrer information from being sent unnecessarily.

`Permissions-Policy`

Disables browser capabilities that are not required by the API:

```
camera
microphone
geolocation
payment
usb
```

### Configuration

Default:

```
"SecurityHeaders": {
  "HttpsRedirectionEnabled": false,
  "Hsts": {
    "Enabled": false,
    "MaxAgeDays": 30,
    "IncludeSubDomains": false,
    "Preload": false
  }
}
```

The defaults do not assume a specific production hosting topology. Deployment-specific settings override them through environment configuration.

### Tests
Added integration coverage for:

- Security headers being added to responses.
- HSTS disabled.
- HSTS enabled.
- HTTPS redirection disabled.
- HTTPS redirection enabled.
- Invalid HSTS max-age configuration failing during startup.

The tests verified actual HTTP behaviour rather than only checking service registration.

---

## CORS Hardening

### Goal
Replace the unrestricted CORS policy with a configurable, fail-closed policy that only allows explicitly approved browser origins, methods and headers.

### Why
The previous policy allowed any browser origin, method and header:

```
.AllowAnyOrigin()
.AllowAnyMethod()
.AllowAnyHeader()
```

That was convenient for development but too permissive for production.

### Architecture

```
Browser / UI
  ↓
Origin header
  ↓
Routing
  ↓
CORS Policy
  ├── Origin allowed?
  ├── Method allowed?
  └── Headers allowed?
  ↓
Authentication / Authorization
  ↓
Controllers
  ↓
Services
```

Only browser origins configured by the deployment environment receive CORS permission.

### Changes
- Replaced `AllowAnyOrigin()` with explicit allowed origins.
- Replaced `AllowAnyMethod()` with configured HTTP methods.
- Replaced `AllowAnyHeader()` with configured request headers.
- Added `CorsSettings` configuration.
- Added startup validation for CORS configuration.
- Added configurable preflight cache duration.
- Added explicit routing before CORS middleware.
- Kept CORS independent of JWT authentication and authorization.
- Added integration tests for allowed, blocked, disabled, and invalid CORS configurations.

### Configuration

Default configuration:

```
"Cors": {
  "Enabled": false,
  "AllowedOrigins": [],
  "AllowedMethods": [
    "GET",
    "POST",
    "PUT",
    "PATCH",
    "DELETE"
  ],
  "AllowedHeaders": [
    "Accept",
    "Authorization",
    "Content-Type",
    "X-Correlation-ID",
    "X-Device-Latitude",
    "X-Device-Longitude",
    "X-Device-Accuracy",
    "X-Device-Location-Captured-At"
  ],
  "PreflightMaxAgeMinutes": 10
}
```

CORS is disabled by default so a deployment must explicitly allow browser origins.

### Design Decisions

#### Explicit Origins
Allowed UI origins are configured through environment settings instead of being hardcoded.

Example for local development:

```env
Cors__Enabled=true
Cors__AllowedOrigins__0=http://localhost:5173
```

Example for production:

```env
Cors__Enabled=true
Cors__AllowedOrigins__0=https://voting.example.com
```

Multiple browser applications can be configured without changing application code:

```env
Cors__AllowedOrigins__0=https://vote.example.com
Cors__AllowedOrigins__1=https://admin.example.com
```

Origins must contain the scheme, host and port when applicable, and must not contain a trailing slash.

#### Fail-Closed Default
`Cors:Enabled` defaults to `false`.

This prevents accidental cross-origin browser access when no production UI origin has been configured.

Swagger remains unaffected when served from the same API origin.

#### Explicit Methods and Headers
The policy only exposes the HTTP methods and request headers currently required by the application.

This avoids silently allowing new browser-access capabilities.

The configured headers include JWT authorization, JSON content, correlation IDs and the existing device-location headers.

#### No CORS Credentials
CORS credentials were not enabled because the current API authentication flow uses JWT bearer tokens through the `Authorization` header.

Credentialed CORS should only be introduced if a future authentication design requires cookies, HTTP authentication or client certificates.

#### Preflight Caching
Successful browser preflight responses can be cached for 10 minutes.

This reduces repeated `OPTIONS` requests while keeping policy changes reasonably short-lived.

#### CORS Is Not Authorization
Blocked origins do not replace authentication or authorization checks.

CORS controls whether browser JavaScript can access a cross-origin response.

JWT and authorization policies remain responsible for protecting API operations from unauthorized users and clients.

### Middleware Order

```
app.UseRouting();

app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();
```

Routing runs before CORS so endpoint information is available to the CORS middleware.

CORS runs before authentication and authorization so browser preflight requests can be handled before protected endpoints are evaluated.

### Validation
Startup validation now rejects invalid configurations, including:

- CORS enabled without any allowed origin.
- Empty allowed-method configuration when CORS is enabled.
- Empty allowed-header configuration when CORS is enabled.
- Invalid HTTP or HTTPS origins.
- Origins containing a trailing slash.
- Invalid preflight cache duration.

This prevents common deployment misconfigurations from silently producing incorrect browser behavior.

### Tests
Added integration coverage for:

- Requests from an allowed origin.
- Requests from a blocked origin.
- Allowed preflight requests.
- Blocked preflight requests.
- CORS disabled.
- CORS enabled without an allowed origin.
- Allowed origin configured with a trailing slash.

The tests verify actual HTTP CORS headers rather than only service registration.

---

## OpenTelemetry Observability

### Goal
Added vendor-neutral observability for API traces and metrics while preserving the existing NLog logging pipeline.

### Changes
- Added configurable OpenTelemetry tracing and metrics.
- Added ASP.NET Core request tracing.
- Added ASP.NET Core and .NET runtime metrics.
- Added configurable parent-based trace sampling.
- Added OTLP HTTP/protobuf export for traces and metrics.
- Added OpenTelemetry resource metadata for service name, namespace, version, and environment.
- Added configuration validation with `ObservabilitySettingsValidator`.
- Added an OpenTelemetry Collector Docker service.
- Added Collector memory limiting and batching.
- Added a local debug exporter for development verification.
- Excluded `/health` and `/swagger` from request tracing.
- Preserved NLog as the application logging pipeline.
- Added `CorrelationId`, `RequestId`, OpenTelemetry `TraceId`, and `SpanId` to the NLog request scope.
- Added validation for incoming `X-Correlation-ID` values.
- Added the correlation ID to traces as `app.correlation_id`.

### Architecture
```text
OnlineVoting.Api
    ├── Traces
    │     └── OTLP HTTP → /v1/traces
    │
    ├── Metrics
    │     └── OTLP HTTP → /v1/metrics
    │
    └── NLog
          ├── CorrelationId
          ├── RequestId
          ├── TraceId
          └── SpanId
                │
                └── TraceId matches OpenTelemetry trace

                    ↓

           OpenTelemetry Collector
                ├── memory_limiter
                ├── batch
                └── debug exporter
```

### Configuration
Observability is configuration-driven and can be enabled or disabled without changing application code.

The Docker-hosted API exports telemetry to the Collector through the Docker network. A locally running API can export through the Collector's published OTLP HTTP port.

The Collector is not treated as a required application dependency. Loss of telemetry does not prevent the API from operating.

The debug exporter is intended for local development verification and should be replaced by an appropriate observability backend for production.

### Disabling Observability
Observability can be disabled completely through configuration.

The default application configuration is:

```json
"Observability": {
  "Enabled": false,
  "ServiceName": "OnlineVoting.Api",
  "ServiceNamespace": "OnlineVoting",
  "TraceSamplingRatio": 1.0,
  "OtlpEndpoint": "http://localhost:4318/",
  "ExcludedTracingPaths": [
    "/health",
    "/swagger"
  ]
}
```

When `Observability:Enabled` is `false`, the application does not register the OpenTelemetry tracing and metrics pipelines and does not export telemetry to the Collector.

For a locally running API, either remove the environment override:

```
Observability__Enabled=true
```

or explicitly set:

```
Observability__Enabled=false
```

For Docker, either remove:

```env
Observability__Enabled=true
Observability__OtlpEndpoint=http://online-voting-otel-collector:4318/
```

from `.env.docker`, allowing the `appsettings.json` default of `false` to apply, or explicitly set:

```
Observability__Enabled=false
```

The OpenTelemetry Collector may remain running when observability is disabled. The API simply stops sending traces and metrics to it.

If the Collector is also not needed locally, it can be stopped independently:

```
docker compose -p online-voting-system -f .\OnlineVoting.Api\docker-compose.yml stop online-voting-otel-collector
```

It can later be started again with:

```
docker compose -p online-voting-system -f .\OnlineVoting.Api\docker-compose.yml up -d online-voting-otel-collector
```

After changing the observability environment configuration, restart the API process or recreate the API container so the new setting is applied.

Disabling observability does not require any code changes.

### Verification
- Observability configuration tests: 11 passed.
- Correlation middleware unit tests: 6 passed.
- Correlation middleware integration tests: 2 passed.
- Collector health endpoint returned HTTP 200.
- Real API traces were received by the Collector.
- ASP.NET Core and .NET runtime metrics were received by the Collector.
- NLog `TraceId` and `SpanId` matched the corresponding OpenTelemetry trace.
- `X-Correlation-ID` was preserved in the response, NLog scope, and OpenTelemetry trace as `app.correlation_id`.
- Full regression suite: 548 passed, 0 failed, 0 skipped.

---

## Redis failure resilience

### Goal

Reduced request latency when distributed caching is enabled but Redis is unavailable.

### Changes

- Added configurable Redis connection and operation timeouts.
- Reduced Redis connection retries.
- Configured Redis backlog behavior to fail fast while disconnected.
- Kept `AbortOnConnectFail` disabled so Redis can reconnect automatically after recovery.
- Added validation for the new Redis configuration values.
- Added unit tests for Redis fail-fast configuration.

### Verification

- Cache configuration tests: 9 passed.
- Full regression suite: 552 passed.
- Redis healthy:
  - Faculty endpoint: approximately 39–48 ms.
- Redis unavailable:
  - Previous behavior: approximately 15.2–15.3 seconds.
  - Updated behavior: first request approximately 1.84 seconds.
  - Subsequent local-cache requests: approximately 30–57 ms.
- Redis restored while API remained running:
  - Faculty endpoint returned to approximately 38–48 ms.
  - No API restart was required.

The distributed cache now degrades much faster when Redis is unavailable while retaining automatic recovery when Redis becomes available again.

---