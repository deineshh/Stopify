import { PLATFORM_ID, inject } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { CanActivateFn, Router } from '@angular/router';
import { map } from 'rxjs';
import { AuthStore } from './auth.store';

/**
 * Signed-in users only; everyone else goes to `/login?redirect=…`.
 * On the server the session is unknown, so protected routes are rendered on the client
 * (see `app.routes.server.ts`) and the guard lets the server pass.
 */
export const authGuard: CanActivateFn = (_route, state) => {
  if (!isPlatformBrowser(inject(PLATFORM_ID))) {
    return true;
  }
  const router = inject(Router);
  return inject(AuthStore)
    .restoreSession()
    .pipe(
      map((status) =>
        status === 'authenticated'
          ? true
          : router.createUrlTree(['/login'], { queryParams: { redirect: state.url } }),
      ),
    );
};

/** Sign up / log in pages: signed-in users are sent home. */
export const guestGuard: CanActivateFn = () => {
  if (!isPlatformBrowser(inject(PLATFORM_ID))) {
    return true;
  }
  const router = inject(Router);
  return inject(AuthStore)
    .restoreSession()
    .pipe(map((status) => (status === 'authenticated' ? router.createUrlTree(['/']) : true)));
};

/** Only follow same-app redirect targets (`/collection`), never `https://evil.example`. */
export function safeRedirect(target: string | null | undefined, fallback = '/'): string {
  return target && target.startsWith('/') && !target.startsWith('//') ? target : fallback;
}
