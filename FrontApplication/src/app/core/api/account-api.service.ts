import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { combineUrl } from '../config/url-builder';
import { AccountActivityModel, AccountModel } from '../models/account.model';
import { DetailResponse, PagedResponse, SearchCriteria } from '../models/api-response';
import { toSearchHttpParams } from './search-http-params';

@Injectable({ providedIn: 'root' })
export class AccountApiService {
  constructor(private readonly http: HttpClient) {}

  me(): Observable<DetailResponse<AccountModel>> {
    return this.http.get<DetailResponse<AccountModel>>(
      combineUrl(environment.api.baseUrl, environment.api.endpoints.account.me),
    );
  }

  activity(criteria: SearchCriteria): Observable<PagedResponse<AccountActivityModel>> {
    return this.http.get<PagedResponse<AccountActivityModel>>(
      combineUrl(environment.api.baseUrl, environment.api.endpoints.account.activity),
      { params: toSearchHttpParams(criteria) },
    );
  }
}
