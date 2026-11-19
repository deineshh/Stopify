import { ChangeDetectionStrategy, Component, computed, input, model, signal } from '@angular/core';

/**
 * Thin progress/volume slider. The filled part turns green while hovered
 * (replaces mockups/js/slider.js).
 */
@Component({
  selector: 'app-range-slider',
  template: `
    <input
      class="now-playing-bar__slider--input"
      type="range"
      min="0"
      [max]="max()"
      [step]="step()"
      [value]="value()"
      [attr.aria-label]="label()"
      [attr.aria-valuetext]="valueText()"
      [style.background]="background()"
      (input)="onInput($event)"
      (pointerenter)="hovered.set(true)"
      (pointerleave)="hovered.set(false)"
      (focus)="hovered.set(true)"
      (blur)="hovered.set(false)"
    />
  `,
  styleUrl: './range-slider.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RangeSlider {
  readonly value = model(0);
  readonly max = input(100);
  readonly step = input(1);
  readonly label = input.required<string>();
  readonly valueText = input<string>();

  protected readonly hovered = signal(false);

  protected readonly background = computed(() => {
    const percent = (this.value() / this.max()) * 100;
    const fill = this.hovered() ? '#1db954' : '#ffffff';
    return `linear-gradient(to right, ${fill} ${percent}%, #535353 ${percent}%)`;
  });

  protected onInput(event: Event): void {
    this.value.set(Number((event.target as HTMLInputElement).value));
  }
}
