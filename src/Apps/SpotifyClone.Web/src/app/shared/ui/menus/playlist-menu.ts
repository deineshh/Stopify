import { ChangeDetectionStrategy, Component, Signal, TemplateRef, inject, viewChild } from '@angular/core';
import { CdkMenu, CdkMenuItem } from '@angular/cdk/menu';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import { PlaylistRef } from '@data/models/playlist.models';
import { PlaylistEditor } from '@data/services/playlist-editor.service';
import { PlaylistDialogs } from '../dialogs/playlist-dialogs';

/**
 * The menu for one of the user's own playlists (library row, playlist page "…"). One per view;
 * pass a signal of the playlist as trigger data: `[cdkMenuTriggerData]="{ $implicit: ref }"`.
 *
 * A signal, not the value: a CDK menu trigger keeps the data it was first opened with, so a
 * plain value would still show the old name or visibility after the playlist changed.
 */
@Component({
  selector: 'app-playlist-menu',
  imports: [CdkMenu, CdkMenuItem, FaIconComponent],
  template: `
    <ng-template #menu let-ref>
      @let playlist = ref();
      <div class="menu" cdkMenu [attr.aria-label]="'Options for ' + (playlist?.name ?? 'playlist')">
        @if (playlist) {
          <button class="menu__item" type="button" cdkMenuItem (cdkMenuItemTriggered)="dialogs.editDetails(playlist)">
            <fa-icon class="menu__icon" [icon]="['fas', 'pen']" />
            <span class="menu__label">Edit details</span>
          </button>
          <button class="menu__item" type="button" cdkMenuItem (cdkMenuItemTriggered)="togglePublic(playlist)">
            <fa-icon class="menu__icon" [icon]="['fas', playlist.isPublic ? 'lock' : 'globe']" />
            <span class="menu__label">{{ playlist.isPublic ? 'Make private' : 'Make public' }}</span>
          </button>
          <hr class="menu__divider" />
          <button
            class="menu__item menu__item--danger"
            type="button"
            cdkMenuItem
            (cdkMenuItemTriggered)="dialogs.confirmDelete(playlist)"
          >
            <fa-icon class="menu__icon" [icon]="['fas', 'circle-minus']" />
            <span class="menu__label">Delete</span>
          </button>
        }
      </div>
    </ng-template>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PlaylistMenu {
  protected readonly dialogs = inject(PlaylistDialogs);
  private readonly editor = inject(PlaylistEditor);

  readonly template = viewChild.required<TemplateRef<{ $implicit: Signal<PlaylistRef | null> }>>('menu');

  protected togglePublic(playlist: PlaylistRef): void {
    this.editor.setPublic(playlist, !playlist.isPublic).subscribe();
  }
}
