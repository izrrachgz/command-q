import { TestBed } from '@angular/core/testing';

import { I18nService } from '../i18n/i18n.service';
import { PreferencesService } from '../preferences/preferences.service';
import { AccountDatePipe } from './account-date.pipe';

describe('AccountDatePipe', () => {
  it('renders server UTC values in the account timezone', () => {
    TestBed.configureTestingModule({
      providers: [
        { provide: I18nService, useValue: { currentCulture: 'en-US' } },
        {
          provide: PreferencesService,
          useValue: { snapshot: { timeZone: 'Pacific/Kiritimati' } },
        },
      ],
    });
    const pipe = TestBed.runInInjectionContext(() => new AccountDatePipe());
    const firstUtc = '2026-08-17T18:00:00Z';
    const secondUtc = '2026-08-18T18:00:00Z';

    const first = pipe.transform(firstUtc);
    const second = pipe.transform(secondUtc);

    expect(first).toContain('Aug 18, 2026');
    expect(second).toContain('Aug 19, 2026');
  });
});
