import { TestBed } from '@angular/core/testing';
import { LUCIDE_ICONS } from '@lucide/angular';

import { appConfig } from './app.config';

describe('appConfig', () => {
  it('registers icons used by account and role editor modals', () => {
    TestBed.configureTestingModule({ providers: appConfig.providers });

    const icons = TestBed.inject(LUCIDE_ICONS);

    expect(icons['pencil']).toBeDefined();
    expect(icons['user-round-search']).toBeDefined();
    expect(icons['user-plus']).toBeDefined();
  });
});
