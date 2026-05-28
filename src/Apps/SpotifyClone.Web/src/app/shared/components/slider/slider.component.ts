import { Component, input, output, HostListener, ElementRef, effect, numberAttribute } from '@angular/core';

@Component({
  selector: 'input[app-slider]',
  standalone: true,
  template: '',
  styleUrl: './slider.component.css',
})
export class SliderComponent {
  readonly value = input(0, { transform: numberAttribute });
  readonly max = input(100, { transform: numberAttribute });
  readonly valueChange = output<number>();
  readonly changeEnd = output<number>();

  private isHovering = false;

  constructor(private elementRef: ElementRef<HTMLInputElement>) {
    effect(() => {
      const el = this.elementRef.nativeElement;
      const v = this.value();
      const m = this.max();
      el.value = String(v);
      el.max = String(m);
      this.updateBackground(v, m);
    });
  }

  @HostListener('input', ['$event'])
  onInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    const v = Number(input.value);
    this.valueChange.emit(v);
  }

  @HostListener('change', ['$event'])
  onChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.changeEnd.emit(Number(input.value));
  }

  @HostListener('mouseenter')
  onMouseEnter(): void {
    this.isHovering = true;
    this.updateBackground(this.value(), this.max());
  }

  @HostListener('mouseleave')
  onMouseLeave(): void {
    this.isHovering = false;
    this.updateBackground(this.value(), this.max());
  }

  private updateBackground(val: number, max: number): void {
    const ratio = max > 0 ? (val / max) * 100 : 0;
    const fillColor = this.isHovering ? '#1db954' : '#ffffff';
    const bgColor = '#535353';
    this.elementRef.nativeElement.style.background =
      `linear-gradient(to right, ${fillColor} ${ratio}%, ${bgColor} ${ratio}%)`;
  }
}
