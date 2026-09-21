# Logging event ID registry

Logging event IDs are global across the CommandQ repositories so aggregated
logs never assign the same number to unrelated behavior. Reusable package IDs
retain the same meaning in every consuming application.

| Repository | Owner | Reserved ranges |
| --- | --- | --- |
| command-q-back | API | 100000-102999 |
| command-q-back | Application | 200000-206999 |
| command-q | CommandQ.Common | 300000-300999 |
| command-q | CommandQ.Persistence | 400000-400999 |
| command-q-back | IdentityServer | 500000-503999 |
| command-emails | Email | 600000-600999 |
| command-q-back | Application.Migrations | 700000-700999 |
| command-emails | Email.Outbox | 800000-803999 |
| command-emails | Email.Outbox.Persistence | 900000-902999 |
| command-emails | Email.Worker | 1000000-1001999 |
| command-emails | Email.Api | 1100000-1103999 |
| command-jobs | Complete solution | 2000000-2999999 |

The gaps are intentionally unallocated. Application repositories document the
project and module meanings inside their reserved ranges.

## Package rules

- `CommandQ.Common` owns `300000-300999`.
- `CommandQ.Persistence` owns `400000-400999`.
- Package IDs stay stable across releases and consumers because the package
  implementation owns the event meaning.
- Use source-generated logging through `LoggerMessage`.
- Do not reuse retired IDs or log credentials, tokens, connection strings,
  command parameters, or other sensitive values.
- Tests enforce uniqueness and range membership inside each logging package.
