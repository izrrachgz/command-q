import { AccountActivityModel } from '../../../../core/models/account.model';
import { SearchCriteria } from '../../../../core/models/api-response';

export interface AdminAccount {
  readonly id: string;
  readonly username: string;
  readonly email: string;
  readonly active: boolean;
  readonly locked: boolean;
  readonly roles: readonly string[];
  readonly createdUtc?: string;
  readonly lockedUntilUtc?: string;
}

export interface AdminAccountFilters extends SearchCriteria {
  readonly locked?: boolean;
  readonly active?: boolean;
}

export interface CreateAdminAccountRequest {
  readonly username: string;
  readonly email: string;
  readonly temporaryPassword: string;
  readonly sendInvite: boolean;
}

export interface AdminAccountDetails {
  readonly account: AdminAccount;
  readonly activity: readonly AccountActivityModel[];
}
