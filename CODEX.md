# Codex setup for command-q

The [solution guidance](AGENTS.md) and this setup follow the sibling `agents-structure` scaffold. This solution owns seven reusable CommandQ NuGet packages for .NET 10 and .NET 11. Consuming applications own hosts, business workflows, schemas and deployments. See the [library inventory](README.md#libraries) and [secure package release procedure](docs/package-release.md).

## Open the project

1. Open this checkout as a trusted Codex project. Start a fresh session after changing repository instructions or project configuration.
2. Review the [shell hook registration](.codex/hooks.json) and [handler](.codex/hooks/validate_shell.py) in the hooks UI before trusting them. The launcher needs Git and Python 3.
3. Invoke a skill by name or let Codex select it when its description matches the task.

| Location | Purpose |
| --- | --- |
| [AGENTS.md](AGENTS.md) | Package ownership, consumer boundaries and workflow routing |
| [.codex/config.toml](.codex/config.toml) | Shared project settings. Personal and managed policy supplies the model, sandbox and approvals. |
| [.codex/agents](.codex/agents) | Read-only review and defensive security roles |
| [.codex/rules/default.rules](.codex/rules/default.rules) | Shell execution policy for selected commands |
| [.agents/skills](.agents/skills) | Library development, verification and package-release workflows |
| [docs/standards](docs/standards) | Implementation, testing, compatibility, security and release conventions |
| [.codex/LICENSE](.codex/LICENSE) | License notice for adapted scaffold material |

Use `AGENTS.override.md` for a deliberate local replacement of repository instructions.

## Work locally

~~~powershell
dotnet restore .\command-q.sln
.\scripts\test.ps1 -Project Cqrs.Test\Cqrs.Test.csproj -Filter "*RequestDispatcherTest*"
~~~

The [quality workflow](.github/workflows/quality.yml) runs a locked and audited Release build, tests, package inspection and vulnerability inventory. The [testing standard](docs/standards/testing.md) shows how to verify packages in a separate output directory when the shared feed is in use.

For a cancellation fix in the request dispatcher, use [$ub-mode](.agents/skills/ub-mode/SKILL.md) to reproduce the observable caller behavior, [$api-contract](.agents/skills/api-contract/SKILL.md) to check existing callers, and [$verify-change](.agents/skills/verify-change/SKILL.md) for final checks.

To check command-policy changes, test allowed and denied samples through the hook launcher's JSON stdin. Compare neighboring rule cases with `codex execpolicy check --rules .codex/rules/default.rules --` followed by command tokens. This evaluates the tokens as data.
