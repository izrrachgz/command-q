import { Component, Input, OnChanges, Optional, Self, forwardRef } from '@angular/core';
import { AbstractControl, ControlValueAccessor, NG_VALIDATORS, NgControl, ValidationErrors, Validator } from '@angular/forms';

import { ValidationMessageComponent } from '../validation-message/validation-message.component';

export interface DateTimeRangeValue {
  readonly start: string;
  readonly end: string;
}

type DateTimeRangePart = keyof DateTimeRangeValue;

const EmptyRange: DateTimeRangeValue = { start: '', end: '' };

@Component({
  selector: 'app-date-time-range-input',
  standalone: true,
  imports: [ValidationMessageComponent],
  templateUrl: './date-time-range-input.component.html',
  providers: [
    {
      provide: NG_VALIDATORS,
      useExisting: forwardRef(() => DateTimeRangeInputComponent),
      multi: true,
    },
  ],
})
export class DateTimeRangeInputComponent implements ControlValueAccessor, Validator, OnChanges {
  @Input() label = '';
  @Input() startLabel = '';
  @Input() endLabel = '';
  @Input() min = '';
  @Input() max = '';
  @Input() required = false;

  readonly startInputId = `cq-date-time-range-start-${crypto.randomUUID()}`;
  readonly endInputId = `cq-date-time-range-end-${crypto.randomUUID()}`;

  value: DateTimeRangeValue = EmptyRange;
  disabled = false;

  private onChange: (value: DateTimeRangeValue | null) => void = () => undefined;
  private onTouched: () => void = () => undefined;
  private onValidatorChange: () => void = () => undefined;

  constructor(@Optional() @Self() public readonly ngControl: NgControl | null) {
    if (this.ngControl) {
      this.ngControl.valueAccessor = this;
    }
  }

  get showErrors(): boolean {
    const control = this.ngControl?.control;
    return !!control?.invalid && (control.dirty || control.touched);
  }

  ngOnChanges(): void {
    this.onValidatorChange();
  }

  writeValue(value: DateTimeRangeValue | null): void {
    this.value = normalizeRange(value);
  }

  registerOnChange(fn: (value: DateTimeRangeValue | null) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  registerOnValidatorChange(fn: () => void): void {
    this.onValidatorChange = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }

  validate(control: AbstractControl): ValidationErrors | null {
    const range = normalizeRange(control.value ?? this.value);
    const hasStart = range.start.length > 0;
    const hasEnd = range.end.length > 0;

    if (this.required && (!hasStart || !hasEnd)) {
      return { required: true };
    }

    if (hasStart !== hasEnd) {
      return { dateTimeRangeIncomplete: true };
    }

    if (hasStart && hasEnd && range.start > range.end) {
      return { dateTimeRangeOrder: true };
    }

    if (this.min && ((hasStart && range.start < this.min) || (hasEnd && range.end < this.min))) {
      return { min: { value: this.min } };
    }

    if (this.max && ((hasStart && range.start > this.max) || (hasEnd && range.end > this.max))) {
      return { max: { value: this.max } };
    }

    return null;
  }

  handleInput(part: DateTimeRangePart, event: Event): void {
    const nextValue = (event.target as HTMLInputElement).value;
    this.value = { ...this.value, [part]: nextValue };
    this.onChange(toControlValue(this.value));
    this.onValidatorChange();
  }

  markTouched(): void {
    this.onTouched();
  }
}

function normalizeRange(value: DateTimeRangeValue | null | undefined): DateTimeRangeValue {
  return {
    start: value?.start ?? EmptyRange.start,
    end: value?.end ?? EmptyRange.end,
  };
}

function toControlValue(value: DateTimeRangeValue): DateTimeRangeValue | null {
  return value.start || value.end ? value : null;
}
