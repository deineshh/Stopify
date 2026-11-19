import { TestBed } from '@angular/core/testing';
import { NowPlayingBar } from './now-playing-bar';
import { LayoutStore } from '@core/layout/layout.store';
import { PlayerService } from '@data/services/player.service';
import { provideTestDefaults } from '../../testing/test-providers';

describe('NowPlayingBar', () => {
  function setup() {
    TestBed.configureTestingModule({ providers: provideTestDefaults() });
    const fixture = TestBed.createComponent(NowPlayingBar);
    fixture.detectChanges();
    const el = fixture.nativeElement as HTMLElement;
    const click = (selector: string) => {
      el.querySelector<HTMLButtonElement>(selector)!.click();
      fixture.detectChanges();
    };
    return { el, click, layout: TestBed.inject(LayoutStore), player: TestBed.inject(PlayerService) };
  }

  it('shows the current track', () => {
    const { el } = setup();
    expect(el.querySelector('.now-playing-bar__track-info--title-span')?.textContent).toContain('PANNONIA');
    expect(el.querySelectorAll('.now-playing-bar__track-info--artist-anchor').length).toBe(3);
  });

  it('toggles play, shuffle and repeat', () => {
    const { el, click, player } = setup();
    click('.now-playing-bar__player--play-btn');
    expect(player.isPlaying()).toBe(true);
    expect(el.querySelector('.now-playing-bar__player--play-btn')?.getAttribute('title')).toBe('Pause');

    click('.now-playing-bar__player--shuffle-btn');
    expect(el.querySelector('.now-playing-bar__player--shuffle-btn')?.classList).toContain('active');

    click('.now-playing-bar__player--repeat-btn');
    click('.now-playing-bar__player--repeat-btn');
    expect(el.querySelector('.now-playing-bar__player--repeat-btn')?.classList).toContain('repeat-one');
  });

  it('opens and closes the queue panel', () => {
    const { el, click, layout } = setup();
    click('.now-playing-bar__queue-btn');
    expect(layout.rightPanel()).toBe('queue');
    expect(el.querySelector('.now-playing-bar__queue-btn')?.getAttribute('aria-pressed')).toBe('true');
    click('.now-playing-bar__queue-btn');
    expect(layout.rightPanel()).toBeNull();
  });
});
