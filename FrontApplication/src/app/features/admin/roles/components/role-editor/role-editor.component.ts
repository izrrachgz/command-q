import { Component, EventEmitter, Input, Output } from '@angular/core';

import { AdminRole, AdminRoleAccount, AdminRoleNames } from '../../models/admin-role.model';

export interface AdminRoleChange {
  readonly roleName: string;
  readonly enabled: boolean;
}

@Component({
  selector: 'app-role-editor',
  standalone: true,
  templateUrl: './role-editor.component.html',
})
export class RoleEditorComponent {
  @Input({ required: true }) account!: AdminRoleAccount;
  @Input() roles: readonly AdminRole[] = [];
  @Input() selectedRoleNames: readonly string[] = [];
  @Input() disabled = false;
  @Input() protectAdminRole = false;

  @Output() roleChanged = new EventEmitter<AdminRoleChange>();

  isSelected(roleName: string): boolean {
    return this.selectedRoleNames.some(
      (selectedRoleName) => selectedRoleName.toLowerCase() === roleName.toLowerCase(),
    );
  }

  isLocked(roleName: string): boolean {
    const normalizedRoleName = roleName.toLowerCase();

    return (
      normalizedRoleName === AdminRoleNames.user.toLowerCase() ||
      (this.protectAdminRole && normalizedRoleName === AdminRoleNames.admin.toLowerCase())
    );
  }

  changeRole(roleName: string, event: Event): void {
    this.roleChanged.emit({
      roleName,
      enabled: (event.target as HTMLInputElement).checked,
    });
  }
}
