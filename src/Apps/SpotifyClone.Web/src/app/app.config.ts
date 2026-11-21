import { ApplicationConfig, DOCUMENT, inject, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';
import {
  provideRouter,
  withComponentInputBinding,
  withInMemoryScrolling,
  withViewTransitions,
} from '@angular/router';
import { provideClientHydration, withEventReplay } from '@angular/platform-browser';

import { routes } from './app.routes';
import { provideIcons } from '@core/icons/icons.provider';
import { authInterceptor } from '@core/auth/auth.interceptor';
import { API_BASE_URL } from '@core/api/api.config';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(
      routes,
      withComponentInputBinding(),
      withInMemoryScrolling({ scrollPositionRestoration: 'disabled' }),
      withViewTransitions({ skipInitialTransition: true }),
    ),
    provideHttpClient(withFetch(), withInterceptors([authInterceptor])),
    provideClientHydration(withEventReplay()),
    provideIcons(),
    // Absolute in the browser so server-rendered responses can be reused from the transfer cache
    // (the server maps its own API origin onto this one, see app.config.server.ts).
    { provide: API_BASE_URL, useFactory: () => inject(DOCUMENT).location.origin },
  ],
};
