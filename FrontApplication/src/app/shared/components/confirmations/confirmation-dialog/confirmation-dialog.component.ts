import { NgClass } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { LucideDynamicIcon } from '@lucide/angular';
import { firstValueFrom, isObservable } from 'rxjs';

import { TextPipe } from '../../../../core/i18n/text.pipe';
import {
  ConfirmationAction,
  ConfirmationCancelCallback,
  ConfirmationFailureCallback,
  ConfirmationSuccessCallback,
} from '../confirmation.types';

@Component({
  selector: 'app-confirmation-dialog',
  standalone: true,
  imports: [LucideDynamicIcon, NgClass, TextPipe],
  templateUrl: './confirmation-dialog.component.html',
})
export class ConfirmationDialogComponent {
  @Input() open = false;
  @Input() title = 'confirmation.title';
  @Input() message = '';
  @Input() confirmLabel = 'action.confirm';
  @Input() cancelLabel = 'action.cancel';
  @Input() icon = 'circle-alert';
  @Input() destructive = false;
  @Input() confirmDisabled = false;
  @Input() confirmAction?: ConfirmationAction;
  @Input() onSuccess?: ConfirmationSuccessCallback;
  @Input() onCancel?: ConfirmationCancelCallback;
  @Input() onFailure?: ConfirmationFailureCallback;

  @Output() openChange = new EventEmitter<boolean>();
  @Output() cancelled = new EventEmitter<void>();
  @Output() confirmed = new EventEmitter<void>();
  @Output() failed = new EventEmitter<unknown>();

  readonly titleId = `cq-confirm-title-${crypto.randomUUID()}`;
  readonly messageId = `cq-confirm-message-${crypto.randomUUID()}`;
  processing = false;
  failureMessage = '';

  cancel(): void {
    if (this.processing) {
      return;
    }

    this.failureMessage = '';
    this.onCancel?.();
    this.cancelled.emit();
    this.openChange.emit(false);
  }

  confirm(): void {
    if (this.processing || this.confirmDisabled) {
      return;
    }

    this.processing = true;
    this.failureMessage = '';

    try {
      const result = this.confirmAction?.();
      const completion = isObservable(result) ? firstValueFrom(result) : Promise.resolve(result);

      void completion
        .then(() => this.handleSuccess())
        .catch((error: unknown) => this.handleFailure(error));
    } catch (error) {
      this.handleFailure(error);
    }
  }

  private handleSuccess(): void {
    this.processing = false;
    this.confirmed.emit();
    this.onSuccess?.();
    this.openChange.emit(false);
  }

  private handleFailure(error: unknown): void {
    this.processing = false;
    this.failureMessage = getFailureMessage(error);
    this.failed.emit(error);
    this.onFailure?.(error);
  }
}

function getFailureMessage(error: unknown): string {
  if (error instanceof Error && error.message) {
    return error.message;
  }

  return typeof error === 'string' ? error : '';
}
