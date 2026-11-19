import { TestBed } from '@angular/core/testing';
import { PlayerService } from './player.service';

describe('PlayerService', () => {
  let player: PlayerService;

  beforeEach(() => {
    player = TestBed.inject(PlayerService);
  });

  it('toggles play / pause', () => {
    expect(player.isPlaying()).toBe(false);
    player.togglePlay();
    expect(player.isPlaying()).toBe(true);
  });

  it('cycles repeat off → all → one → off', () => {
    const modes = [player.repeat()];
    for (let i = 0; i < 3; i++) {
      player.cycleRepeat();
      modes.push(player.repeat());
    }
    expect(modes).toEqual(['off', 'all', 'one', 'off']);
  });

  it('derives the playback position from the progress', () => {
    const duration = player.nowPlaying().track.durationSeconds;
    player.seek(50);
    expect(player.positionSeconds()).toBe(Math.round(duration / 2));
  });

  it('mutes when the volume is dragged to zero and reports an effective volume', () => {
    player.setVolume(0);
    expect(player.muted()).toBe(true);
    expect(player.effectiveVolume()).toBe(0);

    player.setVolume(60);
    expect(player.muted()).toBe(false);
    player.toggleMute();
    expect(player.effectiveVolume()).toBe(0);
    expect(player.volume()).toBe(60);
  });
});
