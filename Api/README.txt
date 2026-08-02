Project: Api
Purpose: Expose the CommandQ application HTTP API as an OAuth resource server.
Framework: .NET 10 (ASP.NET Core).
How to run: From the project root, use `dotnet run` or the Visual Studio launch profile.
Configuration: Check `appsettings.json`, user-secrets and environment variables.

Authentication:
- `IdentityServer` owns users, credentials, lockout, clients, grants and tokens.
- API validation settings live under `Authentication`.
- The API validates issuer discovery keys, audience, lifetime, `at+jwt` token type and the `command_q_api` scope.
- The API never issues JWTs or stores refresh tokens.
- `ClaimsPrincipalExtensions` reads protocol claims; `ICurrentAccountAccessor` resolves `(iss, sub)` to the application profile through `AccountIdentity`.
- Call authenticated `POST /Account/create` once after initial identity registration/login to provision the application profile.

Development notes: Run IdentityServer before testing authenticated API workflows. The frontend remains optional and is not required by either backend host.
