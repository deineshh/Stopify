import { HttpHeaders } from '@angular/common/http';
import { toPage } from './pagination';

describe('toPage', () => {
  it('reads the X-Pagination header', () => {
    const headers = new HttpHeaders({
      'X-Pagination': JSON.stringify({ page: 2, pageSize: 10, hasPreviousPage: true, hasNextPage: true, totalCount: 35 }),
    });
    expect(toPage([1, 2], headers)).toEqual({
      items: [1, 2],
      page: 2,
      pageSize: 10,
      hasPreviousPage: true,
      hasNextPage: true,
      totalCount: 35,
    });
  });

  it('falls back when the header is missing or broken', () => {
    expect(toPage([1], new HttpHeaders(), { page: 1, pageSize: 20 })).toEqual(
      expect.objectContaining({ items: [1], page: 1, pageSize: 20, hasNextPage: false, totalCount: 1 }),
    );
    expect(toPage(null, new HttpHeaders({ 'X-Pagination': '{oops' })).items).toEqual([]);
  });
});
