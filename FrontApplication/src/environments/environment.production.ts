import {
  CommandQEnvironment,
  createApiEndpoints,
  createIdentityEndpoints,
} from '../app/core/config/app-environment';
import {
  CommandQEnvironmentDefaults,
  createPostLogoutRedirectUri,
  createRedirectUrl,
} from './environment-defaults';

const applicationUrl = 'https://app.commandq.local';

export const environment: CommandQEnvironment = {
  name: 'production',
  production: true,
  applicationUrl,
  api: {
    baseUrl: 'https://api.commandq.local/',
    clientHeaderName: 'X-CommandQ-Client',
    clientId: 'replace-with-production-client-id',
    endpoints: createApiEndpoints(),
  },
  identity: {
    authority: 'https://identity.commandq.local/',
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
      enabled: false,
      endpoint: '',
    },
  },
  build: {
    compressReleaseAssets: true,
    obfuscateReleaseAssets: false,
  },
};
