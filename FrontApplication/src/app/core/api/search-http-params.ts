import { HttpParams } from '@angular/common/http';

import { SearchCriteria } from '../models/api-response';
import { SearchQueryParamNames } from './api-constants';

type SearchHttpParamValue = boolean | number | string | null | undefined;

/**
 * Builds the query string used by backend paged search endpoints.
 */
export const toSearchHttpParams = (
  criteria: SearchCriteria,
  extras: Record<string, SearchHttpParamValue> = {},
): HttpParams => {
  const values: Record<string, string> = {
    [SearchQueryParamNames.page]: String(criteria.page),
    [SearchQueryParamNames.size]: String(criteria.size),
  };

  appendIfPresent(values, SearchQueryParamNames.searchTerm, criteria.searchTerm);
  appendIfPresent(values, SearchQueryParamNames.sortBy, criteria.sortBy);
  appendIfPresent(values, SearchQueryParamNames.ascendingSort, criteria.ascendingSort);
  appendIfPresent(values, SearchQueryParamNames.startDate, criteria.startDate);
  appendIfPresent(values, SearchQueryParamNames.endDate, criteria.endDate);

  for (const [key, value] of Object.entries(extras)) {
    appendIfPresent(values, key, value);
  }

  return new HttpParams({ fromObject: values });
};

const appendIfPresent = (values: Record<string, string>, key: string, value: SearchHttpParamValue): void => {
  if (value === null || value === undefined || value === '') {
    return;
  }

  values[key] = String(value);
};
