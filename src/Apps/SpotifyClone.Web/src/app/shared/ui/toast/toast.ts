import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { ToastService } from '@core/notifications/toast.service';

/** Renders `ToastService` messages. The live region always exists so screen readers announce new ones. */
@Component({
  selector: 'app-toast',
  template: `
    @if (toasts.current(); as toast) {
      <p class="toast__message" (click)="toasts.dismiss()">{{ toast.text }}</p>
    }
  `,
  styleUrl: './toast.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'toast', role: 'status', 'aria-live': 'polite' },
})
export class ToastOutlet {
  protected readonly toasts = inject(ToastService);
}
