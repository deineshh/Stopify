namespace SpotifyClone.Desktop.Utilities.Enums.Playlist;

public enum PlaylistSortType
{
    Off = 0, // No sorting applied.

    TitleAsc = 1, // Sort by title in ascending order.
    TitleDesc = 2, // Sort by title in descending order.

    ArtistAsc = 3, // Sort by artist in ascending order.
    ArtistDesc = 4, // Sort by artist in descending order.

    AlbumAsc = 5, // Sort by album in ascending order.
    AlbumDesc = 6, // Sort by album in descending order.

    DateAsc = 7, // Sort by date in ascending order.
    DateDesc = 8, // Sort by date in descending order.

    DurationAsc = 9, // Sort by duration in ascending order.
    DurationDesc = 10, // Sort by duration in descending order.
}
