import { Pipe, PipeTransform } from '@angular/core';

/**
 * Formats seconds as a track time ("4:49") or, with `'long'`, as a total
 * running time ("3 hr 30 min").
 */
@Pipe({ name: 'duration' })
export class DurationPipe implements PipeTransform {
  transform(totalSeconds: number | null | undefined, format: 'clock' | 'long' = 'clock'): string {
    if (totalSeconds == null || !Number.isFinite(totalSeconds) || totalSeconds < 0) {
      return '';
    }
    const seconds = Math.floor(totalSeconds);
    const hours = Math.floor(seconds / 3600);
    const minutes = Math.floor((seconds % 3600) / 60);
    const rest = seconds % 60;

    if (format === 'long') {
      if (hours > 0) {
        return minutes > 0 ? `${hours} hr ${minutes} min` : `${hours} hr`;
      }
      return minutes > 0 ? `${minutes} min ${rest} sec` : `${rest} sec`;
    }

    const paddedSeconds = rest.toString().padStart(2, '0');
    return hours > 0
      ? `${hours}:${minutes.toString().padStart(2, '0')}:${paddedSeconds}`
      : `${minutes}:${paddedSeconds}`;
  }
}
