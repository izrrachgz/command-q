import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { combineUrl } from '../config/url-builder';
import { UserPreferences } from './preferences.model';

@Injectable({ providedIn: 'root' })
export class PreferencesApiService {
  constructor(private readonly http: HttpClient) {}

  get(): Observable<UserPreferences> {
    return this.http.get<UserPreferences>(
      combineUrl(environment.api.baseUrl, environment.api.endpoints.account.preferences),
    );
  }

  save(preferences: UserPreferences): Observable<void> {
    return this.http.put<void>(
      combineUrl(environment.api.baseUrl, environment.api.endpoints.account.preferences),
      preferences,
    );
  }
}
