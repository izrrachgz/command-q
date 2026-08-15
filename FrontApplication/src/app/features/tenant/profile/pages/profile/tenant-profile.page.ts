import { AsyncPipe } from '@angular/common';
import { Component } from '@angular/core';
import { LucideDynamicIcon } from '@lucide/angular';

import { AuthFacade } from '../../../../../core/auth/auth.facade';
import { TextPipe } from '../../../../../core/i18n/text.pipe';
import { PageHeaderComponent } from '../../../../../shared/components';

@Component({
  selector: 'app-tenant-profile-page',
  standalone: true,
  imports: [AsyncPipe, LucideDynamicIcon, PageHeaderComponent, TextPipe],
  templateUrl: './tenant-profile.page.html',
})
export class TenantProfilePage {
  constructor(readonly auth: AuthFacade) {}
}
