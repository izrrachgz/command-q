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
  readonly showDenseTables: boolean;
}

@Injectable({ providedIn: 'root' })
export class PreferencesService {
  private readonly document = inject(DOCUMENT);
  private readonly state = new BehaviorSubject<UserPreferences>(this.readPreferences());
  readonly preferences$ = this.state.asObservable();

  constructor() {
    this.applyTheme(this.snapshot.theme);
  }

  get snapshot(): UserPreferences {
    return this.state.value;
  }

  initialize(): void {
    this.applyTheme(this.snapshot.theme);
  }

  update(patch: Partial<UserPreferences>): void {
    const next = { ...this.snapshot, ...patch };
    localStorage.setItem(FrontendStorageKeys.preferences, JSON.stringify(next));
    this.applyTheme(next.theme);
    this.state.next(next);
  }

  private readPreferences(): UserPreferences {
    const fallback: UserPreferences = {
      culture: environment.localization.defaultCulture,
      theme: UserThemes.dark,
      compactNavigation: false,
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
}

const isUserTheme = (value: unknown): value is UserTheme => {
  return value === UserThemes.light || value === UserThemes.dark;
};
