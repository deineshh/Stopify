import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  Injector,
  PLATFORM_ID,
  afterNextRender,
  booleanAttribute,
  computed,
  inject,
  input,
  output,
  signal,
} from '@angular/core';
import { DatePipe, isPlatformBrowser } from '@angular/common';
import { CdkDrag, CdkDragDrop, CdkDragPreview, CdkDropList } from '@angular/cdk/drag-drop';
import { RouterLink } from '@angular/router';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import { Track } from '@data/models/catalog.models';
import { PlaybackContextRef, PlaybackTarget } from '@data/models/player.models';
import { PlayerService } from '@data/services/player.service';
import { CdkContextMenuTrigger, CdkMenuTrigger } from '@angular/cdk/menu';
import { TrackMenu, TrackMenuMove, TrackMenuPlaylist } from '../menus/track-menu';
import { DurationPipe } from '../../pipes/duration.pipe';
import { ArtistNamesPipe } from '../../pipes/artist-names.pipe';

/** A song moved from one row to another (indexes into `tracks`). */
export interface TrackReorder {
  trackId: string;
  from: number;
  to: number;
}

/**
 * Numbered track list with album and date columns (mockup `.track-table`).
 * Columns drop out as the table gets narrower, so it also works on phones.
 *
 * With `reorderable`, rows can be dragged with a mouse, and the song menu offers Move up / Move
 * down (keyboards and touch screens, where a long press is the menu). The table only reports the
 * move through `reorder`; the owner of the list applies it.
 */
@Component({
  selector: 'app-track-table',
  imports: [
    DatePipe,
    RouterLink,
    FaIconComponent,
    DurationPipe,
    ArtistNamesPipe,
    CdkContextMenuTrigger,
    CdkMenuTrigger,
    CdkDropList,
    CdkDrag,
    CdkDragPreview,
    TrackMenu,
  ],
  templateUrl: './track-table.html',
  styleUrl: './track-table.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TrackTable {
  readonly tracks = input.required<readonly Track[]>();
  readonly showAlbum = input(true, { transform: booleanAttribute });
  readonly showDateAdded = input(true, { transform: booleanAttribute });
  /** Hides the header row (artist "Popular" lists). */
  readonly showHeader = input(true, { transform: booleanAttribute });
  /** On a playlist page: lets the menu offer "Remove from this playlist". */
  readonly menuPlaylist = input<TrackMenuPlaylist | null>(null);

  /** Lets the user reorder the songs (their own or a collaborative playlist). */
  readonly reorderable = input(false, { transform: booleanAttribute });
  readonly reorder = output<TrackReorder>();

  /** The album or playlist the rows play in; without one each row plays from its own album. */
  readonly context = input<PlaybackContextRef | null>(null);

  protected readonly player = inject(PlayerService);
  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);
  private readonly injector = inject(Injector);

  /**
   * Dragging is for mice and trackpads. On touch screens a drag would fight scrolling and the
   * long press that opens the song menu, so moving goes through the menu there.
   */
  private readonly finePointer =
    isPlatformBrowser(inject(PLATFORM_ID)) &&
    typeof matchMedia === 'function' &&
    matchMedia('(hover: hover) and (pointer: fine)').matches;

  protected readonly draggable = computed(() => this.reorderable() && this.finePointer && this.tracks().length > 1);

  protected dropped(event: CdkDragDrop<unknown, unknown, Track>): void {
    if (event.previousIndex !== event.currentIndex) {
      this.reorder.emit({ trackId: event.item.data.id, from: event.previousIndex, to: event.currentIndex });
    }
  }

  /** Move up / Move down from the song menu. */
  protected movedFromMenu(move: TrackMenuMove): void {
    this.reorder.emit(move);
    // The row is moved in the DOM, which drops focus from its "…" button; put it back.
    afterNextRender(
      () =>
        this.host.nativeElement
          .querySelector<HTMLElement>(`[data-track-id="${CSS.escape(move.trackId)}"] .track-table__track--more-btn`)
          ?.focus(),
      { injector: this.injector },
    );
  }

  /** The row whose menu is open stays highlighted, so it's clear which song the menu is for. */
  protected readonly menuTrackId = signal<string | null>(null);

  protected menuClosed(trackId: string): void {
    // Opening another row's menu closes this one after the new one opened.
    if (this.menuTrackId() === trackId) {
      this.menuTrackId.set(null);
    }
  }

  protected target(track: Track): PlaybackTarget | null {
    const context = this.context();
    if (context) {
      return { ...context, trackId: track.id };
    }
    return track.album.id ? { kind: 'album', id: track.album.id, trackId: track.id } : null;
  }
}
