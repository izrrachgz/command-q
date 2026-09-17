# Email.Client

`Email.Client` is command-q's host-neutral protobuf contract for command-emails.
It authenticates through Email.Api's REST token endpoint, caches the short-lived
session token, queues messages, and reads newest-first account message pages
through protobuf/gRPC. A revoked token is refreshed once before an operation
failure is returned to the caller.

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

`IEmailQueueClient.GetPageAsync` requires a server-derived account reference,
uses one-based page numbers, and accepts page sizes from 1 through 100. It
returns indexed summaries and delivery state supplied by Email.Api without
transporting message bodies or attachment bytes. `GetDetailAsync` is the
EmailAdmin-only, owner-scoped detail operation; attachments and inline resources
contain metadata only. Browser-facing account reads must derive their account
reference from the authenticated identity.

IdentityServer account email dispatches use the local identity-user GUID in
compact (`N`) form. command-q normalizes the authenticated `sub` claim to that
same representation before reading history. Other producers that want their
messages included in this history must use the same normalized subject as
`ApplicationEmailMetadata.AccountReference`.

`IEmailQueueClient.SearchAsync` calls the EmailAdmin-only search operation. It
supports optional account, text, recipient, status and UTC date filters. The
Email API always supplies the stable authenticated producer-owner scope from the
token's API-client identity; callers cannot use this operation to cross producer
identities or change ownership through request metadata.
