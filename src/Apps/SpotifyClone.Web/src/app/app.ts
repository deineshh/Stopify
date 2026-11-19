import { ChangeDetectionStrategy, Component, afterNextRender, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AuthStore } from '@core/auth/auth.store';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  template: '<router-outlet />',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class App {
  constructor() {
    // The session is browser-only (HttpOnly refresh cookie). Resolving it after the first
    // render keeps the server-rendered and hydrated markup identical.
    const auth = inject(AuthStore);
    afterNextRender(() => auth.restoreSession().subscribe());
  }
}
