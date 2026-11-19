import { DurationPipe } from './duration.pipe';

describe('DurationPipe', () => {
  const pipe = new DurationPipe();

  it('formats track times as m:ss', () => {
    expect(pipe.transform(0)).toBe('0:00');
    expect(pipe.transform(61)).toBe('1:01');
    expect(pipe.transform(289)).toBe('4:49');
  });

  it('adds hours when needed', () => {
    expect(pipe.transform(3725)).toBe('1:02:05');
  });

  it('formats long running times', () => {
    expect(pipe.transform(3 * 3600 + 30 * 60, 'long')).toBe('3 hr 30 min');
    expect(pipe.transform(7200, 'long')).toBe('2 hr');
    expect(pipe.transform(125, 'long')).toBe('2 min 5 sec');
  });

  it('returns an empty string for invalid input', () => {
    expect(pipe.transform(null)).toBe('');
    expect(pipe.transform(undefined)).toBe('');
    expect(pipe.transform(-1)).toBe('');
    expect(pipe.transform(Number.NaN)).toBe('');
  });
});
