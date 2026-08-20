import { TextKeys } from '../i18n/text-keys';

export const AppRouteAreas = {
  admin: 'admin',
  tenant: 'tenant',
} as const;

export type AppRouteArea = (typeof AppRouteAreas)[keyof typeof AppRouteAreas];

export const AppRouteSegments = {
  account: 'account',
  accounts: 'accounts',
  admin: 'admin',
  auth: 'auth',
  callback: 'callback',
  dashboard: 'dashboard',
  configurations: 'configurations',
  email: 'email',
  login: 'login',
  preferences: 'preferences',
  profile: 'profile',
  provision: 'provision',
  roles: 'roles',
  setup: 'setup',
  register: 'register',
  root: '',
  tenant: 'tenant',
  wildcard: '**',
} as const;

export const AppRouteDataKeys = {
  area: 'area',
  roles: 'roles',
  titleKey: 'titleKey',
} as const;

export const AppRouteQueryParams = {
  realm: 'realm',
  returnUrl: 'returnUrl',
} as const;

export const AppRouteTitleKeys = {
  accountProvision: TextKeys.provision.title,
  adminAccounts: TextKeys.admin.accounts.title,
  adminConfigurations: TextKeys.admin.configurations.title,
  adminDashboard: TextKeys.admin.dashboard.title,
  adminEmail: TextKeys.admin.email.title,
  adminPreferences: TextKeys.admin.preferences.title,
  adminRoles: TextKeys.admin.roles.title,
  authCallback: TextKeys.auth.callback,
  authLogin: TextKeys.action.login,
  authRegister: TextKeys.auth.register,
  landing: TextKeys.landing.title,
  setupPreferences: TextKeys.setup.preferencesTitle,
  tenantDashboard: TextKeys.tenant.dashboard.title,
  tenantPreferences: TextKeys.tenant.preferences.title,
  tenantProfile: TextKeys.tenant.profile.title,
} as const;
