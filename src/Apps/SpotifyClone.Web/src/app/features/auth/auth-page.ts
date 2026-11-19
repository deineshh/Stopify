import {
  ChangeDetectionStrategy,
  Component,
  DOCUMENT,
  DestroyRef,
  computed,
  inject,
  input,
  linkedSignal,
  signal,
} from '@angular/core';
import { NgTemplateOutlet } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import { Observable, finalize, switchMap } from 'rxjs';
import { AuthStore } from '@core/auth/auth.store';
import { AuthApi } from '@core/auth/auth-api';
import { GenderValue } from '@core/auth/auth.models';
import { ApiError, toApiError } from '@core/api/api-error';
import { safeRedirect } from '@core/auth/auth.guards';
import { CodeInput } from './code-input/code-input';
import { passwordRuleResults } from './password-policy';

export type AuthFlow = 'signup' | 'login';

export type AuthStep =
  // sign up
  | 'signup-email'
  | 'signup-password'
  | 'signup-profile'
  | 'signup-terms'
  | 'signup-verify'
  // log in
  | 'login-identifier'
  | 'login-code'
  | 'login-password'
  // forgotten password
  | 'reset-email'
  | 'reset-code'
  | 'reset-password';

const SIGNUP_STEPS: readonly AuthStep[] = ['signup-email', 'signup-password', 'signup-profile', 'signup-terms'];

const STEP_TITLES: Partial<Record<AuthStep, string>> = {
  'signup-password': 'Create a password',
  'signup-profile': 'Tell us about yourself',
  'signup-terms': 'Terms & Conditions',
};

/** Where "Go back" leads from each step. */
const PREVIOUS_STEP: Partial<Record<AuthStep, AuthStep>> = {
  'signup-password': 'signup-email',
  'signup-profile': 'signup-password',
  'signup-terms': 'signup-profile',
  'login-code': 'login-identifier',
  'login-password': 'login-identifier',
  'reset-email': 'login-password',
  'reset-code': 'reset-email',
  'reset-password': 'reset-code',
};

const SUBMIT_LABELS: Record<AuthStep, string> = {
  'signup-email': 'Next',
  'signup-password': 'Next',
  'signup-profile': 'Next',
  'signup-terms': 'Sign up',
  'signup-verify': 'Verify',
  'login-identifier': 'Continue',
  'login-code': 'Log in',
  'login-password': 'Log in',
  'reset-email': 'Send code',
  'reset-code': 'Continue',
  'reset-password': 'Save password',
};

const EMAIL_PATTERN = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
const PHONE_PATTERN = /^\+?[\d\s().-]+$/;
const CODE_LENGTH = 6;
const DISPLAY_NAME_MAX = 32; // DisplayNameRules.MaxLength
const MIN_AGE = 13; // BirthDateRules.MinimumAge
const MAX_AGE = 120; // BirthDateRules.MaximumAge
const DEFAULT_RESEND_SECONDS = 30;

export const GENDERS: readonly { label: string; value: GenderValue }[] = [
  { label: 'Man', value: 'male' },
  { label: 'Woman', value: 'female' },
  { label: 'Non-binary', value: 'non_binary' },
  { label: 'Something else', value: 'something_else' },
  { label: 'Prefer not to say', value: 'not_specified' },
];

export const MONTHS = [
  'January',
  'February',
  'March',
  'April',
  'May',
  'June',
  'July',
  'August',
  'September',
  'October',
  'November',
  'December',
] as const;

interface ServerError {
  message: string;
  /** Show a "Log in" link next to the message (e.g. the email is already registered). */
  offerLogin?: boolean;
}

/**
 * Sign up, log in (password or SMS code), Google sign-in and password reset, talking to the
 * Accounts API. Each screen of the mockup is one step of a small state machine.
 */
@Component({
  selector: 'app-auth-page',
  imports: [NgTemplateOutlet, RouterLink, FaIconComponent, CodeInput],
  templateUrl: './auth-page.html',
  styleUrl: './auth-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AuthPage {
  /** Bound from the route's `data.flow`. */
  readonly flow = input<AuthFlow>('signup');
  /** `?redirect=` — where to go after signing in. */
  readonly redirect = input<string>();
  /** `?error=` — set by the API when the Google redirect flow fails. */
  readonly error = input<string>();

  private readonly auth = inject(AuthStore);
  private readonly api = inject(AuthApi);
  private readonly router = inject(Router);
  private readonly document = inject(DOCUMENT);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly genders = GENDERS;
  protected readonly months = MONTHS;

  private readonly stepOverride = signal<AuthStep | null>(null);
  protected readonly step = computed<AuthStep>(
    () => this.stepOverride() ?? (this.flow() === 'signup' ? 'signup-email' : 'login-identifier'),
  );
  protected readonly submitLabel = computed(() => SUBMIT_LABELS[this.step()]);

  /** "Next" was pressed on the current step — from then on its errors are shown. */
  protected readonly submitted = signal(false);
  /** A request is in flight. */
  protected readonly pending = signal(false);
  protected readonly serverError = linkedSignal<ServerError | null>(() =>
    this.error() === 'auth_failed'
      ? { message: "Couldn't log in with Google. Try again or choose another way to log in." }
      : null,
  );

  // ---------------------------------------------------------------- form values

  protected readonly email = signal('');
  protected readonly identifier = signal('');
  protected readonly phoneNumber = signal('');
  protected readonly password = signal('');
  protected readonly showPassword = signal(false);
  protected readonly name = signal('');
  protected readonly birthDay = signal('');
  protected readonly birthMonth = signal('0');
  protected readonly birthYear = signal('');
  protected readonly gender = signal<GenderValue | null>(null);
  protected readonly marketingOptOut = signal(false);
  protected readonly shareData = signal(false);
  protected readonly acceptTerms = signal(false);
  protected readonly code = signal('');

  private registeredUserId: string | null = null;

  // ---------------------------------------------------------------- validation

  protected readonly emailValid = computed(() => EMAIL_PATTERN.test(this.email().trim()));
  protected readonly identifierValid = computed(() => this.identifier().trim().length > 0);
  protected readonly passwordRules = computed(() => passwordRuleResults(this.password()));
  protected readonly passwordValid = computed(() => this.passwordRules().every((rule) => rule.met));
  protected readonly codeValid = computed(() => new RegExp(`^\\d{${CODE_LENGTH}}$`).test(this.code()));

  protected readonly nameError = computed(() => {
    const name = this.name().trim();
    if (!name) {
      return 'Enter a name for your profile.';
    }
    if (name.length > DISPLAY_NAME_MAX) {
      return `Your name can be up to ${DISPLAY_NAME_MAX} characters.`;
    }
    // eslint-disable-next-line no-control-regex
    return /[\u0000-\u001f\u007f]/.test(name) ? 'Your name contains characters that aren\'t allowed.' : null;
  });

  /** The entered birth date as UTC midnight, or `null` if it isn't a real date. */
  private readonly birthDate = computed(() => {
    const day = Number(this.birthDay());
    const month = Number(this.birthMonth());
    const year = Number(this.birthYear());
    if (!Number.isInteger(day) || !Number.isInteger(year) || month < 1 || year < 1000) {
      return null;
    }
    const date = new Date(Date.UTC(year, month - 1, day));
    const isRealDate =
      date.getUTCFullYear() === year && date.getUTCMonth() === month - 1 && date.getUTCDate() === day;
    return isRealDate ? date : null;
  });

  protected readonly birthDateError = computed(() => {
    if (!this.birthDay() && this.birthMonth() === '0' && !this.birthYear()) {
      return 'Enter your date of birth.';
    }
    const date = this.birthDate();
    if (!date) {
      return 'Enter a valid date of birth.';
    }
    const age = ageOn(date, new Date());
    if (age < 0 || age > MAX_AGE) {
      return 'Enter a valid date of birth.';
    }
    return age < MIN_AGE ? `You need to be at least ${MIN_AGE} years old to create an account.` : null;
  });

  /** Phone numbers log in with an SMS code; everything else with a password. */
  protected readonly identifierIsPhone = computed(() => isPhoneNumber(this.identifier()));

  // ---------------------------------------------------------------- header

  protected readonly signupStepIndex = computed(() => SIGNUP_STEPS.indexOf(this.step()));
  protected readonly stepTitle = computed(() => STEP_TITLES[this.step()]);
  protected readonly showBackHeader = computed(() => PREVIOUS_STEP[this.step()] !== undefined);

  // ---------------------------------------------------------------- resend cooldown

  protected readonly resendIn = signal(0);
  private resendTimer: ReturnType<typeof setInterval> | null = null;

  constructor() {
    this.destroyRef.onDestroy(() => this.stopResendTimer());
  }

  // ---------------------------------------------------------------- actions

  protected next(): void {
    this.submitted.set(true);
    this.serverError.set(null);
    if (this.pending() || !this.isStepValid()) {
      return;
    }

    switch (this.step()) {
      case 'signup-email':
        return this.goTo('signup-password');
      case 'signup-password':
        return this.goTo('signup-profile');
      case 'signup-profile':
        return this.goTo('signup-terms');
      case 'signup-terms':
        return this.run(
          this.auth.register({
            email: this.email().trim(),
            password: this.password(),
            displayName: this.name().trim(),
            birthDate: this.birthDate()!.toISOString(),
            gender: this.gender()!,
          }),
          (response) => {
            this.registeredUserId = response.userId;
            this.goTo('signup-verify');
          },
        );
      case 'signup-verify':
        return this.run(
          this.api
            .verifyEmail({ userId: this.registeredUserId!, code: this.code() })
            .pipe(switchMap(() => this.auth.reloadUser())),
          () => this.finish(),
        );
      case 'login-identifier':
        if (this.identifierIsPhone()) {
          return this.requestOtp(() => this.goTo('login-code'));
        }
        return this.goTo('login-password');
      case 'login-code':
        return this.run(this.auth.loginWithOtp(this.phoneNumber(), this.code()), () => this.finish());
      case 'login-password':
        return this.run(this.auth.login(this.identifier().trim(), this.password()), () => this.finish());
      case 'reset-email':
        return this.requestPasswordReset(() => this.goTo('reset-code'));
      case 'reset-code':
        return this.run(
          this.api.verifyPasswordReset({ email: this.email().trim(), code: this.code() }),
          () => this.goTo('reset-password', { keepCode: true }),
        );
      case 'reset-password':
        return this.run(
          this.api
            .confirmPasswordReset({ email: this.email().trim(), code: this.code(), newPassword: this.password() })
            .pipe(switchMap(() => this.auth.login(this.email().trim(), this.password()))),
          () => this.finish(),
        );
    }
  }

  protected back(): void {
    const previous = PREVIOUS_STEP[this.step()];
    if (previous) {
      this.goTo(previous);
    }
  }

  protected goTo(step: AuthStep, options: { keepCode?: boolean } = {}): void {
    this.submitted.set(false);
    this.serverError.set(null);
    this.showPassword.set(false);
    if (!options.keepCode) {
      this.code.set('');
    }
    if (step === 'reset-email' && !this.email() && EMAIL_PATTERN.test(this.identifier().trim())) {
      this.email.set(this.identifier().trim());
    }
    if (step === 'reset-password' || step === 'login-password') {
      this.password.set('');
    }
    this.stepOverride.set(step);
  }

  protected resendCode(): void {
    if (this.resendIn() > 0 || this.pending()) {
      return;
    }
    this.serverError.set(null);
    if (this.step() === 'login-code') {
      this.requestOtp();
    } else if (this.step() === 'reset-code') {
      this.requestPasswordReset();
    }
  }

  /** The account already exists and is signed in; the email can be verified later. */
  protected skipVerification(): void {
    this.finish();
  }

  protected loginWithGoogle(): void {
    this.document.defaultView?.location.assign(this.api.googleLoginUrl());
  }

  // ---------------------------------------------------------------- helpers

  private requestOtp(then?: () => void): void {
    const phoneNumber = normalizePhoneNumber(this.identifier());
    this.run(this.api.requestOtp({ phoneNumber }), () => {
      this.phoneNumber.set(phoneNumber);
      this.startResendCooldown(DEFAULT_RESEND_SECONDS);
      then?.();
    });
  }

  private requestPasswordReset(then?: () => void): void {
    this.run(this.api.requestPasswordReset({ email: this.email().trim() }), (response) => {
      this.startResendCooldown(response?.resendAvailableInSeconds ?? DEFAULT_RESEND_SECONDS);
      then?.();
    });
  }

  private run<T>(request: Observable<T>, onSuccess: (value: T) => void): void {
    this.pending.set(true);
    request
      .pipe(
        finalize(() => this.pending.set(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe({
        next: onSuccess,
        error: (error: unknown) => this.showError(toApiError(error)),
      });
  }

  private showError(error: ApiError): void {
    const step = this.step();

    if (error.has('Auth.EmailAlreadyInUse')) {
      this.goTo('signup-email');
      this.serverError.set({ message: 'This email is already connected to an account.', offerLogin: true });
      return;
    }
    if (step === 'signup-terms' && error.has('Identity') && error.errors.some((e) => e.code.includes('Password'))) {
      this.goTo('signup-password');
      this.serverError.set({ message: error.errors.map((e) => e.description).join(' ') });
      return;
    }
    if (error.has('Auth.InvalidIdentifier') || error.has('Auth.InvalidPassword')) {
      this.serverError.set({ message: 'Incorrect email, username or password.' });
      return;
    }
    if (error.has('Auth.SignInNotAllowed')) {
      this.serverError.set({ message: "This account can't log in right now. Check your email for a verification code." });
      return;
    }
    const isCodeStep = step === 'signup-verify' || step === 'login-code' || step === 'reset-code';
    if (isCodeStep && !error.isNetworkError && !error.isRateLimited && error.status < 500) {
      this.serverError.set({ message: 'That code is incorrect or has expired.' });
      return;
    }
    this.serverError.set({ message: error.message });
  }

  private finish(): void {
    void this.router.navigateByUrl(safeRedirect(this.redirect()));
  }

  private startResendCooldown(seconds: number): void {
    this.stopResendTimer();
    this.resendIn.set(Math.max(0, Math.ceil(seconds)));
    this.resendTimer = setInterval(() => {
      this.resendIn.update((value) => Math.max(0, value - 1));
      if (this.resendIn() === 0) {
        this.stopResendTimer();
      }
    }, 1000);
  }

  private stopResendTimer(): void {
    if (this.resendTimer !== null) {
      clearInterval(this.resendTimer);
      this.resendTimer = null;
    }
  }

  private isStepValid(): boolean {
    switch (this.step()) {
      case 'signup-email':
      case 'reset-email':
        return this.emailValid();
      case 'signup-password':
      case 'reset-password':
        return this.passwordValid();
      case 'signup-profile':
        return !this.nameError() && !this.birthDateError() && this.gender() !== null;
      case 'signup-terms':
        return this.acceptTerms();
      case 'login-identifier':
        return this.identifierValid();
      case 'login-password':
        return this.identifierValid() && this.password().length > 0;
      case 'signup-verify':
      case 'login-code':
      case 'reset-code':
        return this.codeValid();
    }
  }
}

/** Age in whole years, the same way the API's BirthDateRules counts it. */
export function ageOn(birthDate: Date, today: Date): number {
  let age = today.getUTCFullYear() - birthDate.getUTCFullYear();
  const birthdayThisYear = Date.UTC(today.getUTCFullYear(), birthDate.getUTCMonth(), birthDate.getUTCDate());
  if (birthdayThisYear > Date.UTC(today.getUTCFullYear(), today.getUTCMonth(), today.getUTCDate())) {
    age--;
  }
  return age;
}

export function isPhoneNumber(value: string): boolean {
  const trimmed = value.trim();
  return PHONE_PATTERN.test(trimmed) && trimmed.replace(/\D/g, '').length >= 7;
}

/** "+36 (30) 123-4567" → "+36301234567". */
export function normalizePhoneNumber(value: string): string {
  const trimmed = value.trim();
  return (trimmed.startsWith('+') ? '+' : '') + trimmed.replace(/\D/g, '');
}
