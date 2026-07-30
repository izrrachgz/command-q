Project: Api
Purpose: Expose the HTTP API (controllers, endpoints, DTOs) for CommandQ.
Framework: .NET 10 (ASP.NET Core).
How to run: From the project root: `dotnet run` or use the Visual Studio launch profile.
Configuration: Check `appsettings.json` and environment variables for connection strings and keys.
Key dependencies: References to `Application` and `Common`. Restore packages: `dotnet restore`.
Authentication:
- JWT settings live under `Jwt`.
- Authentication behavior lives under `Auth`.
- Refresh token flows use `AuthService`, `JwtTokenService`, and `RefreshTokenService`.
- Controllers can resolve the current account through `ClaimsPrincipalExtensions`, for example `User.TryGetAccountId(out var accountId)`.

Account/session refresh cache:
- Register auth cache services through `services.AddAccountSessionCache()`.
- `ITypedCache<TEntry>`/`TypedMemoryCache<TEntry>` live in `Common` and provide reusable type-scoped in-memory caching.
- `AccountSessionCache` is the account/session-specific adapter over `ITypedCache<AccountSessionCacheEntry>`.
- Account/session caching reduces database reads during refresh-token flows.
- `Auth:RefreshCacheMinutes` controls sliding cache lifetime.
- `Auth:RefreshCacheCleanupMinutes` controls how often `AccountSessionCacheCleanupService` clears account/session cache entries.
- `Clear()` on a typed cache removes only keys tracked for that cached value type.

Development notes: Use Visual Studio launch profiles for local testing and Postman/Swagger to validate endpoints.
Contact / Author: See CONTRIBUTING or the remote repository.
