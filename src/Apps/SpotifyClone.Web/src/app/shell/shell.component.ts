import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { GlobalHeaderComponent } from './global-header/global-header.component';
import { LeftSidebarComponent } from './left-sidebar/left-sidebar.component';
import { RightSidebarComponent } from './right-sidebar/right-sidebar.component';
import { NowPlayingBarComponent } from './now-playing-bar/now-playing-bar.component';
import { FooterComponent } from './footer/footer.component';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [
    RouterOutlet,
    GlobalHeaderComponent,
    LeftSidebarComponent,
    RightSidebarComponent,
    NowPlayingBarComponent,
    FooterComponent,
  ],
  templateUrl: './shell.component.html',
  styleUrl: './shell.component.css',
})
export class ShellComponent {}
