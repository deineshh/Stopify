import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  Injector,
  afterNextRender,
  inject,
  signal,
  viewChild,
} from '@angular/core';
import { RouterLink } from '@angular/router';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import { AuthStore } from '@core/auth/auth.store';
import { LayoutStore } from '@core/layout/layout.store';
import { LibraryService } from '@data/services/library.service';
import { LibraryFilter } from '@data/models/library.models';

/** "Your Library" panel (left sidebar on desktop, the Library tab on phones). */
@Component({
  selector: 'app-library-sidebar',
  imports: [RouterLink, FaIconComponent],
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

  protected readonly filters: readonly { id: LibraryFilter; label: string }[] = [
    { id: 'playlists', label: 'Playlists' },
    { id: 'artists', label: 'Artists' },
  ];

  protected readonly searchOpen = signal(false);
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
