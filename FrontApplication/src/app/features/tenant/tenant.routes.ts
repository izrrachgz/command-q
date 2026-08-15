import { Routes } from '@angular/router';

import { AppRouteDataKeys, AppRouteSegments, AppRouteTitleKeys } from '../../core/routing/app-route-constants';

export const TENANT_ROUTES: Routes = [
  {
    path: AppRouteSegments.dashboard,
    loadComponent: () =>
      import('./dashboard/pages/dashboard/tenant-dashboard.page').then((component) => component.TenantDashboardPage),
    data: { [AppRouteDataKeys.titleKey]: AppRouteTitleKeys.tenantDashboard },
  },
  {
    path: AppRouteSegments.profile,
    loadComponent: () =>
      import('./profile/pages/profile/tenant-profile.page').then((component) => component.TenantProfilePage),
    data: { [AppRouteDataKeys.titleKey]: AppRouteTitleKeys.tenantProfile },
  },
  {
    path: AppRouteSegments.preferences,
    loadComponent: () =>
      import('./preferences/pages/preferences/tenant-preferences.page').then(
        (component) => component.TenantPreferencesPage,
      ),
    data: { [AppRouteDataKeys.titleKey]: AppRouteTitleKeys.tenantPreferences },
  },
  { path: AppRouteSegments.root, pathMatch: 'full', redirectTo: AppRouteSegments.dashboard },
];
