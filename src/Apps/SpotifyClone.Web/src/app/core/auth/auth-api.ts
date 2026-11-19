import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { injectApiUrl } from '../api/api.config';
import {
  CurrentUser,
  OtpRequest,
  OtpSentResponse,
  OtpVerifyRequest,
  PasswordLoginRequest,
  PasswordResetConfirmRequest,
  PasswordResetRequest,
  PasswordResetRequestedResponse,
  PasswordResetVerifyRequest,
  RegisterRequest,
  RegisterResponse,
  TokenResponse,
  VerifyEmailRequest,
} from './auth.models';

/**
 * Thin typed client for `api/v1/auth` and `api/v1/users/me`.
 * The refresh token travels in an HttpOnly cookie, so every call sends credentials.
 * Session state lives in {@link AuthStore}; components should use that instead.
 */
@Injectable({ providedIn: 'root' })
export class AuthApi {
  private readonly http = inject(HttpClient);
  private readonly url = injectApiUrl();
  private readonly options = { withCredentials: true } as const;

  register(body: RegisterRequest): Observable<RegisterResponse> {
    return this.http.post<RegisterResponse>(this.url('/auth/register'), body, this.options);
  }

  login(body: PasswordLoginRequest): Observable<TokenResponse> {
    return this.http.post<TokenResponse>(this.url('/auth/login'), body, this.options);
  }

  refresh(): Observable<TokenResponse> {
    return this.http.post<TokenResponse>(this.url('/auth/refresh'), null, this.options);
  }

  logout(): Observable<void> {
    return this.http.post<void>(this.url('/auth/logout'), null, this.options);
  }

  requestOtp(body: OtpRequest): Observable<OtpSentResponse> {
    return this.http.post<OtpSentResponse>(this.url('/auth/login-otp/request'), body, this.options);
  }

  verifyOtp(body: OtpVerifyRequest): Observable<TokenResponse> {
    return this.http.post<TokenResponse>(this.url('/auth/login-otp/verify'), body, this.options);
  }

  verifyEmail(body: VerifyEmailRequest): Observable<unknown> {
    return this.http.post(this.url('/auth/email/verify'), body, this.options);
  }

  requestPasswordReset(body: PasswordResetRequest): Observable<PasswordResetRequestedResponse> {
    return this.http.post<PasswordResetRequestedResponse>(
      this.url('/auth/password-reset/request'),
      body,
      this.options,
    );
  }

  verifyPasswordReset(body: PasswordResetVerifyRequest): Observable<unknown> {
    return this.http.post(this.url('/auth/password-reset/verify'), body, this.options);
  }

  confirmPasswordReset(body: PasswordResetConfirmRequest): Observable<unknown> {
    return this.http.post(this.url('/auth/password-reset/confirm'), body, this.options);
  }

  /** Full-page redirect target that starts the Google OAuth flow. */
  googleLoginUrl(): string {
    return this.url('/auth/login-google');
  }

  me(): Observable<CurrentUser> {
    return this.http.get<CurrentUser>(this.url('/users/me'), this.options);
  }
}
