import { Routes } from '@angular/router';

import { AppRouteDataKeys, AppRouteSegments, AppRouteTitleKeys } from '../../core/routing/app-route-constants';

export const ACCOUNT_ROUTES: Routes = [
  {
    path: AppRouteSegments.provision,
    loadComponent: () => import('./pages/provision/provision.page').then((component) => component.ProvisionPage),
    data: { [AppRouteDataKeys.titleKey]: AppRouteTitleKeys.accountProvision },
  },
];
