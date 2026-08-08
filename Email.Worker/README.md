# Email Worker

`Email.Worker` is the independently deployable host that composes `Email`,
`Email.Outbox` and `Email.Outbox.Persistence`.

Run locally with smtp4dev listening on SMTP port 25:

```powershell
.\scripts\run-email-worker.ps1
```

Development enables SMTP delivery and the worker, applies the dedicated SQLite
migrations, and stores the queue under `Email.Worker/App_Data` when the project
directory is the content root. The smtp4dev web UI remains available at
`http://localhost:5000`.

Base, QA and Production settings keep delivery and polling disabled. Enable
both `Email:Enabled` and `EmailOutbox:Worker:Enabled` only after supplying valid
SMTP configuration. QA and Production also require an explicit migration
deployment decision because startup migrations are disabled there.

The OAuth SMTP option still expects a ready access token. Token acquisition and
refresh remain the responsibility of the host's credential provider; the
worker does not persist or refresh OAuth tokens automatically.
