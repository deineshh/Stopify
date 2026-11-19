import { Pipe, PipeTransform } from '@angular/core';
import { EntityRef } from '@data/models/catalog.models';

/** "DESH, Young Fly, Azahriah" — used for titles and accessible labels. */
@Pipe({ name: 'artistNames' })
export class ArtistNamesPipe implements PipeTransform {
  transform(artists: readonly EntityRef[] | null | undefined): string {
    return artists?.map((artist) => artist.name).join(', ') ?? '';
  }
}
