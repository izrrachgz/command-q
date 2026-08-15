import { DOCUMENT } from '@angular/common';
import { inject, Injectable } from '@angular/core';
import { BehaviorSubject, map, Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { SupportedCulture } from '../config/app-environment';
import { FrontendStorageKeys } from '../config/frontend-storage-keys';
import { PreferencesService } from '../preferences/preferences.service';
import { TRANSLATIONS } from './translations';

@Injectable({ providedIn: 'root' })
export class I18nService {
  private readonly preferences = inject(PreferencesService);
  private readonly document = inject(DOCUMENT);

  private readonly culture = new BehaviorSubject<SupportedCulture>(this.resolveInitialCulture());
  readonly culture$ = this.culture.asObservable();

  constructor() {
    this.applyDocumentCulture(this.currentCulture);
  }

  get currentCulture(): SupportedCulture {
    return this.culture.value;
  }

  get supportedCultures(): readonly SupportedCulture[] {
    return environment.localization.supportedCultures;
  }

  setCulture(culture: SupportedCulture): void {
    const supported = this.supportedCultures.includes(culture)
      ? culture
      : environment.localization.fallbackCulture;

    localStorage.setItem(FrontendStorageKeys.culture, supported);
    this.preferences.update({ culture: supported });
    this.culture.next(supported);
    this.applyDocumentCulture(supported);
  }

  text(key: string, params?: Record<string, unknown>): string {
    const current = TRANSLATIONS[this.currentCulture]?.[key];
    const fallback = TRANSLATIONS[environment.localization.fallbackCulture]?.[key];

    return interpolateText(current ?? fallback ?? key, params);
  }

  text$(key: string, params?: Record<string, unknown>): Observable<string> {
    return this.culture$.pipe(map(() => this.text(key, params)));
  }

  private resolveInitialCulture(): SupportedCulture {
    const stored = localStorage.getItem(FrontendStorageKeys.culture) as SupportedCulture | null;
    const preferred = this.preferences.snapshot.culture;
    const browserCulture = navigator.language as SupportedCulture;
    const candidate = stored ?? preferred ?? browserCulture;

    return environment.localization.supportedCultures.includes(candidate)
      ? candidate
      : environment.localization.defaultCulture;
  }

  private applyDocumentCulture(culture: SupportedCulture): void {
    this.document.documentElement.lang = culture;
  }
}

export const interpolateText = (value: string, params?: Record<string, unknown>): string => {
  if (!params) {
    return value;
  }

  return value.replace(/\{\{\s*(\w+)\s*\}\}/g, (_, key: string) => String(params[key] ?? ''));
};
