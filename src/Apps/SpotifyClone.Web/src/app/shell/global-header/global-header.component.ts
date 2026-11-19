import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faHouse, faMagnifyingGlass, faXmark, faBucket, faArrowDown, faBell, faUserGroup } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-global-header',
  standalone: true,
  imports: [RouterLink, FontAwesomeModule],
  templateUrl: './global-header.component.html',
  styleUrl: './global-header.component.css',
})
export class GlobalHeaderComponent {
  protected readonly faHouse = faHouse;
  protected readonly faMagnifyingGlass = faMagnifyingGlass;
  protected readonly faXmark = faXmark;
  protected readonly faBucket = faBucket;
  protected readonly faArrowDown = faArrowDown;
  protected readonly faBell = faBell;
  protected readonly faUserGroup = faUserGroup;
}
