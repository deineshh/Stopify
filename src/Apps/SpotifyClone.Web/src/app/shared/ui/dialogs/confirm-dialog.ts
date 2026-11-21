import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { DIALOG_DATA, DialogRef } from '@angular/cdk/dialog';

export interface ConfirmDialogData {
  title: string;
  message: string;
  confirmLabel: string;
}

/** "Are you sure?" dialog; closes with `true` when confirmed. */
@Component({
  selector: 'app-confirm-dialog',
  template: `
    <div class="dialog confirm-dialog">
      <h2 id="confirm-dialog-title" class="dialog__title">{{ data.title }}</h2>
      <p class="dialog__text">{{ data.message }}</p>
      <div class="dialog__actions">
        <button class="dialog__btn dialog__btn--secondary" type="button" (click)="ref.close(false)">Cancel</button>
        <button class="dialog__btn dialog__btn--danger" type="button" (click)="ref.close(true)">
          {{ data.confirmLabel }}
        </button>
      </div>
    </div>
  `,
  styles: `
    .confirm-dialog {
      width: 420px;
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ConfirmDialog {
  protected readonly data = inject<ConfirmDialogData>(DIALOG_DATA);
  protected readonly ref = inject<DialogRef<boolean>>(DialogRef);
}
