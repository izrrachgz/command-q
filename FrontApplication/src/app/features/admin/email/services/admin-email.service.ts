import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../../../environments/environment';
import { AdminEmailQueryParamNames } from '../../../../core/api/api-constants';
import { toSearchHttpParams } from '../../../../core/api/search-http-params';
import { combineUrl } from '../../../../core/config/url-builder';
import { PagedResponse } from '../../../../core/models/api-response';
import { AdminEmailFilters, AdminEmailRecord } from '../models/admin-email.model';

@Injectable({ providedIn: 'root' })
export class AdminEmailService {
  constructor(private readonly http: HttpClient) {}

  search(filters: AdminEmailFilters): Observable<PagedResponse<AdminEmailRecord>> {
    return this.http.get<PagedResponse<AdminEmailRecord>>(combineUrl(environment.api.baseUrl, environment.api.endpoints.admin.email), {
      params: toParams(filters),
    });
  }
}

const toParams = (filters: AdminEmailFilters) => toSearchHttpParams(filters, {
  [AdminEmailQueryParamNames.recipient]: filters.recipient,
  [AdminEmailQueryParamNames.status]: filters.status,
});
