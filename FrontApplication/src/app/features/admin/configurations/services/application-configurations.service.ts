import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../../../environments/environment';
import { toSearchHttpParams } from '../../../../core/api/search-http-params';
import { combineUrl } from '../../../../core/config/url-builder';
import { PagedResponse } from '../../../../core/models/api-response';
import {
  ApplicationConfigurationFilters,
  ApplicationConfigurationModel,
} from '../models/application-configuration.model';

@Injectable({ providedIn: 'root' })
export class ApplicationConfigurationsService {
  constructor(private readonly http: HttpClient) {}

  search(
    filters: ApplicationConfigurationFilters,
  ): Observable<PagedResponse<ApplicationConfigurationModel>> {
    return this.http.get<PagedResponse<ApplicationConfigurationModel>>(
      combineUrl(environment.api.baseUrl, environment.api.endpoints.admin.configurations),
      { params: toSearchHttpParams(filters) },
    );
  }
}
