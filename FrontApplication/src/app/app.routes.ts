import { Routes } from '@angular/router';

import { accountProvisionGuard } from './core/auth/account-provision.guard';
import { authGuard } from './core/auth/auth.guard';
import { AppRoles } from './core/auth/app-roles';
import { roleGuard } from './core/auth/role.guard';
import { AppRouteAreas, AppRouteDataKeys, AppRouteSegments } from './core/routing/app-route-constants';
import { AppShellComponent } from './layout/app-shell/app-shell.component';
import { PublicShellComponent } from './layout/public-shell/public-shell.component';

export const APP_ROUTES: Routes = [
  {
    path: AppRouteSegments.root,
    component: PublicShellComponent,
    children: [
      {
        path: AppRouteSegments.root,
        loadChildren: () => import('./features/landing/landing.module').then((module) => module.LandingModule),
      },
      {
        path: AppRouteSegments.auth,
        loadChildren: () => import('./features/auth/auth.module').then((module) => module.AuthModule),
      },
    ],
  },
  {
    path: AppRouteSegments.account,
    canActivate: [authGuard],
    component: AppShellComponent,
    data: { [AppRouteDataKeys.area]: AppRouteAreas.tenant },
    loadChildren: () => import('./features/account/account.module').then((module) => module.AccountModule),
  },
  {
    path: AppRouteSegments.tenant,
    canActivate: [authGuard],
    canActivateChild: [accountProvisionGuard],
    component: AppShellComponent,
    data: { [AppRouteDataKeys.area]: AppRouteAreas.tenant },
    loadChildren: () => import('./features/tenant/tenant.module').then((module) => module.TenantModule),
  },
  {
    path: AppRouteSegments.admin,
    canActivate: [authGuard, roleGuard],
    canActivateChild: [accountProvisionGuard],
    component: AppShellComponent,
    data: { [AppRouteDataKeys.area]: AppRouteAreas.admin, [AppRouteDataKeys.roles]: [AppRoles.admin] },
    loadChildren: () => import('./features/admin/admin.module').then((module) => module.AdminModule),
  },
  { path: AppRouteSegments.wildcard, redirectTo: AppRouteSegments.root },
];
