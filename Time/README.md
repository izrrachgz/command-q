# CommandQ.Time

`CommandQ.Time` supplies strict UTC validation, persistence precision, JSON serialization and time-zone conversion helpers for .NET 10. It builds on `TimeProvider` and NodaTime so applications can use testable clocks and unambiguous instants.

## Install

```powershell
dotnet add package CommandQ.Time
```

## Register the clock

```csharp
services.AddTimeServices();
```

This registers `TimeProvider.System`. Consumers inject `TimeProvider`, which can be replaced with a deterministic provider in tests.

```csharp
Instant now = timeProvider.GetCurrentInstant();
DateTime utcNow = timeProvider.GetCurrentUtcDateTime();
```

## UTC contracts

- `UtcDateTime.Require` accepts only `DateTimeKind.Utc` values.
- `UtcDateTime.RestoreFromDatabase` restores the UTC kind after providers return an unspecified database value and rejects local values.
- `UtcTime` truncates values to the shared six-digit persistence precision and formats/parses the canonical UTC persistence representation.
- `DateTimeUtcExtensions` converts between UTC and named time zones through NodaTime and validates time-zone identifiers.

Use the injected `TimeProvider` for current time. Reusable code should not call `DateTime.Now` or `DateTime.UtcNow` directly.

## JSON serialization

Register strict converters on the serializer options used by the consuming application:

```csharp
jsonSerializerOptions.AddUtcDateTimeConverters();
```

The converters enforce UTC input/output for `DateTime` and `DateTimeOffset` rather than silently accepting ambiguous local timestamps.

## Persistence guidance

Use `UtcTime.FormatForPersistence`, `UtcTime.ParseFromPersistence` and `UtcTime.TruncateToPersistencePrecision` when a provider needs the shared representation. Keep timezone display choices in the consuming application; persisted instants remain UTC.

## Tests

UTC, serialization, DI, conversion and architecture tests live in [`Time.Test`](../Time.Test/README.md).
