import { ChangeDetectionStrategy, Component, PLATFORM_ID, Signal, computed, inject, linkedSignal, signal } from '@angular/core';
import { NgTemplateOutlet, isPlatformBrowser } from '@angular/common';
import { rxResource } from '@angular/core/rxjs-interop';
import { DIALOG_DATA, DialogRef } from '@angular/cdk/dialog';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import { finalize, map, switchMap, timer } from 'rxjs';
import { imageUrl } from '@core/api/media';
import { CatalogApi } from '@data/api/catalog-api';
import { Collaborator, PlaylistRef } from '@data/models/playlist.models';
import { PlaylistEditor } from '@data/services/playlist-editor.service';

export interface CollaboratorsDialogData {
  playlist: Pick<PlaylistRef, 'id' | 'name'>;
  ownerId: string;
  /** The playlist page's list, which updates when the playlist reloads. */
  collaborators: Signal<readonly Collaborator[]>;
}

const SEARCH_DEBOUNCE_MS = 250;
const SEARCH_LIMIT = 8;

/**
 * "Invite collaborators": find people by name and let them add, remove and reorder songs, or
 * take that away again. Owner only. Spotify invites with a link; the API adds users directly.
 */
@Component({
  selector: 'app-collaborators-dialog',
  imports: [FaIconComponent, NgTemplateOutlet],
  templateUrl: './collaborators-dialog.html',
  styleUrl: './collaborators-dialog.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CollaboratorsDialog {
  protected readonly data = inject<CollaboratorsDialogData>(DIALOG_DATA);
  protected readonly ref = inject<DialogRef<void>>(DialogRef);
  private readonly api = inject(CatalogApi);
  private readonly editor = inject(PlaylistEditor);
  private readonly debounceMs = isPlatformBrowser(inject(PLATFORM_ID)) ? SEARCH_DEBOUNCE_MS : 0;

  /** Shows a change as soon as the API accepts it; the page's reload then brings the real list. */
  protected readonly members = linkedSignal(() => this.data.collaborators());
  /** People with a request on its way (their button is disabled meanwhile). */
  protected readonly busy = signal<ReadonlySet<string>>(new Set());

  protected readonly term = signal('');
  private readonly query = computed(() => this.term().trim());

  private readonly search = rxResource({
    params: () => this.query() || undefined,
    stream: ({ params }) =>
      timer(this.debounceMs).pipe(
        switchMap(() => this.api.search(params, SEARCH_LIMIT)),
        map((results): Collaborator[] =>
          results.users.map((user) => ({
            id: user.id,
            name: user.name,
            avatar: user.avatarImageId ? imageUrl(user.avatarImageId) : null,
          })),
        ),
      ),
  });

  /** Matches that can still be added (not the owner, not already a collaborator). */
  protected readonly results = computed(() => {
    if (!this.query() || !this.search.hasValue()) {
      return [];
    }
    const taken = new Set([this.data.ownerId, ...this.members().map((member) => member.id)]);
    return this.search.value().filter((user) => !taken.has(user.id));
  });

  protected readonly searching = computed(() => !!this.query() && this.search.isLoading());
  protected readonly searchFailed = computed(() => !!this.query() && !!this.search.error());

  protected add(user: Collaborator): void {
    this.run(user, this.editor.addCollaborator(this.data.playlist, user), () =>
      this.members.update((members) => [...members, user]),
    );
  }

  protected remove(user: Collaborator): void {
    this.run(user, this.editor.removeCollaborator(this.data.playlist, user), () =>
      this.members.update((members) => members.filter((member) => member.id !== user.id)),
    );
  }

  protected initial(name: string): string {
    return name.trim().charAt(0).toUpperCase() || '?';
  }

  private run(user: Collaborator, request: ReturnType<PlaylistEditor['addCollaborator']>, done: () => void): void {
    this.busy.update((busy) => new Set(busy).add(user.id));
    request
      .pipe(
        finalize(() =>
          this.busy.update((busy) => {
            const next = new Set(busy);
            next.delete(user.id);
            return next;
          }),
        ),
      )
      .subscribe(done);
  }
}

