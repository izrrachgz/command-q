# Logging Event IDs

Event IDs are grouped by project and then by ownership area.

Use the project general range for cross-cutting infrastructure logs. Use a module
range when the log message belongs to a feature module.

## Ranges

| Project | General Range | Module Ranges |
| --- | ---: | --- |
| Api | 100000-100999 | Identity: 101000-101999, Health: 102000-102999 |
| Application | 200000-200999 | Health: 201000-201999, Identity: 202000-202999, Files: 203000-203999, Contacts: 204000-204999, Addresses: 205000-205999, Notifications: 206000-206999 |
| Common | 300000-300999 | Add module/shared-area ranges only when needed |
| Persistence | 400000-400999 | Add module/shared-area ranges only when needed |
| IdentityServer | 500000-500999 | OpenIddict: 501000-501999, Health: 502000-502999, Accounts: 503000-503999 |
| Application.Migrations | 700000-700999 | Add module ranges only when migration ownership needs them |

## Rules

- Add new range constants to the project-level `*EventIdRanges` file.
- Add module-owned IDs to the module's `Logging/*EventIds.cs` file.
- Add cross-cutting IDs to the project root `Logging/*EventIds.cs` file.
- Keep each range to 1000 IDs.
- Within a module range, group related log events by hundreds when useful.
- API event `100060` records reloads of operational configuration. Its details are
  limited to non-sensitive summaries and must never include credentials, tokens or
  connection strings.
