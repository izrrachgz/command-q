import { AsyncPipe } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { SupportedCulture } from '../../../../../core/config/app-environment';
import { I18nService } from '../../../../../core/i18n/i18n.service';
import { TextPipe } from '../../../../../core/i18n/text.pipe';
import { PreferencesService, UserTheme, UserThemes } from '../../../../../core/preferences/preferences.service';
import { PageHeaderComponent } from '../../../../../shared/components';

@Component({
  selector: 'app-admin-preferences-page',
  standalone: true,
  imports: [AsyncPipe, FormsModule, PageHeaderComponent, TextPipe],
  templateUrl: './admin-preferences.page.html',
})
export class AdminPreferencesPage {
  readonly themes = UserThemes;

  constructor(
    readonly i18n: I18nService,
    readonly preferences: PreferencesService,
  ) {}

  setCulture(culture: SupportedCulture): void {
    this.i18n.setCulture(culture);
  }

  setTheme(enabled: boolean): void {
    const theme: UserTheme = enabled ? UserThemes.dark : UserThemes.light;
    this.preferences.update({ theme });
  }
}
