import { InjectionToken, inject } from '@angular/core';

/**
 * Origin of the Stopify API. Empty in the browser: requests go to the same origin
 * (`/api/...`), which the dev server proxies and a reverse proxy serves in production.
 * The server-side renderer needs an absolute URL — see `app.config.server.ts`.
 */
export const API_BASE_URL = new InjectionToken<string>('API_BASE_URL', {
  providedIn: 'root',
  factory: () => '',
});

/** Path prefix every versioned endpoint lives under. */
export const API_PREFIX = '/api/v1';

/** Builds a full endpoint URL: `apiUrl('/auth/login')` → `/api/v1/auth/login`. */
export function injectApiUrl(): (path: string) => string {
  const base = inject(API_BASE_URL).replace(/\/$/, '');
  return (path: string) => `${base}${API_PREFIX}${path}`;
}

/** True for requests that target the Stopify API (and may carry the access token). */
export function isApiRequest(url: string, baseUrl: string): boolean {
  const base = baseUrl.replace(/\/$/, '');
  return url.startsWith(`${base}${API_PREFIX}/`);
}
