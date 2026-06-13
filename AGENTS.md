# CommandQ — AGENTS (project summary)

This file centralizes brief information for each project (taken from the `readme.txt` files located at each project's root).

## Api
- Purpose: Expose the HTTP API (controllers, endpoints, DTOs).
- Framework: .NET 10 (ASP.NET Core).
- Run locally: `dotnet run` from `Api` or use the Visual Studio launch profile (__Launch Profiles__).
- Configuration: Check `appsettings.json` and environment variables for connection strings and keys.
- Notes: Use Swagger/Postman to validate endpoints.

## Application
- Purpose: Application logic, use cases and orchestration between domain and persistence.
- Framework: .NET 10 (class library).
- Build/Run: `dotnet build`.
- Dependencies: `Domain`, `Common`. Keep logic decoupled from I/O; use DI.

## Domain
- Purpose: Domain models, entities, value objects and business rules.
- Framework: .NET 10.
- Conventions: Favor immutability and validations within entities; avoid infrastructure dependencies.

## Common
- Purpose: Shared utilities, constants, exceptions and helper types.
- Framework: .NET 10.
- Notes: Keep stable and minimize changes to avoid breaking consumers.

## Persistence
- Purpose: Data access implementation (repositories, EF Core, migrations).
- Framework: .NET 10.
- Migrations: `dotnet ef database update` (run from the appropriate project or solution as configured).
- Configuration: Use `appsettings.json` or environment variables for the connection string.

## Application.Test
- Purpose: Unit and integration tests for `Application` and related layers.
- Framework: .NET 10 (see project for test runner: xUnit/NUnit/MSTest).
- Run tests: `dotnet test` from the solution or project folder.
- Check results in Visual Studio: __Output__ pane and __Test Explorer__.

## Localization
- Purpose: Resources and translations (`.resx` files or other).
- Framework: .NET 10.
- Use: Edit/add resources in `.resx` and rebuild; inject localization services in `Api`.

---

Quick solution-level instructions:
- Restore packages: `dotnet restore` at the solution root.
- Build: `dotnet build`.
- Run API: `cd Api; dotnet run` (or use Visual Studio).
- Run tests: `dotnet test`.
- Review logs and tests in Visual Studio: __Output__ and __Test Explorer__.

Contact: See the remote repository for author information and `CONTRIBUTING` if present.