import { AsyncPipe, DatePipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { LucideDynamicIcon } from '@lucide/angular';

import { I18nService } from '../../../../../core/i18n/i18n.service';
import { TextKeys } from '../../../../../core/i18n/text-keys';
import { TextPipe } from '../../../../../core/i18n/text.pipe';
import { ReportFileNames } from '../../../../../core/reporting/report-constants';
import { ReportExportService } from '../../../../../core/reporting/report-export.service';
import { EmptyStateComponent, PageHeaderComponent } from '../../../../../shared/components';
import { PasswordInputComponent, TextInputComponent } from '../../../../../shared/forms';
import { AdminAccount } from '../../models/admin-account.model';
import { AdminAccountsActions } from '../../actions/admin-accounts.actions';
import { AdminAccountsStore } from '../../stores/admin-accounts.store';

@Component({
  selector: 'app-account-management-page',
  standalone: true,
  imports: [
    AsyncPipe,
    DatePipe,
    EmptyStateComponent,
    LucideDynamicIcon,
    PageHeaderComponent,
    PasswordInputComponent,
    ReactiveFormsModule,
    TextInputComponent,
    TextPipe,
  ],
  templateUrl: './account-management.page.html',
})
export class AccountManagementPage {
  readonly store = inject(AdminAccountsStore);

  private readonly actions = inject(AdminAccountsActions);
  private readonly fb = inject(FormBuilder);
  private readonly i18n = inject(I18nService);
  private readonly reports = inject(ReportExportService);

  readonly filterForm = this.fb.nonNullable.group({
    searchTerm: [''],
  });

  readonly createForm = this.fb.nonNullable.group({
    username: ['', [Validators.required, Validators.maxLength(255)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(255)]],
    temporaryPassword: ['', [Validators.required, Validators.minLength(12), Validators.maxLength(255)]],
    sendInvite: [true],
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
    this.actions.select(account).subscribe();
  }

  lock(account: AdminAccount): void {
    this.actions.lock(account).subscribe();
  }

  activate(account: AdminAccount): void {
    this.actions.activate(account).subscribe();
  }

  create(commandRunning: boolean): void {
    if (commandRunning || this.createForm.invalid) {
      this.createForm.markAllAsTouched();
      return;
    }

    this.actions.create(this.createForm.getRawValue()).subscribe({
      complete: () => this.createForm.reset({ username: '', email: '', temporaryPassword: '', sendInvite: true }),
    });
  }

  exportAccounts(accounts: readonly AdminAccount[]): void {
    const rows = accounts.map((account) => ({ ...account }));

    void this.reports.exportXlsx(ReportFileNames.accounts, this.i18n.text(TextKeys.report.accountsSheetName), rows, [
      { header: this.i18n.text(TextKeys.admin.accounts.username), key: 'username', width: 24 },
      { header: this.i18n.text(TextKeys.admin.accounts.email), key: 'email', width: 32 },
      { header: this.i18n.text(TextKeys.admin.accounts.active), key: 'active', width: 12 },
      { header: this.i18n.text(TextKeys.admin.accounts.locked), key: 'locked', width: 12 },
    ]);
  }
}
