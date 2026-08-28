export type EnvironmentName = 'development' | 'testing' | 'staging' | 'production';
export type SupportedCulture = 'en-US' | 'es-ES';

export interface AppEnvironment {
  readonly name: EnvironmentName;
  readonly production: boolean;
  readonly applicationUrl: string;
  readonly api: ApiEnvironment;
  readonly identity: IdentityEnvironment;
  readonly localization: LocalizationEnvironment;
  readonly preferences: PreferencesEnvironment;
  readonly logging: LoggingEnvironment;
  readonly build: BuildEnvironment;
}

export interface ApiEnvironment {
  readonly baseUrl: string;
  readonly clientHeaderName: string;
  readonly clientId: string;
  readonly endpoints: ApiEndpointMap;
}

export interface ApiEndpointMap {
  readonly account: {
    readonly create: string;
    readonly me: string;
    readonly activity: string;
    readonly preferences: string;
  };
  readonly admin: {
    readonly accounts: string;
    readonly accountDetails: (accountId: string) => string;
    readonly accountRoles: (accountId: string) => string;
    readonly email: string;
    readonly configurations: string;
    readonly roles: string;
  };
}

export interface IdentityEnvironment {
  readonly authority: string;
  readonly clientId: string;
  readonly scope: string;
  readonly redirectUrl: string;
  readonly postLogoutRedirectUri: string;
  readonly endpoints: IdentityEndpointMap;
}

export interface IdentityEndpointMap {
  readonly register: string;
  readonly signIn: string;
  readonly signOut: string;
  readonly me: string;
  readonly securityActivity: string;
  readonly external: (provider: string, returnUrl?: string) => string;
}

export interface LocalizationEnvironment {
  readonly defaultCulture: SupportedCulture;
  readonly fallbackCulture: SupportedCulture;
  readonly supportedCultures: readonly SupportedCulture[];
}

export interface PreferencesEnvironment {
  readonly cacheLifetimeMinutes: number;
  readonly backendRetryDelaySeconds: number;
}

export interface LoggingEnvironment {
  readonly file: FileLoggingEnvironment;
}

export interface FileLoggingEnvironment {
  readonly enabled: boolean;
  readonly endpoint: string;
}

export interface BuildEnvironment {
  readonly compressReleaseAssets: boolean;
  readonly obfuscateReleaseAssets: boolean;
}

const ApiEndpointSegments = {
  accounts: 'accounts',
  activity: 'activity',
  admin: 'admin',
  configurations: 'configurations',
  email: 'email',
  me: 'me',
  preferences: 'preferences',
  roles: 'roles',
} as const;

const IdentityEndpointSegments = {
  account: 'account',
  external: 'external',
  me: 'me',
  register: 'register',
  securityActivity: 'security-activity',
  signIn: 'sign-in',
  signOut: 'sign-out',
} as const;

const IdentityEndpointQueryParams = {
  returnUrl: 'returnUrl',
} as const;

const endpointPath = (...segments: readonly string[]): string => segments.join('/');

export const createApiEndpoints = (): ApiEndpointMap => ({
  account: {
    create: endpointPath(ApiEndpointSegments.accounts),
    me: endpointPath(ApiEndpointSegments.accounts, ApiEndpointSegments.me),
    activity: endpointPath(
      ApiEndpointSegments.accounts,
      ApiEndpointSegments.me,
      ApiEndpointSegments.activity,
    ),
    preferences: endpointPath(
      ApiEndpointSegments.accounts,
      ApiEndpointSegments.me,
      ApiEndpointSegments.preferences,
    ),
  },
  admin: {
    accounts: endpointPath(ApiEndpointSegments.admin, ApiEndpointSegments.accounts),
    accountDetails: (accountId) =>
      endpointPath(
        ApiEndpointSegments.admin,
        ApiEndpointSegments.accounts,
        encodeURIComponent(accountId),
      ),
    accountRoles: (accountId) =>
      endpointPath(
        ApiEndpointSegments.admin,
        ApiEndpointSegments.accounts,
        encodeURIComponent(accountId),
        ApiEndpointSegments.roles,
      ),
    email: endpointPath(ApiEndpointSegments.admin, ApiEndpointSegments.email),
    configurations: endpointPath(ApiEndpointSegments.admin, ApiEndpointSegments.configurations),
    roles: endpointPath(ApiEndpointSegments.admin, ApiEndpointSegments.roles),
  },
});

export const createIdentityEndpoints = (): IdentityEndpointMap => ({
  register: endpointPath(IdentityEndpointSegments.account, IdentityEndpointSegments.register),
  signIn: endpointPath(IdentityEndpointSegments.account, IdentityEndpointSegments.signIn),
  signOut: endpointPath(IdentityEndpointSegments.account, IdentityEndpointSegments.signOut),
  me: endpointPath(IdentityEndpointSegments.account, IdentityEndpointSegments.me),
  securityActivity: endpointPath(
    IdentityEndpointSegments.account,
    IdentityEndpointSegments.securityActivity,
  ),
  external: (provider, returnUrl = '/') =>
    `${endpointPath(
      IdentityEndpointSegments.account,
      IdentityEndpointSegments.external,
      encodeURIComponent(provider),
    )}?${IdentityEndpointQueryParams.returnUrl}=${encodeURIComponent(returnUrl)}`,
});
