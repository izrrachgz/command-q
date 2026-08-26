import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../../../environments/environment';
import { AdminEmailQueryParamNames } from '../../../../core/api/api-constants';
import { toSearchHttpParams } from '../../../../core/api/search-http-params';
import { combineUrl } from '../../../../core/config/url-builder';
import { PageResponse } from '../../../../core/models/api-response';
import { PreferencesService } from '../../../../core/preferences/preferences.service';
import { accountLocalDateToUtcIso } from '../../../../core/time/date-time-utc';
import { AdminEmailFilters, AdminEmailRecord } from '../models/admin-email.model';

@Injectable({ providedIn: 'root' })
export class AdminEmailService {
  constructor(
    private readonly http: HttpClient,
    private readonly preferences: PreferencesService,
  ) {}

  search(filters: AdminEmailFilters): Observable<PageResponse<AdminEmailRecord>> {
    return this.http.get<PageResponse<AdminEmailRecord>>(
      combineUrl(environment.api.baseUrl, environment.api.endpoints.admin.email),
      {
        params: toParams(filters, this.preferences.snapshot.timeZone),
      },
    );
  }
}

const toParams = (filters: AdminEmailFilters, timeZone: string) => {
  const utcFilters: AdminEmailFilters = {
    ...filters,
    startDate: filters.startDate
      ? accountLocalDateToUtcIso(filters.startDate, timeZone)
      : undefined,
    endDate: filters.endDate
      ? accountLocalDateToUtcIso(filters.endDate, timeZone, true)
      : undefined,
  };

  return toSearchHttpParams(utcFilters, {
    [AdminEmailQueryParamNames.recipient]: filters.recipient,
    [AdminEmailQueryParamNames.status]: filters.status,
  });
};
