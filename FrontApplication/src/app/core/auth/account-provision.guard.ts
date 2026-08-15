import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { map } from 'rxjs';

import { AccountProvisionService } from './account-provision.service';
import { AuthFacade, inferRealmFromUrl, sanitizeReturnUrl } from './auth.facade';
import { AppRouteQueryParams } from '../routing/app-route-constants';
import { AppRoutePaths } from '../routing/app-route-paths';

export const accountProvisionGuard: CanActivateFn = (_route, state) => {
  const auth = inject(AuthFacade);
  const provision = inject(AccountProvisionService);
  const router = inject(Router);
  const realm = inferRealmFromUrl(state.url);
  const returnUrl = sanitizeReturnUrl(state.url, realm);

  auth.setRealm(realm);

  if (auth.snapshot.account) {
    return true;
  }

  return provision.loadProvisionedAccount().pipe(
    map((account) =>
      account
        ? true
        : router.createUrlTree([AppRoutePaths.account.provision], {
            queryParams: { [AppRouteQueryParams.realm]: realm, [AppRouteQueryParams.returnUrl]: returnUrl },
          }),
    ),
  );
};
