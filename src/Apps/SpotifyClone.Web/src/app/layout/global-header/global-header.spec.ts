import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { GlobalHeader } from './global-header';
import { provideTestDefaults } from '../../testing/test-providers';

describe('GlobalHeader', () => {
  it('navigates to search results while typing and back to browse when cleared', async () => {
    TestBed.configureTestingModule({ providers: provideTestDefaults() });
    const router = TestBed.inject(Router);
    const navigate = vi.spyOn(router, 'navigate').mockResolvedValue(true);
    const fixture = TestBed.createComponent(GlobalHeader);
    fixture.detectChanges();
    const input = (fixture.nativeElement as HTMLElement).querySelector<HTMLInputElement>('input')!;

    input.value = 'azahriah';
    input.dispatchEvent(new Event('input'));
    expect(navigate).toHaveBeenLastCalledWith(['/search', 'azahriah'], expect.any(Object));

    (fixture.nativeElement as HTMLElement)
      .querySelector<HTMLButtonElement>('.global-header__search-btns--clear-btn')!
      .click();
    expect(navigate).toHaveBeenLastCalledWith(['/search'], expect.any(Object));
  });
});
