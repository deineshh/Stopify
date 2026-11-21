import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { CatalogService } from '@data/services/catalog.service';
import { PageState } from '@shared/ui/page-state/page-state';

/** "Browse all" genre and mood grid shown while the search field is empty. */
@Component({
  selector: 'app-search-browse-page',
  imports: [RouterLink, PageState],
  templateUrl: './search-browse-page.html',
  styleUrl: './search-browse-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'search-browse' },
})
export class SearchBrowsePage {
  private readonly catalog = inject(CatalogService);
  protected readonly genres = rxResource({ stream: () => this.catalog.getGenres() });
}
