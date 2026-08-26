import { Component, Input } from '@angular/core';

import { AdminAccount } from '../../models/admin-account.model';

@Component({
  selector: 'app-account-details',
  standalone: true,
  templateUrl: './account-details.component.html',
})
export class AccountDetailsComponent {
  @Input({ required: true }) account!: AdminAccount;
}
