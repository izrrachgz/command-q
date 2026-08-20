import { Component, inject, Input } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';

import { TextPipe } from '../../../../../core/i18n/text.pipe';
import { PasswordInputComponent, TextInputComponent } from '../../../../../shared/forms';
import { CreateAdminAccountRequest } from '../../models/admin-account.model';

@Component({
  selector: 'app-create-account-form',
  standalone: true,
  imports: [PasswordInputComponent, ReactiveFormsModule, TextInputComponent, TextPipe],
  templateUrl: './create-account-form.component.html',
})
export class CreateAccountFormComponent {
  private readonly fb = inject(FormBuilder);

  readonly form = this.fb.nonNullable.group({
    username: ['', [Validators.required, Validators.maxLength(255)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(255)]],
    temporaryPassword: [
      '',
      [Validators.required, Validators.minLength(12), Validators.maxLength(255)],
    ],
    sendInvite: [true],
  });

  @Input()
  set disabled(value: boolean) {
    if (value) {
      this.form.disable({ emitEvent: false });
    } else {
      this.form.enable({ emitEvent: false });
    }
  }

  get invalid(): boolean {
    return this.form.invalid;
  }

  request(): CreateAdminAccountRequest | null {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return null;
    }

    return this.form.getRawValue();
  }

  reset(): void {
    this.form.reset({ username: '', email: '', temporaryPassword: '', sendInvite: true });
  }
}
