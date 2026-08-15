import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

import { ApiSortFields } from '../../../../core/api/api-constants';
import { AccountActivityModel } from '../../../../core/models/account.model';
import { AdminAccount, AdminAccountFilters } from '../models/admin-account.model';

export type AdminAccountsStatus = 'idle' | 'loading' | 'ready' | 'backendUnavailable' | 'error';

export interface AdminAccountsState {
  readonly status: AdminAccountsStatus;
  readonly filters: AdminAccountFilters;
  readonly accounts: readonly AdminAccount[];
  readonly total: number;
  readonly totalPages: number;
  readonly selected: AdminAccount | null;
  readonly activity: readonly AccountActivityModel[];
  readonly commandRunning: boolean;
  readonly message: string;
}

const INITIAL_STATE: AdminAccountsState = {
  status: 'idle',
  filters: { page: 1, size: 25, sortBy: ApiSortFields.username, ascendingSort: true },
  accounts: [],
  total: 0,
  totalPages: 0,
  selected: null,
  activity: [],
  commandRunning: false,
  message: '',
};

@Injectable({ providedIn: 'root' })
export class AdminAccountsStore {
  private readonly state = new BehaviorSubject<AdminAccountsState>(INITIAL_STATE);
  readonly state$ = this.state.asObservable();

  get snapshot(): AdminAccountsState {
    return this.state.value;
  }

  patch(patch: Partial<AdminAccountsState>): void {
    this.state.next({ ...this.snapshot, ...patch });
  }

  setFilters(filters: Partial<AdminAccountFilters>): void {
    this.patch({ filters: { ...this.snapshot.filters, ...filters } });
  }
}
