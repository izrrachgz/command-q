# Backend Modules

CommandQ currently keeps all existing backend modules in the starter. They are
organized so a future project can keep, replace or remove them deliberately.

## Current Modules

| Module | Status | Purpose |
| --- | --- | --- |
| Identity | Core | Accounts, account sessions, JWT auth, refresh-token flow and auth-related API endpoints. |
| Health | Core | Health checks and operational diagnostics. |
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
   - remove module scripts under `Persistence/Migrations/{Engine}/{Module}`
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
   - `dotnet test .\Api.Test\Api.Test.csproj`
   - `dotnet test .\Application.Test\Application.Test.csproj --filter FullyQualifiedName~Application.Test.Architecture.ArchitectureRulesTest`

## Notes

- Prefer removing entire modules instead of leaving half-empty folders.
- Keep `Identity` and `Health` unless the replacement module is already in place.
- Treat `Contacts`, `Addresses`, `Events` and `Registers` as examples until a product needs them.
