import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import { LayoutStore } from '@core/layout/layout.store';
import { PlaybackTarget } from '@data/models/player.models';
import { Track } from '@data/models/catalog.models';
import { NowPlayingService } from '@data/services/now-playing.service';
import { PlayerService } from '@data/services/player.service';
import { SidebarTrackRow } from '@shared/ui/sidebar-track-row/sidebar-track-row';

/** Right sidebar "Now Playing" view: artwork, artist spotlight, credits and what's next. */
@Component({
  selector: 'app-now-playing-panel',
  imports: [RouterLink, FaIconComponent, SidebarTrackRow],
  templateUrl: './now-playing-panel.html',
  styleUrl: './now-playing-panel.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'now-playing-view__main-group' },
})
export class NowPlayingPanel {
  protected readonly layout = inject(LayoutStore);
  protected readonly player = inject(PlayerService);
  protected readonly nowPlaying = inject(NowPlayingService);

  /** Credits fall back to "Main Artist" for every artist when the roles aren't known. */
  protected readonly credits = computed(() => {
    const track = this.player.track();
    return track?.credits ?? track?.artists.map((artist) => ({ artist, role: 'Main Artist' as const })) ?? [];
  });

  /** UI only: the API has no follows yet. */
  private readonly following = signal<ReadonlySet<string>>(new Set());

  protected isFollowing(artistId: string): boolean {
    return this.following().has(artistId);
  }

  protected toggleFollow(artistId: string): void {
    this.following.update((ids) => {
      const next = new Set(ids);
      if (!next.delete(artistId)) {
        next.add(artistId);
      }
      return next;
    });
  }

  /** Queue rows play the same album/playlist from that song. */
  protected rowTarget(track: Track): PlaybackTarget | null {
    const context = this.player.context();
    return context ? { ...context, trackId: track.id } : null;
  }
}
