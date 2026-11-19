import { ChangeDetectionStrategy, Component, computed, inject, input, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import { CatalogService } from '@data/services/catalog.service';
import { MediaCard as MediaCardModel } from '@data/models/catalog.models';
import { FilterChip } from '@shared/ui/filter-chip/filter-chip';
import { MediaCard } from '@shared/ui/media-card/media-card';
import { PlayButton } from '@shared/ui/play-button/play-button';
import { SingleRowDirective } from '@shared/directives/single-row.directive';
import { DurationPipe } from '@shared/pipes/duration.pipe';
import { ArtistNamesPipe } from '@shared/pipes/artist-names.pipe';

type ResultFilter = 'all' | 'albums' | 'playlists' | 'songs' | 'genres' | 'artists' | 'profiles';

interface CardSection {
  id: Exclude<ResultFilter, 'all' | 'songs' | 'genres'>;
  title: string;
  items: readonly MediaCardModel[];
}

@Component({
  selector: 'app-search-results-page',
  imports: [
    RouterLink,
    FaIconComponent,
    FilterChip,
    MediaCard,
    PlayButton,
    SingleRowDirective,
    DurationPipe,
    ArtistNamesPipe,
  ],
  templateUrl: './search-results-page.html',
  styleUrl: './search-results-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'search-results' },
})
export class SearchResultsPage {
  /** Bound from the `:query` route parameter. */
  readonly query = input.required<string>();

  private readonly catalog = inject(CatalogService);
  protected readonly results = rxResource({
    params: () => this.query(),
    stream: ({ params }) => this.catalog.search(params),
  });

  protected readonly filters: readonly { id: ResultFilter; label: string }[] = [
    { id: 'all', label: 'All' },
    { id: 'albums', label: 'Albums' },
    { id: 'playlists', label: 'Playlists' },
    { id: 'songs', label: 'Songs' },
    { id: 'genres', label: 'Genres & Moods' },
    { id: 'artists', label: 'Artists' },
    { id: 'profiles', label: 'Profiles' },
  ];
  protected readonly activeFilter = signal<ResultFilter>('all');

  /** Card sections in mockup order: Artists, Albums, Playlists, Profiles. */
  protected readonly cardSections = computed<readonly CardSection[]>(() => {
    const data = this.results.hasValue() ? this.results.value() : undefined;
    if (!data) {
      return [];
    }
    return [
      { id: 'artists', title: 'Artists', items: data.artists },
      { id: 'albums', title: 'Albums', items: data.albums },
      { id: 'playlists', title: 'Playlists', items: data.playlists },
      { id: 'profiles', title: 'Profiles', items: data.profiles },
    ];
  });

  protected isVisible(section: ResultFilter): boolean {
    const active = this.activeFilter();
    return active === 'all' || active === section;
  }
}
