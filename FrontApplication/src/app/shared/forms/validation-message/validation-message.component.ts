import { Component, Input } from '@angular/core';
import { ValidationErrors } from '@angular/forms';

import { I18nService } from '../../../core/i18n/i18n.service';
import { TextKeys } from '../../../core/i18n/text-keys';

@Component({
  selector: 'app-validation-message',
  standalone: true,
  templateUrl: './validation-message.component.html',
})
export class ValidationMessageComponent {
  @Input() errors: ValidationErrors | null | undefined;
  @Input() show = false;

  constructor(private readonly i18n: I18nService) {}

  get message(): string {
    const errors = this.errors;
    if (!errors) {
      return '';
    }

    if (errors['required']) {
      return this.i18n.text(TextKeys.validation.required);
    }

    if (errors['email']) {
      return this.i18n.text(TextKeys.validation.email);
    }

    if (errors['minlength']) {
      return this.i18n.text(TextKeys.validation.minlength, errors['minlength']);
    }

    if (errors['maxlength']) {
      return this.i18n.text(TextKeys.validation.maxlength, errors['maxlength']);
    }

    if (errors['min']) {
      return this.i18n.text(TextKeys.validation.min, errors['min']);
    }

    if (errors['max']) {
      return this.i18n.text(TextKeys.validation.max, errors['max']);
    }

    if (errors['passwordMismatch']) {
      return this.i18n.text(TextKeys.validation.passwordMismatch);
    }

    if (errors['dateTimeRangeIncomplete']) {
      return this.i18n.text(TextKeys.validation.dateTimeRangeIncomplete);
    }

    if (errors['dateTimeRangeOrder']) {
      return this.i18n.text(TextKeys.validation.dateTimeRangeOrder);
    }

    return this.i18n.text(TextKeys.validation.pattern);
  }
}
