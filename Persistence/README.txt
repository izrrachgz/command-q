Project: Persistence
Purpose: Data access implementations (EF Core, repositories, migrations).
Framework: .NET 10 (class library).
How to run: Apply migrations with `dotnet ef database update` (if using EF Core) from the project or solution as configured.
Configuration: Check `appsettings.json` for the connection string; use environment variables in CI/CD.
Notes: Expose interfaces consumed by `Application`; do not leak internal implementation details outside the project.