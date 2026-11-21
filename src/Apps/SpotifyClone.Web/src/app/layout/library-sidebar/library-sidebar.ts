import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  Injector,
  Signal,
  afterNextRender,
  computed,
  inject,
  signal,
  viewChild,
} from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { CdkContextMenuTrigger } from '@angular/cdk/menu';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import { AuthStore } from '@core/auth/auth.store';
import { LayoutStore } from '@core/layout/layout.store';
import { LibraryService } from '@data/services/library.service';
import { LibraryFilter, LibraryItem } from '@data/models/library.models';
import { PlaylistRef } from '@data/models/playlist.models';
import { PlaylistEditor } from '@data/services/playlist-editor.service';
import { PlaylistMenu } from '@shared/ui/menus/playlist-menu';

/** "Your Library" panel (left sidebar on desktop, the Library tab on phones). */
@Component({
  selector: 'app-library-sidebar',
  imports: [RouterLink, RouterLinkActive, FaIconComponent, CdkContextMenuTrigger, PlaylistMenu],
  templateUrl: './library-sidebar.html',
  styleUrl: './library-sidebar.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: 'left-sidebar__main-group sidebar-main',
    role: 'navigation',
    'aria-label': 'Your Library',
  },
})
export class LibrarySidebar {
  protected readonly layout = inject(LayoutStore);
  protected readonly library = inject(LibraryService);
  protected readonly auth = inject(AuthStore);
  private readonly editor = inject(PlaylistEditor);

  protected readonly filters: readonly { id: LibraryFilter; label: string }[] = [
    { id: 'playlists', label: 'Playlists' },
    { id: 'artists', label: 'Artists' },
  ];

  protected readonly searchOpen = signal(false);
  /** The playlist whose menu is open stays highlighted. */
  protected readonly menuItemId = signal<string | null>(null);

  protected kindLabel(item: LibraryItem): string {
    return item.kind === 'playlist' ? 'Playlist' : 'Artist';
  }

  /** Creates "My Playlist #N" and opens it (guests are sent to log in). */
  protected createPlaylist(): void {
    this.editor.create().subscribe();
  }

  protected menuClosed(itemId: string): void {
    if (this.menuItemId() === itemId) {
      this.menuItemId.set(null);
    }
  }

  private readonly refs = new Map<string, Signal<PlaylistRef | null>>();

  /**
   * What the playlist menu (right-click on one of your playlists) acts on: the row's playlist as
   * it is now (see PlaylistMenu for why it's a signal). One stable signal per playlist.
   */
  protected refFor(id: string): Signal<PlaylistRef | null> {
    let ref = this.refs.get(id);
    if (!ref) {
      ref = computed(() => {
        const item = this.library.items().find((candidate) => candidate.id === id);
        return item ? toRef(item) : null;
      });
      this.refs.set(id, ref);
    }
    return ref;
  }
  private readonly injector = inject(Injector);
  private readonly searchInput = viewChild<ElementRef<HTMLInputElement>>('searchInput');

  protected openSearch(): void {
    this.searchOpen.set(true);
    // The field only exists after the next render.
    afterNextRender(() => this.searchInput()?.nativeElement.focus(), { injector: this.injector });
  }

  protected closeSearchIfEmpty(): void {
    if (this.library.query() === '') {
      this.searchOpen.set(false);
    }
  }

  protected clearSearch(): void {
    this.library.query.set('');
    this.searchInput()?.nativeElement.focus();
  }
}

function toRef(item: LibraryItem): PlaylistRef {
  return { id: item.id, name: item.title, description: item.description ?? null, isPublic: !!item.isPublic, cover: item.cover };
}
