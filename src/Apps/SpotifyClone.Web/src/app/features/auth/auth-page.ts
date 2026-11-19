import { ChangeDetectionStrategy, Component, computed, inject, input, signal } from '@angular/core';
import { NgTemplateOutlet } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';

export type AuthFlow = 'signup' | 'login';

type Step =
  // sign up
  | 'signup-email'
  | 'signup-password'
  | 'signup-profile'
  | 'signup-terms'
  // log in
  | 'login-identifier'
  | 'login-code'
  | 'login-password';

const SIGNUP_STEPS: readonly Step[] = ['signup-email', 'signup-password', 'signup-profile', 'signup-terms'];

const STEP_TITLES: Partial<Record<Step, string>> = {
  'signup-password': 'Create a password',
  'signup-profile': 'Tell us about yourself',
  'signup-terms': 'Terms & Conditions',
};

const EMAIL_PATTERN = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
const CODE_LENGTH = 6;

export const GENDERS = ['Man', 'Woman', 'Non-binary', 'Something else', 'Prefer not to say'] as const;
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

/**
 * Sign up and log in screens (mockups/sign-up.html). The mockup stacks every step in one page
 * and toggles them with CSS; here each step is a state of a small wizard. Validation is
 * client-side only — nothing is submitted until the Accounts API is wired in.
 */
@Component({
  selector: 'app-auth-page',
  imports: [NgTemplateOutlet, RouterLink, FaIconComponent],
  templateUrl: './auth-page.html',
  styleUrl: './auth-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AuthPage {
  /** Bound from the route's `data.flow`. */
  readonly flow = input<AuthFlow>('signup');

  private readonly router = inject(Router);

  protected readonly genders = GENDERS;
  protected readonly months = MONTHS;
  protected readonly codeSlots = Array.from({ length: CODE_LENGTH }, (_, index) => index);

  private readonly stepOverride = signal<Step | null>(null);
  protected readonly step = computed<Step>(
    () => this.stepOverride() ?? (this.flow() === 'signup' ? 'signup-email' : 'login-identifier'),
  );

  /** "Next" was pressed on the current step — from then on its errors are shown. */
  protected readonly submitted = signal(false);

  // ---------------------------------------------------------------- form values

  protected readonly email = signal('');
  protected readonly password = signal('');
  protected readonly showPassword = signal(false);
  protected readonly name = signal('');
  protected readonly birthDay = signal('');
  protected readonly birthMonth = signal('0');
  protected readonly birthYear = signal('');
  protected readonly gender = signal<string | null>(null);
  protected readonly marketingOptOut = signal(false);
  protected readonly shareData = signal(false);
  protected readonly acceptTerms = signal(false);
  protected readonly code = signal<readonly string[]>(Array(CODE_LENGTH).fill(''));

  // ---------------------------------------------------------------- validation

  protected readonly emailValid = computed(() => EMAIL_PATTERN.test(this.email().trim()));
  protected readonly identifierValid = computed(() => this.email().trim().length > 0);

  protected readonly passwordRules = computed(() => {
    const value = this.password();
    return [
      { label: '1 letter', met: /[A-Za-z]/.test(value) },
      { label: '1 number or special character (example: # ? ! &)', met: /[\d\W_]/.test(value) },
      { label: '10 characters', met: value.length >= 10 },
    ];
  });
  protected readonly passwordValid = computed(() => this.passwordRules().every((rule) => rule.met));

  protected readonly nameValid = computed(() => this.name().trim().length > 0);
  protected readonly birthDateValid = computed(() => {
    const day = Number(this.birthDay());
    const month = Number(this.birthMonth());
    const year = Number(this.birthYear());
    const currentYear = new Date().getFullYear();
    return day >= 1 && day <= 31 && month >= 1 && year >= 1900 && year <= currentYear;
  });
  protected readonly genderValid = computed(() => this.gender() !== null);
  protected readonly codeValid = computed(() => this.code().every((digit) => /^\d$/.test(digit)));

  // ---------------------------------------------------------------- header

  protected readonly signupStepIndex = computed(() => SIGNUP_STEPS.indexOf(this.step()));
  protected readonly stepTitle = computed(() => STEP_TITLES[this.step()]);
  protected readonly showBackHeader = computed(() => {
    const step = this.step();
    return step !== 'signup-email' && step !== 'login-identifier';
  });

  // ---------------------------------------------------------------- actions

  protected next(): void {
    this.submitted.set(true);
    if (!this.isStepValid()) {
      return;
    }

    const step = this.step();
    switch (step) {
      case 'signup-email':
      case 'signup-password':
      case 'signup-profile':
        this.goTo(SIGNUP_STEPS[SIGNUP_STEPS.indexOf(step) + 1]);
        break;
      case 'login-identifier':
        this.goTo('login-code');
        break;
      case 'signup-terms':
      case 'login-code':
      case 'login-password':
        void this.router.navigateByUrl('/');
        break;
    }
  }

  protected back(): void {
    const step = this.step();
    const signupIndex = SIGNUP_STEPS.indexOf(step);
    if (signupIndex > 0) {
      this.goTo(SIGNUP_STEPS[signupIndex - 1]);
    } else {
      this.goTo('login-identifier');
    }
  }

  protected goTo(step: Step): void {
    this.submitted.set(false);
    this.showPassword.set(false);
    this.stepOverride.set(step);
  }

  protected setCodeDigit(index: number, input: HTMLInputElement): void {
    const digit = input.value.replace(/\D/g, '').slice(-1);
    input.value = digit;
    this.code.update((code) => code.map((value, i) => (i === index ? digit : value)));
    if (digit) {
      (input.nextElementSibling as HTMLInputElement | null)?.focus();
    }
  }

  protected onCodeBackspace(index: number, input: HTMLInputElement): void {
    if (input.value === '' && index > 0) {
      (input.previousElementSibling as HTMLInputElement | null)?.focus();
    }
  }

  protected pasteCode(event: ClipboardEvent): void {
    const digits = (event.clipboardData?.getData('text') ?? '').replace(/\D/g, '').slice(0, CODE_LENGTH);
    if (!digits) {
      return;
    }
    event.preventDefault();
    const inputs = (event.currentTarget as HTMLElement).querySelectorAll('input');
    this.code.set(Array.from({ length: CODE_LENGTH }, (_, i) => digits[i] ?? ''));
    inputs.forEach((input, i) => (input.value = digits[i] ?? ''));
    inputs[Math.min(digits.length, CODE_LENGTH - 1)]?.focus();
  }

  private isStepValid(): boolean {
    switch (this.step()) {
      case 'signup-email':
        return this.emailValid();
      case 'signup-password':
        return this.passwordValid();
      case 'signup-profile':
        return this.nameValid() && this.birthDateValid() && this.genderValid();
      case 'signup-terms':
        return this.acceptTerms();
      case 'login-identifier':
        return this.identifierValid();
      case 'login-code':
        return this.codeValid();
      case 'login-password':
        return this.identifierValid() && this.password().length > 0;
    }
  }
}
