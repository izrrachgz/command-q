import { Component, inject } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { LucideDynamicIcon } from '@lucide/angular';

import { AuthFacade, AuthRealms } from '../../../../core/auth/auth.facade';
import { TextPipe } from '../../../../core/i18n/text.pipe';
import { TextKeys } from '../../../../core/i18n/text-keys';
import { AppRouteQueryParams } from '../../../../core/routing/app-route-constants';
import { AppRoutePaths } from '../../../../core/routing/app-route-paths';
import { BrandMarkComponent } from '../../../../shared/components';
import { PasswordInputComponent, TextInputComponent } from '../../../../shared/forms';

@Component({
  selector: 'app-register-page',
  standalone: true,
  imports: [BrandMarkComponent, LucideDynamicIcon, PasswordInputComponent, ReactiveFormsModule, RouterLink, TextInputComponent, TextPipe],
  templateUrl: './register.page.html',
})
export class RegisterPage {
  private readonly auth = inject(AuthFacade);
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);

  readonly form = this.fb.nonNullable.group(
    {
      username: ['', [Validators.required, Validators.maxLength(255)]],
      email: ['', [Validators.required, Validators.email, Validators.maxLength(255)]],
      password: ['', [Validators.required, Validators.minLength(12), Validators.maxLength(255)]],
      confirmPassword: ['', [Validators.required, Validators.maxLength(255)]],
    },
    { validators: passwordMatchValidator },
  );

  loading = false;
  error = '';

  submit(): void {
    if (this.form.invalid || this.loading) {
      this.form.markAllAsTouched();
      return;
    }

    const { username, email, password } = this.form.getRawValue();
    this.loading = true;
    this.error = '';

    this.auth
      .register({ username, email, password })
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: () =>
          this.router.navigate([AppRoutePaths.auth.login], {
            queryParams: { [AppRouteQueryParams.realm]: AuthRealms.tenant },
          }),
        error: () => {
          this.error = TextKeys.auth.registrationFailed;
        },
      });
  }
}

const passwordMatchValidator = (control: AbstractControl): ValidationErrors | null => {
  const password = control.get('password')?.value;
  const confirmPassword = control.get('confirmPassword')?.value;

  return password && confirmPassword && password !== confirmPassword ? { passwordMismatch: true } : null;
};
