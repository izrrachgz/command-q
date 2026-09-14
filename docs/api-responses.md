# API Response Contract

command-q uses HTTP status codes as the primary outcome signal. Successful API responses do not expose the internal application `Success`, `Message` or `Detail` wrappers.

## Success responses

- Resource queries return the resource directly with `200 OK`.
- Collection queries return an object with `items`, `total`, `page`, `pageSize` and `totalPages`.
- Creation endpoints return `201 Created`. When a resolvable resource endpoint exists, the response includes a `Location` header.
- Commands without a useful response representation return `204 No Content`.
- Commands that need a user-facing acknowledgement return `{ "message": "..." }`.

Application-layer `BasicResponse`, `DetailResponse<T>` and `PagedResponse<T>` remain internal workflow contracts. Controllers map them to the public response shapes.

## Error responses

All API errors use `application/problem+json` and ASP.NET Core `ProblemDetails` or `ValidationProblemDetails`.

Every problem contains:

- `type`: a stable `urn:command-q:error:{errorCode}` identifier;
- `title`: the localized HTTP error title;
- `status`: the HTTP status code;
- `detail`: a safe explanation when one is available;
- `instance`: the request path;
- `errorCode`: a stable machine-readable code;
- `traceId`: the request trace identifier.

Validation problems also contain an `errors` dictionary keyed by request property. API contracts do not expose FluentValidation implementation types.

Expected status mappings are:

- `400` for malformed input or validation failures;
- `401` when authentication is required or invalid;
- `403` when the caller or API client is not allowed;
- `404` when a requested resource does not exist;
- `409` for state or uniqueness conflicts;
- `429` when a configured request-rate or concurrency allowance is exhausted;
- `502` when an upstream operation fails;
- `503` when a required integration is unavailable;
- `500` for unexpected server failures.

`GET /accounts/me` returns `404 Not Found` with the stable
`accounts.profile_missing` error code when the authenticated identity has not
yet been linked to an application account. Authorization failures remain
`403 Forbidden` with `authorization.forbidden`.

Unexpected exception details are included only in Development. Production responses contain a sanitized explanation and the trace ID used to correlate server logs.

Operational health-check endpoints retain ASP.NET Core health-check response semantics so infrastructure probes can consume them without depending on the application error contract.

## Controller return types

Use `Task<ActionResult<T>>` when an action has a known success payload and may also return an error result. Use `Task<IActionResult>` for successful `204 No Content` operations or actions whose successful responses genuinely have no typed body.

New endpoints should return module/application DTOs as plain resources, `Api.Models.PageResponse<T>` for pages, or `Api.Models.OperationResponse` for acknowledgements. Do not expose internal application response wrappers from controllers.
