import { TestBed } from '@angular/core/testing';

import { FrontendStorageKeys } from '../config/frontend-storage-keys';
import { PreferencesService, UserThemes } from './preferences.service';

describe('PreferencesService', () => {
  beforeEach(() => {
    localStorage.clear();
    document.documentElement.classList.remove('theme-dark', 'theme-light');
    document.documentElement.style.colorScheme = '';
    TestBed.configureTestingModule({});
  });

  afterEach(() => {
    localStorage.clear();
    document.documentElement.classList.remove('theme-dark', 'theme-light');
    document.documentElement.style.colorScheme = '';
  });

  it('defaults to the dark theme and applies it to the document', () => {
    const service = TestBed.inject(PreferencesService);

    expect(service.snapshot.theme).toBe(UserThemes.dark);
    expect(document.documentElement.classList.contains('theme-dark')).toBe(true);
    expect(document.documentElement.style.colorScheme).toBe('dark');
  });

  it('persists and applies the light theme', () => {
    const service = TestBed.inject(PreferencesService);

    service.update({ theme: UserThemes.light });

    expect(document.documentElement.classList.contains('theme-light')).toBe(true);
    expect(document.documentElement.classList.contains('theme-dark')).toBe(false);
    expect(JSON.parse(localStorage.getItem(FrontendStorageKeys.preferences) || '{}').theme).toBe(UserThemes.light);
  });
});
