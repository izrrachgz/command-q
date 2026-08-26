import { Subject } from 'rxjs';

import { AccountModel } from '../models/account.model';
import { OperationResponse } from '../models/api-response';
import { AccountProvisionService } from './account-provision.service';

describe('AccountProvisionService', () => {
  it('shares concurrent provision requests', () => {
    const account: AccountModel = {
      id: 'account-1',
      username: 'tester',
      email: 'tester@example.com',
    };
    const loadExisting$ = new Subject<AccountModel | null>();
    const create$ = new Subject<OperationResponse>();
    const loadCreated$ = new Subject<AccountModel | null>();
    const auth = {
      snapshot: { account: null },
      loadApplicationAccount: vi
        .fn()
        .mockReturnValueOnce(loadExisting$.asObservable())
        .mockReturnValueOnce(loadCreated$.asObservable()),
      setApplicationAccount: vi.fn(),
    };
    const http = {
      post: vi.fn().mockReturnValue(create$.asObservable()),
    };
    const logger = {
      debug: vi.fn(),
      information: vi.fn(),
      warning: vi.fn(),
      error: vi.fn(),
    };
    const service = new AccountProvisionService(http as never, auth as never, logger as never);
    const results: Array<AccountModel | null> = [];

    service.ensureProvisioned().subscribe((value) => results.push(value));
    service.ensureProvisioned().subscribe((value) => results.push(value));

    expect(auth.loadApplicationAccount).toHaveBeenCalledTimes(1);

    loadExisting$.next(null);
    loadExisting$.complete();

    expect(http.post).toHaveBeenCalledTimes(1);

    create$.next({ message: 'created' });
    create$.complete();

    expect(auth.loadApplicationAccount).toHaveBeenCalledTimes(2);

    loadCreated$.next(account);
    loadCreated$.complete();

    expect(results).toEqual([account, account]);
    expect(auth.setApplicationAccount).toHaveBeenCalledWith(account);
  });
});
