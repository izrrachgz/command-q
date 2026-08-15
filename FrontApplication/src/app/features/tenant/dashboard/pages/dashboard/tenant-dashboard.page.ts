import { AsyncPipe, DatePipe } from '@angular/common';
import { Component } from '@angular/core';
import { catchError, finalize, of } from 'rxjs';
import { LucideDynamicIcon } from '@lucide/angular';

import { AccountApiService } from '../../../../../core/api/account-api.service';
import { ApiSortFields } from '../../../../../core/api/api-constants';
import { AuthFacade } from '../../../../../core/auth/auth.facade';
import { TextPipe } from '../../../../../core/i18n/text.pipe';
import { AccountActivityModel } from '../../../../../core/models/account.model';
import { EmptyStateComponent, PageHeaderComponent, StatCardComponent } from '../../../../../shared/components';

@Component({
  selector: 'app-tenant-dashboard-page',
  standalone: true,
  imports: [AsyncPipe, DatePipe, EmptyStateComponent, LucideDynamicIcon, PageHeaderComponent, StatCardComponent, TextPipe],
  templateUrl: './tenant-dashboard.page.html',
})
export class TenantDashboardPage {
  activity: readonly AccountActivityModel[] = [];
  loading = false;

  constructor(
    readonly auth: AuthFacade,
    private readonly accountApi: AccountApiService,
  ) {
    this.loadActivity();
  }

  get addressCount(): string {
    return String(Object.keys(this.auth.snapshot.account?.addresses ?? {}).length);
  }

  get contactCount(): string {
    return String(Object.keys(this.auth.snapshot.account?.contacts ?? {}).length);
  }

  loadActivity(): void {
    this.loading = true;
    this.accountApi
      .activity({ page: 1, size: 10, sortBy: ApiSortFields.createdUtc, ascendingSort: false })
      .pipe(
        catchError(() => of({ collection: [], page: 1, pageSize: 10, total: 0, totalPages: 0 })),
        finalize(() => (this.loading = false)),
      )
      .subscribe((response) => {
        this.activity = response.collection;
      });
  }
}
