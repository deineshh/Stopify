import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { NgTemplateOutlet } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MediaCard as MediaCardModel } from '@data/models/catalog.models';
import { PlayButton } from '../play-button/play-button';

/** Album / playlist / artist tile used inside shelves (mockup `.track-item`). */
@Component({
  selector: 'app-media-card',
  imports: [NgTemplateOutlet, RouterLink, PlayButton],
  templateUrl: './media-card.html',
  styleUrl: './media-card.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: 'track-item play-btn-parent',
    role: 'listitem',
  },
})
export class MediaCard {
  readonly item = input.required<MediaCardModel>();
}
