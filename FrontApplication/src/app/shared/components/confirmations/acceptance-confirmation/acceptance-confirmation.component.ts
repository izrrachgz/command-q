import { Component, EventEmitter, Input, Output } from '@angular/core';

import { TextPipe } from '../../../../core/i18n/text.pipe';
import {
  ConfirmationAction,
  ConfirmationCancelCallback,
  ConfirmationFailureCallback,
  ConfirmationSuccessCallback,
} from '../confirmation.types';
import { ConfirmationDialogComponent } from '../confirmation-dialog/confirmation-dialog.component';

@Component({
  selector: 'app-acceptance-confirmation',
  standalone: true,
  imports: [ConfirmationDialogComponent, TextPipe],
  templateUrl: './acceptance-confirmation.component.html',
})
export class AcceptanceConfirmationComponent {
  @Input() open = false;
  @Input() title = 'confirmation.title';
  @Input() message = '';
  @Input() acceptLabel = 'confirmation.acceptLabel';
  @Input() confirmLabel = 'action.confirm';
  @Input() cancelLabel = 'action.cancel';
  @Input() icon = 'circle-alert';
  @Input() destructive = false;
  @Input() confirmAction?: ConfirmationAction;
  @Input() onSuccess?: ConfirmationSuccessCallback;
  @Input() onCancel?: ConfirmationCancelCallback;
  @Input() onFailure?: ConfirmationFailureCallback;

  @Output() openChange = new EventEmitter<boolean>();
  @Output() cancelled = new EventEmitter<void>();
  @Output() confirmed = new EventEmitter<void>();
  @Output() failed = new EventEmitter<unknown>();

  readonly acceptInputId = `cq-confirm-accept-${crypto.randomUUID()}`;
  accepted = false;

  handleAccepted(event: Event): void {
    this.accepted = (event.target as HTMLInputElement).checked;
  }

  handleOpenChange(open: boolean): void {
    if (!open) {
      this.accepted = false;
    }

    this.openChange.emit(open);
  }
}
