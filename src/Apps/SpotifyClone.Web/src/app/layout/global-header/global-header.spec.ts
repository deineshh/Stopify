import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { GlobalHeader } from './global-header';
import { provideTestDefaults } from '../../testing/test-providers';
import { mockAuthStore } from '../../testing/auth-store.mock';

describe('GlobalHeader', () => {
  it('navigates to search results while typing and back to browse when cleared', async () => {
    TestBed.configureTestingModule({ providers: [...provideTestDefaults(), mockAuthStore().provider] });
    const router = TestBed.inject(Router);
    const navigate = vi.spyOn(router, 'navigate').mockResolvedValue(true);
    const fixture = TestBed.createComponent(GlobalHeader);
    fixture.detectChanges();
    const input = (fixture.nativeElement as HTMLElement).querySelector<HTMLInputElement>('input')!;

    input.value = 'azahriah';
    input.dispatchEvent(new Event('input'));
    expect(navigate).toHaveBeenLastCalledWith(['/search', 'azahriah'], expect.any(Object));

    (fixture.nativeElement as HTMLElement)
      .querySelector<HTMLButtonElement>('.global-header__search-btns--clear-btn')!
      .click();
    expect(navigate).toHaveBeenLastCalledWith(['/search'], expect.any(Object));
  });

  it('shows sign up / log in when signed out and the account menu when signed in', () => {
    const auth = mockAuthStore('unknown');
    TestBed.configureTestingModule({ providers: [...provideTestDefaults(), auth.provider] });
    const fixture = TestBed.createComponent(GlobalHeader);
    fixture.detectChanges();
    const el = fixture.nativeElement as HTMLElement;

    // Session still restoring: nothing account-related yet (no flash of the wrong state).
    expect(el.querySelector('.global-header__guest-group')).toBeNull();
    expect(el.querySelector('app-user-menu')).toBeNull();

    auth.mock.status.set('anonymous');
    fixture.detectChanges();
    expect(el.querySelector('.global-header__log-in-btn')?.getAttribute('href')).toBe('/login');
    expect(el.querySelector('.global-header__right-group--btns')).toBeNull();

    auth.mock.signIn();
    fixture.detectChanges();
    expect(el.querySelector('.global-header__guest-group')).toBeNull();
    expect(el.querySelector('app-user-menu')).toBeTruthy();
    expect(el.querySelector('.global-header__content-feed-btn')).toBeTruthy();
  });
});
