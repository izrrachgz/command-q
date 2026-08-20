import {
  CommandQEnvironment,
  createApiEndpoints,
  createIdentityEndpoints,
} from '../app/core/config/app-environment';
import {
  CommandQEnvironmentDefaults,
  createPostLogoutRedirectUri,
  createRedirectUrl,
  getRuntimeApplicationUrl,
} from './environment-defaults';

const applicationUrl = getRuntimeApplicationUrl();

export const environment: CommandQEnvironment = {
  name: 'development',
  production: false,
  applicationUrl,
  api: {
    baseUrl: CommandQEnvironmentDefaults.localApiBaseUrl,
    clientHeaderName: 'location_family',
    clientId: 'find_me',
    endpoints: createApiEndpoints(),
  },
  identity: {
    authority: CommandQEnvironmentDefaults.localIdentityAuthority,
    clientId: CommandQEnvironmentDefaults.spaClientId,
    scope: CommandQEnvironmentDefaults.oidcScope,
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
    cacheLifetimeMinutes: CommandQEnvironmentDefaults.preferencesCacheLifetimeMinutes,
    backendRetryDelaySeconds: CommandQEnvironmentDefaults.preferencesBackendRetryDelaySeconds,
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
