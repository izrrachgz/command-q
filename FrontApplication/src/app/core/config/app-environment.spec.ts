import { createApiEndpoints } from './app-environment';

describe('createApiEndpoints', () => {
  it('matches the current lowercase resource-oriented API routes', () => {
    const endpoints = createApiEndpoints();

    expect(endpoints.account.create).toBe('accounts');
    expect(endpoints.account.me).toBe('accounts/me');
    expect(endpoints.account.activity).toBe('accounts/me/activity');
    expect(endpoints.account.preferences).toBe('accounts/me/preferences');
    expect(endpoints.admin.accounts).toBe('admin/accounts');
    expect(endpoints.admin.accountDetails('account/id')).toBe('admin/accounts/account%2Fid');
    expect(endpoints.admin.accountRoles('account/id')).toBe('admin/accounts/account%2Fid/roles');
    expect(endpoints.admin.configurations).toBe('admin/configurations');
    expect(endpoints.admin.roles).toBe('admin/roles');
  });
});
