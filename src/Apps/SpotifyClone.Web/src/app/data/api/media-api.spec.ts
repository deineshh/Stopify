import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { MediaApi } from './media-api';

describe('MediaApi', () => {
  function setup() {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    const api = TestBed.inject(MediaApi);
    vi.spyOn(api, 'readSize').mockResolvedValue({ width: 640, height: 480 });
    return { api, http: TestBed.inject(HttpTestingController) };
  }

  afterEach(() => TestBed.inject(HttpTestingController).verify());

  it('uploads the file as multipart form data', () => {
    const { api, http } = setup();
    const id = vi.fn();
    const file = new File(['x'], 'cover.png', { type: 'image/png' });
    api.uploadImage(file).subscribe(id);

    const req = http.expectOne('/api/v1/media/images');
    expect(req.request.method).toBe('POST');
    const sent = (req.request.body as FormData).get('file') as File;
    expect([sent.name, sent.type, sent.size]).toEqual(['cover.png', 'image/png', 1]);
    req.flush({ imageId: 'img1' }, { status: 202, statusText: 'Accepted' });
    expect(id).toHaveBeenCalledWith('img1');
  });

  it('waits for the converted image, then reports its metadata', async () => {
    vi.useFakeTimers();
    try {
      const { api, http } = setup();
      const done = vi.fn();
      api.waitForImage('img1', 5, 1000).subscribe(done);

      // Not converted yet: object storage answers 404 (or 403).
      http.expectOne('/images/img1/image.webp').flush(null, { status: 404, statusText: 'Not Found' });
      await vi.advanceTimersByTimeAsync(1000);
      http.expectOne('/images/img1/image.webp').flush(null, { status: 403, statusText: 'Forbidden' });
      await vi.advanceTimersByTimeAsync(1000);
      http.expectOne('/images/img1/image.webp').flush(new Blob(['12345678'], { type: 'image/webp' }));
      await vi.advanceTimersByTimeAsync(0);

      expect(done).toHaveBeenCalledWith({ imageId: 'img1', width: 640, height: 480, fileType: 'webp', sizeInBytes: 8 });
    } finally {
      vi.useRealTimers();
    }
  });

  it('gives up after the last attempt', async () => {
    vi.useFakeTimers();
    try {
      const { api, http } = setup();
      const failed = vi.fn();
      api.waitForImage('img1', 2, 1000).subscribe({ error: failed });
      http.expectOne('/images/img1/image.webp').flush(null, { status: 404, statusText: 'Not Found' });
      await vi.advanceTimersByTimeAsync(1000);
      http.expectOne('/images/img1/image.webp').flush(null, { status: 404, statusText: 'Not Found' });
      expect(failed).toHaveBeenCalled();
    } finally {
      vi.useRealTimers();
    }
  });
});
