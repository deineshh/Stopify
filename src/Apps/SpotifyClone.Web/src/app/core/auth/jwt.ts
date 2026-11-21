/**
 * Reads the `exp` claim (seconds since epoch) from a JWT without verifying it — the API
 * verifies tokens, the client only needs to know when to refresh. Returns `null` if the
 * token can't be read.
 */
export function jwtExpiry(token: string): Date | null {
  const payload = token.split('.')[1];
  if (!payload) {
    return null;
  }
  try {
    const base64 = payload.replace(/-/g, '+').replace(/_/g, '/').padEnd(Math.ceil(payload.length / 4) * 4, '=');
    const { exp } = JSON.parse(atob(base64)) as { exp?: unknown };
    return typeof exp === 'number' ? new Date(exp * 1000) : null;
  } catch {
    return null;
  }
}
