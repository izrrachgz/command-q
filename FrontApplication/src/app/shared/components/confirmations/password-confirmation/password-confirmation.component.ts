import { Component, EventEmitter, Input, Output } from '@angular/core';

import { TextPipe } from '../../../../core/i18n/text.pipe';
import {
  ConfirmationCancelCallback,
  ConfirmationFailureCallback,
  ConfirmationSuccessCallback,
  ConfirmationValueAction,
} from '../confirmation.types';
import { ConfirmationDialogComponent } from '../confirmation-dialog/confirmation-dialog.component';

@Component({
  selector: 'app-password-confirmation',
  standalone: true,
  imports: [ConfirmationDialogComponent, TextPipe],
  templateUrl: './password-confirmation.component.html',
})
export class PasswordConfirmationComponent {
  @Input() open = false;
  @Input() title = 'confirmation.title';
  @Input() message = '';
  @Input() passwordLabel = 'field.password';
  @Input() autocomplete = 'current-password';
  @Input() confirmLabel = 'action.confirm';
  @Input() cancelLabel = 'action.cancel';
  @Input() icon = 'lock-keyhole';
  @Input() destructive = false;
  @Input() confirmAction?: ConfirmationValueAction;
  @Input() onSuccess?: ConfirmationSuccessCallback;
  @Input() onCancel?: ConfirmationCancelCallback;
  @Input() onFailure?: ConfirmationFailureCallback;

  @Output() openChange = new EventEmitter<boolean>();
  @Output() cancelled = new EventEmitter<void>();
  @Output() confirmed = new EventEmitter<void>();
  @Output() failed = new EventEmitter<unknown>();

  readonly passwordInputId = `cq-confirm-password-${crypto.randomUUID()}`;
  password = '';

  readonly runConfirmAction = () => this.confirmAction?.(this.password);

  get hasPassword(): boolean {
    return this.password.trim().length > 0;
  }

  handleInput(event: Event): void {
    this.password = (event.target as HTMLInputElement).value;
  }

  handleOpenChange(open: boolean): void {
    if (!open) {
      this.password = '';
    }

    this.openChange.emit(open);
  }
}
