import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

import { ApiSortFields } from '../../../../core/api/api-constants';
import { AdminRole, AdminRoleAccount, AdminRoleFilters } from '../models/admin-role.model';

export type AdminRolesStatus = 'idle' | 'loading' | 'ready' | 'backendUnavailable' | 'error';

export interface AdminRolesState {
  readonly status: AdminRolesStatus;
  readonly filters: AdminRoleFilters;
  readonly accounts: readonly AdminRoleAccount[];
  readonly roles: readonly AdminRole[];
  readonly selected: AdminRoleAccount | null;
  readonly selectedRoleNames: readonly string[];
  readonly total: number;
  readonly totalPages: number;
  readonly commandRunning: boolean;
  readonly message: string;
}

const INITIAL_STATE: AdminRolesState = {
  status: 'idle',
  filters: { page: 1, size: 25, sortBy: ApiSortFields.username, ascendingSort: true },
  accounts: [],
  roles: [],
  selected: null,
  selectedRoleNames: [],
  total: 0,
  totalPages: 0,
  commandRunning: false,
  message: '',
};

@Injectable({ providedIn: 'root' })
export class AdminRolesStore {
  private readonly state = new BehaviorSubject<AdminRolesState>(INITIAL_STATE);
  readonly state$ = this.state.asObservable();

  get snapshot(): AdminRolesState {
    return this.state.value;
  }

  patch(patch: Partial<AdminRolesState>): void {
    this.state.next({ ...this.snapshot, ...patch });
  }

  setFilters(filters: Partial<AdminRoleFilters>): void {
    this.patch({ filters: { ...this.snapshot.filters, ...filters } });
  }
}
