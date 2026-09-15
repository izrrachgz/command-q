# command-q benchmark suite

This suite establishes a repeatable local baseline for the command-q API, IdentityServer, Email API REST and gRPC transports, and Jobs API. It separates application capacity from deliberate rate-limit behavior:

- Ceiling starts a host with rate limiting disabled.
- Protected starts the same Release host with its normal limiter enabled.
- smoke, ramp, spike, and soak control the k6 load shape.

The scripts keep the ASP.NET Core environment at Development by default so existing local user-secrets continue to load. They do not contain or print credentials.

## Prerequisites

- Build and test all three repositories before measuring.
- Install [k6](https://grafana.com/docs/k6/latest/set-up/install-k6/).
- Use isolated benchmark databases with representative data. Do not point write scenarios at production.
- Seed one confirmed IdentityServer account with a known valid password.
- Configure the existing API-client headers and internal Email/Jobs bootstrap accounts through user-secrets.
- Prefer running k6 on another machine for publishable numbers. Local runs are useful for regression comparisons but make the server and load generator compete for CPU.

dotnet-counters is optional:

    dotnet tool install --global dotnet-counters

## Start a host

Build once, then start each host in a separate terminal without rebuilding between benchmark passes:

    dotnet build .\command-q.sln -c Release
    .\benchmarks\start-host.ps1 -HostName Api -Profile Ceiling -NoBuild

Supported host names are Api, IdentityServer, EmailApi, EmailWorker, and JobsApi. The Email and Jobs projects are resolved from sibling repositories under the same parent directory.
EmailWorker has no HTTP rate limiter, so its selected profile changes only the
common benchmark logging behavior.

Run the ceiling and protected passes against the same database snapshot, machine, environment, and k6 profile:

    .\benchmarks\start-host.ps1 -HostName JobsApi -Profile Protected -NoBuild

OpenTelemetry export is disabled unless explicitly requested. Start an OTLP collector first, then use:

    .\benchmarks\start-host.ps1 -HostName Api -Profile Protected -OpenTelemetry -NoBuild

The default collector endpoint is http://localhost:4317; override it with -OtlpEndpoint. The hosts export runtime, HTTP, Kestrel, authentication, and rate-limiter metrics. The rate-limiter meter includes policy and result dimensions, so rejections can be attributed to a specific policy without per-request logging. The Email worker additionally exports its delivery, outbox, queue-age, retry, dead-letter, batch, and poll metrics.

## Configure credentials

Set only the variables required by the selected scenario in the terminal that runs k6. Values below are placeholders:

    $env:API_CLIENT_HEADER = "<Api:ApiClients entry Name>"
    $env:API_CLIENT_KEY = "<Api:ApiClients entry Id>"
    $env:API_ACCESS_TOKEN = "<fresh command-q access token>"

    $env:IDENTITY_USERNAME = "<confirmed benchmark account>"
    $env:IDENTITY_PASSWORD = "<benchmark account password>"

    $env:EMAIL_API_CLIENT_HEADER = "<EmailApi:Clients entry Name>"
    $env:EMAIL_API_CLIENT_KEY = "<EmailApi:Clients entry Id>"
    $env:EMAIL_API_USERNAME = "<Email API bootstrap username>"
    $env:EMAIL_API_PASSWORD = "<Email API bootstrap password>"
    $env:BENCHMARK_EMAIL = "benchmark@example.test"

    $env:JOBS_API_CLIENT_HEADER = "<JobsApi:Clients entry Name>"
    $env:JOBS_API_CLIENT_KEY = "<JobsApi:Clients entry Id>"
    $env:JOBS_API_USERNAME = "<Jobs API bootstrap username>"
    $env:JOBS_API_PASSWORD = "<Jobs API bootstrap password>"

Email and Jobs scenarios acquire one access token during k6 setup. The command-q API scenario accepts a previously issued access token because the SPA uses the interactive authorization-code flow. Renew tokens before they expire during a soak. If needed, increase the configured access-token lifetime only in the isolated benchmark environment and issue a new token afterward.

Self-signed local HTTPS certificates are accepted by default. Set INSECURE_SKIP_TLS_VERIFY=false outside local development.
Benchmark request bodies are discarded by k6 after status and timing capture;
only the one-time internal token response is retained. This reduces load-generator
memory and garbage-collection overhead during same-machine tests.

## Run scenarios

Always run a smoke test first:

    .\benchmarks\run-k6.ps1 -Scenario ApiRead -Mode smoke
    .\benchmarks\run-k6.ps1 -Scenario IdentitySignIn -Mode smoke
    .\benchmarks\run-k6.ps1 -Scenario EmailRestEnqueue -Mode smoke
    .\benchmarks\run-k6.ps1 -Scenario EmailGrpcEnqueue -Mode smoke
    .\benchmarks\run-k6.ps1 -Scenario JobsRead -Mode smoke

Then run a ramp, spike, or soak:

    .\benchmarks\run-k6.ps1 -Scenario ApiRead -Mode ramp -TargetVirtualUsers 100 -SummaryExport .\benchmarks\results\api-ceiling-ramp.json

To retain host and k6 CPU/memory measurements with the summary, pass the exact host process ID to the measured wrapper:

    .\benchmarks\run-measured-k6.ps1 -Scenario ApiRead -ProcessId 12345 -Mode ramp -TargetVirtualUsers 100 `
        -SummaryExport .\benchmarks\results\api-ceiling-ramp.json `
        -ResourceExport .\benchmarks\results\api-ceiling-ramp.resources.json

Default load shapes:

| Mode | Shape |
| --- | --- |
| smoke | 1 VU for 30 seconds |
| ramp | 30-second ramp, 2-minute hold, 30-second ramp down |
| spike | 10-second ramp, 30-second hold, 20-second ramp down |
| soak | 1-minute ramp, 30-minute hold, 1-minute ramp down |

Durations can be overridden with SMOKE_DURATION, RAMP_UP_DURATION, HOLD_DURATION, RAMP_DOWN_DURATION, SPIKE_UP_DURATION, SPIKE_HOLD_DURATION, SPIKE_DOWN_DURATION, and SOAK_DURATION.

The initial latency gates are p95 below 500 ms and p99 below 1 second. Override them with P95_MS and P99_MS. They are regression gates, not capacity promises; record a baseline before tightening them.

HTTP 429 and gRPC ResourceExhausted responses increment throttled_requests and are excluded from unexpected_responses. This lets a protected run finish while making limiter saturation visible. business_success shows the proportion of useful responses and should be compared directly between ceiling and protected passes.

## Scenario behavior

| Scenario | Endpoint | State impact |
| --- | --- | --- |
| ApiRead | configurable, defaults to GET /accounts/me | application database read |
| IdentitySignIn | POST /account/sign-in | password verification, cookie issuance, and security-event write |
| EmailRestEnqueue | POST /v1/email-queue | inserts one outbox message per iteration |
| EmailGrpcEnqueue | EmailQueue/Enqueue | inserts one outbox message per iteration |
| JobsRead | GET /jobs?page=1&pageSize=25 | jobs database read |

Use valid IdentityServer credentials. Invalid credentials can lock the benchmark account and also measure a different path. The protected authentication policy intentionally throttles repeated sign-ins aggressively; use the ceiling pass to find authentication-path capacity.

Leave the Email worker stopped when measuring queue-ingress capacity. Run it with smtp4dev only for a separately labeled end-to-end delivery test at a controlled rate, otherwise SMTP throughput and the local mailbox become the bottleneck. Reset the isolated outbox database between comparable write tests.

The gRPC script expects command-emails beside command-q. Override EMAIL_GRPC_ADDRESS or set EMAIL_GRPC_PLAINTEXT=true when the endpoint differs.

## Observe the host

Find the server process:

    dotnet-counters ps

Then monitor it:

    .\benchmarks\monitor-dotnet.ps1 -ProcessId 12345

At minimum, retain:

- achieved iterations/requests per second;
- p50, p95, and p99 latency;
- business_success, throttled_requests, unexpected responses, and 5xx errors;
- process CPU, working set, allocation rate, GC pauses, and thread-pool queue length;
- Kestrel active/queued connections and ASP.NET Core active requests;
- rate-limit active leases, queue time, requests, policy, and result;
- database CPU, connection-pool use, lock waits, and slow queries.

Warm the host with a smoke run before recording results. Increase load until latency bends sharply, errors grow, or a resource remains saturated. The safe operating target should stay below that knee with headroom, rather than using the last technically successful request rate.
