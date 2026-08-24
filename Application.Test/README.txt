Project: Application.Test
Purpose: Unit and integration tests for `Application` and related layers.
Framework: .NET 10 with xUnit v3 and Xunit.DependencyInjection.
How to run: `dotnet test .\Application.Test\Application.Test.csproj` or target a namespace with `--filter FullyQualifiedName~...`.
Configuration: Database-backed module tests read `ConnectionStrings` from this test project's own user-secrets and require the current application schema.
Tips: Architecture and isolated helper tests do not require the application database. Use the real `ConfigureApplicationServices(configuration)` composition path when testing registrations.
Output/Logs: Check the __Output__ pane and Test Explorer in Visual Studio after running.
