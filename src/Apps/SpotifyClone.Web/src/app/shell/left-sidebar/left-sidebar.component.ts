import { Component } from '@angular/core';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import {
  faCircleLeft,
  faPlus,
  faUpRightAndDownLeftFromCenter,
  faMagnifyingGlass,
  faXmark,
  faListUl,
  faPlay,
  faThumbtack,
} from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-left-sidebar',
  standalone: true,
  imports: [FontAwesomeModule],
  templateUrl: './left-sidebar.component.html',
  styleUrl: './left-sidebar.component.css',
})
export class LeftSidebarComponent {
  protected readonly faCircleLeft = faCircleLeft;
  protected readonly faPlus = faPlus;
  protected readonly faUpRightAndDownLeftFromCenter = faUpRightAndDownLeftFromCenter;
  protected readonly faMagnifyingGlass = faMagnifyingGlass;
  protected readonly faXmark = faXmark;
  protected readonly faListUl = faListUl;
  protected readonly faPlay = faPlay;
  protected readonly faThumbtack = faThumbtack;

  protected readonly libraryItems = Array.from({ length: 15 }, (_, i) => ({
    id: `item-${i}`,
    title: 'Liked Songs',
    description: 'Playlist • 81 songs',
    pinned: true,
  }));
}
