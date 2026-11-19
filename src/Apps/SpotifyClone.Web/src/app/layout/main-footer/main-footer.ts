import { ChangeDetectionStrategy, Component } from '@angular/core';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import { IconName, IconPrefix } from '@fortawesome/fontawesome-svg-core';

interface FooterColumn {
  id: string;
  title: string;
  links: readonly string[];
}

interface SocialLink {
  id: string;
  label: string;
  icon: [IconPrefix, IconName];
}

/** Link footer at the bottom of the main view. */
@Component({
  selector: 'app-main-footer',
  imports: [FaIconComponent],
  templateUrl: './main-footer.html',
  styleUrl: './main-footer.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'main-footer' },
})
export class MainFooter {
  protected readonly year = 2025;

  protected readonly columns: readonly FooterColumn[] = [
    { id: 'company', title: 'Company', links: ['About', 'Jobs', 'For the Record'] },
    {
      id: 'communities',
      title: 'Communities',
      links: ['For Artists', 'Developers', 'Advertising', 'Investors', 'Vendors'],
    },
    {
      id: 'useful-links',
      title: 'Useful links',
      links: ['Support', 'Free Mobile App', 'Popular by Country', 'Import your music'],
    },
    {
      id: 'plans',
      title: 'Stopify Plans',
      links: ['Premium Individual', 'Premium Duo', 'Premium Family', 'Premium Student', 'Stopify Free'],
    },
  ];

  protected readonly socials: readonly SocialLink[] = [
    { id: 'instagram', label: 'Instagram', icon: ['fab', 'instagram'] },
    { id: 'twitter', label: 'Twitter', icon: ['fab', 'twitter'] },
    { id: 'facebook', label: 'Facebook', icon: ['fab', 'facebook'] },
  ];

  protected readonly legalLinks = [
    'Legal',
    'Safety & Privacy Center',
    'Privacy Policy',
    'Cookies',
    'About Ads',
    'Accessibility',
  ] as const;
}
