Project: Api
Purpose: Expose the CommandQ application HTTP API as an OAuth resource server.
Framework: .NET 10 (ASP.NET Core).
How to run: From the project root, use `dotnet run` or the Visual Studio launch profile.
Configuration: Check `appsettings.json`, user-secrets and environment variables.

Authentication:
- `IdentityServer` owns users, credentials, lockout, clients, grants and tokens.
- API validation settings live under `Authentication`.
- The API validates issuer discovery keys, audience, lifetime, `at+jwt` token type and the `command_q_api` scope.
- The API never issues JWTs or stores refresh tokens.
- `ClaimsPrincipalExtensions` reads protocol claims; `ICurrentAccountAccessor` resolves `(iss, sub)` to the application profile through `AccountIdentity`.
- Call authenticated `POST /accounts` once after initial identity registration/login to provision the application profile.
- API routes use lowercase resource names. Current-account resources live under `/accounts/me`; admin resources live under `/admin`.
- Successful API responses expose typed resources, pages or operation acknowledgements without internal `success`/`detail` wrappers. Errors use `ProblemDetails` with stable `errorCode` and `traceId` extensions; see `docs/api-responses.md`.
- `POST /files` accepts common documents, PDFs, text files, images, audio, short videos and ZIP files up to 200 MB. The Files module owns reusable category policies for future specialized upload endpoints.
  - Documents and text: `.pdf`, `.doc`, `.docx`, `.xls`, `.xlsx`, `.ppt`, `.pptx`, `.odt`, `.ods`, `.odp`, `.rtf`, `.txt`, `.csv`, `.md`.
  - Images: `.jpg`, `.jpeg`, `.png`, `.gif`, `.webp`, `.bmp`, `.tif`, `.tiff`, `.heic`, `.heif`.
  - Audio: `.mp3`, `.wav`, `.m4a`, `.aac`, `.ogg`, `.flac`.
  - Video: `.mp4`, `.m4v`, `.mov`, `.webm`, `.avi`, `.mpeg`, `.mpg`.
  - Archives: `.zip`.
  - Upload validation requires an allowed extension and matching declared media type. Stored content is not made executable or publicly served by this endpoint.

Development notes: Run IdentityServer before testing authenticated API workflows. The frontend remains optional and is not required by either backend host.
