import { of, throwError } from 'rxjs';

import { AdminAccountsActions } from './admin-accounts.actions';
import { AdminAccount } from '../models/admin-account.model';
import { AdminAccountsStore } from '../stores/admin-accounts.store';

describe('AdminAccountsActions', () => {
  it('clears command running when a command succeeds but the refresh completes empty', () => {
    const account = createAccount();
    const store = new AdminAccountsStore();
    const service = {
      activate: vi.fn(),
      activity: vi.fn(),
      create: vi.fn(),
      details: vi.fn(),
      lock: vi.fn().mockReturnValue(of({ success: true, message: 'locked' })),
      search: vi.fn().mockReturnValue(throwError(() => new Error('backend unavailable'))),
    };
    const actions = new AdminAccountsActions(service as never, store);

    actions.lock(account).subscribe();

    expect(store.snapshot.commandRunning).toBe(false);
    expect(store.snapshot.status).toBe('backendUnavailable');
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
