import { AsyncPipe } from '@angular/common';
import { Component } from '@angular/core';
import { ScrollingModule } from '@angular/cdk/scrolling';
import { catchError, finalize, of } from 'rxjs';
import { LucideDynamicIcon } from '@lucide/angular';

import { AccountApiService } from '../../../../../core/api/account-api.service';
import { ApiSortFields } from '../../../../../core/api/api-constants';
import { AuthFacade } from '../../../../../core/auth/auth.facade';
import { TextPipe } from '../../../../../core/i18n/text.pipe';
import { AccountActivityModel } from '../../../../../core/models/account.model';
import { AccountDatePipe } from '../../../../../core/time/account-date.pipe';
import {
  EmptyStateComponent,
  PageHeaderComponent,
  StatCardComponent,
} from '../../../../../shared/components';
import {
  trackByEntityId,
  virtualTableBufferPixels,
  virtualTableViewportHeight,
  VirtualTableRowHeight,
} from '../../../../../shared/tables';

@Component({
  selector: 'app-tenant-dashboard-page',
  standalone: true,
  imports: [
    AccountDatePipe,
    AsyncPipe,
    EmptyStateComponent,
    LucideDynamicIcon,
    PageHeaderComponent,
    ScrollingModule,
    StatCardComponent,
    TextPipe,
  ],
  templateUrl: './tenant-dashboard.page.html',
})
export class TenantDashboardPage {
  readonly activityPageSize = 10;
  readonly trackByEntityId = trackByEntityId;
  readonly virtualTableBufferPixels = virtualTableBufferPixels;
  readonly virtualTableViewportHeight = virtualTableViewportHeight;
  readonly virtualTableRowHeight = VirtualTableRowHeight;
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
      .activity({
        page: 1,
        size: this.activityPageSize,
        sortBy: ApiSortFields.createdUtc,
        ascendingSort: false,
      })
      .pipe(
        catchError(() =>
          of({
            items: [],
            page: 1,
            pageSize: this.activityPageSize,
            total: 0,
            totalPages: 0,
          }),
        ),
        finalize(() => (this.loading = false)),
      )
      .subscribe((response) => {
        this.activity = response.items;
      });
  }
}
