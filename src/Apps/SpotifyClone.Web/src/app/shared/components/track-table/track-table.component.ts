import { Component, Input, Output, EventEmitter } from '@angular/core';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faPlay, faCheck, faEllipsisH } from '@fortawesome/free-solid-svg-icons';
import { TrackTableRow, TrackTableColumns } from '../../models/track.models';

@Component({
  selector: 'app-track-table',
  standalone: true,
  imports: [FontAwesomeModule],
  templateUrl: './track-table.component.html',
  styleUrl: './track-table.component.css',
})
export class TrackTableComponent {
  protected readonly faPlay = faPlay;
  protected readonly faCheck = faCheck;
  protected readonly faEllipsisH = faEllipsisH;

  @Input({ required: true }) tracks!: TrackTableRow[];

  @Input() columns: TrackTableColumns = {
    index: true,
    title: true,
    album: false,
    dateAdded: false,
    duration: true,
    save: true,
    more: true,
  };

  @Input() playingTrackId: string | null = null;

  @Output() trackClick = new EventEmitter<string>();
}
