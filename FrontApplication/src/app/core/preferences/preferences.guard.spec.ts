import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router, Routes } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { of } from 'rxjs';

import { AuthFacade, AuthRealms } from '../auth/auth.facade';
import type { AuthRealm } from '../auth/auth.facade';
import { AppRoutePaths } from '../routing/app-route-paths';
import { PreferencesService } from './preferences.service';
import {
  preferencesRequiredGuard,
  publicPreferencesGuard,
  setupPreferencesGuard,
} from './preferences.guard';

@Component({ standalone: true, template: '' })
class RouteTargetComponent {}

const TestRoutes: Routes = [
  {
    path: 'setup/preferences',
    canActivate: [setupPreferencesGuard],
    component: RouteTargetComponent,
  },
  {
    path: 'tenant',
    canActivateChild: [preferencesRequiredGuard],
    children: [{ path: 'dashboard', component: RouteTargetComponent }],
  },
  {
    path: 'admin',
    canActivateChild: [preferencesRequiredGuard],
    children: [{ path: 'dashboard', component: RouteTargetComponent }],
  },
  {
    path: '',
    canActivateChild: [publicPreferencesGuard],
    children: [{ path: '', pathMatch: 'full', component: RouteTargetComponent }],
  },
];

describe('preferences setup routing', () => {
  it.each([
    [AuthRealms.tenant, AppRoutePaths.tenant.dashboard],
    [AuthRealms.admin, AppRoutePaths.admin.dashboard],
  ] as const)(
    'returns a configured %s account to its authenticated dashboard after a public URL rewrite',
    async (realm, expectedDashboard) => {
      let configured = false;
      const auth = createAuth(realm);
      const preferences = {
        activateAccount: vi.fn(() =>
          of({
            configured,
            backendUnavailable: false,
            preferences: {},
          }),
        ),
      };
      TestBed.configureTestingModule({
        providers: [
          provideRouter(TestRoutes),
          { provide: AuthFacade, useValue: auth },
          { provide: PreferencesService, useValue: preferences },
        ],
      });
      const harness = await RouterTestingHarness.create();
      const router = TestBed.inject(Router);

      await harness.navigateByUrl('/');

      expect(router.url).toContain(AppRoutePaths.setup.preferences);
      expect(decodeURIComponent(router.url)).toContain(`returnUrl=${expectedDashboard}`);

      configured = true;
      await harness.navigateByUrl(expectedDashboard);

      expect(router.url).toBe(expectedDashboard);
    },
  );

  it('redirects a direct tenant-route bypass to setup and preserves the protected target', async () => {
    const auth = createAuth(AuthRealms.tenant);
    const preferences = {
      activateAccount: vi.fn(() =>
        of({ configured: false, backendUnavailable: false, preferences: {} }),
      ),
    };
    TestBed.configureTestingModule({
      providers: [
        provideRouter(TestRoutes),
        { provide: AuthFacade, useValue: auth },
        { provide: PreferencesService, useValue: preferences },
      ],
    });
    const harness = await RouterTestingHarness.create();
    const router = TestBed.inject(Router);

    await harness.navigateByUrl(AppRoutePaths.tenant.dashboard);

    expect(router.url).toContain(AppRoutePaths.setup.preferences);
    expect(decodeURIComponent(router.url)).toContain(`returnUrl=${AppRoutePaths.tenant.dashboard}`);
  });
});

const createAuth = (realm: AuthRealm) => {
  const session = {
    initialized: true,
    authenticated: true,
    realm,
    user: {
      subject: 'subject-1',
      username: 'user',
      email: 'user@example.com',
      roles: realm === AuthRealms.admin ? ['Admin'] : ['User'],
    },
    account: {
      id: 'account-1',
      username: 'user',
      email: 'user@example.com',
      active: true,
      created: '2026-08-20T08:00:00Z',
      modified: '2026-08-20T08:00:00Z',
    },
  };

  return {
    snapshot: session,
    checkAuth: vi.fn(() => of(session)),
    setRealm: vi.fn(),
  };
};
