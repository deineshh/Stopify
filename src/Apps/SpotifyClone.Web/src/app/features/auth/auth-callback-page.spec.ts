import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of } from 'rxjs';
import { AuthCallbackPage } from './auth-callback-page';
import { AuthStore } from '@core/auth/auth.store';
import { provideTestDefaults } from '../../testing/test-providers';

const encode = (value: object) => btoa(JSON.stringify(value)).replace(/=+$/, '');

describe('AuthCallbackPage', () => {
  afterEach(() => {
    document.cookie = 'tempAccessToken=; Max-Age=0; Path=/';
    document.cookie = 'tempExpiresAt=; Max-Age=0; Path=/';
  });

  async function setup() {
    const completeExternalLogin = vi.fn().mockReturnValue(of(null));
    TestBed.configureTestingModule({
      providers: [...provideTestDefaults(), { provide: AuthStore, useValue: { completeExternalLogin } }],
    });
    const router = TestBed.inject(Router);
    const navigateByUrl = vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);
    const navigate = vi.spyOn(router, 'navigate').mockResolvedValue(true);
    const fixture = TestBed.createComponent(AuthCallbackPage);
    fixture.detectChanges();
    await fixture.whenStable();
    return { completeExternalLogin, navigateByUrl, navigate };
  }

  it('moves the token from the temporary cookie into the session', async () => {
    const token = `${encode({ alg: 'HS256' })}.${encode({ exp: 1_900_000_000 })}.sig`;
    document.cookie = `tempAccessToken=${token}; Path=/`;
    document.cookie = 'tempExpiresAt=11/19/2026 8:56:41 PM +00:00; Path=/';

    const { completeExternalLogin, navigateByUrl } = await setup();

    expect(completeExternalLogin).toHaveBeenCalledWith({
      accessToken: token,
      expiresAt: new Date(1_900_000_000_000).toISOString(),
    });
    expect(navigateByUrl).toHaveBeenCalledWith('/', { replaceUrl: true });
    expect(document.cookie).not.toContain('tempAccessToken');
  });

  it('goes back to log in when there is no token', async () => {
    const { completeExternalLogin, navigate } = await setup();
    expect(completeExternalLogin).not.toHaveBeenCalled();
    expect(navigate).toHaveBeenCalledWith(['/login'], { queryParams: { error: 'auth_failed' }, replaceUrl: true });
  });
});
