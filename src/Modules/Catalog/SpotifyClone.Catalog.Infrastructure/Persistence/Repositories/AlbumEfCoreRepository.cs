using Microsoft.EntityFrameworkCore;
using SpotifyClone.Catalog.Domain.Aggregates.Albums;
using SpotifyClone.Catalog.Domain.Aggregates.Albums.ValueObjects;
using SpotifyClone.Catalog.Domain.Aggregates.Artists.ValueObjects;
using SpotifyClone.Catalog.Infrastructure.Persistence.Database;

namespace SpotifyClone.Catalog.Infrastructure.Persistence.Repositories;

internal sealed class AlbumEfCoreRepository(CatalogAppDbContext context)
    : IAlbumRepository
{
    private readonly DbSet<Album> _albums = context.Albums;

    public async Task AddAsync(
        Album album,
        CancellationToken cancellationToken = default)
        => await _albums.AddAsync(album, cancellationToken);

    public async Task<Album?> GetByIdAsync(
        AlbumId id,
        CancellationToken cancellationToken = default)
        => await _albums
            .Where(a => a.Id == id)
            .Include(a => a.MainArtists)
            .Include(a => a.Tracks)
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<IEnumerable<Album>> GetAllByMainArtistAsync(
        ArtistId artistId,
        CancellationToken cancellationToken = default)
        => await _albums
            .Where(a => a.MainArtists.Any(id => id.Value == artistId.Value))
            .Include(a => a.MainArtists)
            .Include(a => a.Tracks)
            .ToListAsync(cancellationToken);

    public async Task DeleteAsync(
        Album album,
        CancellationToken cancellationToken = default)
        => _albums.Remove(album);
}
