import { Routes } from '@angular/router';

import { AuthGuard } from '@core/auth';

export const APP_ROUTES: Routes = [
  {
    path: 'auth',
    loadChildren: () => import('@features/auth').then((m) => m.AuthModule),
  },

  {
    path: 'dashboard',
    canLoad: [AuthGuard],
    canActivate: [AuthGuard],
    loadChildren: () => import('@features/dashboard').then((m) => m.DashboardModule),
  },

  {
    path: '',
    redirectTo: 'dashboard',
    pathMatch: 'full',
  },

  {
    path: '**',
    redirectTo: 'dashboard',
  },
];
