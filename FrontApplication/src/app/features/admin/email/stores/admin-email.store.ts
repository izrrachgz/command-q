import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

import { ApiSortFields } from '../../../../core/api/api-constants';
import { AdminEmailFilters, AdminEmailRecord } from '../models/admin-email.model';

export type AdminEmailStatus = 'idle' | 'loading' | 'ready' | 'backendUnavailable' | 'error';

export interface AdminEmailState {
  readonly status: AdminEmailStatus;
  readonly filters: AdminEmailFilters;
  readonly records: readonly AdminEmailRecord[];
  readonly total: number;
  readonly totalPages: number;
  readonly message: string;
}

const INITIAL_STATE: AdminEmailState = {
  status: 'idle',
  filters: { page: 1, size: 25, sortBy: ApiSortFields.createdUtc, ascendingSort: false },
  records: [],
  total: 0,
  totalPages: 0,
  message: '',
};

@Injectable({ providedIn: 'root' })
export class AdminEmailStore {
  private readonly state = new BehaviorSubject<AdminEmailState>(INITIAL_STATE);
  readonly state$ = this.state.asObservable();

  get snapshot(): AdminEmailState {
    return this.state.value;
  }

  patch(patch: Partial<AdminEmailState>): void {
    this.state.next({ ...this.snapshot, ...patch });
  }

  setFilters(filters: Partial<AdminEmailFilters>): void {
    this.patch({ filters: { ...this.snapshot.filters, ...filters } });
  }
}
