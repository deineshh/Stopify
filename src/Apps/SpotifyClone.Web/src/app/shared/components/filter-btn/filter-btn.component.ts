import { Component, Input, Output, EventEmitter, HostBinding, HostListener } from '@angular/core';

@Component({
  selector: 'button[app-filter-btn]',
  standalone: true,
  template: `<ng-content />`,
  styleUrl: './filter-btn.component.css',
})
export class FilterBtnComponent {
  @Input() active = false;
  @Output() clicked = new EventEmitter<void>();

  @HostBinding('class.active') get activeClass() {
    return this.active;
  }

  @HostListener('click')
  onClick(): void {
    this.clicked.emit();
  }
}
