import { TestBed } from '@angular/core/testing';
import { CodeInput } from './code-input';

describe('CodeInput', () => {
  function setup() {
    const fixture = TestBed.createComponent(CodeInput);
    fixture.detectChanges();
    const boxes = Array.from((fixture.nativeElement as HTMLElement).querySelectorAll('input'));
    return { fixture, boxes, component: fixture.componentInstance };
  }

  it('renders six digit boxes', () => {
    expect(setup().boxes.length).toBe(6);
  });

  it('keeps digits only and moves focus forward', () => {
    const { fixture, boxes, component } = setup();
    boxes[0].value = 'a';
    boxes[0].dispatchEvent(new Event('input'));
    expect(component.value()).toBe('');

    boxes[0].value = '4';
    boxes[0].dispatchEvent(new Event('input'));
    fixture.detectChanges();
    expect(component.value()).toBe('4');
    expect(document.activeElement).toBe(boxes[1]);
  });

  it('fills every box from a pasted code', () => {
    const { fixture, boxes, component } = setup();
    const paste = new Event('paste', { bubbles: true, cancelable: true }) as ClipboardEvent;
    Object.defineProperty(paste, 'clipboardData', { value: { getData: () => ' 12-34 56 ' } });
    boxes[0].dispatchEvent(paste);
    fixture.detectChanges();
    expect(component.value()).toBe('123456');
    expect(boxes.map((box) => box.value).join('')).toBe('123456');
  });
});
