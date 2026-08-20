import { Routes } from '@angular/router';

import {
  AppRouteDataKeys,
  AppRouteSegments,
  AppRouteTitleKeys,
} from '../../core/routing/app-route-constants';

export const ADMIN_ROUTES: Routes = [
  {
    path: AppRouteSegments.dashboard,
    loadComponent: () =>
      import('./dashboard/pages/dashboard/admin-dashboard.page').then(
        (component) => component.AdminDashboardPage,
      ),
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
    path: AppRouteSegments.configurations,
    loadComponent: () =>
      import('./configurations/pages/configuration-history/configuration-history.page').then(
        (component) => component.ConfigurationHistoryPage,
      ),
    data: { [AppRouteDataKeys.titleKey]: AppRouteTitleKeys.adminConfigurations },
  },
  {
    path: AppRouteSegments.email,
    loadComponent: () =>
      import('./email/pages/email-management/email-management.page').then(
        (component) => component.EmailManagementPage,
      ),
    data: { [AppRouteDataKeys.titleKey]: AppRouteTitleKeys.adminEmail },
  },
  {
    path: AppRouteSegments.roles,
    loadComponent: () =>
      import('./roles/pages/role-management/role-management.page').then(
        (component) => component.RoleManagementPage,
      ),
    data: { [AppRouteDataKeys.titleKey]: AppRouteTitleKeys.adminRoles },
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
