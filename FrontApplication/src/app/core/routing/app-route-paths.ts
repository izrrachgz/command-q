import { AppRouteSegments } from './app-route-constants';

const routePath = (...segments: readonly string[]): string => `/${segments.join('/')}`;

export const AppRoutePaths = {
  account: {
    provision: routePath(AppRouteSegments.account, AppRouteSegments.provision),
    root: routePath(AppRouteSegments.account),
  },
  admin: {
    accounts: routePath(AppRouteSegments.admin, AppRouteSegments.accounts),
    dashboard: routePath(AppRouteSegments.admin, AppRouteSegments.dashboard),
    email: routePath(AppRouteSegments.admin, AppRouteSegments.email),
    preferences: routePath(AppRouteSegments.admin, AppRouteSegments.preferences),
    roles: routePath(AppRouteSegments.admin, AppRouteSegments.roles),
    root: routePath(AppRouteSegments.admin),
  },
  auth: {
    callback: routePath(AppRouteSegments.auth, AppRouteSegments.callback),
    login: routePath(AppRouteSegments.auth, AppRouteSegments.login),
    register: routePath(AppRouteSegments.auth, AppRouteSegments.register),
    root: routePath(AppRouteSegments.auth),
  },
  tenant: {
    dashboard: routePath(AppRouteSegments.tenant, AppRouteSegments.dashboard),
    preferences: routePath(AppRouteSegments.tenant, AppRouteSegments.preferences),
    profile: routePath(AppRouteSegments.tenant, AppRouteSegments.profile),
    root: routePath(AppRouteSegments.tenant),
  },
} as const;
