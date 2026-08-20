import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { combineUrl } from '../config/url-builder';
import { BasicResponse, DetailResponse } from '../models/api-response';
import { UserPreferences } from './preferences.model';

@Injectable({ providedIn: 'root' })
export class PreferencesApiService {
  constructor(private readonly http: HttpClient) {}

  get(): Observable<DetailResponse<UserPreferences>> {
    return this.http.get<DetailResponse<UserPreferences>>(
      combineUrl(environment.api.baseUrl, environment.api.endpoints.account.preferences),
    );
  }

  save(preferences: UserPreferences): Observable<BasicResponse> {
    return this.http.put<BasicResponse>(
      combineUrl(environment.api.baseUrl, environment.api.endpoints.account.preferences),
      preferences,
    );
  }
}
