import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, forkJoin, of } from 'rxjs';
import { map, switchMap } from 'rxjs/operators';
import { PlaylistResponse, TrackResponse } from '../models/playlist.models';
import { TrackTableRow } from '../../shared/models/track.models';

@Injectable({ providedIn: 'root' })
export class PlaylistService {
  private readonly http = inject(HttpClient);

  getPlaylist(id: string): Observable<PlaylistResponse> {
    return this.http.get<PlaylistResponse>(`/api/v1/playlists/${id}`);
  }

  getTrack(id: string): Observable<TrackResponse> {
    return this.http.get<TrackResponse>(`/api/v1/tracks/${id}`);
  }

  getPlaylistWithTracks(id: string): Observable<{ playlist: PlaylistResponse; tracks: TrackResponse[] }> {
    return this.getPlaylist(id).pipe(
      switchMap(playlist => {
        if (playlist.tracks.length === 0) {
          return of({ playlist, tracks: [] as TrackResponse[] });
        }
        const trackCalls = playlist.tracks.map(ref => this.getTrack(ref.id));
        return forkJoin(trackCalls).pipe(
          map(tracks => ({ playlist, tracks })),
        );
      }),
    );
  }

  mapToTrackTableRows(playlist: PlaylistResponse, tracks: TrackResponse[]): TrackTableRow[] {
    const positionMap = new Map(playlist.tracks.map(ref => [ref.id, ref.position]));

    return tracks
      .sort((a, b) => (positionMap.get(a.id) ?? 0) - (positionMap.get(b.id) ?? 0))
      .map(track => ({
        id: track.id,
        index: (positionMap.get(track.id) ?? 0) + 1,
        title: track.title,
        coverUrl: '',
        authors: track.mainArtists.map(a => ({ name: a.name, id: a.id })),
        album: { name: '', id: track.albumId },
        duration: this.formatDuration(track.duration),
      }));
  }

  private formatDuration(duration: string): string {
    const match = duration.match(/^(?:(\d+):)?(\d+):(\d+)/);
    if (!match) return duration;
    const hours = parseInt(match[1] || '0', 10);
    const minutes = parseInt(match[2], 10);
    const seconds = parseInt(match[3], 10);
    if (hours > 0) {
      return `${hours}:${String(minutes).padStart(2, '0')}:${String(seconds).padStart(2, '0')}`;
    }
    return `${minutes}:${String(seconds).padStart(2, '0')}`;
  }
}
