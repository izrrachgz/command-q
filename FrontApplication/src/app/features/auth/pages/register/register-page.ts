import { Component } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '@core/auth';

@Component({
  selector: 'app-register-page',
  standalone: false,
  templateUrl: './register-page.html',
  styleUrl: './register-page.css',
})
export class RegisterPage {
  form: FormGroup;

  loading = false;

  showPassword = false;

  rules = {
    lowercase: false,
    uppercase: false,
    number: false,
    symbol: false,
    length: false,
  };

  passwordStrength = 0;
  strengthColor = 'bg-red-500';

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute,
  ) {
    this.form = this.fb.group(
      {
        username: ['', Validators.required],
        email: ['', [Validators.required, Validators.email]],
        password: ['', Validators.required],
        confirmPassword: ['', Validators.required],
      },
      { validators: this.passwordMatchValidator },
    );
  }

  passwordMatchValidator(group: AbstractControl) {
    const password = group.get('password')?.value;
    const confirm = group.get('confirmPassword')?.value;

    return password === confirm ? null : { passwordMismatch: true };
  }

  checkPassword() {
    const value = this.form.controls['password'].value || '';

    this.rules = {
      lowercase: /[a-z]/.test(value),
      uppercase: /[A-Z]/.test(value),
      number: /\d/.test(value),
      symbol: /[\W_]/.test(value),
      length: value.length >= 8,
    };

    const score = Object.values(this.rules).filter((v) => v).length;

    this.passwordStrength = (score / 5) * 100;

    if (score <= 2) this.strengthColor = 'bg-red-500';
    else if (score <= 4) this.strengthColor = 'bg-yellow-500';
    else this.strengthColor = 'bg-green-500';
  }

  submit(): void {
    if (this.form.invalid) return;

    this.loading = true;

    this.authService.register(this.form.value as any).subscribe({
      next: () => {
        const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl') || '/dashboard';

        this.router.navigateByUrl(returnUrl);
      },
      error: () => {
        this.loading = false;
      },
    });
  }
}
