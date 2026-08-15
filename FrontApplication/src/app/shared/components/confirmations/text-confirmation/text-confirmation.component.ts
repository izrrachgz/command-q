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
  selector: 'app-text-confirmation',
  standalone: true,
  imports: [ConfirmationDialogComponent, TextPipe],
  templateUrl: './text-confirmation.component.html',
})
export class TextConfirmationComponent {
  @Input() open = false;
  @Input() title = 'confirmation.title';
  @Input() message = '';
  @Input() textLabel = 'confirmation.textLabel';
  @Input() expectedText = '';
  @Input() caseSensitive = false;
  @Input() confirmLabel = 'action.confirm';
  @Input() cancelLabel = 'action.cancel';
  @Input() icon = 'circle-alert';
  @Input() destructive = false;
  @Input() confirmAction?: ConfirmationValueAction;
  @Input() onSuccess?: ConfirmationSuccessCallback;
  @Input() onCancel?: ConfirmationCancelCallback;
  @Input() onFailure?: ConfirmationFailureCallback;

  @Output() openChange = new EventEmitter<boolean>();
  @Output() cancelled = new EventEmitter<void>();
  @Output() confirmed = new EventEmitter<void>();
  @Output() failed = new EventEmitter<unknown>();

  readonly textInputId = `cq-confirm-text-${crypto.randomUUID()}`;
  value = '';

  readonly runConfirmAction = () => this.confirmAction?.(this.value);

  get matchesExpectedText(): boolean {
    if (this.caseSensitive) {
      return this.value === this.expectedText;
    }

    return this.value.localeCompare(this.expectedText, undefined, { sensitivity: 'accent' }) === 0;
  }

  handleInput(event: Event): void {
    this.value = (event.target as HTMLInputElement).value;
  }

  handleOpenChange(open: boolean): void {
    if (!open) {
      this.value = '';
    }

    this.openChange.emit(open);
  }
}
