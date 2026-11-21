import { ChangeDetectionStrategy, Component, booleanAttribute, inject, input, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import { Track } from '@data/models/catalog.models';
import { PlaybackContextRef, PlaybackTarget } from '@data/models/player.models';
import { PlayerService } from '@data/services/player.service';
import { CdkContextMenuTrigger, CdkMenuTrigger } from '@angular/cdk/menu';
import { TrackMenu, TrackMenuPlaylist } from '../menus/track-menu';
import { DurationPipe } from '../../pipes/duration.pipe';
import { ArtistNamesPipe } from '../../pipes/artist-names.pipe';

/**
 * Numbered track list with album and date columns (mockup `.track-table`).
 * Columns drop out as the table gets narrower, so it also works on phones.
 */
@Component({
  selector: 'app-track-table',
  imports: [DatePipe, RouterLink, FaIconComponent, DurationPipe, ArtistNamesPipe, CdkContextMenuTrigger, CdkMenuTrigger, TrackMenu],
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

  /** The album or playlist the rows play in; without one each row plays from its own album. */
  readonly context = input<PlaybackContextRef | null>(null);

  protected readonly player = inject(PlayerService);

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
