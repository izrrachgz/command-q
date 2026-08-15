import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { finalize, take } from 'rxjs';
import { LucideDynamicIcon } from '@lucide/angular';

import { AccountProvisionService } from '../../../../core/auth/account-provision.service';
import { AuthFacade, AuthRealm, AuthRealms, sanitizeReturnUrl } from '../../../../core/auth/auth.facade';
import { TextPipe } from '../../../../core/i18n/text.pipe';
import { FrontendLoggerService } from '../../../../core/logging/frontend-logger.service';
import { FrontendLogMessages } from '../../../../core/logging/frontend-log-messages';
import { AppRouteQueryParams } from '../../../../core/routing/app-route-constants';

@Component({
  selector: 'app-provision-page',
  standalone: true,
  imports: [LucideDynamicIcon, TextPipe],
  templateUrl: './provision.page.html',
})
export class ProvisionPage implements OnInit {
  loading = false;
  failed = false;
  private readonly returnUrl: string;

  constructor(
    route: ActivatedRoute,
    private readonly auth: AuthFacade,
    private readonly provisionService: AccountProvisionService,
    private readonly router: Router,
    private readonly logger: FrontendLoggerService,
  ) {
    const realm: AuthRealm = route.snapshot.queryParamMap.get(AppRouteQueryParams.realm) === AuthRealms.admin
      ? AuthRealms.admin
      : this.auth.snapshot.realm;
    this.auth.setRealm(realm);
    this.returnUrl = sanitizeReturnUrl(route.snapshot.queryParamMap.get(AppRouteQueryParams.returnUrl), realm);
  }

  ngOnInit(): void {
    if (this.auth.snapshot.account) {
      this.redirectToTarget();
      return;
    }

    this.provision();
  }

  provision(): void {
    if (this.loading) {
      return;
    }

    this.loading = true;
    this.failed = false;

    this.provisionService
      .provisionOrThrow()
      .pipe(
        take(1),
        finalize(() => (this.loading = false)),
      )
      .subscribe({
        next: () => this.redirectToTarget(),
        error: (error) => {
          this.failed = true;
          this.logger.error(FrontendLogMessages.provisionPageFailed, error);
        },
      });
  }

  private redirectToTarget(): void {
    void this.router.navigateByUrl(this.returnUrl, { replaceUrl: true });
  }
}
