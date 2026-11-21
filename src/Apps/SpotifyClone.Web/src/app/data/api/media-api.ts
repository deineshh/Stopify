import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, defer, from, map, retry, switchMap } from 'rxjs';
import { injectApiUrl } from '@core/api/api.config';
import { imageUrl } from '@core/api/media';

/** An uploaded image, converted by the API, with the metadata the link endpoints ask for. */
export interface UploadedImage {
  imageId: string;
  width: number;
  height: number;
  fileType: string;
  sizeInBytes: number;
}

/** How long to wait for the API to convert an upload (it converts in the background). */
const PROCESSING_ATTEMPTS = 30;
const PROCESSING_INTERVAL_MS = 1000;

/** Typed client for the Streaming module's image uploads (`api/v1/media/images`). */
@Injectable({ providedIn: 'root' })
export class MediaApi {
  private readonly http = inject(HttpClient);
  private readonly url = injectApiUrl();

  /** Starts an upload and returns the new image's id. The API converts it to WebP in the background. */
  uploadImage(file: File): Observable<string> {
    const form = new FormData();
    form.append('file', file, file.name);
    return this.http.post<{ imageId: string }>(this.url('/media/images'), form).pipe(map((response) => response.imageId));
  }

  /**
   * Waits for the converted image to appear in storage, then reads its size. The API's link
   * endpoints need the metadata, and the upload endpoint doesn't return it, so it's read from
   * the WebP itself: the same file the API stores and serves.
   */
  waitForImage(imageId: string, attempts = PROCESSING_ATTEMPTS, intervalMs = PROCESSING_INTERVAL_MS): Observable<UploadedImage> {
    return defer(() => this.http.get(imageUrl(imageId), { responseType: 'blob' })).pipe(
      // Not there yet answers 403 or 404 (object storage), so any failure means "try again".
      retry({ count: attempts - 1, delay: intervalMs }),
      switchMap((blob) =>
        from(this.readSize(blob)).pipe(
          map((size) => ({ imageId, ...size, fileType: 'webp', sizeInBytes: blob.size })),
        ),
      ),
    );
  }

  /** Pixel size of an image file. */
  async readSize(blob: Blob): Promise<{ width: number; height: number }> {
    const bitmap = await createImageBitmap(blob);
    const size = { width: bitmap.width, height: bitmap.height };
    bitmap.close();
    return size;
  }
}
