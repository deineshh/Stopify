import { ChangeDetectionStrategy, Component, DOCUMENT, afterNextRender, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthStore } from '@core/auth/auth.store';
import { jwtExpiry } from '@core/auth/jwt';

const ACCESS_TOKEN_COOKIE = 'tempAccessToken';
const EXPIRES_AT_COOKIE = 'tempExpiresAt';
const FALLBACK_LIFETIME_MS = 5 * 60_000;

/**
 * `/auth-success` — where the API sends the browser after Google sign-in. The API leaves the
 * access token in a short-lived, readable cookie (the refresh token is already in its HttpOnly
 * cookie); this page moves it into memory, deletes the cookies and continues to the app.
 */
@Component({
  selector: 'app-auth-callback-page',
  template: `<p class="auth-callback" role="status">Logging you in…</p>`,
  styles: `
    :host {
      align-items: center;
      background-color: var(--secondary-bg-color);
      display: flex;
      height: 100%;
      justify-content: center;
    }

    .auth-callback {
      color: var(--light-gray-fg-color);
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AuthCallbackPage {
  private readonly document = inject(DOCUMENT);
  private readonly auth = inject(AuthStore);
  private readonly router = inject(Router);

  constructor() {
    afterNextRender(() => this.complete());
  }

  private complete(): void {
    const accessToken = this.readCookie(ACCESS_TOKEN_COOKIE);
    const expiresAtCookie = this.readCookie(EXPIRES_AT_COOKIE);
    this.deleteCookie(ACCESS_TOKEN_COOKIE);
    this.deleteCookie(EXPIRES_AT_COOKIE);

    if (!accessToken) {
      void this.router.navigate(['/login'], { queryParams: { error: 'auth_failed' }, replaceUrl: true });
      return;
    }

    const expiresAt =
      jwtExpiry(accessToken) ??
      parseDate(expiresAtCookie) ??
      new Date(Date.now() + FALLBACK_LIFETIME_MS);

    this.auth.completeExternalLogin({ accessToken, expiresAt: expiresAt.toISOString() }).subscribe({
      next: () => void this.router.navigateByUrl('/', { replaceUrl: true }),
      error: () =>
        void this.router.navigate(['/login'], { queryParams: { error: 'auth_failed' }, replaceUrl: true }),
    });
  }

  private readCookie(name: string): string | null {
    const prefix = `${name}=`;
    const entry = this.document.cookie.split('; ').find((part) => part.startsWith(prefix));
    return entry ? decodeURIComponent(entry.slice(prefix.length)) : null;
  }

  private deleteCookie(name: string): void {
    this.document.cookie = `${name}=; Max-Age=0; Path=/; SameSite=Lax`;
  }
}

function parseDate(value: string | null): Date | null {
  if (!value) {
    return null;
  }
  const time = Date.parse(value);
  return Number.isNaN(time) ? null : new Date(time);
}
