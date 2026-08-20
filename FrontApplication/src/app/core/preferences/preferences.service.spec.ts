import { TestBed } from '@angular/core/testing';
import { firstValueFrom, of, Subject, throwError } from 'rxjs';

import { FrontendStorageKeys } from '../config/frontend-storage-keys';
import { PreferencesApiService } from './preferences-api.service';
import { PreferencesService, UserThemes } from './preferences.service';
import type { UserPreferences } from './preferences.service';

describe('PreferencesService', () => {
  let api: {
    get: ReturnType<typeof vi.fn>;
    save: ReturnType<typeof vi.fn>;
  };

  beforeEach(() => {
    localStorage.clear();
    document.documentElement.classList.remove(
      'a11y-enhanced-focus',
      'a11y-reduce-motion',
      'theme-dark',
      'theme-light',
    );
    document.documentElement.style.colorScheme = '';
    api = {
      get: vi.fn().mockReturnValue(of({ success: true, message: '', detail: undefined })),
      save: vi.fn().mockReturnValue(of({ success: true, message: '' })),
    };
    TestBed.configureTestingModule({
      providers: [
        {
          provide: PreferencesApiService,
          useValue: api,
        },
      ],
    });
  });

  afterEach(() => {
    localStorage.clear();
    document.documentElement.classList.remove(
      'a11y-enhanced-focus',
      'a11y-reduce-motion',
      'theme-dark',
      'theme-light',
    );
    document.documentElement.style.colorScheme = '';
  });

  it('defaults to the dark theme and applies it to the document', () => {
    const service = TestBed.inject(PreferencesService);

    expect(service.snapshot.theme).toBe(UserThemes.dark);
    expect(document.documentElement.classList.contains('theme-dark')).toBe(true);
    expect(document.documentElement.style.colorScheme).toBe('dark');
    expect(service.snapshot.timeZone).toBe(Intl.DateTimeFormat().resolvedOptions().timeZone);
  });

  it('persists and applies the light theme', () => {
    const service = TestBed.inject(PreferencesService);

    service.update({ theme: UserThemes.light });

    expect(document.documentElement.classList.contains('theme-light')).toBe(true);
    expect(document.documentElement.classList.contains('theme-dark')).toBe(false);
    expect(JSON.parse(localStorage.getItem(FrontendStorageKeys.preferences) || '{}').theme).toBe(
      UserThemes.light,
    );
  });

  it('persists and applies accessibility preferences', () => {
    const service = TestBed.inject(PreferencesService);

    service.update({ enhancedFocus: true, reduceMotion: true });

    expect(document.documentElement.classList.contains('a11y-enhanced-focus')).toBe(true);
    expect(document.documentElement.classList.contains('a11y-reduce-motion')).toBe(true);
    expect(
      JSON.parse(localStorage.getItem(FrontendStorageKeys.preferences) || '{}').reduceMotion,
    ).toBe(true);
  });

  it('marks an account as requiring setup when the backend has no preferences', async () => {
    const service = TestBed.inject(PreferencesService);

    const result = await firstValueFrom(service.activateAccount('account-1'));

    expect(result.configured).toBe(false);
    const cache = JSON.parse(
      localStorage.getItem(`${FrontendStorageKeys.preferencesPrefix}account-1`) || '{}',
    );
    expect(cache.configured).toBe(false);
    expect(cache.cachedAtUtc.endsWith('Z')).toBe(true);
    expect(cache.expiresAtUtc.endsWith('Z')).toBe(true);
  });

  it('saves backend preferences and completes the account setup cache', async () => {
    const service = TestBed.inject(PreferencesService);
    const preferences = { ...service.snapshot, timeZone: 'America/Hermosillo' };

    await firstValueFrom(service.save('account-1', preferences));

    const cache = JSON.parse(
      localStorage.getItem(`${FrontendStorageKeys.preferencesPrefix}account-1`) || '{}',
    );
    expect(cache.configured).toBe(true);
    expect(cache.preferences.timeZone).toBe('America/Hermosillo');
  });

  it('does not apply a previous account response after the session resets', () => {
    const response = new Subject<{
      success: boolean;
      message: string;
      detail: UserPreferences;
    }>();
    api.get.mockReturnValue(response.asObservable());
    const service = TestBed.inject(PreferencesService);
    const originalTheme = service.snapshot.theme;

    service.activateAccount('account-1').subscribe();
    service.resetSession();
    response.next({
      success: true,
      message: '',
      detail: { ...service.snapshot, theme: UserThemes.light },
    });
    response.complete();

    expect(service.snapshot.theme).toBe(originalTheme);
    expect(localStorage.getItem(`${FrontendStorageKeys.preferencesPrefix}account-1`)).toBeNull();
  });

  it('backs off repeated backend refreshes while an expired configured cache remains available', async () => {
    const service = TestBed.inject(PreferencesService);
    await firstValueFrom(
      service.save('account-1', { ...service.snapshot, timeZone: 'America/Hermosillo' }),
    );
    const cacheKey = `${FrontendStorageKeys.preferencesPrefix}account-1`;
    const cache = JSON.parse(localStorage.getItem(cacheKey) || '{}');
    localStorage.setItem(
      cacheKey,
      JSON.stringify({ ...cache, expiresAtUtc: new Date(0).toISOString() }),
    );
    service.resetSession();
    api.get.mockReturnValue(throwError(() => new Error('backend unavailable')));

    const first = await firstValueFrom(service.activateAccount('account-1'));
    const second = await firstValueFrom(service.activateAccount('account-1'));

    expect(first.backendUnavailable).toBe(true);
    expect(second.backendUnavailable).toBe(true);
    expect(api.get).toHaveBeenCalledTimes(1);
  });
});
