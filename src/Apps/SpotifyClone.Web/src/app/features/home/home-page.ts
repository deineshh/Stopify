import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { AuthStore } from '@core/auth/auth.store';
import { LayoutStore } from '@core/layout/layout.store';
import { CatalogService } from '@data/services/catalog.service';
import { FilterChip } from '@shared/ui/filter-chip/filter-chip';
import { MediaShelf } from '@shared/ui/media-shelf/media-shelf';
import { PlayButton } from '@shared/ui/play-button/play-button';

type HomeFilter = 'all' | 'music' | 'podcasts';

/** Distance (px) over which the sticky header fades to black (mockup homePageHeaderBgChangeOnScroll.js). */
const HEADER_FADE_DISTANCE = 100;

@Component({
  selector: 'app-home-page',
  imports: [RouterLink, FilterChip, MediaShelf, PlayButton],
  templateUrl: './home-page.html',
  styleUrl: './home-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'home-page' },
})
export class HomePage {
  private readonly layout = inject(LayoutStore);
  private readonly catalog = inject(CatalogService);
  private readonly auth = inject(AuthStore);

  protected readonly shelves = rxResource({ stream: () => this.catalog.getHomeShelves() });

  /** Re-runs when the session resolves, swapping new albums for the user's own playlists. */
  protected readonly shortcuts = rxResource({
    params: () => this.auth.isAuthenticated(),
    stream: ({ params: signedIn }) => this.catalog.getShortcuts(signedIn),
  });

  protected readonly filters: readonly { id: HomeFilter; label: string }[] = [
    { id: 'all', label: 'All' },
    { id: 'music', label: 'Music' },
    { id: 'podcasts', label: 'Podcasts' },
  ];
  protected readonly activeFilter = signal<HomeFilter>('all');

  protected readonly visibleShelves = computed(() => {
    const shelves = this.shelves.hasValue() ? this.shelves.value() : [];
    const filter = this.activeFilter();
    return filter === 'all' ? shelves : shelves.filter((shelf) => shelf.category === filter);
  });

  protected readonly visibleShortcuts = computed(() =>
    this.activeFilter() !== 'podcasts' && this.shortcuts.hasValue() ? this.shortcuts.value() : [],
  );

  protected readonly headerBackground = computed(() => {
    const opacity = Math.min(this.layout.mainScrollTop() / HEADER_FADE_DISTANCE, 1);
    return `rgba(0, 0, 0, ${opacity})`;
  });
}
