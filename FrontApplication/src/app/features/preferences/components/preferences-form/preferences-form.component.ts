import { Component, DestroyRef, EventEmitter, Input, OnInit, Output, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import { LucideDynamicIcon } from '@lucide/angular';
import { finalize } from 'rxjs';

import { AuthFacade } from '../../../../core/auth/auth.facade';
import { SupportedCulture } from '../../../../core/config/app-environment';
import { I18nService } from '../../../../core/i18n/i18n.service';
import { TextPipe } from '../../../../core/i18n/text.pipe';
import {
  AccountPreferenceLimits,
  PreferencesService,
  UserPreferences,
  UserTheme,
  UserThemes,
} from '../../../../core/preferences/preferences.service';
import { TimeZoneService } from '../../../../core/time/time-zone.service';
import { TimeZoneAutocompleteComponent } from '../../../../shared/forms';

@Component({
  selector: 'app-preferences-form',
  standalone: true,
  imports: [LucideDynamicIcon, ReactiveFormsModule, TextPipe, TimeZoneAutocompleteComponent],
  templateUrl: './preferences-form.component.html',
})
export class PreferencesFormComponent implements OnInit {
  @Input() setupMode = false;
  @Output() saved = new EventEmitter<UserPreferences>();

  readonly i18n = inject(I18nService);
  readonly limits = AccountPreferenceLimits;
  readonly themes = UserThemes;

  private readonly auth = inject(AuthFacade);
  private readonly destroyRef = inject(DestroyRef);
  private readonly fb = inject(FormBuilder);
  private readonly preferences = inject(PreferencesService);
  private readonly timeZones = inject(TimeZoneService);

  readonly form = this.fb.nonNullable.group({
    culture: [this.preferences.snapshot.culture, Validators.required],
    theme: [this.preferences.snapshot.theme, Validators.required],
    compactNavigation: [this.preferences.snapshot.compactNavigation],
    enhancedFocus: [this.preferences.snapshot.enhancedFocus],
    reduceMotion: [this.preferences.snapshot.reduceMotion],
    showDenseTables: [this.preferences.snapshot.showDenseTables],
    timeZone: [
      this.preferences.snapshot.timeZone,
      [
        Validators.required,
        Validators.maxLength(AccountPreferenceLimits.timeZoneMaxLength),
        timeZoneValidator(this.timeZones),
      ],
    ],
  });

  loading = false;
  saving = false;
  message = '';
  error = '';

  constructor() {
    this.preferences.preferences$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((preferences) => this.resetForm(preferences));
  }

  ngOnInit(): void {
    const accountKey = this.accountKey;
    if (!accountKey) {
      return;
    }

    this.loading = true;
    this.preferences
      .activateAccount(accountKey)
      .pipe(
        finalize(() => (this.loading = false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe();
  }

  setTheme(dark: boolean): void {
    const theme: UserTheme = dark ? UserThemes.dark : UserThemes.light;
    this.form.controls.theme.setValue(theme);
    this.form.controls.theme.markAsDirty();
  }

  save(): void {
    if (this.form.invalid || this.saving || !this.accountKey) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving = true;
    this.message = '';
    this.error = '';
    const raw = this.form.getRawValue();
    const preferences: UserPreferences = {
      ...raw,
      culture: raw.culture as SupportedCulture,
      theme: raw.theme as UserTheme,
      timeZone: raw.timeZone.trim(),
    };

    this.preferences
      .save(this.accountKey, preferences)
      .pipe(
        finalize(() => (this.saving = false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe({
        next: (savedPreferences) => {
          this.message = 'preferences.saved';
          this.form.markAsPristine();
          this.saved.emit(savedPreferences);
        },
        error: () => {
          this.error = 'preferences.saveFailed';
        },
      });
  }

  refresh(): void {
    if (this.loading || this.saving || !this.accountKey) {
      return;
    }

    this.loading = true;
    this.message = '';
    this.error = '';
    this.preferences
      .refresh(this.accountKey)
      .pipe(
        finalize(() => (this.loading = false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe({
        error: () => (this.error = 'preferences.loadFailed'),
      });
  }

  private get accountKey(): string {
    return this.auth.snapshot.account?.id ?? this.auth.snapshot.user?.subject ?? '';
  }

  private resetForm(preferences: UserPreferences): void {
    if (this.form.dirty && !this.saving) {
      return;
    }

    this.form.reset(preferences);
  }
}

const timeZoneValidator = (timeZones: TimeZoneService): ValidatorFn => {
  return (control: AbstractControl<string>): ValidationErrors | null => {
    return !control.value || timeZones.isValid(control.value) ? null : { timeZone: true };
  };
};
