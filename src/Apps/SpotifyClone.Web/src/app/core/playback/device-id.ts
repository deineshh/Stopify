import { InjectionToken } from '@angular/core';

const STORAGE_KEY = 'stopify.deviceId';

/**
 * Identifies this browser to the playback API, so the server knows which device is playing and
 * can hand playback over between devices. Kept in localStorage; a fresh id if storage is unavailable.
 */
export const DEVICE_ID = new InjectionToken<string>('DEVICE_ID', {
  providedIn: 'root',
  factory: () => {
    try {
      const stored = globalThis.localStorage?.getItem(STORAGE_KEY);
      if (stored) {
        return stored;
      }
      const id = newUuid();
      globalThis.localStorage?.setItem(STORAGE_KEY, id);
      return id;
    } catch {
      return newUuid();
    }
  },
});

/** `crypto.randomUUID` only exists in secure contexts (not on http://<LAN IP>), so fall back to v4 by hand. */
export function newUuid(): string {
  if (typeof crypto.randomUUID === 'function') {
    return crypto.randomUUID();
  }
  const bytes = crypto.getRandomValues(new Uint8Array(16));
  bytes[6] = (bytes[6] & 0x0f) | 0x40;
  bytes[8] = (bytes[8] & 0x3f) | 0x80;
  const hex = [...bytes].map((byte) => byte.toString(16).padStart(2, '0')).join('');
  return `${hex.slice(0, 8)}-${hex.slice(8, 12)}-${hex.slice(12, 16)}-${hex.slice(16, 20)}-${hex.slice(20)}`;
}
