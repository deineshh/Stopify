import { Injectable, PLATFORM_ID, computed, effect, inject, signal, untracked } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { ViewportService } from './viewport.service';

export type RightPanel = 'now-playing' | 'queue';

export const LIBRARY_WIDTH = {
  collapsed: 72,
  min: 280,
  default: 288,
  max: 420,
  /** Dragging the resizer below this width snaps the library to the collapsed rail. */
  collapseThreshold: 200,
} as const;

export const RIGHT_PANEL_WIDTH = {
  min: 288,
  default: 288,
  max: 420,
} as const;

const STORAGE_KEY = 'stopify.layout';

interface PersistedLayout {
  libraryWidth: number;
  rightPanelWidth: number;
}

/**
 * UI state of the application shell: sidebar sizes, which right panel is open
 * and the scroll position of the main view (pages use it for their sticky headers).
 */
@Injectable({ providedIn: 'root' })
export class LayoutStore {
  private readonly viewport = inject(ViewportService);
  private readonly isBrowser = isPlatformBrowser(inject(PLATFORM_ID));

  private readonly _libraryWidth = signal<number>(LIBRARY_WIDTH.default);
  private readonly _libraryCollapsed = signal(false);
  private readonly _rightPanel = signal<RightPanel | null>('now-playing');
  private readonly _rightPanelWidth = signal<number>(RIGHT_PANEL_WIDTH.default);
  private readonly _mainScrollTop = signal(0);

  readonly libraryCollapsed = this._libraryCollapsed.asReadonly();
  readonly rightPanel = this._rightPanel.asReadonly();
  readonly rightPanelWidth = this._rightPanelWidth.asReadonly();
  readonly mainScrollTop = this._mainScrollTop.asReadonly();

  /** Width the library actually renders at. */
  readonly libraryWidth = computed(() =>
    this._libraryCollapsed() ? LIBRARY_WIDTH.collapsed : this._libraryWidth(),
  );

  /** The right panel only fits next to the main view on desktop-sized screens. */
  readonly isRightPanelVisible = computed(
    () => this._rightPanel() !== null && !this.viewport.isMobile(),
  );

  constructor() {
    this.restore();

    // Narrow screens start with a collapsed library and no right panel; widening restores them.
    effect(() => {
      const belowLaptop = this.viewport.isBelowLaptop();
      untracked(() => this._libraryCollapsed.set(belowLaptop));
    });
    effect(() => {
      const belowDesktop = this.viewport.isBelowDesktop();
      untracked(() => this._rightPanel.set(belowDesktop ? null : 'now-playing'));
    });
  }

  toggleLibrary(): void {
    this._libraryCollapsed.update((collapsed) => !collapsed);
  }

  /** Called while dragging the library resizer. */
  resizeLibrary(width: number): void {
    if (width < LIBRARY_WIDTH.collapseThreshold) {
      this._libraryCollapsed.set(true);
      return;
    }
    this._libraryCollapsed.set(false);
    this._libraryWidth.set(clamp(width, LIBRARY_WIDTH.min, LIBRARY_WIDTH.max));
    this.persist();
  }

  resizeRightPanel(width: number): void {
    this._rightPanelWidth.set(clamp(width, RIGHT_PANEL_WIDTH.min, RIGHT_PANEL_WIDTH.max));
    this.persist();
  }

  /** Opens `panel`, or closes the right sidebar if `panel` is already showing. */
  toggleRightPanel(panel: RightPanel): void {
    this._rightPanel.update((current) => (current === panel ? null : panel));
  }

  openRightPanel(panel: RightPanel): void {
    this._rightPanel.set(panel);
  }

  closeRightPanel(): void {
    this._rightPanel.set(null);
  }

  setMainScrollTop(scrollTop: number): void {
    this._mainScrollTop.set(scrollTop);
  }

  private persist(): void {
    if (!this.isBrowser) {
      return;
    }
    const state: PersistedLayout = {
      libraryWidth: this._libraryWidth(),
      rightPanelWidth: this._rightPanelWidth(),
    };
    try {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(state));
    } catch {
      // Storage can be unavailable (private mode, quota) — sizes simply won't persist.
    }
  }

  private restore(): void {
    if (!this.isBrowser) {
      return;
    }
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      if (!raw) {
        return;
      }
      const state = JSON.parse(raw) as Partial<PersistedLayout>;
      if (typeof state.libraryWidth === 'number') {
        this._libraryWidth.set(clamp(state.libraryWidth, LIBRARY_WIDTH.min, LIBRARY_WIDTH.max));
      }
      if (typeof state.rightPanelWidth === 'number') {
        this._rightPanelWidth.set(
          clamp(state.rightPanelWidth, RIGHT_PANEL_WIDTH.min, RIGHT_PANEL_WIDTH.max),
        );
      }
    } catch {
      // Corrupt or inaccessible storage — fall back to defaults.
    }
  }
}

function clamp(value: number, min: number, max: number): number {
  return Math.min(Math.max(value, min), max);
}
