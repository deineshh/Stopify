export interface TrackItemData {
  id: string;
  title: string;
  coverUrl: string;
  authors: { name: string; id?: string }[];
  description?: string;
  coverShape?: 'circle' | 'rounded';
}

export interface TrackTableRow {
  id: string;
  index: number;
  title: string;
  coverUrl: string;
  authors: { name: string; id?: string }[];
  album: { name: string; id?: string };
  dateAdded?: string;
  duration: string;
  saved?: boolean;
}

export interface TrackTableColumns {
  index: boolean;
  title: boolean;
  album: boolean;
  dateAdded: boolean;
  duration: boolean;
  save: boolean;
  more: boolean;
}

export interface FilterBtnData {
  id: string;
  label: string;
  active?: boolean;
}
