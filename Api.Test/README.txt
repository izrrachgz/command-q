Project: Api.Test
Purpose: API-layer tests for controllers, middleware and API architecture rules.
Framework: .NET 10 test project.

Layout:
- Architecture tests live under Architecture.
- API module tests mirror Api/Modules under Modules.
- Cross-cutting API infrastructure tests live under Modules/Common.

Notes:
- Avoid booting the full API host for isolated API tests unless the test intentionally validates startup/pipeline behavior.
- WebApplicationFactory tests use ApiFactory, which sets the Testing environment, in-memory test configuration and a unique temporary SQLite database.
- ApiFactory applies the SQLite baseline so full-host startup can record configuration and exercise the unsupported-engine RLS path; it deletes the temporary database when disposed.
- Full host tests outside ApiFactory may run migrations and require configured local database settings.
- Keep frontend concerns out of this project.
