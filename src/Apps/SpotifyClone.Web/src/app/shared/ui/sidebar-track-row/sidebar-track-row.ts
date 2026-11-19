import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import { Track } from '@data/models/catalog.models';
import { ArtistNamesPipe } from '../../pipes/artist-names.pipe';

/** Compact track row used in the right sidebar ("Next in queue", queue list). */
@Component({
  selector: 'app-sidebar-track-row',
  imports: [RouterLink, FaIconComponent, ArtistNamesPipe],
  templateUrl: './sidebar-track-row.html',
  styleUrl: './sidebar-track-row.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'right-sidebar__track-item' },
})
export class SidebarTrackRow {
  readonly track = input.required<Track>();
}
