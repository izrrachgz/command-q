# Frontend integration

The Angular SPA is maintained in the separate
[`command-q-front`](https://github.com/izrrachgz/command-q-front) repository.
The repositories have independent dependency graphs, CI pipelines and releases.

For local development, keep sibling checkouts and start each stack from its own
repository:

```text
repos/
  command-q/
  command-q-front/
```

The backend owns the public HTTP and authentication contracts:

- API routes, request/response shapes and `ProblemDetails` errors;
- API CORS origins under `Api:Cors`;
- the `command-q_api` scope and token validation;
- OpenIddict client registration, redirect URIs and logout URIs; and
- IdentityServer CORS origins.

The frontend owns API and authority base URLs, its `command-q_spa` client ID,
requested scopes and callback routes. Development defaults currently expect the
API at `https://localhost:7025/`, IdentityServer at
`https://localhost:7125/`, and Angular at `http://localhost:4200/`.

Changes to endpoints, DTOs, response envelopes, scopes, claims, client IDs,
redirects or browser origins require linked pull requests in both repositories.
Keep backend changes compatible until the matching frontend version is deployed.
The authoritative API response conventions remain in
[`api-responses.md`](api-responses.md).

The API does not currently publish an OpenAPI document. If one is added, treat
it as the machine-readable contract and add frontend schema-diff or generated
client validation to both CI workflows.
