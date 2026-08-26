import { Injectable } from '@angular/core';
import { catchError, EMPTY, finalize, forkJoin, map, Observable, switchMap, tap } from 'rxjs';

import { TextKeys } from '../../../../core/i18n/text-keys';
import { AdminRoleAccount, AdminRoleNames } from '../models/admin-role.model';
import { AdminRolesService } from '../services/admin-roles.service';
import { AdminRolesStore } from '../stores/admin-roles.store';

@Injectable({ providedIn: 'root' })
export class AdminRolesActions {
  constructor(
    private readonly service: AdminRolesService,
    private readonly store: AdminRolesStore,
  ) {}

  load(): Observable<void> {
    this.store.patch({ status: 'loading', message: '' });

    return forkJoin({
      accounts: this.service.searchAccounts(this.store.snapshot.filters),
      roles: this.service.roles().pipe(map((response) => response.roles)),
    }).pipe(
      tap(({ accounts, roles }) =>
        this.store.patch({
          status: 'ready',
          accounts: accounts.items,
          roles,
          total: accounts.total,
          totalPages: accounts.totalPages,
        }),
      ),
      map(() => undefined),
      catchError(() => {
        this.store.patch({
          status: 'backendUnavailable',
          accounts: [],
          roles: [],
          selected: null,
          selectedRoleNames: [],
          total: 0,
          totalPages: 0,
          message: TextKeys.admin.roles.backendPending,
        });
        return EMPTY;
      }),
    );
  }

  applySearch(searchTerm: string): Observable<void> {
    this.store.setFilters({ page: 1, searchTerm: searchTerm || undefined });
    return this.load();
  }

  select(account: AdminRoleAccount): void {
    this.store.patch({
      selected: account,
      selectedRoleNames: ensureBaselineRole(account.roles),
      message: '',
    });
  }

  toggle(roleName: string, enabled: boolean): void {
    if (roleName === AdminRoleNames.user && !enabled) {
      return;
    }

    const current = new Set(this.store.snapshot.selectedRoleNames);

    if (enabled) {
      current.add(roleName);
    } else {
      current.delete(roleName);
    }

    this.store.patch({ selectedRoleNames: ensureBaselineRole([...current]) });
  }

  save(): Observable<void> {
    const selected = this.store.snapshot.selected;

    if (!selected) {
      this.store.patch({ message: TextKeys.admin.roles.selectAccount });
      return EMPTY;
    }

    this.store.patch({ commandRunning: true, message: '' });

    return this.service.updateAccountRoles(selected.id, { roles: ensureBaselineRole(this.store.snapshot.selectedRoleNames) }).pipe(
      switchMap(() => this.load()),
      tap(() => {
        const updated = this.store.snapshot.accounts.find((account) => account.id === selected.id) ?? null;
        if (updated) {
          this.select(updated);
        }
      }),
      map(() => undefined),
      catchError(() => {
        this.store.patch({ message: TextKeys.admin.roles.backendPending });
        return EMPTY;
      }),
      finalize(() => this.store.patch({ commandRunning: false })),
    );
  }
}

const ensureBaselineRole = (roles: readonly string[]): readonly string[] => {
  const roleSet = new Set(roles.filter((role) => role.trim().length > 0));
  roleSet.add(AdminRoleNames.user);

  return [...roleSet].sort((left, right) => left.localeCompare(right));
};
