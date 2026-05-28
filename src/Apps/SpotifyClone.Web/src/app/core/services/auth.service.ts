import { Injectable, inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, catchError, throwError, BehaviorSubject } from 'rxjs';
import { LoginRequest, LoginResponse, RefreshResponse, AuthState } from '../models/auth.models';
import { LoggingService } from './logging.service';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly logger = inject(LoggingService);
  private readonly platformId = inject(PLATFORM_ID);

  private readonly ACCESS_TOKEN_KEY = 'stopify_access_token';
  private readonly authState = new BehaviorSubject<AuthState>(this.loadInitialState());

  readonly authState$ = this.authState.asObservable();

  private get isBrowser(): boolean {
    return isPlatformBrowser(this.platformId);
  }

  private loadInitialState(): AuthState {
    if (!this.isBrowser) return { isAuthenticated: false, accessToken: null };

    try {
      const token = localStorage.getItem(this.ACCESS_TOKEN_KEY);
      return { isAuthenticated: !!token, accessToken: token };
    } catch {
      return { isAuthenticated: false, accessToken: null };
    }
  }

  login(credentials: LoginRequest): Observable<LoginResponse> {
    this.logger.info('Attempting login', { identifier: credentials.identifier });

    return this.http.post<LoginResponse>('/api/v1/auth/login', credentials).pipe(
      tap((response) => {
        this.setAccessToken(response.accessToken);
        this.logger.info('Login successful');
      }),
      catchError((error) => {
        this.logger.error('Login failed', error);
        return throwError(() => error);
      }),
    );
  }

  refreshToken(): Observable<RefreshResponse> {
    this.logger.debug('Refreshing access token');

    return this.http.post<RefreshResponse>('/api/v1/auth/refresh', {}).pipe(
      tap((response) => {
        this.setAccessToken(response.accessToken);
        this.logger.info('Token refresh successful');
      }),
      catchError((error) => {
        this.logger.error('Token refresh failed', error);
        this.logout();
        return throwError(() => error);
      }),
    );
  }

  logout(): void {
    if (this.isBrowser) {
      try { localStorage.removeItem(this.ACCESS_TOKEN_KEY); } catch { /* noop */ }
    }
    this.authState.next({ isAuthenticated: false, accessToken: null });
    this.logger.info('Logged out');
  }

  getAccessToken(): string | null {
    return this.authState.value.accessToken;
  }

  isAuthenticated(): boolean {
    return this.authState.value.isAuthenticated;
  }

  private setAccessToken(token: string): void {
    if (this.isBrowser) {
      try { localStorage.setItem(this.ACCESS_TOKEN_KEY, token); } catch { /* noop */ }
    }
    this.authState.next({ isAuthenticated: true, accessToken: token });
  }
}
