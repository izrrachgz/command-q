import { Component, Input } from '@angular/core';

import { TextPipe } from '../../../../../core/i18n/text.pipe';
import { AccountActivityModel } from '../../../../../core/models/account.model';
import { AdminAccount } from '../../models/admin-account.model';

@Component({
  selector: 'app-account-details',
  standalone: true,
  imports: [TextPipe],
  templateUrl: './account-details.component.html',
})
export class AccountDetailsComponent {
  @Input({ required: true }) account!: AdminAccount;
  @Input() activity: readonly AccountActivityModel[] = [];
  @Input() loading = false;
}
