import { DOCUMENT } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import {
  BehaviorSubject,
  catchError,
  finalize,
  map,
  Observable,
  of,
  shareReplay,
  Subject,
  takeUntil,
  tap,
  throwError,
} from 'rxjs';

import { environment } from '../../../environments/environment';
import { FrontendStorageKeys } from '../config/frontend-storage-keys';
import { detailPayload } from '../models/api-response';
import { resolveBrowserTimeZone, TimeZoneService } from '../time/time-zone.service';
import { PreferencesApiService } from './preferences-api.service';
import {
  AccountPreferencesCache,
  PreferencesLoadResult,
  UserPreferences,
  UserTheme,
  UserThemes,
} from './preferences.model';

export { AccountPreferenceLimits, UserThemes } from './preferences.model';
export type { UserPreferences, UserTheme } from './preferences.model';

const CacheLifetimeMilliseconds = environment.preferences.cacheLifetimeMinutes * 60 * 1000;
const BackendRetryDelayMilliseconds = environment.preferences.backendRetryDelaySeconds * 1000;

@Injectable({ providedIn: 'root' })
export class PreferencesService {
  private readonly api = inject(PreferencesApiService);
  private readonly document = inject(DOCUMENT);
  private readonly timeZones = inject(TimeZoneService);
  private readonly state = new BehaviorSubject<UserPreferences>(this.readLegacyPreferences());
  private readonly refreshedAccounts = new Set<string>();
  private readonly loadRequests = new Map<string, Observable<PreferencesLoadResult>>();
  private readonly retryAfterByAccount = new Map<string, number>();
  private readonly cancelPendingRequests = new Subject<void>();
  private activeAccountKey = '';

  readonly preferences$ = this.state.asObservable();

  constructor() {
    this.apply(this.snapshot);
  }

  get snapshot(): UserPreferences {
    return this.state.value;
  }

  initialize(): void {
    this.apply(this.snapshot);
  }

  activateAccount(accountKey: string, forceRefresh = false): Observable<PreferencesLoadResult> {
    const normalizedKey = accountKey.trim();
    if (!normalizedKey) {
      return of({ configured: false, backendUnavailable: false, preferences: this.snapshot });
    }

    this.activateAccountContext(normalizedKey);
    const cache = this.readAccountCache(normalizedKey);

    if (cache) {
      this.commitLocal(cache.preferences);
    }

    const cacheIsFresh = cache !== null && Date.parse(cache.expiresAtUtc) > Date.now();
    if (!forceRefresh && this.refreshedAccounts.has(normalizedKey) && cacheIsFresh) {
      return of({
        configured: cache.configured,
        backendUnavailable: false,
        preferences: cache.preferences,
      });
    }

    const retryAfter = this.retryAfterByAccount.get(normalizedKey) ?? 0;
    if (!forceRefresh && retryAfter > Date.now()) {
      return of({
        configured: cache?.configured ?? false,
        backendUnavailable: true,
        preferences: cache?.preferences ?? this.snapshot,
      });
    }

    const existingRequest = this.loadRequests.get(normalizedKey);
    if (existingRequest) {
      return existingRequest;
    }

    let request$: Observable<PreferencesLoadResult>;
    request$ = this.api.get().pipe(
      map((response) => detailPayload(response)),
      map((preferences) => {
        if (!preferences) {
          return this.markSetupRequired(normalizedKey);
        }

        const normalized = this.normalize(preferences);
        this.retryAfterByAccount.delete(normalizedKey);
        this.writeAccountCache(normalizedKey, normalized, true);
        this.commitIfActive(normalizedKey, normalized);
        this.refreshedAccounts.add(normalizedKey);
        return { configured: true, backendUnavailable: false, preferences: normalized };
      }),
      catchError((error: unknown) => {
        if (error instanceof HttpErrorResponse && error.status === 404) {
          return of(this.markSetupRequired(normalizedKey));
        }

        this.retryAfterByAccount.set(normalizedKey, Date.now() + BackendRetryDelayMilliseconds);
        const availableCache = this.readAccountCache(normalizedKey);
        if (availableCache?.configured) {
          this.commitIfActive(normalizedKey, availableCache.preferences);
          return of({
            configured: true,
            backendUnavailable: true,
            preferences: availableCache.preferences,
          });
        }

        return of({ configured: false, backendUnavailable: true, preferences: this.snapshot });
      }),
      takeUntil(this.cancelPendingRequests),
      finalize(() => {
        if (this.loadRequests.get(normalizedKey) === request$) {
          this.loadRequests.delete(normalizedKey);
        }
      }),
      shareReplay({ bufferSize: 1, refCount: false }),
    );
    this.loadRequests.set(normalizedKey, request$);

    return request$;
  }

  refresh(accountKey: string): Observable<PreferencesLoadResult> {
    return this.activateAccount(accountKey, true);
  }

  save(accountKey: string, preferences: UserPreferences): Observable<UserPreferences> {
    const normalizedKey = accountKey.trim();
    const normalized = this.normalize(preferences);

    if (!normalizedKey || !this.timeZones.isValid(normalized.timeZone)) {
      return throwError(() => new RangeError('A valid account and timezone are required.'));
    }

    this.activateAccountContext(normalizedKey);
    return this.api.save(normalized).pipe(
      takeUntil(this.cancelPendingRequests),
      tap(() => {
        this.retryAfterByAccount.delete(normalizedKey);
        this.refreshedAccounts.add(normalizedKey);
        this.writeAccountCache(normalizedKey, normalized, true);
        this.commitIfActive(normalizedKey, normalized);
      }),
      map(() => normalized),
    );
  }

  update(patch: Partial<UserPreferences>): void {
    const next = this.normalize({ ...this.snapshot, ...patch });
    localStorage.setItem(FrontendStorageKeys.preferences, JSON.stringify(next));

    if (this.activeAccountKey) {
      const cache = this.readAccountCache(this.activeAccountKey);
      this.writeAccountCache(this.activeAccountKey, next, cache?.configured ?? false);
    }

    this.commitLocal(next);
  }

  resetSession(): void {
    this.cancelPendingRequests.next();
    this.loadRequests.clear();
    this.refreshedAccounts.clear();
    this.retryAfterByAccount.clear();
    this.activeAccountKey = '';
    this.commitLocal(this.readLegacyPreferences());
  }

  private markSetupRequired(accountKey: string): PreferencesLoadResult {
    const defaults = this.createDefaults();
    this.retryAfterByAccount.delete(accountKey);
    this.writeAccountCache(accountKey, defaults, false);
    this.commitIfActive(accountKey, defaults);
    this.refreshedAccounts.add(accountKey);
    return { configured: false, backendUnavailable: false, preferences: defaults };
  }

  private readLegacyPreferences(): UserPreferences {
    const fallback = this.createDefaults();
    const stored = localStorage.getItem(FrontendStorageKeys.preferences);
    if (!stored) {
      return fallback;
    }

    try {
      return this.normalize({ ...fallback, ...(JSON.parse(stored) as Partial<UserPreferences>) });
    } catch {
      return fallback;
    }
  }

  private readAccountCache(accountKey: string): AccountPreferencesCache | null {
    const stored = localStorage.getItem(this.cacheKey(accountKey));
    if (!stored) {
      return null;
    }

    try {
      const cache = JSON.parse(stored) as Partial<AccountPreferencesCache>;
      if (cache.version !== 1 || cache.accountKey !== accountKey || !cache.preferences) {
        return null;
      }

      return {
        version: 1,
        accountKey,
        configured: cache.configured === true,
        cachedAtUtc: cache.cachedAtUtc ?? new Date(0).toISOString(),
        expiresAtUtc: cache.expiresAtUtc ?? new Date(0).toISOString(),
        preferences: this.normalize(cache.preferences),
      };
    } catch {
      return null;
    }
  }

  private writeAccountCache(
    accountKey: string,
    preferences: UserPreferences,
    configured: boolean,
  ): void {
    const now = Date.now();
    const cache: AccountPreferencesCache = {
      version: 1,
      accountKey,
      configured,
      cachedAtUtc: new Date(now).toISOString(),
      expiresAtUtc: new Date(now + CacheLifetimeMilliseconds).toISOString(),
      preferences,
    };

    localStorage.setItem(this.cacheKey(accountKey), JSON.stringify(cache));
    localStorage.setItem(FrontendStorageKeys.preferences, JSON.stringify(preferences));
  }

  private normalize(preferences: UserPreferences): UserPreferences {
    const fallback = this.createDefaults();
    const culture = environment.localization.supportedCultures.includes(preferences.culture)
      ? preferences.culture
      : fallback.culture;
    const theme = isUserTheme(preferences.theme) ? preferences.theme : fallback.theme;
    const timeZone = this.timeZones.isValid(preferences.timeZone)
      ? preferences.timeZone.trim()
      : fallback.timeZone;

    return {
      culture,
      theme,
      compactNavigation: preferences.compactNavigation === true,
      enhancedFocus: preferences.enhancedFocus === true,
      reduceMotion: preferences.reduceMotion === true,
      showDenseTables: preferences.showDenseTables === true,
      timeZone,
    };
  }

  private createDefaults(): UserPreferences {
    return {
      culture: environment.localization.defaultCulture,
      theme: UserThemes.dark,
      compactNavigation: false,
      enhancedFocus: prefersEnhancedFocus(),
      reduceMotion: prefersReducedMotion(),
      showDenseTables: false,
      timeZone: this.timeZones?.browserTimeZone ?? resolveBrowserTimeZone(),
    };
  }

  private commitLocal(preferences: UserPreferences): void {
    this.apply(preferences);
    this.state.next(preferences);
  }

  private commitIfActive(accountKey: string, preferences: UserPreferences): void {
    if (this.activeAccountKey === accountKey) {
      this.commitLocal(preferences);
    }
  }

  private activateAccountContext(accountKey: string): void {
    if (this.activeAccountKey && this.activeAccountKey !== accountKey) {
      this.cancelPendingRequests.next();
      this.loadRequests.clear();
    }

    this.activeAccountKey = accountKey;
  }

  private apply(preferences: UserPreferences): void {
    const root = this.document.documentElement;
    root.classList.toggle('theme-dark', preferences.theme === UserThemes.dark);
    root.classList.toggle('theme-light', preferences.theme === UserThemes.light);
    root.classList.toggle('a11y-enhanced-focus', preferences.enhancedFocus);
    root.classList.toggle('a11y-reduce-motion', preferences.reduceMotion);
    root.style.colorScheme = preferences.theme;
  }

  private cacheKey(accountKey: string): string {
    return `${FrontendStorageKeys.preferencesPrefix}${encodeURIComponent(accountKey)}`;
  }
}

const isUserTheme = (value: unknown): value is UserTheme => {
  return value === UserThemes.light || value === UserThemes.dark;
};

const prefersEnhancedFocus = (): boolean => {
  return globalThis.matchMedia?.('(forced-colors: active)').matches ?? false;
};

const prefersReducedMotion = (): boolean => {
  return globalThis.matchMedia?.('(prefers-reduced-motion: reduce)').matches ?? false;
};
