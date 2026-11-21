import { ChangeDetectionStrategy, Component, PLATFORM_ID, computed, inject, input, signal } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { rxResource } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import { switchMap, timer } from 'rxjs';
import { CatalogService } from '@data/services/catalog.service';
import { MediaCard as MediaCardModel, Track } from '@data/models/catalog.models';
import { PlaybackTarget } from '@data/models/player.models';
import { PlayerService } from '@data/services/player.service';
import { FilterChip } from '@shared/ui/filter-chip/filter-chip';
import { MediaCard } from '@shared/ui/media-card/media-card';
import { PageState } from '@shared/ui/page-state/page-state';
import { CdkContextMenuTrigger, CdkMenuTrigger } from '@angular/cdk/menu';
import { TrackMenu } from '@shared/ui/menus/track-menu';
import { PlayButton } from '@shared/ui/play-button/play-button';
import { SingleRowDirective } from '@shared/directives/single-row.directive';
import { DurationPipe } from '@shared/pipes/duration.pipe';
import { ArtistNamesPipe } from '@shared/pipes/artist-names.pipe';

const SEARCH_DEBOUNCE_MS = 250;

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
    PageState,
    CdkContextMenuTrigger,
    CdkMenuTrigger,
    TrackMenu,
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
  protected readonly player = inject(PlayerService);
  /** The song whose menu is open stays highlighted. */
  protected readonly menuSongId = signal<string | null>(null);
  /** Typing changes the route on every keystroke; wait for a pause before searching (browser only). */
  private readonly debounceMs = isPlatformBrowser(inject(PLATFORM_ID)) ? SEARCH_DEBOUNCE_MS : 0;

  protected readonly results = rxResource({
    params: () => this.query(),
    stream: ({ params }) => timer(this.debounceMs).pipe(switchMap(() => this.catalog.search(params))),
  });

  protected readonly isEmpty = computed(() => {
    const data = this.results.hasValue() ? this.results.value() : undefined;
    return (
      !!data &&
      !data.topResult &&
      [data.songs, data.artists, data.albums, data.playlists, data.profiles, data.genres].every((list) => !list.length)
    );
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
    const sections: CardSection[] = [
      { id: 'artists', title: 'Artists', items: data.artists },
      { id: 'albums', title: 'Albums', items: data.albums },
      { id: 'playlists', title: 'Playlists', items: data.playlists },
      { id: 'profiles', title: 'Profiles', items: data.profiles },
    ];
    return sections.filter((section) => section.items.length > 0);
  });

  protected isVisible(section: ResultFilter): boolean {
    const active = this.activeFilter();
    return active === 'all' || active === section;
  }

  protected menuClosed(songId: string): void {
    if (this.menuSongId() === songId) {
      this.menuSongId.set(null);
    }
  }

  /** Search has no playback context of its own yet, so a song plays from its album. */
  protected songTarget(song: Track): PlaybackTarget | null {
    return song.album.id ? { kind: 'album', id: song.album.id, trackId: song.id } : null;
  }
}
