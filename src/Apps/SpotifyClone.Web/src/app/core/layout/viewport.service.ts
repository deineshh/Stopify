import { DestroyRef, Injectable, PLATFORM_ID, Signal, inject, signal } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

/** Breakpoints in px — keep in sync with src/styles/_breakpoints.scss. */
export const BREAKPOINTS = {
  tablet: 768,
  laptop: 1024,
  desktop: 1280,
} as const;

/**
 * Exposes viewport media queries as signals so components can switch
 * behaviour (not just styling) between mobile and desktop layouts.
 * On the server every query reports the desktop layout.
 */
@Injectable({ providedIn: 'root' })
export class ViewportService {
  private readonly isBrowser = isPlatformBrowser(inject(PLATFORM_ID));
  private readonly destroyRef = inject(DestroyRef);

  /** Phones (< 768px): bottom navigation + mini player. */
  readonly isMobile = this.query(`(max-width: ${BREAKPOINTS.tablet - 0.02}px)`);

  /** Below 1024px: the library collapses to an icon rail. */
  readonly isBelowLaptop = this.query(`(max-width: ${BREAKPOINTS.laptop - 0.02}px)`);

  /** Below 1280px: there is no room for the right sidebar next to the main view. */
  readonly isBelowDesktop = this.query(`(max-width: ${BREAKPOINTS.desktop - 0.02}px)`);

  private query(media: string): Signal<boolean> {
    const matches = signal(false);
    // Server rendering (and environments without matchMedia) fall back to the desktop layout.
    if (!this.isBrowser || typeof window.matchMedia !== 'function') {
      return matches.asReadonly();
    }

    const list = window.matchMedia(media);
    matches.set(list.matches);
    const onChange = (event: MediaQueryListEvent) => matches.set(event.matches);
    list.addEventListener('change', onChange);
    this.destroyRef.onDestroy(() => list.removeEventListener('change', onChange));
    return matches.asReadonly();
  }
}
