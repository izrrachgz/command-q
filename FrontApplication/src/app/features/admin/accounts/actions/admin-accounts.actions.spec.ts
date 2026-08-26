import { of } from 'rxjs';

import { AdminAccountsActions } from './admin-accounts.actions';
import { AdminAccount } from '../models/admin-account.model';
import { AdminAccountsStore } from '../stores/admin-accounts.store';

describe('AdminAccountsActions', () => {
  it('loads supported account details without calling removed command endpoints', () => {
    const account = createAccount();
    const details = { ...account, roles: ['Admin', 'User'] };
    const store = new AdminAccountsStore();
    const service = {
      details: vi.fn().mockReturnValue(of(details)),
      search: vi.fn(),
    };
    const actions = new AdminAccountsActions(service as never, store);

    actions.select(account).subscribe();

    expect(service.details).toHaveBeenCalledWith(account.id);
    expect(store.snapshot.commandRunning).toBe(false);
    expect(store.snapshot.selected).toEqual(details);
  });
});

const createAccount = (): AdminAccount => ({
  active: true,
  email: 'admin@example.com',
  id: 'account-1',
  locked: false,
  roles: ['Admin'],
  username: 'admin',
});
