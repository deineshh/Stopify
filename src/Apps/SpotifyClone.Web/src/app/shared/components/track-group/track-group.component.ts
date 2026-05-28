import { Component, Input } from '@angular/core';
import { FilterBtnComponent } from '../filter-btn/filter-btn.component';
import { FilterBtnData } from '../../models/track.models';

@Component({
  selector: 'app-track-group',
  standalone: true,
  imports: [FilterBtnComponent],
  templateUrl: './track-group.component.html',
  styleUrl: './track-group.component.css',
})
export class TrackGroupComponent {
  @Input() title?: string;
  @Input() showAll?: string;
  @Input() topText?: string;
  @Input() filters?: FilterBtnData[];
}
