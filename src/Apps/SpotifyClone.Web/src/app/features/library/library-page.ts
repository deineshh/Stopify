import { ChangeDetectionStrategy, Component } from '@angular/core';
import { LibrarySidebar } from '@layout/library-sidebar/library-sidebar';

/** "Your Library" as a full page — the Library tab on phones. */
@Component({
  selector: 'app-library-page',
  imports: [LibrarySidebar],
  template: `<app-library-sidebar />`,
  styles: `
    :host {
      display: flex;
      min-height: 100%;
    }

    app-library-sidebar {
      border-radius: 0;
      overflow: visible;
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LibraryPage {}
