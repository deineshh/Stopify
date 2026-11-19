import { TestBed } from '@angular/core/testing';
import { LibraryService } from './library.service';

describe('LibraryService', () => {
  let service: LibraryService;

  beforeEach(() => {
    service = TestBed.inject(LibraryService);
  });

  it('lists every item by default', () => {
    expect(service.visibleItems().length).toBe(20);
  });

  it('filters by title, case-insensitively', () => {
    service.query.set('LIKED');
    expect(service.visibleItems().length).toBe(20);
    service.query.set('no such playlist');
    expect(service.visibleItems()).toEqual([]);
  });

  it('toggles the kind filter on and off', () => {
    service.toggleFilter('artists');
    expect(service.filter()).toBe('artists');
    expect(service.visibleItems()).toEqual([]);

    service.toggleFilter('artists');
    expect(service.filter()).toBeNull();
    expect(service.visibleItems().length).toBe(20);
  });
});
