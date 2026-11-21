import { Injectable, inject } from '@angular/core';
import { Dialog } from '@angular/cdk/dialog';
import { filter, switchMap } from 'rxjs';
import { PlaylistRef } from '@data/models/playlist.models';
import { PlaylistEditor } from '@data/services/playlist-editor.service';
import type { CollaboratorsDialogData } from './collaborators-dialog';
import { ConfirmDialog, ConfirmDialogData } from './confirm-dialog';
import { EditPlaylistDialog } from './edit-playlist-dialog';

const DIALOG_LOOK = { backdropClass: 'dialog-backdrop', panelClass: 'dialog-panel' } as const;

/** Opens the playlist dialogs and runs what they confirm. */
@Injectable({ providedIn: 'root' })
export class PlaylistDialogs {
  private readonly dialog = inject(Dialog);
  private readonly editor = inject(PlaylistEditor);

  editDetails(playlist: PlaylistRef): void {
    this.dialog.open<boolean, PlaylistRef>(EditPlaylistDialog, {
      ...DIALOG_LOOK,
      data: playlist,
      ariaLabelledBy: 'edit-playlist-title',
    });
  }

  /**
   * Owner only: add or remove collaborators. Loaded on first use, as only owners ever open it
   * and this service is part of the app shell (the library uses it).
   */
  async collaborators(data: CollaboratorsDialogData): Promise<void> {
    const { CollaboratorsDialog } = await import('./collaborators-dialog');
    this.dialog.open<void, CollaboratorsDialogData>(CollaboratorsDialog, {
      ...DIALOG_LOOK,
      data,
      ariaLabelledBy: 'collaborators-title',
    });
  }

  confirmDelete(playlist: PlaylistRef): void {
    this.dialog
      .open<boolean, ConfirmDialogData>(ConfirmDialog, {
        ...DIALOG_LOOK,
        role: 'alertdialog',
        ariaLabelledBy: 'confirm-dialog-title',
        data: {
          title: 'Delete from Your Library?',
          message: `This will delete ${playlist.name} from Your Library.`,
          confirmLabel: 'Delete',
        },
      })
      .closed.pipe(
        filter(Boolean),
        switchMap(() => this.editor.delete(playlist)),
      )
      .subscribe();
  }
}
