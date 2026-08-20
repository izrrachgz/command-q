import { Pipe, PipeTransform, inject } from '@angular/core';

import { I18nService } from '../i18n/i18n.service';
import { PreferencesService } from '../preferences/preferences.service';
import { ensureUtcSuffix } from './date-time-utc';

const DateTimeFormatters = new Map<string, Intl.DateTimeFormat>();

@Pipe({
  name: 'accountDate',
  standalone: true,
  pure: false,
})
export class AccountDatePipe implements PipeTransform {
  private readonly i18n = inject(I18nService);
  private readonly preferences = inject(PreferencesService);

  transform(value: string | Date | null | undefined): string {
    if (!value) {
      return '';
    }

    const date = value instanceof Date ? value : new Date(ensureUtcSuffix(value));
    if (Number.isNaN(date.getTime())) {
      return '';
    }

    const culture = this.i18n.currentCulture;
    const timeZone = this.preferences.snapshot.timeZone;
    const formatterKey = `${culture}|${timeZone}`;
    let formatter = DateTimeFormatters.get(formatterKey);

    if (!formatter) {
      formatter = new Intl.DateTimeFormat(culture, {
        dateStyle: 'medium',
        timeStyle: 'medium',
        timeZone,
      });
      DateTimeFormatters.set(formatterKey, formatter);
    }

    return formatter.format(date);
  }
}
