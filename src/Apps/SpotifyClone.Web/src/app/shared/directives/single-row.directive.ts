import { DestroyRef, Directive, ElementRef, PLATFORM_ID, afterNextRender, inject } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

/**
 * Clamps a wrapping flex container to the height of its first child, so a shelf
 * only ever shows one row of cards and hides the rest (replaces mockups/js/trackItemsSizeCalc.js).
 *
 * Only the first child is observed (its size follows the container width), and the
 * height is written in the next frame so the observer never re-triggers itself.
 */
@Directive({ selector: '[appSingleRow]' })
export class SingleRowDirective {
  constructor() {
    const element = inject<ElementRef<HTMLElement>>(ElementRef).nativeElement;
    const destroyRef = inject(DestroyRef);
    // No ResizeObserver on the server (or in jsdom): the shelf then simply keeps its natural height.
    if (!isPlatformBrowser(inject(PLATFORM_ID)) || typeof ResizeObserver === 'undefined') {
      return;
    }

    afterNextRender(() => {
      let frame = 0;
      const observer = new ResizeObserver(([entry]) => {
        cancelAnimationFrame(frame);
        frame = requestAnimationFrame(() => {
          element.style.height = `${(entry.target as HTMLElement).offsetHeight}px`;
        });
      });

      const first = element.firstElementChild;
      if (first) {
        observer.observe(first);
      }
      destroyRef.onDestroy(() => {
        cancelAnimationFrame(frame);
        observer.disconnect();
      });
    });
  }
}
