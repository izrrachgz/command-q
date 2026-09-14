# Email.Client

`Email.Client` is command-q's host-neutral producer contract for command-emails.
It authenticates through Email.Api's REST token endpoint, caches the short-lived
session token, and queues messages through protobuf/gRPC. A revoked token is
refreshed once before an enqueue failure is returned to the caller.

Hosts register the client against their own configuration section:

```csharp
services.AddEmailApiClient(hostConfiguration);
```

The supplied section must contain `EmailApiClient` with `Enabled`, `BaseUrl`,
`ApiClientHeaderName`, `ApiClientKey`, `UserName`, `Password`, and
`TimeoutSeconds`. Store the key and password in user-secrets or the deployment
secret store.

The protobuf file is intentionally duplicated from command-emails so these
repositories remain independently buildable. Preserve package/service names and
field numbers when evolving it.
