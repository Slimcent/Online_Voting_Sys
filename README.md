# Online Voting System

A backend platform for managing institutional elections from election setup and candidate applications through payments, voter registration, voting and result publication.

Built with ASP.NET Core, Entity Framework Core, SQL Server, JWT authentication, background processing, caching, and OpenTelemetry-based observability.

<p>
  <a href="https://online-voting-api.runasp.net/swagger/index.html">Live API</a>
  ·
  <a href="https://github.com/Slimcent/Online_Voting_Sys">Repository</a>
</p>

---

## Overview

The Online Voting System is designed around the full election lifecycle rather than only the act of casting a vote.

The backend handles:

- election configuration;
- election types, scopes, statuses and positions;
- candidate applications;
- application fees and payment processing;
- administrative application review;
- contestant creation;
- voter registration;
- secure vote casting;
- personal voting history;
- election result calculation;
- authentication and authorization;
- refresh-token rotation and session management;
- caching;
- background jobs;
- audit trails;
- logging, metrics and distributed tracing.

The application follows a service-oriented layered architecture. Controllers are kept thin, business rules live in services, persistence is handled through repositories and Unit of Work, and cross-cutting concerns such as caching, logging, background processing, payments, and telemetry are isolated behind abstractions.

---

## Technology Stack

| Technology | Usage |
|---|---|
| ASP.NET Core | REST API |
| .NET 10 | Application runtime |
| C# | Main language |
| Entity Framework Core | Data access and ORM |
| SQL Server | Primary database |
| JWT | Access-token authentication |
| Refresh Tokens | Session renewal and revocation |
| AutoMapper | Object mapping |
| FluentValidation | Request validation |
| Swagger / OpenAPI | API documentation |
| Docker | Containerized development and infrastructure |
| xUnit | Automated testing |
| Moq | Mocking |
| SQLite In-Memory | Isolated service tests |
| OpenTelemetry | Application telemetry |
| OpenTelemetry Collector | Telemetry routing |
| Prometheus | Metrics |
| Grafana | Dashboards and visualization |
| Grafana Tempo | Distributed tracing |
| NLog | Application logging |

---

# Architecture

The application is split into dedicated projects and responsibilities.

```
Client
  |
  v
ASP.NET Core API
  |
  v
Controllers
  |
  v
Service Interfaces
  |
  v
Service Implementations
  |
  +--------------------------+
  |                          |
  v                          v
Repositories / UnitOfWork   Infrastructure
  |                          |
  v                          +-- Caching
Entity Framework Core        +-- Email
  |                          +-- Background Tasks
  v                          +-- Payment Gateways
SQL Server                   +-- Current User Context
                             +-- Logging
                             +-- Telemetry
```

### Request Flow

A normal request generally follows:

```
HTTP Request
    |
    v
Controller
    |
    v
Service
    |
    v
Repository / UnitOfWork
    |
    v
Entity Framework Core
    |
    v
SQL Server
```

Controllers deal with HTTP concerns only.

Business rules remain in the service layer, while persistence and infrastructure concerns are kept behind abstractions.

---

# Domain Model

The domain is built around elections, applications, payments, voters and votes.

A simplified view of the main model is:

```
Faculty
   |
   +---- Department
             |
             +---- Student
                     |
                     +--------------------------+
                     |                          |
                     v                          v
             PositionApplication         RegisteredVoter
                     |                          |
                     v                          v
                 Contestant                    Vote
                     ^                          |
                     |                          |
                     +--------------------------+


ElectionType
     |
     v
Election
     |
     +---- ElectionScope
     |
     +---- ElectionStatus
     |
     +---- ElectionPosition
              |
              +---- Position
              |
              +---- PositionApplication
                        |
                        +---- Invoice
                        |       |
                        |       v
                        |   PaymentTransaction
                        |
                        +---- Contestant
```

## Main Entities

### Election

Represents an actual election event.

An election contains configuration such as:

- election type;
- election scope;
- election status;
- year;
- faculty or department context;
- application period;
- voter-registration period;
- voting period;
- activation state.

### Position

Represents a permanent office such as:

- President;
- Treasurer;
- Secretary.

A position exists independently of any particular election.

### ElectionPosition

Connects a permanent `Position` to a specific `Election`.

For example:

```
Position
Engineering Faculty President
        |
        v
ElectionPosition
2026 Engineering Faculty Election
```

This allows election-specific information such as application fees and activation state to change without modifying the permanent position.

### PositionApplication

Represents a student's application to contest for an election position.

An application can move through statuses such as:

```
Pending Payment
Pending Review
Approved
Rejected
Withdrawn
```

### Contestant

Represents an approved position application.

A student is not considered a contestant simply because an application was submitted or paid for.

The application must first pass the administrative review process.

### RegisteredVoter

Represents a student who has successfully registered to vote in a particular election.

The voter receives a generated voting code that forms part of the voting credentials.

### Vote

Represents one vote cast by a registered voter for a contestant in a particular election position.

The database enforces a unique constraint on:

```
RegisteredVoterId + ElectionPositionId
```

This prevents a voter from casting more than one vote for the same election position.

### Invoice

Represents the amount a student must pay for a position application.

### PaymentTransaction

Represents an individual attempt to pay an invoice.

An invoice can have multiple payment attempts, which allows failed attempts to remain recorded instead of being overwritten.

---

# Election Management

The system supports configurable elections rather than hard-coded election types.

Administrators can manage:

- election types;
- election scopes;
- election statuses;
- election positions;
- application periods;
- voter-registration periods;
- voting periods;
- activation state.

Election scopes are stored as reference data and can represent contexts such as:

```
University
Faculty
Department
```

This makes the election model flexible enough to support different organizational levels.

---

# Candidate Application

Candidates enter the election through an application workflow.

```
Student
   |
   v
Position Application
   |
   v
Pending Payment
   |
   v
Payment Completed
   |
   v
Pending Review
   |
   +------------------+
   |                  |
   v                  v
Approved           Rejected
   |
   v
Contestant
```

A contestant is created only after the application is approved.

This keeps application state separate from contestant state and prevents applicants from appearing on the ballot before administrative approval.

---

# Payments

Election positions can require an application fee.

The payment model separates the amount owed from individual payment attempts:

```
Position Application
        |
        v
      Invoice
        |
        v
Payment Transaction
        |
        v
 Payment Gateway
```

## Payment Initiation

Payment initiation performs the following operations:

1. validate the invoice;
2. validate the idempotency key;
3. create a pending payment transaction;
4. resolve the configured payment gateway;
5. initialize the provider checkout;
6. update the local transaction;
7. return the checkout information.

## Idempotency

Payment initiation uses idempotency keys to prevent repeated client requests from creating duplicate payment transactions.

An idempotency record can move through:

```
Processing
Failed
Completed
```

A completed request can safely return its previous result when the same key and request data are sent again.

## Payment Verification

Payment verification contacts the provider and reconciles the provider result with local application state.

```
Payment Provider
      |
      v
Verify Transaction
      |
      v
Validate Amount / Currency / Reference
      |
      v
Payment Transaction -> Succeeded
      |
      v
Invoice -> Paid
      |
      v
Position Application -> Pending Review
```

Failed and cancelled payments update the payment transaction without moving the application into a paid state.

## Webhooks

Webhook processing uses the same reconciliation path as explicit payment verification.

This avoids maintaining separate business rules for manual verification and provider callbacks.

---

# Voter Registration

Students register separately for each election.

The registration process validates:

- the student;
- the election;
- election activation;
- the configured voter-registration period;
- duplicate registration.

After registration, a `RegisteredVoter` record is created and a voting code is generated.

Registered-voter queries support filtering, pagination, search, and caching.

---

# Voting

A vote request contains:

- registered voter identifier;
- voting code;
- election position identifier;
- contestant identifier.

Before a vote is stored, the service validates:

- the authenticated user;
- voter credentials;
- ownership of the voter credentials;
- registered voter activation;
- election activation;
- election position activation;
- election eligibility;
- voting start and end times;
- contestant existence;
- contestant activation;
- contestant membership of the selected election position;
- duplicate voting.

The database uniqueness constraint provides an additional layer of protection against duplicate votes.

---

## Vote Confirmation

After the vote has been successfully stored, a confirmation email is queued through the background task system.

The email confirms the election, position and voting time.

The selected contestant is deliberately excluded from the email so the voter's ballot choice is not exposed through an external communication channel.

---

# Voting History

Authenticated users can retrieve their own voting history.

The response includes:

- election;
- election position;
- contestant;
- voting time.

The endpoint supports pagination, filtering and search.

Voting history is scoped using the authenticated user's identity rather than accepting another user's voting credentials.

---

# Election Results

Results become available after the voting period has ended.

Result calculation starts from contestants instead of votes so contestants with zero votes are still returned.

Example:

```
President

Candidate A       3 votes       60%
Candidate B       2 votes       40%
Candidate C       0 votes        0%

Total votes       5
```

For each contestant, the result contains:

- election;
- election position;
- contestant;
- vote count;
- total votes for the position;
- percentage.

The result endpoint supports pagination, election filtering, position filtering, and search.

---

# Authentication and Session Management

The authentication layer uses JWT access tokens together with refresh tokens.

It supports:

- login;
- account verification;
- password reset;
- password change;
- recovery email;
- email change confirmation;
- refresh-token rotation;
- logout;
- logout from all sessions;
- token-family tracking;
- refresh-token reuse detection.

Refresh tokens are stored securely and can be revoked independently of access tokens.

The application also uses role and claims-based authorization for protected operations.

---

# Caching

Read-heavy operations use the application's caching layer.

Cached areas include business data such as:

- registered voters;
- voting history;
- election results;
- position applications;
- invoices;
- payment transactions.

The general flow is:

```
Request
   |
   v
Cache
   |
   +---- Hit ------> Response
   |
   +---- Miss
           |
           v
        Database
           |
           v
        Response
           |
           v
          Cache
```

Invalidation happens after persistence succeeds.

For example, after a vote is stored:

```
Vote Persisted
      |
      +---- Invalidate VoteHistory
      |
      +---- Invalidate ElectionResult
```

---

# Background Processing

Secondary operations that do not need to block an HTTP request are handled through the background-task infrastructure.

Examples include:

- user creation emails;
- voter emails;
- vote confirmation emails;
- other asynchronous notification work.

The background-processing implementation lives in the dedicated:

```
OnlineVoting.BackgroundTasks
```

project.

---

# Audit Trail

The application records important changes to business data through a centralized audit trail.

An audit entry can record:

- the user who performed an operation;
- the affected entity;
- operation type;
- previous values;
- new values;
- request information;
- outcome;
- timestamp;
- endpoint;
- location metadata where available.

The audit trail provides traceability for administrative and business operations without requiring individual services to manually build audit records.

---

# Observability

The application includes an OpenTelemetry-based observability stack for metrics, distributed tracing, dashboards and application logging.

```
Online Voting API
       |
       v
OpenTelemetry
       |
       v
OpenTelemetry Collector
       |
       +----------------+
       |                |
       v                v
   Prometheus          Tempo
     Metrics           Traces
       |                |
       +--------+-------+
                |
                v
             Grafana
```

## OpenTelemetry

OpenTelemetry provides instrumentation for the ASP.NET Core application.

Telemetry collection remains separate from the application's election and voting business logic.

## OpenTelemetry Collector

The collector receives application telemetry and forwards it to the configured backends.

Configuration:

```
Observability/otel-collector-config.yaml
```

## Prometheus

Prometheus collects and stores metrics.

Configuration:

```
Observability/prometheus.yml
```

## Tempo

Grafana Tempo stores distributed traces produced through OpenTelemetry.

Configuration:

```
Observability/tempo.yaml
```

## Grafana

Grafana is used to inspect metrics and traces.

The repository includes a custom API dashboard:

```
Observability/Grafana/dashboards/
└── online-voting-api-overview.json
```

Dashboard and datasource provisioning are also stored in source control:

```
Observability/Grafana/provisioning/
├── dashboards/
│   └── dashboards.yaml
└── datasources/
    └── datasources.yaml
```

Keeping the dashboard and provisioning files in the repository makes the monitoring environment reproducible.

## Logging

Application logging uses NLog.

The service layer logs normal operations, warnings and failures through the application's logging abstraction.

```
_loggerMessage.LogInfo(...);
_loggerMessage.LogWarn(...);
_loggerMessage.LogError(...);
```

Logs, metrics and traces cover different operational concerns:

```
Logs       -> application events and failures
Metrics    -> system behaviour over time
Traces     -> execution path of individual requests
```

---

# API Documentation

Swagger documentation is centralized rather than being defined directly inside each controller.

The documentation infrastructure includes:

```
ApiDocumentationAttribute
ApiDocumentationRegistry
ApiDocumentationOperationFilter
ApiOperationDocumentation
ApiResponseDocumentation
CommonApiResponses
```

API areas maintain their own documentation definitions, for example:

```
ElectionDocumentation
PaymentDocumentation
VoterDocumentation
FacultyDocumentation
DepartmentDocumentation
```

The live Swagger UI is available at:

https://online-voting-api.runasp.net/swagger/index.html

---

# Result Pattern

Services return `Result<T>` for expected business outcomes.

Supported result states include:

```
Success
Created
No Content
Validation Error
Unauthorized
Forbidden
Not Found
Conflict
```

Controllers convert the result into the appropriate HTTP response.

This prevents business services from becoming coupled to ASP.NET controller response types.

---

# Repository Structure

```
Online_Voting_Sys
│
├── .github/
│   └── workflows/
│
├── Inventory/
│
├── Nlog/
│
├── Observability/
│   ├── Grafana/
│   │   ├── dashboards/
│   │   │   └── online-voting-api-overview.json
│   │   └── provisioning/
│   │       ├── dashboards/
│   │       │   └── dashboards.yaml
│   │       └── datasources/
│   │           └── datasources.yaml
│   ├── otel-collector-config.yaml
│   ├── prometheus.yml
│   └── tempo.yaml
│
├── OnlineVoting.Api/
│   ├── Controllers/
│   ├── Documentation/
│   ├── Extensions/
│   └── Middlewares/
│
├── OnlineVoting.BackgroundTasks/
│
├── OnlineVoting.Caching/
│
├── OnlineVoting.Models/
│   ├── Context/
│   ├── Dtos/
│   ├── Entities/
│   ├── Pagination/
│   ├── Results/
│   └── Validators/
│
├── OnlineVoting.Services/
│   ├── Exceptions/
│   ├── Extensions/
│   ├── Helpers/
│   ├── Implementation/
│   ├── Infrastructures/
│   ├── Interfaces/
│   └── Utilities/
│
├── OnlineVoting.Tests/
│   ├── IntegrationTests/
│   ├── UnitTests/
│   └── TestData/
│
├── VotingSystem.Data/
├── VotingSystem.Logger/
│
├── Online_Voting_Sys.sln
├── docker-compose.dcproj
├── .dockerignore
└── .gitignore
```

---

# Running the Project

## Requirements

You will need:

- .NET SDK;
- SQL Server;
- Docker Desktop;
- Git.

Clone the repository:

```
git clone https://github.com/Slimcent/Online_Voting_Sys.git
cd Online_Voting_Sys
```

Restore dependencies:

```bash
dotnet restore
```

Configure the required application settings for:

- SQL Server;
- JWT;
- refresh tokens;
- email;
- payment provider;
- caching;
- logging;
- OpenTelemetry.

Secrets should be provided through environment variables, user secrets, or another secure configuration provider.

---

## Database

Apply the EF Core migrations:

```
dotnet ef database update \
  --project OnlineVoting.Api \
  --startup-project OnlineVoting.Api
```

---

## Run the API

```
dotnet run --project OnlineVoting.Api
```

Swagger will be available from the configured application URL.

The deployed instance is available at:

https://online-voting-api.runasp.net/swagger/index.html

---

## Docker

The repository includes Docker configuration for the application and its supporting infrastructure.

```
docker compose up --build
```

The containerized environment includes the services needed by the application and observability stack, including SQL Server, OpenTelemetry Collector, Prometheus, Grafana and Tempo where configured.

---

# Development Notes

The main conventions followed in the backend are:

- thin controllers;
- service interfaces and implementations;
- repository and Unit of Work for persistence;
- `Result<T>` for normal business outcomes;
- AutoMapper for model mapping;
- FluentValidation for request validation;
- `IQueryable` for database-side filtering and pagination;
- `AsNoTracking()` for read-only queries;
- cache invalidation after successful persistence;
- background processing for non-blocking secondary work;
- centralized Swagger documentation;
- automated unit and integration testing;
- telemetry and logging as infrastructure concerns.

---

# Current Scope

The backend currently covers the complete core workflow:

```
Election Setup
     |
     v
Election Positions
     |
     v
Candidate Application
     |
     v
Application Payment
     |
     v
Administrative Review
     |
     v
Contestant
     |
     v
Voter Registration
     |
     v
Voting
     |
     v
Voting History
     |
     v
Election Results
```

The project also includes the supporting infrastructure required around that workflow:

```
Authentication
Authorization
Refresh Tokens
Caching
Background Processing
Audit Trail
Payments
API Documentation
Logging
Metrics
Distributed Tracing
Grafana Dashboards
```

# Future Improvements

Possible future extensions include:

- dedicated React frontend;
- richer election-result dashboards;
- real-time election monitoring;
- additional payment gateways;
- notification preferences;
- administrative analytics;
- advanced reporting;
- downloadable election reports;
- richer audit visualization;
- alerting rules for operational metrics;
- deployment-specific Grafana dashboards;
- additional OpenTelemetry instrumentation;
- performance and load testing.

---

---

# Author

**Obinna Achara**

GitHub: [@Slimcent](https://github.com/Slimcent)

---

<p align="center">
  <a href="https://online-voting-api.runasp.net/swagger/index.html">Explore the API</a>
</p>
