import { Injectable, computed, inject } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { Observable, map, switchMap } from 'rxjs';
import { imageUrl } from '@core/api/media';
import { LayoutStore } from '@core/layout/layout.store';
import { CatalogApi } from '../api/catalog-api';
import { PlaybackApi } from '../api/playback-api';
import { EntityRef } from '../models/catalog.models';
import { PlaybackContextRef } from '../models/player.models';
import { PlayerService } from './player.service';
import { TrackLookup } from './track-lookup.service';

export interface PlayingContext {
  name: string;
  link: readonly string[];
}

export interface ArtistSpotlight {
  artist: EntityRef;
  avatar: string;
  bio: string | null;
  verified: boolean;
}

/**
 * What the right-hand panels show about the current song: the album or playlist it plays from,
 * its first artist and the queue. Each part loads on its own; the queue only while a panel is on screen.
 */
@Injectable({ providedIn: 'root' })
export class NowPlayingService {
  private readonly player = inject(PlayerService);
  private readonly catalog = inject(CatalogApi);
  private readonly playback = inject(PlaybackApi);
  private readonly lookup = inject(TrackLookup);
  private readonly layout = inject(LayoutStore);

  private readonly contextResource = rxResource({
    params: () => this.player.context() ?? undefined,
    stream: ({ params }) => this.loadContext(params),
  });
  readonly context = computed(() => (this.contextResource.hasValue() ? this.contextResource.value() : null));

  private readonly artistResource = rxResource({
    params: () => this.player.track()?.artists[0]?.id,
    stream: ({ params: id }) =>
      this.catalog.artist(id).pipe(
        map(
          (artist): ArtistSpotlight => ({
            artist: { id: artist.id, name: artist.name },
            avatar: imageUrl(artist.avatar?.imageId),
            bio: artist.bio?.trim() || null,
            verified: artist.status.toLowerCase() === 'verified',
          }),
        ),
      ),
  });
  readonly artist = computed(() => (this.artistResource.hasValue() ? this.artistResource.value() : null));

  /** Upcoming songs. Reloaded when the song, shuffle or repeat changes, or a song is queued. */
  private readonly queueResource = rxResource({
    params: () => {
      const track = this.player.track();
      return track && this.layout.isRightPanelVisible()
        ? {
            trackId: track.id,
            shuffle: this.player.shuffle(),
            repeat: this.player.repeat(),
            queued: this.player.queueRevision(),
          }
        : undefined;
    },
    stream: () => this.playback.queue().pipe(switchMap((queue) => this.lookup.tracks(queue.tracksInQueue))),
  });
  readonly queue = computed(() => (this.queueResource.hasValue() ? this.queueResource.value() : []));
  readonly queueLoading = computed(() => this.queueResource.isLoading());
  readonly next = computed(() => this.queue()[0] ?? null);

  private loadContext(context: PlaybackContextRef): Observable<PlayingContext> {
    return context.kind === 'album'
      ? this.catalog.album(context.id).pipe(map((album) => ({ name: album.title, link: ['/album', album.id] })))
      : this.catalog.playlist(context.id).pipe(map((playlist) => ({ name: playlist.name, link: ['/playlist', playlist.id] })));
  }
}
