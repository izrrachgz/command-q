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

const applicationUrl = 'https://staging.command-q.local';

export const environment: AppEnvironment = {
  name: 'staging',
  production: true,
  applicationUrl,
  api: {
    baseUrl: 'https://staging-api.command-q.local/',
    clientHeaderName: 'X-command-q-Client',
    clientId: 'replace-with-staging-client-id',
    endpoints: createApiEndpoints(),
  },
  identity: {
    authority: 'https://staging-identity.command-q.local/',
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
