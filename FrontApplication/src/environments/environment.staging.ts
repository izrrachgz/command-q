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

const applicationUrl = 'https://staging.commandq.local';

export const environment: CommandQEnvironment = {
  name: 'staging',
  production: true,
  applicationUrl,
  api: {
    baseUrl: 'https://staging-api.commandq.local/',
    clientHeaderName: 'X-CommandQ-Client',
    clientId: 'replace-with-staging-client-id',
    endpoints: createApiEndpoints(),
  },
  identity: {
    authority: 'https://staging-identity.commandq.local/',
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
