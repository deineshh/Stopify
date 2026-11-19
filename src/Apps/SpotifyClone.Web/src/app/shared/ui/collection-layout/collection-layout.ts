import {
  ChangeDetectionStrategy,
  Component,
  booleanAttribute,
  computed,
  inject,
  input,
  model,
  output,
} from '@angular/core';
import { RouterLink } from '@angular/router';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import { LayoutStore } from '@core/layout/layout.store';
import { PlayButton } from '../play-button/play-button';

/** One entry of the "Owner • 2025 • 81 songs, 3 hr 30 min" line under the title. */
export interface CollectionMetaItem {
  text: string;
  /** Tooltip, e.g. the full release date behind a year. */
  title?: string;
  /** Renders the entry as a bold link (owner / artist). */
  link?: readonly string[];
}

/** Scroll distance (px) over which the compact sticky header fades in (mockup playlistPageHeaderBgChangeOnScroll.js). */
const HEADER_FADE_DISTANCE = 300;

/**
 * Frame shared by playlist, album and artist pages: sticky mini header, hero (cover, title,
 * meta line) and the action bar. Page-specific content (track list, shelves, about) is projected
 * below the action bar. Buttons are rendered here — set `following` / `saved` to `null` to hide them.
 */
@Component({
  selector: 'app-collection-layout',
  imports: [RouterLink, FaIconComponent, PlayButton],
  templateUrl: './collection-layout.html',
  styleUrl: './collection-layout.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CollectionLayout {
  private readonly layout = inject(LayoutStore);

  readonly title = input.required<string>();
  readonly cover = input.required<string>();
  readonly coverShape = input<'square' | 'circle'>('square');
  /** Small label above the title ("Public Playlist", "Album", "Verified Artist"). */
  readonly eyebrow = input<string>();
  readonly verified = input(false, { transform: booleanAttribute });
  readonly description = input<string | null>();
  readonly meta = input<readonly CollectionMetaItem[]>([]);
  /** Extra lines under the meta line, e.g. "18 Public Playlists" on profiles. */
  readonly details = input<readonly string[]>([]);
  /** Shows "Choose photo" on hover (the owner's own playlists). */
  readonly editableCover = input(false, { transform: booleanAttribute });

  readonly playing = input(false);
  readonly play = output<void>();
  readonly shuffle = model(false);
  /** `null` hides the Follow button. */
  readonly following = model<boolean | null>(null);
  /** `null` hides the Save button. */
  readonly saved = model<boolean | null>(null);
  readonly showDownload = input(true, { transform: booleanAttribute });
  readonly showInvite = input(false, { transform: booleanAttribute });
  readonly showViewAs = input(true, { transform: booleanAttribute });

  protected readonly headerOpacity = computed(() =>
    Math.min(this.layout.mainScrollTop() / HEADER_FADE_DISTANCE, 1),
  );

  protected toggleFollowing(): void {
    this.following.update((value) => !value);
  }

  protected toggleSaved(): void {
    this.saved.update((value) => !value);
  }
}
