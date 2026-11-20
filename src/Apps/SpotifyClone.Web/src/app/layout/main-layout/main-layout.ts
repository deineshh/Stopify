import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  ElementRef,
  inject,
  viewChild,
} from '@angular/core';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { filter } from 'rxjs';
import { LayoutStore } from '@core/layout/layout.store';
import { ViewportService } from '@core/layout/viewport.service';
import { GlobalHeader } from '../global-header/global-header';
import { LibrarySidebar } from '../library-sidebar/library-sidebar';
import { NowPlayingPanel } from '../now-playing-panel/now-playing-panel';
import { QueuePanel } from '../queue-panel/queue-panel';
import { NowPlayingBar } from '../now-playing-bar/now-playing-bar';
import { MainFooter } from '../main-footer/main-footer';
import { MobileNav } from '../mobile-nav/mobile-nav';
import { ToastOutlet } from '@shared/ui/toast/toast';
import { ResizeHandle } from '../resize-handle/resize-handle';

/**
 * Application shell: header, library, main view, right sidebar and player bar
 * on desktop; main view, mini player and tab bar on phones.
 */
@Component({
  selector: 'app-main-layout',
  imports: [
    RouterOutlet,
    GlobalHeader,
    LibrarySidebar,
    NowPlayingPanel,
    QueuePanel,
    NowPlayingBar,
    MainFooter,
    MobileNav,
    ResizeHandle, ToastOutlet],
  templateUrl: './main-layout.html',
  styleUrl: './main-layout.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MainLayout {
  protected readonly layout = inject(LayoutStore);
  protected readonly viewport = inject(ViewportService);

  private readonly mainView = viewChild.required<ElementRef<HTMLElement>>('mainView');

  constructor() {
    // Every navigation starts at the top of the main view (it is the scroll container, not the window).
    inject(Router)
      .events.pipe(
        filter((event) => event instanceof NavigationEnd),
        takeUntilDestroyed(inject(DestroyRef)),
      )
      .subscribe(() => {
        this.mainView().nativeElement.scrollTop = 0;
        this.layout.setMainScrollTop(0);
      });
  }

  protected onMainScroll(event: Event): void {
    this.layout.setMainScrollTop((event.target as HTMLElement).scrollTop);
  }
}
