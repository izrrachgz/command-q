import { Component, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import {
  AuthFacade,
  AuthRealm,
  AuthRealms,
  sanitizeReturnUrl,
} from '../../../../core/auth/auth.facade';
import { TextPipe } from '../../../../core/i18n/text.pipe';
import { AppRouteQueryParams } from '../../../../core/routing/app-route-constants';
import { BrandMarkComponent } from '../../../../shared/components';
import { PreferencesFormComponent } from '../../../preferences/components/preferences-form/preferences-form.component';

@Component({
  selector: 'app-setup-preferences-page',
  standalone: true,
  imports: [BrandMarkComponent, PreferencesFormComponent, TextPipe],
  templateUrl: './setup-preferences.page.html',
})
export class SetupPreferencesPage {
  private readonly auth = inject(AuthFacade);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  private readonly realm: AuthRealm =
    this.route.snapshot.queryParamMap.get(AppRouteQueryParams.realm) === AuthRealms.admin
      ? AuthRealms.admin
      : AuthRealms.tenant;
  private readonly returnUrl = sanitizeReturnUrl(
    this.route.snapshot.queryParamMap.get(AppRouteQueryParams.returnUrl),
    this.realm,
  );

  constructor() {
    this.auth.setRealm(this.realm);
  }

  complete(): void {
    void this.router.navigateByUrl(this.returnUrl, { replaceUrl: true });
  }
}
