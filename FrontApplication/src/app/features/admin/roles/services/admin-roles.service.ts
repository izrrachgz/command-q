import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../../../environments/environment';
import { toSearchHttpParams } from '../../../../core/api/search-http-params';
import { combineUrl } from '../../../../core/config/url-builder';
import { PageResponse } from '../../../../core/models/api-response';
import {
  AdminRoleAccount,
  AdminRoleFilters,
  AdminRoleList,
  UpdateAdminAccountRolesRequest,
} from '../models/admin-role.model';

@Injectable({ providedIn: 'root' })
export class AdminRolesService {
  constructor(private readonly http: HttpClient) {}

  searchAccounts(filters: AdminRoleFilters): Observable<PageResponse<AdminRoleAccount>> {
    return this.http.get<PageResponse<AdminRoleAccount>>(
      combineUrl(environment.api.baseUrl, environment.api.endpoints.admin.accounts),
      { params: toSearchHttpParams(filters) },
    );
  }

  roles(): Observable<AdminRoleList> {
    return this.http.get<AdminRoleList>(
      combineUrl(environment.api.baseUrl, environment.api.endpoints.admin.roles),
    );
  }

  updateAccountRoles(accountId: string, request: UpdateAdminAccountRolesRequest): Observable<void> {
    return this.http.put<void>(
      combineUrl(environment.api.baseUrl, environment.api.endpoints.admin.accountRoles(accountId)),
      request,
    );
  }
}
