import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../../../environments/environment';
import { AdminAccountQueryParamNames, ApiSortFields } from '../../../../core/api/api-constants';
import { toSearchHttpParams } from '../../../../core/api/search-http-params';
import { combineUrl } from '../../../../core/config/url-builder';
import { AccountActivityModel } from '../../../../core/models/account.model';
import { BasicResponse, DetailResponse, PagedResponse } from '../../../../core/models/api-response';
import { AdminAccount, AdminAccountFilters, CreateAdminAccountRequest } from '../models/admin-account.model';

@Injectable({ providedIn: 'root' })
export class AdminAccountsService {
  constructor(private readonly http: HttpClient) {}

  search(filters: AdminAccountFilters): Observable<PagedResponse<AdminAccount>> {
    return this.http.get<PagedResponse<AdminAccount>>(combineUrl(environment.api.baseUrl, environment.api.endpoints.admin.accounts), {
      params: toParams(filters),
    });
  }

  details(accountId: string): Observable<DetailResponse<AdminAccount>> {
    return this.http.get<DetailResponse<AdminAccount>>(
      combineUrl(environment.api.baseUrl, environment.api.endpoints.admin.accountDetails(accountId)),
    );
  }

  activity(accountId: string): Observable<PagedResponse<AccountActivityModel>> {
    return this.http.get<PagedResponse<AccountActivityModel>>(
      combineUrl(environment.api.baseUrl, environment.api.endpoints.admin.accountActivity(accountId)),
      { params: toSearchHttpParams({ page: 1, size: 25, sortBy: ApiSortFields.createdUtc, ascendingSort: false }) },
    );
  }

  lock(accountId: string): Observable<BasicResponse> {
    return this.http.post<BasicResponse>(
      combineUrl(environment.api.baseUrl, environment.api.endpoints.admin.lockAccount(accountId)),
      {},
    );
  }

  activate(accountId: string): Observable<BasicResponse> {
    return this.http.post<BasicResponse>(
      combineUrl(environment.api.baseUrl, environment.api.endpoints.admin.activateAccount(accountId)),
      {},
    );
  }

  create(request: CreateAdminAccountRequest): Observable<DetailResponse<AdminAccount>> {
    return this.http.post<DetailResponse<AdminAccount>>(
      combineUrl(environment.api.baseUrl, environment.api.endpoints.admin.accounts),
      request,
    );
  }
}

const toParams = (filters: AdminAccountFilters) => toSearchHttpParams(filters, {
  [AdminAccountQueryParamNames.active]: filters.active,
  [AdminAccountQueryParamNames.locked]: filters.locked,
});
