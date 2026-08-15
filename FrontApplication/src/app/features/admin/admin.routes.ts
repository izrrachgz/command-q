import { Routes } from '@angular/router';

import { AppRouteDataKeys, AppRouteSegments, AppRouteTitleKeys } from '../../core/routing/app-route-constants';

export const ADMIN_ROUTES: Routes = [
  {
    path: AppRouteSegments.dashboard,
    loadComponent: () =>
      import('./dashboard/pages/dashboard/admin-dashboard.page').then((component) => component.AdminDashboardPage),
    data: { [AppRouteDataKeys.titleKey]: AppRouteTitleKeys.adminDashboard },
  },
  {
    path: AppRouteSegments.accounts,
    loadComponent: () =>
      import('./accounts/pages/account-management/account-management.page').then(
        (component) => component.AccountManagementPage,
      ),
    data: { [AppRouteDataKeys.titleKey]: AppRouteTitleKeys.adminAccounts },
  },
  {
    path: AppRouteSegments.email,
    loadComponent: () =>
      import('./email/pages/email-management/email-management.page').then((component) => component.EmailManagementPage),
    data: { [AppRouteDataKeys.titleKey]: AppRouteTitleKeys.adminEmail },
  },
  {
    path: AppRouteSegments.preferences,
    loadComponent: () =>
      import('./preferences/pages/preferences/admin-preferences.page').then(
        (component) => component.AdminPreferencesPage,
      ),
    data: { [AppRouteDataKeys.titleKey]: AppRouteTitleKeys.adminPreferences },
  },
  { path: AppRouteSegments.root, pathMatch: 'full', redirectTo: AppRouteSegments.dashboard },
];
