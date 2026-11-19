import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { toApiError } from '@core/api/api-error';

/**
 * Loading skeleton, "not found" and error screens for data-driven pages.
 * Pass the resource's error; 404s show the not-found message, everything else offers a retry.
 */
@Component({
  selector: 'app-page-state',
  imports: [RouterLink],
  template: `
    @if (loading()) {
      <div class="page-state__skeleton" role="status" aria-live="polite">
        <span class="visually-hidden">Loading…</span>
        <div class="page-state__skeleton-hero">
          <div class="page-state__skeleton-cover" [class.circle]="shape() === 'circle'"></div>
          <div class="page-state__skeleton-lines">
            <div class="page-state__skeleton-line short"></div>
            <div class="page-state__skeleton-line title"></div>
            <div class="page-state__skeleton-line"></div>
          </div>
        </div>
        @for (row of rows; track row) {
          <div class="page-state__skeleton-row"></div>
        }
      </div>
    } @else if (error()) {
      <div class="page-state__message" role="alert">
        @if (notFound()) {
          <h1 class="page-state__title">Couldn't find that {{ thing() }}</h1>
          <p class="page-state__text">It may have been removed, or the link might be wrong.</p>
          <a class="page-state__btn" routerLink="/">Go home</a>
        } @else {
          <h1 class="page-state__title">Something went wrong</h1>
          <p class="page-state__text">{{ message() }}</p>
          <button class="page-state__btn" type="button" (click)="retry.emit()">Try again</button>
        }
      </div>
    }
  `,
  styleUrl: './page-state.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PageState {
  readonly loading = input(false);
  readonly error = input<unknown>();
  /** What the page shows, for the not-found title ("album", "artist", ...). */
  readonly thing = input('page');
  readonly shape = input<'square' | 'circle'>('square');
  readonly retry = output<void>();

  protected readonly rows = [1, 2, 3, 4, 5];

  protected readonly notFound = computed(() => {
    const error = this.error();
    const status = error instanceof HttpErrorResponse ? error.status : toApiError(error).status;
    return status === 404 || status === 400;
  });

  protected readonly message = computed(() => toApiError(this.error()).message);
}
