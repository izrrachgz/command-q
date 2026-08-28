import { AppRoutePaths } from '../app/core/routing/app-route-paths';

export const AppEnvironmentDefaults = {
  localApiBaseUrl: 'https://localhost:7025/',
  localApplicationUrl: 'http://localhost:4200',
  localIdentityAuthority: 'https://localhost:7125/',
  oidcScope: 'openid profile email roles offline_access command-q_api',
  preferencesBackendRetryDelaySeconds: 30,
  preferencesCacheLifetimeMinutes: 15,
  spaClientId: 'command-q_spa',
} as const;

export const getRuntimeApplicationUrl = (): string => {
  return globalThis.location?.origin ?? AppEnvironmentDefaults.localApplicationUrl;
};

export const createRedirectUrl = (applicationUrl: string): string => {
  return `${trimTrailingSlash(applicationUrl)}${AppRoutePaths.auth.callback}`;
};

export const createPostLogoutRedirectUri = (applicationUrl: string): string => {
  return `${trimTrailingSlash(applicationUrl)}/`;
};

const trimTrailingSlash = (value: string): string => {
  return value.replace(/\/+$/, '');
};
