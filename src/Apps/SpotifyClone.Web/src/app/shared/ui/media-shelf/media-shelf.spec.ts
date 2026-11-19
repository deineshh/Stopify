import { TestBed } from '@angular/core/testing';
import { MediaShelf } from './media-shelf';
import { provideTestDefaults } from '../../../testing/test-providers';

describe('MediaShelf', () => {
  it('renders the header, every card and switches the active filter', () => {
    TestBed.configureTestingModule({ providers: provideTestDefaults() });
    const fixture = TestBed.createComponent(MediaShelf);
    fixture.componentRef.setInput('shelf', {
      id: 'discography',
      eyebrow: 'Made For',
      title: 'Discography',
      showAllLabel: 'Show all',
      filters: ['Popular releases', 'Albums'],
      items: [
        { id: 'a', kind: 'album', title: 'A', cover: '/a.jpg', shape: 'rounded' },
        { id: 'b', kind: 'album', title: 'B', cover: '/b.jpg', shape: 'rounded' },
      ],
    });
    fixture.detectChanges();
    const el = fixture.nativeElement as HTMLElement;

    expect(el.querySelector('h2')?.textContent).toContain('Discography');
    expect(el.querySelector('.track-group__header-group--title-top-text')?.textContent).toContain('Made For');
    expect(el.querySelectorAll('app-media-card').length).toBe(2);

    const chips = el.querySelectorAll<HTMLButtonElement>('.track-group__filter-group button');
    expect(chips[0].classList).toContain('active');
    chips[1].click();
    fixture.detectChanges();
    expect(chips[0].classList).not.toContain('active');
    expect(chips[1].getAttribute('aria-pressed')).toBe('true');
  });
});
