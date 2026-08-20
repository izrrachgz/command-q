import { AuthRealms, inferRealmFromUrl, sanitizeReturnUrl } from './auth.facade';
import { AppRouteQueryParams } from '../routing/app-route-constants';
import { AppRoutePaths } from '../routing/app-route-paths';

describe('auth return url helpers', () => {
  it('uses the admin realm for admin routes', () => {
    expect(inferRealmFromUrl(AppRoutePaths.admin.dashboard)).toBe(AuthRealms.admin);
  });

  it('does not allow auth routes as post-login return urls', () => {
    expect(sanitizeReturnUrl(AppRoutePaths.auth.login, AuthRealms.tenant)).toBe(
      AppRoutePaths.tenant.dashboard,
    );
    expect(sanitizeReturnUrl(`${AppRoutePaths.auth.callback}?code=123`, AuthRealms.admin)).toBe(
      AppRoutePaths.admin.dashboard,
    );
  });

  it('uses the realm dashboard when the public landing page is supplied', () => {
    expect(sanitizeReturnUrl('/', AuthRealms.tenant)).toBe(AppRoutePaths.tenant.dashboard);
    expect(sanitizeReturnUrl('/?source=setup', AuthRealms.admin)).toBe(
      AppRoutePaths.admin.dashboard,
    );
  });

  it('does not allow account provisioning as a post-login return url', () => {
    expect(
      sanitizeReturnUrl(
        `${AppRoutePaths.account.provision}?${AppRouteQueryParams.realm}=${AuthRealms.admin}`,
        AuthRealms.admin,
      ),
    ).toBe(AppRoutePaths.admin.dashboard);
  });

  it('does not allow setup pages to become post-login return urls', () => {
    expect(sanitizeReturnUrl(AppRoutePaths.setup.preferences, AuthRealms.admin)).toBe(
      AppRoutePaths.admin.dashboard,
    );
  });

  it('keeps protected application routes as post-login return urls', () => {
    expect(sanitizeReturnUrl(AppRoutePaths.tenant.preferences, AuthRealms.tenant)).toBe(
      AppRoutePaths.tenant.preferences,
    );
    expect(sanitizeReturnUrl(`${AppRoutePaths.admin.accounts}?page=2`, AuthRealms.admin)).toBe(
      `${AppRoutePaths.admin.accounts}?page=2`,
    );
  });
});
