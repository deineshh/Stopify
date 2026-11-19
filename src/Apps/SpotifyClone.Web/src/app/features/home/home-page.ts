import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
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

  protected readonly feed = toSignal(inject(CatalogService).getHomeFeed(), { requireSync: true });

  protected readonly filters: readonly { id: HomeFilter; label: string }[] = [
    { id: 'all', label: 'All' },
    { id: 'music', label: 'Music' },
    { id: 'podcasts', label: 'Podcasts' },
  ];
  protected readonly activeFilter = signal<HomeFilter>('all');

  protected readonly headerBackground = computed(() => {
    const opacity = Math.min(this.layout.mainScrollTop() / HEADER_FADE_DISTANCE, 1);
    return `rgba(0, 0, 0, ${opacity})`;
  });
}
