import { TestBed } from '@angular/core/testing';
import { LibrarySidebar } from './library-sidebar';
import { provideTestDefaults } from '../../testing/test-providers';
import { mockAuthStore } from '../../testing/auth-store.mock';

describe('LibrarySidebar', () => {
  function setup(status: 'anonymous' | 'authenticated') {
    const auth = mockAuthStore(status);
    TestBed.configureTestingModule({ providers: [...provideTestDefaults(), auth.provider] });
    const fixture = TestBed.createComponent(LibrarySidebar);
    fixture.detectChanges();
    return fixture.nativeElement as HTMLElement;
  }

  it('invites signed-out visitors to log in', () => {
    const el = setup('anonymous');
    expect(el.querySelector('.left-sidebar__guest-card')).toBeTruthy();
    expect(el.querySelector('.left-sidebar__guest-btn')?.getAttribute('href')).toBe('/login?redirect=%2Fcollection');
    expect(el.querySelector('.left-sidebar__main-item')).toBeNull();
    expect(el.querySelector('.left-sidebar__filter-btns')).toBeNull();
  });

  it("shows the user's library when signed in", () => {
    const el = setup('authenticated');
    expect(el.querySelector('.left-sidebar__guest-card')).toBeNull();
    expect(el.querySelectorAll('.left-sidebar__main-item').length).toBe(20);
    expect(el.querySelector('.left-sidebar__filter-btns')).toBeTruthy();
  });
});
