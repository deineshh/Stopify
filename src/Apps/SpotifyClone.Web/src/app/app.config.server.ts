import { ApplicationConfig, REQUEST, inject, mergeApplicationConfig } from '@angular/core';
import { HTTP_TRANSFER_CACHE_ORIGIN_MAP } from '@angular/common/http';
import { provideServerRendering, withRoutes } from '@angular/ssr';
import { appConfig } from './app.config';
import { serverRoutes } from './app.routes.server';
import { API_BASE_URL } from '@core/api/api.config';

/** Where the renderer reaches the API (it runs next to it, not behind the browser's proxy). */
const serverApiUrl = () => process.env['API_URL'] ?? 'http://localhost:5000';

const serverConfig: ApplicationConfig = {
  providers: [
    provideServerRendering(withRoutes(serverRoutes)),
    { provide: API_BASE_URL, useFactory: serverApiUrl },
    // Responses fetched from API_URL are handed to the browser as if they came from the page's
    // own origin, so hydration reuses them instead of requesting everything again.
    {
      provide: HTTP_TRANSFER_CACHE_ORIGIN_MAP,
      useFactory: () => {
        const request = inject(REQUEST, { optional: true });
        return request ? { [new URL(serverApiUrl()).origin]: new URL(request.url).origin } : {};
      },
    },
  ],
};

export const config = mergeApplicationConfig(appConfig, serverConfig);
