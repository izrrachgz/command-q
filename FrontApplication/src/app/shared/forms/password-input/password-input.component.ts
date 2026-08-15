import { Component, Input, Optional, Self } from '@angular/core';
import { ControlValueAccessor, NgControl } from '@angular/forms';
import { LucideDynamicIcon } from '@lucide/angular';

import { ValidationMessageComponent } from '../validation-message/validation-message.component';

@Component({
  selector: 'app-password-input',
  standalone: true,
  imports: [LucideDynamicIcon, ValidationMessageComponent],
  templateUrl: './password-input.component.html',
})
export class PasswordInputComponent implements ControlValueAccessor {
  @Input() label = '';
  @Input() placeholder = '';
  @Input() autocomplete = 'current-password';

  readonly inputId = `cq-password-${crypto.randomUUID()}`;
  value = '';
  visible = false;
  disabled = false;

  private onChange: (value: string) => void = () => undefined;
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

  writeValue(value: string | null): void {
    this.value = value ?? '';
  }

  registerOnChange(fn: (value: string) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }

  handleInput(event: Event): void {
    this.value = (event.target as HTMLInputElement).value;
    this.onChange(this.value);
  }

  markTouched(): void {
    this.onTouched();
  }
}
