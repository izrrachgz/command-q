import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

import { ApiSortFields } from '../../../../core/api/api-constants';
import {
  ApplicationConfigurationFilters,
  ApplicationConfigurationModel,
} from '../models/application-configuration.model';

export type ApplicationConfigurationsStatus = 'idle' | 'loading' | 'ready' | 'error';

export interface ApplicationConfigurationsState {
  readonly status: ApplicationConfigurationsStatus;
  readonly filters: ApplicationConfigurationFilters;
  readonly records: readonly ApplicationConfigurationModel[];
  readonly total: number;
  readonly totalPages: number;
  readonly message: string;
}

const InitialState: ApplicationConfigurationsState = {
  status: 'idle',
  filters: { page: 1, size: 25, sortBy: ApiSortFields.created, ascendingSort: false },
  records: [],
  total: 0,
  totalPages: 0,
  message: '',
};

@Injectable({ providedIn: 'root' })
export class ApplicationConfigurationsStore {
  private readonly state = new BehaviorSubject<ApplicationConfigurationsState>(InitialState);
  readonly state$ = this.state.asObservable();

  get snapshot(): ApplicationConfigurationsState {
    return this.state.value;
  }

  patch(patch: Partial<ApplicationConfigurationsState>): void {
    this.state.next({ ...this.snapshot, ...patch });
  }

  setFilters(filters: Partial<ApplicationConfigurationFilters>): void {
    this.patch({ filters: { ...this.snapshot.filters, ...filters } });
  }
}
