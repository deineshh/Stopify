import { Injectable, computed, inject, linkedSignal, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { map } from 'rxjs';
import { AuthStore } from '@core/auth/auth.store';
import { imageUrl } from '@core/api/media';
import { CatalogApi } from '../api/catalog-api';
import { PlaylistSummaryDto } from '../api/catalog.dtos';
import { LibraryFilter, LibraryItem } from '../models/library.models';

/** The API returns at most this many playlists per page; more than enough for a sidebar. */
const LIBRARY_PAGE_SIZE = 50;

/**
 * The signed-in user's library ("Your Library" sidebar). Loads the user's playlists when a
 * session starts and drops them on sign-out. The API has no followed artists yet.
 */
@Injectable({ providedIn: 'root' })
export class LibraryService {
  private readonly api = inject(CatalogApi);
  private readonly auth = inject(AuthStore);

  /**
   * Bumped by `reload()`. It's part of the request on purpose: `resource.reload()` is dropped when
   * it lands between a response arriving and being applied, so two quick changes could leave the
   * library stale. A new request always runs, and cancels an older one still in flight.
   */
  private readonly version = signal(0);

  /** Keyed on the user id: idle (no request) while signed out, reloads when the account changes. */
  private readonly playlists = rxResource({
    params: () => {
      const userId = this.auth.user()?.id;
      return userId ? { userId, version: this.version() } : undefined;
    },
    stream: () => this.api.myPlaylists({ pageSize: LIBRARY_PAGE_SIZE }).pipe(map((page) => page.items)),
  });

  /** The last loaded list, kept on screen while a newer one loads; empty once signed out. */
  private readonly loaded = linkedSignal<readonly PlaylistSummaryDto[] | undefined, readonly PlaylistSummaryDto[]>({
    source: () => {
      if (this.playlists.hasValue()) {
        return this.playlists.value();
      }
      return this.auth.user() ? undefined : [];
    },
    computation: (playlists, previous) => playlists ?? previous?.value ?? [],
  });

  readonly filter = signal<LibraryFilter | null>(null);
  readonly query = signal('');

  readonly isLoading = computed(() => this.playlists.isLoading());
  readonly error = computed(() => this.playlists.error());

  readonly items = computed<readonly LibraryItem[]>(() => {
    const user = this.auth.user();
    return this.loaded().map((playlist) => toLibraryItem(playlist, playlist.ownerId === user?.id ? user.displayName : undefined));
  });

  /** Playlists the user can add songs to ("Add to playlist"). */
  readonly ownPlaylists = computed(() => this.items().filter((item) => item.kind === 'playlist' && item.owned));

  readonly visibleItems = computed(() => {
    const filter = this.filter();
    const query = this.query().trim().toLowerCase();
    return this.items().filter(
      (item) =>
        (filter === null || item.kind === filterToKind[filter]) &&
        (query === '' || item.title.toLowerCase().includes(query)),
    );
  });

  toggleFilter(filter: LibraryFilter): void {
    this.filter.update((current) => (current === filter ? null : filter));
  }

  reload(): void {
    this.version.update((version) => version + 1);
  }
}

const filterToKind: Record<LibraryFilter, LibraryItem['kind']> = {
  playlists: 'playlist',
  artists: 'artist',
};

function toLibraryItem(playlist: PlaylistSummaryDto, ownerName: string | undefined): LibraryItem {
  return {
    id: playlist.id,
    title: playlist.name,
    kind: 'playlist',
    cover: imageUrl(playlist.customCover?.imageId ?? playlist.generatedCoverImageIds[0], '') || undefined,
    subtitle: ownerName,
    link: ['/playlist', playlist.id],
    owned: ownerName !== undefined,
    isPublic: playlist.isPublic,
    description: playlist.description,
  };
}
