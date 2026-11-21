import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  ElementRef,
  Injector,
  afterNextRender,
  computed,
  inject,
  signal,
  viewChild,
} from '@angular/core';
import { DIALOG_DATA, DialogRef } from '@angular/cdk/dialog';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import { Observable, catchError, concat, defer, last } from 'rxjs';
import { PLAYLIST_DESCRIPTION_MAX_LENGTH, PLAYLIST_NAME_MAX_LENGTH } from '@data/api/playlists-api';
import { PlaylistRef } from '@data/models/playlist.models';
import { CoverUploadError, PlaylistEditor } from '@data/services/playlist-editor.service';
import { ApiError } from '@core/api/api-error';

/** Photos the API can convert, and the largest one accepted here. */
export const COVER_TYPES = ['image/jpeg', 'image/png', 'image/webp'] as const;
export const COVER_MAX_BYTES = 10 * 1024 * 1024;

/**
 * Spotify's "Edit details" dialog: cover photo, name and description. A chosen photo (or
 * removing the current one) shows at once and is applied on Save, before the name and description.
 */
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
  private readonly injector = inject(Injector);
  private readonly coverButton = viewChild.required<ElementRef<HTMLButtonElement>>('coverButton');

  protected readonly nameMax = PLAYLIST_NAME_MAX_LENGTH;
  protected readonly descriptionMax = PLAYLIST_DESCRIPTION_MAX_LENGTH;

  /** A newly chosen photo, uploaded on Save, and its preview. */
  protected readonly photo = signal<File | null>(null);
  protected readonly preview = signal<string | null>(null);
  protected readonly uploading = signal(false);
  /** "Remove photo" was chosen: the cover goes back to the one made from the songs on Save. */
  protected readonly removePhoto = signal(false);

  /** What the cover box shows: a new photo, the current cover, or the empty tile once removed. */
  protected readonly shownCover = computed(() => this.preview() ?? (this.removePhoto() ? null : (this.playlist.cover ?? null)));
  /** Offered for a newly chosen photo (to drop it) or a current uploaded one. */
  protected readonly canRemovePhoto = computed(
    () => !!this.photo() || (!!this.playlist.hasCustomCover && !this.removePhoto()),
  );
  protected readonly coverTypes = COVER_TYPES.join(',');

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

  constructor() {
    inject(DestroyRef).onDestroy(() => this.revokePreview());
  }

  /** A photo was picked: checked here, shown at once, uploaded on Save. */
  protected choosePhoto(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    // Picking the same file again after an error should still fire `change`.
    input.value = '';
    if (!file) {
      return;
    }
    if (!(COVER_TYPES as readonly string[]).includes(file.type)) {
      this.error.set('Choose a JPEG, PNG or WebP image.');
      return;
    }
    if (file.size > COVER_MAX_BYTES) {
      this.error.set('That photo is too big. Choose one under 10 MB.');
      return;
    }
    this.error.set(null);
    this.revokePreview();
    this.removePhoto.set(false);
    this.photo.set(file);
    this.preview.set(URL.createObjectURL(file));
  }

  /** Drops a newly chosen photo, or marks the current one for removal. */
  protected removeChosenPhoto(): void {
    this.error.set(null);
    if (this.photo()) {
      this.revokePreview();
      this.photo.set(null);
      this.preview.set(null);
    } else {
      this.removePhoto.set(true);
    }
    // "Remove photo" may disappear with it; keep focus in the same place.
    afterNextRender(() => this.coverButton().nativeElement.focus(), { injector: this.injector });
  }

  protected save(event: Event): void {
    event.preventDefault();
    if (!this.canSave()) {
      return;
    }
    const name = this.name().trim();
    const description = this.description().trim() || null;
    const photo = this.photo();
    const removePhoto = !photo && this.removePhoto();
    const detailsChanged = name !== this.playlist.name || description !== this.playlist.description;
    if (!photo && !removePhoto && !detailsChanged) {
      this.ref.close(false);
      return;
    }

    const steps: Observable<void>[] = [];
    if (photo) {
      steps.push(
        defer(() => {
          this.uploading.set(true);
          return this.editor.changeCover(this.playlist, photo);
        }),
      );
    } else if (removePhoto) {
      steps.push(
        defer(() =>
          this.editor.removeCover(this.playlist).pipe(
            catchError((error: ApiError) => {
              throw new CoverRemovalError(error);
            }),
          ),
        ),
      );
    }
    if (detailsChanged) {
      steps.push(
        defer(() => {
          this.uploading.set(false);
          // Once the photo is in (or out), a failure here shouldn't send it again on retry.
          this.photo.set(null);
          this.removePhoto.set(false);
          return this.editor.editDetails(this.playlist, { name, description, isPublic: this.playlist.isPublic });
        }),
      );
    }

    this.saving.set(true);
    this.error.set(null);
    concat(...steps)
      .pipe(last())
      .subscribe({
        next: () => this.ref.close(true),
        error: (error: ApiError | CoverUploadError | CoverRemovalError) => {
          this.saving.set(false);
          this.uploading.set(false);
          this.error.set(
            error instanceof CoverUploadError
              ? coverMessage(error)
              : error instanceof CoverRemovalError
                ? removalMessage(error.apiError)
                : detailsMessage(error),
          );
        },
      });
  }

  private revokePreview(): void {
    const preview = this.preview();
    if (preview) {
      URL.revokeObjectURL(preview);
    }
  }
}

function detailsMessage(error: ApiError): string {
  if (error.isNetworkError) {
    return "Couldn't reach Stopify. Check your connection.";
  }
  if (error.status === 403 || error.has('Playlist.NotOwned')) {
    return 'Only the owner can edit this playlist.';
  }
  return error.has('Playlist.InvalidMetadata') ? error.message : "Couldn't save your changes. Try again.";
}

function coverMessage({ step, apiError }: CoverUploadError): string {
  if (apiError.isNetworkError) {
    return "Couldn't reach Stopify. Check your connection.";
  }
  if (step === 'processing') {
    return "Your photo is taking too long to process. Try again in a moment.";
  }
  if (step === 'link' && (apiError.status === 403 || apiError.has('Playlist.NotOwned'))) {
    return 'Only the owner can change the cover.';
  }
  return step === 'upload' ? "Couldn't upload that photo. Try another one." : "Couldn't change the cover. Try again.";
}

/** Removing the photo failed (kept apart from the details' errors for its own messages). */
class CoverRemovalError extends Error {
  constructor(readonly apiError: ApiError) {
    super(apiError.message);
  }
}

function removalMessage(error: ApiError): string {
  if (error.isNetworkError) {
    return "Couldn't reach Stopify. Check your connection.";
  }
  // The API doesn't have DELETE /playlists/{id}/cover yet: the route answers 405 (only PUT).
  if (error.status === 405 || (error.status === 404 && !error.has('Playlist.NotFound'))) {
    return "Removing photos isn't available yet.";
  }
  if (error.status === 403 || error.has('Playlist.NotOwned')) {
    return 'Only the owner can change the cover.';
  }
  return "Couldn't remove the photo. Try again.";
}
