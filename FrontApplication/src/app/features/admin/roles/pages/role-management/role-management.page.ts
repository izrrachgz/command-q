import { AsyncPipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { ScrollingModule } from '@angular/cdk/scrolling';
import { LucideDynamicIcon } from '@lucide/angular';
import { Observable } from 'rxjs';

import { AuthFacade } from '../../../../../core/auth/auth.facade';
import { TextPipe } from '../../../../../core/i18n/text.pipe';
import { AccountDatePipe } from '../../../../../core/time/account-date.pipe';
import {
  ConfirmationDialogComponent,
  EmptyStateComponent,
  PageHeaderComponent,
} from '../../../../../shared/components';
import { TextInputComponent } from '../../../../../shared/forms';
import {
  trackByEntityId,
  virtualTableBufferPixels,
  virtualTableViewportHeight,
  VirtualTableRowHeight,
} from '../../../../../shared/tables';
import { AdminRoleAccount, AdminRoleNames } from '../../models/admin-role.model';
import { AdminRolesActions } from '../../actions/admin-roles.actions';
import { AdminRolesStore } from '../../stores/admin-roles.store';
import {
  AdminRoleChange,
  RoleEditorComponent,
} from '../../components/role-editor/role-editor.component';

@Component({
  selector: 'app-role-management-page',
  standalone: true,
  imports: [
    AsyncPipe,
    AccountDatePipe,
    ConfirmationDialogComponent,
    EmptyStateComponent,
    LucideDynamicIcon,
    PageHeaderComponent,
    ReactiveFormsModule,
    RoleEditorComponent,
    ScrollingModule,
    TextInputComponent,
    TextPipe,
  ],
  templateUrl: './role-management.page.html',
})
export class RoleManagementPage {
  readonly store = inject(AdminRolesStore);
  readonly trackByEntityId = trackByEntityId;
  readonly virtualTableBufferPixels = virtualTableBufferPixels;
  readonly virtualTableViewportHeight = virtualTableViewportHeight;
  readonly virtualTableRowHeight = VirtualTableRowHeight;
  roleEditorOpen = false;
  protectSelectedAdminRole = false;

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

  edit(account: AdminRoleAccount): void {
    this.actions.select(account);
    this.protectSelectedAdminRole = this.isCurrentAdminAccount(account);
    this.roleEditorOpen = true;
  }

  changeRole(change: AdminRoleChange): void {
    this.actions.toggle(change.roleName, change.enabled);
  }

  readonly saveRoles = (): Observable<void> => this.actions.save();

  private isCurrentAdminAccount(account: AdminRoleAccount): boolean {
    const currentSubject = this.auth.snapshot.user?.subject;

    return (
      !!currentSubject &&
      account.id.toLowerCase() === currentSubject.toLowerCase() &&
      account.roles.some(
        (roleName) => roleName.toLowerCase() === AdminRoleNames.admin.toLowerCase(),
      )
    );
  }
}
