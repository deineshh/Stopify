import { TestBed } from '@angular/core/testing';
import { ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree, provideRouter } from '@angular/router';
import { Observable, firstValueFrom, of } from 'rxjs';
import { AuthStatus, AuthStore } from './auth.store';
import { authGuard, guestGuard, safeRedirect } from './auth.guards';

function run(guard: typeof authGuard, status: AuthStatus, url = '/collection') {
  TestBed.resetTestingModule();
  TestBed.configureTestingModule({
    providers: [provideRouter([]), { provide: AuthStore, useValue: { restoreSession: () => of(status) } }],
  });
  const result = TestBed.runInInjectionContext(() =>
    guard({} as ActivatedRouteSnapshot, { url } as RouterStateSnapshot),
  ) as Observable<boolean | UrlTree>;
  return firstValueFrom(result);
}

describe('auth guards', () => {
  it('authGuard lets signed-in users through', async () => {
    expect(await run(authGuard, 'authenticated')).toBe(true);
  });

  it('authGuard sends everyone else to log in with a redirect back', async () => {
    const result = (await run(authGuard, 'anonymous')) as UrlTree;
    expect(result.toString()).toBe('/login?redirect=%2Fcollection');
  });

  it('guestGuard sends signed-in users home', async () => {
    expect(((await run(guestGuard, 'authenticated')) as UrlTree).toString()).toBe('/');
    expect(await run(guestGuard, 'anonymous')).toBe(true);
  });

  it('only follows in-app redirect targets', () => {
    expect(safeRedirect('/collection')).toBe('/collection');
    expect(safeRedirect('https://evil.example')).toBe('/');
    expect(safeRedirect('//evil.example')).toBe('/');
    expect(safeRedirect(null)).toBe('/');
  });
});
