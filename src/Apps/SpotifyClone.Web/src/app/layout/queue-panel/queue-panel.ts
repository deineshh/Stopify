import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import { LayoutStore } from '@core/layout/layout.store';
import { Track } from '@data/models/catalog.models';
import { PlaybackTarget } from '@data/models/player.models';
import { NowPlayingService } from '@data/services/now-playing.service';
import { PlayerService } from '@data/services/player.service';
import { SidebarTrackRow } from '@shared/ui/sidebar-track-row/sidebar-track-row';

/** Right sidebar "Queue" view: the current song and what plays after it. */
@Component({
  selector: 'app-queue-panel',
  imports: [RouterLink, FaIconComponent, SidebarTrackRow],
  templateUrl: './queue-panel.html',
  styleUrl: './queue-panel.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'queue-view__main-group' },
})
export class QueuePanel {
  protected readonly layout = inject(LayoutStore);
  protected readonly player = inject(PlayerService);
  protected readonly nowPlaying = inject(NowPlayingService);

  /** A queued song plays the same album/playlist from that song. */
  protected rowTarget(track: Track): PlaybackTarget | null {
    const context = this.player.context();
    return context ? { ...context, trackId: track.id } : null;
  }
}
