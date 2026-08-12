# Email

`Email` is a reusable .NET 10 SMTP delivery library backed by MailKit and
MimeKit. It is transport infrastructure, not an application feature, and is not
referenced by API or IdentityServer until a host explicitly chooses to use it.

## Registration

Reference `Email.csproj` from the consuming host and register the services:

```csharp
using Email.Extensions;

services.AddEmailServices(configuration);
```

The project copies `Email/appsettings.json` to the same relative output path.
Registration loads that file first and then layers the consuming host's
configuration over it, so user-secrets, environment variables and deployment
providers remain authoritative. The merged `Email` section is validated on
startup when delivery is enabled, and `IEmailSender` is exposed as a singleton.
Delivery is disabled by default so a host cannot accidentally contact a
placeholder endpoint. Sending while disabled throws `EmailDeliveryException`
with the `Disabled` classification and performs no network work.

`IEmailSender` uses `IOptionsMonitor<EmailOptions>`. When the configuration
provider supports reload-on-change, each new batch uses the latest valid SMTP,
sender, authentication and delivery-policy settings. A batch captures one
configuration snapshot for its entire connection, so a reload cannot change an
active SMTP session. Reload events record only safe endpoint and mode metadata;
credentials and OAuth tokens are never logged.

Hosts that namespace their settings can pass an exact section instead:

```csharp
services.AddEmailServices(
    configuration.GetRequiredSection("EmailWorker:Email"));
```

This keeps the reusable library independent of the host's chosen prefix. The
CommandQ worker uses this form.

Configuration can also be supplied directly for tests or programmatic hosts:

```csharp
services.AddEmailServices(options =>
{
    options.Enabled = true;
    options.Smtp.Host = "smtp.example.com";
    options.Authentication.Type = EmailAuthenticationType.Anonymous;
    options.Sender.Address = "no-reply@example.com";
});
```

## Configuration

```json
{
  "Email": {
    "Enabled": true,
    "Smtp": {
      "Host": "smtp.example.com",
      "Port": 587,
      "Security": "StartTls",
      "TimeoutMilliseconds": 100000,
      "CheckCertificateRevocation": true
    },
    "Authentication": {
      "Type": "Anonymous",
      "Login": {
        "Username": "",
        "Password": ""
      },
      "Plain": {
        "Username": "",
        "Password": ""
      },
      "OAuth": {
        "Username": "",
        "AccessToken": ""
      }
    },
    "Sender": {
      "Address": "no-reply@example.com",
      "Name": "CommandQ"
    },
    "DeliveryPolicy": {
      "MaxRecipients": 100,
      "MaxAttachmentCount": 20,
      "MaxAttachmentBytes": 26214400,
      "MaxTotalAttachmentBytes": 41943040,
      "AllowedSenderDomains": [ "example.com" ]
    }
  }
}
```

Keep SMTP credentials in user-secrets or the deployment secret store. The
equivalent environment variables are:

```text
Email__Enabled
Email__Smtp__Host
Email__Smtp__Port
Email__Smtp__Security
Email__Smtp__TimeoutMilliseconds
Email__Smtp__CheckCertificateRevocation
Email__Authentication__Type
Email__Authentication__Login__Username
Email__Authentication__Login__Password
Email__Authentication__Plain__Username
Email__Authentication__Plain__Password
Email__Authentication__OAuth__Username
Email__Authentication__OAuth__AccessToken
Email__Sender__Address
Email__Sender__Name
Email__DeliveryPolicy__MaxRecipients
Email__DeliveryPolicy__MaxAttachmentCount
Email__DeliveryPolicy__MaxAttachmentBytes
Email__DeliveryPolicy__MaxTotalAttachmentBytes
Email__DeliveryPolicy__AllowedSenderDomains__0
```

Those names apply when `Email` is at the configuration root. The CommandQ
worker owns the section under `EmailWorker:Email`, so its equivalents start with
`EmailWorker__Email__`, for example `EmailWorker__Email__Smtp__Host`.

`StartTls` is the secure default. Use `SslOnConnect` for implicit TLS endpoints
and `None` only for deliberately unsecured anonymous local test relays.
Certificate validation is never bypassed by this library.

All recipients, regular attachments and inline resources are validated before
the SMTP connection opens. The configured policy limits recipient count,
individual content size, total content size and sender domains. An empty
`AllowedSenderDomains` collection allows any syntactically valid sender domain.

## Authentication

Select one authentication type. Only its matching values are validated and
used:

| Type | MailKit mechanism | Required values |
| --- | --- | --- |
| `Anonymous` | No authentication call | None |
| `Login` | SASL LOGIN | `Login:Username`, `Login:Password` |
| `Plain` | SASL PLAIN | `Plain:Username`, `Plain:Password` |
| `OAuth` | OAuth 2.0 bearer/XOAUTH2 | `OAuth:Username`, `OAuth:AccessToken` |

LOGIN, PLAIN and OAuth require `StartTls` or `SslOnConnect`; startup validation
rejects authenticated delivery over `None`, `Auto` or
`StartTlsWhenAvailable`. The OAuth access token must already be valid and ready
for the SMTP server. This library does not request, refresh or persist OAuth
tokens.

The committed project settings keep every credential and token empty. Put
secrets in the consuming host's user-secrets, environment variables or
deployment secret store rather than committing them to `Email/appsettings.json`.

## Sending

Common text and HTML overloads return the SMTP acceptance result:

```csharp
var result = await emailSender.SendAsync(
    "person@example.com",
    "Plain text subject",
    "The body is ready to send.",
    cancellationToken);

logger.LogInformation(
    "SMTP accepted {MessageId}: {Response}",
    result.MessageId,
    result.SmtpResponse);

await emailSender.SendHtmlAsync(
    "person@example.com",
    "HTML subject",
    "<p>The body is already rendered.</p>",
    cancellationToken);
```

Pass file paths to attach existing files. MimeKit infers each content type and
attachment name from the file:

```csharp
await emailSender.SendAsync(
    "person@example.com",
    "Files",
    "The requested files are attached.",
    new[] { invoicePath, reportPath },
    cancellationToken);
```

Use `FromBytes` for application-generated content. `FromStreamAsync` copies a
readable stream immediately, so the sender does not retain or dispose the
caller's stream:

```csharp
var generatedAttachment = EmailAttachment.FromBytes(
    "report.csv",
    reportBytes,
    "text/csv");

var uploadedAttachment = await EmailAttachment.FromStreamAsync(
    "document.pdf",
    uploadedStream,
    "application/pdf",
    cancellationToken);

await emailSender.SendAsync(
    "person@example.com",
    "Generated files",
    "The files are attached.",
    new[] { generatedAttachment, uploadedAttachment },
    cancellationToken);
```

For full control over sender, To, Cc, Bcc, Reply-To, text/HTML alternatives and
attachment display names, send an `EmailMessage`:

```csharp
await emailSender.SendAsync(new EmailMessage
{
    To = new[] { new EmailAddress("person@example.com", "Person") },
    Cc = new[] { new EmailAddress("audit@example.com") },
    Subject = "Complete message",
    TextBody = "Plain-text alternative",
    HtmlBody = "<p>HTML alternative <img src=\"cid:brand-logo\"></p>",
    Attachments = new[] { new EmailAttachment(reportPath, "report.pdf") },
    InlineResources = new[]
    {
        new EmailInlineResource(logoPath, "brand-logo")
    }
}, cancellationToken);
```

Inline resource content IDs must be unique within a message and require an HTML
body. Byte and copied-stream factories are available for inline resources too.

## HTML Templates

The template overload reads an HTML file verbatim:

```csharp
await emailSender.SendHtmlTemplateAsync(
    "person@example.com",
    "Rendered template",
    renderedTemplatePath,
    attachmentPaths,
    cancellationToken);
```

The library deliberately performs no placeholder replacement, Razor execution,
localization or model binding. Render and validate the final HTML before calling
the sender.

## Batch Behavior

Use the message collection overload for multiple independent messages:

```csharp
var batchResult = await emailSender.SendAsync(messages, cancellationToken);
```

The sender validates every message and attachment path before connecting, then
opens one SMTP connection, authenticates once, sends sequentially, disconnects
once and disposes the client. SMTP clients are not used concurrently. An empty
batch performs no network work.

Batch delivery is fail-fast. If the third message fails, the first two may
already have been delivered; SMTP does not provide a transactional batch. The
connection is still disconnected and disposed. `EmailDeliveryException`
contains the requested count, failed message number and ID when available, plus
an `EmailSendResult` for every message already accepted:

```csharp
try
{
    await emailSender.SendAsync(messages, cancellationToken);
}
catch (EmailDeliveryException exception)
{
    logger.LogWarning(
        exception,
        "Email failed as {FailureKind} after {SentCount}/{RequestedCount} messages.",
        exception.FailureKind,
        exception.SentCount,
        exception.RequestedCount);
}
```

Failure classifications distinguish disabled delivery, authentication,
connectivity, transient SMTP, permanent SMTP, protocol and unknown failures.
Do not blindly retry in the catch block: a durable outbox/worker must use the
accepted-message details for deduplication and retry decisions.

The solution provides that durable workflow through `Email.Outbox`,
`Email.Outbox.Persistence` and the standalone `Email.Worker`. File and in-memory
content can be snapshotted through `CopyContentToAsync` before it is persisted.
See `Email.Outbox/README.md` for enqueueing and delivery semantics.

Structured email log fields contain endpoint and batch counts only. Recipients,
subjects, bodies, attachment names and credentials are intentionally excluded;
transport exception details remain subject to the SMTP provider's messages.

## smtp4dev Integration Test

`Email.IntegrationTest` is opt-in because normal builds and CI should not depend
on a developer service. Its defaults expect smtp4dev SMTP on `localhost:25` and
the web API on `http://localhost:5000`:

```powershell
.\scripts\test-email-integration.ps1
```

The test sends a unique message and verifies SMTP acceptance, text and HTML
bodies, a normal attachment and a CID resource through smtp4dev's HTTP API. It
deletes the received test message afterward. Override local endpoints with the
script parameters or `Smtp4Dev__*` environment variables.
