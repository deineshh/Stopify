import { TestBed } from '@angular/core/testing';
import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { firstValueFrom } from 'rxjs';
import { authInterceptor } from './auth.interceptor';
import { AuthStore } from './auth.store';
import { TEST_USER } from '../../testing/auth.fixtures';

const inAnHour = () => new Date(Date.now() + 3_600_000).toISOString();

describe('authInterceptor', () => {
  let http: HttpClient;
  let backend: HttpTestingController;
  let store: AuthStore;

  beforeEach(async () => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(withInterceptors([authInterceptor])), provideHttpClientTesting()],
    });
    http = TestBed.inject(HttpClient);
    backend = TestBed.inject(HttpTestingController);
    store = TestBed.inject(AuthStore);

    const restored = firstValueFrom(store.restoreSession());
    backend.expectOne('/api/v1/auth/refresh').flush({ accessToken: 'old', expiresAt: inAnHour() });
    backend.expectOne((req) => req.url === '/api/v1/users/me').flush(TEST_USER);
    await restored;
  });

  afterEach(() => {
    store.clearSession();
    backend.verify();
  });

  it('adds the bearer token to API requests only', () => {
    http.get('/api/v1/playlists').subscribe();
    http.get('/assets/images/song.jpg').subscribe();
    expect(backend.expectOne('/api/v1/playlists').request.headers.get('Authorization')).toBe('Bearer old');
    expect(backend.expectOne('/assets/images/song.jpg').request.headers.has('Authorization')).toBe(false);
  });

  it('refreshes once on 401 and replays every waiting request with the new token', async () => {
    const a = firstValueFrom(http.get<string>('/api/v1/a'));
    const b = firstValueFrom(http.get<string>('/api/v1/b'));
    backend.expectOne('/api/v1/a').flush(null, { status: 401, statusText: 'Unauthorized' });
    backend.expectOne('/api/v1/b').flush(null, { status: 401, statusText: 'Unauthorized' });

    backend.expectOne('/api/v1/auth/refresh').flush({ accessToken: 'new', expiresAt: inAnHour() });

    const retriedA = backend.expectOne('/api/v1/a');
    const retriedB = backend.expectOne('/api/v1/b');
    expect(retriedA.request.headers.get('Authorization')).toBe('Bearer new');
    retriedA.flush('A');
    retriedB.flush('B');
    expect(await a).toBe('A');
    expect(await b).toBe('B');
  });

  it('clears the session and rethrows when the refresh fails', async () => {
    const result = firstValueFrom(http.get('/api/v1/a')).catch((error) => error);
    backend.expectOne('/api/v1/a').flush(null, { status: 401, statusText: 'Unauthorized' });
    backend.expectOne('/api/v1/auth/refresh').flush(null, { status: 401, statusText: 'Unauthorized' });
    expect((await result).status).toBe(401);
    expect(store.status()).toBe('anonymous');
  });

  it('never retries the auth endpoints themselves', async () => {
    const result = firstValueFrom(http.post('/api/v1/auth/login', {})).catch((error) => error);
    backend.expectOne('/api/v1/auth/login').flush(null, { status: 401, statusText: 'Unauthorized' });
    expect((await result).status).toBe(401);
    backend.expectNone('/api/v1/auth/refresh');
  });

  it('passes other errors through untouched', async () => {
    const result = firstValueFrom(http.get('/api/v1/a')).catch((error) => error);
    backend.expectOne('/api/v1/a').flush(null, { status: 404, statusText: 'Not Found' });
    expect((await result).status).toBe(404);
  });
});
