import { PLACEHOLDER_COVER, imageUrl, timeSpanToSeconds } from './media';

describe('media helpers', () => {
  it('builds image URLs from ids', () => {
    expect(imageUrl('3f2c')).toBe('/images/3f2c/image.webp');
    expect(imageUrl(null)).toBe(PLACEHOLDER_COVER);
    expect(imageUrl(undefined, '/fallback.png')).toBe('/fallback.png');
  });

  it('parses .NET TimeSpan strings', () => {
    expect(timeSpanToSeconds('00:04:49')).toBe(289);
    expect(timeSpanToSeconds('00:01:01.5000000')).toBe(61);
    expect(timeSpanToSeconds('1.02:00:00')).toBe(93_600);
    expect(timeSpanToSeconds(null)).toBe(0);
    expect(timeSpanToSeconds('garbage')).toBe(0);
  });
});
