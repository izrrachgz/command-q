import { AsyncPipe, DatePipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { LucideDynamicIcon } from '@lucide/angular';

import { I18nService } from '../../../../../core/i18n/i18n.service';
import { TextKeys } from '../../../../../core/i18n/text-keys';
import { TextPipe } from '../../../../../core/i18n/text.pipe';
import { ReportFileNames } from '../../../../../core/reporting/report-constants';
import { ReportExportService } from '../../../../../core/reporting/report-export.service';
import { EmptyStateComponent, PageHeaderComponent } from '../../../../../shared/components';
import { DateInputComponent, TextInputComponent } from '../../../../../shared/forms';
import { AdminEmailActions } from '../../actions/admin-email.actions';
import { AdminEmailRecord } from '../../models/admin-email.model';
import { AdminEmailStore } from '../../stores/admin-email.store';

@Component({
  selector: 'app-email-management-page',
  standalone: true,
  imports: [
    AsyncPipe,
    DateInputComponent,
    DatePipe,
    EmptyStateComponent,
    LucideDynamicIcon,
    PageHeaderComponent,
    ReactiveFormsModule,
    TextInputComponent,
    TextPipe,
  ],
  templateUrl: './email-management.page.html',
})
export class EmailManagementPage {
  readonly store = inject(AdminEmailStore);

  private readonly actions = inject(AdminEmailActions);
  private readonly fb = inject(FormBuilder);
  private readonly i18n = inject(I18nService);
  private readonly reports = inject(ReportExportService);

  readonly filterForm = this.fb.nonNullable.group({
    searchTerm: [''],
    recipient: [''],
    startDate: [''],
    endDate: [''],
  });

  constructor() {
    this.load();
  }

  load(): void {
    this.actions.load().subscribe();
  }

  search(): void {
    this.actions.load(this.filterForm.getRawValue()).subscribe();
  }

  exportRecords(records: readonly AdminEmailRecord[]): void {
    const rows = records.map((record) => ({ ...record }));

    void this.reports.exportXlsx(ReportFileNames.email, this.i18n.text(TextKeys.admin.email.sheetName), rows, [
      { header: this.i18n.text(TextKeys.admin.email.subject), key: 'subject', width: 36 },
      { header: this.i18n.text(TextKeys.admin.email.recipient), key: 'recipient', width: 34 },
      { header: this.i18n.text(TextKeys.admin.email.status), key: 'status', width: 18 },
      { header: this.i18n.text(TextKeys.admin.email.attempts), key: 'attempts', width: 12 },
    ]);
  }
}
