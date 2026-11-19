import { ChangeDetectionStrategy, Component, inject, linkedSignal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import { LayoutStore } from '@core/layout/layout.store';
import { PlayerService } from '@data/services/player.service';
import { SidebarTrackRow } from '@shared/ui/sidebar-track-row/sidebar-track-row';

/** Right sidebar "Now Playing" view: artwork, artist spotlight, credits and what's next. */
@Component({
  selector: 'app-now-playing-panel',
  imports: [DecimalPipe, RouterLink, FaIconComponent, SidebarTrackRow],
  templateUrl: './now-playing-panel.html',
  styleUrl: './now-playing-panel.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'now-playing-view__main-group' },
})
export class NowPlayingPanel {
  protected readonly layout = inject(LayoutStore);
  protected readonly player = inject(PlayerService);

  /** Local follow state per artist (UI only until the API exists). */
  protected readonly following = linkedSignal(() => {
    const details = this.player.nowPlaying();
    return new Map<string, boolean>([
      [details.spotlight.artist.id, details.spotlight.following],
      ...details.credits.map((credit) => [credit.artist.id, credit.following] as const),
    ]);
  });

  protected isFollowing(artistId: string): boolean {
    return this.following().get(artistId) ?? false;
  }

  protected toggleFollow(artistId: string): void {
    this.following.update((map) => new Map(map).set(artistId, !map.get(artistId)));
  }
}
