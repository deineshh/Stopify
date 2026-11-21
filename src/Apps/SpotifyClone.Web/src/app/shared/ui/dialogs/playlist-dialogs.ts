import { Injectable, inject } from '@angular/core';
import { Dialog } from '@angular/cdk/dialog';
import { filter, switchMap } from 'rxjs';
import { PlaylistRef } from '@data/models/playlist.models';
import { PlaylistEditor } from '@data/services/playlist-editor.service';
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
