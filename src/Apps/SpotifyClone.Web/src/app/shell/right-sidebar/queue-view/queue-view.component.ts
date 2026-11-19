import { Component } from '@angular/core';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faXmark, faPlay, faEllipsisH } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-queue-view',
  standalone: true,
  imports: [FontAwesomeModule],
  templateUrl: './queue-view.component.html',
  styleUrl: './queue-view.component.css',
})
export class QueueViewComponent {
  protected readonly faXmark = faXmark;
  protected readonly faPlay = faPlay;
  protected readonly faEllipsisH = faEllipsisH;
}
