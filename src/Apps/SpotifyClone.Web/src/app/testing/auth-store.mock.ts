import { computed, signal } from '@angular/core';
import { of } from 'rxjs';
import { AuthStatus, AuthStore } from '@core/auth/auth.store';
import { CurrentUser } from '@core/auth/auth.models';
import { TEST_USER } from './auth.fixtures';

/** Signal-driven AuthStore stand-in for component specs. */
export function mockAuthStore(initial: AuthStatus = 'anonymous', user: CurrentUser | null = TEST_USER) {
  const status = signal<AuthStatus>(initial);
  const currentUser = signal<CurrentUser | null>(initial === 'authenticated' ? user : null);
  const mock = {
    status,
    user: currentUser,
    isAuthenticated: computed(() => status() === 'authenticated'),
    initial: computed(() => currentUser()?.displayName.charAt(0).toUpperCase() ?? '?'),
    restoreSession: vi.fn(() => of(status())),
    logout: vi.fn(() => {
      status.set('anonymous');
      currentUser.set(null);
      return of(undefined);
    }),
    signIn(next: CurrentUser = TEST_USER) {
      currentUser.set(next);
      status.set('authenticated');
    },
  };
  return { mock, provider: { provide: AuthStore, useValue: mock } };
}
