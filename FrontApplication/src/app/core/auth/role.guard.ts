import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

import { AuthFacade } from './auth.facade';
import { AppRouteDataKeys } from '../routing/app-route-constants';
import { AppRoutePaths } from '../routing/app-route-paths';

export const roleGuard: CanActivateFn = (route) => {
  const roles = (route.data[AppRouteDataKeys.roles] as readonly string[] | undefined) ?? [];
  const auth = inject(AuthFacade);
  const router = inject(Router);

  return roles.length === 0 || auth.hasAnyRole(roles) ? true : router.createUrlTree([AppRoutePaths.tenant.dashboard]);
};
