import { Component, forwardRef, Input, inject } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { LucideDynamicIcon } from '@lucide/angular';

import { I18nService } from '../../../core/i18n/i18n.service';
import { TextPipe } from '../../../core/i18n/text.pipe';
import { TimeZoneOption, TimeZoneService } from '../../../core/time/time-zone.service';

let nextTimeZoneInputId = 0;

@Component({
  selector: 'app-time-zone-autocomplete',
  standalone: true,
  imports: [LucideDynamicIcon, TextPipe],
  templateUrl: './time-zone-autocomplete.component.html',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => TimeZoneAutocompleteComponent),
      multi: true,
    },
  ],
})
export class TimeZoneAutocompleteComponent implements ControlValueAccessor {
  @Input() label = 'field.timeZone';
  @Input() placeholder = 'preferences.timeZonePlaceholder';

  private readonly i18n = inject(I18nService);
  private readonly timeZones = inject(TimeZoneService);
  private optionCulture = '';
  private cachedOptions: readonly TimeZoneOption[] = [];
  private onChange: (value: string) => void = () => undefined;
  private onTouched: () => void = () => undefined;

  readonly inputId = `cq-time-zone-${++nextTimeZoneInputId}`;
  readonly listId = `${this.inputId}-list`;
  value = '';
  disabled = false;

  get options(): readonly TimeZoneOption[] {
    if (this.optionCulture !== this.i18n.currentCulture) {
      this.optionCulture = this.i18n.currentCulture;
      this.cachedOptions = this.timeZones.options(this.optionCulture);
    }

    return this.cachedOptions;
  }

  writeValue(value: string | null): void {
    this.value = value ?? '';
  }

  registerOnChange(fn: (value: string) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(disabled: boolean): void {
    this.disabled = disabled;
  }

  handleInput(event: Event): void {
    this.value = (event.target as HTMLInputElement).value;
    this.onChange(this.value);
  }

  handleBlur(): void {
    this.onTouched();
  }
}
