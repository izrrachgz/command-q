Project: Api.Test
Purpose: API-layer tests for controllers, middleware and API architecture rules.
Framework: .NET 10 test project.

Layout:
- Architecture tests live under Architecture.
- API module tests mirror Api/Modules under Modules.
- Cross-cutting API infrastructure tests live under Modules/Common.

Notes:
- Avoid booting the full API host for isolated API tests unless the test intentionally validates startup/pipeline behavior.
- WebApplicationFactory tests use CommandQApiFactory, which sets the Testing environment and in-memory test configuration.
- Testing environment startup skips database migrations.
- Full host tests outside CommandQApiFactory may run migrations and require configured local database settings.
- Keep frontend concerns out of this project.
