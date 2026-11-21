import { ChangeDetectionStrategy, Component, TemplateRef, inject, input, output, viewChild } from '@angular/core';
import { Router } from '@angular/router';
import { CdkMenu, CdkMenuItem, CdkMenuTrigger } from '@angular/cdk/menu';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import { AuthStore } from '@core/auth/auth.store';
import { Track } from '@data/models/catalog.models';
import { PlaylistRef } from '@data/models/playlist.models';
import { LibraryService } from '@data/services/library.service';
import { PlayerService } from '@data/services/player.service';
import { PlaylistEditor } from '@data/services/playlist-editor.service';
import { SubmenuTrigger } from './submenu-trigger';

/** The playlist a track list belongs to, for "Remove from this playlist". */
export type TrackMenuPlaylist = Pick<PlaylistRef, 'id' | 'name'> & { canRemove: boolean };

/** What a row passes to the menu. */
export interface TrackMenuContext {
  $implicit: Track;
}

/** Move up / Move down: the song and its row indexes before and after. */
export interface TrackMenuMove {
  trackId: string;
  from: number;
  to: number;
}

/**
 * The menu for a song (right-click a row or press its "…" button). Place one per list and pass
 * each row's track as trigger data (see `TrackMenuContext`):
 *
 *   <app-track-menu #menu />
 *   <tr [cdkContextMenuTriggerFor]="menu.template()" [cdkContextMenuTriggerData]="{ $implicit: track }">
 */
@Component({
  selector: 'app-track-menu',
  imports: [CdkMenu, CdkMenuItem, CdkMenuTrigger, SubmenuTrigger, FaIconComponent],
  templateUrl: './track-menu.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TrackMenu {
  protected readonly auth = inject(AuthStore);
  protected readonly library = inject(LibraryService);
  private readonly player = inject(PlayerService);
  private readonly editor = inject(PlaylistEditor);
  private readonly router = inject(Router);

  /** Set on a playlist page; adds "Remove from this playlist" when allowed. */
  readonly playlist = input<TrackMenuPlaylist | null>(null);

  /**
   * The list's songs when the user may reorder them, which adds Move up / Move down. An input
   * rather than trigger data: a CDK menu trigger keeps the data it was first opened with, and a
   * row's position changes as songs move.
   */
  readonly reorderList = input<readonly Track[] | null>(null);
  /** Move up / Move down was chosen; the list applies it. */
  readonly moved = output<TrackMenuMove>();

  /** Where the song is in a reorderable list, or `null`. */
  protected position(track: Track): { index: number; count: number } | null {
    const list = this.reorderList();
    const index = list ? list.findIndex((candidate) => candidate.id === track.id) : -1;
    return list && index >= 0 && list.length > 1 ? { index, count: list.length } : null;
  }

  readonly template = viewChild.required<TemplateRef<TrackMenuContext>>('menu');

  protected move(track: Track, from: number, to: number): void {
    this.moved.emit({ trackId: track.id, from, to });
  }

  protected addTo(playlist: Pick<PlaylistRef, 'id' | 'name'>, track: Track): void {
    this.editor.addTrack(playlist, track.id).subscribe();
  }

  protected addToNewPlaylist(track: Track): void {
    this.editor.createWith(track.id).subscribe();
  }

  protected removeFromPlaylist(track: Track): void {
    const playlist = this.playlist();
    if (playlist) {
      this.editor.removeTrack(playlist, track.id).subscribe();
    }
  }

  protected addToQueue(track: Track): void {
    this.player.addToQueue(track);
  }

  protected logIn(): void {
    void this.router.navigate(['/login'], { queryParams: { redirect: this.router.url } });
  }

  protected open(link: readonly string[]): void {
    void this.router.navigate(link);
  }
}
