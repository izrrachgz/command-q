import {
  AppEnvironment,
  createApiEndpoints,
  createIdentityEndpoints,
} from '../app/core/config/app-environment';
import {
  AppEnvironmentDefaults,
  createPostLogoutRedirectUri,
  createRedirectUrl,
} from './environment-defaults';

const applicationUrl = 'https://app.command-q.local';

export const environment: AppEnvironment = {
  name: 'production',
  production: true,
  applicationUrl,
  api: {
    baseUrl: 'https://api.command-q.local/',
    clientHeaderName: 'X-command-q-Client',
    clientId: 'replace-with-production-client-id',
    endpoints: createApiEndpoints(),
  },
  identity: {
    authority: 'https://identity.command-q.local/',
    clientId: AppEnvironmentDefaults.spaClientId,
    scope: AppEnvironmentDefaults.oidcScope,
    redirectUrl: createRedirectUrl(applicationUrl),
    postLogoutRedirectUri: createPostLogoutRedirectUri(applicationUrl),
    endpoints: createIdentityEndpoints(),
  },
  localization: {
    defaultCulture: 'en-US',
    fallbackCulture: 'en-US',
    supportedCultures: ['en-US', 'es-ES'],
  },
  preferences: {
    cacheLifetimeMinutes: AppEnvironmentDefaults.preferencesCacheLifetimeMinutes,
    backendRetryDelaySeconds: AppEnvironmentDefaults.preferencesBackendRetryDelaySeconds,
  },
  logging: {
    file: {
      enabled: false,
      endpoint: '',
    },
  },
  build: {
    compressReleaseAssets: true,
    obfuscateReleaseAssets: false,
  },
};
