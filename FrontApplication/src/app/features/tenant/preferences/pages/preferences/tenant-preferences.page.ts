import { Component } from '@angular/core';

import { TextPipe } from '../../../../../core/i18n/text.pipe';
import { PreferencesFormComponent } from '../../../../preferences/components/preferences-form/preferences-form.component';
import { PageHeaderComponent } from '../../../../../shared/components';

@Component({
  selector: 'app-tenant-preferences-page',
  standalone: true,
  imports: [PageHeaderComponent, PreferencesFormComponent, TextPipe],
  templateUrl: './tenant-preferences.page.html',
})
export class TenantPreferencesPage {}
