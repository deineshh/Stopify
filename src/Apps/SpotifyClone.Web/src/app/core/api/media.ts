/** Shown when an entity has no artwork yet. */
export const PLACEHOLDER_COVER = '/assets/images/song.jpg';

/**
 * Public URL of an uploaded image. The Streaming module stores every image as
 * `/{imageBucket}/{imageId}/image.webp` (see GetImageAssetQueryHandler), so no request is
 * needed to resolve it. `/images` is proxied to object storage like `/audio`.
 */
export function imageUrl(imageId: string | null | undefined, fallback = PLACEHOLDER_COVER): string {
  return imageId ? `/images/${encodeURIComponent(imageId)}/image.webp` : fallback;
}

/**
 * Parses a .NET `TimeSpan` as serialised by System.Text.Json (`[d.]hh:mm:ss[.fffffff]`)
 * into whole seconds. Returns 0 for missing or unreadable values.
 */
export function timeSpanToSeconds(value: string | null | undefined): number {
  if (!value) {
    return 0;
  }
  const match = /^(-)?(?:(\d+)\.)?(\d{1,2}):(\d{2}):(\d{2})(?:\.\d+)?$/.exec(value.trim());
  if (!match) {
    return 0;
  }
  const [, negative, days, hours, minutes, seconds] = match;
  const total = Number(days ?? 0) * 86_400 + Number(hours) * 3600 + Number(minutes) * 60 + Number(seconds);
  return negative ? -total : total;
}
