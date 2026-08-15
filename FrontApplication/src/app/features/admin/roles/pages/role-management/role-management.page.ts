import { AsyncPipe, DatePipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { LucideDynamicIcon } from '@lucide/angular';

import { AuthFacade } from '../../../../../core/auth/auth.facade';
import { TextPipe } from '../../../../../core/i18n/text.pipe';
import { EmptyStateComponent, PageHeaderComponent } from '../../../../../shared/components';
import { TextInputComponent } from '../../../../../shared/forms';
import { AdminRoleAccount, AdminRoleNames } from '../../models/admin-role.model';
import { AdminRolesActions } from '../../actions/admin-roles.actions';
import { AdminRolesState, AdminRolesStore } from '../../stores/admin-roles.store';

@Component({
  selector: 'app-role-management-page',
  standalone: true,
  imports: [
    AsyncPipe,
    DatePipe,
    EmptyStateComponent,
    LucideDynamicIcon,
    PageHeaderComponent,
    ReactiveFormsModule,
    TextInputComponent,
    TextPipe,
  ],
  templateUrl: './role-management.page.html',
})
export class RoleManagementPage {
  readonly store = inject(AdminRolesStore);

  private readonly auth = inject(AuthFacade);
  private readonly actions = inject(AdminRolesActions);
  private readonly fb = inject(FormBuilder);

  readonly filterForm = this.fb.nonNullable.group({
    searchTerm: [''],
  });

  constructor() {
    this.load();
  }

  load(): void {
    this.actions.load().subscribe();
  }

  search(): void {
    this.actions.applySearch(this.filterForm.controls.searchTerm.value).subscribe();
  }

  select(account: AdminRoleAccount): void {
    this.actions.select(account);
  }

  save(commandRunning: boolean): void {
    if (commandRunning) {
      return;
    }

    this.actions.save().subscribe();
  }

  isRoleSelected(state: AdminRolesState, roleName: string): boolean {
    return state.selectedRoleNames.some((selected) => selected.toLowerCase() === roleName.toLowerCase());
  }

  isBaselineRole(roleName: string): boolean {
    return roleName.toLowerCase() === AdminRoleNames.user.toLowerCase();
  }

  isRoleLocked(state: AdminRolesState, roleName: string): boolean {
    return this.isBaselineRole(roleName) || this.isSelectedSelfAdminRole(state, roleName);
  }

  toggleRole(roleName: string, event: Event): void {
    this.actions.toggle(roleName, (event.target as HTMLInputElement).checked);
  }

  private isSelectedSelfAdminRole(state: AdminRolesState, roleName: string): boolean {
    const currentSubject = this.auth.snapshot.user?.subject;

    return roleName.toLowerCase() === AdminRoleNames.admin.toLowerCase() &&
      !!currentSubject &&
      state.selected?.id.toLowerCase() === currentSubject.toLowerCase() &&
      this.isRoleSelected(state, roleName);
  }
}
