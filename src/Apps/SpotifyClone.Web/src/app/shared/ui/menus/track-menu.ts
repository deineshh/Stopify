import { ChangeDetectionStrategy, Component, TemplateRef, inject, input, viewChild } from '@angular/core';
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

/**
 * The menu for a song (right-click a row or press its "…" button). Place one per list and pass
 * each row's track as trigger data:
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

  readonly template = viewChild.required<TemplateRef<{ $implicit: Track }>>('menu');

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
