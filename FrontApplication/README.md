# CommandQ FrontApplication

Angular SPA for CommandQ. The app uses standalone components inside feature-owned Angular modules, with lazy route chunks for tenant and admin areas.

## Scripts

```powershell
npm start
npm run start:testing
npm run start:staging
npm run start:production
npm run log:development

npm run build
npm run build:testing
npm run build:staging
npm run build:production
npm run build:release
npm run build:release:obfuscated

npm test -- --run
```

`npm start` and `npm run build` default to the Development environment. `npm start` runs both Angular and the local development log writer. The repository launcher also supports:

```powershell
.\scripts\run-front.ps1 -Environment Development
.\scripts\run-front.ps1 -Environment Testing
.\scripts\run-front.ps1 -Environment Staging
.\scripts\run-front.ps1 -Environment Production
```

## Runtime Configuration

Environment files live under `src/environments`. Import only `environment`; Angular file replacements select the correct values at build or serve time.

Keep endpoint lookup centralized:

- `environment.api.baseUrl`
- `environment.api.endpoints`
- `environment.identity.authority`
- `environment.identity.endpoints`

Development expects the backend defaults:

- API: `https://localhost:7025/`
- IdentityServer: `https://localhost:7125/`
- SPA origin: the current browser origin, normally `http://localhost:4200/`

If Angular starts on a fallback port such as `4201`, the local SPA redirect URL follows that browser origin. Restart IdentityServer after changing or pulling development client redirect settings so the OpenIddict client seed includes the matching local redirect URI.

Development and testing `ng serve` profiles proxy OIDC protocol XHRs through `/identity/*` to `https://localhost:7125/*`. Browser redirects still go to IdentityServer directly, but discovery, token, userinfo, JWKS and revocation calls use the same-origin proxy so local Firefox certificate/CORS behavior does not block the SPA.

Development `ng serve` also proxies `/front-dev-log` to the local log writer started by `npm start`. The browser cannot write files directly, so frontend file logging is implemented as a local-only companion process. Logs are written to `FrontApplication/Logs/front-application-log-YYYYMMDD.txt`, roll when the file grows beyond 10 MB, and retain the latest 16 log files. Testing, staging and production environments keep file logging disabled.

If Firefox or another browser fails on `/.well-known/openid-configuration`, first verify the local ASP.NET Core HTTPS certificate:

```powershell
dotnet dev-certs https --check --trust
dotnet dev-certs https --trust
```

Open `https://localhost:7125/.well-known/openid-configuration` directly in the same browser before testing login. A certificate warning or TLS failure there will make the SPA auth flow look like a CORS problem because discovery never reaches the HTTP pipeline.

## Architecture

Feature code lives under `src/app/features/{area}/{feature}`. Each area owns a `*.module.ts` and `*.routes.ts` file; the root `src/app/app.routes.ts` only composes shells, guards and lazy feature modules. Shared shell concerns live under:

- `core`: auth, API clients/interceptors, i18n, preferences, reporting, shared models
- `layout`: public and authenticated shells
- `shared`: reusable components and form controls

Feature routes use `loadComponent` so pages still ship as lazy chunks inside the owning feature module. Tenant routes live under `/tenant/*`; admin routes live under `/admin/*`.

Components keep view markup in sibling `.html` files and behavior in `.ts` files. Shared form controls follow the same convention as feature pages.

Actions are used where a UI event orchestrates backend calls and store transitions, such as admin accounts and admin email. Simple shell, auth and preference event handlers call their facade/service directly until they need multi-step service/store coordination.

Keep repeated or externally visible strings close to the feature that owns them:

- route segments, route data keys and query parameter names: `core/routing`
- API headers, query names, sort fields and MIME types: `core/api/api-constants.ts`
- environment endpoint segments: `core/config/app-environment.ts`
- local storage keys: `core/config/frontend-storage-keys.ts`
- OIDC protocol paths and response values: `core/auth/oidc-protocol-constants.ts`
- frontend diagnostic log messages: `core/logging/frontend-log-messages.ts`
- report file names: `core/reporting/report-constants.ts`

## Authentication

The SPA uses `angular-auth-oidc-client` with authorization code + PKCE and refresh tokens. IdentityServer owns credentials, local/external login, and future social providers.

The login page starts the OIDC authorization flow and lets IdentityServer render the credential page as a first-party site. This avoids browser-specific third-party cookie behavior and keeps future Google, Microsoft and social providers behind the same identity boundary. API bearer tokens are attached by the OIDC interceptor for `environment.api.baseUrl`.

API context headers are added in `core/api/api-context.interceptor.ts`:

- `Accept-Language`
- `X-Correlation-Id`
- configured API client header, when present

Application account provisioning is handled by `accountProvisionGuard` and `AccountProvisionService`, which calls `POST /Account/create` only when `/Account/me` cannot resolve an application profile yet.

## Localization

Supported cultures mirror the backend defaults: `en-US` and `es-ES`.

Use `TextPipe` in templates:

```html
{{ 'action.save' | text }}
```

Use `TextKeys` with `I18nService.text()` or `text$()` in frontend services, actions, validators and report builders when text is needed outside the template layer:

```ts
this.i18n.text(TextKeys.app.documentTitle, { app: 'CommandQ', title: 'Dashboard' });
```

The interpolation syntax is `{{name}}`; `I18nService` resolves it with the supplied object. Route titles store a `titleKey` in route data, and `AppTitleStrategy` translates the browser title when navigation or culture changes. Preferences persist the selected culture locally and update the document language.

Templates may keep literal translation keys because the pipe is the resource boundary in markup. TypeScript should prefer `TextKeys` so feature actions, stores, reports and validators do not repeat string keys. Reports use translated sheet names and column headers at export time.

## Forms And UI

Reusable form controls live in `src/app/shared/forms`:

- text input with prefix/suffix icons
- password input
- textarea
- number
- date
- date-time
- time

Controls implement `ControlValueAccessor` and surface common validation states through `ValidationMessageComponent`.

Tailwind CSS and Roboto are bundled locally through npm packages. Icons use `@lucide/angular` and are also bundled.

Brand artwork is packaged with the app under `public/assets`. Use
`BrandAssets` and `BrandMarkComponent` for the CommandQ mark instead of
repeating asset paths in feature templates. The current robot mark is an
original local SVG and is not loaded from a CDN.

## Admin Stubs

Admin account and email pages are wired through actions, services, and stores. The UI is ready for backend endpoints, but the services intentionally point at future admin API routes and surface a backend-pending message instead of changing backend behavior in this pass.

## Reporting

`ReportExportService` uses `write-excel-file` through a dynamic import. This keeps the XLSX dependency out of the initial route payload until an export action is actually used.

## Release Output

Production builds use Angular optimization and output hashing. `build:release` adds `.gz` and `.br` sidecars. `build:release:obfuscated` applies a conservative JavaScript obfuscation pass before compression.
