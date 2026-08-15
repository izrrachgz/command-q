import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { LucideDynamicIcon } from '@lucide/angular';

import { AuthRealms } from '../../../../core/auth/auth.facade';
import { TextPipe } from '../../../../core/i18n/text.pipe';
import { AppRouteQueryParams } from '../../../../core/routing/app-route-constants';
import { AppRoutePaths } from '../../../../core/routing/app-route-paths';
import { BrandMarkComponent } from '../../../../shared/components';

@Component({
  selector: 'app-landing-page',
  standalone: true,
  imports: [BrandMarkComponent, LucideDynamicIcon, RouterLink, TextPipe],
  templateUrl: './landing.page.html',
})
export class LandingPage {
  readonly links = {
    adminLogin: [AppRoutePaths.auth.login],
    register: [AppRoutePaths.auth.register],
    tenantLogin: [AppRoutePaths.auth.login],
  } as const;

  readonly queryParams = {
    admin: {
      [AppRouteQueryParams.realm]: AuthRealms.admin,
      [AppRouteQueryParams.returnUrl]: AppRoutePaths.admin.dashboard,
    },
    tenant: {
      [AppRouteQueryParams.realm]: AuthRealms.tenant,
      [AppRouteQueryParams.returnUrl]: AppRoutePaths.tenant.dashboard,
    },
  } as const;
}
