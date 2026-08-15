import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../../../environments/environment';
import { toSearchHttpParams } from '../../../../core/api/search-http-params';
import { combineUrl } from '../../../../core/config/url-builder';
import { BasicResponse, DetailResponse, PagedResponse } from '../../../../core/models/api-response';
import {
  AdminRoleAccount,
  AdminRoleFilters,
  AdminRoleList,
  UpdateAdminAccountRolesRequest,
} from '../models/admin-role.model';

@Injectable({ providedIn: 'root' })
export class AdminRolesService {
  constructor(private readonly http: HttpClient) {}

  searchAccounts(filters: AdminRoleFilters): Observable<PagedResponse<AdminRoleAccount>> {
    return this.http.get<PagedResponse<AdminRoleAccount>>(
      combineUrl(environment.api.baseUrl, environment.api.endpoints.admin.accounts),
      { params: toSearchHttpParams(filters) },
    );
  }

  roles(): Observable<DetailResponse<AdminRoleList>> {
    return this.http.get<DetailResponse<AdminRoleList>>(
      combineUrl(environment.api.baseUrl, environment.api.endpoints.admin.roles),
    );
  }

  updateAccountRoles(accountId: string, request: UpdateAdminAccountRolesRequest): Observable<BasicResponse> {
    return this.http.put<BasicResponse>(
      combineUrl(environment.api.baseUrl, environment.api.endpoints.admin.accountRoles(accountId)),
      request,
    );
  }
}
