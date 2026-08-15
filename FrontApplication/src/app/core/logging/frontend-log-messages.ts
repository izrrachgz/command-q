export const FrontendLogMessages = {
  accountProfileMissing: 'Account profile is missing.',
  angularUnhandledError: 'Unhandled Angular error.',
  authOperationFailed: (operation: string, message: string) => `[auth] ${operation} failed: ${message}`,
  frontendLoggingInitialized: 'Frontend local file logging initialized.',
  identityHttpError: (status: number, statusText: string, url?: string | null) =>
    `HTTP ${status} ${statusText || 'error'} from ${url ?? 'the identity service'}.`,
  identityUnreachable: (url?: string | null) =>
    `Unable to reach ${url ?? 'the identity service'}. Check HTTPS trust, server availability, and CORS.`,
  oidcClientDebug: 'OIDC client debug.',
  oidcClientError: 'OIDC client error.',
  oidcClientWarning: 'OIDC client warning.',
  provisionAccountLoadSkipped: 'Application account was not loaded before provisioning.',
  provisionCompleted: 'Application account provisioning completed.',
  provisionCompletedWithoutPayload: 'Application account provisioning completed without an account payload.',
  provisionFailed: 'Application account provisioning failed.',
  provisionPageFailed: 'Provision page could not complete account provisioning.',
  provisionStarted: 'Starting application account provisioning.',
  unknownAuthenticationError: 'Unknown authentication error.',
  unhandledBrowserError: 'Unhandled browser error.',
  unhandledPromiseRejection: 'Unhandled promise rejection.',
} as const;
