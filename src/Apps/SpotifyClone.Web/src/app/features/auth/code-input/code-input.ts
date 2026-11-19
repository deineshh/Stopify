import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  afterNextRender,
  booleanAttribute,
  computed,
  input,
  model,
  viewChildren,
} from '@angular/core';

/**
 * Six one-digit boxes for verification codes (SMS log-in, email verification, password reset).
 * Typing advances, Backspace goes back, and pasting a whole code fills every box.
 * The value is the digits typed so far, e.g. "1234".
 */
@Component({
  selector: 'app-code-input',
  template: `
    <div class="sign-up-page__verification-group--code-group" role="group" [attr.aria-label]="label()" (paste)="onPaste($event)">
      @for (slot of slots(); track slot) {
        <input
          #box
          class="sign-up-page__verification-group--code-input"
          type="text"
          inputmode="numeric"
          maxlength="1"
          autocomplete="one-time-code"
          required
          [class.invalid]="invalid()"
          [value]="value().charAt(slot)"
          [attr.aria-label]="'Digit ' + (slot + 1)"
          [attr.aria-invalid]="invalid()"
          (input)="onInput(slot, box)"
          (keydown.backspace)="onBackspace(slot, box)"
          (focus)="box.select()"
        />
      }
    </div>
  `,
  styleUrl: './code-input.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CodeInput {
  readonly value = model('');
  readonly length = input(6);
  readonly label = input('Verification code');
  readonly invalid = input(false, { transform: booleanAttribute });
  readonly autofocus = input(true, { transform: booleanAttribute });

  protected readonly slots = computed(() => Array.from({ length: this.length() }, (_, index) => index));
  private readonly boxes = viewChildren<ElementRef<HTMLInputElement>>('box');

  constructor() {
    afterNextRender(() => {
      if (this.autofocus()) {
        this.focus(Math.min(this.value().length, this.length() - 1));
      }
    });
  }

  protected onInput(index: number, box: HTMLInputElement): void {
    const digit = box.value.replace(/\D/g, '').slice(-1);
    box.value = digit;
    const digits = this.digits();
    digits[index] = digit;
    this.value.set(digits.join(''));
    if (digit) {
      this.focus(index + 1);
    }
  }

  protected onBackspace(index: number, box: HTMLInputElement): void {
    if (box.value === '' && index > 0) {
      this.focus(index - 1);
    }
  }

  protected onPaste(event: ClipboardEvent): void {
    const pasted = (event.clipboardData?.getData('text') ?? '').replace(/\D/g, '').slice(0, this.length());
    if (!pasted) {
      return;
    }
    event.preventDefault();
    this.value.set(pasted);
    this.focus(Math.min(pasted.length, this.length() - 1));
  }

  /** Current digits padded to `length` (empty string where nothing was typed yet). */
  private digits(): string[] {
    const current = this.value();
    return Array.from({ length: this.length() }, (_, index) => current[index] ?? '');
  }

  private focus(index: number): void {
    this.boxes()[index]?.nativeElement.focus();
  }
}
