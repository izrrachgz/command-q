import { Component, Input, Optional, Self } from '@angular/core';
import { ControlValueAccessor, NgControl } from '@angular/forms';

import { ValidationMessageComponent } from '../validation-message/validation-message.component';

@Component({
  selector: 'app-number-input',
  standalone: true,
  imports: [ValidationMessageComponent],
  templateUrl: './number-input.component.html',
})
export class NumberInputComponent implements ControlValueAccessor {
  @Input() label = '';
  @Input() placeholder = '';
  @Input() min?: number;
  @Input() max?: number;
  @Input() step?: number;

  readonly inputId = `cq-number-${crypto.randomUUID()}`;
  value: number | null = null;
  disabled = false;

  private onChange: (value: number | null) => void = () => undefined;
  private onTouched: () => void = () => undefined;

  constructor(@Optional() @Self() public readonly ngControl: NgControl | null) {
    if (this.ngControl) {
      this.ngControl.valueAccessor = this;
    }
  }

  get showErrors(): boolean {
    const control = this.ngControl?.control;
    return !!control?.invalid && (control.dirty || control.touched);
  }

  writeValue(value: number | null): void {
    this.value = value;
  }

  registerOnChange(fn: (value: number | null) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }

  handleInput(event: Event): void {
    const rawValue = (event.target as HTMLInputElement).value;
    this.value = rawValue === '' ? null : Number(rawValue);
    this.onChange(this.value);
  }

  markTouched(): void {
    this.onTouched();
  }
}
