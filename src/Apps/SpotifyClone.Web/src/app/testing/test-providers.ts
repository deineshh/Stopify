import { EnvironmentProviders, Provider } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { provideIcons } from '@core/icons/icons.provider';

/**
 * Providers every component spec needs: a router, the registered icons and a
 * testing HTTP backend (inject HttpTestingController to answer requests).
 */
export function provideTestDefaults(): (Provider | EnvironmentProviders)[] {
  return [provideRouter([]), provideIcons(), provideHttpClient(), provideHttpClientTesting()];
}
