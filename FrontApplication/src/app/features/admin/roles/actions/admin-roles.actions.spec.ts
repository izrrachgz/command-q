import { of } from 'rxjs';

import { AdminRolesActions } from './admin-roles.actions';
import { AdminRoleAccount } from '../models/admin-role.model';
import { AdminRolesStore } from '../stores/admin-roles.store';

describe('AdminRolesActions', () => {
  it('keeps the baseline user role when saving assignments', () => {
    const account = createAccount(['Admin']);
    const refreshedAccount = createAccount(['Admin', 'User']);
    const store = new AdminRolesStore();
    const service = {
      roles: vi.fn().mockReturnValue(of({
        success: true,
        message: '',
        detail: { roles: [{ id: 'role-user', name: 'User' }, { id: 'role-admin', name: 'Admin' }] },
      })),
      searchAccounts: vi.fn().mockReturnValue(of({
        collection: [refreshedAccount],
        page: 1,
        pageSize: 25,
        total: 1,
        totalPages: 1,
      })),
      updateAccountRoles: vi.fn().mockReturnValue(of({ success: true, message: 'updated' })),
    };
    const actions = new AdminRolesActions(service as never, store);

    actions.select(account);
    actions.save().subscribe();

    expect(service.updateAccountRoles).toHaveBeenCalledWith('account-1', { roles: ['Admin', 'User'] });
    expect(store.snapshot.commandRunning).toBe(false);
    expect(store.snapshot.selected?.roles).toEqual(['Admin', 'User']);
  });
});

const createAccount = (roles: readonly string[]): AdminRoleAccount => ({
  active: true,
  createdUtc: '2026-08-15T00:00:00Z',
  email: 'admin@example.com',
  id: 'account-1',
  locked: false,
  roles,
  username: 'admin',
});
