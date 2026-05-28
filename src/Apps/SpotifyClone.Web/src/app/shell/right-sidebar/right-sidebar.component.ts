import { Component } from '@angular/core';
import { NowPlayingViewComponent } from './now-playing-view/now-playing-view.component';
import { QueueViewComponent } from './queue-view/queue-view.component';

@Component({
  selector: 'app-right-sidebar',
  standalone: true,
  imports: [NowPlayingViewComponent, QueueViewComponent],
  templateUrl: './right-sidebar.component.html',
  styleUrl: './right-sidebar.component.css',
})
export class RightSidebarComponent {
  protected showNowPlaying = true;
  protected showQueue = false;
}
