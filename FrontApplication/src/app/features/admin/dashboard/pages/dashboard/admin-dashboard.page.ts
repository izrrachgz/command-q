import { Component } from '@angular/core';
import { LucideDynamicIcon } from '@lucide/angular';

import { TextPipe } from '../../../../../core/i18n/text.pipe';
import { PageHeaderComponent, StatCardComponent } from '../../../../../shared/components';

@Component({
  selector: 'app-admin-dashboard-page',
  standalone: true,
  imports: [LucideDynamicIcon, PageHeaderComponent, StatCardComponent, TextPipe],
  templateUrl: './admin-dashboard.page.html',
})
export class AdminDashboardPage {}
