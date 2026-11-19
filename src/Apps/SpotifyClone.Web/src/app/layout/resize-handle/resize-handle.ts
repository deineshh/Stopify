import { ChangeDetectionStrategy, Component, input, output, signal } from '@angular/core';

/**
 * Vertical drag handle between layout columns (mockup `.layout-resizer`).
 * Emits the new width of the panel it belongs to; keyboard arrows resize too.
 */
@Component({
  selector: 'app-resize-handle',
  template: '',
  styles: `
    :host {
      cursor: col-resize;
      flex-shrink: 0;
      height: 100%;
      position: relative;
      touch-action: none;
      width: 8px;
    }

    :host::after {
      background-color: transparent;
      content: '';
      inset: 0 3px;
      position: absolute;
      transition: background-color 0.15s ease-in-out;
    }

    :host(:hover)::after,
    :host(.dragging)::after,
    :host(:focus-visible)::after {
      background-color: rgba(255, 255, 255, 0.3);
    }

    :host(:focus-visible) {
      outline: none;
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: 'layout-resizer',
    role: 'separator',
    tabindex: '0',
    'aria-orientation': 'vertical',
    '[attr.aria-label]': 'label()',
    '[attr.aria-valuenow]': 'width()',
    '[class.dragging]': 'dragging()',
    '(pointerdown)': 'start($event)',
    '(pointermove)': 'move($event)',
    '(pointerup)': 'stop($event)',
    '(pointercancel)': 'stop($event)',
    '(keydown.arrowleft)': 'nudge($event, -16)',
    '(keydown.arrowright)': 'nudge($event, 16)',
  },
})
export class ResizeHandle {
  /** Current width of the panel being resized. */
  readonly width = input.required<number>();
  /** Which edge of the panel the handle sits on (decides the drag direction). */
  readonly edge = input<'start' | 'end'>('end');
  readonly label = input('Resize');
  readonly resized = output<number>();

  protected readonly dragging = signal(false);
  private startX = 0;
  private startWidth = 0;

  protected start(event: PointerEvent): void {
    if (event.button !== 0) {
      return;
    }
    event.preventDefault();
    (event.currentTarget as HTMLElement).setPointerCapture(event.pointerId);
    this.startX = event.clientX;
    this.startWidth = this.width();
    this.dragging.set(true);
  }

  protected move(event: PointerEvent): void {
    if (!this.dragging()) {
      return;
    }
    const delta = event.clientX - this.startX;
    this.resized.emit(this.startWidth + (this.edge() === 'end' ? delta : -delta));
  }

  protected stop(event: PointerEvent): void {
    if (!this.dragging()) {
      return;
    }
    (event.currentTarget as HTMLElement).releasePointerCapture(event.pointerId);
    this.dragging.set(false);
  }

  protected nudge(event: Event, step: number): void {
    event.preventDefault();
    this.resized.emit(this.width() + (this.edge() === 'end' ? step : -step));
  }
}
