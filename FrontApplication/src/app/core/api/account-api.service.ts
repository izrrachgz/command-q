import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { combineUrl } from '../config/url-builder';
import { AccountActivityModel, AccountModel } from '../models/account.model';
import { PageResponse, SearchCriteria } from '../models/api-response';
import { toSearchHttpParams } from './search-http-params';

@Injectable({ providedIn: 'root' })
export class AccountApiService {
  constructor(private readonly http: HttpClient) {}

  me(): Observable<AccountModel> {
    return this.http.get<AccountModel>(
      combineUrl(environment.api.baseUrl, environment.api.endpoints.account.me),
    );
  }

  activity(criteria: SearchCriteria): Observable<PageResponse<AccountActivityModel>> {
    return this.http.get<PageResponse<AccountActivityModel>>(
      combineUrl(environment.api.baseUrl, environment.api.endpoints.account.activity),
      { params: toSearchHttpParams(criteria) },
    );
  }
}
