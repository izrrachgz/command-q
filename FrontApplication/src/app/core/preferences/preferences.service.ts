import { DOCUMENT } from '@angular/common';
import { inject, Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

import { SupportedCulture } from '../config/app-environment';
import { environment } from '../../../environments/environment';
import { FrontendStorageKeys } from '../config/frontend-storage-keys';

export const UserThemes = {
  dark: 'dark',
  light: 'light',
} as const;

export type UserTheme = (typeof UserThemes)[keyof typeof UserThemes];

export interface UserPreferences {
  readonly culture: SupportedCulture;
  readonly theme: UserTheme;
  readonly compactNavigation: boolean;
  readonly enhancedFocus: boolean;
  readonly reduceMotion: boolean;
  readonly showDenseTables: boolean;
}

@Injectable({ providedIn: 'root' })
export class PreferencesService {
  private readonly document = inject(DOCUMENT);
  private readonly state = new BehaviorSubject<UserPreferences>(this.readPreferences());
  readonly preferences$ = this.state.asObservable();

  constructor() {
    this.applyTheme(this.snapshot.theme);
    this.applyAccessibility(this.snapshot);
  }

  get snapshot(): UserPreferences {
    return this.state.value;
  }

  initialize(): void {
    this.applyTheme(this.snapshot.theme);
    this.applyAccessibility(this.snapshot);
  }

  update(patch: Partial<UserPreferences>): void {
    const next = { ...this.snapshot, ...patch };
    localStorage.setItem(FrontendStorageKeys.preferences, JSON.stringify(next));
    this.applyTheme(next.theme);
    this.applyAccessibility(next);
    this.state.next(next);
  }

  private readPreferences(): UserPreferences {
    const fallback: UserPreferences = {
      culture: environment.localization.defaultCulture,
      theme: UserThemes.dark,
      compactNavigation: false,
      enhancedFocus: prefersEnhancedFocus(),
      reduceMotion: prefersReducedMotion(),
      showDenseTables: false,
    };

    const stored = localStorage.getItem(FrontendStorageKeys.preferences);
    if (!stored) {
      return fallback;
    }

    try {
      const parsed = JSON.parse(stored) as Partial<UserPreferences>;
      return {
        ...fallback,
        ...parsed,
        theme: isUserTheme(parsed.theme) ? parsed.theme : fallback.theme,
      };
    } catch {
      return fallback;
    }
  }

  private applyTheme(theme: UserTheme): void {
    const root = this.document.documentElement;
    root.classList.toggle('theme-dark', theme === UserThemes.dark);
    root.classList.toggle('theme-light', theme === UserThemes.light);
    root.style.colorScheme = theme;
  }

  private applyAccessibility(preferences: UserPreferences): void {
    const root = this.document.documentElement;
    root.classList.toggle('a11y-enhanced-focus', preferences.enhancedFocus);
    root.classList.toggle('a11y-reduce-motion', preferences.reduceMotion);
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
