import { of } from 'rxjs';

import { ApplicationConfigurationsActions } from './application-configurations.actions';
import { ApplicationConfigurationsStore } from '../stores/application-configurations.store';

describe('ApplicationConfigurationsActions', () => {
  it('loads the server-recorded configuration history', () => {
    const store = new ApplicationConfigurationsStore();
    const service = {
      search: vi.fn().mockReturnValue(
        of({
          items: [
            {
              id: 1,
              serverTimeZone: 'America/Hermosillo',
              applicationVersion: '1.0.0',
              language: 'en-US',
              environment: 'Development',
              instanceName: 'api-01',
              createdUtc: '2026-08-20T08:00:00Z',
            },
          ],
          page: 1,
          pageSize: 25,
          total: 1,
          totalPages: 1,
        }),
      ),
    };
    const actions = new ApplicationConfigurationsActions(service as never, store);

    actions.load().subscribe();

    expect(service.search).toHaveBeenCalledWith(store.snapshot.filters);
    expect(store.snapshot.status).toBe('ready');
    expect(store.snapshot.records).toHaveLength(1);
  });
});
