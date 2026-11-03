# Privacy boundaries

`command-q` provides product-neutral libraries and does not independently decide
why personal data is processed. Consuming applications remain responsible for
controller/processor roles, lawful basis, notices, consent, data-subject rights,
retention, deletion, residency and supplier management.

## Potential data paths

| Package | Potential personal-data interaction | Library boundary |
| --- | --- | --- |
| `CommandQ.Common` | Consumer-provided cache keys/values, HTTP bodies, headers, query values and file streams | Values are processed only when called. Cache keys and HTTP/file payloads are not written to library logs. Consumers own cache lifetime, HTTP destinations and content classification. |
| `CommandQ.Cqrs` | Arbitrary request/response objects passed through handlers and behaviors | In-memory dispatch only; no persistence or telemetry is added by the package. Consumers must keep sensitive payloads out of generic behavior logs. |
| `CommandQ.Localization` | Locale keys and application-owned translated content | The package composes sources but owns no user profile or translation catalog. Consumers determine whether localized content contains personal data. |
| `CommandQ.Persistence` | SQL commands, parameters, result objects and database session-context values | Connection strings, SQL text, parameter names/values and result payloads are excluded from operational templates. Consumers own authorization, schemas, encryption, retention, backups and row-level-security policy. |
| `CommandQ.Time` | Timestamps and time-zone identifiers that may become linkable when associated with a person | Conversion and serialization only; no independent storage, telemetry or profiling. |

Exceptions passed to consumer-configured logging may contain provider-generated
diagnostic text. Production applications must configure access-controlled sinks,
appropriate retention and redaction/filtering for exception details. Do not put
credentials or personal data in cache keys, SQL text, localization keys, exception
messages or package test fixtures.

Use synthetic data in this repository. A consuming system's processing inventory
must record the actual database, cache, HTTP, file, telemetry and backup flows
created when these libraries are configured.
