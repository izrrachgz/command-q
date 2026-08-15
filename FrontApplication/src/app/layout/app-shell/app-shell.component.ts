import { NgClass } from '@angular/common';
import { Component, inject } from '@angular/core';
import { ActivatedRoute, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { LucideDynamicIcon } from '@lucide/angular';

import { AuthFacade } from '../../core/auth/auth.facade';
import { SupportedCulture } from '../../core/config/app-environment';
import { I18nService } from '../../core/i18n/i18n.service';
import { TextPipe } from '../../core/i18n/text.pipe';
import { TextKeys } from '../../core/i18n/text-keys';
import { AppRouteAreas, AppRouteArea, AppRouteDataKeys } from '../../core/routing/app-route-constants';
import { AppRoutePaths } from '../../core/routing/app-route-paths';
import { BrandMarkComponent } from '../../shared/components';

interface NavigationItem {
  readonly labelKey: string;
  readonly icon: string;
  readonly route: readonly string[];
}

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [BrandMarkComponent, LucideDynamicIcon, NgClass, RouterLink, RouterLinkActive, RouterOutlet, TextPipe],
  templateUrl: './app-shell.component.html',
})
export class AppShellComponent {
  readonly auth = inject(AuthFacade);
  readonly i18n = inject(I18nService);

  private readonly route = inject(ActivatedRoute);

  readonly session$ = this.auth.session$;
  readonly area = (this.route.snapshot.data[AppRouteDataKeys.area] as AppRouteArea | undefined) ?? AppRouteAreas.tenant;
  readonly tenantNavigation: readonly NavigationItem[] = [
    { labelKey: TextKeys.nav.dashboard, icon: 'layout-dashboard', route: [AppRoutePaths.tenant.dashboard] },
    { labelKey: TextKeys.nav.profile, icon: 'user', route: [AppRoutePaths.tenant.profile] },
    { labelKey: TextKeys.nav.preferences, icon: 'settings', route: [AppRoutePaths.tenant.preferences] },
  ];
  readonly adminNavigation: readonly NavigationItem[] = [
    { labelKey: TextKeys.nav.dashboard, icon: 'layout-dashboard', route: [AppRoutePaths.admin.dashboard] },
    { labelKey: TextKeys.nav.accounts, icon: 'users-round', route: [AppRoutePaths.admin.accounts] },
    { labelKey: TextKeys.nav.email, icon: 'mail', route: [AppRoutePaths.admin.email] },
    { labelKey: TextKeys.nav.preferences, icon: 'settings', route: [AppRoutePaths.admin.preferences] },
  ];

  mobileNavOpen = false;

  get navigation(): readonly NavigationItem[] {
    return this.area === AppRouteAreas.admin ? this.adminNavigation : this.tenantNavigation;
  }

  get areaLabel(): string {
    return this.i18n.text(this.area === AppRouteAreas.admin ? TextKeys.nav.admin : TextKeys.nav.tenant);
  }

  changeCulture(event: Event): void {
    this.i18n.setCulture((event.target as HTMLSelectElement).value as SupportedCulture);
  }
}
