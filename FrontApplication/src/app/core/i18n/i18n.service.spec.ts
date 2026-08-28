import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';

import { PreferencesApiService } from '../preferences/preferences-api.service';
import { I18nService } from './i18n.service';
import { TextKeys } from './text-keys';

describe('I18nService', () => {
  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [
        {
          provide: PreferencesApiService,
          useValue: {
            get: () => of(undefined),
            save: () => of(undefined),
          },
        },
      ],
    });
  });

  it('returns translated text for the selected culture', () => {
    const service = TestBed.inject(I18nService);

    service.setCulture('es-ES');

    expect(service.text(TextKeys.action.save)).toBe('Guardar');
  });

  it('interpolates translated text parameters', () => {
    const service = TestBed.inject(I18nService);

    expect(service.text(TextKeys.app.documentTitle, { app: 'command-q', title: 'Dashboard' })).toBe(
      'command-q | Dashboard',
    );
  });

  it('falls back to the key when no translation exists', () => {
    const service = TestBed.inject(I18nService);

    expect(service.text('missing.key')).toBe('missing.key');
  });
});
