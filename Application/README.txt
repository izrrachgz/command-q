Project: Application
Purpose: Implements application logic, use cases, services and orchestration between domain and persistence.
Framework: .NET 10 (class library).
How to run/test: Build with `dotnet build`. Integration/unit tests target this project from `Application.Test`.
Dependencies: `Domain`, `Common`, `Localization`, `Persistence`, `Email` and `Email.Outbox`; persistence is consumed through `Persistence.Abstractions`, and notification workflows depend only on reusable email/outbox contracts.
Patterns: CQRS / Mediator, validators, mappers and application services.
Registration: Use `AddApplicationModules(configuration)`; each module owns explicit handler, validator, mapper and service registrations in its `*ModuleExtensions.cs` file.
Notes: Keep logic free of direct I/O; inject repositories/services via DI. Keep commands, queries and boundary contracts public while module implementations remain internal by default.
