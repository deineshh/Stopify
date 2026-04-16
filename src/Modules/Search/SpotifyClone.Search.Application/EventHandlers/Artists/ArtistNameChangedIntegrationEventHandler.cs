using MediatR;
using SpotifyClone.Search.Application.Abstractions.Services;
using SpotifyClone.Search.Application.Models;
using SpotifyClone.Search.Application.Models.Documents;
using SpotifyClone.Search.Application.Models.Documents.Compacts;
using SpotifyClone.Shared.IntegrationEvents.Catalog.Artists;

namespace SpotifyClone.Search.Application.EventHandlers.Artists;

internal sealed class ArtistNameChangedIntegrationEventHandler(
    ISearchIndexer searchIndexer,
    ISearchProvider searchProvider)
    : INotificationHandler<ArtistNameChangedIntegrationEvent>
{
    private readonly ISearchIndexer _searchIndexer = searchIndexer;
    private readonly ISearchProvider _searchProvider = searchProvider;

    public async Task Handle(
        ArtistNameChangedIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        SearchResult<ArtistSearchDocument> artistsResult
            = await _searchProvider.SearchAsync<ArtistSearchDocument>(
                SearchIndexNames.Artists,
                $"id = {notification.Id}",
                cancellationToken: cancellationToken);
        if (artistsResult.TotalCount <= 0)
        {
            return;
        }

        ArtistSearchDocument artist = artistsResult.Items.First() with
        {
            Name = notification.Name
        };
        await _searchIndexer.IndexDocumentAsync(
            SearchIndexNames.Artists, artist, cancellationToken);

        SearchResult<AlbumSearchDocument> albumsResult
            = await _searchProvider.SearchAsync<AlbumSearchDocument>(
                SearchIndexNames.Albums,
                $"artists.id = {notification.Id}",
                cancellationToken: cancellationToken);
        if (albumsResult.TotalCount <= 0)
        {
            return;
        }

        AlbumSearchDocument[] updatedAlbums = albumsResult.Items.Select(album =>
        {
            ArtistCompactDocument[] updatedArtists = album.Artists
                .Select(artist =>
                    artist.Id == notification.Id.ToString()
                        ? artist with { Name = notification.Name }
                        : artist)
                .ToArray();

            return album with { Artists = updatedArtists };
        }).ToArray();

        await _searchIndexer.IndexDocumentsAsync(
            SearchIndexNames.Albums, updatedAlbums, cancellationToken);

        SearchResult<TrackSearchDocument> tracksResult
            = await _searchProvider.SearchAsync<TrackSearchDocument>(
                SearchIndexNames.Tracks,
                $"genres.id = {notification.Id}",
                cancellationToken: cancellationToken);
        if (tracksResult.TotalCount <= 0)
        {
            return;
        }

        TrackSearchDocument[] updatedTracks = tracksResult.Items.Select(track =>
        {
            ArtistCompactDocument[] updatedArtists = track.Artists
                .Select(artist =>
                    artist.Id == notification.Id.ToString()
                        ? artist with { Name = notification.Name }
                        : artist)
                .ToArray();
            return track with { Artists = updatedArtists };
        }).ToArray();

        await _searchIndexer.IndexDocumentsAsync(
            SearchIndexNames.Tracks, updatedTracks, cancellationToken);
    }
}
