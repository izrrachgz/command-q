import { SupportedCulture } from '../config/app-environment';

export const AccountPreferenceLimits = {
  timeZoneMaxLength: 64,
} as const;

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
  readonly timeZone: string;
}

export interface AccountPreferencesCache {
  readonly version: 1;
  readonly accountKey: string;
  readonly configured: boolean;
  readonly cachedAtUtc: string;
  readonly expiresAtUtc: string;
  readonly preferences: UserPreferences;
}

export interface PreferencesLoadResult {
  readonly configured: boolean;
  readonly backendUnavailable: boolean;
  readonly preferences: UserPreferences;
}
