import { DestroyRef, Injectable, PLATFORM_ID, computed, inject, signal } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import {
  Observable,
  catchError,
  finalize,
  map,
  of,
  shareReplay,
  switchMap,
  tap,
  throwError,
} from 'rxjs';
import { AuthApi } from './auth-api';
import { CurrentUser, RegisterRequest, RegisterResponse, TokenResponse } from './auth.models';
import { toApiError } from '../api/api-error';

/** `unknown` until the refresh cookie has been tried (always `unknown` on the server). */
export type AuthStatus = 'unknown' | 'authenticated' | 'anonymous';

/** Refresh this long before the access token expires. */
const REFRESH_AHEAD_MS = 60_000;
const MIN_REFRESH_DELAY_MS = 5_000;

/**
 * The signed-in session.
 *
 * - The access token is kept in memory only (never in storage), so XSS can't read it from disk.
 * - The refresh token is an HttpOnly cookie owned by the API; a page reload restores the
 *   session through `POST /auth/refresh` (see {@link restoreSession}).
 * - Refreshes are single-flight: concurrent callers share one request, which matters because
 *   the API rotates the refresh token on every use.
 */
@Injectable({ providedIn: 'root' })
export class AuthStore {
  private readonly api = inject(AuthApi);
  private readonly isBrowser = isPlatformBrowser(inject(PLATFORM_ID));

  private readonly _status = signal<AuthStatus>('unknown');
  private readonly _user = signal<CurrentUser | null>(null);
  private accessTokenValue: string | null = null;
  private expiresAt: number | null = null;

  private refresh$: Observable<string> | null = null;
  private restore$: Observable<AuthStatus> | null = null;
  private refreshTimer: ReturnType<typeof setTimeout> | null = null;

  readonly status = this._status.asReadonly();
  readonly user = this._user.asReadonly();
  readonly isAuthenticated = computed(() => this._status() === 'authenticated');

  /** First letter of the display name, for avatar placeholders. */
  readonly initial = computed(() => this._user()?.displayName.trim().charAt(0).toUpperCase() || '?');

  constructor() {
    inject(DestroyRef).onDestroy(() => this.cancelRefreshTimer());
  }

  accessToken(): string | null {
    return this.accessTokenValue;
  }

  /**
   * Resolves the session once per app start by trying the refresh cookie.
   * Safe to call any number of times; on the server it reports `unknown`.
   */
  restoreSession(): Observable<AuthStatus> {
    if (!this.isBrowser) {
      return of<AuthStatus>('unknown');
    }
    if (this._status() !== 'unknown') {
      return of(this._status());
    }
    this.restore$ ??= this.refresh().pipe(
      map((): AuthStatus => 'authenticated'),
      catchError(() => {
        this.clearSession();
        return of<AuthStatus>('anonymous');
      }),
      shareReplay(1),
    );
    return this.restore$;
  }

  /** Exchanges the refresh cookie for a new access token. Emits the token. */
  refresh(): Observable<string> {
    this.refresh$ ??= this.api.refresh().pipe(
      switchMap((tokens) => this.startSession(tokens)),
      finalize(() => (this.refresh$ = null)),
      shareReplay({ bufferSize: 1, refCount: false }),
    );
    return this.refresh$;
  }

  login(identifier: string, password: string): Observable<CurrentUser | null> {
    return this.api.login({ identifier, password }).pipe(
      switchMap((tokens) => this.startSession(tokens)),
      map(() => this._user()),
      catchError((error) => throwError(() => toApiError(error))),
    );
  }

  loginWithOtp(phoneNumber: string, code: string): Observable<CurrentUser | null> {
    return this.api.verifyOtp({ phoneNumber, code }).pipe(
      switchMap((tokens) => this.startSession(tokens)),
      map(() => this._user()),
      catchError((error) => throwError(() => toApiError(error))),
    );
  }

  /** Creates the account; the API signs the new user in straight away. */
  register(request: RegisterRequest): Observable<RegisterResponse> {
    return this.api.register(request).pipe(
      switchMap((response) => this.startSession(response).pipe(map(() => response))),
      catchError((error) => throwError(() => toApiError(error))),
    );
  }

  /** Finishes the Google redirect flow with the token handed over by the API. */
  completeExternalLogin(tokens: TokenResponse): Observable<CurrentUser | null> {
    return this.startSession(tokens).pipe(map(() => this._user()));
  }

  /** Re-reads the current user (e.g. after verifying the email address). */
  reloadUser(): Observable<CurrentUser | null> {
    return this.api.me().pipe(
      tap((user) => this._user.set(user)),
      catchError(() => of(null)),
    );
  }

  /** Ends the session on the server (best effort) and locally (always). */
  logout(): Observable<void> {
    if (this._status() !== 'authenticated') {
      this.clearSession();
      return of(undefined);
    }
    return this.api.logout().pipe(
      catchError(() => of(undefined)),
      finalize(() => this.clearSession()),
      map(() => undefined),
    );
  }

  /** Forgets the session locally (used when a refresh fails). */
  clearSession(): void {
    this.cancelRefreshTimer();
    this.accessTokenValue = null;
    this.expiresAt = null;
    this._user.set(null);
    this.restore$ = null;
    if (this.isBrowser) {
      this._status.set('anonymous');
    }
  }

  private startSession(tokens: TokenResponse): Observable<string> {
    this.accessTokenValue = tokens.accessToken;
    this.expiresAt = Date.parse(tokens.expiresAt);
    this.scheduleRefresh();

    const user$ = this._user() ? of(this._user()) : this.reloadUser();
    return user$.pipe(
      tap(() => this._status.set('authenticated')),
      map(() => tokens.accessToken),
    );
  }

  private scheduleRefresh(): void {
    this.cancelRefreshTimer();
    if (!this.isBrowser || this.expiresAt === null || Number.isNaN(this.expiresAt)) {
      return;
    }
    const delay = Math.max(this.expiresAt - Date.now() - REFRESH_AHEAD_MS, MIN_REFRESH_DELAY_MS);
    this.refreshTimer = setTimeout(() => {
      this.refresh().subscribe({ error: () => this.clearSession() });
    }, delay);
  }

  private cancelRefreshTimer(): void {
    if (this.refreshTimer !== null) {
      clearTimeout(this.refreshTimer);
      this.refreshTimer = null;
    }
  }
}
