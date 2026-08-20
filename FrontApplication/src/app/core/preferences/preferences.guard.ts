import { inject } from '@angular/core';
import { CanActivateChildFn, CanActivateFn, Router } from '@angular/router';
import { map, of, switchMap } from 'rxjs';

import {
  AuthFacade,
  AuthRealm,
  AuthRealms,
  inferRealmFromUrl,
  sanitizeReturnUrl,
} from '../auth/auth.facade';
import { AppRouteQueryParams } from '../routing/app-route-constants';
import { AppRoutePaths } from '../routing/app-route-paths';
import { PreferencesService } from './preferences.service';

export const preferencesRequiredGuard: CanActivateChildFn = (route, state) => {
  return enforcePreferences(
    inject(AuthFacade),
    inject(PreferencesService),
    inject(Router),
    route.queryParamMap.get(AppRouteQueryParams.realm),
    state.url,
  );
};

export const publicPreferencesGuard: CanActivateChildFn = (_route, state) => {
  if (state.url.startsWith(AppRoutePaths.auth.root)) {
    return true;
  }

  const auth = inject(AuthFacade);
  const preferences = inject(PreferencesService);
  const router = inject(Router);

  return auth.checkAuth().pipe(
    switchMap((session) => {
      if (!session.authenticated) {
        return of(true);
      }

      if (!session.account) {
        return of(
          router.createUrlTree([AppRoutePaths.account.provision], {
            queryParams: {
              [AppRouteQueryParams.realm]: session.realm,
              [AppRouteQueryParams.returnUrl]: state.url,
            },
          }),
        );
      }

      return enforcePreferences(auth, preferences, router, session.realm, state.url);
    }),
  );
};

export const setupPreferencesGuard: CanActivateFn = (route) => {
  const auth = inject(AuthFacade);
  const preferences = inject(PreferencesService);
  const router = inject(Router);
  const realm = resolveRealm(
    route.queryParamMap.get(AppRouteQueryParams.realm),
    AppRoutePaths.setup.preferences,
  );
  const returnUrl = sanitizeReturnUrl(
    route.queryParamMap.get(AppRouteQueryParams.returnUrl),
    realm,
  );
  const accountKey = auth.snapshot.account?.id ?? auth.snapshot.user?.subject ?? '';

  auth.setRealm(realm);

  return preferences
    .activateAccount(accountKey)
    .pipe(map((result) => (result.configured ? router.parseUrl(returnUrl) : true)));
};

const enforcePreferences = (
  auth: AuthFacade,
  preferences: PreferencesService,
  router: Router,
  realmValue: string | null,
  url: string,
) => {
  const realm = resolveRealm(realmValue, url);
  const returnUrl = sanitizeReturnUrl(url, realm);
  const accountKey = auth.snapshot.account?.id ?? auth.snapshot.user?.subject ?? '';

  auth.setRealm(realm);

  return preferences.activateAccount(accountKey).pipe(
    map((result) =>
      result.configured
        ? true
        : router.createUrlTree([AppRoutePaths.setup.preferences], {
            queryParams: {
              [AppRouteQueryParams.realm]: realm,
              [AppRouteQueryParams.returnUrl]: returnUrl,
            },
          }),
    ),
  );
};

const resolveRealm = (realmValue: string | null, url: string): AuthRealm => {
  return realmValue === AuthRealms.admin ? AuthRealms.admin : inferRealmFromUrl(url);
};
