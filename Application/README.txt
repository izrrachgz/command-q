Project: Application
Purpose: Implements application logic, use cases, services and orchestration between domain and persistence.
Framework: .NET 10 (class library).
How to run/test: Build with `dotnet build`. Integration/unit tests target this project from `Application.Test`.
Dependencies: `Domain`, `Common`, interfaces for `Persistence`.
Patterns: CQRS / Mediator or application services (see folder structure).
Notes: Keep logic free of direct I/O; inject repositories/services via DI.