import { Injectable, computed, signal } from '@angular/core';
import { LibraryFilter, LibraryItem } from '../models/library.models';
import { LIBRARY_ITEMS } from '../mock/library.mock';

/** The signed-in user's library ("Your Library" sidebar). */
@Injectable({ providedIn: 'root' })
export class LibraryService {
  private readonly items = signal<readonly LibraryItem[]>(LIBRARY_ITEMS);

  readonly filter = signal<LibraryFilter | null>(null);
  readonly query = signal('');

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
}

const filterToKind: Record<LibraryFilter, LibraryItem['kind']> = {
  playlists: 'playlist',
  artists: 'artist',
};
