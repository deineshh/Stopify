import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import { LayoutStore } from '@core/layout/layout.store';
import { PlayerService } from '@data/services/player.service';
import { SidebarTrackRow } from '@shared/ui/sidebar-track-row/sidebar-track-row';

/** Right sidebar "Queue" view. */
@Component({
  selector: 'app-queue-panel',
  imports: [RouterLink, FaIconComponent, SidebarTrackRow],
  templateUrl: './queue-panel.html',
  styleUrl: './queue-panel.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'queue-view__main-group' },
})
export class QueuePanel {
  protected readonly layout = inject(LayoutStore);
  protected readonly player = inject(PlayerService);
}
