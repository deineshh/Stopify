import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { firstValueFrom } from 'rxjs';
import { AuthStore } from './auth.store';
import { TEST_USER } from '../../testing/auth.fixtures';
import { ApiError } from '../api/api-error';


const inAnHour = () => new Date(Date.now() + 3_600_000).toISOString();

describe('AuthStore', () => {
  let store: AuthStore;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    store = TestBed.inject(AuthStore);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    store.clearSession();
    http.verify();
  });

  function answerSession(token = 'token-1') {
    const refresh = http.expectOne('/api/v1/auth/refresh');
    expect(refresh.request.withCredentials).toBe(true);
    refresh.flush({ accessToken: token, expiresAt: inAnHour() });
    http.expectOne('/api/v1/users/me').flush(TEST_USER);
  }

  it('restores the session from the refresh cookie', async () => {
    const status = firstValueFrom(store.restoreSession());
    answerSession();
    expect(await status).toBe('authenticated');
    expect(store.accessToken()).toBe('token-1');
    expect(store.user()?.displayName).toBe('Aqua Studios');
    expect(store.initial()).toBe('A');
  });

  it('is anonymous when there is no valid refresh cookie', async () => {
    const status = firstValueFrom(store.restoreSession());
    http.expectOne('/api/v1/auth/refresh').flush({ message: 'Refresh token not found.' }, { status: 401, statusText: 'Unauthorized' });
    expect(await status).toBe('anonymous');
    expect(store.isAuthenticated()).toBe(false);
    expect(store.accessToken()).toBeNull();
  });

  it('only restores once', async () => {
    const first = firstValueFrom(store.restoreSession());
    const second = firstValueFrom(store.restoreSession());
    answerSession();
    expect(await first).toBe('authenticated');
    expect(await second).toBe('authenticated');
    expect(await firstValueFrom(store.restoreSession())).toBe('authenticated');
  });

  it('shares one refresh request between concurrent callers', async () => {
    const a = firstValueFrom(store.refresh());
    const b = firstValueFrom(store.refresh());
    answerSession('rotated');
    expect(await a).toBe('rotated');
    expect(await b).toBe('rotated');
  });

  it('logs in with a password and loads the user', async () => {
    const user = firstValueFrom(store.login('aqua', 'secret'));
    const login = http.expectOne('/api/v1/auth/login');
    expect(login.request.body).toEqual({ identifier: 'aqua', password: 'secret' });
    login.flush({ accessToken: 't', expiresAt: inAnHour() });
    http.expectOne('/api/v1/users/me').flush(TEST_USER);
    expect((await user)?.email).toBe('aqua@studios.com');
    expect(store.status()).toBe('authenticated');
  });

  it('turns API failures into ApiError', async () => {
    const result = firstValueFrom(store.login('aqua', 'wrong')).catch((error) => error);
    http.expectOne('/api/v1/auth/login').flush(
      { title: 'Auth.InvalidPassword', detail: 'The provided password is invalid.', errors: [] },
      { status: 400, statusText: 'Bad Request' },
    );
    const error = await result;
    expect(error).toBeInstanceOf(ApiError);
    expect(error.code).toBe('Auth.InvalidPassword');
    expect(store.isAuthenticated()).toBe(false);
  });

  it('registers and starts the session from the response', async () => {
    const response = firstValueFrom(
      store.register({ email: 'a@b.co', password: 'Pa$$w0rd', displayName: 'A', birthDate: '2000-01-01T00:00:00Z', gender: 'male' }),
    );
    http.expectOne('/api/v1/auth/register').flush({ userId: 'u1', email: 'a@b.co', displayName: 'A', birthDateUtc: null, gender: 'male', accessToken: 't', expiresAt: inAnHour() });
    http.expectOne('/api/v1/users/me').flush(TEST_USER);
    expect((await response).userId).toBe('u1');
    expect(store.accessToken()).toBe('t');
  });

  it('keeps the session when /users/me fails', async () => {
    const status = firstValueFrom(store.restoreSession());
    http.expectOne('/api/v1/auth/refresh').flush({ accessToken: 't', expiresAt: inAnHour() });
    http.expectOne('/api/v1/users/me').flush(null, { status: 500, statusText: 'Server Error' });
    expect(await status).toBe('authenticated');
    expect(store.user()).toBeNull();
  });

  it('logs out on the server and always clears locally', async () => {
    const restored = firstValueFrom(store.restoreSession());
    answerSession();
    await restored;

    const done = firstValueFrom(store.logout());
    http.expectOne('/api/v1/auth/logout').flush(null, { status: 500, statusText: 'Server Error' });
    await done;
    expect(store.status()).toBe('anonymous');
    expect(store.user()).toBeNull();
  });

  it('refreshes ahead of expiry', async () => {
    vi.useFakeTimers();
    try {
      const status = firstValueFrom(store.restoreSession());
      http.expectOne('/api/v1/auth/refresh').flush({ accessToken: 'first', expiresAt: new Date(Date.now() + 120_000).toISOString() });
      http.expectOne('/api/v1/users/me').flush(TEST_USER);
      await status;

      vi.advanceTimersByTime(61_000); // 120s token, refreshed 60s early
      http.expectOne('/api/v1/auth/refresh').flush({ accessToken: 'second', expiresAt: inAnHour() });
      expect(store.accessToken()).toBe('second');
    } finally {
      vi.useRealTimers();
    }
  });
});
