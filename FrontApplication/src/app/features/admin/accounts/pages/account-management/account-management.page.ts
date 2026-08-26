import { AsyncPipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { ScrollingModule } from '@angular/cdk/scrolling';
import { LucideDynamicIcon } from '@lucide/angular';

import { I18nService } from '../../../../../core/i18n/i18n.service';
import { TextKeys } from '../../../../../core/i18n/text-keys';
import { TextPipe } from '../../../../../core/i18n/text.pipe';
import { AccountDatePipe } from '../../../../../core/time/account-date.pipe';
import { ReportFileNames } from '../../../../../core/reporting/report-constants';
import { ReportExportService } from '../../../../../core/reporting/report-export.service';
import {
  ConfirmationDialogComponent,
  EmptyStateComponent,
  PageHeaderComponent,
} from '../../../../../shared/components';
import { TextInputComponent } from '../../../../../shared/forms';
import {
  trackByEntityId,
  virtualTableBufferPixels,
  virtualTableViewportHeight,
  VirtualTableRowHeight,
} from '../../../../../shared/tables';
import { AdminAccount } from '../../models/admin-account.model';
import { AdminAccountsActions } from '../../actions/admin-accounts.actions';
import { AdminAccountsStore } from '../../stores/admin-accounts.store';
import { AccountDetailsComponent } from '../../components/account-details/account-details.component';

@Component({
  selector: 'app-account-management-page',
  standalone: true,
  imports: [
    AsyncPipe,
    AccountDatePipe,
    AccountDetailsComponent,
    ConfirmationDialogComponent,
    EmptyStateComponent,
    LucideDynamicIcon,
    PageHeaderComponent,
    ReactiveFormsModule,
    ScrollingModule,
    TextInputComponent,
    TextPipe,
  ],
  templateUrl: './account-management.page.html',
})
export class AccountManagementPage {
  readonly store = inject(AdminAccountsStore);
  readonly trackByEntityId = trackByEntityId;
  readonly virtualTableBufferPixels = virtualTableBufferPixels;
  readonly virtualTableViewportHeight = virtualTableViewportHeight;
  readonly virtualTableRowHeight = VirtualTableRowHeight;
  accountDetailsOpen = false;

  private readonly actions = inject(AdminAccountsActions);
  private readonly fb = inject(FormBuilder);
  private readonly i18n = inject(I18nService);
  private readonly reports = inject(ReportExportService);

  readonly filterForm = this.fb.nonNullable.group({
    searchTerm: [''],
  });

  constructor() {
    this.load();
  }

  load(): void {
    this.actions.load().subscribe();
  }

  search(): void {
    this.actions.applySearch(this.filterForm.controls.searchTerm.value).subscribe();
  }

  select(account: AdminAccount): void {
    this.accountDetailsOpen = true;
    this.actions.select(account).subscribe();
  }

  exportAccounts(accounts: readonly AdminAccount[]): void {
    const rows = accounts.map((account) => ({ ...account }));

    void this.reports.exportXlsx(
      ReportFileNames.accounts,
      this.i18n.text(TextKeys.report.accountsSheetName),
      rows,
      [
        { header: this.i18n.text(TextKeys.admin.accounts.username), key: 'username', width: 24 },
        { header: this.i18n.text(TextKeys.admin.accounts.email), key: 'email', width: 32 },
        { header: this.i18n.text(TextKeys.admin.accounts.active), key: 'active', width: 12 },
        { header: this.i18n.text(TextKeys.admin.accounts.locked), key: 'locked', width: 12 },
      ],
    );
  }
}
