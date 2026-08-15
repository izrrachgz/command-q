import { Injectable } from '@angular/core';
import { catchError, EMPTY, finalize, forkJoin, map, Observable, of, switchMap, tap } from 'rxjs';

import { AccountActivityModel } from '../../../../core/models/account.model';
import { detailPayload } from '../../../../core/models/api-response';
import { TextKeys } from '../../../../core/i18n/text-keys';
import { AdminAccount, CreateAdminAccountRequest } from '../models/admin-account.model';
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
          accounts: response.collection,
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
    this.store.patch({ selected: account, activity: [], commandRunning: true });

    return forkJoin({
      details: this.service.details(account.id).pipe(
        map((response) => detailPayload(response) ?? account),
        catchError(() => of(account)),
      ),
      activity: this.service.activity(account.id).pipe(
        map((response) => response.collection),
        catchError(() => of([] as readonly AccountActivityModel[])),
      ),
    }).pipe(
      tap(({ details, activity }) => this.store.patch({ selected: details, activity })),
      map(() => undefined),
      catchError(() => {
        return EMPTY;
      }),
      finalize(() => this.store.patch({ commandRunning: false })),
    );
  }

  lock(account: AdminAccount): Observable<void> {
    return this.runCommand(this.service.lock(account.id));
  }

  activate(account: AdminAccount): Observable<void> {
    return this.runCommand(this.service.activate(account.id));
  }

  create(request: CreateAdminAccountRequest): Observable<void> {
    return this.runCommand(this.service.create(request));
  }

  private runCommand(command$: Observable<unknown>): Observable<void> {
    this.store.patch({ commandRunning: true });

    return command$.pipe(
      switchMap(() => this.load()),
      catchError(() => {
        this.store.patch({ message: TextKeys.admin.accounts.backendPending });
        return EMPTY;
      }),
      finalize(() => this.store.patch({ commandRunning: false })),
    );
  }
}
