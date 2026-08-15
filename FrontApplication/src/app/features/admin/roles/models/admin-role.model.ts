import { AdminAccount, AdminAccountFilters } from '../../accounts/models/admin-account.model';

export interface AdminRole {
  readonly id: string;
  readonly name: string;
}

export interface AdminRoleList {
  readonly roles: readonly AdminRole[];
}

export interface UpdateAdminAccountRolesRequest {
  readonly roles: readonly string[];
}

export type AdminRoleAccount = AdminAccount;
export type AdminRoleFilters = AdminAccountFilters;

export const AdminRoleNames = {
  admin: 'Admin',
  user: 'User',
} as const;
