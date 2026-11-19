import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { CatalogService } from '@data/services/catalog.service';

/** "Browse all" genre grid shown while the search field is empty. */
@Component({
  selector: 'app-search-browse-page',
  imports: [RouterLink],
  templateUrl: './search-browse-page.html',
  styleUrl: './search-browse-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'search-browse' },
})
export class SearchBrowsePage {
  protected readonly genres = toSignal(inject(CatalogService).getGenres(), { requireSync: true });
}
