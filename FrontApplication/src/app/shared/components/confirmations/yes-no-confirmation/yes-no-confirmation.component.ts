import { Component, EventEmitter, Input, Output } from '@angular/core';

import {
  ConfirmationAction,
  ConfirmationCancelCallback,
  ConfirmationFailureCallback,
  ConfirmationSuccessCallback,
} from '../confirmation.types';
import { ConfirmationDialogComponent } from '../confirmation-dialog/confirmation-dialog.component';

@Component({
  selector: 'app-yes-no-confirmation',
  standalone: true,
  imports: [ConfirmationDialogComponent],
  templateUrl: './yes-no-confirmation.component.html',
})
export class YesNoConfirmationComponent {
  @Input() open = false;
  @Input() title = 'confirmation.title';
  @Input() message = '';
  @Input() confirmLabel = 'action.yes';
  @Input() cancelLabel = 'action.no';
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
}
