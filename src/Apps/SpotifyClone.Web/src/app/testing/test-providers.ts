import { EnvironmentProviders, Provider } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { provideIcons } from '@core/icons/icons.provider';
import { fakeHub } from './hub.fake';

/**
 * Providers every component spec needs: a router, the registered icons, a
 * testing HTTP backend (inject HttpTestingController to answer requests) and a
 * SignalR hub that never connects (see `fakeHub` to drive it).
 */
export function provideTestDefaults(): (Provider | EnvironmentProviders)[] {
  return [provideRouter([]), provideIcons(), provideHttpClient(), provideHttpClientTesting(), fakeHub().provider];
}
