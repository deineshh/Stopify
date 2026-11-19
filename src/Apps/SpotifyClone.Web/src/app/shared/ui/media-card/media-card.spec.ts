import { TestBed } from '@angular/core/testing';
import { MediaCard } from './media-card';
import { MediaCard as MediaCardModel } from '@data/models/catalog.models';
import { provideTestDefaults } from '../../../testing/test-providers';

function render(item: MediaCardModel): HTMLElement {
  TestBed.configureTestingModule({ providers: provideTestDefaults() });
  const fixture = TestBed.createComponent(MediaCard);
  fixture.componentRef.setInput('item', item);
  fixture.detectChanges();
  return fixture.nativeElement as HTMLElement;
}

describe('MediaCard', () => {
  it('renders title, bulleted meta and a comma separated author list', () => {
    const el = render({
      id: 'album-1',
      kind: 'album',
      title: 'AL NACER!',
      cover: '/cover.jpg',
      shape: 'rounded',
      meta: ['2025'],
      authors: [
        { id: 'sayfalse', name: 'Sayfalse' },
        { id: 'nulteex', name: 'Nulteex' },
      ],
    });

    expect(el.querySelector('.track-item__title-text')?.textContent?.trim()).toBe('AL NACER!');
    expect(el.querySelector('.track-item__description-section')?.textContent?.trim()).toBe('2025');
    const authors = el.querySelectorAll('.track-item__author');
    expect(authors.length).toBe(2);
    expect(authors[1].classList).toContain('last-author');
    expect(authors[0].getAttribute('href')).toBe('/artist/sayfalse');
    expect(el.querySelector('.track-item__cover')?.classList).toContain('rounded');
    expect(el.querySelector('a.track-item__title-anchor')?.getAttribute('href')).toBe('/album/album-1');
  });

  it('renders mixes without a title (description only)', () => {
    const el = render({
      id: 'mix-1',
      kind: 'mix',
      cover: '/cover.jpg',
      shape: 'circle',
      text: ['Repsaj, Kordhell and more'],
    });

    expect(el.querySelector('.track-item__info-group')).toBeNull();
    expect(el.querySelector('.track-item__description')?.textContent).toContain('Repsaj, Kordhell and more');
    expect(el.querySelector('.track-item__cover')?.classList).toContain('circle');
  });
});
