import { ApplicationRef } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { HttpTestingController } from '@angular/common/http/testing';
import { DEVICE_ID } from '@core/playback/device-id';
import { PlayerService } from '@data/services/player.service';
import { PlaybackTarget } from '@data/models/player.models';
import { provideTestDefaults } from './test-providers';
import { mockAuthStore } from './auth-store.mock';
import { fakeAudioEngine } from './audio-engine.fake';
import { albumDto, streamDto, trackDto } from './catalog.fixtures';

/**
 * Signed-in test bed with a fake audio engine and no previous playback session.
 * `startPlaying` answers the play call and the track/album lookups that follow it.
 */
export function playbackTestBed() {
  const audio = fakeAudioEngine();
  TestBed.configureTestingModule({
    providers: [...provideTestDefaults(), mockAuthStore('authenticated').provider, audio.provider, { provide: DEVICE_ID, useValue: 'd1' }],
  });
  const http = TestBed.inject(HttpTestingController);
  const player = TestBed.inject(PlayerService);
  TestBed.tick();
  http.expectOne('/api/v1/me/playback').flush(null, { status: 404, statusText: 'Not Found' });
  const settle = () => TestBed.inject(ApplicationRef).whenStable();

  async function startPlaying(target: PlaybackTarget, trackId: string, track: object = {}) {
    player.play(target);
    http.expectOne('/api/v1/me/playback/play').flush(streamDto(trackId));
    http.expectOne(`/api/v1/tracks/${trackId}`).flush(trackDto(trackId, 'a1', track));
    for (const req of http.match('/api/v1/albums/a1')) req.flush(albumDto('a1', 'BAKPAKK'));
    // Not settle(): the panels' own requests may be pending now, which keeps the app unstable.
    TestBed.tick();
  }

  return { http, player, engine: audio.engine, settle, startPlaying };
}
