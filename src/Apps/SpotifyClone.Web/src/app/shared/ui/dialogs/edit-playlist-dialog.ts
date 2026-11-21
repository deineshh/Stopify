import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { DIALOG_DATA, DialogRef } from '@angular/cdk/dialog';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import { PLAYLIST_DESCRIPTION_MAX_LENGTH, PLAYLIST_NAME_MAX_LENGTH } from '@data/api/playlists-api';
import { PlaylistRef } from '@data/models/playlist.models';
import { PlaylistEditor } from '@data/services/playlist-editor.service';
import { ApiError } from '@core/api/api-error';

/** Spotify's "Edit details" dialog: name and description (cover upload isn't wired to the API yet). */
@Component({
  selector: 'app-edit-playlist-dialog',
  imports: [FaIconComponent],
  templateUrl: './edit-playlist-dialog.html',
  styleUrl: './edit-playlist-dialog.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EditPlaylistDialog {
  protected readonly playlist = inject<PlaylistRef>(DIALOG_DATA);
  protected readonly ref = inject<DialogRef<boolean>>(DialogRef);
  private readonly editor = inject(PlaylistEditor);

  protected readonly nameMax = PLAYLIST_NAME_MAX_LENGTH;
  protected readonly descriptionMax = PLAYLIST_DESCRIPTION_MAX_LENGTH;

  protected readonly name = signal(this.playlist.name);
  protected readonly description = signal(this.playlist.description ?? '');
  protected readonly saving = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly nameMissing = computed(() => this.name().trim() === '');
  protected readonly canSave = computed(
    () =>
      !this.saving() &&
      !this.nameMissing() &&
      this.name().length <= this.nameMax &&
      this.description().length <= this.descriptionMax,
  );

  protected save(event: Event): void {
    event.preventDefault();
    if (!this.canSave()) {
      return;
    }
    const name = this.name().trim();
    const description = this.description().trim() || null;
    if (name === this.playlist.name && description === this.playlist.description) {
      this.ref.close(false);
      return;
    }
    this.saving.set(true);
    this.error.set(null);
    this.editor.editDetails(this.playlist, { name, description, isPublic: this.playlist.isPublic }).subscribe({
      next: () => this.ref.close(true),
      error: (error: ApiError) => {
        this.saving.set(false);
        this.error.set(
          error.isNetworkError
            ? "Couldn't reach Stopify. Check your connection."
            : error.status === 403 || error.has('Playlist.NotOwned')
              ? 'Only the owner can edit this playlist.'
              : error.has('Playlist.InvalidMetadata')
                ? error.message
                : "Couldn't save your changes. Try again.",
        );
      },
    });
  }
}
