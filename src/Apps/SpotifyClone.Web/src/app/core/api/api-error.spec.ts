import { HttpErrorResponse } from '@angular/common/http';
import { ApiError, toApiError } from './api-error';

const http = (status: number, error: unknown) => new HttpErrorResponse({ status, error });

describe('toApiError', () => {
  it('maps the backend ProblemDetails shape', () => {
    const error = toApiError(
      http(400, {
        title: 'Auth.InvalidPassword',
        detail: 'The provided password is invalid.',
        status: 400,
        errors: [{ code: 'Auth.InvalidPassword', description: 'The provided password is invalid.' }],
      }),
    );

    expect(error).toBeInstanceOf(ApiError);
    expect(error.status).toBe(400);
    expect(error.code).toBe('Auth.InvalidPassword');
    expect(error.message).toBe('The provided password is invalid.');
    expect(error.has('Auth.InvalidPassword')).toBe(true);
    expect(error.has('Auth')).toBe(true);
    expect(error.has('Auth.EmailAlreadyInUse')).toBe(false);
  });

  it('keeps every Identity error', () => {
    const error = toApiError(
      http(400, {
        title: 'Identity.PasswordTooShort',
        errors: [
          { code: 'Identity.PasswordTooShort', description: 'Passwords must be at least 6 characters.' },
          { code: 'Identity.PasswordRequiresUpper', description: "Passwords must have at least one uppercase ('A'-'Z')." },
        ],
      }),
    );
    expect(error.errors.length).toBe(2);
    expect(error.has('Identity.PasswordRequiresUpper')).toBe(true);
  });

  it('maps ASP.NET model validation dictionaries', () => {
    const error = toApiError(http(400, { title: 'One or more validation errors occurred.', errors: { Email: ['The Email field is not a valid e-mail address.'] } }));
    expect(error.has('Validation.Email')).toBe(true);
  });

  it('handles network failures and rate limiting', () => {
    expect(toApiError(http(0, null)).isNetworkError).toBe(true);
    expect(toApiError(http(429, null)).isRateLimited).toBe(true);
  });

  it('falls back to a generic message', () => {
    const error = toApiError(http(500, 'boom'));
    expect(error.code).toBe('Http.500');
    expect(error.message).toContain('Something went wrong');
  });

  it('wraps non-HTTP errors', () => {
    expect(toApiError(new Error('oops')).message).toBe('oops');
  });
});
