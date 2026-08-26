import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../../../environments/environment';
import { AdminAccountQueryParamNames } from '../../../../core/api/api-constants';
import { toSearchHttpParams } from '../../../../core/api/search-http-params';
import { combineUrl } from '../../../../core/config/url-builder';
import { PageResponse } from '../../../../core/models/api-response';
import { AdminAccount, AdminAccountFilters } from '../models/admin-account.model';

@Injectable({ providedIn: 'root' })
export class AdminAccountsService {
  constructor(private readonly http: HttpClient) {}

  search(filters: AdminAccountFilters): Observable<PageResponse<AdminAccount>> {
    return this.http.get<PageResponse<AdminAccount>>(combineUrl(environment.api.baseUrl, environment.api.endpoints.admin.accounts), {
      params: toParams(filters),
    });
  }

  details(accountId: string): Observable<AdminAccount> {
    return this.http.get<AdminAccount>(
      combineUrl(environment.api.baseUrl, environment.api.endpoints.admin.accountDetails(accountId)),
    );
  }
}

const toParams = (filters: AdminAccountFilters) => toSearchHttpParams(filters, {
  [AdminAccountQueryParamNames.active]: filters.active,
  [AdminAccountQueryParamNames.locked]: filters.locked,
});
