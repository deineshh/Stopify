import { ArtistNamesPipe } from './artist-names.pipe';

describe('ArtistNamesPipe', () => {
  const pipe = new ArtistNamesPipe();

  it('joins artist names with commas', () => {
    expect(
      pipe.transform([
        { id: 'desh', name: 'DESH' },
        { id: 'young-fly', name: 'Young Fly' },
      ]),
    ).toBe('DESH, Young Fly');
  });

  it('handles missing input', () => {
    expect(pipe.transform(undefined)).toBe('');
    expect(pipe.transform([])).toBe('');
  });
});
