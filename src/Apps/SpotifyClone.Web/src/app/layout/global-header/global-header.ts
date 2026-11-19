import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { NavigationEnd, Router, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { filter } from 'rxjs';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';

const SEARCH_ROUTE = /^\/search(?:\/([^?#]*))?/;

/**
 * Top bar: logo, home, the global search field and account shortcuts.
 * Typing in the search field drives the `/search/:query` route.
 */
@Component({
  selector: 'app-global-header',
  imports: [RouterLink, FaIconComponent],
  templateUrl: './global-header.html',
  styleUrl: './global-header.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'global-header layout-section', role: 'banner' },
})
export class GlobalHeader {
  private readonly router = inject(Router);

  protected readonly query = signal('');

  constructor() {
    // Keep the field in sync when the URL changes (back/forward, links).
    this.router.events
      .pipe(
        filter((event): event is NavigationEnd => event instanceof NavigationEnd),
        takeUntilDestroyed(inject(DestroyRef)),
      )
      .subscribe(({ urlAfterRedirects }) => {
        const match = SEARCH_ROUTE.exec(urlAfterRedirects);
        this.query.set(match?.[1] ? decodeURIComponent(match[1]) : '');
      });
  }

  protected onInput(value: string): void {
    this.query.set(value);
    const trimmed = value.trim();
    void this.router.navigate(trimmed ? ['/search', trimmed] : ['/search'], {
      replaceUrl: this.router.url.startsWith('/search'),
    });
  }

  protected clear(): void {
    this.onInput('');
  }
}
