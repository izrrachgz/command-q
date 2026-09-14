# Host Rate Limiting

The application API combines a configuration-selected default policy with explicit policies
for endpoints whose cost or abuse profile differs from ordinary application
traffic. Every rejection returns `429 Too Many Requests`; policies that expose a
retry delay also return `Retry-After`.

## Application API policy assignment

| Workload | Strategy | Partition | Default configuration |
| --- | --- | --- | --- |
| Ordinary account, contact, address and read traffic | Token bucket | Authenticated `(iss, sub)` | Burst 40, replenish 20 every 10 seconds, queue 8 |
| Account profile provisioning | Fixed window | Authenticated `(iss, sub)` | 6 requests per 60 seconds, no queue |
| Administrative account, role and configuration traffic | Sliding window | Authenticated `(iss, sub)` | 120 requests per 60 seconds, 6 segments, queue 4 |
| File uploads up to 200 MB | Concurrency | Whole API instance | 4 active uploads, queue 4 |

Authenticated partitions prevent unrelated users behind one proxy or NAT
address from consuming each other's ordinary API allowance. Requests without a
validated identity fall back to `HttpContext.Connection.RemoteIpAddress`.
Deployments that depend on the fallback must configure their hosting layer to
restore only trusted forwarded client addresses; never trust forwarded headers
from arbitrary callers.

The upload concurrency partition is intentionally global. A per-user limit alone
would allow many identities to exhaust the instance's network, temporary storage
or database resources simultaneously.

## Application API configuration

`Api:RateLimiting:Type` selects the default controller policy. Supported values
are `None`, `FixedWindow`, `SlidingWindow`, `TokenBucket` and `Concurrent`.
`TokenBucket` is the production-oriented default because the SPA commonly loads
several resources together and then becomes comparatively quiet.

Each strategy reads its current values from:

- `Api:RateLimiting:FixedWindow`
- `Api:RateLimiting:Sliding`
- `Api:RateLimiting:TokenBucket`
- `Api:RateLimiting:Concurrency`

These option values reload for new requests when the configuration provider
supports reload-on-change. Changing `Type` changes endpoint metadata and
therefore requires an application restart. Explicit administrative,
provisioning and upload assignments remain in effect independently of the
selected default unless rate limiting is disabled for the controller mapping.

## IdentityServer policy assignment

IdentityServer uses fixed policies by workload because authentication limits are
security controls rather than a selectable application-traffic default.

| Workload | Strategy | Partition | Default configuration |
| --- | --- | --- | --- |
| Registration, sign-in, token issuance and external-login initiation | Fixed window | Remote IP before authentication | 10 requests per 60 seconds, no queue |
| Email confirmation and password recovery challenges | Sliding window | Remote IP before authentication | 30 requests per 5 minutes, 10 segments, no queue |
| Ordinary OIDC and authenticated account traffic | Token bucket | Authenticated subject, otherwise remote IP | Burst 120, replenish 60 every 10 seconds, queue 12 |
| Internal identity administration | Sliding window | Authenticated subject, otherwise remote IP | 120 requests per 60 seconds, 6 segments, queue 4 |

The HTTP limits complement rather than replace ASP.NET Identity lockout and the
persisted three-attempt challenge/reset budgets. Sharing the anonymous limits
across related endpoints prevents an attacker from bypassing protection by
rotating routes. Health checks are not mapped through the controller policies.

IdentityServer values live under `IdentityServer:RateLimiting`. Values apply to
new partitions when configuration reload is supported. Set
`IdentityServer:RateLimiting:Enabled` to `false` and restart IdentityServer for
an unthrottled benchmark.

For capacity benchmarks, first disable the relevant host limiter to measure the
application and database ceiling. Repeat the same scenario with production policies enabled to
measure user-visible throttling and verify that overload produces controlled
`429` responses rather than resource exhaustion.
