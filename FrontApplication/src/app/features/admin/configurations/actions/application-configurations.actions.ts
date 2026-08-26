import { Injectable } from '@angular/core';
import { catchError, EMPTY, map, Observable, tap } from 'rxjs';

import { TextKeys } from '../../../../core/i18n/text-keys';
import { ApplicationConfigurationsService } from '../services/application-configurations.service';
import { ApplicationConfigurationsStore } from '../stores/application-configurations.store';

@Injectable({ providedIn: 'root' })
export class ApplicationConfigurationsActions {
  constructor(
    private readonly service: ApplicationConfigurationsService,
    private readonly store: ApplicationConfigurationsStore,
  ) {}

  load(searchTerm?: string): Observable<void> {
    if (searchTerm !== undefined) {
      this.store.setFilters({ page: 1, searchTerm: searchTerm || undefined });
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
          status: 'error',
          records: [],
          total: 0,
          totalPages: 0,
          message: TextKeys.admin.configurations.loadFailed,
        });
        return EMPTY;
      }),
    );
  }
}
