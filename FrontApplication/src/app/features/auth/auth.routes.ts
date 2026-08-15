import { Routes } from '@angular/router';

import { AppRouteDataKeys, AppRouteSegments, AppRouteTitleKeys } from '../../core/routing/app-route-constants';

export const AUTH_ROUTES: Routes = [
  {
    path: AppRouteSegments.login,
    loadComponent: () => import('./pages/login/login.page').then((component) => component.LoginPage),
    data: { [AppRouteDataKeys.titleKey]: AppRouteTitleKeys.authLogin },
  },
  {
    path: AppRouteSegments.register,
    loadComponent: () => import('./pages/register/register.page').then((component) => component.RegisterPage),
    data: { [AppRouteDataKeys.titleKey]: AppRouteTitleKeys.authRegister },
  },
  {
    path: AppRouteSegments.callback,
    loadComponent: () => import('./pages/callback/callback.page').then((component) => component.CallbackPage),
    data: { [AppRouteDataKeys.titleKey]: AppRouteTitleKeys.authCallback },
  },
];
