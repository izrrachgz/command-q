import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { map } from 'rxjs';

import { AuthFacade, AuthRealms, inferRealmFromUrl, sanitizeReturnUrl } from './auth.facade';
import { AppRouteQueryParams } from '../routing/app-route-constants';
import { AppRoutePaths } from '../routing/app-route-paths';

export const authGuard: CanActivateFn = (route, state) => {
  const auth = inject(AuthFacade);
  const router = inject(Router);
  const realm = route.queryParamMap.get(AppRouteQueryParams.realm) === AuthRealms.admin
    ? AuthRealms.admin
    : inferRealmFromUrl(state.url);
  const returnUrl = sanitizeReturnUrl(state.url, realm);

  auth.setRealm(realm);

  return auth.checkAuth().pipe(
    map((session) => {
      if (session.authenticated) {
        return true;
      }

      return router.createUrlTree([AppRoutePaths.auth.login], {
        queryParams: { [AppRouteQueryParams.returnUrl]: returnUrl, [AppRouteQueryParams.realm]: realm },
      });
    }),
  );
};
