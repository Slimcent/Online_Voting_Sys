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