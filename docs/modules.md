# Backend Modules

The standalone `IdentityServer` project is not an API module. It owns users,
credentials, lockout, external logins, OAuth clients, scopes, grants and tokens.
The API validates its tokens through OpenID Connect discovery without taking a
project reference to it.

CommandQ currently keeps all existing backend modules in the starter. They are
organized so a future project can keep, replace or remove them deliberately.

## Current Modules

| Module | Status | Purpose |
| --- | --- | --- |
| Identity | Core | Application profiles, account preferences, external identity mapping and queryable business account activity. Authentication belongs to `IdentityServer`. |
| Configuration | Core | Immutable application configuration snapshots, including startup version, environment, language and server timezone. |
| Health | Core | Health checks and operational diagnostics. |
| Notifications | Optional infrastructure | Application commands and services for validated durable email enqueueing. SMTP delivery and persistence remain outside Application. |
| Files | Optional | Authenticated file upload and account file metadata. |
| Contacts | Sample | Example account-owned feature module. |
| Addresses | Sample | Example account-owned feature module. |
| Events | Sample/domain-only | Event and place entities used by register-style scenarios. |
| Registers | Sample/domain-only | Tracking/register entities used by event/place scenarios. |

## Removing A Module

Use this checklist when removing a module.

1. Remove API endpoints:
   - delete the module folder under `Api/Modules/{Module}`
   - remove the module mapping call from `Api/Modules/ApiModuleExtensions.cs`

2. Remove application code:
   - delete the module folder under `Application/Modules/{Module}`
   - remove the module registration call from `Application/Modules/ApplicationModuleExtensions.cs`

3. Remove domain code:
   - delete the module folder under `Domain/Modules/{Module}`
   - remove related global usings from `Application/GlobalUsings.cs` if they are no longer needed

4. Remove tests:
   - delete matching tests under `Application.Test/Modules/{Module}`
   - delete matching tests under `Api.Test/Modules/{Module}`
   - update architecture tests if the module was listed as an allowed namespace or logging range

5. Remove migrations:
   - remove module scripts under `Application.Migrations/Migrations/{Engine}/{Module}`
   - keep migration IDs unique per engine
   - do not renumber existing applied migrations

6. Remove localization:
   - remove unused resource keys from `Localization/Resources/*.resx`
   - remove unused JSON locale keys from `Localization/Locales/*.json`

7. Remove logging:
   - delete module logging files under `{Project}/Modules/{Module}/Logging`
   - remove unused range constants from `{Project}/Logging/*EventIdRanges.cs`
   - update `docs/logging-event-ids.md`

8. Verify:
   - `dotnet build .\CommandQ.sln`
   - `dotnet test .\Application.Migrations.Test\Application.Migrations.Test.csproj`
   - `dotnet test .\Api.Test\Api.Test.csproj`
   - `dotnet test .\Application.Test\Application.Test.csproj --filter FullyQualifiedName~Application.Test.Architecture.ArchitectureRulesTest`

## Notes

- Prefer removing entire modules instead of leaving half-empty folders.
- Keep `Identity` and `Health` unless the replacement module is already in place.
- Use Identity account activity as the queryable audit trail for business account actions instead of per-account application log files. IdentityServer keeps authentication security events in its own database.
- Account preferences are soft-deletable and have one active row per account. `PUT /accounts/me/preferences` records the complete saved preference payload in account activity metadata; `GET /accounts/me/preferences` returns the active row.
- Configuration history is read-only and admin-only at `GET /admin/configurations`. Startup records authoritative server values and the server instance name after application migrations complete; clients cannot append snapshots.
- API modules own transport-only request DTOs under their `Requests` folders. Controllers map those requests to application commands after applying authenticated-account and transport policies.
- `POST /files` accepts common documents, images, audio, video and ZIP files up to 200 MB. Reusable category policies remain in the API Files module for specialized future endpoints.
- The Notifications module references only the reusable email and outbox contracts. Producer hosts select and register `IEmailOutboxStore`; `Email.Worker` owns delivery and retries.
- Removing IdentityServer requires replacing the API `Authentication:Authority` with another standards-compatible issuer; do not move credentials back into the API.
- Treat `Contacts`, `Addresses`, `Events` and `Registers` as examples until a product needs them.
