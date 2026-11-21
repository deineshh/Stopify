import { TestBed } from '@angular/core/testing';
import { ComponentFixture } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { AuthPage, ageOn, isPhoneNumber, normalizePhoneNumber } from './auth-page';
import { AuthStore } from '@core/auth/auth.store';
import { provideTestDefaults } from '../../testing/test-providers';
import { TEST_USER } from '../../testing/auth.fixtures';

const inAnHour = () => new Date(Date.now() + 3_600_000).toISOString();
const tokens = () => ({ accessToken: 'token', expiresAt: inAnHour() });
const badRequest = (title: string, detail = title) => [
  { title, detail, status: 400, errors: [{ code: title, description: detail }] },
  { status: 400, statusText: 'Bad Request' },
] as const;

describe('AuthPage', () => {
  let fixture: ComponentFixture<AuthPage>;
  let el: HTMLElement;
  let http: HttpTestingController;
  let navigate: ReturnType<typeof vi.spyOn>;

  function setup(flow: 'signup' | 'login', inputs: Record<string, string> = {}) {
    TestBed.configureTestingModule({
      providers: [...provideTestDefaults(), provideHttpClient(), provideHttpClientTesting()],
    });
    http = TestBed.inject(HttpTestingController);
    navigate = vi.spyOn(TestBed.inject(Router), 'navigateByUrl').mockResolvedValue(true);
    fixture = TestBed.createComponent(AuthPage);
    fixture.componentRef.setInput('flow', flow);
    for (const [name, value] of Object.entries(inputs)) {
      fixture.componentRef.setInput(name, value);
    }
    fixture.detectChanges();
    el = fixture.nativeElement as HTMLElement;
  }

  afterEach(() => {
    TestBed.inject(AuthStore).clearSession();
    http.verify();
  });

  const render = () => fixture.detectChanges();
  const $ = <T extends Element = HTMLElement>(selector: string) => el.querySelector<T & HTMLElement>(selector)!;
  const text = (selector: string) => el.querySelector(selector)?.textContent?.trim() ?? '';
  const type = (selector: string, value: string) => {
    const input = $<HTMLInputElement>(selector);
    input.value = value;
    input.dispatchEvent(new Event('input'));
    render();
  };
  const typeCode = (code: string) => {
    el.querySelectorAll<HTMLInputElement>('app-code-input input').forEach((box, index) => {
      box.value = code[index] ?? '';
      box.dispatchEvent(new Event('input'));
    });
    render();
  };
  const submit = () => {
    $<HTMLButtonElement>('.sign-up-page__next-btn').click();
    render();
  };
  const fillProfile = () => {
    type('#signup-name', 'Aqua Studios');
    type('#sign-up-page__birth-date-group--day-input', '12');
    const month = $<HTMLSelectElement>('select');
    month.value = '5';
    month.dispatchEvent(new Event('change'));
    type('.sign-up-page__birth-date-group--year-input', '2000');
    $<HTMLInputElement>('input[type="radio"][value="female"]').dispatchEvent(new Event('change'));
    render();
  };
  const toTerms = () => {
    type('#signup-email', 'new@stopify.com');
    submit();
    type('#signup-password', 'Pa$$w0rd');
    submit();
    fillProfile();
    submit();
    $<HTMLInputElement>('#terms-accept').dispatchEvent(new Event('change'));
    render();
  };

  // ---------------------------------------------------------------- sign up

  it('validates the email before moving on', () => {
    setup('signup');
    type('#signup-email', 'not-an-email');
    submit();
    expect(text('[role="alert"]')).toContain('This email is invalid');
    type('#signup-email', 'new@stopify.com');
    submit();
    expect(text('.sign-up-page__title-group--step-number')).toBe('Step 1 of 3');
  });

  it('checks the password against the API password policy', () => {
    setup('signup');
    type('#signup-email', 'new@stopify.com');
    submit();
    type('#signup-password', 'password');
    expect(el.querySelectorAll('.sign-up-page__new-password-group--requirement-icon.unmet').length).toBe(3);
    submit();
    expect(text('.sign-up-page__title-group--step-number')).toBe('Step 1 of 3');
    type('#signup-password', 'Pa$$w0rd');
    expect(el.querySelectorAll('.unmet').length).toBe(0);
    submit();
    expect(text('.sign-up-page__title-group--step-title')).toBe('Tell us about yourself');
  });

  it('enforces the API age limits', () => {
    setup('signup');
    type('#signup-email', 'new@stopify.com');
    submit();
    type('#signup-password', 'Pa$$w0rd');
    submit();
    fillProfile();
    type('.sign-up-page__birth-date-group--year-input', String(new Date().getUTCFullYear() - 5));
    submit();
    expect(text('[role="alert"]')).toContain('at least 13');
    type('#sign-up-page__birth-date-group--day-input', '31');
    const month = $<HTMLSelectElement>('select');
    month.value = '2';
    month.dispatchEvent(new Event('change'));
    type('.sign-up-page__birth-date-group--year-input', '2000');
    expect(text('[role="alert"]')).toContain('valid date of birth');
  });

  it('registers, verifies the email and continues to the redirect target', () => {
    setup('signup', { redirect: '/collection' });
    toTerms();
    submit();

    const register = http.expectOne('/api/v1/auth/register');
    expect(register.request.body).toEqual({
      email: 'new@stopify.com',
      password: 'Pa$$w0rd',
      displayName: 'Aqua Studios',
      birthDate: '2000-05-12T00:00:00.000Z',
      gender: 'female',
    });
    expect($<HTMLButtonElement>('.sign-up-page__next-btn').disabled).toBe(true);
    register.flush({ userId: 'u1', email: 'new@stopify.com', displayName: 'Aqua Studios', birthDateUtc: null, gender: 'female', ...tokens() });
    http.expectOne('/api/v1/users/me').flush(TEST_USER);
    render();

    expect(text('h1')).toBe('Verify your email');
    typeCode('123456');
    submit();
    const verify = http.expectOne('/api/v1/auth/email/verify');
    expect(verify.request.body).toEqual({ userId: 'u1', code: '123456' });
    verify.flush({ message: 'Email verified successfully' });
    http.expectOne('/api/v1/users/me').flush(TEST_USER);
    expect(navigate).toHaveBeenCalledWith('/collection');
  });

  it('sends people back to the email step when the address is taken', () => {
    setup('signup');
    toTerms();
    submit();
    http.expectOne('/api/v1/auth/register').flush(
      { title: 'Auth.EmailAlreadyInUse', detail: 'The specified email is already in use.', status: 409, errors: [{ code: 'Auth.EmailAlreadyInUse', description: '' }] },
      { status: 409, statusText: 'Conflict' },
    );
    render();
    expect($('#signup-email')).toBeTruthy();
    expect(text('.sign-up-page__server-error')).toContain('already connected to an account');
    expect($('.sign-up-page__server-error a').getAttribute('href')).toBe('/login');
  });

  it('shows Identity password errors on the password step', () => {
    setup('signup');
    toTerms();
    submit();
    http.expectOne('/api/v1/auth/register').flush(...badRequest('Identity.PasswordRequiresUniqueChars', 'Passwords must use at least 2 different characters.'));
    render();
    expect($('#signup-password')).toBeTruthy();
    expect(text('.sign-up-page__server-error')).toContain('2 different characters');
  });

  it('lets people skip email verification', () => {
    setup('signup');
    toTerms();
    submit();
    http.expectOne('/api/v1/auth/register').flush({ userId: 'u1', email: 'a', displayName: 'a', birthDateUtc: null, gender: 'male', ...tokens() });
    http.expectOne('/api/v1/users/me').flush(TEST_USER);
    render();
    $<HTMLAnchorElement>('.sign-up-page__skip-verify').click();
    expect(navigate).toHaveBeenCalledWith('/');
  });

  // ---------------------------------------------------------------- log in

  it('logs in with a password', () => {
    setup('login');
    type('#login-identifier', 'aqua@studios.com');
    submit();
    expect(text('h1')).toBe('Log in with a password');
    expect($<HTMLInputElement>('#login-username').value).toBe('aqua@studios.com');

    type('[autocomplete="current-password"]', 'secret');
    submit();
    const login = http.expectOne('/api/v1/auth/login');
    expect(login.request.body).toEqual({ identifier: 'aqua@studios.com', password: 'secret' });
    login.flush(tokens());
    http.expectOne('/api/v1/users/me').flush(TEST_USER);
    expect(navigate).toHaveBeenCalledWith('/');
  });

  it('does not reveal whether the identifier or the password was wrong', () => {
    setup('login');
    type('#login-identifier', 'aqua');
    submit();
    type('[autocomplete="current-password"]', 'wrong');
    submit();
    http.expectOne('/api/v1/auth/login').flush(...badRequest('Auth.InvalidPassword'));
    render();
    expect(text('.sign-up-page__server-error')).toBe('Incorrect email, username or password.');
    expect(navigate).not.toHaveBeenCalled();
  });

  it('logs in with an SMS code for phone numbers', () => {
    setup('login');
    type('#login-identifier', '+36 (30) 123-4567');
    submit();
    const request = http.expectOne('/api/v1/auth/login-otp/request');
    expect(request.request.body).toEqual({ phoneNumber: '+36301234567' });
    request.flush({ expiresInSeconds: 300 });
    render();
    expect(text('h1')).toContain('6-digit code');
    expect(text('.sign-up-page__verification-group--resend-btn')).toMatch(/Resend code in \d+s/);

    typeCode('654321');
    submit();
    const verify = http.expectOne('/api/v1/auth/login-otp/verify');
    expect(verify.request.body).toEqual({ phoneNumber: '+36301234567', code: '654321' });
    verify.flush(tokens());
    http.expectOne('/api/v1/users/me').flush(TEST_USER);
    expect(navigate).toHaveBeenCalledWith('/');
  });

  it('reports a wrong SMS code', () => {
    setup('login');
    type('#login-identifier', '+36301234567');
    submit();
    http.expectOne('/api/v1/auth/login-otp/request').flush({ expiresInSeconds: 300 });
    render();
    typeCode('000000');
    submit();
    http.expectOne('/api/v1/auth/login-otp/verify').flush(...badRequest('Otp.Invalid'));
    render();
    expect(text('.sign-up-page__server-error')).toBe('That code is incorrect or has expired.');
  });

  it('resets a forgotten password and logs in with the new one', () => {
    setup('login');
    type('#login-identifier', 'aqua@studios.com');
    submit();
    $<HTMLAnchorElement>('.sign-up-page__forgot-password').click();
    render();
    expect($<HTMLInputElement>('#reset-email').value).toBe('aqua@studios.com');
    submit();
    http.expectOne('/api/v1/auth/password-reset/request').flush({ expiresInSeconds: 600, resendAvailableInSeconds: 60 });
    render();
    expect(text('.sign-up-page__verification-group--resend-btn')).toBe('Resend code in 60s');

    typeCode('111111');
    submit();
    http.expectOne('/api/v1/auth/password-reset/verify').flush(null);
    render();
    expect(text('h1')).toBe('Create a new password');

    type('#reset-password', 'N3w-Passw0rd');
    submit();
    const confirm = http.expectOne('/api/v1/auth/password-reset/confirm');
    expect(confirm.request.body).toEqual({ email: 'aqua@studios.com', code: '111111', newPassword: 'N3w-Passw0rd' });
    confirm.flush(null);
    const login = http.expectOne('/api/v1/auth/login');
    expect(login.request.body).toEqual({ identifier: 'aqua@studios.com', password: 'N3w-Passw0rd' });
    login.flush(tokens());
    http.expectOne('/api/v1/users/me').flush(TEST_USER);
    expect(navigate).toHaveBeenCalledWith('/');
  });

  it('shows network and rate limit errors', () => {
    setup('login');
    type('#login-identifier', 'aqua');
    submit();
    type('[autocomplete="current-password"]', 'x');
    submit();
    http.expectOne('/api/v1/auth/login').flush(null, { status: 429, statusText: 'Too Many Requests' });
    render();
    expect(text('.sign-up-page__server-error')).toContain('Too many attempts');
  });

  it('explains a failed Google sign-in', () => {
    setup('login', { error: 'auth_failed' });
    expect(text('.sign-up-page__server-error')).toContain("Couldn't log in with Google");
  });

  it('ignores redirect targets outside the app', () => {
    setup('login', { redirect: 'https://evil.example' });
    type('#login-identifier', 'aqua');
    submit();
    type('[autocomplete="current-password"]', 'x');
    submit();
    http.expectOne('/api/v1/auth/login').flush(tokens());
    http.expectOne('/api/v1/users/me').flush(TEST_USER);
    expect(navigate).toHaveBeenCalledWith('/');
  });
});

describe('auth helpers', () => {
  it('detects and normalises phone numbers', () => {
    expect(isPhoneNumber('+36 30 123 4567')).toBe(true);
    expect(isPhoneNumber('aqua@studios.com')).toBe(false);
    expect(isPhoneNumber('12345')).toBe(false);
    expect(normalizePhoneNumber('+36 (30) 123-4567')).toBe('+36301234567');
  });

  it('counts age like the API', () => {
    const today = new Date(Date.UTC(2026, 10, 19));
    expect(ageOn(new Date(Date.UTC(2013, 10, 19)), today)).toBe(13);
    expect(ageOn(new Date(Date.UTC(2013, 10, 20)), today)).toBe(12);
  });
});
