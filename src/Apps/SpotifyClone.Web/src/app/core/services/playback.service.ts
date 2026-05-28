import { Injectable, inject, PLATFORM_ID, signal } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Observable, firstValueFrom } from 'rxjs';
import { PlayRequest, PlayResponse, ShuffleResponse, RepeatResponse } from '../models/playback.models';
import { TrackResponse } from '../models/playlist.models';
import type HlsType from 'hls.js';

export interface CurrentTrackInfo {
  id: string;
  title: string;
  artists: { name: string; id: string }[];
  duration: number;
}

@Injectable({ providedIn: 'root' })
export class PlaybackService {
  private readonly http = inject(HttpClient);
  private readonly platformId = inject(PLATFORM_ID);
  private readonly DEVICE_ID_KEY = 'stopify_device_id';

  private audioEl: HTMLAudioElement | null = null;
  private hlsInstance: HlsType | null = null;
  private syncInterval: ReturnType<typeof setInterval> | null = null;

  readonly currentTrack = signal<CurrentTrackInfo | null>(null);
  readonly isPlaying = signal(false);
  readonly currentPosition = signal(0);
  readonly currentDuration = signal(0);
  readonly isShuffled = signal(false);
  readonly repeatMode = signal<'Off' | 'All' | 'Track'>('Off');

  private get isBrowser(): boolean {
    return isPlatformBrowser(this.platformId);
  }

  getDeviceId(): string {
    if (!this.isBrowser) return '';

    let deviceId = localStorage.getItem(this.DEVICE_ID_KEY);
    if (!deviceId) {
      deviceId = crypto.randomUUID();
      localStorage.setItem(this.DEVICE_ID_KEY, deviceId);
    }
    return deviceId;
  }

  playPlaylist(playlistId: string, startTrackId?: string): Observable<PlayResponse> {
    const body: PlayRequest = {
      deviceId: this.getDeviceId(),
      contextType: 'playlist',
      contextExternalId: playlistId,
      startTrackId: startTrackId ?? null,
    };
    return this.http.post<PlayResponse>('/api/v1/me/playback/play', body);
  }

  next(): Observable<PlayResponse> {
    return this.http.post<PlayResponse>('/api/v1/me/playback/next', { deviceId: this.getDeviceId() });
  }

  previous(): Observable<PlayResponse> {
    return this.http.post<PlayResponse>('/api/v1/me/playback/previous', { deviceId: this.getDeviceId() });
  }

  toggleShuffle(): void {
    this.http.patch<ShuffleResponse>('/api/v1/me/playback/shuffle', { deviceId: this.getDeviceId() })
      .subscribe({
        next: (response) => this.isShuffled.set(response.isShuffled),
      });
  }

  toggleRepeat(): void {
    this.http.patch<RepeatResponse>('/api/v1/me/playback/repeat', { deviceId: this.getDeviceId() })
      .subscribe({
        next: (response) => this.repeatMode.set(response.repeatMode),
      });
  }

  pause(): Observable<void> {
    return this.http.post<void>('/api/v1/me/playback/pause', { deviceId: this.getDeviceId() });
  }

  resume(): Observable<void> {
    return this.http.post<void>('/api/v1/me/playback/resume', { deviceId: this.getDeviceId() });
  }

  syncPosition(positionMs: number): Observable<void> {
    return this.http.patch<void>('/api/v1/me/playback/sync', {
      deviceId: this.getDeviceId(),
      positionMs,
    });
  }

  seekTo(positionMs: number): Observable<void> {
    return this.http.patch<void>('/api/v1/me/playback/seek', {
      deviceId: this.getDeviceId(),
      positionMs,
    });
  }

  formatTime(seconds: number): string {
    if (!seconds || !isFinite(seconds)) return '0:00';
    const m = Math.floor(seconds / 60);
    const s = Math.floor(seconds % 60);
    return `${m}:${String(s).padStart(2, '0')}`;
  }

  async playStream(response: PlayResponse): Promise<void> {
    if (!this.isBrowser) return;

    this.cleanup();
    this.stopSync();

    if (!response.hlsUrl && !response.dashUrl) return;

    const audio = new Audio();
    this.audioEl = audio;

    if (response.hlsUrl) {
      const Hls = await import('hls.js').then(m => m.default);
      if (Hls.isSupported()) {
        const hls = new Hls();
        hls.loadSource(response.hlsUrl);
        hls.attachMedia(audio);
        this.hlsInstance = hls;
      } else {
        audio.src = response.hlsUrl;
      }
    }

    if (response.startPositionMs > 0) {
      audio.currentTime = response.startPositionMs / 1000;
    }

    await audio.play();
    this.isPlaying.set(true);
    this.currentPosition.set(audio.currentTime);
    this.currentDuration.set(audio.duration || 0);

    const onTimeUpdate = () => {
      this.currentPosition.set(audio.currentTime);
    };
    const onLoadedMeta = () => {
      this.currentDuration.set(audio.duration);
    };
    const onEnded = () => {
      this.isPlaying.set(false);
      this.stopSync();
      this.next().subscribe({
        next: (response) => this.playStream(response),
      });
    };
    const onPause = () => {
      this.isPlaying.set(false);
      this.stopSync();
    };
    const onPlay = () => {
      this.isPlaying.set(true);
      this.startSync();
    };

    audio.addEventListener('timeupdate', onTimeUpdate);
    audio.addEventListener('loadedmetadata', onLoadedMeta);
    audio.addEventListener('ended', onEnded);
    audio.addEventListener('pause', onPause);
    audio.addEventListener('play', onPlay);

    try {
      const track = await firstValueFrom(
        this.http.get<TrackResponse>(`/api/v1/tracks/${response.trackId}`)
      );
      const durationSec = track.duration ? this.parseDuration(track.duration) : (audio.duration || 0);
      this.currentTrack.set({
        id: track.id,
        title: track.title,
        artists: track.mainArtists.map(a => ({ name: a.name, id: a.id })),
        duration: durationSec,
      });
    } catch {
      this.currentTrack.set({
        id: response.trackId,
        title: 'Unknown Track',
        artists: [],
        duration: audio.duration || 0,
      });
    }

    this.startSync();
  }

  private startSync(): void {
    this.stopSync();
    this.syncInterval = setInterval(() => {
      const pos = this.audioEl?.currentTime;
      if (pos != null) {
        this.syncPosition(Math.round(pos * 1000)).subscribe();
      }
    }, 5000);
  }

  private stopSync(): void {
    if (this.syncInterval) {
      clearInterval(this.syncInterval);
      this.syncInterval = null;
    }
  }

  private parseDuration(duration: string): number {
    const match = duration.match(/^(?:(\d+):)?(\d+):(\d+)/);
    if (!match) return 0;
    const hours = parseInt(match[1] || '0', 10);
    const minutes = parseInt(match[2], 10);
    const seconds = parseInt(match[3], 10);
    return hours * 3600 + minutes * 60 + seconds;
  }

  pauseStream(): void {
    this.audioEl?.pause();
  }

  resumeStream(): void {
    this.audioEl?.play();
  }

  seek(seconds: number): void {
    if (!this.audioEl) return;
    this.audioEl.currentTime = seconds;
    this.currentPosition.set(seconds);
    this.seekTo(Math.round(seconds * 1000)).subscribe();
  }

  private cleanup(): void {
    this.hlsInstance?.destroy();
    this.hlsInstance = null;

    this.audioEl?.pause();
    this.audioEl = null;

    this.isPlaying.set(false);
    this.currentTrack.set(null);
    this.currentPosition.set(0);
    this.currentDuration.set(0);
  }
}
