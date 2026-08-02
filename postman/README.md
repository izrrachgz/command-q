# CommandQ Postman

The collection exercises the standalone IdentityServer and application API using authorization code with PKCE. It does not use the removed password grant or the former API-issued JWT flow.

## Import

Import these files into Postman:

- `CommandQ.postman_collection.json`
- `environments/CommandQ.Development.postman_environment.json`
- the QA or Production environment template when needed

Select `CommandQ - Development` as the active environment and start the Visual Studio `Backend (IdentityServer + API)` profile. Trust the local ASP.NET Core HTTPS certificate with `dotnet dev-certs https --trust` if needed.

## Local Secrets

Set these values locally in the imported Development environment before sending authenticated requests:

- `accountPassword`: a new password satisfying the IdentityServer policy
- `apiClientKey`: the value configured in the API's `ApiClients:0:Id` user-secret

The former password, `encoding`, does not satisfy the new minimum length and complexity policy. Choose a new local password; do not place it in the committed environment file.

The committed environment already contains:

- `accountUsername`: `TurboDecoder`
- `accountEmail`: `admin@local.com`
- `apiClientHeaderName`: `location_family`
- local IdentityServer/API URLs and the `command_q_local` OAuth client

Do not share or commit an exported environment after adding passwords, API keys or tokens. Postman marks those variables as secret, but repository files must remain secret-free.

## Initial Account Flow

Send these requests in order:

1. `01 - Identity account / Register legacy local account`
2. `01 - Identity account / Sign in legacy local account`
3. `02 - OAuth 2.0 with PKCE / Create authorization code`
4. `02 - OAuth 2.0 with PKCE / Exchange authorization code`
5. `03 - Application API / Account profile / Provision application profile`

The registration request creates the former local account in ASP.NET Identity. The old seed used the `User` role, so the new account receives the same default role. IdentityServer and application account IDs are generated afresh after the database reset. A registration or profile-provisioning `400` is expected when that record already exists.

The authorization request generates a fresh PKCE verifier/challenge and intentionally does not follow the callback redirect. Its post-response script validates `state`, captures the authorization code from the `Location` header and stores it in the active environment. The token exchange stores access, refresh and ID tokens as environment secrets.

Postman's cookie jar must remain enabled between sign-in and authorization. The application API folder validates the access token type, issuer, audience, scope and expiration before explicitly adding it as a bearer token. It also injects the configured API-client header and `Origin`. If the access token has expired, run `02 - OAuth 2.0 with PKCE / Refresh access token`; if the refresh token is unavailable or rejected, repeat the authorization-code flow.

IdentityServer sign-in and API authorization are separate. Identity account endpoints use the IdentityServer cookie, while the application API requires a short-lived OAuth access token. After obtaining a valid access token, run `Provision application profile` once to create the application-owned `Account` and `AccountIdentity` records. Until that request succeeds, the `command_q` account tables are expected to be empty.

## Other Environments

QA and Production use non-resolving `.invalid` URL placeholders. Before using either template, configure its real URLs, OAuth client ID, API-client header/key, allowed origin and account values. The Postman callback URI must also be registered for that environment's OpenIddict client.

Use `99 - Session cleanup` to revoke the refresh token and clear the IdentityServer cookie. Revoking a refresh token or ending the browser session does not immediately invalidate an already issued access token.
