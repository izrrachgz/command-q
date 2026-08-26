import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import {
  BehaviorSubject,
  catchError,
  finalize,
  firstValueFrom,
  map,
  Observable,
  of,
  shareReplay,
  switchMap,
  throwError,
} from 'rxjs';

import { environment } from '../../../environments/environment';
import { FrontendStorageKeys } from '../config/frontend-storage-keys';
import { combineUrl } from '../config/url-builder';
import { AuthDiagnosticsService } from './auth-diagnostics.service';
import {
  AccountModel,
  IdentityAccountModel,
  RegisterAccountRequest,
} from '../models/account.model';
import { PreferencesService } from '../preferences/preferences.service';
import { AppRouteAreas } from '../routing/app-route-constants';
import { AppRoutePaths } from '../routing/app-route-paths';

export const AuthRealms = AppRouteAreas;

export type AuthRealm = (typeof AuthRealms)[keyof typeof AuthRealms];

export interface AuthUser {
  readonly subject: string;
  readonly username: string;
  readonly email: string;
  readonly roles: readonly string[];
}

export interface AuthSession {
  readonly initialized: boolean;
  readonly authenticated: boolean;
  readonly realm: AuthRealm;
  readonly user: AuthUser | null;
  readonly account: AccountModel | null;
}

const DEFAULT_SESSION: AuthSession = {
  initialized: false,
  authenticated: false,
  realm: AuthRealms.tenant,
  user: null,
  account: null,
};
const AUTH_PREFIX = `${AppRoutePaths.auth.root}/`;
const SETUP_PREFIX = `${AppRoutePaths.setup.root}/`;

@Injectable({ providedIn: 'root' })
export class AuthFacade {
  private readonly session = new BehaviorSubject<AuthSession>(DEFAULT_SESSION);
  private checkAuthRequest$: Observable<AuthSession> | null = null;
  readonly session$ = this.session.asObservable();
  readonly authenticated$ = this.session$.pipe(map((value) => value.authenticated));
  readonly account$ = this.session$.pipe(map((value) => value.account));

  constructor(
    private readonly http: HttpClient,
    private readonly oidc: OidcSecurityService,
    private readonly preferences: PreferencesService,
    private readonly router: Router,
    private readonly diagnostics: AuthDiagnosticsService,
  ) {}

  get snapshot(): AuthSession {
    return this.session.value;
  }

  initialize(): Promise<void> {
    if (isAuthCallbackUrl()) {
      this.session.next({ ...this.snapshot, initialized: true });
      return Promise.resolve();
    }

    return firstValueFrom(this.checkAuth().pipe(map(() => undefined)));
  }

  checkAuth(url?: string): Observable<AuthSession> {
    if (!url && this.snapshot.initialized) {
      return of(this.snapshot);
    }

    if (!url && this.checkAuthRequest$) {
      return this.checkAuthRequest$;
    }

    const request$ = this.oidc.checkAuth(url).pipe(
      switchMap((result) => {
        if (!result.isAuthenticated) {
          const next = { ...DEFAULT_SESSION, initialized: true, realm: this.snapshot.realm };
          this.session.next(next);
          return of(next);
        }

        const user = mapUserData(result.userData);
        const next = {
          ...this.snapshot,
          initialized: true,
          authenticated: true,
          user,
        };
        this.session.next(next);

        return this.loadApplicationAccount().pipe(
          map((account) => {
            const withAccount = { ...next, account };
            this.session.next(withAccount);
            return withAccount;
          }),
          catchError(() => of(next)),
        );
      }),
      catchError((error) => {
        this.diagnostics.log('checkAuth', error);
        const next = { ...DEFAULT_SESSION, initialized: true };
        this.session.next(next);
        return of(next);
      }),
      finalize(() => {
        if (!url) {
          this.checkAuthRequest$ = null;
        }
      }),
      shareReplay({ bufferSize: 1, refCount: false }),
    );

    if (!url) {
      this.checkAuthRequest$ = request$;
    }

    return request$;
  }

  login(realm: AuthRealm, returnUrl: string): Observable<void> {
    this.setRealm(realm);
    sessionStorage.setItem(FrontendStorageKeys.authReturnUrl, sanitizeReturnUrl(returnUrl, realm));
    sessionStorage.setItem(FrontendStorageKeys.authRealm, realm);

    try {
      this.oidc.authorize();
      return of(undefined);
    } catch (error) {
      this.diagnostics.log('authorize', error);
      return throwError(() => error);
    }
  }

  register(request: RegisterAccountRequest): Observable<IdentityAccountModel> {
    return this.http
      .post<IdentityAccountModel>(
        combineUrl(environment.identity.authority, environment.identity.endpoints.register),
        request,
      )
      .pipe(
        catchError((error) => {
          this.diagnostics.log('register', error);
          return throwError(() => error);
        }),
      );
  }

  finishCallback(): Observable<AuthSession> {
    const realm =
      (sessionStorage.getItem(FrontendStorageKeys.authRealm) as AuthRealm | null) ??
      AuthRealms.tenant;
    this.setRealm(realm);

    return this.checkAuth(globalThis.location?.href);
  }

  resolveReturnUrl(): string {
    const realm = this.snapshot.realm;
    const stored = sessionStorage.getItem(FrontendStorageKeys.authReturnUrl);
    sessionStorage.removeItem(FrontendStorageKeys.authReturnUrl);
    sessionStorage.removeItem(FrontendStorageKeys.authRealm);

    return sanitizeReturnUrl(stored, realm);
  }

  getDefaultReturnUrl(realm: AuthRealm = this.snapshot.realm): string {
    return defaultReturnUrl(realm);
  }

  setRealm(realm: AuthRealm): void {
    this.session.next({ ...this.snapshot, realm });
  }

  hasAnyRole(roles: readonly string[]): boolean {
    const actualRoles = this.snapshot.user?.roles ?? [];
    return roles.some((role) =>
      actualRoles.some((actual) => actual.toLowerCase() === role.toLowerCase()),
    );
  }

  loadApplicationAccount(): Observable<AccountModel | null> {
    return this.http
      .get<AccountModel>(
        combineUrl(environment.api.baseUrl, environment.api.endpoints.account.me),
      )
      .pipe(map((account) => account ?? null));
  }

  setApplicationAccount(account: AccountModel | null): void {
    this.session.next({ ...this.snapshot, account });
  }

  logout(): void {
    this.preferences.resetSession();
    this.session.next(DEFAULT_SESSION);
    this.oidc.logoffAndRevokeTokens().subscribe({
      next: () => undefined,
      error: () => this.router.navigateByUrl('/'),
    });
  }
}

const mapUserData = (userData: unknown): AuthUser => {
  const data = (Array.isArray(userData) ? userData[0] : userData) as
    Record<string, unknown> | null | undefined;
  const roleClaim = data?.['role'] ?? data?.['roles'];
  const roles = Array.isArray(roleClaim)
    ? roleClaim.map((role) => String(role))
    : roleClaim
      ? [String(roleClaim)]
      : [];

  return {
    subject: String(data?.['sub'] ?? ''),
    username: String(data?.['name'] ?? data?.['preferred_username'] ?? ''),
    email: String(data?.['email'] ?? ''),
    roles,
  };
};

const isAuthCallbackUrl = (): boolean => {
  return globalThis.location?.pathname?.toLowerCase() === AppRoutePaths.auth.callback;
};

export const inferRealmFromUrl = (url: string): AuthRealm => {
  return url.toLowerCase().startsWith(AppRoutePaths.admin.root)
    ? AuthRealms.admin
    : AuthRealms.tenant;
};

export const sanitizeReturnUrl = (url: string | null | undefined, realm: AuthRealm): string => {
  if (!url || !url.startsWith('/') || url.startsWith('//')) {
    return defaultReturnUrl(realm);
  }

  const normalizedPath = url.split(/[?#]/, 1)[0].toLowerCase();

  if (normalizedPath === '/') {
    return defaultReturnUrl(realm);
  }

  if (normalizedPath === AppRoutePaths.auth.root || normalizedPath.startsWith(AUTH_PREFIX)) {
    return defaultReturnUrl(realm);
  }

  if (normalizedPath === AppRoutePaths.account.provision) {
    return defaultReturnUrl(realm);
  }

  if (normalizedPath === AppRoutePaths.setup.root || normalizedPath.startsWith(SETUP_PREFIX)) {
    return defaultReturnUrl(realm);
  }

  return url;
};

const defaultReturnUrl = (realm: AuthRealm): string => {
  return realm === AuthRealms.admin
    ? AppRoutePaths.admin.dashboard
    : AppRoutePaths.tenant.dashboard;
};
