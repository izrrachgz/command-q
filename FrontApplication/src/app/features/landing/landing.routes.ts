import { Routes } from '@angular/router';

import { AppRouteDataKeys, AppRouteSegments, AppRouteTitleKeys } from '../../core/routing/app-route-constants';

export const LANDING_ROUTES: Routes = [
  {
    path: AppRouteSegments.root,
    loadComponent: () => import('./pages/landing/landing.page').then((component) => component.LandingPage),
    data: { [AppRouteDataKeys.titleKey]: AppRouteTitleKeys.landing },
  },
];
