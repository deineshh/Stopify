/**
 * Password rules shown while creating or resetting a password.
 *
 * They mirror the API's ASP.NET Identity password options (currently the Identity defaults,
 * configured in Accounts.Infrastructure/DependencyInjection/AccountsModule.cs). If those
 * options change, change these rules too — the API stays the source of truth and its
 * `Identity.Password*` errors are still shown if the two ever disagree.
 */
export interface PasswordRule {
  label: string;
  test: (password: string) => boolean;
}

export const PASSWORD_RULES: readonly PasswordRule[] = [
  { label: '6 characters', test: (value) => value.length >= 6 },
  { label: '1 uppercase and 1 lowercase letter', test: (value) => /[A-Z]/.test(value) && /[a-z]/.test(value) },
  { label: '1 number', test: (value) => /\d/.test(value) },
  { label: '1 special character (example: # ? ! &)', test: (value) => /[^A-Za-z0-9]/.test(value) },
];

export function passwordRuleResults(password: string): { label: string; met: boolean }[] {
  return PASSWORD_RULES.map((rule) => ({ label: rule.label, met: rule.test(password) }));
}
