import { HttpHeaders } from '@angular/common/http';

/** One page of a list endpoint. Items come in the body, paging info in the `X-Pagination` header. */
export interface Page<T> {
  items: T[];
  page: number;
  pageSize: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
  totalCount: number;
}

export interface PageRequest {
  page?: number;
  /** The API caps this at 50. */
  pageSize?: number;
}

/** Builds a {@link Page} from a list response, tolerating a missing or malformed header. */
export function toPage<T>(items: T[] | null, headers: HttpHeaders, request: PageRequest = {}): Page<T> {
  const list = items ?? [];
  const fallback: Omit<Page<T>, 'items'> = {
    page: request.page ?? 1,
    pageSize: request.pageSize ?? list.length,
    hasNextPage: false,
    hasPreviousPage: (request.page ?? 1) > 1,
    totalCount: list.length,
  };
  const raw = headers.get('X-Pagination');
  if (!raw) {
    return { items: list, ...fallback };
  }
  try {
    const parsed = JSON.parse(raw) as Partial<Omit<Page<T>, 'items'>>;
    return { items: list, ...fallback, ...parsed };
  } catch {
    return { items: list, ...fallback };
  }
}
