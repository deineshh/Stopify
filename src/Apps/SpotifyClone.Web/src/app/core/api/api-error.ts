import { HttpErrorResponse } from '@angular/common/http';

export interface ApiErrorEntry {
  code: string;
  description: string;
}

/**
 * Normalised API failure. The backend answers with RFC 7807 ProblemDetails where
 * `title` is the primary error code and `errors` lists every `{ code, description }`.
 */
export class ApiError extends Error {
  constructor(
    readonly status: number,
    /** Primary error code, e.g. `Auth.InvalidPassword` or `Validation.Email`. */
    readonly code: string,
    message: string,
    readonly errors: readonly ApiErrorEntry[] = [],
  ) {
    super(message);
    this.name = 'ApiError';
  }

  /** True if any of the returned errors has `code` (exact) or starts with `code.` */
  has(code: string): boolean {
    return [this.code, ...this.errors.map((error) => error.code)].some(
      (value) => value === code || value.startsWith(`${code}.`),
    );
  }

  get isNetworkError(): boolean {
    return this.status === 0;
  }

  get isRateLimited(): boolean {
    return this.status === 429;
  }
}

interface ProblemDetailsBody {
  title?: string;
  detail?: string;
  status?: number;
  message?: string;
  errors?: unknown;
}

/** Converts anything thrown by HttpClient into an {@link ApiError}. */
export function toApiError(error: unknown): ApiError {
  if (error instanceof ApiError) {
    return error;
  }
  if (!(error instanceof HttpErrorResponse)) {
    return new ApiError(-1, 'Client.Unknown', error instanceof Error ? error.message : 'Unknown error');
  }

  if (error.status === 0) {
    return new ApiError(0, 'Network.Unavailable', "Couldn't reach Stopify. Check your connection and try again.");
  }
  if (error.status === 429) {
    return new ApiError(429, 'RateLimit.Exceeded', 'Too many attempts. Wait a moment and try again.');
  }

  const body = (typeof error.error === 'object' && error.error !== null ? error.error : {}) as ProblemDetailsBody;
  const errors = parseErrors(body.errors);
  const code = body.title ?? errors[0]?.code ?? `Http.${error.status}`;
  const message =
    body.detail ?? errors[0]?.description ?? body.message ?? 'Something went wrong. Please try again.';
  return new ApiError(error.status, code, message, errors);
}

function parseErrors(value: unknown): ApiErrorEntry[] {
  // Our ProblemDetails: [{ code, description }]
  if (Array.isArray(value)) {
    return value
      .filter((entry): entry is ApiErrorEntry => typeof entry?.code === 'string')
      .map((entry) => ({ code: entry.code, description: String(entry.description ?? '') }));
  }
  // ASP.NET model validation: { Field: ["message", ...] }
  if (value && typeof value === 'object') {
    return Object.entries(value as Record<string, unknown>).flatMap(([field, messages]) =>
      (Array.isArray(messages) ? messages : [messages]).map((message) => ({
        code: `Validation.${field}`,
        description: String(message),
      })),
    );
  }
  return [];
}
