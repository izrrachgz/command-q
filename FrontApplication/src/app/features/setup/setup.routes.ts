import { Routes } from '@angular/router';

import { setupPreferencesGuard } from '../../core/preferences/preferences.guard';
import {
  AppRouteDataKeys,
  AppRouteSegments,
  AppRouteTitleKeys,
} from '../../core/routing/app-route-constants';

export const SETUP_ROUTES: Routes = [
  {
    path: AppRouteSegments.preferences,
    canActivate: [setupPreferencesGuard],
    loadComponent: () =>
      import('./pages/preferences/setup-preferences.page').then(
        (component) => component.SetupPreferencesPage,
      ),
    data: { [AppRouteDataKeys.titleKey]: AppRouteTitleKeys.setupPreferences },
  },
  { path: AppRouteSegments.root, pathMatch: 'full', redirectTo: AppRouteSegments.preferences },
];
