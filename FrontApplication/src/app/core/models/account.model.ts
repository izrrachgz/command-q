export interface AccountModel {
  readonly id: string;
  readonly username: string;
  readonly email: string;
  readonly addresses?: Record<string, unknown>;
  readonly contacts?: Record<string, unknown>;
}

export interface AccountActivityModel {
  readonly id: number;
  readonly accountId: string;
  readonly module: string;
  readonly action: string;
  readonly entityType?: string;
  readonly entityId?: string;
  readonly message?: string;
  readonly createdUtc?: string;
}

export interface IdentityAccountModel {
  readonly id: string;
  readonly username: string;
  readonly email: string;
}

export interface SignInRequest {
  readonly usernameOrEmail: string;
  readonly password: string;
  readonly rememberMe: boolean;
}

export interface RegisterAccountRequest {
  readonly username: string;
  readonly email: string;
  readonly password: string;
}
