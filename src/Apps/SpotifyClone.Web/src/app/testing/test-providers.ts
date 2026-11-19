import { EnvironmentProviders, Provider } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideIcons } from '@core/icons/icons.provider';

/** Providers every component spec needs: a router and the registered icons. */
export function provideTestDefaults(): (Provider | EnvironmentProviders)[] {
  return [provideRouter([]), provideIcons()];
}
