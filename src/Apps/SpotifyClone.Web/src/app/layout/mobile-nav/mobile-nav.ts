import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import { IconName } from '@fortawesome/fontawesome-svg-core';

interface Tab {
  label: string;
  icon: IconName;
  link: string;
  exact: boolean;
}

/** Bottom tab bar shown on phones in place of the header and sidebars. */
@Component({
  selector: 'app-mobile-nav',
  imports: [RouterLink, RouterLinkActive, FaIconComponent],
  template: `
    @for (tab of tabs; track tab.link) {
      <a
        class="mobile-nav__tab"
        [routerLink]="tab.link"
        routerLinkActive="active"
        [routerLinkActiveOptions]="{ exact: tab.exact }"
        ariaCurrentWhenActive="page"
      >
        <fa-icon class="mobile-nav__icon" [icon]="['fas', tab.icon]" />
        <span class="mobile-nav__label">{{ tab.label }}</span>
      </a>
    }
  `,
  styleUrl: './mobile-nav.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'mobile-nav', role: 'navigation', 'aria-label': 'Main' },
})
export class MobileNav {
  protected readonly tabs: readonly Tab[] = [
    { label: 'Home', icon: 'house', link: '/', exact: true },
    { label: 'Search', icon: 'magnifying-glass', link: '/search', exact: false },
    { label: 'Your Library', icon: 'book-open', link: '/collection', exact: false },
  ];
}
