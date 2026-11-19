import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import { Track } from '@data/models/catalog.models';
import { DurationPipe } from '../../pipes/duration.pipe';
import { ArtistNamesPipe } from '../../pipes/artist-names.pipe';

/**
 * Numbered track list with album and date columns (mockup `.track-table`).
 * Columns drop out as the table gets narrower, so it also works on phones.
 */
@Component({
  selector: 'app-track-table',
  imports: [DatePipe, RouterLink, FaIconComponent, DurationPipe, ArtistNamesPipe],
  templateUrl: './track-table.html',
  styleUrl: './track-table.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TrackTable {
  readonly tracks = input.required<readonly Track[]>();
  readonly play = output<Track>();
}
