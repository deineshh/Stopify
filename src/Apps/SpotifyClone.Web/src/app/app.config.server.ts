import { mergeApplicationConfig, ApplicationConfig } from '@angular/core';
import { provideServerRendering, withRoutes } from '@angular/ssr';
import { appConfig } from './app.config';
import { serverRoutes } from './app.routes.server';
import { API_BASE_URL } from '@core/api/api.config';

const serverConfig: ApplicationConfig = {
  providers: [
    provideServerRendering(withRoutes(serverRoutes)),
    // The renderer runs next to the API, not behind the browser's proxy, so it needs an absolute URL.
    { provide: API_BASE_URL, useFactory: () => process.env['API_URL'] ?? 'http://localhost:5000' },
  ],
};

export const config = mergeApplicationConfig(appConfig, serverConfig);
