import { toSearchHttpParams } from './search-http-params';
import { AdminAccountQueryParamNames, ApiSortFields, SearchQueryParamNames } from './api-constants';

describe('toSearchHttpParams', () => {
  it('maps common search criteria and feature extras into backend query names', () => {
    const params = toSearchHttpParams(
      {
        page: 2,
        size: 50,
        searchTerm: 'admin',
        sortBy: ApiSortFields.createdUtc,
        ascendingSort: false,
        startDate: '2026-08-01',
        endDate: '2026-08-15',
      },
      {
        [AdminAccountQueryParamNames.active]: false,
        [AdminAccountQueryParamNames.locked]: true,
        Empty: '',
      },
    );

    expect(params.get(SearchQueryParamNames.page)).toBe('2');
    expect(params.get(SearchQueryParamNames.size)).toBe('50');
    expect(params.get(SearchQueryParamNames.searchTerm)).toBe('admin');
    expect(params.get(SearchQueryParamNames.sortBy)).toBe(ApiSortFields.createdUtc);
    expect(params.get(SearchQueryParamNames.ascendingSort)).toBe('false');
    expect(params.get(SearchQueryParamNames.startDate)).toBe('2026-08-01');
    expect(params.get(SearchQueryParamNames.endDate)).toBe('2026-08-15');
    expect(params.get(AdminAccountQueryParamNames.active)).toBe('false');
    expect(params.get(AdminAccountQueryParamNames.locked)).toBe('true');
    expect(params.has('Empty')).toBe(false);
  });
});
