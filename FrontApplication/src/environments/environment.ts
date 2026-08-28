import {
  AppEnvironment,
  createApiEndpoints,
  createIdentityEndpoints,
} from '../app/core/config/app-environment';
import {
  AppEnvironmentDefaults,
  createPostLogoutRedirectUri,
  createRedirectUrl,
  getRuntimeApplicationUrl,
} from './environment-defaults';

const applicationUrl = getRuntimeApplicationUrl();

export const environment: AppEnvironment = {
  name: 'development',
  production: false,
  applicationUrl,
  api: {
    baseUrl: AppEnvironmentDefaults.localApiBaseUrl,
    clientHeaderName: 'location_family',
    clientId: 'find_me',
    endpoints: createApiEndpoints(),
  },
  identity: {
    authority: AppEnvironmentDefaults.localIdentityAuthority,
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
      enabled: true,
      endpoint: '/front-dev-log',
    },
  },
  build: {
    compressReleaseAssets: false,
    obfuscateReleaseAssets: false,
  },
};
