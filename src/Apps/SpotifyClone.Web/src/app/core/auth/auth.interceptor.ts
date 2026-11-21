import { HttpErrorResponse, HttpInterceptorFn, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { API_BASE_URL, API_PREFIX, isApiRequest } from '../api/api.config';
import { AuthStore } from './auth.store';

/** Endpoints that must never trigger a refresh-and-retry (they *are* the auth flow). */
const NO_RETRY = /\/auth\/(refresh|login|register|login-otp\/|password-reset\/|email\/verify)/;

/**
 * Adds the bearer token to API requests. On a 401 it refreshes the session once
 * (shared between concurrent requests) and replays the request with the new token;
 * if the refresh fails the session is cleared and the original 401 is rethrown.
 */
export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const baseUrl = inject(API_BASE_URL);
  if (!isApiRequest(request.url, baseUrl)) {
    return next(request);
  }

  const auth = inject(AuthStore);
  const path = request.url.slice(baseUrl.replace(/\/$/, '').length + API_PREFIX.length);

  return next(withToken(request, auth.accessToken())).pipe(
    catchError((error: unknown) => {
      const canRetry =
        error instanceof HttpErrorResponse &&
        error.status === 401 &&
        !NO_RETRY.test(path) &&
        auth.status() !== 'anonymous';
      if (!canRetry) {
        return throwError(() => error);
      }

      return auth.refresh().pipe(
        catchError(() => {
          auth.clearSession();
          return throwError(() => error);
        }),
        switchMap((token) => next(withToken(request, token))),
      );
    }),
  );
};

function withToken<T>(request: HttpRequest<T>, token: string | null): HttpRequest<T> {
  return token ? request.clone({ setHeaders: { Authorization: `Bearer ${token}` } }) : request;
}
