import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { UserMenu } from './user-menu';
import { provideTestDefaults } from '../../../testing/test-providers';
import { mockAuthStore } from '../../../testing/auth-store.mock';

describe('UserMenu', () => {
  function setup() {
    const auth = mockAuthStore('authenticated');
    TestBed.configureTestingModule({ providers: [...provideTestDefaults(), auth.provider] });
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigateByUrl').mockResolvedValue(true);
    const fixture = TestBed.createComponent(UserMenu);
    fixture.detectChanges();
    const el = fixture.nativeElement as HTMLElement;
    const trigger = el.querySelector<HTMLButtonElement>('.global-header__user-widget--btn')!;
    return { fixture, el, trigger, auth: auth.mock, navigate };
  }

  it("shows the user's initial and opens the menu", () => {
    const { fixture, el, trigger } = setup();
    expect(trigger.textContent?.trim()).toBe('A');
    expect(trigger.getAttribute('aria-expanded')).toBe('false');

    trigger.click();
    fixture.detectChanges();
    expect(trigger.getAttribute('aria-expanded')).toBe('true');
    expect(el.querySelectorAll('[role="menuitem"]').length).toBe(3);
  });

  it('closes on Escape and on outside clicks', () => {
    const { fixture, el, trigger } = setup();
    trigger.click();
    fixture.detectChanges();
    el.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape' }));
    fixture.detectChanges();
    expect(el.querySelector('[role="menu"]')).toBeNull();
    expect(document.activeElement).toBe(trigger);

    trigger.click();
    fixture.detectChanges();
    document.body.dispatchEvent(new PointerEvent('pointerdown', { bubbles: true }));
    fixture.detectChanges();
    expect(el.querySelector('[role="menu"]')).toBeNull();
  });

  it('logs out and goes home', () => {
    const { fixture, el, trigger, auth, navigate } = setup();
    trigger.click();
    fixture.detectChanges();
    el.querySelector<HTMLButtonElement>('button[role="menuitem"]')!.click();
    expect(auth.logout).toHaveBeenCalled();
    expect(navigate).toHaveBeenCalledWith('/');
  });
});
