import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { AuthPage } from './auth-page';
import { provideTestDefaults } from '../../testing/test-providers';

describe('AuthPage', () => {
  function setup(flow: 'signup' | 'login') {
    TestBed.configureTestingModule({ providers: provideTestDefaults() });
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigateByUrl').mockResolvedValue(true);
    const fixture = TestBed.createComponent(AuthPage);
    fixture.componentRef.setInput('flow', flow);
    fixture.detectChanges();
    const el = fixture.nativeElement as HTMLElement;
    const type = (selector: string, value: string) => {
      const input = el.querySelector<HTMLInputElement>(selector)!;
      input.value = value;
      input.dispatchEvent(new Event('input'));
      fixture.detectChanges();
    };
    const next = () => {
      el.querySelector<HTMLButtonElement>('.sign-up-page__next-btn')!.click();
      fixture.detectChanges();
    };
    return { el, type, next, navigate, fixture };
  }

  it('blocks an invalid email and moves on with a valid one', () => {
    const { el, type, next } = setup('signup');
    expect(el.querySelector('h1')?.textContent).toContain('Sign up to start listening');

    type('#signup-email', 'not-an-email');
    next();
    expect(el.querySelector('[role="alert"]')?.textContent).toContain('This email is invalid');

    type('#signup-email', 'name@domain.com');
    next();
    expect(el.querySelector('.sign-up-page__title-group--step-number')?.textContent).toContain('Step 1 of 3');
  });

  it('walks through the whole sign up flow', () => {
    const { el, type, next, navigate, fixture } = setup('signup');
    type('#signup-email', 'name@domain.com');
    next();

    type('#signup-password', 'short');
    next();
    expect(el.querySelector('.sign-up-page__title-group--step-number')?.textContent).toContain('Step 1 of 3');
    type('#signup-password', 'longenough1!');
    next();
    expect(el.querySelector('.sign-up-page__title-group--step-title')?.textContent).toContain('Tell us about yourself');

    type('#signup-name', 'Aqua Studios');
    type('#sign-up-page__birth-date-group--day-input', '12');
    const month = el.querySelector<HTMLSelectElement>('select')!;
    month.value = '5';
    month.dispatchEvent(new Event('change'));
    type('.sign-up-page__birth-date-group--year-input', '2000');
    el.querySelector<HTMLInputElement>('input[type="radio"]')!.dispatchEvent(new Event('change'));
    fixture.detectChanges();
    next();
    expect(el.querySelector('.sign-up-page__title-group--step-title')?.textContent).toContain('Terms');

    next();
    expect(navigate).not.toHaveBeenCalled();
    const accept = el.querySelector<HTMLInputElement>('#terms-accept')!;
    accept.dispatchEvent(new Event('change'));
    fixture.detectChanges();
    next();
    expect(navigate).toHaveBeenCalledWith('/');
  });

  it('goes back a step', () => {
    const { el, type, next, fixture } = setup('signup');
    type('#signup-email', 'name@domain.com');
    next();
    el.querySelector<HTMLButtonElement>('.sign-up-page__title-group--back-btn')!.click();
    fixture.detectChanges();
    expect(el.querySelector('#signup-email')).toBeTruthy();
  });

  it('switches between code and password login', () => {
    const { el, type, next, fixture } = setup('login');
    expect(el.querySelector('h1')?.textContent).toContain('Welcome back');
    type('#login-identifier', 'aqua');
    next();
    expect(el.querySelectorAll('.sign-up-page__verification-group--code-input').length).toBe(6);

    el.querySelector<HTMLAnchorElement>('.sign-up-page__password-login-group a')!.click();
    fixture.detectChanges();
    expect(el.querySelector('h2')?.textContent).toContain('Log in with a password');
  });
});
