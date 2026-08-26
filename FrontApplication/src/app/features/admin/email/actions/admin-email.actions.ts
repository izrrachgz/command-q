import { Injectable } from '@angular/core';
import { catchError, EMPTY, map, Observable, tap } from 'rxjs';

import { TextKeys } from '../../../../core/i18n/text-keys';
import { AdminEmailService } from '../services/admin-email.service';
import { AdminEmailFilters } from '../models/admin-email.model';
import { AdminEmailStore } from '../stores/admin-email.store';

@Injectable({ providedIn: 'root' })
export class AdminEmailActions {
  constructor(
    private readonly service: AdminEmailService,
    private readonly store: AdminEmailStore,
  ) {}

  load(filters?: Partial<AdminEmailFilters>): Observable<void> {
    if (filters) {
      this.store.setFilters({ ...filters, page: 1 });
    }

    this.store.patch({ status: 'loading', message: '' });

    return this.service.search(this.store.snapshot.filters).pipe(
      tap((response) =>
        this.store.patch({
          status: 'ready',
          records: response.items,
          total: response.total,
          totalPages: response.totalPages,
        }),
      ),
      map(() => undefined),
      catchError(() => {
        this.store.patch({
          status: 'backendUnavailable',
          records: [],
          total: 0,
          totalPages: 0,
          message: TextKeys.admin.email.pendingBackend,
        });
        return EMPTY;
      }),
    );
  }
}
