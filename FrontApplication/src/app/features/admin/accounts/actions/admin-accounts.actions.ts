import { Injectable } from '@angular/core';
import { catchError, EMPTY, finalize, map, Observable, tap } from 'rxjs';

import { TextKeys } from '../../../../core/i18n/text-keys';
import { AdminAccount } from '../models/admin-account.model';
import { AdminAccountsService } from '../services/admin-accounts.service';
import { AdminAccountsStore } from '../stores/admin-accounts.store';

@Injectable({ providedIn: 'root' })
export class AdminAccountsActions {
  constructor(
    private readonly service: AdminAccountsService,
    private readonly store: AdminAccountsStore,
  ) {}

  load(): Observable<void> {
    this.store.patch({ status: 'loading', message: '' });

    return this.service.search(this.store.snapshot.filters).pipe(
      tap((response) =>
        this.store.patch({
          status: 'ready',
          accounts: response.items,
          total: response.total,
          totalPages: response.totalPages,
        }),
      ),
      map(() => undefined),
      catchError(() => {
        this.store.patch({
          status: 'backendUnavailable',
          accounts: [],
          total: 0,
          totalPages: 0,
          message: TextKeys.admin.accounts.backendPending,
        });
        return EMPTY;
      }),
    );
  }

  applySearch(searchTerm: string): Observable<void> {
    this.store.setFilters({ page: 1, searchTerm: searchTerm || undefined });
    return this.load();
  }

  select(account: AdminAccount): Observable<void> {
    this.store.patch({ selected: account, commandRunning: true });

    return this.service.details(account.id).pipe(
      tap((details) => this.store.patch({ selected: details })),
      map(() => undefined),
      catchError(() => EMPTY),
      finalize(() => this.store.patch({ commandRunning: false })),
    );
  }
}
