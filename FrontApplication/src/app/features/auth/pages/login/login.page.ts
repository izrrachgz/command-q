import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { LucideDynamicIcon } from '@lucide/angular';

import { AuthFacade, AuthRealm, AuthRealms, sanitizeReturnUrl } from '../../../../core/auth/auth.facade';
import { AuthDiagnosticsService } from '../../../../core/auth/auth-diagnostics.service';
import { TextPipe } from '../../../../core/i18n/text.pipe';
import { TextKeys } from '../../../../core/i18n/text-keys';
import { AppRouteQueryParams } from '../../../../core/routing/app-route-constants';
import { BrandMarkComponent } from '../../../../shared/components';

@Component({
  selector: 'app-login-page',
  standalone: true,
  imports: [BrandMarkComponent, LucideDynamicIcon, RouterLink, TextPipe],
  templateUrl: './login.page.html',
})
export class LoginPage implements OnInit {
  private readonly auth = inject(AuthFacade);
  private readonly diagnostics = inject(AuthDiagnosticsService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly realm: AuthRealm = this.route.snapshot.queryParamMap.get(AppRouteQueryParams.realm) === AuthRealms.admin
    ? AuthRealms.admin
    : AuthRealms.tenant;
  readonly returnUrl = sanitizeReturnUrl(this.route.snapshot.queryParamMap.get(AppRouteQueryParams.returnUrl), this.realm);
  readonly loginTitleKey = this.realm === AuthRealms.admin ? TextKeys.auth.adminLogin : TextKeys.auth.tenantLogin;

  loading = false;
  error = '';

  constructor() {
    this.auth.setRealm(this.realm);
  }

  ngOnInit(): void {
    if (this.auth.snapshot.authenticated) {
      void this.router.navigateByUrl(this.returnUrl);
      return;
    }

    this.startLogin();
  }

  startLogin(): void {
    if (this.loading) {
      return;
    }

    this.loading = true;
    this.error = '';

    this.auth
      .login(this.realm, this.returnUrl)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        error: (error) => {
          const diagnostic = this.diagnostics.describe(error);
          this.error = diagnostic.status === 0 ? TextKeys.auth.identityUnavailable : TextKeys.auth.signInFailed;
        },
      });
  }
}
