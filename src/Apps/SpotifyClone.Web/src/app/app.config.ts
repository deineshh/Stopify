import { ApplicationConfig, provideBrowserGlobalErrorListeners, APP_INITIALIZER, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors, withFetch } from '@angular/common/http';
import { FaIconLibrary } from '@fortawesome/angular-fontawesome';
import { firstValueFrom } from 'rxjs';

import { routes } from './app.routes';
import { provideClientHydration, withEventReplay } from '@angular/platform-browser';
import { registerIcons } from './shared/icons';
import { authInterceptor } from './core/interceptors/auth.interceptor';
import { AuthService } from './core/services/auth.service';
import { LoggingService } from './core/services/logging.service';

function autoLoginFactory(auth: AuthService, logger: LoggingService, platformId: object): () => Promise<void> {
  return async () => {
    if (!isPlatformBrowser(platformId)) {
      return;
    }

    try {
      logger.info('Attempting token refresh');
      await firstValueFrom(auth.refreshToken());
      logger.info('Token refresh successful');
    } catch {
      try {
        logger.info('Refresh failed, auto-login with default credentials');
        await firstValueFrom(
          auth.login({
            identifier: 'stopify@gmail.com',
            password: 'Admin_Password123',
          }),
        );
        logger.info('Auto-login successful');
      } catch (err) {
        logger.error('Auto-login failed', err);
      }
    }
  };
}

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideClientHydration(withEventReplay()),
    provideHttpClient(withFetch(), withInterceptors([authInterceptor])),
    {
      provide: APP_INITIALIZER,
      useFactory: autoLoginFactory,
      deps: [AuthService, LoggingService, PLATFORM_ID],
      multi: true,
    },
    {
      provide: 'ICON_REGISTRY',
      useFactory: (library: FaIconLibrary) => registerIcons(library),
      deps: [FaIconLibrary],
    },
  ]
};
