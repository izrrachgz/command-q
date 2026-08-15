import { TestBed } from '@angular/core/testing';

import { AdminAccountsStore } from './admin-accounts.store';

describe('AdminAccountsStore', () => {
  it('updates filters without dropping the current paging defaults', () => {
    TestBed.configureTestingModule({});
    const store = TestBed.inject(AdminAccountsStore);

    store.setFilters({ searchTerm: 'alice' });

    expect(store.snapshot.filters.page).toBe(1);
    expect(store.snapshot.filters.searchTerm).toBe('alice');
  });
});
